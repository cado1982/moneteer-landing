
namespace Moneteer.Landing.Helpers
{
    public interface IConfigurationHelper
    {
        string IdentityUri { get; }
        string AppUri { get; }
        int TrialNumberOfDays { get; }
        StripeConfiguration Stripe { get; }
    }
}
