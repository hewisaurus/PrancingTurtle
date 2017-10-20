using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Dapper;
using Database.Models;
using Database.Models.Misc;
using Database.QueryModels.Misc;
using Database.Repositories.Interfaces;

namespace Database.Repositories
{
    public class LeaderboardRepository : DapperRepositoryBase, ILeaderboardRepository
    {
        public LeaderboardRepository(IConnectionFactory connectionFactory)
            : base(connectionFactory)
        {
        }
        
        public Leaderboards GetLeaderboards_v2(int bossFightId, int topX, int difficultyId)
        {
            var leaderboards = new Leaderboards();

            string timeElapsed;
            var dbDifficulty = difficultyId;

            Func<RankPlayerGuild, Player, Guild, RankPlayerGuild> map = (rpg, p, g) =>
            {
                rpg.Player = p;
                if (g != null)
                {
                    rpg.Guild = g;
                }
                return rpg;
            };

            using (var connection = OpenConnection())
            {
                #region Get difficulty
                if (difficultyId == -1)
                {
                    var defaultDifficulty = connection.Query<EncounterDifficulty>(MySQL.EncounterDifficulty.Default).SingleOrDefault();
                    if (defaultDifficulty != null)
                    {
                        dbDifficulty = defaultDifficulty.Id;
                    }
                }
                #endregion
                #region Fastest kills
                var dbKillList = connection.Query<Encounter, Guild, Shard, Encounter>
                    (MySQL.Encounter.GetFastestKills,
                        (e, g, s) =>
                        {
                            g.Shard = s;
                            e.UploadGuild = g;
                            return e;
                        }, new { @id = bossFightId, difficultyId }).ToList();

                var sortedKillList = new List<Encounter>();

                foreach (var kill in dbKillList)
                {
                    if (!sortedKillList.Any(k => k.GuildId == kill.GuildId))
                    {
                        sortedKillList.Add(kill);
                    }
                }

                if (sortedKillList.Any())
                {
                    leaderboards.FastestKills = sortedKillList.ToList();
                    leaderboards.SingleFastestKill = leaderboards.FastestKills.First();
                }
                #endregion
                #region Top APS/DPS/HPS
                var topAps = connection.Query(MySQL.BossFight.GetTopXPerSecond("APS"), map, new { @id = bossFightId, @limit = 1000000, difficultyId }).ToList();
                if (topAps.Any())
                {
                    leaderboards.TopAps = topAps.Take(10).ToList();
                    leaderboards.SingleTopAps = leaderboards.TopAps.First();
                    leaderboards.WarriorTopAps = topAps.Where(c => c.Class == "Warrior").Take(10).ToList();
                    leaderboards.MageTopAps = topAps.Where(c => c.Class == "Mage").Take(10).ToList();
                    leaderboards.ClericTopAps = topAps.Where(c => c.Class == "Cleric").Take(10).ToList();
                    leaderboards.RogueTopAps = topAps.Where(c => c.Class == "Rogue").Take(10).ToList();
                    leaderboards.PrimalistTopAps = topAps.Where(c => c.Class == "Primalist").Take(10).ToList();
                }
                var topDps = connection.Query(MySQL.BossFight.GetTopXPerSecond("DPS"), map, new { @id = bossFightId, @limit = 1000000, difficultyId }).ToList();
                if (topDps.Any())
                {
                    leaderboards.TopDps = topDps.Take(10).ToList();
                    leaderboards.SingleTopDps = leaderboards.TopDps.First();
                    leaderboards.WarriorTopDps = topDps.Where(c => c.Class == "Warrior").Take(10).ToList();
                    leaderboards.MageTopDps = topDps.Where(c => c.Class == "Mage").Take(10).ToList();
                    leaderboards.ClericTopDps = topDps.Where(c => c.Class == "Cleric").Take(10).ToList();
                    leaderboards.RogueTopDps = topDps.Where(c => c.Class == "Rogue").Take(10).ToList();
                    leaderboards.PrimalistTopDps = topDps.Where(c => c.Class == "Primalist").Take(10).ToList();
                    //leaderboards.TopSupportDps = topDps.Where(c => c.Role == "Support").Take(10).ToList(); REMOVED - inaccurate

                    
                }
                var topHps = connection.Query(MySQL.BossFight.GetTopXPerSecond("HPS"), map, new { @id = bossFightId, @limit = 1000000, difficultyId }).ToList();
                if (topHps.Any())
                {
                    leaderboards.TopHps = topHps.Take(10).ToList();
                    leaderboards.SingleTopHps = leaderboards.TopHps.First();
                    leaderboards.WarriorTopHps = topHps.Where(c => c.Class == "Warrior").Take(10).ToList();
                    leaderboards.MageTopHps = topHps.Where(c => c.Class == "Mage").Take(10).ToList();
                    leaderboards.ClericTopHps = topHps.Where(c => c.Class == "Cleric").Take(10).ToList();
                    leaderboards.RogueTopHps = topHps.Where(c => c.Class == "Rogue").Take(10).ToList();
                    leaderboards.PrimalistTopHps = topHps.Where(c => c.Class == "Primalist").Take(10).ToList();
                }
                var topSingleTargetDps = connection.Query(MySQL.BossFight.GetTopSingleTargetDps, map, new { @id = bossFightId, @limit = 1000000, difficultyId }).ToList();
                if (topSingleTargetDps.Any())
                {
                    leaderboards.TopSingleTargetDps = topSingleTargetDps.Take(10).ToList();
                    leaderboards.WarriorTopSingleTarget = topSingleTargetDps.Where(c => c.Class == "Warrior").Take(10).ToList();
                    leaderboards.MageTopSingleTarget = topSingleTargetDps.Where(c => c.Class == "Mage").Take(10).ToList();
                    leaderboards.ClericTopSingleTarget = topSingleTargetDps.Where(c => c.Class == "Cleric").Take(10).ToList();
                    leaderboards.RogueTopSingleTarget = topSingleTargetDps.Where(c => c.Class == "Rogue").Take(10).ToList();
                    leaderboards.PrimalistTopSingleTarget = topSingleTargetDps.Where(c => c.Class == "Primalist").Take(10).ToList();
                }
                var topSupportDps = connection.Query(MySQL.BossFight.GetTopXPerSecondPerRole("DPS"), map, new { id = bossFightId, limit = 1000000, difficultyId, @role = "Support" }).ToList();
                if (topSupportDps.Any())
                {
                    leaderboards.TopSupportDps = topSupportDps.Take(10).ToList();
                    leaderboards.WarriorTopSupportDps = topSupportDps.Where(c => c.Class == "Warrior").Take(10).ToList();
                    leaderboards.MageTopSupportDps = topSupportDps.Where(c => c.Class == "Mage").Take(10).ToList();
                    leaderboards.ClericTopSupportDps = topSupportDps.Where(c => c.Class == "Cleric").Take(10).ToList();
                    leaderboards.RogueTopSupportDps = topSupportDps.Where(c => c.Class == "Rogue").Take(10).ToList();
                    leaderboards.PrimalistTopSupportDps = topSupportDps.Where(c => c.Class == "Primalist").Take(10).ToList();
                }


                //leaderboards.WarriorTopSingleTarget = connection.Query(MySQL.BossFight.GetTopSingleTargetDpsPerClass("Warrior"), map, new { @id = encounterId, @limit = topX, difficultyId }).ToList();
                //leaderboards.MageTopSingleTarget = connection.Query(MySQL.BossFight.GetTopSingleTargetDpsPerClass("Mage"), map, new { @id = encounterId, @limit = topX, difficultyId }).ToList();
                //leaderboards.ClericTopSingleTarget = connection.Query(MySQL.BossFight.GetTopSingleTargetDpsPerClass("Cleric"), map, new { @id = encounterId, @limit = topX, difficultyId }).ToList();
                //leaderboards.RogueTopSingleTarget = connection.Query(MySQL.BossFight.GetTopSingleTargetDpsPerClass("Rogue"), map, new { @id = encounterId, @limit = topX, difficultyId }).ToList();
                //leaderboards.PrimalistTopSingleTarget = connection.Query(MySQL.BossFight.GetTopSingleTargetDpsPerClass("Primalist"), map, new { @id = encounterId, @limit = topX, difficultyId }).ToList();

                #endregion
                #region Top burst
                #region Damage

                var topDamage1SBurst = connection.Query(
                    MySQL.BossFight.GetTopBurstX(BurstFilter.Duration.OneSecond, BurstFilter.Type.Damage), map,
                    new { id = bossFightId, limit = 1000000, difficultyId }).ToList();
                if (topDamage1SBurst.Any())
                {
                    leaderboards.TopBurstDamage1S = new List<RankPlayerGuild>();
                    leaderboards.TopBurstDamage1S.AddRange(topDamage1SBurst.Where(b => b.Class == "Warrior").OrderByDescending(b => b.Value).Take(10));
                    leaderboards.TopBurstDamage1S.AddRange(topDamage1SBurst.Where(b => b.Class == "Mage").OrderByDescending(b => b.Value).Take(10));
                    leaderboards.TopBurstDamage1S.AddRange(topDamage1SBurst.Where(b => b.Class == "Cleric").OrderByDescending(b => b.Value).Take(10));
                    leaderboards.TopBurstDamage1S.AddRange(topDamage1SBurst.Where(b => b.Class == "Rogue").OrderByDescending(b => b.Value).Take(10));
                    leaderboards.TopBurstDamage1S.AddRange(topDamage1SBurst.Where(b => b.Class == "Primalist").OrderByDescending(b => b.Value).Take(10));
                }

                #endregion

                #endregion
            }

            return leaderboards;
        }
    }
}
