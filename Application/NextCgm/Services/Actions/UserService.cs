using Microsoft.EntityFrameworkCore;
using NextCgm.DataEntities.User;
using NextCgm.DContentext;
using NextCgm.Helpers.SubDomainGenerator;
using NextCgm.Helpers.Utils;
using NextCgm.Shared.ApiViewModels;
using NextCgm.Shared.DTOS;

namespace NextCgm.Services.Actions
{
    public interface IUserService
    {
        Task<GetUserResponseDTO> CreateUserAsync(CreateUserRequestDTO request);
        Task<LoginResponseDTO> LoginAsync(LoginRequestDTO request);
        Task<LogoutResponseDTO> LogoutAsync(Guid userId);
        Task<RemoveUserResponseDTO> RemoveUserAsync(RemoveUserRequestDTO request);
        Task<ForgotPasswordResponseDTO> ForgotPasswordAsync(ForgotPasswordRequestDTO request);
        Task<ResetPasswordResponseDTO> ResetPasswordAsync(ResetPasswordRequestDTO request);
        Task<VerifyOtpResponseDTO> VerifyOtpAsync(VerifyOtpRequestDTO request);
    }

    public class UserService : IUserService
    {
        private readonly AppDBContext _context;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IEmailService _emailService;

        public UserService(AppDBContext context, IJwtTokenGenerator jwtTokenGenerator, IEmailService emailService)
        {
            _context = context;
            _jwtTokenGenerator = jwtTokenGenerator;
            _emailService = emailService;
        }

        public async Task<LoginResponseDTO> LoginAsync(LoginRequestDTO request)
        {
            var user = await _context.UserEntities.FirstOrDefaultAsync(u => u.EmailUsername == request.EmailUsername);
            if (user == null)
            {
                return LoginResponseDTO.Failure("Invalid credentials");
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return LoginResponseDTO.Failure("Invalid credentials");
            }

            // Generate and send OTP for login
            user.VerificationCode = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
            user.VerificationCodeExpiry = DateTime.UtcNow.AddMinutes(15);
            user.VerificationCodeSentTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _emailService.SendVerificationCodeAsync(user.EmailUsername, user.FirstName, user.VerificationCode);

            return new LoginResponseDTO
            {
                Success = true,
                Message = "OTP sent to your email. Please verify to complete login.",
                Token = string.Empty,
                Payload = null
            };
        }

        public async Task<LogoutResponseDTO> LogoutAsync(Guid userId)
        {
            // In a stateless JWT setup, logout is usually handled client-side by deleting the token.
            // If we want to invalidate it server-side, we would need a token blacklist or refresh token rotation.
            // For now, we just return success.
            return new LogoutResponseDTO
            {
                Success = true,
                Message = "Logged out successfully"
            };
        }

        public async Task<RemoveUserResponseDTO> RemoveUserAsync(RemoveUserRequestDTO request)
        {
            var user = await _context.UserEntities.FirstOrDefaultAsync(u => u.UserEntityID == request.UserId);
            if (user == null)
            {
                return RemoveUserResponseDTO.Failure("User not found");
            }

            _context.UserEntities.Remove(user);
            await _context.SaveChangesAsync();

            return new RemoveUserResponseDTO
            {
                Success = true,
                Message = "User removed successfully"
            };
        }

        public async Task<GetUserResponseDTO> CreateUserAsync(CreateUserRequestDTO request)
        {
            string generatedSubDomain = NameGenerator.GetAdjNatio();
            while (await _context.UserEntities.AnyAsync(u => u.UserSubDomain == generatedSubDomain))
            {
                generatedSubDomain = NameGenerator.GetAdjNatio();
            }

            string generatedApiKey = NameGenerator.GetApiKey();
            while (await _context.UserEntities.AnyAsync(u => u.ApiKeyForNightScout == generatedApiKey))
            {
                generatedApiKey = NameGenerator.GetApiKey();
            }

            var newUser = new UserEntity
            {
                FirstName = request.Payload.FirstName,
                LastName = request.Payload.LastName,
                EmailUsername = request.Payload.EmailUsername,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Payload.PasswordHash),
                Active = true,
                IsEmailVerified = false,
                VerificationCode = Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                VerificationCodeExpiry = DateTime.UtcNow.AddHours(24),
                VerificationCodeSentTime = DateTime.UtcNow,
                UserSubDomain = generatedSubDomain,
                ApiKeyForNightScout = generatedApiKey,
                CountryListID = request.Payload.CountryListID,
                ProvinceStateListID = request.Payload.ProvinceStateListID,
                DockerStatus = DockerContainerStatus.Pending.ToString()
            };

            try
            {
                _context.UserEntities.Add(newUser);
                await _context.SaveChangesAsync();

                // Send OTP for registration
                await _emailService.SendVerificationCodeAsync(newUser.EmailUsername, newUser.FirstName, newUser.VerificationCode);

                var viewModel = new UserApiViewModel
                {
                    FirstName = newUser.FirstName,
                    LastName = newUser.LastName,
                    UserSubDomain = $"{newUser.UserSubDomain}.nextcgm.com",
                    ApiKeyForNightScout = newUser.ApiKeyForNightScout,
                    DockerStatus = newUser.DockerStatus.ToString()
                };

                return new GetUserResponseDTO
                {
                    Success = true,
                    Message = "User created successfully. OTP sent to email. Provisioning container...",
                    Payload = viewModel
                };
            }
            catch (Exception ex)
            {
                return new GetUserResponseDTO
                {
                    Success = false,
                    Message = "Error saving user: " + ex.Message,
                    Payload = null
                };
            }
        }

        public async Task<ForgotPasswordResponseDTO> ForgotPasswordAsync(ForgotPasswordRequestDTO request)
        {
            var user = await _context.UserEntities.FirstOrDefaultAsync(u => u.EmailUsername == request.EmailUsername);
            if (user == null)
            {
                // Don't reveal that the user doesn't exist
                return new ForgotPasswordResponseDTO
                {
                    Success = true,
                    Message = "If your email is registered, you will receive an OTP."
                };
            }

            user.VerificationCode = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
            user.VerificationCodeExpiry = DateTime.UtcNow.AddMinutes(15);
            user.VerificationCodeSentTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _emailService.SendVerificationCodeAsync(user.EmailUsername, user.FirstName, user.VerificationCode);

            return new ForgotPasswordResponseDTO
            {
                Success = true,
                Message = "If your email is registered, you will receive an OTP."
            };
        }

        public async Task<ResetPasswordResponseDTO> ResetPasswordAsync(ResetPasswordRequestDTO request)
        {
            var user = await _context.UserEntities.FirstOrDefaultAsync(u => u.EmailUsername == request.EmailUsername);
            if (user == null)
            {
                return ResetPasswordResponseDTO.Failure("Invalid request");
            }

            if (user.VerificationCode != request.VerificationCode || user.VerificationCodeExpiry < DateTime.UtcNow)
            {
                return ResetPasswordResponseDTO.Failure("Invalid or expired verification code");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.VerificationCode = string.Empty; // Clear the code after use
            
            await _context.SaveChangesAsync();

            return new ResetPasswordResponseDTO
            {
                Success = true,
                Message = "Password reset successfully"
            };
        }

        public async Task<VerifyOtpResponseDTO> VerifyOtpAsync(VerifyOtpRequestDTO request)
        {
            var user = await _context.UserEntities.FirstOrDefaultAsync(u => u.EmailUsername == request.EmailUsername);
            if (user == null)
            {
                return VerifyOtpResponseDTO.Failure("Invalid request");
            }

            if (user.VerificationCode != request.VerificationCode || user.VerificationCodeExpiry < DateTime.UtcNow)
            {
                return VerifyOtpResponseDTO.Failure("Invalid or expired verification code");
            }

            // Mark email as verified if it wasn't
            if (!user.IsEmailVerified)
            {
                user.IsEmailVerified = true;
            }

            user.VerificationCode = string.Empty; // Clear code after successful verification
            user.LastLogin = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var token = _jwtTokenGenerator.GenerateToken(user);

            var viewModel = new UserApiViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserSubDomain = $"{user.UserSubDomain}.nextcgm.com",
                ApiKeyForNightScout = user.ApiKeyForNightScout,
                DockerStatus = user.DockerStatus.ToString()
            };

            return new VerifyOtpResponseDTO
            {
                Success = true,
                Message = "OTP verified successfully",
                Token = token,
                Payload = viewModel
            };
        }
    }
}