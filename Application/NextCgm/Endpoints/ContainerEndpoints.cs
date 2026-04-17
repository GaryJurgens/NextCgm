using FastEndpoints;
using NextCgm.Services.Actions;
using NextCgm.Shared.DTOS;
using System.Security.Claims;

namespace NextCgm.Endpoints
{
    public class CreateContainerEndpoint : Endpoint<CreateContainerRequestDTO, CreateContainerResponseDTO>
    {
        private readonly IDockerContainerService _dockerContainerService;

        public CreateContainerEndpoint(IDockerContainerService dockerContainerService)
        {
            _dockerContainerService = dockerContainerService;
        }

        public override void Configure()
        {
            Post("/api/Containers/CreateContainer");
            
            // Require authentication
            Summary(s =>
            {
                s.Summary = "Create a new container";
                s.Description = "Creates a new Docker container for the user.";
            });
        }

        public override async Task HandleAsync(CreateContainerRequestDTO req, CancellationToken ct)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    ThrowError("User ID not found", 400);
                    return;
                }

                var response = await _dockerContainerService.CreateDockerContainer(userId);
                if (response.Success)
                {
                    await Send.OkAsync(response,  ct);
                }
                else
                {
                    await Send.StatusCodeAsync(400,  ct);
                }
            }
            catch (Exception ex)
            {
                ThrowError(ex.Message, 400);
            }
        }
    }

    public class StopContainerEndpoint : Endpoint<StopContainerRequestDTO, StopContainerResponseDTO>
    {
        private readonly IDockerContainerService _dockerContainerService;

        public StopContainerEndpoint(IDockerContainerService dockerContainerService)
        {
            _dockerContainerService = dockerContainerService;
        }

        public override void Configure()
        {
            Post("/api/Containers/StopContainer");
            // Require authentication
            Summary(s =>
            {
                s.Summary = "Stop an existing container";
                s.Description = "Stops a running Docker container for the user.";
            });
        }

        public override async Task HandleAsync(StopContainerRequestDTO req, CancellationToken ct)
        {
            try
            {
                var response = await _dockerContainerService.StopDockerContainer(req);
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