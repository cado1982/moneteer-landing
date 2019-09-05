using System;

namespace Moneteer.Landing.Models
{
    public class Subscription
    {
        public DateTime? Expiry { get; set; }
        public string Id { get; set; }
        public string Status { get; set; }
    }
}