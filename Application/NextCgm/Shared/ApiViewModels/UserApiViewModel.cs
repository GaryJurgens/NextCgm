using NextCgm.DataEntities.Locations;
using NextCgm.Helpers.Utils;

namespace NextCgm.Shared.ApiViewModels
{
    public class UserApiViewModel
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string EmailUsername { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        public bool Active { get; set; }

        public DateTime LastLogin { get; set; }

        public bool IsEmailVerified { get; set; }

        public string VerificationCode { get; set; } = string.Empty;
        public DateTime VerificationCodeSentTime { get; set; }

        public DateTime VerificationCodeExpiry { get; set; }

        public string MobileNumber { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

        public Guid CountryListID { get; set; }
        public CountryList? Country { get; set; }

        public string CountryName { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;

        public Guid ProvinceStateListID { get; set; }

        public string ApiKeyForNightScout { get; set; } = string.Empty; // like oros450 onlylowesrcase, no special characters, and must be unique across all users. This is used for authenticating API requests from the user's Nightscout instance to the backend server, and for mapping to the user's container and database. This can be a combination of the user's first name, last name, and a unique identifier, such as Dolfie1234567890, where Dolfie is the user's first name, and 1234567890 is a unique identifier for this user. This is used to create a unique API key and to identify the user in the system.

        public string UserSubDomain { get; set; } = string.Empty; // like dolfie.nextcgm.com, where Dolfie is the user subdomain, and nextcgm.com is the main domain. This is used for mapping to the host machine and for creating a unique URL for the user to access their container.

        // both the apikey and UserSubDomain generate a url like this https://oros450.dolfie.nextgcm.com/api/v1/

        public Guid TimeZoneID { get; set; }

        public ICollection<DockerContainerStatus> DockerContainers { get; set; } = new List<DockerContainerStatus>();

        public string DockerStatus { get; set; }
    }
}