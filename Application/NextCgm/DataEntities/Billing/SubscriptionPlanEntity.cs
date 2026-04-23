using System.ComponentModel.DataAnnotations;

namespace NextCgm.DataEntities.Billing
{
    public class SubscriptionPlanEntity
    {
        [Key]
        public Guid SubscriptionPlanID { get; set; } = Medo.Uuid7.NewUuid7();
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        public string FeaturesHtml { get; set; } = string.Empty;

        // Price in South African Rands (ZAR)
        public decimal PriceZAR { get; set; }
        
        // Price in US Dollars (USD)
        public decimal PriceUSD { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        // Interval in months (e.g., 1 for monthly, 12 for annual)
        public int BillingCycleMonths { get; set; } = 1;
        
        public string PaystackPlanCode { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
    }
}