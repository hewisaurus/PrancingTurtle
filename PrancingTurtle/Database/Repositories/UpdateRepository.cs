using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Database.Models;
using Database.QueryModels;
using Database.Repositories.Interfaces;
using Logging;

namespace Database.Repositories
{
    public class UpdateRepository : DapperRepositoryBase, IUpdateRepository
    {
        private readonly ILogger _logger;

        public UpdateRepository(IConnectionFactory connectionFactory, ILogger logger)
            : base(connectionFactory)
        {
            _logger = logger;
        }



        public void UpdateAllPlayerClasses()
        {
            try
            {
                string timeElapsed;
                var players = Query(q => q.Query<Player>(SQL.Player.GetAll), out timeElapsed).ToList();

                var classes = Query(q => q.Query<PlayerClass>(SQL.PlayerClass.GetAll), out timeElapsed).ToList();

                // Key: ClassId, Value: list of PlayerId to update with the key
                var playerClassIdsToUpdate = new Dictionary<int, List<int>>();

                foreach (var player in players)
                {
                    var playerId = player.Id;
                    #region Shielding
                    var shieldResult = Query(q => q.Query<PlayerClassNameCount>
                        (SQL.Player.Class.FromShieldingDone,
                        new { @playerId = playerId, @minPoints = 20 }),
                        out timeElapsed).ToList();
                    if (shieldResult.Any())
                    {
                        var playerClass = shieldResult.First();
                        var classId = classes.First(c => c.Name == playerClass.ClassName).Id;
                        #region Check if the existing one is different or null, and set it if it is
                        if (player.PlayerClassId == null || player.PlayerClassId != classId)
                        {
                            if (playerClassIdsToUpdate.ContainsKey(classId))
                            {
                                playerClassIdsToUpdate[classId].Add(player.Id);
                            }
                            else
                            {
                                playerClassIdsToUpdate.Add(classId, new List<int>() { player.Id });
                            }
                            continue;
                        }
                        #endregion
                    }
                    #endregion
                    #region Healing
                    var healResult = Query(q => q.Query<PlayerClassNameCount>
                        (SQL.Player.Class.FromHealingDone,
                        new { @playerId = playerId, @minPoints = 20 }),
                        out timeElapsed).ToList();
                    if (healResult.Any())
                    {
                        var playerClass = healResult.First();
                        var classId = classes.First(c => c.Name == playerClass.ClassName).Id;
                        #region Check if the existing one is different or null, and set it if it is
                        if (player.PlayerClassId == null || player.PlayerClassId != classId)
                        {
                            if (playerClassIdsToUpdate.ContainsKey(classId))
                            {
                                playerClassIdsToUpdate[classId].Add(player.Id);
                            }
                            else
                            {
                                playerClassIdsToUpdate.Add(classId, new List<int>() { player.Id });
                            }
                            continue;
                        }
                        #endregion
                    }
                    #endregion
                    #region Damage
                    var damageResult = Query(q => q.Query<PlayerClassNameCount>
                        (SQL.Player.Class.FromDamageDone,
                        new { @playerId = playerId, @minPoints = 20 }),
                        out timeElapsed).ToList();
                    if (damageResult.Any())
                    {
                        var playerClass = damageResult.First();
                        var classId = classes.First(c => c.Name == playerClass.ClassName).Id;
                        #region Check if the existing one is different or null, and set it if it is
                        if (player.PlayerClassId == null || player.PlayerClassId != classId)
                        {
                            if (playerClassIdsToUpdate.ContainsKey(classId))
                            {
                                playerClassIdsToUpdate[classId].Add(player.Id);
                            }
                            else
                            {
                                playerClassIdsToUpdate.Add(classId, new List<int>() { player.Id });
                            }
                        }
                        #endregion
                    }
                    #endregion
                }

                if (playerClassIdsToUpdate.Any())
                {
                    foreach (var kvp in playerClassIdsToUpdate)
                    {
                        var classId = kvp.Key;
                        string updateQuery = 
                            string.Format("UPDATE [Player] SET PlayerClassId = @classId WHERE Id IN ({0})",
                            string.Join(", ", kvp.Value));
                        Execute(q => q.Execute(updateQuery, new { @classId = classId }), out timeElapsed);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Debug(string.Format("An error occurred while updating classes! {0}", ex.Message));
            }
        }

    }
}
