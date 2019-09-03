using System;
using Microsoft.Extensions.Configuration;

namespace Moneteer.Landing.Helpers
{
    public class ConfigurationHelper : IConfigurationHelper
    {
        private readonly IConfiguration _configuration;

        public ConfigurationHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string IdentityUri
        {
            get { return _configuration["IdentityUri"]; }
        }

        public string AppUri
        {
            get { return _configuration["AppUri"]; }
        }

        public int TrialNumberOfDays
        {
            get { return Int32.Parse(_configuration["TrialNumberOfDays"]); }
        }

        public StripeConfiguration Stripe
        {
            get
            {
                return new StripeConfiguration
                {
                    SubscriptionSuccessUrl = _configuration["Stripe:SubscriptionSuccessUrl"],
                    SubscriptionCancelledUrl = _configuration["Stripe:SubscriptionCancelledUrl"],
                    SubscriptionPlanId = _configuration["Stripe:SubscriptionPlanId"],
                    PublicKey = _configuration["Stripe:PublicKey"]
                };
            }
        }
    }

    public class StripeConfiguration
    {
        public string SubscriptionSuccessUrl { get; set; }
        public string SubscriptionCancelledUrl { get; set; }
        public string SubscriptionPlanId { get; set; }
        public string PublicKey { get; set; }
    }
}