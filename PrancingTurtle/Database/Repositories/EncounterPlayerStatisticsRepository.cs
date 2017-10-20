using Database.Repositories.Interfaces;
using Logging;

namespace Database.Repositories
{
    public class EncounterPlayerStatisticsRepository : DapperRepositoryBase, IEncounterPlayerStatisticsRepository
    {
        private readonly ILogger _logger;

        public EncounterPlayerStatisticsRepository(IConnectionFactory connectionFactory, ILogger logger) : base(connectionFactory)
        {
            _logger = logger;
        }
    }
}
