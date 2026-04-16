using FastEndpoints;
using NextCgm.Services.Actions;
using NextCgm.Shared.DTOS;

namespace NextCgm.Endpoints
{
    public class CreateNginxMappingEndpoint : Endpoint<CreateNginxMappingRequestDTO, CreateNginxMappingResponseDTO>
    {
        private readonly INginxService _nginxService;

        public CreateNginxMappingEndpoint(INginxService nginxService)
        {
            _nginxService = nginxService;
        }

        public override void Configure()
        {
            Post("/api/Nginx/CreateNginxMapping");
            // Require authentication
            Summary(s =>
            {
                s.Summary = "Create a new Nginx mapping";
                s.Description = "Creates a new Nginx mapping for a Docker container.";
            });
        }

        public override async Task HandleAsync(CreateNginxMappingRequestDTO req, CancellationToken ct)
        {
            try
            {
                if (req.Payload != null)
                {
                    var response = await _nginxService.CreateNginxMappingAsync(req);
                    if (response.Success)
                    {
                        await Send.OkAsync(response, ct);
                    }
                    else
                    {
                        await Send.StatusCodeAsync(400, ct);
                    }
                }
                else
                {
                    ThrowError("Payload cannot be null", 400);
                    return;
                }
            }
            catch (Exception ex)
            {
                ThrowError(ex.Message, 400);
            }
        }
    }
}
