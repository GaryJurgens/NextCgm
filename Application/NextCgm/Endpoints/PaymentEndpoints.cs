using FastEndpoints;
using NextCgm.Services.Actions;
using NextCgm.Shared.DTOS;

namespace NextCgm.Endpoints
{
    public class InitiatePaymentEndpoint : Endpoint<InitiatePaymentRequestDTO, InitiatePaymentResponseDTO>
    {
        private readonly IPaymentService _paymentService;

        public InitiatePaymentEndpoint(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        public override void Configure()
        {
            Post("/api/Payment/Initiate");
            AllowAnonymous(); // Adjust as needed based on auth requirements
            Summary(s =>
            {
                s.Summary = "Initiate a Paystack payment";
                s.Description = "Initializes a payment transaction with Paystack and returns an authorization URL.";
            });
        }

        public override async Task HandleAsync(InitiatePaymentRequestDTO req, CancellationToken ct)
        {
            var response = await _paymentService.InitiatePaymentAsync(req);

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

    public class VerifyTransactionEndpoint : Endpoint<VerifyTransactionRequestDTO, VerifyTransactionResponseDTO>
    {
        private readonly IPaymentService _paymentService;

        public VerifyTransactionEndpoint(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        public override void Configure()
        {
            Post("/api/Payment/Verify");
            AllowAnonymous();
            Summary(s =>
            {
                s.Summary = "Verify a Paystack transaction";
                s.Description = "Verifies a completed transaction with Paystack using its reference.";
            });
        }

        public override async Task HandleAsync(VerifyTransactionRequestDTO req, CancellationToken ct)
        {
            var response = await _paymentService.VerifyTransactionAsync(req);

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