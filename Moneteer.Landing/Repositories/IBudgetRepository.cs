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
        Task Create(Budget budget, IDbConnection connection);
    }
}
