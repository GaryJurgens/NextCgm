using NextCgm.Shared.ApiViewModels;

namespace NextCgm.Shared.DTOS
{
    public class CreateSubscriptionRequestDTO
    {
        public Guid UserEntityID { get; set; }
        public string SubscriptionPlanId { get; set; } = string.Empty;
        public string PaystackSubscriptionCode { get; set; } = string.Empty;
        public string PaystackCustomerCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CurrentPeriodStart { get; set; }
        public DateTime CurrentPeriodEnd { get; set; }
    }

    public class CreateSubscriptionResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public BillingSubscriptionApiViewModel? Payload { get; set; }

        public static CreateSubscriptionResponseDTO Failure(string msg)
        {
            return new CreateSubscriptionResponseDTO
            {
                Success = false,
                Message = msg,
                Payload = null
            };
        }
    }

    public class GetSubscriptionRequestDTO
    {
        public Guid BillingSubscriptionEntityID { get; set; }
    }

    public class GetSubscriptionResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public BillingSubscriptionApiViewModel? Payload { get; set; }

        public static GetSubscriptionResponseDTO Failure(string msg)
        {
            return new GetSubscriptionResponseDTO
            {
                Success = false,
                Message = msg,
                Payload = null
            };
        }
    }
    
    public class GetUserSubscriptionsRequestDTO
    {
        public Guid UserEntityID { get; set; }
    }

    public class GetUserSubscriptionsResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<BillingSubscriptionApiViewModel>? Payload { get; set; }

        public static GetUserSubscriptionsResponseDTO Failure(string msg)
        {
            return new GetUserSubscriptionsResponseDTO
            {
                Success = false,
                Message = msg,
                Payload = null
            };
        }
    }
}
