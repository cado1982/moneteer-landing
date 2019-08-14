using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Moneteer.Landing.Models
{
    public class Envelope
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public EnvelopeCategory EnvelopeCategory { get; set; }
        public bool IsHidden { get; set; }
        public bool IsDeleted { get; set; }
        public decimal Assigned { get; set; }
    }
}
