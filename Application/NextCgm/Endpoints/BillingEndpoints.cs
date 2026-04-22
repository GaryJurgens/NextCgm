using FastEndpoints;
using NextCgm.Services.Actions;
using NextCgm.Shared.DTOS;

namespace NextCgm.Endpoints
{
    public class CreateSubscriptionEndpoint : Endpoint<CreateSubscriptionRequestDTO, CreateSubscriptionResponseDTO>
    {
        private readonly IBillingSubscriptionService _billingService;

        public CreateSubscriptionEndpoint(IBillingSubscriptionService billingService)
        {
            _billingService = billingService;
        }

        public override void Configure()
        {
            Post("/api/Billing/CreateSubscription");
            AllowAnonymous(); // Depending on auth setup, adjust this
            Summary(s =>
            {
                s.Summary = "Create a new billing subscription";
                s.Description = "Creates a new subscription record for a user.";
            });
        }

        public override async Task HandleAsync(CreateSubscriptionRequestDTO req, CancellationToken ct)
        {
            var response = await _billingService.CreateSubscriptionAsync(req);

            if (response.Success)
            {
                await Send.OkAsync(response, ct);
            }
            else
            {
                await Send.OkAsync(response,  ct);
            }
        }
    }

    public class GetSubscriptionEndpoint : Endpoint<GetSubscriptionRequestDTO, GetSubscriptionResponseDTO>
    {
        private readonly IBillingSubscriptionService _billingService;

        public GetSubscriptionEndpoint(IBillingSubscriptionService billingService)
        {
            _billingService = billingService;
        }

        public override void Configure()
        {
            Post("/api/Billing/GetSubscription");
            AllowAnonymous();
            Summary(s =>
            {
                s.Summary = "Get billing subscription by ID";
                s.Description = "Retrieves a specific subscription record.";
            });
        }

        public override async Task HandleAsync(GetSubscriptionRequestDTO req, CancellationToken ct)
        {
            var response = await _billingService.GetSubscriptionAsync(req);

            if (response.Success)
            {
                await Send.OkAsync(response, ct);
            }
            else
            {
                await Send.OkAsync(response, ct);
            }
        }
    }

    public class GetUserSubscriptionsEndpoint : Endpoint<GetUserSubscriptionsRequestDTO, GetUserSubscriptionsResponseDTO>
    {
        private readonly IBillingSubscriptionService _billingService;

        public GetUserSubscriptionsEndpoint(IBillingSubscriptionService billingService)
        {
            _billingService = billingService;
        }

        public override void Configure()
        {
            Post("/api/Billing/GetUserSubscriptions");
            AllowAnonymous();
            Summary(s =>
            {
                s.Summary = "Get all subscriptions for a user";
                s.Description = "Retrieves all subscription records for a specific user.";
            });
        }

        public override async Task HandleAsync(GetUserSubscriptionsRequestDTO req, CancellationToken ct)
        {
            var response = await _billingService.GetUserSubscriptionsAsync(req);

            if (response.Success)
            {
                await Send.OkAsync(response, ct);
            }
            else
            {
                await Send.OkAsync(response, ct);
            }
        }
    }
}
