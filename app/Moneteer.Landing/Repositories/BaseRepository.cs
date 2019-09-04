using Microsoft.Extensions.Logging;
using Npgsql;
using System.Text;

namespace Moneteer.Landing.Repositories
{
    public abstract class BaseRepository<T>
    {
        protected readonly ILogger<T> Logger;

        protected BaseRepository(ILogger<T> logger)
        {
            Logger = logger;
        }

        protected void LogPostgresException(PostgresException exception, string message)
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

            Logger.LogError(builder.ToString());
        }
    }
}
