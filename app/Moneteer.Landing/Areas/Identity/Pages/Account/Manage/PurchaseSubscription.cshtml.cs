using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Moneteer.Identity.Domain.Entities;
using Moneteer.Landing.Helpers;
using Moneteer.Landing.Managers;
using Moneteer.Landing.Models;
using Newtonsoft.Json;
using Stripe;

namespace Moneteer.Landing.V2.Areas.Identity.Pages.Account.Manage
{
    public class PurchaseSubscriptionModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ISubscriptionManager _subscriptionManager;
        private readonly IConfigurationHelper _configurationHelper;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ILogger<PurchaseSubscriptionModel> _logger;

        public PurchaseSubscriptionModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ISubscriptionManager subscriptionManager,
            IConfigurationHelper configurationHelper,
            IHostingEnvironment hostingEnvironment,
            ILogger<PurchaseSubscriptionModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _subscriptionManager = subscriptionManager;
            _configurationHelper = configurationHelper;
            _hostingEnvironment = hostingEnvironment;
            _logger = logger;
        }

        [TempData]
        public string StatusMessage { get; set; }

        public DateTime TrialExpiry { get; set; }

        public string Email { get; set; }
        public string UserId { get; set; }

        public DateTime? SubscriptionExpiry { get; set; }

        public List<SelectListItem> Countries { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Address Line 1")]
            public string AddressLine1 { get; set; }

            [DataType(DataType.Text)]
            [Display(Name = "Address Line 2")]
            public string AddressLine2 { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "City")]
            public string City { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "State")]
            public string State { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Zip Code")]
            public string Zip { get; set; }

            [DataType(DataType.Text)]
            [Display(Name = "Country")]
            public string CountryCode { get; set; }

            [Required]
            public string StripeToken { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            Email = await _userManager.GetEmailAsync(user);

            var countriesJsonPath = _hostingEnvironment.ContentRootPath + "/countries.json";
            var json = System.IO.File.ReadAllText(countriesJsonPath);
            var bleh = JsonConvert.DeserializeObject<List<Country>>(json);

            Countries = new List<SelectListItem>();

            foreach (var country in bleh)
            {
                Countries.Add(new SelectListItem
                {
                    Text = country.Name,
                    Value = country.Code
                });
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

            try
            {
                var stripeCustomerId = await _subscriptionManager.GetStripeCustomerId(user.Id);

                if (stripeCustomerId == null)
                {
                    var email = await _userManager.GetEmailAsync(user);

                    var options = new CustomerCreateOptions
                    {
                        Email = email,
                        Source = Input.StripeToken,
                        Name = $"{Input.FirstName} {Input.LastName}",
                        Address = new AddressOptions
                        {
                            Line1 = Input.AddressLine1,
                            Line2 = Input.AddressLine2,
                            City = Input.City,
                            Country = Input.CountryCode,
                            PostalCode = Input.Zip,
                            State = Input.State
                        }
                    };

                    var customer = await _subscriptionManager.CreateStripeCustomer(user.Id, options);

                    stripeCustomerId = customer.Id;
                }

                await _subscriptionManager.CreateSubscription(stripeCustomerId);

                return Page();
            }
            catch (Exception ex)
            {
                return Page();
                throw;
            }
            
        }
    }
}
