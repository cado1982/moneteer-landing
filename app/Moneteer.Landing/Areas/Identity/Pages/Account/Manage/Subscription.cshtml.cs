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
using Moneteer.Landing.Managers;
using Moneteer.Landing.Models;

namespace Moneteer.Landing.V2.Areas.Identity.Pages.Account.Manage
{
    public class SubscriptionModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ISubscriptionManager _subscriptionManager;
        private readonly ILogger<SubscriptionModel> _logger;

        public SubscriptionModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ISubscriptionManager subscriptionManager,
            ILogger<SubscriptionModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _subscriptionManager = subscriptionManager;
            _logger = logger;
        }

        [TempData]
        public string StatusMessage { get; set; }

        public DateTime TrialExpiry { get; set; }

        public string Email { get; set; }

        public string UserId { get; set; }

        public Subscription Subscription { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            Email = await _userManager.GetEmailAsync(user);
            UserId = user.Id.ToString();

            var customerId = await _subscriptionManager.GetStripeCustomerId(user.Id);

            Subscription = await _subscriptionManager.GetSubscriptionByUser(customerId);

            TrialExpiry = user.TrialExpiry;

            return Page();
        }
    }
}
