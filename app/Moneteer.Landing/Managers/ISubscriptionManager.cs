using Moneteer.Identity.Domain.Entities;
using Moneteer.Landing.Models;
using System;
using System.Threading.Tasks;

namespace Moneteer.Landing.Managers
{
    public interface ISubscriptionManager
    {
        Task<string> CreateStripeCustomer(User user);
        Task<string> GetStripeCustomerId(Guid moneteerUserId);
        Task UpdateSubscriptionExpiry(string customerId, DateTime? newExpiry);
        Task CancelSubscription(string subscriptionId);
        Task UpdateSubscriptionStatus(string customerId, string newStatus);
        Task UpdateSubscription(string customerId, string subscriptionId, string status);
        Task<SubscriptionInfo> GetSubscriptionInfo(Guid userId);
    }
}
