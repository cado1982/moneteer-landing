using System;
using System.Data;
using System.Threading.Tasks;
using Moneteer.Landing.Models;

namespace Moneteer.Landing.Repositories
{
    public interface ISubscriptionRepository
    {
        Task<string> GetStripeId(Guid userId, IDbConnection connection);
        Task SetStripeId(Guid userId, string stripeId, IDbConnection connection);
        Task<string> GetUserIdFromStripeCustomerId(string stripeId, IDbConnection connection);
        Task UpdateSubscriptionExpiry(string customerId, DateTime? expiry, IDbConnection connection);
        Task UpdateSubscriptionStatus(string customerId, string status, IDbConnection connection);
        Task UpdateSubscription(string customerId, string subscriptionId, string status, IDbConnection connection);
        Task<SubscriptionInfo> GetSubscriptionInfo(Guid userId, IDbConnection connection);
    }
}

