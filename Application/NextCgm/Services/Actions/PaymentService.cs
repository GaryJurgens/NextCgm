using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NextCgm.DContentext;
using NextCgm.Helpers.Utils;
using NextCgm.Shared.DTOS;
using NextCgm.DataEntities.Billing;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace NextCgm.Services.Actions
{
    public interface IPaymentService
    {
        Task<InitiatePaymentResponseDTO> InitiatePaymentAsync(InitiatePaymentRequestDTO request);
        Task<VerifyTransactionResponseDTO> VerifyTransactionAsync(VerifyTransactionRequestDTO request);
    }

    public class PaymentService : IPaymentService
    {
        private readonly AppDBContext _dbContext;
        private readonly PaystackSettings _paystackSettings;
        private readonly ILogger<PaymentService> _logger;
        private readonly HttpClient _httpClient;

        public PaymentService(AppDBContext dbContext, IOptions<PaystackSettings> paystackSettings, ILogger<PaymentService> logger)
        {
            _dbContext = dbContext;
            _paystackSettings = paystackSettings.Value;
            _logger = logger;
            
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://api.paystack.co/");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _paystackSettings.SecretKey);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<InitiatePaymentResponseDTO> InitiatePaymentAsync(InitiatePaymentRequestDTO request)
        {
            try
            {
                var user = await _dbContext.UserEntities
                    .Include(u => u.Country)
                    .FirstOrDefaultAsync(u => u.UserEntityID == request.UserEntityID);
                    
                if (user == null)
                {
                    return new InitiatePaymentResponseDTO { Success = false, Message = "User not found" };
                }

                if (!Guid.TryParse(request.PlanId, out var planGuid))
                {
                    return new InitiatePaymentResponseDTO { Success = false, Message = "Invalid Plan ID" };
                }

                var plan = await _dbContext.SubscriptionPlans.FirstOrDefaultAsync(p => p.SubscriptionPlanID == planGuid);
                if (plan == null)
                {
                    return new InitiatePaymentResponseDTO { Success = false, Message = "Subscription Plan not found" };
                }

                // Determine currency and price based on user's country
                bool isSouthAfrica = false;
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

                // Force ZAR for Paystack as South African Paystack accounts only support ZAR transactions.
                // The frontend will display the USD price to non-SA users, but the actual charge will be processed in ZAR.
                string currency = "ZAR";
                decimal amountToCharge = plan.PriceZAR;

                var reference = Guid.NewGuid().ToString();
                var amountInKobo = (int)(amountToCharge * 100);

                var paystackRequest = new PaystackInitializeRequest
                {
                    amount = amountInKobo.ToString(),
                    email = request.Email,
                    reference = reference,
                    callback_url = _paystackSettings.CallbackUrl,
                    currency = currency,
                    channels = new[] { "card" }, // Force card channel to ensure we get a reusable authorization code
                    metadata = new {
                        userId = request.UserEntityID.ToString(),
                        planId = request.PlanId,
                        custom_fields = new[]
                        {
                            new { display_name = "User ID", variable_name = "userId", value = request.UserEntityID.ToString() },
                            new { display_name = "Plan ID", variable_name = "planId", value = request.PlanId }
                        }
                    }
                };

                // Track transaction as Pending
                var transaction = new PaymentTransactionEntity
                {
                    UserEntityID = request.UserEntityID,
                    Amount = amountToCharge,
                    Currency = currency,
                    Status = "Pending",
                    PaystackReference = reference,
                    TransactionDate = DateTime.UtcNow
                };
                _dbContext.PaymentTransactions.Add(transaction);
                await _dbContext.SaveChangesAsync();

                var content = JsonContent.Create(paystackRequest);
                var response = await _httpClient.PostAsync("transaction/initialize", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Paystack Initialization Failed: {StatusCode} {Content}", response.StatusCode, errorContent);
                    return new InitiatePaymentResponseDTO { Success = false, Message = "Failed to initialize payment with Paystack" };
                }

                var result = await response.Content.ReadFromJsonAsync<PaystackInitializeResponse>();

                if (result == null || !result.status)
                {
                    return new InitiatePaymentResponseDTO { Success = false, Message = result?.message ?? "Paystack error" };
                }

                return new InitiatePaymentResponseDTO
                {
                    Success = true,
                    Message = "Payment initialized successfully",
                    AuthorizationUrl = result.data.authorization_url,
                    Reference = reference
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Paystack Payment Initiation Error");
                return new InitiatePaymentResponseDTO { Success = false, Message = $"Error: {ex.Message}" };
            }
        }

        public async Task<VerifyTransactionResponseDTO> VerifyTransactionAsync(VerifyTransactionRequestDTO request)
        {
            try
            {
                var response = await _httpClient.GetAsync($"transaction/verify/{request.Reference}");
                if (!response.IsSuccessStatusCode)
                {
                    return new VerifyTransactionResponseDTO { Success = false, Message = "Failed to verify transaction with Paystack" };
                }

                var result = await response.Content.ReadFromJsonAsync<PaystackVerifyResponse>();

                if (result == null || !result.status)
                {
                    return new VerifyTransactionResponseDTO { Success = false, Message = result?.message ?? "Paystack verify error" };
                }

                if (result.data.status == "success")
                {
                    var transaction = await _dbContext.PaymentTransactions.FirstOrDefaultAsync(t => t.PaystackReference == request.Reference);
                    
                    if (transaction != null)
                    {
                        transaction.Status = "Complete";
                        transaction.RawResponse = System.Text.Json.JsonSerializer.Serialize(result);
                    }

                    // Save Payment Method for future recurring charges
                    Guid? userId = transaction?.UserEntityID;
                    if (userId.HasValue && result.data.authorization != null && result.data.authorization.reusable)
                    {
                        var paymentMethod = await _dbContext.PaymentMethods
                            .FirstOrDefaultAsync(pm => pm.UserEntityID == userId.Value && pm.AuthorizationCode == result.data.authorization.authorization_code);

                        if (paymentMethod == null)
                        {
                            paymentMethod = new PaymentMethodEntity
                            {
                                UserEntityID = userId.Value,
                                AuthorizationCode = result.data.authorization.authorization_code,
                                CardType = result.data.authorization.card_type ?? "",
                                Last4 = result.data.authorization.last4 ?? "",
                                ExpMonth = result.data.authorization.exp_month ?? "",
                                ExpYear = result.data.authorization.exp_year ?? "",
                                Bank = result.data.authorization.bank ?? "",
                                Brand = result.data.authorization.brand ?? "",
                                Signature = result.data.authorization.signature ?? "",
                                Email = result.data.customer.email,
                                Reusable = true,
                                IsActive = true
                            };
                            _dbContext.PaymentMethods.Add(paymentMethod);
                        }
                    }

                    // Payment successful, update user's subscription
                    var existingSubscription = await _dbContext.BillingSubscriptions
                        .FirstOrDefaultAsync(b => b.PaystackCustomerCode == result.data.customer.customer_code);

                    string planId = "";
                    if (result.data.metadata.HasValue && result.data.metadata.Value.TryGetProperty("planId", out var planIdElement))
                    {
                        planId = planIdElement.GetString() ?? "";
                    }

                    if (existingSubscription != null)
                    {
                        existingSubscription.Status = "active";
                        if (!string.IsNullOrEmpty(planId))
                        {
                            existingSubscription.SubscriptionPlanId = planId;
                        }
                        existingSubscription.LastUpdatedAt = DateTime.UtcNow;
                        existingSubscription.CurrentPeriodEnd = DateTime.UtcNow.AddMonths(1); // update period
                        
                        if (transaction != null) transaction.BillingSubscriptionEntityID = existingSubscription.BillingSubscriptionEntityID;
                    }
                    else if (userId.HasValue)
                    {
                        // Create new subscription linked to user
                        var newSub = new BillingSubscriptionEntity
                        {
                            UserEntityID = userId.Value,
                            SubscriptionPlanId = planId,
                            Status = "active",
                            PaystackCustomerCode = result.data.customer.customer_code,
                            PaystackSubscriptionCode = result.data.authorization?.authorization_code ?? "",
                            CreatedAt = DateTime.UtcNow,
                            LastUpdatedAt = DateTime.UtcNow,
                            CurrentPeriodStart = DateTime.UtcNow,
                            CurrentPeriodEnd = DateTime.UtcNow.AddMonths(1)
                        };
                        _dbContext.BillingSubscriptions.Add(newSub);
                        
                        if (transaction != null) transaction.BillingSubscriptionEntityID = newSub.BillingSubscriptionEntityID;
                    }
                    
                    await _dbContext.SaveChangesAsync();

                    return new VerifyTransactionResponseDTO 
                    { 
                        Success = true, 
                        Message = "Transaction verified successfully",
                        Status = result.data.status 
                    };
                }

                return new VerifyTransactionResponseDTO 
                { 
                    Success = false, 
                    Message = $"Transaction status: {result.data.status}",
                    Status = result.data.status 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying transaction");
                return new VerifyTransactionResponseDTO { Success = false, Message = $"Error: {ex.Message}" };
            }
        }
    }
}