namespace Moneteer.Landing.Models
{
    public static class SubscriptionStatus
    {
        public static string Incomplete = "incomplete";
        public static string IncompleteExpired = "incomplete_expired";
        public static string Trialing = "trialing";
        public static string Active = "active";
        public static string PastDue = "past_due";
        public static string Canceled = "canceled";
        public static string Unpaid = "unpaid";
    }
}