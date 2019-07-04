using Moneteer.Landing.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Moneteer.Landing.Repositories
{
    public interface IBudgetRepository
    {
        Task<Budget> Create(Budget budget, IDbConnection connection);
        Task<BudgetEnvelopes> CreateDefaultEnvelopes(Guid budgetId, IDbConnection conn);
    }
}
