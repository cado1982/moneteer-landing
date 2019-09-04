using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Moneteer.Landing.Sql;
using Npgsql;

namespace Moneteer.Landing.Repositories
{
    public class SubscriptionRepository : BaseRepository<SubscriptionRepository>, ISubscriptionRepository
    {
        public SubscriptionRepository(ILogger<SubscriptionRepository> logger) : base(logger)
        {
        }

        public async Task<string> GetStripeId(Guid userId, IDbConnection connection)
        {
            if (userId == Guid.Empty) throw new ArgumentException("userId must be provided");

            try
            {
                var parameters = new DynamicParameters();

                parameters.Add("@UserId", userId);

                var stripeId = await connection.ExecuteScalarAsync<string>(SubscriptionSql.GetStripeId, parameters).ConfigureAwait(false);

                return stripeId;
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == PostgresErrorCodes.UniqueViolation)
                {
                    throw new ApplicationException("Budget already exists");
                }
                else
                {
                    LogPostgresException(ex, $"Error getting stripe id for user {userId}");
                    throw;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error creating budget");
                throw;
            }
        }

        public async Task SetStripeId(Guid userId, string stripeId, IDbConnection connection)
        {
            if (userId == Guid.Empty) throw new ArgumentException("userId must be provided");
            if (String.IsNullOrWhiteSpace(stripeId)) throw new ArgumentException("stripeId must be provided");

            try
            {
                var parameters = new DynamicParameters();

                parameters.Add("@UserId", userId);
                parameters.Add("@StripeId", stripeId);

                await connection.ExecuteAsync(SubscriptionSql.SetStripeId, parameters).ConfigureAwait(false);
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == PostgresErrorCodes.UniqueViolation)
                {
                    throw new ApplicationException("Budget already exists");
                }
                else
                {
                    LogPostgresException(ex, $"Error getting stripe id for user {userId}");
                    throw;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error creating budget");
                throw;
            }
        }
    }
}

