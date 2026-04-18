using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using NextCgm.Helpers.Utils;
using NextCgm.Shared.DTOS;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NextCgm.Services.Actions
{
    public interface ICloudflareService
    {
        Task<CreateDnsRecordResponseDTO> CreateDnsRecordAsync(CreateDnsRecordRequestDTO request);
        Task<RemoveDnsRecordResponseDTO> RemoveDnsRecordAsync(RemoveDnsRecordRequestDTO request);
    }

    public class CloudflareService : ICloudflareService
    {
        private readonly HttpClient _httpClient;
        private readonly CloudflareOptions _options;
        private readonly ILogger<CloudflareService> _logger;

        public CloudflareService(HttpClient httpClient, IOptions<CloudflareOptions> options, ILogger<CloudflareService> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;

            var baseUrl = string.IsNullOrEmpty(_options.ApiBaseUrl)
                ? "https://api.cloudflare.com/client/v4/"
                : _options.ApiBaseUrl;

            _httpClient.BaseAddress = new Uri(baseUrl);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiToken);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<CreateDnsRecordResponseDTO> CreateDnsRecordAsync(CreateDnsRecordRequestDTO request)
        {
            if (string.IsNullOrEmpty(_options.ZoneId) || string.IsNullOrEmpty(_options.ApiToken))
            {
                _logger.LogError("Cloudflare configuration is missing ZoneId or ApiToken.");
                return CreateDnsRecordResponseDTO.Failure("Cloudflare configuration is missing.");
            }

            var fullDomainName = string.IsNullOrEmpty(request.Subdomain)
                ? _options.Domain
                : $"{request.Subdomain}.{_options.Domain}";

            var target = request.TargetIp ?? _options.TargetIp;

            if (string.IsNullOrEmpty(target))
            {
                return CreateDnsRecordResponseDTO.Failure("Target IP/Domain is required.");
            }

            // Fix: Cloudflare throws 400 if you send an IP for a CNAME or a Hostname for an A record.
            var recordType = request.RecordType.ToUpperInvariant();

            var payload = new
            {
                type = recordType,
                name = request.Subdomain,
                content = target,
                proxied = request.Proxied,
                ttl = 1 // 1 = automatic
            };

            try
            {
                _logger.LogInformation("Sending {Type} record request for {Name} to Cloudflare...", recordType, fullDomainName);

                var response = await _httpClient.PostAsJsonAsync($"zones/{_options.ZoneId}/dns_records", payload);
                var responseContent = await response.Content.ReadAsStringAsync();

                // Deserialize the response to get structured error data
                var result = JsonSerializer.Deserialize<CloudflareApiResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (response.IsSuccessStatusCode && result != null && result.Success)
                {
                    _logger.LogInformation("Successfully created {Type} record: {Id}", recordType, result.Result?.Id);
                    return new CreateDnsRecordResponseDTO
                    {
                        Success = true,
                        Message = "DNS record created successfully.",
                        RecordId = result.Result?.Id ?? string.Empty
                    };
                }

                // If we reach here, Cloudflare rejected the request (HTTP 400, 401, etc.)
                var errorMsg = result?.Errors != null && result.Errors.Any()
                    ? string.Join(" | ", result.Errors.Select(e => $"Code {e.Code}: {e.Message}"))
                    : responseContent;

                _logger.LogWarning("Cloudflare API Rejected Request: {Error}", errorMsg);
                return CreateDnsRecordResponseDTO.Failure($"Cloudflare Error: {errorMsg}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while creating DNS record for {Name}", fullDomainName);
                return CreateDnsRecordResponseDTO.Failure($"Internal Error: {ex.Message}");
            }
        }

        public async Task<RemoveDnsRecordResponseDTO> RemoveDnsRecordAsync(RemoveDnsRecordRequestDTO request)
        {
            if (string.IsNullOrEmpty(_options.ZoneId) || string.IsNullOrEmpty(_options.ApiToken))
            {
                return RemoveDnsRecordResponseDTO.Failure("Cloudflare configuration is missing.");
            }

            if (string.IsNullOrEmpty(request.RecordId))
            {
                return RemoveDnsRecordResponseDTO.Failure("RecordId is required.");
            }

            try
            {
                var response = await _httpClient.DeleteAsync($"zones/{_options.ZoneId}/dns_records/{request.RecordId}");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully removed DNS record: {Id}", request.RecordId);
                    return new RemoveDnsRecordResponseDTO
                    {
                        Success = true,
                        Message = "DNS record removed successfully."
                    };
                }

                _logger.LogWarning("Cloudflare Failed to remove record {Id}. Response: {Content}", request.RecordId, responseContent);
                return RemoveDnsRecordResponseDTO.Failure($"Failed to remove DNS record: {responseContent}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while removing DNS record {Id}", request.RecordId);
                return RemoveDnsRecordResponseDTO.Failure($"Error removing DNS record: {ex.Message}");
            }
        }

        // --- Internal Helper Classes for Deserialization ---

        private class CloudflareApiResponse
        {
            public bool Success { get; set; }
            public CloudflareApiResult? Result { get; set; }
            public List<CloudflareApiError>? Errors { get; set; }
        }

        private class CloudflareApiResult
        {
            public string Id { get; set; } = string.Empty;
        }

        private class CloudflareApiError
        {
            public int Code { get; set; }
            public string Message { get; set; } = string.Empty;
        }
    }
}