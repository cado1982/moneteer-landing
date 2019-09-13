using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Moneteer.Identity.Domain.Entities;
using Moneteer.Landing.Helpers;
using Moneteer.Landing.Managers;
using Moneteer.Landing.Models;
using Stripe;
using Stripe.Checkout;

namespace Moneteer.Landing.V2.Areas.Identity.Pages.Account.Manage
{
    public class SubscriptionModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly ISubscriptionManager _subscriptionManager;
        private readonly ILogger<SubscriptionModel> _logger;

        public SubscriptionModel(
            UserManager<User> userManager,
            ISubscriptionManager subscriptionManager,
            ILogger<SubscriptionModel> logger)
        {
            _userManager = userManager;
            _subscriptionManager = subscriptionManager;
            _logger = logger;
        }

        [TempData]
        public string StatusMessage { get; set; }

        public DateTimeOffset TrialExpiry { get; set; }
        public DateTimeOffset? SubscriptionExpiry { get; set; }

        public StripeList<Invoice> Invoices { get; set; }
        public Subscription ActiveSubscription { get; set; }

        public string StripeCustomerId { get; set; }


        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            StripeCustomerId = user.StripeId;

            TrialExpiry = new DateTimeOffset(user.TrialExpiry);
            SubscriptionExpiry = user.SubscriptionExpiry.HasValue ? new DateTimeOffset(user.SubscriptionExpiry.Value) : (DateTimeOffset?)null;

            if (user.StripeId != null)
            {
                ActiveSubscription = await _subscriptionManager.GetActiveSubscription(user.StripeId);
                Invoices = await _subscriptionManager.GetInvoices(user.StripeId);
            }

            if (ActiveSubscription != null)
            {
                // We might not need this session but we have to create it in case the user clicks on Update Payment Method button
                var updatePaymentMethodSession = await _subscriptionManager.CreateUpdatePaymentMethodSessionAsync(user);
                ViewData["StripeSessionId"] = updatePaymentMethodSession.Id;
            }

            return Page();
        }
    }
}
