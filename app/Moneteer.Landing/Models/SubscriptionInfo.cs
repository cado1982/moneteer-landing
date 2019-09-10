using System;

namespace Moneteer.Landing.Models
{
    public class SubscriptionInfo
    {
        public DateTime? SubscriptionExpiry { get; set; }
        public DateTime TrialExpiry { get; set; }
        public string Id { get; set; }
        public string Status { get; set; }
        public string CustomerId { get; set; }

        public bool IsActive()
        {
            return IsTrialInDate() || IsSubscriptionInDate();
        }

        public bool IsSubscriptionInDate()
        {
            return SubscriptionExpiry != null && SubscriptionExpiry > DateTime.UtcNow;
        }

        public bool IsTrialInDate()
        {
            return TrialExpiry > DateTime.UtcNow;
        }

        public bool IsStatusGood()
        {
            return Status == "active" || Status == "past_due";
        }
    }
}