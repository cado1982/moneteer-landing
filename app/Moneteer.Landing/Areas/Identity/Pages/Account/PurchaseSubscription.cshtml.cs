using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Moneteer.Identity.Domain.Entities;
using Moneteer.Landing.Helpers;
using Moneteer.Landing.Managers;

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

            if (user.StripeId == null)
            {
                var newCustomer = await _subscriptionManager.CreateStripeCustomer(user);
                user.StripeId = newCustomer.Id;
            }

            if (user.SubscriptionId != null && user.SubscriptionStatus != "canceled") 
            {
                return RedirectToPage("Manage/Subscription");
            }

            TrialExpiry = user.TrialExpiry;
            SubscriptionExpiry = user.SubscriptionExpiry;

            var session = await _subscriptionManager.CreatePurchaseSubscriptionSessionAsync(user);
            ViewData["StripeSessionId"] = session.Id;

            return Page();
        }
    }
}
