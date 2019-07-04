using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Moneteer.Landing.Models;
using Moneteer.Landing.Sql;
using Npgsql;

namespace Moneteer.Landing.Repositories
{
    public class BudgetRepository : IBudgetRepository
    {
        private readonly ILogger<BudgetRepository> _logger;

        public BudgetRepository(ILogger<BudgetRepository> logger)
        {
            _logger = logger;
        }

        public async Task Create(Budget budget, IDbConnection connection)
        {
            try
            {
                budget.Id = Guid.NewGuid();

                var parameters = new DynamicParameters();

                parameters.Add("@Id", budget.Id);
                parameters.Add("@Name", budget.Name);
                parameters.Add("@UserId", budget.UserId);
                parameters.Add("@CurrencyCode", budget.CurrencyCode);
                parameters.Add("@CurrencySymbolLocation", budget.CurrencySymbolLocation);
                parameters.Add("@ThousandsSeparator", budget.ThousandsSeparator);
                parameters.Add("@DecimalSeparator", budget.DecimalSeparator);
                parameters.Add("@DecimalPlaces", budget.DecimalPlaces);
                parameters.Add("@DateFormat", budget.DateFormat);

                await connection.ExecuteAsync(BudgetSql.Create, parameters).ConfigureAwait(false);
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == PostgresErrorCodes.UniqueViolation)
                {
                    throw new ApplicationException("Budget already exists");
                }
                else
                {
                    LogPostgresException(ex, "Error creating budget");
                    throw new ApplicationException("Oops! Something went wrong. Please try again");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating budget");
                throw new ApplicationException("Oops! Something went wrong. Please try again");
            }
        }

        private void LogPostgresException(PostgresException exception, string message)
        {
            var builder = new StringBuilder();

            builder.AppendLine(message);
            builder.AppendLine(exception.Message);
            builder.AppendLine(exception.StackTrace);
            builder.AppendLine(exception.Statement.SQL);

            foreach (var parameter in exception.Statement.InputParameters)
            {
                builder.AppendLine($"{parameter.ParameterName}: {parameter.Value}");
            }

            _logger.LogError(builder.ToString());
        }
    }
}
