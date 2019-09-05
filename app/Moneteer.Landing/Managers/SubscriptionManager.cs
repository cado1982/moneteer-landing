using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Moneteer.Identity.Domain.Entities;
using Moneteer.Landing.Helpers;
using Moneteer.Landing.Repositories;
using Stripe;
using Subscription = Moneteer.Landing.Models.Subscription;

namespace Moneteer.Landing.Managers
{
    public class SubscriptionManager : ISubscriptionManager
    {
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly IConnectionProvider _connectionProvider;
        private readonly IConfigurationHelper _configurationHelper;
        private readonly UserManager<User> _userManager;

        public SubscriptionManager(
            ISubscriptionRepository subscriptionRepository,
            IConnectionProvider connectionProvider,
            IConfigurationHelper configurationHelper,
            UserManager<User> userManager)
        {
            _subscriptionRepository = subscriptionRepository;
            _connectionProvider = connectionProvider;
            _configurationHelper = configurationHelper;
            _userManager = userManager;
        }

        public async Task<string> CreateStripeCustomer(User user)
        {
            using (var conn = _connectionProvider.GetOpenConnection())
            {
                var email = await _userManager.GetEmailAsync(user);

                var stripeCustomerService = new CustomerService();

                var options = new CustomerCreateOptions
                {
                    Metadata = new Dictionary<string, string>
                    {
                        { "MoneteerUserId", user.Id.ToString() }
                    },
                    Email = email
                };

                var customer = await stripeCustomerService.CreateAsync(options);

                await _subscriptionRepository.SetStripeId(user.Id, customer.Id, conn);

                return customer.Id;
            }
        }

        public async Task<string> GetStripeCustomerId(Guid moneteerUserId)
        {
            using (var conn = _connectionProvider.GetOpenConnection())
            {
                var stripeId = await _subscriptionRepository.GetStripeId(moneteerUserId, conn);

                return stripeId;
            }
        }

        public async Task<Moneteer.Landing.Models.Subscription> GetSubscriptionByUser(string customerId)
        {
            if (String.IsNullOrWhiteSpace(customerId)) return null;

            var service = new SubscriptionService();
            var subscription = (await service.ListAsync(new SubscriptionListOptions
            {
                CustomerId = customerId,
                Limit = 5
            })).FirstOrDefault();

            if (subscription == null) return null;

            var returnValue = new  Moneteer.Landing.Models.Subscription
            {
                Expiry = subscription.CurrentPeriodEnd,
                Id = subscription.Id,
                Status = subscription.Status
            };

            return returnValue;
        }

        public async Task UpdateSubscriptionExpiry(string customerId, DateTime newExpiry)
        {
            if (String.IsNullOrWhiteSpace(customerId)) throw new ArgumentException("Customer id is empty");

            using (var conn = _connectionProvider.GetOpenConnection())
            {
                await _subscriptionRepository.UpdateSubscriptionExpiry(customerId, newExpiry, conn);
            }
        }

        public async Task CancelSubscription(string subscriptionId)
        {
            var service = new SubscriptionService();

            await service.CancelAsync(subscriptionId, new SubscriptionCancelOptions
            {
                InvoiceNow = false,
                Prorate = false
            });
        }

        public async Task UpdateSubscriptionStatus(string customerId, string newStatus)
        {
            if (String.IsNullOrWhiteSpace(customerId)) throw new ArgumentException("Customer id is empty");

            using (var conn = _connectionProvider.GetOpenConnection())
            {
                await _subscriptionRepository.UpdateSubscriptionStatus(customerId, newStatus, conn);
            }
            
        }
    }
}
