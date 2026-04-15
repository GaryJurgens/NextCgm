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
    public class DockerContainerService
    {
        private readonly AppDBContext _context;

        private readonly string SubDomainGen = Helpers.SubDomainGenerator.NameGenerator.GetAdjNatio();
        private readonly DockerOptions _options;

        public DockerContainerService(AppDBContext context, IOptions<DockerOptions> options)
        {
            _context = context;
            _options = options.Value;
        }

        public async Task<CreateContainerResponseDTO> CreateDockerContainer()
        {
            try
            {
                // 1. Initialize the client (Standard for Windows/Linux)
                var client = new DockerClientConfiguration().CreateClient();

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

                int ExposedPort = GeneratePortInRange(8000, 9000);

                await _context.DockerLogger.AddAsync(new DockerLogger
                {
                    DockerLoggerID = Uuid7.NewUuid7(),
                    LogMessage = $"Attempting to create container with image: {_options.ImageName}:{_options.Tag} on exposed port: {ExposedPort}",
                    FriendlyContanierName = SubDomainGen + _options.EndDomain,
                    ContainerStatus = DockerContainerStatus.Creating.ToString(),
                    CreatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();

                var response = await client.Containers.CreateContainerAsync(new CreateContainerParameters
                {
                    Image = _options.ImageName,
                    Name = SubDomainGen,

                    HostConfig = new HostConfig
                    {
                        PortBindings = new Dictionary<string, IList<PortBinding>>
                    {
                         { _options.ContainerPort.ToString(), new List<PortBinding> { new PortBinding { HostPort = ExposedPort.ToString() } } }
                     }
                    }
                });

                // 4. Try to inspect the container to see if it exists
                var containerId = response.ID;
                bool isFullyStarted = false;
                string finalStatus = DockerContainerStatus.Error.ToString();

                await _context.DockerContainers.AddAsync(new DockerContainers
                {
                    ContaierID = Uuid7.NewUuid7(),
                    InstanceID = containerId,
                    FriendlyContainerURL = SubDomainGen + _options.EndDomain,
                    ImageNameInUse = _options.ImageName,
                    ExposedPortLeft = ExposedPort,
                    HostPortRight = _options.ContainerPort,
                    DockerStatus = DockerContainerStatus.Created.ToString(),
                    CreatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();

                await _context.DockerLogger.AddAsync(new DockerLogger
                {
                    DockerLoggerID = Uuid7.NewUuid7(),
                    LogMessage = $"Container created with ID: {response.ID}, Status: {DockerContainerStatus.Created} || but not started yet.",
                    FriendlyContanierName = SubDomainGen + _options.EndDomain,
                    ContainerStatus = DockerContainerStatus.Created.ToString(),
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
                    var newContainerRecord = new DockerContainers
                    {
                        ContaierID = Uuid7.NewUuid7(),
                        InstanceID = containerId,
                        FriendlyContainerURL = SubDomainGen + _options.EndDomain,
                        ImageNameInUse = _options.ImageName,
                        ExposedPortLeft = ExposedPort,
                        HostPortRight = _options.ContainerPort,
                        DockerStatus = DockerContainerStatus.Running.ToString(),
                        CreatedAt = DateTime.UtcNow
                    };

                    await _context.DockerContainers.AddAsync(newContainerRecord);
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

        public static int GeneratePortInRange(int startPort, int endPort)
        {
            try
            {
                var properties = IPGlobalProperties.GetIPGlobalProperties();
                var activeEndPoints = properties.GetActiveTcpListeners();

                // Get all ports currently in use
                var usedPorts = activeEndPoints.Select(p => p.Port).ToList();

                for (int port = startPort; port <= endPort; port++)
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
    }
}