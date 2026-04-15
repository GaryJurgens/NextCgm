using Microsoft.Extensions.Options;
using NextCgm.DataEntities.Nginx;
using NextCgm.DContentext;
using NextCgm.Helpers.Utils;
using NextCgm.Shared.ApiViewModels;
using NextCgm.Shared.DTOS;
using System;
using System.Threading.Tasks;
using Medo;

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
                    SslCertName = request.Payload.SslCertName
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

                // 3. Log to NginxSyncLog
                var syncLog = new NginxSyncLog
                {
                    NginxSyncLogID = Uuid7.NewUuid7(),
                    ContainerEntityId = request.Payload.DockerContainerID,
                    SyncTimestamp = DateTime.UtcNow,
                    WasSuccessful = true,
                    LastErrorCode = string.Empty,
                    RawJsonSent = "{\"status\":\"pending_sync\"}" // Placeholder for actual Nginx API request
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
