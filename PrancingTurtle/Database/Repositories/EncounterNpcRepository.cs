using System.Collections.Generic;
using System.Linq;
using Dapper;
using Database.Repositories.Interfaces;
using Logging;

namespace Database.Repositories
{
    public class EncounterNpcRepository : DapperRepositoryBase, IEncounterNpcRepository
    {
        private readonly ILogger _logger;

        public EncounterNpcRepository(IConnectionFactory connectionFactory, ILogger logger) : base(connectionFactory)
        {
            _logger = logger;
        }

       
    }
}
