using Stripe;
using System;
using System.Threading.Tasks;

namespace Moneteer.Landing.Managers
{
    public interface ISubscriptionManager
    {
        Task<Customer> CreateStripeCustomer(Guid moneteerUserId, CustomerCreateOptions options);
        Task<string> GetStripeCustomerId(Guid moneteerUserId);
        Task<bool> IsStripeCustomer(Guid moneteerUserId);
        Task CreateSubscription(string stripeCustomerId);
    }
}
