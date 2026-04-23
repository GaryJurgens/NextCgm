using Microsoft.EntityFrameworkCore;
using NextCgm.DataEntities.Billing;
using NextCgm.DContentext;
using NextCgm.Shared.ApiViewModels;
using NextCgm.Shared.DTOS;

namespace NextCgm.Services.Actions
{
    public interface IBillingSubscriptionService
    {
        Task<CreateSubscriptionResponseDTO> CreateSubscriptionAsync(CreateSubscriptionRequestDTO request);
        Task<GetSubscriptionResponseDTO> GetSubscriptionAsync(GetSubscriptionRequestDTO request);
        Task<GetUserSubscriptionsResponseDTO> GetUserSubscriptionsAsync(GetUserSubscriptionsRequestDTO request);
        Task<CancelSubscriptionResponseDTO> CancelSubscriptionAsync(CancelSubscriptionRequestDTO request);
    }

    public class BillingSubscriptionService : IBillingSubscriptionService
    {
        private readonly AppDBContext _context;

        public BillingSubscriptionService(AppDBContext context)
        {
            _context = context;
        }

        public async Task<CreateSubscriptionResponseDTO> CreateSubscriptionAsync(CreateSubscriptionRequestDTO request)
        {
            try
            {
                var userExists = await _context.UserEntities.AnyAsync(u => u.UserEntityID == request.UserEntityID);
                if (!userExists)
                {
                    return CreateSubscriptionResponseDTO.Failure("User not found.");
                }

                var entity = new BillingSubscriptionEntity
                {
                    UserEntityID = request.UserEntityID,
                    SubscriptionPlanId = request.SubscriptionPlanId,
                    Status = request.Status,
                    CurrentPeriodStart = request.CurrentPeriodStart,
                    CurrentPeriodEnd = request.CurrentPeriodEnd,
                    PaystackSubscriptionCode = request.PaystackSubscriptionCode,
                    PaystackCustomerCode = request.PaystackCustomerCode,
                    CreatedAt = DateTime.UtcNow,
                    LastUpdatedAt = DateTime.UtcNow
                };

                _context.BillingSubscriptions.Add(entity);
                await _context.SaveChangesAsync();

                var viewModel = new BillingSubscriptionApiViewModel
                {
                    BillingSubscriptionEntityID = entity.BillingSubscriptionEntityID,
                    UserEntityID = entity.UserEntityID,
                    SubscriptionPlanId = entity.SubscriptionPlanId,
                    Status = entity.Status,
                    CurrentPeriodStart = entity.CurrentPeriodStart,
                    CurrentPeriodEnd = entity.CurrentPeriodEnd,
                    PaystackSubscriptionCode = entity.PaystackSubscriptionCode,
                    PaystackCustomerCode = entity.PaystackCustomerCode,
                    CreatedAt = entity.CreatedAt,
                    LastUpdatedAt = entity.LastUpdatedAt
                };

                return new CreateSubscriptionResponseDTO
                {
                    Success = true,
                    Message = "Subscription created successfully",
                    Payload = viewModel
                };
            }
            catch (Exception ex)
            {
                return CreateSubscriptionResponseDTO.Failure($"Error creating subscription: {ex.Message}");
            }
        }

        public async Task<GetSubscriptionResponseDTO> GetSubscriptionAsync(GetSubscriptionRequestDTO request)
        {
            try
            {
                var entity = await _context.BillingSubscriptions
                    .FirstOrDefaultAsync(b => b.BillingSubscriptionEntityID == request.BillingSubscriptionEntityID);

                if (entity == null)
                {
                    return GetSubscriptionResponseDTO.Failure("Subscription not found.");
                }

                var viewModel = new BillingSubscriptionApiViewModel
                {
                    BillingSubscriptionEntityID = entity.BillingSubscriptionEntityID,
                    UserEntityID = entity.UserEntityID,
                    SubscriptionPlanId = entity.SubscriptionPlanId,
                    Status = entity.Status,
                    CurrentPeriodStart = entity.CurrentPeriodStart,
                    CurrentPeriodEnd = entity.CurrentPeriodEnd,
                    CanceledAt = entity.CanceledAt,
                    FailedChargeAttempts = entity.FailedChargeAttempts,
                    NextRetryDate = entity.NextRetryDate,
                    GracePeriodEndDate = entity.GracePeriodEndDate,
                    PaystackSubscriptionCode = entity.PaystackSubscriptionCode,
                    PaystackCustomerCode = entity.PaystackCustomerCode,
                    CreatedAt = entity.CreatedAt,
                    LastUpdatedAt = entity.LastUpdatedAt
                };

                return new GetSubscriptionResponseDTO
                {
                    Success = true,
                    Message = "Subscription retrieved successfully",
                    Payload = viewModel
                };
            }
            catch (Exception ex)
            {
                return GetSubscriptionResponseDTO.Failure($"Error retrieving subscription: {ex.Message}");
            }
        }

        public async Task<CancelSubscriptionResponseDTO> CancelSubscriptionAsync(CancelSubscriptionRequestDTO request)
        {
            try
            {
                var sub = await _context.BillingSubscriptions.FindAsync(request.SubscriptionId);
                if (sub == null) return new CancelSubscriptionResponseDTO { Success = false, Message = "Not found" };

                sub.Status = "canceled";
                sub.LastUpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return new CancelSubscriptionResponseDTO { Success = true, Message = "Canceled successfully" };
            }
            catch(Exception ex)
            {
                return new CancelSubscriptionResponseDTO { Success = false, Message = ex.Message };
            }
        }

        public async Task<GetUserSubscriptionsResponseDTO> GetUserSubscriptionsAsync(GetUserSubscriptionsRequestDTO request)
        {
            try
            {
                var entities = await _context.BillingSubscriptions
                    .Where(b => b.UserEntityID == request.UserEntityID)
                    .ToListAsync();

                var viewModels = entities.Select(entity => new BillingSubscriptionApiViewModel
                {
                    BillingSubscriptionEntityID = entity.BillingSubscriptionEntityID,
                    UserEntityID = entity.UserEntityID,
                    SubscriptionPlanId = entity.SubscriptionPlanId,
                    Status = entity.Status,
                    CurrentPeriodStart = entity.CurrentPeriodStart,
                    CurrentPeriodEnd = entity.CurrentPeriodEnd,
                    CanceledAt = entity.CanceledAt,
                    FailedChargeAttempts = entity.FailedChargeAttempts,
                    NextRetryDate = entity.NextRetryDate,
                    GracePeriodEndDate = entity.GracePeriodEndDate,
                    PaystackSubscriptionCode = entity.PaystackSubscriptionCode,
                    PaystackCustomerCode = entity.PaystackCustomerCode,
                    CreatedAt = entity.CreatedAt,
                    LastUpdatedAt = entity.LastUpdatedAt
                }).ToList();

                return new GetUserSubscriptionsResponseDTO
                {
                    Success = true,
                    Message = "Subscriptions retrieved successfully",
                    Payload = viewModels
                };
            }
            catch (Exception ex)
            {
                return GetUserSubscriptionsResponseDTO.Failure($"Error retrieving user subscriptions: {ex.Message}");
            }
        }
    }
}
