using System.Data.Common;

namespace Database
{
    public class MySqlConnectionFactory : IConnectionFactory
    {
        private readonly string _connectionString;

        public MySqlConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DbConnection Create()
        {
            var factory = DbProviderFactories.GetFactory("MySql.Data.MySqlClient");
            var connection = factory.CreateConnection();
            connection.ConnectionString = _connectionString;
            return connection;
        }

        public string GetConnectionString()
        {
            return _connectionString;
        }
    }
}
