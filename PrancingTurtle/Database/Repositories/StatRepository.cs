using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Dapper;
using Database.QueryModels;
using Database.Repositories.Interfaces;
using Logging;

namespace Database.Repositories
{
    public class StatRepository : DapperRepositoryBase, IStatRepository
    {
        private readonly ILogger _logger;

        public StatRepository(IConnectionFactory connectionFactory, ILogger logger) : base(connectionFactory)
        {
            _logger = logger;
        }

        public SiteStats GetSiteStats()
        {
            try
            {
                var stats = new SiteStats();
                Stopwatch sw = new Stopwatch();
                sw.Start();
                using (var connection = OpenConnection())
                {
                    stats.TotalUsers = connection.Query<long>(MySQL.StatisticsSql.TotalUsers).SingleOrDefault();
                    stats.TotalCharacters = connection.Query<long>(MySQL.StatisticsSql.TotalCharacters).SingleOrDefault();
                    stats.TotalGuilds = connection.Query<long>(MySQL.StatisticsSql.TotalGuilds).SingleOrDefault();
                    stats.TotalSessions = connection.Query<long>(MySQL.StatisticsSql.TotalSessions).SingleOrDefault();
                    stats.TotalEncounters = connection.Query<long>(MySQL.StatisticsSql.TotalEncounters).SingleOrDefault();
                    var encNotRemoved = connection.Query<long>(MySQL.StatisticsSql.TotalEncountersNotRemoved).SingleOrDefault();
                    var dmgRows = (IDictionary<string, object>)connection.Query(MySQL.StatisticsSql.TotalDamageRows).Single();
                    stats.TotalDamageRecords = Convert.ToInt64(dmgRows.First().Value);
                    var healRows = (IDictionary<string, object>)connection.Query(MySQL.StatisticsSql.TotalHealingRows).Single();
                    stats.TotalHealingRecords = Convert.ToInt64(healRows.First().Value);
                    var shieldRows = (IDictionary<string, object>)connection.Query(MySQL.StatisticsSql.TotalShieldingRows).Single();
                    stats.TotalShieldingRecords = Convert.ToInt64(shieldRows.First().Value);
                    stats.AverageDamageRecords = stats.TotalEncounters > 0 ? stats.TotalDamageRecords / encNotRemoved : 0;
                    stats.AverageHealingRecords = stats.TotalEncounters > 0 ? stats.TotalHealingRecords / encNotRemoved : 0;
                    stats.AverageShieldingRecords = stats.TotalEncounters > 0 ? stats.TotalShieldingRecords / encNotRemoved : 0;
                    var allDurations = connection.Query<TimeSpan>(MySQL.StatisticsSql.TotalDurationNotSum).ToList();
                    stats.TotalDuration = new TimeSpan(allDurations.Sum(d => d.Ticks));
                    stats.TotalLogSize = Convert.ToInt64(connection.Query<decimal>(MySQL.StatisticsSql.TotalLogSize).SingleOrDefault());
                    stats.TotalPlayedTimeTicks = Convert.ToInt64(connection.Query<decimal>(MySQL.StatisticsSql.TotalPlayedTimeTicks).SingleOrDefault());
                    stats.TotalLogLines = Convert.ToInt64(connection.Query<decimal>(MySQL.StatisticsSql.TotalLogLines).SingleOrDefault());
                    stats.TotalPlayers = connection.Query<long>(MySQL.StatisticsSql.TotalPlayers).SingleOrDefault();
                }

                sw.Stop();
                _logger.Debug(string.Format("Returned stats in {0}", sw.Elapsed));

                return stats;
            }
            catch (Exception ex)
            {
                _logger.Debug(string.Format("Error while getting site statistics: {0}", ex.Message));
                return null;
            }
        }
    }
}
