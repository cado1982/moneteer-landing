using System.Threading.Tasks;
using Stripe;

namespace Moneteer.Landing.Managers
{
    public interface IStripeWebhookManager
    {
         Task HandleStripeWebhookEvent(Event stripeEvent);
    }
}