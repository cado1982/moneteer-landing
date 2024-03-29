﻿using System;
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
            if (user == null) throw new ArgumentNullException(nameof(user));

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

                _logger.LogInformation($"Created stripe customer {customer.Id} for user {user.Id}");

                return customer;
            }
        }
                
        public async Task UpdateSubscriptionExpiry(string customerId, DateTime newExpiry)
        {
            if (String.IsNullOrWhiteSpace(customerId)) throw new ArgumentException("Customer id is empty");

            using (var conn = _connectionProvider.GetOpenConnection())
            {
                await _subscriptionRepository.UpdateSubscriptionExpiry(customerId, newExpiry.Add(Constants.SubscriptionBuffer), conn);

                _logger.LogInformation($"Updated subscription expiry for customer {customerId} to {newExpiry}");
            }
        }

        public async Task CancelSubscription(string subscriptionId)
        {
            if (String.IsNullOrWhiteSpace(subscriptionId)) throw new ArgumentException("subscriptionId must be provided");

            try
            {
                _logger.LogDebug($"Attempting subscription cancel for {subscriptionId}");

                var service = new SubscriptionService();

                await service.CancelAsync(subscriptionId, new SubscriptionCancelOptions
                {
                    InvoiceNow = false,
                    Prorate = false
                });

                _logger.LogInformation($"Cancelled stripe subscription {subscriptionId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unable to cancel subscription with id {subscriptionId}");
                throw;
            }
        }
        
        public async Task<StripeList<Invoice>> GetInvoices(string customerId, int count = 100)
        {
            try
            {
                _logger.LogDebug($"Attempting to retrieve invoices for stripe customer {customerId}");

                var service = new InvoiceService();

                var options = new InvoiceListOptions();
                options.CustomerId = customerId;
                options.Limit = count;
                
                var invoices = await service.ListAsync(options);

                _logger.LogDebug($"Found {invoices?.Count() ?? 0} invoices for stripe customer {customerId}");

                return invoices;
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
                _logger.LogDebug($"Attempting to retrieve session {sessionId}");

                var service = new SessionService();

                var options = new SessionGetOptions();
                options.AddExpand("subscription");
                var session = await service.GetAsync(sessionId, options);

                _logger.LogDebug($"Found session {sessionId}");

                return session;
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, $"Unable to get session with id {sessionId}");
                throw;
            }
            
        }

        public Task<Session> CreateUpdatePaymentMethodSessionAsync(User user)
        {
            var options = new SessionCreateOptions
            {
                SetupIntentData = new SessionSetupIntentDataOptions
                {
                    Metadata = new Dictionary<String, String>()
                    {
                        { "customer_id", user.StripeId }
                    }
                },
                ClientReferenceId = user.Id.ToString(),
                CustomerEmail = user.Email,
                PaymentMethodTypes = new List<string>
                {
                    "card"
                },
                Mode = "setup",
                SuccessUrl = _configurationHelper.Stripe.SubscriptionSuccessUrl,
                CancelUrl = _configurationHelper.Stripe.SubscriptionCancelledUrl,
            };

            return CreateStripeSession(options);
        }

        public Task<Session> CreatePurchaseSubscriptionSessionAsync(User user)
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
                    },
                },
                ClientReferenceId = user.Id.ToString(),
                CustomerId = user.StripeId,
                Mode = "subscription",
                SuccessUrl = _configurationHelper.Stripe.SubscriptionSuccessUrl,
                CancelUrl = _configurationHelper.Stripe.SubscriptionCancelledUrl
            };

            return CreateStripeSession(options);
        }

        private async Task<Session> CreateStripeSession(SessionCreateOptions options)
        {
            _logger.LogDebug($"Attempting to create stripe session for stripe customer {options.CustomerId ?? "null"} user {options.ClientReferenceId}");

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            _logger.LogInformation($"Generated stripe checkout session with id {session.Id} for stripe customer {options.CustomerId ?? "null"} user {options.ClientReferenceId}");

            return session;
        }

        public async Task<Subscription> GetActiveSubscription(string customerId)
        {
            if (String.IsNullOrWhiteSpace(customerId)) throw new ArgumentException("customerId must be provided");

            _logger.LogDebug($"Attempting to get active subscription for stripe customer {customerId}");

            var service = new SubscriptionService();

            var options = new SubscriptionListOptions();
            options.CustomerId = customerId;
            
            var subscriptions = await service.ListAsync(options);

            _logger.LogInformation($"Found {subscriptions.Count()} subscription(s) for stripe customer {customerId}");

            return subscriptions.FirstOrDefault();
        }
    }
}
