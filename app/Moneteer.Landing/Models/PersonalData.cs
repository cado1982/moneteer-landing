using Moneteer.Models;
using System.Collections.Generic;

namespace Moneteer.Landing.Models
{
    public class PersonalData
    {
        public Dictionary<string, string> UserData { get; set; }
        public AppData AppData { get; set; }
    }

    public class AppData
    {
        public List<Transaction> Transactions { get; set; }
        public List<Account> Accounts { get; set; }
        public List<Envelope> Envelopes { get; set; }
        public List<EnvelopeCategory> EnvelopeCategories { get; set; }
    }
}
