using FastEndpoints;
using NextCgm.Services.Actions;
using NextCgm.Shared.DTOS;

namespace NextCgm.Endpoints
{
    public class GetCountriesEndpoint : EndpointWithoutRequest<GetCountriesResponseDTO>
    {
        private readonly ILocationService _locationService;

        public GetCountriesEndpoint(ILocationService locationService)
        {
            _locationService = locationService;
        }

        public override void Configure()
        {
            Get("/api/Locations/Countries");
            AllowAnonymous();
            Summary(s =>
            {
                s.Summary = "Get all countries";
                s.Description = "Retrieves a list of all countries.";
            });
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            try
            {
                var response = await _locationService.GetCountriesAsync();
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

    public class GetStatesEndpoint : EndpointWithoutRequest<GetStatesResponseDTO>
    {
        private readonly ILocationService _locationService;

        public GetStatesEndpoint(ILocationService locationService)
        {
            _locationService = locationService;
        }

        public override void Configure()
        {
            Get("/api/Locations/States/{CountryId}");
            AllowAnonymous();
            Summary(s =>
            {
                s.Summary = "Get states by country";
                s.Description = "Retrieves a list of states for a specific country.";
            });
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            try
            {
                var countryIdStr = Route<string>("CountryId");
                if (!Guid.TryParse(countryIdStr, out var countryId))
                {
                    ThrowError("Invalid Country ID", 400);
                    return;
                }

                var response = await _locationService.GetStatesByCountryAsync(countryId);
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

    public class GetTimeZonesEndpoint : EndpointWithoutRequest<GetTimeZonesResponseDTO>
    {
        private readonly ILocationService _locationService;

        public GetTimeZonesEndpoint(ILocationService locationService)
        {
            _locationService = locationService;
        }

        public override void Configure()
        {
            Get("/api/Locations/TimeZones/{CountryId}");
            AllowAnonymous();
            Summary(s =>
            {
                s.Summary = "Get time zones by country";
                s.Description = "Retrieves a list of time zones for a specific country.";
            });
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            try
            {
                var countryIdStr = Route<string>("CountryId");
                if (!Guid.TryParse(countryIdStr, out var countryId))
                {
                    ThrowError("Invalid Country ID", 400);
                    return;
                }

                var response = await _locationService.GetTimeZonesByCountryAsync(countryId);
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
