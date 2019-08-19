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

        public async Task<Budget> Create(Budget budget, IDbConnection connection)
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

                return budget;
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
                    throw new ApplicationException("Oops! Something went wrong. Please try again", ex);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating budget");
                throw new ApplicationException("Oops! Something went wrong. Please try again", ex);
            }
        }


        public async Task<BudgetEnvelopes> CreateDefaultEnvelopes(Guid budgetId, IDbConnection conn)
        {
            try
            {
                var defaultEnvelopes = GenerateDefaultEnvelopes(budgetId);

                await conn.ExecuteAsync(EnvelopeSql.CreateEnvelopeCategory, defaultEnvelopes.Categories).ConfigureAwait(false);

                foreach (var envelope in defaultEnvelopes.Envelopes)
                {
                    await conn.ExecuteAsync(EnvelopeSql.CreateEnvelope, new
                    {
                        envelope.Id,
                        EnvelopeCategoryId = envelope.EnvelopeCategory.Id,
                        envelope.Name
                    }).ConfigureAwait(false);
                }

                return defaultEnvelopes;
            }
            catch (PostgresException ex)
            {
                LogPostgresException(ex, $"Error creating default envelopes for budget: {budgetId}");
                throw new ApplicationException("Oops! Something went wrong. Please try again", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating default envelopes for budget: {budgetId}");
                throw new ApplicationException("Oops! Something went wrong. Please try again", ex);
            }
        }

        private void LogPostgresException(PostgresException exception, string message)
        {
            var builder = new StringBuilder();

            builder.AppendLine(message);
            builder.AppendLine(exception.Message);
            builder.AppendLine(exception.StackTrace);

            if (exception.Statement != null)
            {
                builder.AppendLine(exception.Statement.SQL);

                if (exception.Statement.InputParameters != null)
                {
                    foreach (var parameter in exception.Statement.InputParameters)
                    {
                        builder.AppendLine($"{parameter.ParameterName}: {parameter.Value}");

                    }
                }
            }

            _logger.LogError(builder.ToString());
        }

        private BudgetEnvelopes GenerateDefaultEnvelopes(Guid budgetId)
        {
            var result = new BudgetEnvelopes
            {
                Envelopes = new List<Envelope>(),
                Categories = new List<EnvelopeCategory>()
            };

            var monthlyBills = new EnvelopeCategory { Id = Guid.NewGuid(), BudgetId = budgetId, Name = "Monthly Bills" };
            var everydayExpenses = new EnvelopeCategory { Id = Guid.NewGuid(), BudgetId = budgetId, Name = "Everyday Expenses" };
            var rainyDayFunds = new EnvelopeCategory { Id = Guid.NewGuid(), BudgetId = budgetId, Name = "Rainy Day Funds" };
            var savingsGoals = new EnvelopeCategory { Id = Guid.NewGuid(), BudgetId = budgetId, Name = "Savings Goals" };
            var debt = new EnvelopeCategory { Id = Guid.NewGuid(), BudgetId = budgetId, Name = "Debt" };
            var giving = new EnvelopeCategory { Id = Guid.NewGuid(), BudgetId = budgetId, Name = "Giving" };
            var income = new EnvelopeCategory { Id = Guid.NewGuid(), BudgetId = budgetId, Name = "Income" };

            result.Categories.Add(monthlyBills);
            result.Categories.Add(everydayExpenses);
            result.Categories.Add(rainyDayFunds);
            result.Categories.Add(savingsGoals);
            result.Categories.Add(debt);
            result.Categories.Add(giving);
            result.Categories.Add(income);

            result.Envelopes.Add(new Envelope { Id = Guid.NewGuid(), EnvelopeCategory = monthlyBills, Name = "Rent/Mortgage" });
            result.Envelopes.Add(new Envelope { Id = Guid.NewGuid(), EnvelopeCategory = monthlyBills, Name = "Phone" });
            result.Envelopes.Add(new Envelope { Id = Guid.NewGuid(), EnvelopeCategory = monthlyBills, Name = "Internet" });
            result.Envelopes.Add(new Envelope { Id = Guid.NewGuid(), EnvelopeCategory = monthlyBills, Name = "Cable TV" });
            result.Envelopes.Add(new Envelope { Id = Guid.NewGuid(), EnvelopeCategory = monthlyBills, Name = "Electricity" });
            result.Envelopes.Add(new Envelope { Id = Guid.NewGuid(), EnvelopeCategory = monthlyBills, Name = "Water" });

            result.Envelopes.Add(new Envelope { Id = Guid.NewGuid(), EnvelopeCategory = everydayExpenses, Name = "Spending Money" });
            result.Envelopes.Add(new Envelope { Id = Guid.NewGuid(), EnvelopeCategory = everydayExpenses, Name = "Groceries" });
            result.Envelopes.Add(new Envelope { Id = Guid.NewGuid(), EnvelopeCategory = everydayExpenses, Name = "Fuel" });
            result.Envelopes.Add(new Envelope { Id = Guid.NewGuid(), EnvelopeCategory = everydayExpenses, Name = "Restaurants" });
            result.Envelopes.Add(new Envelope { Id = Guid.NewGuid(), EnvelopeCategory = everydayExpenses, Name = "Medical/Dental" });
            result.Envelopes.Add(new Envelope { Id = Guid.NewGuid(), EnvelopeCategory = everydayExpenses, Name = "Clothing" });
            result.Envelopes.Add(new Envelope { Id = Guid.NewGuid(), EnvelopeCategory = everydayExpenses, Name = "Household Goods" });

            result.Envelopes.Add(new Envelope { Id = Guid.NewGuid(), EnvelopeCategory = rainyDayFunds, Name = "Emergency Fund" });
            result.Envelopes.Add(new Envelope { Id = Guid.NewGuid(), EnvelopeCategory = rainyDayFunds, Name = "Car Maintenance" });
            result.Envelopes.Add(new Envelope { Id = Guid.NewGuid(), EnvelopeCategory = rainyDayFunds, Name = "Car Insurance" });
            result.Envelopes.Add(new Envelope { Id = Guid.NewGuid(), EnvelopeCategory = rainyDayFunds, Name = "Birthdays" });
            result.Envelopes.Add(new Envelope { Id = Guid.NewGuid(), EnvelopeCategory = rainyDayFunds, Name = "Christmas" });
            result.Envelopes.Add(new Envelope { Id = Guid.NewGuid(), EnvelopeCategory = rainyDayFunds, Name = "Renters Insurance" });
            result.Envelopes.Add(new Envelope { Id = Guid.NewGuid(), EnvelopeCategory = rainyDayFunds, Name = "Retirement" });

            result.Envelopes.Add(new Envelope { Id = Guid.NewGuid(), EnvelopeCategory = savingsGoals, Name = "Car Replacement" });
            result.Envelopes.Add(new Envelope { Id = Guid.NewGuid(), EnvelopeCategory = savingsGoals, Name = "Vacation" });

            result.Envelopes.Add(new Envelope { Id = Guid.NewGuid(), EnvelopeCategory = debt, Name = "Car Payment" });
            result.Envelopes.Add(new Envelope { Id = Guid.NewGuid(), EnvelopeCategory = debt, Name = "Student Loan Payment" });
            result.Envelopes.Add(new Envelope { Id = Guid.NewGuid(), EnvelopeCategory = debt, Name = "Pre Moneteer Debt" });

            result.Envelopes.Add(new Envelope { Id = Guid.NewGuid(), EnvelopeCategory = giving, Name = "Tithing" });
            result.Envelopes.Add(new Envelope { Id = Guid.NewGuid(), EnvelopeCategory = giving, Name = "Charitable" });

            result.Envelopes.Add(new Envelope { Id = Guid.NewGuid(), EnvelopeCategory = income, Name = "Available Income" });

            return result;
        }

    }
}
