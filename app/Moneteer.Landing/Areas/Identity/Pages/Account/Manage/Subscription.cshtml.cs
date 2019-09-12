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
        private readonly SignInManager<User> _signInManager;
        private readonly ISubscriptionManager _subscriptionManager;
        private readonly IConfigurationHelper _configuration;
        private readonly ILogger<SubscriptionModel> _logger;

        public SubscriptionModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ISubscriptionManager subscriptionManager,
            IConfigurationHelper configuration,
            ILogger<SubscriptionModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _subscriptionManager = subscriptionManager;
            _configuration = configuration;
            _logger = logger;
        }

        [TempData]
        public string StatusMessage { get; set; }

        public DateTimeOffset TrialExpiry { get; set; }

        public string Email { get; set; }

        public string UserId { get; set; }
        
        public string SubscriptionId { get; set; }
        public DateTimeOffset? SubscriptionExpiry { get; set; }
        public string SubscriptionStatus { get; set; }
        public StripeList<Invoice> Invoices { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            Email = await _userManager.GetEmailAsync(user);
            UserId = user.Id.ToString();
            SubscriptionId = user.SubscriptionId;
            SubscriptionExpiry = user.SubscriptionExpiry == null ? (DateTimeOffset?)null : new DateTimeOffset(user.SubscriptionExpiry.Value.Add(-Constants.SubscriptionBuffer));
            TrialExpiry = new DateTimeOffset(user.TrialExpiry);
            SubscriptionStatus = user.SubscriptionStatus;

            if (user.StripeId != null)
            {
                Invoices = await _subscriptionManager.GetInvoices(user.StripeId, 10, null);
            }

            var updatePaymentMethodSession = await _subscriptionManager.CreateUpdatePaymentMethodSessionAsync(user);

            ViewData["StripeSessionId"] = updatePaymentMethodSession.Id;

            return Page();
        }
    }
}
