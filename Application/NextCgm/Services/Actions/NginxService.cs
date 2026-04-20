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
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text;

namespace NextCgm.Services.Actions
{
    public interface INginxService
    {
        Task<CreateNginxMappingResponseDTO> CreateNginxMappingAsync(CreateNginxMappingRequestDTO request);
    }

    public class NginxService : INginxService
    {
        private readonly AppDBContext _context;
        private readonly NginxProxyManagerOptions _options;

        public NginxService(AppDBContext context, IOptions<NginxProxyManagerOptions> options)
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
                    SslCertName = "",
                    SslCertKey = ""
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

                string rawJsonSent = string.Empty;

                // Use Nginx Proxy Manager REST API
                using var httpClient = new HttpClient();
                
                // 1. Authenticate to get token
                var tokenPayload = new
                {
                    identity = _options.Email,
                    secret = _options.Password
                };
                
                var tokenContent = new StringContent(JsonSerializer.Serialize(tokenPayload), Encoding.UTF8, "application/json");
                var tokenResponse = await httpClient.PostAsync($"{_options.ApiBaseUrl.TrimEnd('/')}/tokens", tokenContent);
                
                if (!tokenResponse.IsSuccessStatusCode)
                {
                    var errorContent = await tokenResponse.Content.ReadAsStringAsync();
                    throw new Exception($"NPM Authentication failed with status {tokenResponse.StatusCode}: {errorContent}");
                }
                
                var tokenResult = await tokenResponse.Content.ReadAsStringAsync();
                var tokenDoc = JsonDocument.Parse(tokenResult);
                var token = tokenDoc.RootElement.GetProperty("token").GetString();
                
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                
                // 2. Create Proxy Host
                var apiPayload = new
                {
                    domain_names = new[] { request.Payload.Hostname },
                    forward_scheme = "http",
                    forward_host = request.Payload.InternalAddress,
                    forward_port = request.Payload.InternalPort,
                    access_list_id = "0",
                    certificate_id = "new",
                    ssl_forced = true,
                    meta = new { 
                        letsencrypt_email = _options.Email, 
                        letsencrypt_agree = true 
                    },
                    advanced_config = "",
                    locations = new object[] {},
                    block_exploits = false,
                    caching_enabled = false,
                    allow_websocket_upgrade = request.Payload.EnableWebSockets,
                    http2_support = false,
                    hsts_enabled = false,
                    hsts_subdomains = false
                };
                
                var jsonContent = new StringContent(JsonSerializer.Serialize(apiPayload), Encoding.UTF8, "application/json");
                rawJsonSent = await jsonContent.ReadAsStringAsync();
                
                var apiResponse = await httpClient.PostAsync($"{_options.ApiBaseUrl.TrimEnd('/')}/nginx/proxy-hosts", jsonContent);
                
                if (!apiResponse.IsSuccessStatusCode)
                {
                    var errorContent = await apiResponse.Content.ReadAsStringAsync();
                    throw new Exception($"NPM API failed with status {apiResponse.StatusCode}: {errorContent}");
                }

                // 3. Log to NginxSyncLog
                var syncLog = new NginxSyncLog
                {
                    NginxSyncLogID = Uuid7.NewUuid7(),
                    ContainerEntityId = request.Payload.DockerContainerID,
                    SyncTimestamp = DateTime.UtcNow,
                    WasSuccessful = true,
                    LastErrorCode = string.Empty,
                    RawJsonSent = rawJsonSent
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
