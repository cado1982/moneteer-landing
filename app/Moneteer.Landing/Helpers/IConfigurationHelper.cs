using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Moneteer.Landing.Helpers
{
    public interface IConfigurationHelper
    {
        string IdentityUri { get; }
        string AppUri { get; }
        int TrialNumberOfDays { get; }
    }
}
