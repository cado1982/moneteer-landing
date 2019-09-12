using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
                case Events.CustomerSubscriptionCreated:
                    await HandleCustomerSubscriptionCreatedEvent(stripeEvent);
                    break;
                case Events.CustomerSubscriptionDeleted:
                    await HandleCustomerSubscriptionDeletedEvent(stripeEvent);
                    break;
                case Events.CustomerSubscriptionUpdated:
                    await HandleCustomerSubscriptionUpdatedEvent(stripeEvent);
                    break;
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

        private async Task HandleCustomerSubscriptionCreatedEvent(Event stripeEvent)
        {
            if (stripeEvent == null) throw new ArgumentNullException(nameof(stripeEvent));

            var subscription = stripeEvent.Data.Object as Subscription;

            _logger.LogInformation($"Received stripe event {stripeEvent.Type} for subscription {subscription.Id}. Status: {subscription.Status}. PeriodEnd: {subscription.CurrentPeriodEnd}");

            await _subscriptionManager.UpdateSubscription(subscription.CustomerId, subscription.Id, subscription.Status);
        }


        private async Task HandleCustomerSubscriptionDeletedEvent(Event stripeEvent)
        {
            if (stripeEvent == null) throw new ArgumentNullException(nameof(stripeEvent));

            var subscription = stripeEvent.Data.Object as Subscription;

            _logger.LogInformation($"Received stripe event {stripeEvent.Type} for subscription {subscription.Id}.");

            await _subscriptionManager.UpdateSubscriptionStatus(subscription.CustomerId, subscription.Status);
        }

        private async Task HandleCustomerSubscriptionUpdatedEvent(Event stripeEvent)
        {
            if (stripeEvent == null) throw new ArgumentNullException(nameof(stripeEvent));

            var subscription = stripeEvent.Data.Object as Subscription;

            _logger.LogInformation($"Received stripe event {stripeEvent.Type} for subscription {subscription.Id}. Status: {subscription.Status}. PeriodEnd: {subscription.CurrentPeriodEnd}");

            if (stripeEvent.Data?.PreviousAttributes == null) {
                return;
            }
            
            if (stripeEvent.Data.PreviousAttributes.status != null)
            {
                var newStatus = subscription.Status;
                await _subscriptionManager.UpdateSubscriptionStatus(subscription.CustomerId, newStatus);
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
                await _subscriptionManager.UpdateSubscriptionExpiry(invoice.CustomerId, periodEnd);
            }
        }

        private async Task HandleCheckoutSessionCompleted(Event stripeEvent)
        {
            if (stripeEvent == null) throw new ArgumentNullException(nameof(stripeEvent));

            var session = stripeEvent.Data.Object as Session;

            _logger.LogInformation($"Received stripe event {stripeEvent.Type} for session {session.Id}.");
                        
            if (session.SetupIntentId != null)
            // We're updating the payment on an existing subscription
            {
                var setupIntentService = new SetupIntentService();

                var setupIntent = await setupIntentService.GetAsync(session.SetupIntentId);

                var paymentMethodId = setupIntent.PaymentMethodId;
                var customerId = setupIntent.Metadata["customer_id"];
                var subscriptionId = setupIntent.Metadata["subscription_id"];

                var paymentMethodsService = new PaymentMethodService();
                var attachOptions = new PaymentMethodAttachOptions();
                attachOptions.CustomerId = customerId;
                await paymentMethodsService.AttachAsync(paymentMethodId, attachOptions);

                var subscriptionService = new SubscriptionService();
                var updateOptions = new SubscriptionUpdateOptions();
                updateOptions.DefaultPaymentMethodId = paymentMethodId;
                await subscriptionService.UpdateAsync(subscriptionId, updateOptions);
            }
        }
    }
}