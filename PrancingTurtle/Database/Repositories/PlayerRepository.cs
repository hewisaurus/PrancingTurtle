using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Common;
using Dapper;
using Database.Models;
using Database.QueryModels.Misc;
using Database.Repositories.Interfaces;
using Logging;

namespace Database.Repositories
{
    public class PlayerRepository : DapperRepositoryBase, IPlayerRepository
    {
        private readonly ILogger _logger;

        public PlayerRepository(IConnectionFactory connectionFactory, ILogger logger)
            : base(connectionFactory)
        {
            _logger = logger;
        }

        public Player Get(int id)
        {
            const string query = "SELECT * FROM Player P LEFT JOIN PlayerClass PC ON P.PlayerClassId = PC.Id WHERE P.Id = @playerId";
            string timeElapsed;
            return Query(q => q.Query<Player, PlayerClass, Player>
                (query, (p, pc) =>
                {
                    if (pc != null)
                    {
                        p.
                            PlayerClass = pc;
                    }
                    return p;
                }, new { @playerId = id }), out timeElapsed).SingleOrDefault();
        }

        public List<Player> GetByIds(List<int> playerIds)
        {
            string timeElapsed;
            return Query(q => q.Query<Player>(MySQL.Player.GetByIds, new { playerIds }), out timeElapsed).ToList();
        }

        public PlayerSearchResult GetFromSearch(int id)
        {
            string timeElapsed;
            return
                Query(q => q.Query<PlayerSearchResult>(MySQL.Player.SearchFromPlayerId, new { @playerId = id }),
                    out timeElapsed).SingleOrDefault();
        }

        public PlayerSearchResult GetSingleFromPlayerId(int playerId)
        {
            string timeElapsed;
            return
                Query(q => q.Query<PlayerSearchResult>(MySQL.Player.GetSinglePlayerSearchFromPlayerId, new { playerId }),
                    out timeElapsed).SingleOrDefault();
        }

        /// <summary>
        /// Updated for MySQL
        /// </summary>
        /// <param name="logId"></param>
        /// <returns></returns>
        public string GetTargetNameFromLogId(string logId)
        {
            string timeElapsed;
            return Query(q => q.Query<string>(MySQL.Player.GetPlayerNameFromLogId, new { logId }), out timeElapsed)
                    .SingleOrDefault();

            //var targetName = "";

            //targetName =
            //    Query(q => q.Query<string>(MySQL.Player.GetPlayerNameFromLogId, new { logId }), out timeElapsed)
            //        .SingleOrDefault();

            //if (string.IsNullOrEmpty(targetName))
            //{
            //    targetName =
            //    Query(q => q.Query<string>(MySQL.Player.GetNpcNameFromLogId, new { logId }), out timeElapsed)
            //        .SingleOrDefault();
            //}

            //return targetName;
        }

        /// <summary>
        /// Returns a list of players that have @ in their name, and need to be updated
        /// </summary>
        /// <returns></returns>
        public List<Player> PlayersWithShardNamesInPlayerName()
        {
            string timeElapsed;
            return Query(q => q.Query<Player>(MySQL.Player.PlayerWithShardNames), out timeElapsed).ToList();
        }

        public ReturnValue UpdatePlayerNameAndShard(List<Player> updatePlayers)
        {
            var returnValue = new ReturnValue();

            Stopwatch sw = new Stopwatch();

            try
            {
                sw.Start();

                using (var connection = OpenConnection())
                {
                    foreach (var player in updatePlayers)
                    {
                        connection.Execute(MySQL.Player.UpdatePlayerShardName,
                            new { @name = player.Name, @shard = player.Shard, @id = player.Id });
                    }
                }

                sw.Stop();
                _logger.Debug(string.Format("Player names and shards updated in {0}", sw.Elapsed));

                returnValue.Success = true;
            }
            catch (Exception ex)
            {
                sw.Stop();
                returnValue.Message = ex.Message;
                _logger.Debug(string.Format("Error while updating player names and shards: {0}", ex.Message));
            }

            return returnValue;
        }

        public List<Player> GetAll()
        {
            string timeElapsed;
            return Query(q => q.Query<Player>(MySQL.Player.GetAll), out timeElapsed).ToList();
        }

        public List<int> GetAllUniquePlayerIds()
        {
            string timeElapsed;
            return Query(q => q.Query<int>(MySQL.Player.GetAllUniquePlayerIds, commandTimeout: 300 //5 minutes
                ), out timeElapsed).ToList();
        }

        public ReturnValue RemoveOrphanPlayers(List<int> playerIds)
        {
            var returnValue = new ReturnValue();

            Stopwatch sw = new Stopwatch();

            try
            {
                sw.Start();

                using (var connection = OpenConnection())
                {
                    while (true)
                    {
                        if (playerIds.Count <= 100)
                        {
                            connection.Execute(MySQL.Player.RemovePlayerById, new { @Ids = playerIds }, commandTimeout: 120); // 2 minutes
                            break;
                        }
                        var smallerPlayerIdList = playerIds.Take(100);

                        connection.Execute(MySQL.Player.RemovePlayerById, new { @Ids = smallerPlayerIdList }, commandTimeout: 120); // 2 minutes

                        playerIds.RemoveRange(0, 100);
                    }
                }

                sw.Stop();
                _logger.Debug(string.Format("Orphaned player record remove completed in {0}", sw.Elapsed));

                returnValue.Success = true;
            }
            catch (Exception ex)
            {
                sw.Stop();
                returnValue.Message = ex.Message;
                _logger.Debug(string.Format("Error while removing orphaned player records: {0}", ex.Message));
            }

            return returnValue;
        }
    }
}
