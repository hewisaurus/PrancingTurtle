using System.Linq;
using Dapper;
using Database.Repositories.Interfaces;
using Logging;

namespace Database.Repositories
{
    public class ApiRepository : DapperRepositoryBase, IApiRepository
    {
        private readonly ILogger _logger;

        public ApiRepository(IConnectionFactory connectionFactory, ILogger logger) : base(connectionFactory)
        {
            _logger = logger;
        }

        public bool ValidateAuthKey(string authKey)
        {
            string timeElapsed;
            return Query(q => q.Query<long>(MySQL.Api.Validate, new { authKey }), out timeElapsed).SingleOrDefault() == 1;
        }
    }
}
