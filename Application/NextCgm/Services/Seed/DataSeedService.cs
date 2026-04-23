using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NextCgm.DataEntities.Locations;

namespace NextCgm.Services.Seed
{
    public interface IDataSeedService
    {
        Task<bool> IsDataSeededAsync();

        Task SeedCountriesAndStatesAsync();
    }

    public class DataSeedService : IDataSeedService
    {
        private readonly DContentext.AppDBContext _context;
        private readonly ILogger<DataSeedService> _logger;

        public DataSeedService(DContentext.AppDBContext context, ILogger<DataSeedService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> IsDataSeededAsync()
        {
            return await _context.CountryLists.AnyAsync();
        }

        public async Task SeedCountriesAndStatesAsync()
        {
            try
            {
                if (await IsDataSeededAsync())
                {
                    _logger.LogInformation("Data already seeded, skipping seed operation");
                    return;
                }

                _logger.LogInformation("Starting database seeding...");

                var jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DBSeed/SeedData", "countries+states.json");

                if (!File.Exists(jsonPath))
                {
                    _logger.LogError("Countries+states.json file not found at: {JsonPath}", jsonPath);
                    return;
                }

                var jsonContent = await File.ReadAllTextAsync(jsonPath);
                var countriesData = JsonConvert.DeserializeObject<List<CountryData>>(jsonContent);

                if (countriesData == null || !countriesData.Any())
                {
                    _logger.LogError("No country data found in JSON file");
                    return;
                }

                var countries = new List<CountryList>();
                var states = new List<ProvinceStateList>();
                var timezones = new List<TimeZoneData>();

                foreach (var countryData in countriesData)
                {
                    var countryId = Medo.Uuid7.NewUuid7().ToGuid();

                    var country = new CountryList
                    {
                        CountryListID = countryId,
                        Name = countryData.Name,
                        Iso2 = countryData.Iso2,
                        Iso3 = countryData.Iso3,
                        NumericCode = countryData.NumericCode,
                        PhoneCode = countryData.PhoneCode,
                        Capital = countryData.Capital,
                        Currency = countryData.Currency,
                        CurrencyName = countryData.CurrencyName,
                        CurrencySymbol = countryData.CurrencySymbol,
                        Tld = countryData.Tld,
                        Native = countryData.Native,
                        Region = countryData.Region,
                        RegionId = countryData.RegionId,
                        Subregion = countryData.Subregion,
                        SubregionId = countryData.SubregionId,
                        Nationality = countryData.Nationality,
                        Latitude = countryData.Latitude,
                        Longitude = countryData.Longitude,
                        Emoji = countryData.Emoji,
                        EmojiU = countryData.EmojiU
                    };

                    countries.Add(country);

                    // Add timezones for this country
                    if (countryData.Timezones != null)
                    {
                        foreach (var tzData in countryData.Timezones)
                        {
                            var timezone = new TimeZoneData
                            {
                                TimeZoneDataID = Medo.Uuid7.NewUuid7().ToGuid(),
                                ZoneName = tzData.ZoneName,
                                GmtOffset = tzData.GmtOffset,
                                GmtOffsetName = tzData.GmtOffsetName,
                                Abbreviation = tzData.Abbreviation,
                                TzName = tzData.TzName,
                                CountryListID = countryId
                            };

                            timezones.Add(timezone);
                        }
                    }

                    // Add states for this country
                    if (countryData.States != null)
                    {
                        foreach (var stateData in countryData.States)
                        {
                            var state = new ProvinceStateList
                            {
                                ProvinceStateListID = Medo.Uuid7.NewUuid7().ToGuid(),
                                Name = stateData.Name,
                                StateCode = stateData.StateCode,
                                Type = stateData.Type,
                                Latitude = stateData.Latitude,
                                Longitude = stateData.Longitude,
                                CountryListID = countryId
                            };

                            states.Add(state);
                        }
                    }
                }

                // Bulk insert all entities
                await _context.CountryLists.AddRangeAsync(countries);
                await _context.TimeZoneData.AddRangeAsync(timezones);
                await _context.ProvinceStateLists.AddRangeAsync(states);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Database seeding completed successfully. Added {CountryCount} countries, {StateCount} states, {TimezoneCount} timezones",
                    countries.Count, states.Count, timezones.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while seeding database");
                throw;
            }
        }
    }

    // JSON mapping classes
    public class CountryData
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("iso3")]
        public string Iso3 { get; set; } = string.Empty;

        [JsonProperty("iso2")]
        public string Iso2 { get; set; } = string.Empty;

        [JsonProperty("numeric_code")]
        public string? NumericCode { get; set; }

        [JsonProperty("phonecode")]
        public string? PhoneCode { get; set; }

        [JsonProperty("capital")]
        public string? Capital { get; set; }

        [JsonProperty("currency")]
        public string? Currency { get; set; }

        [JsonProperty("currency_name")]
        public string? CurrencyName { get; set; }

        [JsonProperty("currency_symbol")]
        public string? CurrencySymbol { get; set; }

        [JsonProperty("tld")]
        public string? Tld { get; set; }

        [JsonProperty("native")]
        public string? Native { get; set; }

        [JsonProperty("region")]
        public string? Region { get; set; }

        [JsonProperty("region_id")]
        public int? RegionId { get; set; }

        [JsonProperty("subregion")]
        public string? Subregion { get; set; }

        [JsonProperty("subregion_id")]
        public int? SubregionId { get; set; }

        [JsonProperty("nationality")]
        public string? Nationality { get; set; }

        [JsonProperty("latitude")]
        public decimal? Latitude { get; set; }

        [JsonProperty("longitude")]
        public decimal? Longitude { get; set; }

        [JsonProperty("emoji")]
        public string? Emoji { get; set; }

        [JsonProperty("emojiU")]
        public string? EmojiU { get; set; }

        [JsonProperty("timezones")]
        public List<Timezones>? Timezones { get; set; }

        [JsonProperty("states")]
        public List<StateData>? States { get; set; }
    }

    public class Timezones
    {
        [JsonProperty("zoneName")]
        public string ZoneName { get; set; } = string.Empty;

        [JsonProperty("gmtOffset")]
        public int GmtOffset { get; set; }

        [JsonProperty("gmtOffsetName")]
        public string GmtOffsetName { get; set; } = string.Empty;

        [JsonProperty("abbreviation")]
        public string? Abbreviation { get; set; }

        [JsonProperty("tzName")]
        public string? TzName { get; set; }
    }

    public class StateData
    {
        [JsonProperty("id")]
        public int OriginalId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("state_code")]
        public string? StateCode { get; set; }

        [JsonProperty("latitude")]
        public decimal? Latitude { get; set; }

        [JsonProperty("longitude")]
        public decimal? Longitude { get; set; }

        [JsonProperty("type")]
        public string? Type { get; set; }
    }
}