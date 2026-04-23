using FastEndpoints;
using Microsoft.EntityFrameworkCore;
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

    public class GetPlansRequestDTO {
        public Guid? UserId { get; set; }
    }
    public class GetPlansResponseDTO {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<PlanDTO> Plans { get; set; } = new();
    }
    public class PlanDTO {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string FeaturesHtml { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string PaystackPlanCode { get; set; } = string.Empty;
        public string CurrencySymbol { get; set; } = string.Empty;
    }

    public class GetPlansEndpoint : Endpoint<GetPlansRequestDTO, GetPlansResponseDTO>
    {
        private readonly NextCgm.DContentext.AppDBContext _dbContext;

        public GetPlansEndpoint(NextCgm.DContentext.AppDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public override void Configure()
        {
            Post("/api/Billing/GetPlans");
            AllowAnonymous();
            Summary(s => {
                s.Summary = "Get active subscription plans";
                s.Description = "Retrieves all active subscription plans from the database.";
            });
        }

        public override async Task HandleAsync(GetPlansRequestDTO req, CancellationToken ct)
        {
            bool isSouthAfrica = false;
            string currencySymbol = "$";

            if (req.UserId.HasValue)
            {
                var user = await _dbContext.UserEntities
                    .Include(u => u.Country)
                    .FirstOrDefaultAsync(u => u.UserEntityID == req.UserId.Value, ct);

                if (user != null)
                {
                    if (user.CountryName.Equals("South Africa", StringComparison.OrdinalIgnoreCase) || 
                        user.CountryCode.Equals("ZA", StringComparison.OrdinalIgnoreCase))
                    {
                        isSouthAfrica = true;
                    }
                    else if (user.Country != null && 
                            (user.Country.Name.Equals("South Africa", StringComparison.OrdinalIgnoreCase) || 
                             user.Country.Iso2.Equals("ZA", StringComparison.OrdinalIgnoreCase)))
                    {
                        isSouthAfrica = true;
                    }

                    if (user.Country != null && !string.IsNullOrEmpty(user.Country.CurrencySymbol))
                    {
                        currencySymbol = user.Country.CurrencySymbol;
                    }
                    else if (isSouthAfrica)
                    {
                        currencySymbol = "R";
                    }
                }
            }

            var dbPlans = await _dbContext.SubscriptionPlans
                .Where(p => p.IsActive)
                .ToListAsync(ct);

            var plans = dbPlans.Select(p => new PlanDTO {
                Id = p.SubscriptionPlanID,
                Name = p.Name,
                Description = p.Description,
                FeaturesHtml = p.FeaturesHtml,
                Price = isSouthAfrica ? p.PriceZAR : p.PriceUSD,
                PaystackPlanCode = p.PaystackPlanCode,
                CurrencySymbol = currencySymbol
            }).ToList();

            await Send.OkAsync(new GetPlansResponseDTO {
                Success = true,
                Plans = plans
            }, ct);
        }
    }

    public class CancelSubscriptionEndpoint : Endpoint<CancelSubscriptionRequestDTO, CancelSubscriptionResponseDTO>
    {
        private readonly IBillingSubscriptionService _billingService;

        public CancelSubscriptionEndpoint(IBillingSubscriptionService billingService)
        {
            _billingService = billingService;
        }

        public override void Configure()
        {
            Post("/api/Billing/CancelSubscription");
            AllowAnonymous();
            Summary(s =>
            {
                s.Summary = "Cancel subscription";
                s.Description = "Cancels a recurring billing subscription.";
            });
        }

        public override async Task HandleAsync(CancelSubscriptionRequestDTO req, CancellationToken ct)
        {
            var response = await _billingService.CancelSubscriptionAsync(req);

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
