using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Moneteer.Identity.Domain.Entities;
using Moneteer.Landing.Helpers;
using Moneteer.Landing.Models;
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

        public async Task<Customer> CreateStripeCustomer(Guid moneteerUserId, CustomerCreateOptions options)
        {
            using (var conn = _connectionProvider.GetOpenConnection())
            {
                var stripeCustomerService = new CustomerService();

                options.Metadata = new Dictionary<string, string>
                {
                    { "MoneteerUserId", moneteerUserId.ToString() }
                };

                var customer = await stripeCustomerService.CreateAsync(options);

                await _subscriptionRepository.SetStripeId(moneteerUserId, customer.Id, conn);

                return customer;
            }
        }

        public async Task CreateSubscription(string stripeCustomerId)
        {
            var items = new List<SubscriptionItemOption>
            {
                new SubscriptionItemOption
                {
                    PlanId = _configurationHelper.Stripe.SubscriptionPlanId
                }
            };

            var options = new SubscriptionCreateOptions
            {
                CustomerId = stripeCustomerId,
                Items = items
            };
            options.AddExpand("latest_invoice.payment_intent");

            var service = new SubscriptionService();
            Subscription subscription = await service.CreateAsync(options);
        }

        public async Task<string> GetStripeCustomerId(Guid moneteerUserId)
        {
            //var service = new CountrySpecService();
            //var options = new CountrySpecListOptions
            //{
            //    Limit = 100,
            //};
            //var countrySpecs = service.List(options);

            using (var conn = _connectionProvider.GetOpenConnection())
            {
                var stripeId = await _subscriptionRepository.GetStripeId(moneteerUserId, conn);

                return stripeId;
            }
        }

        public async Task<bool> IsStripeCustomer(Guid moneteerUserId)
        {
            var stripeCustomerId = await GetStripeCustomerId(moneteerUserId);

            return !String.IsNullOrWhiteSpace(stripeCustomerId);
        }
    }
}
