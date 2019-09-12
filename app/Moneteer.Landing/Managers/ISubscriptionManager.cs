using Moneteer.Identity.Domain.Entities;
using Moneteer.Landing.Models;
using Stripe;
using Stripe.Checkout;
using System;
using System.Threading.Tasks;

namespace Moneteer.Landing.Managers
{
    public interface ISubscriptionManager
    {
        Task<Customer> CreateStripeCustomer(User user);
        Task UpdateSubscriptionExpiry(string customerId, DateTime? newExpiry);
        Task CancelSubscription(string subscriptionId);
        Task UpdateSubscriptionStatus(string customerId, string newStatus);
        Task UpdateSubscription(string customerId, string subscriptionId, string status);
        Task<StripeList<Invoice>> GetInvoices(string customerId, int count, string previousId = null);
        Task<Session> GetSession(string sessionId);
        Task<Session> CreateUpdatePaymentMethodSessionAsync(User user);
        Task<Session> CreatePurchaseSubscriptionSessionAsync(User user);
    }
}
