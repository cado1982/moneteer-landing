using System.Data;
using Npgsql;

namespace Moneteer.Landing
{
    public class ConnectionProvider: IConnectionProvider
    {
        private readonly DatabaseConnectionInfo _connectionInfo;

        public ConnectionProvider(DatabaseConnectionInfo connectionInfo)
        {
            _connectionInfo = connectionInfo;
        }

        public IDbConnection GetConnection()
        {
            return new NpgsqlConnection(_connectionInfo.ConnectionString);
        }

        public IDbConnection GetOpenConnection()
        {
            var connection = GetConnection();
            connection.Open();
            return connection;
        }
    }


    public interface IConnectionProvider
    {
        IDbConnection GetConnection();
        IDbConnection GetOpenConnection();
    }
}
