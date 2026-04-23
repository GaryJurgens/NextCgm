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
            AllowAnonymous();
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

    public class LoginEndpoint : Endpoint<LoginRequestDTO, LoginResponseDTO>
    {
        private readonly IUserService _userService;

        public LoginEndpoint(IUserService userService)
        {
            _userService = userService;
        }

        public override void Configure()
        {
            Post("/api/Users/Login");
            AllowAnonymous();
            Summary(s =>
            {
                s.Summary = "Login a user";
                s.Description = "Authenticates a user and sends an OTP.";
            });
        }

        public override async Task HandleAsync(LoginRequestDTO req, CancellationToken ct)
        {
            try
            {
                var response = await _userService.LoginAsync(req);
                if (response.Success)
                {
                    await Send.OkAsync(response, ct);
                }
                else
                {
                    ThrowError(response.Message, 400);
                }
            }
            catch (Exception ex)
            {
                ThrowError(ex.Message, 400);
            }
        }
    }

    public class LogoutEndpoint : EndpointWithoutRequest<LogoutResponseDTO>
    {
        private readonly IUserService _userService;

        public LogoutEndpoint(IUserService userService)
        {
            _userService = userService;
        }

        public override void Configure()
        {
            Post("/api/Users/Logout");
            Summary(s =>
            {
                s.Summary = "Logout a user";
                s.Description = "Logs out the current user.";
            });
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    ThrowError("User ID not found", 400);
                    return;
                }

                var response = await _userService.LogoutAsync(userId);
                await Send.OkAsync(response, ct);
            }
            catch (Exception ex)
            {
                ThrowError(ex.Message, 400);
            }
        }
    }

    public class ForgotPasswordEndpoint : Endpoint<ForgotPasswordRequestDTO, ForgotPasswordResponseDTO>
    {
        private readonly IUserService _userService;

        public ForgotPasswordEndpoint(IUserService userService)
        {
            _userService = userService;
        }

        public override void Configure()
        {
            Post("/api/Users/ForgotPassword");
            AllowAnonymous();
            Summary(s =>
            {
                s.Summary = "Forgot password";
                s.Description = "Sends an OTP to reset password.";
            });
        }

        public override async Task HandleAsync(ForgotPasswordRequestDTO req, CancellationToken ct)
        {
            try
            {
                var response = await _userService.ForgotPasswordAsync(req);
                await Send.OkAsync(response, ct);
            }
            catch (Exception ex)
            {
                ThrowError(ex.Message, 400);
            }
        }
    }

    public class ResetPasswordEndpoint : Endpoint<ResetPasswordRequestDTO, ResetPasswordResponseDTO>
    {
        private readonly IUserService _userService;

        public ResetPasswordEndpoint(IUserService userService)
        {
            _userService = userService;
        }

        public override void Configure()
        {
            Post("/api/Users/ResetPassword");
            AllowAnonymous();
            Summary(s =>
            {
                s.Summary = "Reset password";
                s.Description = "Resets user password using OTP.";
            });
        }

        public override async Task HandleAsync(ResetPasswordRequestDTO req, CancellationToken ct)
        {
            try
            {
                var response = await _userService.ResetPasswordAsync(req);
                if (response.Success)
                {
                    await Send.OkAsync(response, ct);
                }
                else
                {
                    ThrowError(response.Message, 400);
                }
            }
            catch (Exception ex)
            {
                ThrowError(ex.Message, 400);
            }
        }
    }

    public class VerifyOtpEndpoint : Endpoint<VerifyOtpRequestDTO, VerifyOtpResponseDTO>
    {
        private readonly IUserService _userService;

        public VerifyOtpEndpoint(IUserService userService)
        {
            _userService = userService;
        }

        public override void Configure()
        {
            Post("/api/Users/VerifyOtp");
            AllowAnonymous();
            Summary(s =>
            {
                s.Summary = "Verify OTP";
                s.Description = "Verifies the OTP and returns a JWT token.";
            });
        }

        public override async Task HandleAsync(VerifyOtpRequestDTO req, CancellationToken ct)
        {
            try
            {
                var response = await _userService.VerifyOtpAsync(req);
                if (response.Success)
                {
                    await Send.OkAsync(response, ct);
                }
                else
                {
                    ThrowError(response.Message, 400);
                }
            }
            catch (Exception ex)
            {
                ThrowError(ex.Message, 400);
            }
        }
    }
}

