using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Moneteer.Landing.Models;
using Moneteer.Landing.Repositories;

namespace Moneteer.Landing.V2.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ResendSignUpLinkModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IBudgetRepository _budgetRepository;
        private readonly IConnectionProvider _connectionProvider;

        public ResendSignUpLinkModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            IBudgetRepository budgetRepository,
            IConnectionProvider connectionProvider)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _budgetRepository = budgetRepository;
            _connectionProvider = connectionProvider;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string StatusMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }
        }

        public void OnGet()
        {
            
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var email = Input.Email;

            if (!ModelState.IsValid)
            {
                return Page();
            }
    
            var user = await _userManager.FindByEmailAsync(Input.Email);
            var isEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);

            if (user != null && !isEmailConfirmed)
            {
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { userId = user.Id, code = code },
                    protocol: Request.Scheme);
                await _emailSender.SendEmailAsync(email,
                "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
            }

            StatusMessage = "Confirmation link resent.";

            return Page();
        }
    }
}
