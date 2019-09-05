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
        Task UpdateSubscriptionExpiry(string customerId, DateTime newExpiry);
        Task<Subscription> GetSubscriptionByUser(string customerId);
        Task CancelSubscription(string subscriptionId);
        Task UpdateSubscriptionStatus(string customerId, string newStatus);
    }
}
