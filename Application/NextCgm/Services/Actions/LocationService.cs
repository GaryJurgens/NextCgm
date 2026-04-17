using Microsoft.EntityFrameworkCore;
using NextCgm.DContentext;
using NextCgm.Shared.DTOS;

namespace NextCgm.Services.Actions
{
    public class LocationService : ILocationService
    {
        private readonly AppDBContext _context;

        public LocationService(AppDBContext context)
        {
            _context = context;
        }

        public async Task<GetCountriesResponseDTO> GetCountriesAsync()
        {
            try
            {
                var countries = await _context.CountryLists
                    .OrderBy(c => c.Name)
                    .Select(c => new CountryDTO
                    {
                        CountryListID = c.CountryListID,
                        Name = c.Name,
                        Iso2 = c.Iso2
                    })
                    .ToListAsync();

                return new GetCountriesResponseDTO
                {
                    Success = true,
                    Message = "Countries retrieved successfully",
                    Payload = countries
                };
            }
            catch (Exception ex)
            {
                return new GetCountriesResponseDTO
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<GetStatesResponseDTO> GetStatesByCountryAsync(Guid countryId)
        {
            try
            {
                var states = await _context.ProvinceStateLists
                    .Where(s => s.CountryListID == countryId)
                    .OrderBy(s => s.Name)
                    .Select(s => new ProvinceStateDTO
                    {
                        ProvinceStateListID = s.ProvinceStateListID,
                        Name = s.Name,
                        StateCode = s.StateCode ?? string.Empty
                    })
                    .ToListAsync();

                return new GetStatesResponseDTO
                {
                    Success = true,
                    Message = "States retrieved successfully",
                    Payload = states
                };
            }
            catch (Exception ex)
            {
                return new GetStatesResponseDTO
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<GetTimeZonesResponseDTO> GetTimeZonesByCountryAsync(Guid countryId)
        {
            try
            {
                var timeZones = await _context.TimeZoneData
                    .Where(t => t.CountryListID == countryId)
                    .OrderBy(t => t.ZoneName)
                    .Select(t => new TimeZoneDTO
                    {
                        TimeZoneDataID = t.TimeZoneDataID,
                        ZoneName = t.ZoneName,
                        GmtOffsetName = t.GmtOffsetName
                    })
                    .ToListAsync();

                return new GetTimeZonesResponseDTO
                {
                    Success = true,
                    Message = "Time zones retrieved successfully",
                    Payload = timeZones
                };
            }
            catch (Exception ex)
            {
                return new GetTimeZonesResponseDTO
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }
    }
}
