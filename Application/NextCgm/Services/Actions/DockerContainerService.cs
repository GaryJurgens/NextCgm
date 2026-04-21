using Docker.DotNet;
using Docker.DotNet.Models;
using Medo;
using Microsoft.Extensions.Options;
using NextCgm.DataEntities;
using NextCgm.DataEntities.Containers;
using NextCgm.DContentext;
using NextCgm.Helpers.Utils;
using NextCgm.Shared.DTOS;
using System.Net.NetworkInformation;

namespace NextCgm.Services.Actions
{
    public interface IDockerContainerService
    {
        public Task<int> GeneratePortInRange(int startPort, int endPort);
        Task<CreateContainerResponseDTO> CreateDockerContainer(Guid userId);
        Task<StopContainerResponseDTO> StopDockerContainer(StopContainerRequestDTO request);
        Task<DeleteContainerResponseDTO> DeleteDockerContainer(DeleteContainerRequestDTO request);
        Task<GetAllContainersResponseDTO> GetAllContainersByUser(Guid userId);
    }

    public class DockerContainerService : IDockerContainerService
    {
        private readonly AppDBContext _context;

        private readonly string SubDomainGen = Helpers.SubDomainGenerator.NameGenerator.GetAdjNatio();
        private readonly DockerOptions _options;
        private readonly IDocumentDbService _documentDbService;

        public DockerContainerService(AppDBContext context, IOptions<DockerOptions> options, IDocumentDbService documentDbService)
        {
            _context = context;
            _options = options.Value;
            _documentDbService = documentDbService;
        }

        public async Task<CreateContainerResponseDTO> CreateDockerContainer(Guid userId)
        {
            try
            {
                var user = await _context.UserEntities.FindAsync(userId);
                if (user == null)
                {
                    return new CreateContainerResponseDTO
                    {
                        Success = false,
                        Message = "User not found.",
                    };
                }

                // Find the base Mongo connection string from Docker options
                string baseMongoConnectionString = string.Empty;
                foreach (var env in _options.EnvironmentVariables)
                {
                    if (env.StartsWith("MONGO_CONNECTION="))
                    {
                        baseMongoConnectionString = env.Substring("MONGO_CONNECTION=".Length);
                        break;
                    }
                }

                // Create Document DB Database first so we can inject the connection string
                var dbRequest = new CreateDocumentDbRequestDTO
                {
                    Success = true,
                    Message = "Requesting Document DB Creation",
                    Payload = new Shared.ApiViewModels.DocumentDbApiViewModel
                    {
                        DatabaseName = SubDomainGen,
                        ConnectionString = baseMongoConnectionString
                    }
                };
                var dbResponse = await _documentDbService.CreateDatabaseAsync(dbRequest);
                
                if (!dbResponse.Success)
                {
                    return new CreateContainerResponseDTO
                    {
                        Success = false,
                        Message = $"Failed to create Document DB: {dbResponse.Message}",
                    };
                }

                string newMongoConnectionString = dbResponse.Payload?.ConnectionString ?? string.Empty;

                // Prepare environment variables, replacing any placeholder API_SECRET with the user's actual API key
                var envVars = new List<string>();
                bool mongoConnectionSet = false;
                foreach (var env in _options.EnvironmentVariables)
                {
                    if (env.StartsWith("API_SECRET="))
                    {
                        envVars.Add($"API_SECRET={user.ApiKeyForNightScout}");
                    }
                    else if (env.StartsWith("MONGO_CONNECTION=") && !string.IsNullOrEmpty(newMongoConnectionString))
                    {
                        envVars.Add($"MONGO_CONNECTION={newMongoConnectionString}");
                        mongoConnectionSet = true;
                    }
                    else
                    {
                        envVars.Add(env);
                    }
                }

                if (!mongoConnectionSet && !string.IsNullOrEmpty(newMongoConnectionString))
                {
                    envVars.Add($"MONGO_CONNECTION={newMongoConnectionString}");
                }

                // 1. Initialize the client (Standard for Windows/Linux)
                var dockerUri = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows)
                    ? new Uri("npipe://./pipe/docker_engine")
                    : new Uri("unix:///var/run/docker.sock");
                var client = new DockerClientConfiguration(dockerUri).CreateClient();

                // 2. Pull the image first (Docker won't create a container if image is missing)
                await client.Images.CreateImageAsync(
                    new ImagesCreateParameters { FromImage = _options.ImageName, Tag = _options.Tag },
                    null,
                    new Progress<JSONMessage>());

                await _context.DockerLogger.AddAsync(new DockerLogger
                {
                    DockerLoggerID = Uuid7.NewUuid7(),
                    LogMessage = $"Pulled Image: {_options.ImageName}:{_options.Tag}",
                    FriendlyContanierName = SubDomainGen + _options.EndDomain,
                    ContainerStatus = DockerContainerStatus.Pulled.ToString(),
                    CreatedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();

                // GeneratePortInRange exposed ports

                int ExposedPort = await GeneratePortInRange(10000, 20000);

                var hostConfig = new HostConfig
                {
                    RestartPolicy = new RestartPolicy { Name = RestartPolicyKind.Always }
                };

                if (_options.IsLocalDevelopment)
                {
                    hostConfig.PortBindings = new Dictionary<string, IList<PortBinding>>
                    {
                        { _options.ContainerPort.ToString() + "/tcp", new List<PortBinding> { new PortBinding { HostPort = ExposedPort.ToString() } } }
                    };
                }
                else
                {
                    // In production, attach to a specific Docker network so containers can resolve each other by name
                    hostConfig.NetworkMode = _options.DockerNetworkName;
                }

                var createParams = new CreateContainerParameters
                {
                    Image = _options.ImageName,
                    Name = SubDomainGen,
                    Env = envVars,
                    ExposedPorts = new Dictionary<string, EmptyStruct>
                    {
                        { _options.ContainerPort.ToString() + "/tcp", default(EmptyStruct) }
                    },
                    HostConfig = hostConfig,
                    Labels = new Dictionary<string, string>
                    {
                        { "traefik.enable", "true" },
                        { $"traefik.http.routers.{SubDomainGen}.rule", $"Host(`{SubDomainGen}{_options.EndDomain}`)" },
                        { $"traefik.http.services.{SubDomainGen}.loadbalancer.server.port", _options.ContainerPort.ToString() }
                    }
                };

                await _context.DockerLogger.AddAsync(new DockerLogger
                {
                    DockerLoggerID = Uuid7.NewUuid7(),
                    LogMessage = $"Attempting to create container with image: {_options.ImageName}:{_options.Tag} on exposed port: {ExposedPort}",
                    FriendlyContanierName = SubDomainGen + _options.EndDomain,
                    ContainerStatus = DockerContainerStatus.Creating.ToString(),
                    RequestPayload = System.Text.Json.JsonSerializer.Serialize(createParams),
                    CreatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();

                var response = await client.Containers.CreateContainerAsync(createParams);

                // 4. Try to inspect the container to see if it exists
                var containerId = response.ID;
                bool isFullyStarted = false;
                string finalStatus = DockerContainerStatus.Error.ToString();

                var newContainerRecord = new DockerContainers
                {
                    DockerContainersID = Uuid7.NewUuid7(),
                    UserEntityID = userId,
                    InstanceID = containerId,
                    AppUniqueName = SubDomainGen,
                    DockerLable = "Owner=" + SubDomainGen,
                    ImageNameInUse = _options.ImageName,
                    ExposedPortLeft = ExposedPort,
                    HostPortRight = _options.ContainerPort,
                    DockerStatus = DockerContainerStatus.Created.ToString(),
                    CreatedAt = DateTime.UtcNow
                };
                await _context.DockerContainers.AddAsync(newContainerRecord);
                await _context.SaveChangesAsync();

                await _context.DockerLogger.AddAsync(new DockerLogger
                {
                    DockerLoggerID = Uuid7.NewUuid7(),
                    LogMessage = $"Container created with ID: {response.ID}, Status: {DockerContainerStatus.Created} || but not started yet.",
                    FriendlyContanierName = SubDomainGen + _options.EndDomain,
                    ContainerStatus = DockerContainerStatus.Created.ToString(),
                    ResponsePayload = System.Text.Json.JsonSerializer.Serialize(response),
                    CreatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();

                try
                {
                    // 1. Initial Start Command
                    await client.Containers.StartContainerAsync(containerId, null);

                    // 2. Polling Loop to check for "running" state
                    int maxRetries = 10;
                    int delayMs = 1000; // 1 second intervals

                    for (int i = 0; i < maxRetries; i++)
                    {
                        var inspect = await client.Containers.InspectContainerAsync(containerId);
                        var currentStatus = inspect.State.Status.ToLower(); // e.g., "created", "running", "exited"

                        if (currentStatus == "running")
                        {
                            isFullyStarted = true;
                            finalStatus = DockerContainerStatus.Running.ToString();

                            await _context.DockerLogger.AddAsync(new DockerLogger
                            {
                                DockerLoggerID = Uuid7.NewUuid7(),
                                LogMessage = $"Container created with ID: {response.ID}, Status: {DockerContainerStatus.Running} || but not started yet.",
                                ContainerStatus = DockerContainerStatus.Running.ToString(),
                                FriendlyContanierName = SubDomainGen + _options.EndDomain,
                                CreatedAt = DateTime.UtcNow
                            }); await _context.SaveChangesAsync();
                            break;
                        }

                        if (currentStatus == "exited" || currentStatus == "dead")
                        {
                            finalStatus = DockerContainerStatus.Error.ToString();
                            await _context.DockerLogger.AddAsync(new DockerLogger
                            {
                                DockerLoggerID = Uuid7.NewUuid7(),
                                LogMessage = $"Container created with ID: {response.ID}, Status: {DockerContainerStatus.Error} || failed to start.",
                                ContainerStatus = DockerContainerStatus.Error.ToString(),
                                FriendlyContanierName = SubDomainGen + _options.EndDomain,
                                CreatedAt = DateTime.UtcNow
                            }); await _context.SaveChangesAsync();
                            break;
                        }

                        // Optional: Log that we are still waiting
                        await Task.Delay(delayMs);
                    }
                }
                catch (Exception ex)
                {
                    // Log exception if Docker API fails
                    finalStatus = DockerContainerStatus.Error.ToString();

                    await _context.DockerLogger.AddAsync(new DockerLogger
                    {
                        DockerLoggerID = Uuid7.NewUuid7(),
                        LogMessage = $"Exception while starting container ID: {response.ID}. Exception: {ex}",
                        ContainerStatus = DockerContainerStatus.Error.ToString(),
                        FriendlyContanierName = SubDomainGen + _options.EndDomain,
                        CreatedAt = DateTime.UtcNow
                    }); await _context.SaveChangesAsync();
                }

                if (isFullyStarted)
                {
                    // 3. Save to Database only after confirmed Running
                    newContainerRecord.DockerStatus = DockerContainerStatus.Running.ToString();
                    _context.DockerContainers.Update(newContainerRecord);
                    await _context.SaveChangesAsync();

                    await _context.DockerLogger.AddAsync(new DockerLogger
                    {
                        DockerLoggerID = Uuid7.NewUuid7(),
                        LogMessage = $"Container created with ID: {response.ID}, Status: {DockerContainerStatus.Running} ",
                        ContainerStatus = DockerContainerStatus.Running.ToString(),
                        FriendlyContanierName = SubDomainGen + _options.EndDomain,
                        DockerInstanceID = containerId,
                        CreatedAt = DateTime.UtcNow,

                        ImageNameInUse = _options.ImageName,
                        ExposedPortLeft = ExposedPort,
                        HostPortRight = _options.ContainerPort,
                        DockerStatus = finalStatus
                    }); await _context.SaveChangesAsync();

                    return new CreateContainerResponseDTO
                    {
                        Success = true,
                        Message = "Container created and verified as running.",
                        Payload = new Shared.ApiViewModels.ContainerApiViewModel
                        {
                            DockerContainersID = newContainerRecord.DockerContainersID,
                            InstanceID = containerId,
                            FriendlyContainerURL = SubDomainGen + _options.EndDomain,
                            ImageNameInUse = _options.ImageName,
                            ExposedPortLeft = ExposedPort,
                            HostPortRight = _options.ContainerPort,
                            DockerStatus = finalStatus
                        }
                    };
                }

                // 4. Return Failure if it never reached "running"

                await _context.DockerLogger.AddAsync(new DockerLogger
                {
                    DockerLoggerID = Uuid7.NewUuid7(),
                    LogMessage = $"Container created with ID: {response.ID}, Status: {finalStatus} || failed to start within timeout.",
                    ContainerStatus = finalStatus,
                    FriendlyContanierName = SubDomainGen + _options.EndDomain,
                    DockerInstanceID = containerId,
                    ImageNameInUse = _options.ImageName,
                    ExposedPortLeft = ExposedPort,
                    HostPortRight = _options.ContainerPort,
                    CreatedAt = DateTime.UtcNow
                }); await _context.SaveChangesAsync();

                return new CreateContainerResponseDTO
                {
                    Success = false,
                    Message = $"Container reached state: {finalStatus} but failed to start fully within the timeout.",
                };
            }
            catch (DockerContainerNotFoundException ex)
            {
                // Handle case where ID is invalid or container was deleted

                await _context.DockerLogger.AddAsync(new DockerLogger
                {
                    DockerLoggerID = Uuid7.NewUuid7(),
                    LogMessage = $"Docker Container Not Found.",
                    ExeptionMessage = ex.ToString(),
                    ContainerStatus = DockerContainerStatus.TimeOutError.ToString(),
                    FriendlyContanierName = SubDomainGen + _options.EndDomain,
                    CreatedAt = DateTime.UtcNow
                }); await _context.SaveChangesAsync();

                return await Task.FromResult(new CreateContainerResponseDTO
                {
                    Success = false,
                    Message = $"Container creation failed",
                    Payload = null
                });
            }
            catch (DockerApiException ex)
            {
                await _context.DockerLogger.AddAsync(new DockerLogger
                {
                    DockerLoggerID = Uuid7.NewUuid7(),
                    LogMessage = $"API Error " + ex.Message,
                    ContainerStatus = DockerContainerStatus.Error.ToString(),
                    FriendlyContanierName = SubDomainGen + _options.EndDomain,
                    CreatedAt = DateTime.UtcNow,
                    ImageNameInUse = _options.ImageName
                });

                await _context.SaveChangesAsync();

                return await Task.FromResult(new CreateContainerResponseDTO
                {
                    Success = false,
                    Message = $"Container creation failed",
                    Payload = null
                });
            }
        }

        public async Task<int> GeneratePortInRange(int startPort, int endPort)
        {
            try
            {
                // Since this app runs in a container, we cannot check the host's active TCP listeners.
                // Instead, we query the database for ports already assigned to our containers.
                var usedPorts = _context.DockerContainers
                    .Where(c => c.ExposedPortLeft >= startPort && c.ExposedPortLeft <= endPort)
                    .Select(c => c.ExposedPortLeft)
                    .ToList();

                // Start from a random port in the range to avoid collisions with host services
                // that might be using the lower end of the range (like 8000).
                Random rnd = new Random();
                int initialPort = rnd.Next(startPort, endPort + 1);

                for (int port = initialPort; port <= endPort; port++)
                {
                    if (!usedPorts.Contains(port))
                    {
                        return port;
                    }
                }
                
                // Wrap around if needed
                for (int port = startPort; port < initialPort; port++)
                {
                    if (!usedPorts.Contains(port))
                    {
                        return port;
                    }
                }

                throw new Exception("No free ports available in the specified range!");
            }
            catch (Exception ex)
            {
                throw new Exception("Error generating port: " + ex.Message);
            }
        }

        public async Task<StopContainerResponseDTO> StopDockerContainer(StopContainerRequestDTO request)
        {
            try
            {
                var containerRecord = await _context.DockerContainers.FindAsync(request.DockerContainersID);
                if (containerRecord == null)
                {
                    return StopContainerResponseDTO.Failure("Container record not found in the database.");
                }

                var dockerUri = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows)
                    ? new Uri("npipe://./pipe/docker_engine")
                    : new Uri("unix:///var/run/docker.sock");
                var client = new DockerClientConfiguration(dockerUri).CreateClient();

                // Stop the container using Docker API
                var stopped = await client.Containers.StopContainerAsync(containerRecord.InstanceID, new ContainerStopParameters
                {
                    WaitBeforeKillSeconds = 10
                });

                if (stopped)
                {
                    containerRecord.DockerStatus = DockerContainerStatus.Exited.ToString();
                    containerRecord.StoppedAt = DateTime.UtcNow;

                    await _context.DockerLogger.AddAsync(new DockerLogger
                    {
                        DockerLoggerID = Uuid7.NewUuid7(),
                        LogMessage = $"Container stopped successfully.",
                        FriendlyContanierName = containerRecord.AppUniqueName,
                        ContainerStatus = DockerContainerStatus.Exited.ToString(),
                        DockerInstanceID = containerRecord.InstanceID,
                        CreatedAt = DateTime.UtcNow
                    });

                    await _context.SaveChangesAsync();

                    return new StopContainerResponseDTO
                    {
                        Success = true,
                        Message = "Container stopped successfully.",
                        Payload = new Shared.ApiViewModels.ContainerApiViewModel
                        {
                            InstanceID = containerRecord.InstanceID,
                            FriendlyContainerURL = containerRecord.AppUniqueName,
                            DockerStatus = containerRecord.DockerStatus
                        }
                    };
                }
                else
                {
                    return StopContainerResponseDTO.Failure("Failed to stop the container via Docker API.");
                }
            }
            catch (DockerContainerNotFoundException ex)
            {
                return StopContainerResponseDTO.Failure("Docker container not found on the host.");
            }
            catch (DockerApiException ex)
            {
                return StopContainerResponseDTO.Failure($"Docker API Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StopContainerResponseDTO.Failure($"An error occurred: {ex.Message}");
            }
        }
        public async Task<GetAllContainersResponseDTO> GetAllContainersByUser(Guid userId)
        {
            try
            {
                var containers = _context.DockerContainers
                    .Where(c => c.UserEntityID == userId && c.DockerStatus != DockerContainerStatus.Deleted.ToString())
                    .ToList();

                var payload = containers.Select(c => new Shared.ApiViewModels.ContainerApiViewModel
                {
                    DockerContainersID = c.DockerContainersID,
                    InstanceID = c.InstanceID,
                    FriendlyContainerURL = c.AppUniqueName + _options.EndDomain,
                    ImageNameInUse = c.ImageNameInUse,
                    ExposedPortLeft = c.ExposedPortLeft,
                    HostPortRight = c.HostPortRight,
                    DockerStatus = c.DockerStatus
                }).ToList();

                return new GetAllContainersResponseDTO
                {
                    Success = true,
                    Message = "Containers retrieved successfully.",
                    Payload = payload
                };
            }
            catch (Exception ex)
            {
                return GetAllContainersResponseDTO.Failure($"Error retrieving containers: {ex.Message}");
            }
        }

        public async Task<DeleteContainerResponseDTO> DeleteDockerContainer(DeleteContainerRequestDTO request)
        {
            try
            {
                var containerRecord = await _context.DockerContainers.FindAsync(request.DockerContainersID);
                if (containerRecord == null)
                {
                    return DeleteContainerResponseDTO.Failure("Container record not found in the database.");
                }

                var dockerUri = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows)
                    ? new Uri("npipe://./pipe/docker_engine")
                    : new Uri("unix:///var/run/docker.sock");
                var client = new DockerClientConfiguration(dockerUri).CreateClient();

                try
                {
                    // Remove the container using Docker API
                    if (!string.IsNullOrEmpty(containerRecord.InstanceID))
                    {
                        await client.Containers.RemoveContainerAsync(containerRecord.InstanceID, new ContainerRemoveParameters
                        {
                            Force = true,
                            RemoveVolumes = true
                        });
                    }
                }
                catch (Exception)
                {
                    // Container already removed from docker host or docker API failed, continue to clean up DB
                }

                containerRecord.DockerStatus = DockerContainerStatus.Deleted.ToString();
                containerRecord.RemovedAt = DateTime.UtcNow;

                await _context.DockerLogger.AddAsync(new DockerLogger
                {
                    DockerLoggerID = Uuid7.NewUuid7(),
                    LogMessage = $"Container deleted successfully.",
                    FriendlyContanierName = containerRecord.AppUniqueName,
                    ContainerStatus = DockerContainerStatus.Deleted.ToString(),
                    DockerInstanceID = containerRecord.InstanceID,
                    CreatedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();

                return new DeleteContainerResponseDTO
                {
                    Success = true,
                    Message = "Container deleted successfully."
                };
            }
            catch (DockerApiException ex)
            {
                return DeleteContainerResponseDTO.Failure($"Docker API Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return DeleteContainerResponseDTO.Failure($"An error occurred: {ex.Message}");
            }
        }
    }
}