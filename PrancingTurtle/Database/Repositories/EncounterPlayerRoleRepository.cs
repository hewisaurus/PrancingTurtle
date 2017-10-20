using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Database.Models;
using Database.Models.Misc;
using Database.QueryModels.Misc;
using Database.Repositories.Interfaces;
using Logging;

namespace Database.Repositories
{
    public class EncounterPlayerRoleRepository : DapperRepositoryBase, IEncounterPlayerRoleRepository
    {
        private readonly ILogger _logger;

        public EncounterPlayerRoleRepository(IConnectionFactory connectionFactory, ILogger logger)
            : base(connectionFactory)
        {
            _logger = logger;
        }

        public List<EncounterPlayerRole> GetAll()
        {
            string timeElapsed;
            return
                Query(q => q.Query<EncounterPlayerRole>(MySQL.EncounterPlayerRole.GetAllSorted), out timeElapsed)
                    .ToList();
        }

        public void RemoveDuplicates(List<long> removeIds, string email)
        {
            string timeElapsed;
            try
            {
                int totalRemove = removeIds.Count;
                if (removeIds.Count > 500)
                {
                    while (removeIds.Count > 500)
                    {
                        var theseIds = removeIds.Take(500);

                        Execute(q => q.Execute(MySQL.EncounterPlayerRole.DeleteMultiple, new { @ids = theseIds }), out timeElapsed);

                        removeIds.RemoveRange(0, 500);
                    }

                    if (removeIds.Any())
                    {
                        Execute(q => q.Execute(MySQL.EncounterPlayerRole.DeleteMultiple, new { @ids = removeIds }),
                            out timeElapsed);
                    }
                }
                else
                {
                    Execute(q => q.Execute(MySQL.EncounterPlayerRole.DeleteMultiple, new { @ids = removeIds }), out timeElapsed);
                }
                _logger.Debug(string.Format("Finished removing {0} duplicates!", totalRemove));
            }
            catch (Exception ex)
            {
                _logger.Debug(string.Format("An error occurred while trying to remove EncounterPlayerRole records - {0}", ex.Message));
            }
        }

        public List<EncounterPlayerRole> GetAllForEncounter(int encounterId)
        {
            string timeElapsed;
            return
                Query(q => q.Query<EncounterPlayerRole>(MySQL.EncounterPlayerRole.GetAllForEncounter, new { encounterId }), out timeElapsed)
                    .ToList();
        }
        
    }
}
