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
                string confContent = $@"server {{
    listen 80;
    server_name {request.Payload.Hostname};
    client_max_body_size {request.Payload.ClientMaxBodySizeMb}M;

    location {request.Payload.PathPrefix} {{
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
";
                if (request.Payload.EnableWebSockets)
                {
                    confContent += @"        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection $connection_upgrade;
";
                }
                confContent += $@"        proxy_pass http://{request.Payload.InternalAddress}:{request.Payload.InternalPort}/;
    }}
}}
";
                // Normalize line endings to Linux format (LF) to prevent Nginx UI parser issues
                confContent = confContent.Replace("\r\n", "\n");

                string rawJsonSent = string.Empty;

                if (!string.IsNullOrEmpty(_options.NginxUiApiToken))
                {
                    // Use Nginx UI REST API
                    using var httpClient = new System.Net.Http.HttpClient();
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.NginxUiApiToken);
                    
                    var apiPayload = new
                    {
                        name = $"{request.Payload.Hostname}.conf",
                        content = confContent
                    };
                    
                    var jsonContent = new System.Net.Http.StringContent(System.Text.Json.JsonSerializer.Serialize(apiPayload), System.Text.Encoding.UTF8, "application/json");
                    rawJsonSent = await jsonContent.ReadAsStringAsync();
                    
                    var apiResponse = await httpClient.PostAsync($"{_options.NginxUiApiUrl.TrimEnd('/')}/api/sites", jsonContent);
                    
                    if (!apiResponse.IsSuccessStatusCode)
                    {
                        var errorContent = await apiResponse.Content.ReadAsStringAsync();
                        throw new Exception($"Nginx UI API failed with status {apiResponse.StatusCode}: {errorContent}");
                    }
                }
                else
                {
                    // Fallback: Write file directly, update SQLite, and reload via Docker API
                    
                    // Nginx UI stores sites in sites-available and symlinks to sites-enabled
                    string sitesAvailableDir = Path.Combine(_options.ConfigDirectory, "nginx", "sites-available");
                    string sitesEnabledDir = Path.Combine(_options.ConfigDirectory, "nginx", "sites-enabled");
                    
                    if (!Directory.Exists(sitesAvailableDir)) Directory.CreateDirectory(sitesAvailableDir);
                    if (!Directory.Exists(sitesEnabledDir)) Directory.CreateDirectory(sitesEnabledDir);

                    string siteName = request.Payload.Hostname;
                    string confFilePath = Path.Combine(sitesAvailableDir, siteName);
                    string symlinkPath = Path.Combine(sitesEnabledDir, siteName);

                    // Write file
                    await File.WriteAllTextAsync(confFilePath, confContent);

                    // Create symlink if it doesn't exist
                    if (!File.Exists(symlinkPath))
                    {
                        File.CreateSymbolicLink(symlinkPath, $"/etc/nginx/sites-available/{siteName}");
                    }

                    // Update SQLite database
                    string dbPath = Path.Combine(_options.ConfigDirectory, "nginx-ui", "database.db");
                    if (File.Exists(dbPath))
                    {
                        using var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={dbPath}");
                        await connection.OpenAsync();
                        
                        // Check if site already exists
                        using var checkCmd = connection.CreateCommand();
                        checkCmd.CommandText = "SELECT COUNT(1) FROM sites WHERE path = @path";
                        checkCmd.Parameters.AddWithValue("@path", $"/etc/nginx/sites-available/{siteName}");
                        var count = Convert.ToInt64(await checkCmd.ExecuteScalarAsync());
                        
                        if (count == 0)
                        {
                            using var insertCmd = connection.CreateCommand();
                            insertCmd.CommandText = "INSERT INTO sites (created_at, updated_at, path, advanced, namespace_id) VALUES (datetime('now'), datetime('now'), @path, 0, 0)";
                            insertCmd.Parameters.AddWithValue("@path", $"/etc/nginx/sites-available/{siteName}");
                            await insertCmd.ExecuteNonQueryAsync();
                        }
                    }

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
                    rawJsonSent = confContent;
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
