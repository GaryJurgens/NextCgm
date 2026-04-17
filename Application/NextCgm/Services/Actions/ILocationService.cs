using NextCgm.Shared.DTOS;

namespace NextCgm.Services.Actions
{
    public interface ILocationService
    {
        Task<GetCountriesResponseDTO> GetCountriesAsync();
        Task<GetStatesResponseDTO> GetStatesByCountryAsync(Guid countryId);
        Task<GetTimeZonesResponseDTO> GetTimeZonesByCountryAsync(Guid countryId);
    }
}
