namespace NextCgm.Shared.ApiViewModels
{
    public class BillingSubscriptionApiViewModel
    {
        public Guid BillingSubscriptionEntityID { get; set; }
        public Guid UserEntityID { get; set; }
        public string SubscriptionPlanId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CurrentPeriodStart { get; set; }
        public DateTime CurrentPeriodEnd { get; set; }
        public DateTime? CanceledAt { get; set; }
        public int FailedChargeAttempts { get; set; } = 0;
        public DateTime? NextRetryDate { get; set; }
        public DateTime? GracePeriodEndDate { get; set; }
        public string PaystackSubscriptionCode { get; set; } = string.Empty;
        public string PaystackCustomerCode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
    }
}
