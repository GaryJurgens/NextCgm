using NextCgm.DataEntities.Locations;

namespace NextCgm.DataEntities.User
{
    public class UserEntity
    {

        public Guid UserEntityID { get; set; }   = Medo.Uuid7.NewUuid7();
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

        public Guid TimeZoneID { get; set; }
       




    }
}
