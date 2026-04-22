using System.ComponentModel.DataAnnotations;
using NextCgm.DataEntities.User;

namespace NextCgm.DataEntities.Billing
{
    public class PaymentTransactionEntity
    {
        [Key]
        public Guid PaymentTransactionID { get; set; } = Medo.Uuid7.NewUuid7();
        
        public Guid UserEntityID { get; set; }
        public UserEntity? User { get; set; }

        public Guid? BillingSubscriptionEntityID { get; set; }
        public BillingSubscriptionEntity? BillingSubscription { get; set; }

        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty; // "Pending", "Complete", "Failed"

        public string PaystackReference { get; set; } = string.Empty;
        public string PaystackAuthorizationCode { get; set; } = string.Empty;

        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
        public string RawResponse { get; set; } = string.Empty;
    }
}