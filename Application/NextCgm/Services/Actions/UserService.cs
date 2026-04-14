using Microsoft.EntityFrameworkCore;
using NextCgm.DataEntities.User;
using NextCgm.DContentext;
using NextCgm.Helpers.SubDomainGenerator;
using NextCgm.Helpers.Utils;
using NextCgm.Shared.ApiViewModels;
using NextCgm.Shared.DTOS;
using System.Security.Cryptography.X509Certificates;

namespace NextCgm.Services.Actions
{

    public class UserService
    {
        // 1. Fields go here (At the top of the CLASS)
        private readonly AppDBContext _context;

        // 2. Constructor goes here (Also at the top of the CLASS)
        public UserService(AppDBContext context)
        {
            _context = context;
        }

        // 3. The Method goes here
        public async Task<GetUserResponseDTO> CreateUserAsync(GetUserRequestDTO request)
        {
            // A. Generate identifiers
            string generatedSubDomain = NameGenerator.GetAdjNatio();
            string generatedApiKey = NameGenerator.GetAdjColorNato(false);

            // B. Initialize the Entity
            var newUser = new UserEntity
            {
                // Accessing properties from your request DTO
                FirstName = request.Payload.FirstName,
                LastName = request.Payload.LastName,
                EmailUsername = request.Payload.EmailUsername,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Payload.PasswordHash),
                Active = true,
                IsEmailVerified = false,
                VerificationCode = Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                VerificationCodeExpiry = DateTime.UtcNow.AddHours(24),
                UserSubDomain = generatedSubDomain,
                ApiKeyForNightScout = generatedApiKey,
                CountryListID = request.Payload.CountryListID,
                ProvinceStateListID = request.Payload.ProvinceStateListID,
                DockerStatus = DockerContainerStatus.Pending.ToString()
            }; 

            try
            {
                // C. Save to Postgres
                _context.UserEntities.Add(newUser); // Fixed: using 'newUser'
                await _context.SaveChangesAsync();

                // D. Map to your ViewModel
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
                    Message = "User created successfully. Provisioning container...",
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
    }

}
