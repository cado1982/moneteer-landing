using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Moneteer.Landing.Models
{
    public class Budget
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string CurrencyCode { get; set; }
        public string ThousandsSeparator { get; set; }
        public string DecimalSeparator { get; set; }
        public short DecimalPlaces { get; set; }
        public SymbolLocation CurrencySymbolLocation { get; set; }
        public string DateFormat { get; set; }
        public Guid UserId { get; set; }
    }

    public enum SymbolLocation
    {
        Before,
        After,
        Hidden
    }
}
