using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moneteer.Landing.Models;
using Newtonsoft.Json;
using Stripe;
using Stripe.Checkout;

namespace Moneteer.Landing.Managers
{
    public class StripeWebhookManager : IStripeWebhookManager
    {
        private readonly ISubscriptionManager _subscriptionManager;
        private readonly ILogger<StripeWebhookManager> _logger;

        public StripeWebhookManager(
            ISubscriptionManager subscriptionManager,
            ILogger<StripeWebhookManager> logger)
        {
            _subscriptionManager = subscriptionManager;
            _logger = logger;
        }

        public async Task HandleStripeWebhookEvent(Event stripeEvent)
        {
            switch (stripeEvent.Type)
            {
                case Events.InvoicePaymentSucceeded:
                    await HandleInvoicePaymentSucceededEvent(stripeEvent);
                    break;
                case Events.CheckoutSessionCompleted:
                    await HandleCheckoutSessionCompleted(stripeEvent);
                    break;
                default:
                    _logger.LogError($"Unexpected stripe event: {stripeEvent.Type}");
                    break;
            }
        }

        private async Task HandleInvoicePaymentSucceededEvent(Event stripeEvent)
        {
            if (stripeEvent == null) throw new ArgumentNullException(nameof(stripeEvent));

            var invoice = stripeEvent.Data.Object as Invoice;

            _logger.LogInformation($"Received stripe event {stripeEvent.Type} for invoice {invoice.Id}.");

            var periodEnd = invoice?.Lines?.Data?.FirstOrDefault()?.Period.End;

            if (periodEnd != null)
            {
                await _subscriptionManager.UpdateSubscriptionExpiry(invoice.CustomerId, periodEnd.Value);
            }
        }

        private async Task HandleCheckoutSessionCompleted(Event stripeEvent)
        {
            if (stripeEvent == null) throw new ArgumentNullException(nameof(stripeEvent));

            var session = stripeEvent.Data.Object as Session;

            _logger.LogInformation($"Received stripe event {stripeEvent.Type} for session {session.Id}.");
                        
            // We're updating the payment on an existing subscription
            if (session.SetupIntentId != null)
            {
                var setupIntentService = new SetupIntentService();

                var setupIntent = await setupIntentService.GetAsync(session.SetupIntentId);

                var paymentMethodId = setupIntent.PaymentMethodId;
                var customerId = setupIntent.Metadata["customer_id"];

                var paymentMethodsService = new PaymentMethodService();
                var attachOptions = new PaymentMethodAttachOptions();
                attachOptions.CustomerId = customerId;
                await paymentMethodsService.AttachAsync(paymentMethodId, attachOptions);

                var customerService = new CustomerService();
                var options = new CustomerUpdateOptions 
                {
                    InvoiceSettings = new CustomerInvoiceSettingsOptions 
                    {
                        DefaultPaymentMethodId = paymentMethodId,
                    },
                };
                customerService.Update(customerId, options);

                _logger.LogInformation($"Updated default payment method for customer {customerId}");
            }
        }
    }
}