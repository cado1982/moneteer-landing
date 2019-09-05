using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moneteer.Landing.Helpers;
using Moneteer.Landing.Managers;
using Newtonsoft.Json;
using Stripe;

namespace Moneteer.Landing.V2.Controllers
{
    [AllowAnonymous]
    [Route("api/stripe")]
    public class StripeApiController : Controller
    {
        private readonly IConfigurationHelper _configurationHepler;
        private readonly ILogger<StripeApiController> _logger;
        private readonly ISubscriptionManager _subscriptionManager;

        public StripeApiController(
            IConfigurationHelper configurationHepler,
            ILogger<StripeApiController> logger,
            ISubscriptionManager subscriptionManager)
        {
            _logger = logger;
            _subscriptionManager = subscriptionManager;
            _configurationHepler = configurationHepler;
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Index()
        {
            var json = new StreamReader(HttpContext.Request.Body).ReadToEnd();

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json,
                    Request.Headers["Stripe-Signature"], _configurationHepler.Stripe.WebhookSigningKey);

                _logger.LogDebug($"Received stripe webhook: {JsonConvert.SerializeObject(stripeEvent)}");

                if (stripeEvent.Type == Events.InvoicePaymentSucceeded)
                {
                    var invoice = stripeEvent.Data.Object as Invoice;

                    await HandleInvoicePaymentSucceeded(invoice);
                }
                else if (stripeEvent.Type == Events.CustomerSubscriptionCreated)
                {
                    var subscription = stripeEvent.Data.Object as Subscription;

                    if (stripeEvent.Data.PreviousAttributes.status != null)
                    {
                        var newStatus = subscription.Status;
                        await _subscriptionManager.UpdateSubscriptionStatus(subscription.CustomerId, newStatus);
                    }
                }
                else if (stripeEvent.Type == Events.CustomerSubscriptionUpdated)
                {
                    var subscription = stripeEvent.Data.Object as Subscription;

                    if (stripeEvent.Data.PreviousAttributes.status != null)
                    {
                        var newStatus = subscription.Status;
                        await _subscriptionManager.UpdateSubscriptionStatus(subscription.CustomerId, newStatus);
                    }
                }
                else if (stripeEvent.Type == Events.CustomerSubscriptionDeleted)
                {
                    var subscription = stripeEvent.Data.Object as Subscription;

                    if (stripeEvent.Data.PreviousAttributes.status != null)
                    {
                        var newStatus = subscription.Status;
                        await _subscriptionManager.UpdateSubscriptionStatus(subscription.CustomerId, newStatus);
                    }
                }
                else
                {
                    _logger.LogError($"Unexpected stripe event: {stripeEvent.Type}");
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error with stripe webhook api");
                return BadRequest();
            }
        }

        private async Task HandleInvoicePaymentSucceeded(Invoice invoice)
        {
            _logger.LogDebug($"Received invoice.payment.succeeded webhook. customer: {invoice.CustomerId}");

            await _subscriptionManager.UpdateSubscriptionExpiry(invoice.CustomerId, invoice.PeriodEnd);
        }
    }
}
