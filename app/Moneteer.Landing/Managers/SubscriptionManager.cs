using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moneteer.Identity.Domain.Entities;
using Moneteer.Landing.Helpers;
using Moneteer.Landing.Repositories;
using Stripe;
using Stripe.Checkout;

namespace Moneteer.Landing.Managers
{
    public class SubscriptionManager : ISubscriptionManager
    {
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly IConnectionProvider _connectionProvider;
        private readonly IConfigurationHelper _configurationHelper;
        private readonly ILogger<SubscriptionManager> _logger;
        private readonly UserManager<User> _userManager;

        public SubscriptionManager(
            ISubscriptionRepository subscriptionRepository,
            IConnectionProvider connectionProvider,
            IConfigurationHelper configurationHelper,
            ILogger<SubscriptionManager> logger,
            UserManager<User> userManager)
        {
            _subscriptionRepository = subscriptionRepository;
            _connectionProvider = connectionProvider;
            _configurationHelper = configurationHelper;
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<Customer> CreateStripeCustomer(User user)
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

                return customer;
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
            try
            {
                var service = new SubscriptionService();

                await service.CancelAsync(subscriptionId, new SubscriptionCancelOptions
                {
                    InvoiceNow = false,
                    Prorate = false
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unable to cancel subscription with id {subscriptionId}");
                throw;
            }
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
        
        public async Task<StripeList<Invoice>> GetInvoices(string customerId, int count, string previousId = null)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unable to retrieve invoices for stripe customer id {customerId}");
                throw;
            }
        }

        public async Task<Session> GetSession(string sessionId)
        {
            try
            {
                var service = new SessionService();

                var options = new SessionGetOptions();
                options.AddExpand("subscription");
                var session = await service.GetAsync(sessionId, options);

                return session;
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, $"Unable to get session with id {sessionId}");
                throw;
            }
            
        }

        public async Task<Session> CreateUpdatePaymentMethodSessionAsync(User user)
        {
            var options = new SessionCreateOptions
            {
                SetupIntentData = new SessionSetupIntentDataOptions
                {
                    Metadata = new Dictionary<String, String>()
                    {
                        { "customer_id", user.StripeId },
                        { "subscription_id", user.SubscriptionId },
                    }
                },
                CustomerEmail = user.Email,
                PaymentMethodTypes = new List<string>
                {
                    "card"
                },
                Mode = "setup",
                SuccessUrl = _configurationHelper.Stripe.SubscriptionSuccessUrl,
                CancelUrl = _configurationHelper.Stripe.SubscriptionCancelledUrl,
            };

            var service = new SessionService();
            Session session = await service.CreateAsync(options);

            _logger.LogDebug($"Generated stripe checkout session with id {session.Id} for user {user.Id}");

            return session;
        }

        public async Task<Session> CreatePurchaseSubscriptionSessionAsync(User user)
        {
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string>
                {
                    "card",
                },
                SubscriptionData = new SessionSubscriptionDataOptions
                {
                    Items = new List<SessionSubscriptionDataItemOptions>
                    {
                        new SessionSubscriptionDataItemOptions
                        {
                            PlanId = _configurationHelper.Stripe.SubscriptionPlanId
                        },
                    }
                },
                ClientReferenceId = user.Id.ToString(),
                CustomerId = user.StripeId,
                Mode = "subscription",
                SuccessUrl = _configurationHelper.Stripe.SubscriptionSuccessUrl,
                CancelUrl = _configurationHelper.Stripe.SubscriptionCancelledUrl
            };

            var service = new SessionService();
            Session session = await service.CreateAsync(options);

            _logger.LogDebug($"Generated stripe checkout session with id {session.Id} for user {user.Id}");

            return session;
        }
    }
}
