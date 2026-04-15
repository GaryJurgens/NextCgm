using FastEndpoints;
using NextCgm.Services.Actions;
using NextCgm.Shared.DTOS;
using System.Security.Claims;

namespace NextCgm.Endpoints
{
    public class CreateUserEndpoint : Endpoint<CreateUserRequestDTO, CreateUserResponseDTO>
    {
        private readonly IUserService _userService;
       

        public CreateUserEndpoint(IUserService userService)
        {
            _userService = userService;
            
        }

        public override void Configure()
        {
            Post("/api/Users/CreateUser");
           
            Summary(s =>
            {
                s.Summary = "Create a new user";
                s.Description = "Creates a new user in NextCgm.";
            });
        }

        public override async Task HandleAsync(CreateUserRequestDTO req, CancellationToken ct)
        {

            try
            {
                // not for creating New user, for everything we vlaidate. 

                //var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

               // if (!Guid.TryParse(userIdClaim, out var userId))
               // {
                    // FastEndpoints approach for sending errors
                //    await Send.ErrorsAsync(400, ct);
                  //  return;
                //}


                if (req.Payload != null)
                {
                    var user = await _userService.CreateUserAsync(req);

                   await Send.OkAsync(new CreateUserResponseDTO
                    {
                        Success = true,
                        Message = "User created successfully",
                        Payload = user.Payload
                    }, ct);
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
