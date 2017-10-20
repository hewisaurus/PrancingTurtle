using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Database
{
    public abstract class DapperRepositoryBase
    {
        protected readonly IConnectionFactory _connectionFactory;
        protected readonly string _connectionString;

        protected DapperRepositoryBase(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
            _connectionString = connectionFactory.GetConnectionString();
        }

        protected DbConnection OpenConnection()
        {
            var connection = _connectionFactory.Create();
            connection.Open();
            return connection;
        }

        protected async Task<DbConnection> OpenConnectionAsync()
        {
            var connection = _connectionFactory.Create();
            await connection.OpenAsync();
            return connection;
        }

        protected DbConnection GetConnectionUnopened()
        {
            return _connectionFactory.Create();
        }

        protected T Query<T>(Func<DbConnection, T> func, out string elapsedTime)
        {
            if (func == null)
            {
                throw new ArgumentNullException("func");
            }

            var sw = new Stopwatch();
            sw.Start();

            using (var connection = OpenConnection())
            {
                var result = func(connection);
                sw.Stop();
                elapsedTime = sw.Elapsed.ToString();
                return result;
            }
        }

        protected async Task<T> QueryAsync<T>(Func<IDbConnection, Task<T>> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException("func");
            }
            try
            {
                using (var connection = await OpenConnectionAsync())
                {
                    return await func(connection);
                }
            }
            catch (TimeoutException ex)
            {
                throw new Exception(string.Format("{0}.ExecuteAsync() timed out", GetType().FullName));
            }
            catch (Exception ex)
            {
                throw new Exception("Exception in ExecuteAsync()", ex);
            }
        }

        protected int Execute(Func<DbConnection, int> func, out string elapsedTime)
        {
            if (func == null)
            {
                throw new ArgumentNullException("func");
            }

            var sw = new Stopwatch();
            sw.Start();

            using (var connection = OpenConnection())
            {
                var result = func(connection);
                sw.Stop();
                elapsedTime = sw.Elapsed.ToString();
                return result;
            }
        }

        protected async Task<int> ExecuteAsync(Func<IDbConnection, Task<int>> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException("func");
            }
            try
            {
                using (var connection = await OpenConnectionAsync())
                {
                    return await func(connection);
                }
            }
            catch (TimeoutException ex)
            {
                throw new Exception(string.Format("{0}.ExecuteAsync() timed out", GetType().FullName));
            }
            catch (Exception ex)
            {
                throw new Exception("Exception in ExecuteAsync()", ex);
            }
        }
    }
}
