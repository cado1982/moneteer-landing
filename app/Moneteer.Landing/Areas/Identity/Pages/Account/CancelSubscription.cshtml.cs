﻿using System;
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
    public class CancelSubscriptionModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfigurationHelper _configurationHelper;
        private readonly ILogger<PurchaseSubscriptionModel> _logger;
        private readonly ISubscriptionManager _subscriptionManager;

        [BindProperty]
        public string SubscriptionId { get; set; }

        public CancelSubscriptionModel
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

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (user.StripeId != null)
            {
                var activeSubscription = await _subscriptionManager.GetActiveSubscription(user.StripeId);

                if (activeSubscription != null)
                {
                    SubscriptionId = activeSubscription.Id;
                    return Page();
                }
            }

            return RedirectToPage("Manage/Subscription");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _subscriptionManager.CancelSubscription(SubscriptionId);

            return RedirectToPage("CancelSubscriptionSuccess");
        }
    }
}
