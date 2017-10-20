using System.Collections.Generic;
using System.Linq;
using Dapper;
using Database.Models;
using Database.Repositories.Interfaces;
using Logging;

namespace Database.Repositories
{
    public class NavigationRepository : DapperRepositoryBase, INavigationRepository
    {
        private readonly ILogger _logger;

        public NavigationRepository(IConnectionFactory connectionFactory, ILogger logger) : base(connectionFactory)
        {
            _logger = logger;
        }

        /// <summary>
        /// Gets the list of guilds and shards for the guild nav menu. Updated for MySQL
        /// </summary>
        /// <returns></returns>
        public List<Guild> GetGuildNavigation()
        {
            string timeElapsed;
            return Query(q => q.Query<Guild, Shard, Guild>
                (MySQL.Guild.GetGuildNavigation, (g, s) =>
                {
                    g.Shard = s;
                    return g;
                }), out timeElapsed).ToList();
        }
    }
}
