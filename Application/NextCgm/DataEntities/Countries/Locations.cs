using Medo;
using Newtonsoft.Json;
using NextCgm.DataEntities.User;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NextCgm.DataEntities.Locations
{
    /// <summary>
    /// A lookup table containing comprehensive country details (ISO codes, currency, etc.).
    /// Acts as the parent entity for ProvinceStateList and TimeZones to provide a geographical hierarchy.
    /// </summary>
    public class CountryList
    {
        public Guid CountryListID = Uuid7.NewGuid();

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(3)]
        public string Iso2 { get; set; } = string.Empty;

        [Required]
        [MaxLength(3)]
        public string Iso3 { get; set; } = string.Empty;

        [MaxLength(10)]
        public string? NumericCode { get; set; }

        [MaxLength(10)]
        public string? PhoneCode { get; set; }

        [MaxLength(100)]
        public string? Capital { get; set; }

        [MaxLength(10)]
        public string? Currency { get; set; }

        [MaxLength(100)]
        public string? CurrencyName { get; set; }

        [MaxLength(10)]
        public string? CurrencySymbol { get; set; }

        [MaxLength(10)]
        public string? Tld { get; set; }

        [MaxLength(100)]
        public string? Native { get; set; }

        [MaxLength(50)]
        public string? Region { get; set; }

        public int? RegionId { get; set; }

        [MaxLength(50)]
        public string? Subregion { get; set; }

        public int? SubregionId { get; set; }

        [MaxLength(100)]
        public string? Nationality { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        [MaxLength(10)]
        public string? Emoji { get; set; }

        [MaxLength(50)]
        public string? EmojiU { get; set; }

        // Navigation properties
        public ICollection<ProvinceStateList> ProvinceStates { get; set; } = new List<ProvinceStateList>();

        public ICollection<TimeZones> TimeZones { get; set; } = new List<TimeZones>();

        public ICollection<UserEntity> Users { get; set; } = new List<UserEntity>();
    }

    /// <summary>
    /// Represents states or provinces within a country.
    /// Links to CountryList to establish the geographical parent-child relationship.
    /// </summary>
    public class ProvinceStateList
    {
        public Guid ProvinceStateListID = Medo.Uuid7.NewGuid();

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(10)]
        public string? StateCode { get; set; }

        [MaxLength(50)]
        public string? Type { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        // Foreign Keys
        [Required]
        public Guid CountryListID { get; set; }

        public CountryList Country { get; set; } = null!;

        public ICollection<UserEntity> Users { get; set; } = new List<UserEntity>();
    }

    /// <summary>
    /// Represents time zones.
    /// Links to CountryList to associate valid time zones with specific countries.
    /// </summary>
    public class TimeZones
    {
        public Guid TimeZoneID { get; set; } = Medo.Uuid7.NewGuid();

        [Required]
        [MaxLength(100)]
        public string ZoneName { get; set; } = string.Empty;

        /// <summary>
        /// GMT offset in seconds
        /// </summary>
        [Required]
        public int GmtOffset { get; set; }

        [Required]
        [MaxLength(20)]
        public string GmtOffsetName { get; set; } = string.Empty;

        [MaxLength(10)]
        public string? Abbreviation { get; set; }

        [MaxLength(100)]
        public string? TzName { get; set; }

        // Computed property for backward compatibility
        [NotMapped]
        public string Name => ZoneName;

        // Foreign Keys
        [Required]
        public Guid CountryListID { get; set; }

        public CountryList Country { get; set; } = null!;

        public ICollection<UserEntity> Users { get; } = new List<UserEntity>();
    }
}