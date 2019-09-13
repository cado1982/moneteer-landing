using Moneteer.Identity.Domain.Entities;
using Moneteer.Landing.Models;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Moneteer.Landing.Managers
{
    public interface ISubscriptionManager
    {
        Task<Customer> CreateStripeCustomer(User user);
        Task UpdateSubscriptionExpiry(string customerId, DateTime newExpiry);
        Task CancelSubscription(string subscriptionId);
        Task<StripeList<Invoice>> GetInvoices(string customerId, int count = 100);
        Task<Session> GetSession(string sessionId);
        Task<Session> CreateUpdatePaymentMethodSessionAsync(User user);
        Task<Session> CreatePurchaseSubscriptionSessionAsync(User user);
        Task<Subscription> GetActiveSubscription(string customerId);
    }
}
