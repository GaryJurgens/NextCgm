using System.ComponentModel.DataAnnotations;
using NextCgm.DataEntities.User;

namespace NextCgm.DataEntities.Billing
{
    public class PaymentMethodEntity
    {
        [Key]
        public Guid PaymentMethodID { get; set; } = Medo.Uuid7.NewUuid7();
        
        public Guid UserEntityID { get; set; }
        public UserEntity? User { get; set; }

        public string AuthorizationCode { get; set; } = string.Empty;
        public string CardType { get; set; } = string.Empty;
        public string Last4 { get; set; } = string.Empty;
        public string ExpMonth { get; set; } = string.Empty;
        public string ExpYear { get; set; } = string.Empty;
        public string Bank { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Signature { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        
        public bool Reusable { get; set; }
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}