using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Moneteer.Landing
{
    public static class Constants
    {
        /// <summary>
        /// The amount of time after a subscription has lapsed that access to the webapp is revoked
        /// </summary>
        public static TimeSpan SubscriptionBuffer = TimeSpan.FromDays(2);
    }
}
