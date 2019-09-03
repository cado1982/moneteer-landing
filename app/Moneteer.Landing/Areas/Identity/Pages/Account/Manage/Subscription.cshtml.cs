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

namespace Moneteer.Landing.V2.Areas.Identity.Pages.Account.Manage
{
    public class SubscriptionModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<SubscriptionModel> _logger;

        public SubscriptionModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ILogger<SubscriptionModel> logger)
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

            TrialExpiry = user.TrialExpiry;
            SubscriptionExpiry = user.SubscriptionExpiry;

            return Page();
        }
    }
}
