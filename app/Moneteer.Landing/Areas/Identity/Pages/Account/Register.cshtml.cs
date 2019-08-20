using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Moneteer.Landing.Models;
using Moneteer.Landing.Repositories;
using Moneteer.Identity.Domain.Entities;

namespace Moneteer.Landing.V2.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IBudgetRepository _budgetRepository;
        private readonly IConnectionProvider _connectionProvider;

        public RegisterModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
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

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public void OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (ModelState.IsValid)
            {
                var user = new User { UserName = Input.Email, Email = Input.Email };
                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    await CreateBudgetForNewUser(user);
                    await SendEmailConfirmation(user);

                    return RedirectToPage("./RegisterCheckEmail");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private async Task CreateBudgetForNewUser(User user)
        {
            try
            {
                var budget = new Budget
                {
                    CurrencyCode = "USD",
                    CurrencySymbolLocation = SymbolLocation.Before,
                    DateFormat = "dd/MM/yyyy",
                    DecimalPlaces = 2,
                    DecimalSeparator = ".",
                    ThousandsSeparator = ",",
                    Name = "My Budget",
                    UserId = user.Id
                };

                using (var conn = _connectionProvider.GetOpenConnection())
                {
                    var budgetEntity = await _budgetRepository.Create(budget, conn);
                    await _budgetRepository.CreateDefaultEnvelopes(budgetEntity.Id, conn);
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Unable to create budget for user {user.Id}");
            }
        }

        private async Task SendEmailConfirmation(User user)
        {
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { userId = user.Id, code = code },
                protocol: Request.Scheme);

            await _emailSender.SendEmailAsync(Input.Email, "Moneteer - Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
        }
    }
}
