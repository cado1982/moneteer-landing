using System;
using System.Data;
using System.Threading.Tasks;

namespace Moneteer.Landing.Repositories
{
    public interface ISubscriptionRepository
    {
        Task<string> GetStripeId(Guid userId, IDbConnection connection);
        Task SetStripeId(Guid userId, string stripeId, IDbConnection connection);
    }
}
