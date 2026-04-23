using NextCgm.DataEntities.User;

namespace NextCgm.DataEntities.Billing
{
    public class BillingSubscriptionEntity
    {
        public Guid BillingSubscriptionEntityID { get; set; } = Medo.Uuid7.NewUuid7();
        
        public Guid UserEntityID { get; set; }
        public UserEntity? User { get; set; }

        public string SubscriptionPlanId { get; set; } = string.Empty; // e.g., "basic", "premium"
        public string Status { get; set; } = string.Empty; // e.g., "active", "canceled", "past_due"
        
        public DateTime CurrentPeriodStart { get; set; }
        public DateTime CurrentPeriodEnd { get; set; }
        public DateTime? CanceledAt { get; set; }

        public string PaystackSubscriptionCode { get; set; } = string.Empty;
        public string PaystackCustomerCode { get; set; } = string.Empty;

        // Retry logic tracking
        public int FailedChargeAttempts { get; set; } = 0;
        public DateTime? NextRetryDate { get; set; }
        public DateTime? GracePeriodEndDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
