using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moneteer.Landing.Helpers;
using Moneteer.Landing.Managers;
using Stripe;

namespace Moneteer.Landing.V2.Controllers
{
    [AllowAnonymous]
    [Route("api/stripe")]
    public class StripeApiController : ControllerBase
    {
        private readonly IConfigurationHelper _configurationHepler;
        private readonly ILogger<StripeApiController> _logger;
        private readonly IStripeWebhookManager _stripeWebhookManager;

        public StripeApiController(
            IConfigurationHelper configurationHepler,
            ILogger<StripeApiController> logger,
            IStripeWebhookManager stripeWebhookManager)
        {
            _logger = logger;
            _stripeWebhookManager = stripeWebhookManager;
            _configurationHepler = configurationHepler;
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Index()
        {
            try
            {
                var stripeEvent = ConstructStripeEvent();

                await _stripeWebhookManager.HandleStripeWebhookEvent(stripeEvent);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error with stripe webhook api");
                return BadRequest();
            }
        }

        private Event ConstructStripeEvent()
        {
            var json = new StreamReader(HttpContext.Request.Body).ReadToEnd();
            var stripeSignature = Request.Headers["Stripe-Signature"];
            var webhookSigningKey = _configurationHepler.Stripe.WebhookSigningKey;

            var stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, webhookSigningKey);

            return stripeEvent;
        }
    }
}
