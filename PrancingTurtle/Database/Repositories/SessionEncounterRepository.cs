using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Database.Models;
using Database.Repositories.Interfaces;
using Logging;

namespace Database.Repositories
{
    public class SessionEncounterRepository : DapperRepositoryBase, ISessionEncounterRepository
    {
        private readonly ILogger _logger;

        public SessionEncounterRepository(IConnectionFactory connectionFactory, ILogger logger) : base(connectionFactory)
        {
            _logger = logger;
        }

        public List<int> GetEncounterIds()
        {
            string timeElapsed;
            var result = Query(q => q.Query<int>(SQL.SessionEncounter.EncounterIds), out timeElapsed).ToList();
            _logger.Debug(string.Format("Retrieved all encounters (in sessions) in {0}", timeElapsed));
            return result;
        }

        public Session GetSessionForEncounter(int encounterId)
        {
            string timeElapsed;
            return
                Query(q => q.Query<Session>(MySQL.SessionEncounter.GetSessionForEncounter, new { encounterId }),
                    out timeElapsed).SingleOrDefault();
        }

        public async Task<Session> GetSessionForEncounterAsync(int encounterId)
        {
            return (await QueryAsync(q => q.QueryAsync<Session>(MySQL.SessionEncounter.GetSessionForEncounter, new { encounterId }))).SingleOrDefault();
        }
    }
}
