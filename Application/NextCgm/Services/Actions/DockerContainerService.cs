using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Options;
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

                // 3. Define and Create the Container

                // GeneratePortInRange exposed ports

                int ExposedPort = GeneratePortInRange(8000, 9000);
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
                var containerDetails = await client.Containers.InspectContainerAsync(response.ID);

                if (containerDetails != null)
                {
                    //Start the container
                    await client.Containers.StartContainerAsync(response.ID, null);

                    // 3. If we reach here, it exists and started! Save to DB
                   await _context.DockerContainers.AddAsync(new DockerContainers
                    {
                        InstanceID = response.ID,
                        FriendlyContainerURL = SubDomainGen + _options.EndDomain,
                        ImageNameInUse = _options.ImageName,
                        ExposedPortLeft = ExposedPort,
                        HostPortRight = _options.ContainerPort,
                        DockerStatus = DockerContainerStatus.Running.ToString(), // Updated to Running
                        CreatedAt = DateTime.UtcNow
                    });

                    await _context.SaveChangesAsync();

                    return await Task.FromResult(new CreateContainerResponseDTO
                    {
                        Success = true,
                        Message = "Container created successfully",
                        Payload = new Shared.ApiViewModels.ContainerApiViewModel
                        {
                            InstanceID = response.ID,
                            FriendlyContainerURL = SubDomainGen + _options.EndDomain,
                            ImageNameInUse = _options.ImageName,
                            ExposedPortLeft = ExposedPort,
                            HostPortRight = _options.ContainerPort,
                            DockerStatus = DockerContainerStatus.Running.ToString()
                        }

                    });
                }
                return await Task.FromResult(new CreateContainerResponseDTO
                {
                    Success = false,
                    Message = "Container creation failed",
                    Payload = new Shared.ApiViewModels.ContainerApiViewModel
                    {
                        InstanceID = response.ID,
                        FriendlyContainerURL = SubDomainGen + _options.EndDomain,
                        ImageNameInUse = _options.ImageName,
                        ExposedPortLeft = ExposedPort,
                        HostPortRight = _options.ContainerPort,
                        DockerStatus = DockerContainerStatus.Error.ToString()
                    }

                });


            }
            catch (DockerContainerNotFoundException ex)
            {
                // Handle case where ID is invalid or container was deleted

               await _context.DockerContainers.AddAsync(new DockerContainers
                {
                    InstanceID = "N/A",
                    FriendlyContainerURL = SubDomainGen + _options.EndDomain,
                    ImageNameInUse = _options.ImageName,
                    ExposedPortLeft = 0,
                    HostPortRight = 0,
                    DockerStatus = DockerContainerStatus.Error.ToString(),
                    DockerStatusExeption = ex.ToString(),// Updated to Error,
                    CreatedAt = DateTime.UtcNow
                });
               await  _context.SaveChangesAsync();

                return await Task.FromResult(new CreateContainerResponseDTO
                {
                    Success = false,
                    Message = $"Container creation failed",
                    Payload = null


                });
                
            }
            catch (DockerApiException ex)
            {
                
         
               await _context.DockerContainers.AddAsync(new DockerContainers
                {
                    InstanceID = "N/A",
                    FriendlyContainerURL = SubDomainGen + _options.EndDomain,
                    ImageNameInUse = _options.ImageName,
                    ExposedPortLeft = 0,
                    HostPortRight = 0,
                    DockerStatus = DockerContainerStatus.Error.ToString(),
                    DockerStatusExeption = ex.ToString(),// Updated to Error,
                    CreatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();

                return await Task.FromResult(new CreateContainerResponseDTO
                {
                    Success = false,
                    Message = $"Container creation failed",
                    Payload = null


                });

            }

            // 5. Return response DTO
       
            
          
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
