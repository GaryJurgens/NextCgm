using Microsoft.Extensions.Options;
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

        public CloudflareService(HttpClient httpClient, IOptions<CloudflareOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
            
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
                return CreateDnsRecordResponseDTO.Failure("Cloudflare configuration is missing.");
            }

            var fullDomainName = string.IsNullOrEmpty(request.Subdomain) 
                ? _options.Domain 
                : $"{request.Subdomain}.{_options.Domain}";

            var target = request.Target ?? _options.TargetIp;

            if (string.IsNullOrEmpty(target))
            {
                return CreateDnsRecordResponseDTO.Failure("Target IP/Domain is required.");
            }

            var payload = new
            {
                type = request.RecordType.ToUpper(),
                name = fullDomainName,
                content = target,
                proxied = request.Proxied,
                ttl = 1 // 1 = automatic
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync($"zones/{_options.ZoneId}/dns_records", payload);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<CloudflareApiResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (result != null && result.Success && result.Result != null)
                    {
                        return new CreateDnsRecordResponseDTO
                        {
                            Success = true,
                            Message = "DNS record created successfully.",
                            RecordId = result.Result.Id
                        };
                    }
                }

                return CreateDnsRecordResponseDTO.Failure($"Failed to create DNS record: {responseContent}");
            }
            catch (Exception ex)
            {
                return CreateDnsRecordResponseDTO.Failure($"Error creating DNS record: {ex.Message}");
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
                    return new RemoveDnsRecordResponseDTO
                    {
                        Success = true,
                        Message = "DNS record removed successfully."
                    };
                }

                return RemoveDnsRecordResponseDTO.Failure($"Failed to remove DNS record: {responseContent}");
            }
            catch (Exception ex)
            {
                return RemoveDnsRecordResponseDTO.Failure($"Error removing DNS record: {ex.Message}");
            }
        }

        private class CloudflareApiResponse
        {
            public bool Success { get; set; }
            public CloudflareApiResult? Result { get; set; }
        }

        private class CloudflareApiResult
        {
            public string Id { get; set; } = string.Empty;
        }
    }
}
