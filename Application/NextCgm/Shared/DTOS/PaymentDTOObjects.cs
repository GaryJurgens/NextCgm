namespace NextCgm.Shared.DTOS
{
    public class InitiatePaymentRequestDTO
    {
        public Guid UserEntityID { get; set; }
        public string Email { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PlanId { get; set; } = string.Empty;
    }

    public class InitiatePaymentResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string AuthorizationUrl { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
    }

    public class VerifyTransactionRequestDTO
    {
        public string Reference { get; set; } = string.Empty;
    }

    public class VerifyTransactionResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    // Paystack Models
    public class PaystackInitializeRequest
    {
        public string amount { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string reference { get; set; } = string.Empty;
        public string callback_url { get; set; } = string.Empty;
        public string currency { get; set; } = "ZAR"; // Or whatever currency is configured
        public string[] channels { get; set; } = new[] { "card" };
        public string? plan { get; set; }
        public object? metadata { get; set; }
    }

    public class PaystackInitializeResponse
    {
        public bool status { get; set; }
        public string message { get; set; } = string.Empty;
        public PaystackInitializeData data { get; set; } = new();
    }

    public class PaystackInitializeData
    {
        public string authorization_url { get; set; } = string.Empty;
        public string access_code { get; set; } = string.Empty;
        public string reference { get; set; } = string.Empty;
    }

    public class PaystackVerifyResponse
    {
        public bool status { get; set; }
        public string message { get; set; } = string.Empty;
        public PaystackVerifyData data { get; set; } = new();
    }

    public class PaystackVerifyData
    {
        public string status { get; set; } = string.Empty;
        public string reference { get; set; } = string.Empty;
        public decimal amount { get; set; }
        public PaystackCustomer customer { get; set; } = new();
        public PaystackAuthorization authorization { get; set; } = new();
        public System.Text.Json.JsonElement? metadata { get; set; }
    }

    public class PaystackCustomer
    {
        public string email { get; set; } = string.Empty;
        public string customer_code { get; set; } = string.Empty;
    }

    public class PaystackAuthorization
    {
        public string authorization_code { get; set; } = string.Empty;
        public string card_type { get; set; } = string.Empty;
        public string last4 { get; set; } = string.Empty;
        public string exp_month { get; set; } = string.Empty;
        public string exp_year { get; set; } = string.Empty;
        public string bank { get; set; } = string.Empty;
        public string brand { get; set; } = string.Empty;
        public string signature { get; set; } = string.Empty;
        public bool reusable { get; set; }
    }

    public class PaystackChargeRequest
    {
        public string amount { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string authorization_code { get; set; } = string.Empty;
        public string reference { get; set; } = string.Empty;
    }
}