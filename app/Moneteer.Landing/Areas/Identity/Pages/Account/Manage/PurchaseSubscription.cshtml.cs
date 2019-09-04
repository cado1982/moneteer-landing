using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Moneteer.Identity.Domain.Entities;
using Stripe;

namespace Moneteer.Landing.V2.Areas.Identity.Pages.Account.Manage
{
    public class PurchaseSubscriptionModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<PurchaseSubscriptionModel> _logger;

        public PurchaseSubscriptionModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ILogger<PurchaseSubscriptionModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [TempData]
        public string StatusMessage { get; set; }

        public DateTime TrialExpiry { get; set; }

        public DateTime? SubscriptionExpiry { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var service = new PaymentIntentService();
            var options = new PaymentIntentCreateOptions
            {
                Amount = 299,
                Currency = "usd",
            };
            var intent = service.Create(options);
            ViewData["ClientSecret"] = intent.ClientSecret;

            TrialExpiry = user.TrialExpiry;
            SubscriptionExpiry = user.SubscriptionExpiry;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            return Page();
        }
    }
}
