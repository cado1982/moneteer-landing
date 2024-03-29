﻿using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Moneteer.Landing.Models;
using Moneteer.Landing.Sql;
using Npgsql;

namespace Moneteer.Landing.Repositories
{
    public class SubscriptionRepository : BaseRepository<SubscriptionRepository>, ISubscriptionRepository
    {
        public SubscriptionRepository(ILogger<SubscriptionRepository> logger) : base(logger)
        {
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
                LogPostgresException(ex, $"Error setting stripe id for user {userId}");
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error setting stripe id for user {userId}");
                throw;
            }
        }

        public async Task UpdateSubscriptionExpiry(string customerId, DateTime? expiry, IDbConnection connection)
        {
            if (String.IsNullOrWhiteSpace(customerId)) throw new ArgumentException("customerId must be provided");

            try
            {
                var parameters = new DynamicParameters();

                parameters.Add("@StripeId", customerId);
                parameters.Add("@Expiry", expiry);

                await connection.ExecuteAsync(SubscriptionSql.UpdateExpiry, parameters).ConfigureAwait(false);
            }
            catch (PostgresException ex)
            {
                LogPostgresException(ex, $"Error updating subscription expiry for customer {customerId}");
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error updating subscription expiry for customer {customerId}");
                throw;
            }
        }

        public async Task UpdateSubscriptionStatus(string customerId, string status, IDbConnection connection)
        {
            if (String.IsNullOrWhiteSpace(customerId)) throw new ArgumentException("customerId must be provided");

            try
            {
                var parameters = new DynamicParameters();

                parameters.Add("@StripeId", customerId);
                parameters.Add("@Status", status);

                await connection.ExecuteAsync(SubscriptionSql.UpdateStatus, parameters).ConfigureAwait(false);
            }
            catch (PostgresException ex)
            {
                LogPostgresException(ex, $"Error updating subscription status for customer {customerId}");
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error updating subscription status for customer {customerId}");
                throw;
            }
        }

        public async Task UpdateSubscription(string customerId, string subscriptionId, string status, IDbConnection connection)
        {
            if (String.IsNullOrWhiteSpace(customerId)) throw new ArgumentException("customerId must be provided");

            try
            {
                var parameters = new DynamicParameters();

                parameters.Add("@StripeId", customerId);
                parameters.Add("@Status", status);
                parameters.Add("@SubscriptionId", subscriptionId);

                await connection.ExecuteAsync(SubscriptionSql.Update, parameters).ConfigureAwait(false);
            }
            catch (PostgresException ex)
            {
                LogPostgresException(ex, $"Error updating subscription for customer {customerId}");
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error updating subscription for customer {customerId}");
                throw;
            }
        }
    }
}

