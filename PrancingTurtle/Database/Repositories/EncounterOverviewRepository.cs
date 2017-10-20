using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Dapper;
using Database.Models;
using Database.Repositories.Interfaces;
using Logging;

namespace Database.Repositories
{
    public class EncounterOverviewRepository : DapperRepositoryBase, IEncounterOverviewRepository
    {
        private readonly ILogger _logger;

        public EncounterOverviewRepository(IConnectionFactory connectionFactory, ILogger logger) : base(connectionFactory)
        {
            _logger = logger;
        }

        /// <summary>
        /// Updated for MySQL
        /// </summary>
        /// <param name="overview"></param>
        /// <returns></returns>
        public ReturnValue Add(EncounterOverview overview)
        {
            var returnValue = new ReturnValue();

            try
            {
                DapperDb dapperDb = DapperDb.Init(OpenConnection(), 3, false);

                var newId = dapperDb.EncounterOverviewTable.Insert(
                    new //EncounterOverview()
                    {
                        EncounterId = overview.EncounterId,
                        AverageDps = overview.AverageDps,
                        PlayerDeaths = overview.PlayerDeaths,
                        AverageHps = overview.AverageHps,
                        AverageAps = overview.AverageAps
                    });

                if (newId > 0)
                {
                    _logger.Debug(string.Format("Overview has successfully created for encounter {0}", overview.EncounterId));
                    returnValue.Message = newId.ToString();
                    returnValue.Success = true;
                }
            }
            catch (Exception ex)
            {
                returnValue.Message = ex.Message;
            }

            return returnValue;
        }
        
        /// <summary>
        /// This method is really just used to check which encounters don't have HPS and APS values for the overview
        /// </summary>
        /// <returns></returns>
        public List<Encounter> GetEncountersWithIncompleteOverviews(int limit = 100)
        {
            // The default value for AverageHps and AverageAps is -1, some encounters may be valid and have 0 APS or 0 HPS
            string timeElapsed;
            return
                Query(q => q.Query<Encounter>(MySQL.EncounterOverview.GetEncountersMissingHPSorAPS, new { limit }), out timeElapsed)
                    .ToList();
        }
    }
}
