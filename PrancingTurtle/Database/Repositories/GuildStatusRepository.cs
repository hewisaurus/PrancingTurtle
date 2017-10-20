using System.Linq;
using Dapper;
using Database.Repositories.Interfaces;
using Logging;

namespace Database.Repositories
{
    public class GuildStatusRepository : DapperRepositoryBase, IGuildStatusRepository
    {
        private readonly ILogger _logger;

        public GuildStatusRepository(IConnectionFactory connectionFactory, ILogger logger) : base(connectionFactory)
        {
            _logger = logger;
        }

        public int GetDefaultApprovedStatus()
        {
            string timeElapsed;
            return
                Query(q => q.Query<int>(MySQL.GuildStatus.GetDefaultApprovedStatus), out timeElapsed).SingleOrDefault();
        }
    }
}
