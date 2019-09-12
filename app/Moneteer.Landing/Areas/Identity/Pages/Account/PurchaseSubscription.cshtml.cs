using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Moneteer.Identity.Domain.Entities;
using Moneteer.Landing.Helpers;
using Moneteer.Landing.Managers;
using Stripe;
using Stripe.Checkout;

namespace Moneteer.Landing.V2.Areas.Identity.Pages.Account
{
    public class PurchaseSubscriptionModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfigurationHelper _configurationHelper;
        private readonly ILogger<PurchaseSubscriptionModel> _logger;
        private readonly ISubscriptionManager _subscriptionManager;

        public PurchaseSubscriptionModel
        (
            UserManager<User> userManager,
            IConfigurationHelper configurationHelper,
            ILogger<PurchaseSubscriptionModel> logger,
            ISubscriptionManager subscriptionManager
        )
        {
            _userManager = userManager;
            _configurationHelper = configurationHelper;
            _logger = logger;
            _subscriptionManager = subscriptionManager;
        }

        public DateTime TrialExpiry { get; set; }

        public string Email { get; set; }
        public string UserId { get; set; }

        public DateTime? SubscriptionExpiry { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            Email = await _userManager.GetEmailAsync(user);

            var customerId = await GetOrCreateCustomer(user);

            if (user.SubscriptionId != null && user.SubscriptionStatus != "canceled") 
            {
                return RedirectToPage("Manage/Subscription");
            }

            TrialExpiry = user.TrialExpiry;
            SubscriptionExpiry = user.SubscriptionExpiry;

            var session = GetStripeSession(user, customerId);
            ViewData["StripeSessionId"] = session.Id;

            return Page();
        }

        private Session GetStripeSession(User user, string customerId)
        {
            if (String.IsNullOrWhiteSpace(customerId)) throw new ArgumentException("Customer Id must be provided");
            
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
                CustomerId = customerId,
                Mode = "subscription",

                SuccessUrl = _configurationHelper.Stripe.SubscriptionSuccessUrl,
                CancelUrl = _configurationHelper.Stripe.SubscriptionCancelledUrl
            };

            var service = new SessionService();
            Session session = service.Create(options);

            _logger.LogDebug($"Generated stripe checkout session with id {session.Id} for user {user.Id}");

            return session;
        }

        private async Task<string> GetOrCreateCustomer(User user)
        {
            var customerId = await _subscriptionManager.GetStripeCustomerId(user.Id);

            if (customerId == null)
            {
                var newCustomerId = await _subscriptionManager.CreateStripeCustomer(user);

                customerId = newCustomerId;
            }

            return customerId;
        }
    }
}
