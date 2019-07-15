using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Moneteer.Landing.Models
{
    public class BudgetEnvelopes
    {
        public List<EnvelopeCategory> Categories { get; set; }
        public List<Envelope> Envelopes { get; set; }
    }
}
