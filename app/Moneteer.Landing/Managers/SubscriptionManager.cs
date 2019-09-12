using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Moneteer.Identity.Domain.Entities;
using Moneteer.Landing.Helpers;
using Moneteer.Landing.Repositories;
using Stripe;

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
        
        public async Task UpdateSubscriptionExpiry(string customerId, DateTime? newExpiry)
        {
            if (String.IsNullOrWhiteSpace(customerId)) throw new ArgumentException("Customer id is empty");

            using (var conn = _connectionProvider.GetOpenConnection())
            {
                await _subscriptionRepository.UpdateSubscriptionExpiry(customerId, newExpiry?.Add(Constants.SubscriptionBuffer), conn);
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

        public async Task UpdateSubscription(string customerId, string subscriptionId, string newStatus)
        {
            if (String.IsNullOrWhiteSpace(customerId)) throw new ArgumentException("Customer id is empty");

            using (var conn = _connectionProvider.GetOpenConnection())
            {
                await _subscriptionRepository.UpdateSubscription(customerId, subscriptionId, newStatus, conn);
            }
        }

        public async Task<Models.SubscriptionInfo> GetSubscriptionInfo(Guid userId)
        {
            if (userId == Guid.Empty) throw new ArgumentException("UserId must be provided", nameof(userId));

            using (var conn = _connectionProvider.GetOpenConnection())
            {
                var info = await _subscriptionRepository.GetSubscriptionInfo(userId, conn);

                return info;
            }
        }

        public async Task<StripeList<Invoice>> GetInvoices(string customerId, int count, string previousId = null)
        {
            var service = new InvoiceService();

            var options = new InvoiceListOptions();
            options.CustomerId = customerId;
            options.Limit = count;

            if (previousId != null)
            {
                options.StartingAfter = previousId;
            }
            
            return await service.ListAsync(options);
        }
    }
}
