using FastEndpoints;
using NextCgm.Services.Actions;
using NextCgm.Shared.DTOS;

namespace NextCgm.Endpoints
{
    public class CreateDnsRecordEndpoint : Endpoint<CreateDnsRecordRequestDTO, CreateDnsRecordResponseDTO>
    {
        private readonly ICloudflareService _cloudflareService;

        public CreateDnsRecordEndpoint(ICloudflareService cloudflareService)
        {
            _cloudflareService = cloudflareService;
        }

        public override void Configure()
        {
            Post("/api/Cloudflare/CreateDnsRecord");
            // Require authentication
            Summary(s =>
            {
                s.Summary = "Create a new DNS record";
                s.Description = "Creates a new DNS record (A or CNAME) in Cloudflare.";
            });
        }

        public override async Task HandleAsync(CreateDnsRecordRequestDTO req, CancellationToken ct)
        {
            try
            {
                var response = await _cloudflareService.CreateDnsRecordAsync(req);
                if (response.Success)
                {
                    await Send.OkAsync(response, ct);
                }
                else
                {
                    await Send.StatusCodeAsync(400, ct);
                }
            }
            catch (Exception ex)
            {
                ThrowError(ex.Message, 400);
            }
        }
    }

    public class RemoveDnsRecordEndpoint : Endpoint<RemoveDnsRecordRequestDTO, RemoveDnsRecordResponseDTO>
    {
        private readonly ICloudflareService _cloudflareService;

        public RemoveDnsRecordEndpoint(ICloudflareService cloudflareService)
        {
            _cloudflareService = cloudflareService;
        }

        public override void Configure()
        {
            Post("/api/Cloudflare/RemoveDnsRecord");
            // Require authentication
            Summary(s =>
            {
                s.Summary = "Remove a DNS record";
                s.Description = "Removes an existing DNS record from Cloudflare.";
            });
        }

        public override async Task HandleAsync(RemoveDnsRecordRequestDTO req, CancellationToken ct)
        {
            try
            {
                var response = await _cloudflareService.RemoveDnsRecordAsync(req);
                if (response.Success)
                {
                    await Send.OkAsync(response, ct);
                }
                else
                {
                    await Send.StatusCodeAsync(400, ct);
                }
            }
            catch (Exception ex)
            {
                ThrowError(ex.Message, 400);
            }
        }
    }
}
