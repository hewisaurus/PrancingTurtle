using System.Collections.Generic;
using System.Linq;
using Dapper;
using Database.Models;
using Database.Repositories.Interfaces;
using Logging;

namespace Database.Repositories
{
    public class NewsRecentChangesRepository : DapperRepositoryBase, INewsRecentChangesRepository
    {
        private readonly ILogger _logger;

        public NewsRecentChangesRepository(IConnectionFactory connectionFactory, ILogger logger) : base(connectionFactory)
        {
            _logger = logger;
        }


        public List<NewsRecentChanges> GetRecentChanges()
        {
            string timeElapsed;
            return
                Query(q => q.Query<NewsRecentChanges>(MySQL.NewsRecentChanges.GetRecentChanges), out timeElapsed)
                    .ToList();
        }
    }
}
