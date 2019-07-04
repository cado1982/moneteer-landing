using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Moneteer.Landing.Models
{
    public class EnvelopeCategory
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid BudgetId { get; set; }
        public bool IsHidden { get; set; }
        public bool IsDeleted { get; set; }
    }
}
