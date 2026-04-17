using Microsoft.Extensions.Options;
using NextCgm.DataEntities.Nginx;
using NextCgm.DContentext;
using NextCgm.Helpers.Utils;
using NextCgm.Shared.ApiViewModels;
using NextCgm.Shared.DTOS;
using System;
using System.IO;
using System.Threading.Tasks;
using Medo;
using Docker.DotNet;
using Docker.DotNet.Models;
using System.Collections.Generic;

namespace NextCgm.Services.Actions
{
    public interface INginxService
    {
        Task<CreateNginxMappingResponseDTO> CreateNginxMappingAsync(CreateNginxMappingRequestDTO request);
    }

    public class NginxService : INginxService
    {
        private readonly AppDBContext _context;
        private readonly NginxUnitOptions _options;

        public NginxService(AppDBContext context, IOptions<NginxUnitOptions> options)
        {
            _context = context;
            _options = options.Value;
        }

        public async Task<CreateNginxMappingResponseDTO> CreateNginxMappingAsync(CreateNginxMappingRequestDTO request)
        {
            if (request.Payload == null)
            {
                return CreateNginxMappingResponseDTO.Failure("Payload is required.");
            }

            try
            {
                // 1. Create NginxContainer
                var nginxContainer = new NginxContainer
                {
                    NginxContainerID = Uuid7.NewUuid7(),
                    DockerContainerID = request.Payload.DockerContainerID,
                    Hostname = request.Payload.Hostname,
                    PathPrefix = request.Payload.PathPrefix,
                    ExternalPort = request.Payload.ExternalPort,
                    InternalAddress = request.Payload.InternalAddress,
                    InternalPort = request.Payload.InternalPort,
                    EnableWebSockets = request.Payload.EnableWebSockets,
                    ClientMaxBodySizeMb = request.Payload.ClientMaxBodySizeMb,
                    SslCertName = _options.CertificatePath ?? "",
                    SslCertKey = _options.KeyPath ?? ""
                };

                _context.NginxContainers.Add(nginxContainer);

                // 2. Create NginxRoutingRule
                var routingRule = new NginxRoutingRule
                {
                    NginxRoutingRuleID = Uuid7.NewUuid7(),
                    DockerContainerID = request.Payload.DockerContainerID,
                    ExternalHost = request.Payload.Hostname,
                    ListenPort = request.Payload.ExternalPort,
                    TargetContainerPort = request.Payload.InternalPort
                };

                _context.NginxRoutingRules.Add(routingRule);

                // Generate Nginx configuration
                string confContent = $@"
server {{
    listen 80;
    server_name {request.Payload.Hostname};

    location {request.Payload.PathPrefix} {{
        proxy_pass http://{request.Payload.InternalAddress}:{request.Payload.InternalPort};
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
";
                if (request.Payload.EnableWebSockets)
                {
                    confContent += @"
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection ""upgrade"";
";
                }
                confContent += $@"
        client_max_body_size {request.Payload.ClientMaxBodySizeMb}M;
    }}
}}
";
                // Ensure directory exists
                if (!Directory.Exists(_options.ConfigDirectory))
                {
                    Directory.CreateDirectory(_options.ConfigDirectory);
                }

                // Write file
                string confFilePath = Path.Combine(_options.ConfigDirectory, $"{request.Payload.Hostname}.conf");
                await File.WriteAllTextAsync(confFilePath, confContent);

                // Reload Nginx via Docker API
                var dockerUri = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows)
                    ? new Uri("npipe://./pipe/docker_engine")
                    : new Uri("unix:///var/run/docker.sock");
                var client = new DockerClientConfiguration(dockerUri).CreateClient();

                var execCreateResponse = await client.Exec.ExecCreateContainerAsync(_options.NginxContainerName, new ContainerExecCreateParameters
                {
                    AttachStdout = true,
                    AttachStderr = true,
                    Cmd = new List<string> { "nginx", "-s", "reload" }
                });

                await client.Exec.StartContainerExecAsync(execCreateResponse.ID);

                // 3. Log to NginxSyncLog
                var syncLog = new NginxSyncLog
                {
                    NginxSyncLogID = Uuid7.NewUuid7(),
                    ContainerEntityId = request.Payload.DockerContainerID,
                    SyncTimestamp = DateTime.UtcNow,
                    WasSuccessful = true,
                    LastErrorCode = string.Empty,
                    RawJsonSent = confContent
                };

                _context.NginxSyncLogs.Add(syncLog);

                await _context.SaveChangesAsync();

                var viewModel = new NginxApiViewModel
                {
                    NginxContainerID = nginxContainer.NginxContainerID,
                    DockerContainerID = nginxContainer.DockerContainerID,
                    Hostname = nginxContainer.Hostname,
                    PathPrefix = nginxContainer.PathPrefix,
                    ExternalPort = nginxContainer.ExternalPort,
                    InternalAddress = nginxContainer.InternalAddress,
                    InternalPort = nginxContainer.InternalPort,
                    EnableWebSockets = nginxContainer.EnableWebSockets,
                    ClientMaxBodySizeMb = nginxContainer.ClientMaxBodySizeMb,
                    SslCertName = nginxContainer.SslCertName
                };

                return new CreateNginxMappingResponseDTO
                {
                    Success = true,
                    Message = "Nginx mapping created successfully.",
                    Payload = viewModel
                };
            }
            catch (Exception ex)
            {
                // Log failure to NginxSyncLog
                var errorLog = new NginxSyncLog
                {
                    NginxSyncLogID = Uuid7.NewUuid7(),
                    ContainerEntityId = request.Payload.DockerContainerID,
                    SyncTimestamp = DateTime.UtcNow,
                    WasSuccessful = false,
                    LastErrorCode = "EXCEPTION",
                    RawJsonSent = ex.Message
                };

                _context.NginxSyncLogs.Add(errorLog);
                await _context.SaveChangesAsync();

                return CreateNginxMappingResponseDTO.Failure("Error creating Nginx mapping: " + ex.Message);
            }
        }
    }
}
