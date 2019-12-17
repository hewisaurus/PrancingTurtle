using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Dapper;
using Database.Models;
using Database.Models.Misc;
using Database.MySQL;
using Database.QueryModels;
using Database.QueryModels.Misc;
using Database.Repositories.Interfaces;
using Database.SQL;
using DiscordLogger.Provider;
using Logging;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Logging;
using Ability = Database.Models.Ability;
using EncounterNpc = Database.Models.EncounterNpc;
using AuthUserCharacter = Database.Models.AuthUserCharacter;
using BossFight = Database.Models.BossFight;
using Encounter = Database.Models.Encounter;
using Guild = Database.Models.Guild;
using Player = Database.Models.Player;
using Shard = Database.Models.Shard;
using EncounterPlayerStatistics = Database.Models.EncounterPlayerStatistics;
using EncounterPlayerRole = Database.Models.EncounterPlayerRole;

namespace Database.Repositories
{
    public class EncounterRepository : DapperRepositoryBase, IEncounterRepository
    {
        private readonly Logging.ILogger _logger;
        private readonly IAuthenticationRepository _authRepository;
        private readonly IAuthUserCharacterRepository _authUserCharacterRepository;
        public readonly IDiscordService _discord;

        public EncounterRepository(IConnectionFactory connectionFactory, Logging.ILogger logger,
            IAuthUserCharacterRepository authUserCharacterRepository, IAuthenticationRepository authRepository, IDiscordService discord)
            : base(connectionFactory)
        {
            _logger = logger;
            _authUserCharacterRepository = authUserCharacterRepository;
            _authRepository = authRepository;
            _discord = discord;
        }

        public async Task<EncounterPlayersAndRoles> CountEncounterPlayersAndRoles(int id)
        {
            return (await QueryAsync(
                    q => q.QueryAsync<EncounterPlayersAndRoles>
                        (SQL.Encounter.PlayerRoles.CountPlayersAndRolesForEncounter, new { id })))
                .SingleOrDefault();
        }

        public async Task<List<int>> GetAllEncounterIdsDescending()
        {
            return (await QueryAsync(q => q.QueryAsync<int>(SQL.Encounter.GetAllIds))).AsList();
        }

        public async Task<bool> RemoveRoleRecordsForEncounter(int id)
        {
            var result = await ExecuteAsync(q => q.ExecuteAsync(SQL.Encounter.PlayerRoles.RemoveRecords, new { id }));
            return result > 0;
        }

        public ReturnValue ChangePrivacy(int id, int userId, bool setToPublic = false)
        {
            string timeElapsed;
            string query = setToPublic ? SQL.Encounter.MakePublic : SQL.Encounter.MakePrivate;

            var returnValue = new ReturnValue();
            try
            {
                var enc = Get(id);
                if (enc.GuildId != null)
                {
                    // Check if the current user is part of the same guild
                    var guildMembers = _authUserCharacterRepository.GetGuildMembers((int)enc.GuildId);
                    if (guildMembers.Any(m => m.AuthUserId == userId))
                    {
                        // Get the character with the highest rank within this guild and see if the user is allowed to modify privacy
                        var highestRankedCharacter =
                            _authUserCharacterRepository.GetCharacterWithHighestGuildRank(userId, (int)enc.GuildId);
                        if (highestRankedCharacter != null)
                        {
                            var result = Execute(q => q.Execute(query, new { id }), out timeElapsed);
                            returnValue.Success = result > 0;
                            _logger.Debug(result > 0
                                ? string.Format("Encounter {0} successfully set to private", id)
                                : string.Format("Failed to set encounter {0} to private", id));
                        }
                    }
                }
                else if (enc.UploaderId != null && enc.GuildId == null)
                {
                    // If the uploader isn't null but the guild is, check privacy settings against the logged-in user.
                    // We make sure the Guild ID is null here in case the uploader leaves the guild and encounters should
                    // still be set to private.

                    // Check if the current user is the original uploader
                    var user = _authRepository.Get(userId);
                    if (user != null)
                    {
                        var userCharacters = _authUserCharacterRepository.GetCharacters(user.Email);
                        if (userCharacters.Any(u => u.Id == (int)enc.UploaderId))
                        {
                            var result = Execute(q => q.Execute(query, new { id }), out timeElapsed);
                            returnValue.Success = result > 0;
                            _logger.Debug(result > 0
                                ? string.Format("Encounter {0} successfully set to private", id)
                                : string.Format("Failed to set encounter {0} to private", id));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format("An error occurred while trying to set encounter {0} privacy: {1}", id,
                    ex.Message);
                _logger.Debug(msg);
                returnValue.Message = msg;
            }

            return returnValue;
        }

        public void MakeValidForRankings(int id)
        {
            string timeElapsed;
            var result = Execute(q => q.Execute(SQL.Encounter.MakeValidForRankings, new { id }), out timeElapsed);
            _logger.Debug(result > 0
                ? string.Format("Encounter {0} successfully set to ValidForRankings", id)
                : string.Format("Failed to set encounter {0} to ValidForRankings", id));
        }

        public void MakeValidForRankings(int id, int difficultyId)
        {
            string timeElapsed;
            var result = Execute(q => q.Execute(SQL.Encounter.MakeValidForRankingsIncDifficulty, new { id, difficultyId }), out timeElapsed);
            _logger.Debug(result > 0
                ? string.Format("Encounter {0} successfully set to ValidForRankings", id)
                : string.Format("Failed to set encounter {0} to ValidForRankings", id));
        }

        public void MakeInvalidForRankings(int id)
        {
            string timeElapsed;
            var result = Execute(q => q.Execute(SQL.Encounter.MakeInvalidForRankings, new { id }), out timeElapsed);
            _logger.Debug(result > 0
                ? string.Format("Encounter {0} successfully set to InvalidForRankings", id)
                : string.Format("Failed to set encounter {0} to InvalidForRankings", id));
        }

        /// <summary>
        /// Gets a single encounter by its ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Encounter Get(int id)
        {
            string timeElapsed;
            var result = Query(q => q.Query<Encounter, Models.EncounterDifficulty, BossFight, Models.Instance, AuthUserCharacter, Shard, Guild, Encounter>
                (SQL.Encounter.GetSingle,
                    (e, ed, bf, i, auc, s, g) =>
                    {
                        e.EncounterDifficulty = ed;
                        bf.Instance = i;
                        e.BossFight = bf;
                        auc.Shard = s;
                        e.UploadCharacter = auc;
                        if (g != null)
                        {
                            e.UploadGuild = g;
                        }
                        return e;
                    }, new { id }), out timeElapsed).SingleOrDefault();

            return result;
        }

        /// <summary>
        /// Gets a list of all of the encounters that have no duration
        /// </summary>
        /// <returns></returns>
        public List<int> GetEncounterIdsWithNoDuration()
        {
            string timeElapsed;
            return Query(q => q.Query<int>(MySQL.Encounter.GetEncounterIdsWithNoDuration), out timeElapsed).ToList();
        }

        /// <summary>
        /// Gets the duration (in seconds) from the DamageDone table
        /// Useful for where encounters have no duration for whatever reason
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int GetTotalSecondsFromDamageDone(int id)
        {
            string timeElapsed;
            var seconds =
                Query(q => q.Query<long>(MySQL.Encounter.GetTotalSecondsFromDamageDone, new { id }), out timeElapsed)
                    .SingleOrDefault();
            //return Query(q => q.Query<int>(MySQL.Encounter.GetTotalSecondsFromDamageDone, new { id }), out timeElapsed).SingleOrDefault();
            //var seconds =
            //    Query(q => q.Query<long>(MySQL.Encounter.GetTotalSecondsFromDamageDone, new {id}), out timeElapsed)
            //        .SingleOrDefault();
            return Convert.ToInt32(seconds);
        }

        public List<Encounter> GetSuccessfulEncounters(int bossFightId)
        {
            string timeElapsed;
            return
                Query(
                    q =>
                        q.Query<Encounter>(SQL.Encounter.GetAllSuccessfulEncountersForSpecificBossFight,
                            new { bossFightId }), out timeElapsed).ToList();
        }

        public List<Encounter> GetSuccessfulEncounters(int bossFightId, bool onlyNotValidForRankings)
        {
            string timeElapsed;
            if (onlyNotValidForRankings)
            {
                return
                    Query(
                        q =>
                            q.Query<Encounter>(SQL.Encounter.GetAllSuccessfulButInvalidEncountersForSpecificBossFight,
                                new { bossFightId }), out timeElapsed).ToList();
            }
            return
                Query(
                    q =>
                        q.Query<Encounter>(SQL.Encounter.GetAllSuccessfulEncountersForSpecificBossFight,
                            new { bossFightId }), out timeElapsed).ToList();
        }

        public List<Encounter> GetSuccessfulEncountersSince(DateTime date)
        {
            string timeElapsed;
            return
                Query(
                    q =>
                        q.Query<Encounter>(SQL.Encounter.GetAllSuccessfulEncountersSinceDate,
                            new { date }), out timeElapsed).ToList();
        }

        public List<Encounter> GetUnsuccessfulEncountersBefore(DateTime date)
        {
            string timeElapsed;
            return
                Query(
                    q =>
                        q.Query<Encounter>(SQL.Encounter.GetAllUnsuccessfulEncountersBeforeDate,
                            new { date }), out timeElapsed).ToList();
        }

        public List<Encounter> GetUnsuccessfulEncountersBefore(DateTime date, int bossFightId)
        {
            string timeElapsed;
            return
                Query(
                    q =>
                        q.Query<Encounter>(SQL.Encounter.GetAllUnsuccessfulEncountersBeforeDateBossFight,
                            new { date, bossFightId }), out timeElapsed).ToList();
        }

        public long GetTopDamageTakenForNpc(int encounterId, string npcName)
        {
            string timeElapsed;
            if (npcName.Contains(','))
            {
                long topDamage = 0;
                var npcNames = npcName.Split(',');
                foreach (string name in npcNames)
                {
                    var totalFound = Query(q => q.Query<long>(SQL.Encounter.GetTopDamageTakenForNpc,
                            new { encounterId, name }), out timeElapsed).SingleOrDefault();
                    if (totalFound > topDamage)
                    {
                        topDamage = totalFound;
                    }
                }

                return topDamage;
            }

            return
                Query(
                    q =>
                        q.Query<long>(SQL.Encounter.GetTopDamageTakenForNpc,
                            new { encounterId, @name = npcName }), out timeElapsed).SingleOrDefault();
        }

        public List<Encounter> GetFastestKills(int id, int d = -1)
        {
            string timeElapsed;
            var difficultyId = d;
            if (d == -1)
            {
                var defaultDifficulty = Query(q => q.Query<Models.EncounterDifficulty>(MySQL.EncounterDifficulty.Default), out timeElapsed).SingleOrDefault();
                if (defaultDifficulty != null)
                {
                    difficultyId = defaultDifficulty.Id;
                }
            }
            var dbKillList = Query(q => q.Query<Encounter, Guild, Shard, Encounter>
                (MySQL.Encounter.GetFastestKills,
                    (e, g, s) =>
                    {
                        g.Shard = s;
                        e.UploadGuild = g;
                        return e;
                    }, new { id, difficultyId }), out timeElapsed).ToList();

            var sortedKillList = new List<Encounter>();

            foreach (var kill in dbKillList)
            {
                if (!sortedKillList.Any(k => k.GuildId == kill.GuildId))
                {
                    sortedKillList.Add(kill);
                }
            }

            return sortedKillList.ToList();
        }

        public List<EncounterPlayerStatistics> GetTopDamageHits(int id)
        {
            string timeElapsed;
            return
                Query(q => q.Query<EncounterPlayerStatistics>(MySQL.BossFight.GetTopDamageHits, new { id }),
                    out timeElapsed).ToList();
        }

        public List<EncounterPlayerStatistics> GetTopHealingHits(int id)
        {
            string timeElapsed;
            return
                Query(q => q.Query<EncounterPlayerStatistics>(MySQL.BossFight.GetTopHealHits, new { id }),
                    out timeElapsed).ToList();
        }

        public List<EncounterPlayerStatistics> GetTopShieldHits(int id)
        {
            string timeElapsed;
            return
                Query(q => q.Query<EncounterPlayerStatistics>(MySQL.BossFight.GetTopShieldHits, new { id }),
                    out timeElapsed).ToList();
        }

        public List<Encounter> GetDateSortedKills(int id, int d = -1)
        {
            string timeElapsed;
            var difficultyId = d;
            if (d == -1)
            {
                var defaultDifficulty = Query(q => q.Query<Models.EncounterDifficulty>(MySQL.EncounterDifficulty.Default), out timeElapsed).SingleOrDefault();
                if (defaultDifficulty != null)
                {
                    difficultyId = defaultDifficulty.Id;
                }
            }

            var queryList = Query(q => q.Query<Encounter, Guild, Encounter>
                (MySQL.Encounter.GetDateSortedKills,
                    (e, g) =>
                    {
                        e.UploadGuild = g;
                        return e;
                    }, new { id, difficultyId }), out timeElapsed).ToList();

            var skipEncounterIds = new List<int>();
            var guildGroup = queryList.GroupBy(q => q.GuildId);
            foreach (var guildEncounterList in guildGroup)
            {
                foreach (var guildEncounterListDate in guildEncounterList.GroupBy(e => e.Date.ToShortDateString()))
                {
                    if (guildEncounterListDate.Count() > 1)
                    {
                        // There's more than one kill for this date, so figure out which is the correct one and which ones to skip
                        skipEncounterIds.AddRange(guildEncounterListDate.OrderBy(e => e.Duration.TotalSeconds).Skip(1).Select(skipEnc => skipEnc.Id));
                    }
                }
            }

            return queryList.Where(e => !skipEncounterIds.Contains(e.Id)).ToList();
        }

        /// <summary>
        /// Gets the player DPS for a given encounter
        /// </summary>
        /// <param name="id">The ID of the encounter</param>
        /// <param name="totalSeconds">The duration (in seconds) of the encounter</param>
        /// <returns>The player DPS</returns>
        public long GetEncounterDps(int id, int totalSeconds)
        {
            try
            {
                if (totalSeconds == 0) return 0;

                string timeElapsed;
                var result = Query(q => q.Query<EncounterTotalAverage>
                    (SQL.Encounter.Overview.PlayerDpsAverage,
                        new { @id = id, @totalSeconds = totalSeconds }), out timeElapsed).SingleOrDefault();
                return result == null ? 0 : result.Average;
            }
            catch (Exception ex)
            {
                _logger.Debug(string.Format("An error occurred while getting encounter DPS: {0}", ex.Message));
            }
            return 0;
        }
        /// <summary>
        /// Gets the player HPS for a given encounter
        /// </summary>
        /// <param name="id">The ID of the encounter</param>
        /// <param name="totalSeconds">The length (in seconds) of the encounter</param>
        /// <returns></returns>
        public long GetEncounterHps(int id, int totalSeconds)
        {
            try
            {
                if (totalSeconds == 0) return 0;

                string timeElapsed;
                var result = Query(q => q.Query<EncounterTotalAverage>
                    (SQL.Encounter.Overview.PlayerHpsAverage,
                        new { @id = id, @totalSeconds = totalSeconds }), out timeElapsed).SingleOrDefault();
                return result == null ? 0 : result.Average;
            }
            catch (Exception ex)
            {
                _logger.Debug(string.Format("An error occurred while getting encounter HPS: {0}", ex.Message));
            }
            return 0;
        }
        /// <summary>
        /// Gets the player APS for a given encounter
        /// </summary>
        /// <param name="id">The ID of the encounter</param>
        /// <param name="totalSeconds">The length (in seconds) of the encounter</param>
        /// <returns>The player APS</returns>
        public long GetEncounterAps(int id, int totalSeconds)
        {
            try
            {
                if (totalSeconds == 0) return 0;

                string timeElapsed;
                var result = Query(q => q.Query<EncounterTotalAverage>
                    (SQL.Encounter.Overview.PlayerApsAverage,
                        new { @id = id, @totalSeconds = totalSeconds }), out timeElapsed).SingleOrDefault();
                return result == null ? 0 : result.Average;
            }
            catch (Exception ex)
            {
                _logger.Debug(string.Format("An error occurred while getting encounter APS: {0}", ex.Message));
            }
            return 0;
        }
        /// <summary>
        /// Gets the list of debuffs seen in a given encounter
        /// </summary>
        /// <param name="id">The ID of the encounter</param>
        /// <returns>A list of debuffs seen</returns>

        /// <summary>
        /// Gets the total number of player deaths in a given encounter. Updated for MySQL
        /// </summary>
        /// <param name="id">The ID of the encounter</param>
        /// <returns>The total number of deaths</returns>
        public int GetTotalPlayerDeaths(int id)
        {
            string timeElapsed;
            return Convert.ToInt32(Query(q => q.Query<long>(SQL.Encounter.Overview.TotalPlayerDeathsForEncounter, new { id }), out timeElapsed).SingleOrDefault());
        }

        /// <summary>
        /// Gets the second elapsed when each player died
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<int> GetAllPlayerDeathTimers(int id)
        {
            string timeElapsed;
            return Query(q => q.Query<int>(MySQL.Encounter.Character.Player.AllPlayerDeathsForEncounter,
                new { id }), out timeElapsed).ToList();
        }

        public List<int> GetAllNpcDeathTimers(int id)
        {
            string timeElapsed;
            return Query(q => q.Query<int>(MySQL.Encounter.Character.Npc.AllNpcDeathsForEncounter,
                new { id }), out timeElapsed).ToList();
        }

        /// <summary>
        /// Gets the list of player deaths in a given encounter
        /// </summary>
        /// <param name="id">The ID of the encounter</param>
        /// <returns></returns>
        public List<EncounterDeath> GetDeaths(int id)
        {
            try
            {
                string timeElapsed;
                var results = Query(s => s.Query<EncounterDeath, Player, Player, Encounter, Ability, EncounterDeath>
                    (SQL.Encounter.Overview.Deaths, (ed, p1, p2, e, a) =>
                    {
                        if (p1 != null)
                        {
                            ed.SourcePlayer = p1;
                        }
                        if (p2 != null)
                        {
                            ed.TargetPlayer = p2;
                        }
                        ed.Encounter = e;
                        ed.Ability = a;
                        return ed;
                    }, new { @id = id }), out timeElapsed);
                return results.ToList();
            }
            catch (Exception ex)
            {
                _logger.Debug(string.Format("An error occurred while getting deaths: {0}", ex.Message));
            }
            return new List<EncounterDeath>();
        }

        public List<PlayerIdDeathCount> CountDeathsPerPlayer(int id)
        {
            string timeElapsed;
            return
                Query(q => q.Query<PlayerIdDeathCount>(MySQL.Encounter.CountDeathsPerPlayer, new { @encounterId = id }),
                    out timeElapsed).ToList();
        }

        public List<DamageDone> GetDamageForEncounter(int id, List<int> playerIds)
        {
            string timeElapsed;
            return Query(s => s.Query<DamageDone, Ability, DamageDone>
                (MySQL.Encounter.GetMultiplePlayerDamageForEncounter, (dd, a) =>
                {
                    dd.Ability = a;
                    return dd;
                }, new { id, playerIds }), out timeElapsed).ToList();
        }

        public List<HealingDone> GetHealingForEncounter(int id, List<int> playerIds)
        {
            string timeElapsed;
            return Query(s => s.Query<HealingDone, Ability, HealingDone>
                (MySQL.Encounter.GetMultiplePlayerHealingForEncounter, (d, a) =>
                {
                    d.Ability = a;
                    return d;
                }, new { id, playerIds }), out timeElapsed).ToList();
        }

        public List<ShieldingDone> GetShieldingForEncounter(int id, List<int> playerIds)
        {
            string timeElapsed;
            return Query(s => s.Query<ShieldingDone, Ability, ShieldingDone>
                (MySQL.Encounter.GetMultiplePlayerShieldingForEncounter, (d, a) =>
                {
                    d.Ability = a;
                    return d;
                }, new { id, playerIds }), out timeElapsed).ToList();
        }

        /// <summary>
        /// Gets Flaring Power and Command to Attack timers
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<CharacterBuffAction> GetMainRaidBuffs(int id)
        {
            string timeElapsed;

            var flaringList = Query(q => q.Query<CharacterBuffAction>
                (SQL.Encounter.Overview.MainRaidBuffTimers, new { @id = id, @buffName = "Flaring Power", @outputName = "FP" }), out timeElapsed).ToList();
            var commandToAttackList = Query(q => q.Query<CharacterBuffAction>
                (SQL.Encounter.Overview.MainRaidBuffTimers, new { @id = id, @buffName = "Command to Attack", @outputName = "CtA" }), out timeElapsed).ToList();

            var returnList = flaringList.Select(flaring => new CharacterBuffAction()
            {
                BuffName = "FP",
                SecondBuffWentUp = flaring.SecondBuffWentUp,
                SecondBuffWentDown = flaring.SecondBuffWentDown
            }).ToList();
            returnList.AddRange(commandToAttackList.Select(cta => new CharacterBuffAction()
            {
                BuffName = "CtA",
                SecondBuffWentUp = cta.SecondBuffWentUp,
                SecondBuffWentDown = cta.SecondBuffWentDown
            }));
            //returnList.AddRange(flaringList);
            //returnList.AddRange(commandToAttackList);

            return returnList;
        }

        public List<CharacterBuffAction> GetCharacterBuffs(int id, string target)
        {
            string timeElapsed;
            return
                Query(
                    q =>
                        q.Query<CharacterBuffAction>(SQL.Encounter.Character.CharacterBuffs,
                            new { @id = id, @targetId = target }), out timeElapsed).ToList();
        }

        public List<CharacterDebuffAction> GetCharacterDebuffs(int id, string target)
        {
            string timeElapsed;
            return
                Query(
                    q =>
                        q.Query<CharacterDebuffAction>(SQL.Encounter.Character.CharacterDebuffs,
                            new { @id = id, @targetId = target }), out timeElapsed).ToList();
        }


        #region Overview
        //TODO: COMPRESS THIS SECTION. OVERVIEWPLAYERSOMETHINGDONE RECORDS SHOULD BE IN ONE METHOD, WHETHER IT'S DPS, HPS OR APS, INCOMING OR OUTGOING, ETC
        public List<OverviewPlayerSomethingDone> GetOverviewPlayerDamageDone(int id)
        {
            string timeElapsed;
            return Query(q => q.Query<OverviewPlayerSomethingDone>(SQL.Encounter.Overview.Player.Damage.Done.Totals, new { @encounterId = id }), out timeElapsed).ToList();
        }

        public List<OverviewCharacterSomethingDoneGraph> GetOverviewPlayerDamageDoneGraph(int id)
        {
            string timeElapsed;
            return Query(q => q.Query<OverviewCharacterSomethingDoneGraph>(SQL.Encounter.Overview.Player.Damage.Done.PerSecond, new { @encounterId = id }), out timeElapsed).ToList();
        }

        public List<OverviewNpcSomethingDone> GetOverviewNpcDamageDone(int id)
        {
            string timeElapsed;
            return Query(q => q.Query<OverviewNpcSomethingDone>(SQL.Encounter.Overview.Npc.Damage.Done.Totals, new { @encounterId = id }), out timeElapsed).ToList();
        }

        public List<OverviewCharacterSomethingDoneGraph> GetOverviewNpcDamageDoneGraph(int id)
        {
            string timeElapsed;
            return Query(q => q.Query<OverviewCharacterSomethingDoneGraph>(SQL.Encounter.Overview.Npc.Damage.Done.PerSecond, new { @encounterId = id }), out timeElapsed).ToList();
        }

        public List<OverviewPlayerSomethingDone> GetOverviewPlayerHealingDone(int id)
        {
            string timeElapsed;
            return Query(q => q.Query<OverviewPlayerSomethingDone>(SQL.Encounter.Overview.Player.Healing.Done.Totals, new { @encounterId = id }), out timeElapsed).ToList();
        }

        public List<OverviewCharacterSomethingDoneGraph> GetOverviewPlayerHealingDoneGraph(int id)
        {
            string timeElapsed;
            return Query(q => q.Query<OverviewCharacterSomethingDoneGraph>(SQL.Encounter.Overview.Player.Healing.Done.PerSecond, new { @encounterId = id }), out timeElapsed).ToList();
        }

        public List<OverviewNpcSomethingDone> GetOverviewNpcHealingDone(int id)
        {
            string timeElapsed;
            return Query(q => q.Query<OverviewNpcSomethingDone>(SQL.Encounter.Overview.Npc.Healing.Done.Totals, new { @encounterId = id }), out timeElapsed).ToList();
        }

        public List<OverviewCharacterSomethingDoneGraph> GetOverviewNpcHealingDoneGraph(int id)
        {
            string timeElapsed;
            return Query(q => q.Query<OverviewCharacterSomethingDoneGraph>(SQL.Encounter.Overview.Npc.Healing.Done.PerSecond, new { @encounterId = id }), out timeElapsed).ToList();
        }

        public List<OverviewPlayerSomethingDone> GetOverviewPlayerShieldingDone(int id)
        {
            string timeElapsed;
            return Query(q => q.Query<OverviewPlayerSomethingDone>(SQL.Encounter.Overview.Player.Shielding.Done.Totals, new { @encounterId = id }), out timeElapsed).ToList();
        }

        public List<OverviewCharacterSomethingDoneGraph> GetOverviewPlayerShieldingDoneGraph(int id)
        {
            string timeElapsed;
            return Query(q => q.Query<OverviewCharacterSomethingDoneGraph>(SQL.Encounter.Overview.Player.Shielding.Done.PerSecond, new { @encounterId = id }), out timeElapsed).ToList();
        }

        public List<OverviewNpcSomethingDone> GetOverviewNpcShieldingDone(int id)
        {
            string timeElapsed;
            return Query(q => q.Query<OverviewNpcSomethingDone>(SQL.Encounter.Overview.Npc.Shielding.Done.Totals, new { @encounterId = id }), out timeElapsed).ToList();
        }

        public List<OverviewCharacterSomethingDoneGraph> GetOverviewNpcShieldingDoneGraph(int id)
        {
            string timeElapsed;
            return Query(q => q.Query<OverviewCharacterSomethingDoneGraph>(SQL.Encounter.Overview.Npc.Shielding.Done.PerSecond, new { @encounterId = id }), out timeElapsed).ToList();
        }

        public List<OverviewPlayerSomethingTaken> GetOverviewPlayerDamageTaken(int id)
        {
            string timeElapsed;
            return Query(q => q.Query<OverviewPlayerSomethingTaken>(SQL.Encounter.Overview.Player.Damage.Taken.Totals, new { @encounterId = id }), out timeElapsed).ToList();
        }

        public List<OverviewCharacterSomethingTakenGraph> GetOverviewPlayerDamageTakenGraph(int id)
        {
            string timeElapsed;
            return Query(q => q.Query<OverviewCharacterSomethingTakenGraph>(SQL.Encounter.Overview.Player.Damage.Taken.PerSecond, new { @encounterId = id }), out timeElapsed).ToList();
        }

        public List<OverviewNpcSomethingTaken> GetOverviewNpcDamageTaken(int id)
        {
            string timeElapsed;
            return Query(q => q.Query<OverviewNpcSomethingTaken>(SQL.Encounter.Overview.Npc.Damage.Taken.Totals, new { @encounterId = id }), out timeElapsed).ToList();
        }

        public List<OverviewCharacterSomethingTakenGraph> GetOverviewNpcDamageTakenGraph(int id)
        {
            string timeElapsed;
            return Query(q => q.Query<OverviewCharacterSomethingTakenGraph>(SQL.Encounter.Overview.Npc.Damage.Taken.PerSecond, new { @encounterId = id }), out timeElapsed).ToList();
        }

        public List<string> GetOverviewNpcDamageTakenTop25Abilities(int id, string npcId, string filter)
        {
            string timeElapsed;
            string query = MySQL.Encounter.Character.Npc.Damage.Taken.Top25AbilitiesAllSources;
            if (filter == "npcs" || filter == "othernpcs") { query = MySQL.Encounter.Character.Npc.Damage.Taken.Top25AbilitiesNpcs; }
            if (filter == "players" || filter == "otherplayers") { query = MySQL.Encounter.Character.Npc.Damage.Taken.Top25AbilitiesPlayers; }
            return Query(q => q.Query<string>(query, new { @Id = id, npcId }), out timeElapsed).ToList();
        }

        public List<OverviewPlayerSomethingTaken> GetOverviewPlayerHealingTaken(int id)
        {
            string timeElapsed;
            return Query(q => q.Query<OverviewPlayerSomethingTaken>(SQL.Encounter.Overview.Player.Healing.Taken.Totals, new { @encounterId = id }), out timeElapsed).ToList();
        }

        public List<OverviewCharacterSomethingTakenGraph> GetOverviewPlayerHealingTakenGraph(int id)
        {
            string timeElapsed;
            return Query(q => q.Query<OverviewCharacterSomethingTakenGraph>(SQL.Encounter.Overview.Player.Healing.Taken.PerSecond, new { @encounterId = id }), out timeElapsed).ToList();
        }

        public List<OverviewNpcSomethingTaken> GetOverviewNpcHealingTaken(int id)
        {
            string timeElapsed;
            return Query(q => q.Query<OverviewNpcSomethingTaken>(SQL.Encounter.Overview.Npc.Healing.Taken.Totals, new { @encounterId = id }), out timeElapsed).ToList();
        }

        public List<OverviewCharacterSomethingTakenGraph> GetOverviewNpcHealingTakenGraph(int id)
        {
            string timeElapsed;
            return Query(q => q.Query<OverviewCharacterSomethingTakenGraph>(SQL.Encounter.Overview.Npc.Healing.Taken.PerSecond, new { @encounterId = id }), out timeElapsed).ToList();
        }

        public List<OverviewPlayerSomethingTaken> GetOverviewPlayerShieldingTaken(int id)
        {
            string timeElapsed;
            return Query(q => q.Query<OverviewPlayerSomethingTaken>(SQL.Encounter.Overview.Player.Shielding.Taken.Totals, new { @encounterId = id }), out timeElapsed).ToList();
        }

        public List<OverviewCharacterSomethingTakenGraph> GetOverviewPlayerShieldingTakenGraph(int id)
        {
            string timeElapsed;
            return Query(q => q.Query<OverviewCharacterSomethingTakenGraph>(SQL.Encounter.Overview.Player.Shielding.Taken.PerSecond, new { @encounterId = id }), out timeElapsed).ToList();
        }

        public List<OverviewNpcSomethingTaken> GetOverviewNpcShieldingTaken(int id)
        {
            string timeElapsed;
            return Query(q => q.Query<OverviewNpcSomethingTaken>(SQL.Encounter.Overview.Npc.Shielding.Taken.Totals, new { @encounterId = id }), out timeElapsed).ToList();
        }

        public List<OverviewCharacterSomethingTakenGraph> GetOverviewNpcShieldingTakenGraph(int id)
        {
            string timeElapsed;
            return Query(q => q.Query<OverviewCharacterSomethingTakenGraph>(SQL.Encounter.Overview.Npc.Shielding.Taken.PerSecond, new { @encounterId = id }), out timeElapsed).ToList();
        }
        #region Detail

        public List<DetailDamageByPlane> GetDetailDamageToNpcsByPlane(int encounterId)
        {
            string timeElapsed;
            return
                Query(q => q.Query<DetailDamageByPlane>(MySQL.Encounter.Detail.DamageToNpcsByPlane, new { encounterId }),
                    out timeElapsed).ToList();
        }

        public List<DetailDamageByPlane> GetDetailDamageToPlayersByPlane(int encounterId)
        {
            string timeElapsed;
            return
                Query(q => q.Query<DetailDamageByPlane>(MySQL.Encounter.Detail.DamageToPlayersByPlane, new { encounterId }),
                    out timeElapsed).ToList();
        }

        public List<DetailDamageByClass> GetDetailDamageToNpcsByClass(int encounterId)
        {
            string timeElapsed;
            return
                Query(q => q.Query<DetailDamageByClass>(MySQL.Encounter.Detail.DamageToNpcsByClass, new { encounterId }),
                    out timeElapsed).ToList();
        }

        #endregion
        public List<EncounterPlayerRole> GetEncounterRoleRecords(int id)
        {
            string timeElapsed;
            return Query(q => q.Query<EncounterPlayerRole>
                (MySQL.Encounter.GetEncounterRoleRecords, new { id }), out timeElapsed).ToList();
        }

        #endregion
        public List<PlayerRole> GetPlayerRoles(int id)
        {
            string timeElapsed;
            // Previous method:
            // This worked, but in the event of players being able to change specs quickly, reported too many players for an encounter.
            //return Query(q => q.Query<PlayerRole>(SQL.Encounter.PlayerRoles.All, new { @id = id }), out timeElapsed).ToList();
            var roles = new List<PlayerRole>();

            var shieldRoles =
                Query(q => q.Query<PlayerRole>(SQL.Encounter.PlayerRoles.Shield, new { id }), out timeElapsed)
                    .ToList();
            var healRoles =
                Query(q => q.Query<PlayerRole>(SQL.Encounter.PlayerRoles.Heal, new { id }), out timeElapsed)
                    .ToList();
            var damageRoles =
                Query(q => q.Query<PlayerRole>(SQL.Encounter.PlayerRoles.Damage, new { id }), out timeElapsed)
                    .ToList();

            roles.AddRange(shieldRoles);

            foreach (var healRole in healRoles.Where(healRole => roles.Any(r => r.Id == healRole.Id) == false))
            {
                roles.Add(healRole);
            }

            //foreach (var dmgRole in damageRoles.Where(dmgRole => roles.Any(r => r.Id == dmgRole.Id) == false))
            //{
            //    roles.Add(dmgRole);
            //}
            if (damageRoles.Any())
            {
                for (int i = 0; i < damageRoles.Count; i++)
                {
                    var dmgRole = damageRoles[i];
                    if (!roles.Any(r => r.Id == dmgRole.Id))
                    {
                        roles.Add(dmgRole);
                    }
                }
            }

            return roles;
        }

        public ReturnValue AddPlayerEncounterRoles(List<EncounterPlayerRole> playerRoles)
        {
            var returnValue = new ReturnValue();
            try
            {
                DapperDb dapperDb = DapperDb.Init(OpenConnection(), 3, false);

                foreach (var playerRole in playerRoles)
                {
                    dapperDb.EncounterPlayerRoleTable.Insert(
                        new //EncounterPlayerRole()
                        {
                            EncounterId = playerRole.EncounterId,
                            Class = playerRole.Class,
                            PlayerId = playerRole.PlayerId,
                            Role = playerRole.Role,
                            Name = playerRole.Name
                        });
                }

                returnValue.Success = true;
                return returnValue;
            }
            catch (Exception ex)
            {
                returnValue.Message = ex.Message;
                return returnValue;
            }
        }

        public bool EncounterNpcRecordsExist(int id)
        {
            string timeElapsed;
            return
                Query(q => q.Query<long>(MySQL.EncounterNpc.RecordsExistForEncounter, new { id }), out timeElapsed)
                    .SingleOrDefault() == 1;
        }

        public List<EncounterNpc> GetEncounterNpcsFromEncounterInfo(int id)
        {
            string timeElapsed;
            return
                Query(q => q.Query<EncounterNpc>(MySQL.EncounterNpc.CalculateForEncounter, new { id }),
                    out timeElapsed).ToList();
        }


        public ReturnValue AddEncounterNpcs(List<EncounterNpc> encounterNpcs)
        {
            var returnValue = new ReturnValue();
            try
            {
                DapperDb dapperDb = DapperDb.Init(OpenConnection(), 3, false);

                foreach (var encounterNpc in encounterNpcs)
                {
                    dapperDb.EncounterNpcTable.Insert(
                        new //Models.EncounterNpc()
                        {
                            EncounterId = encounterNpc.EncounterId,
                            NpcId = encounterNpc.NpcId,
                            NpcName = encounterNpc.NpcName
                        });
                }

                returnValue.Success = true;
                return returnValue;
            }
            catch (Exception ex)
            {
                returnValue.Message = ex.Message;
                return returnValue;
            }
        }

        public string GetNpcName(int encounterId, string npcId)
        {
            string timeElapsed;
            return
                Query(q => q.Query<string>(MySQL.EncounterNpc.GetNpcNameForId, new { encounterId, npcId }),
                    out timeElapsed).SingleOrDefault();
        }

        public List<Encounter> GetEncountersMissingNpcRecords(int encounterLimit = 20)
        {
            string timeElapsed;
            return
                Query(q => q.Query<Encounter>(MySQL.Encounter.GetEncountersWithNoNpcRecords, new { @limit = encounterLimit }, commandTimeout: 600), // 10 minutes
                    out timeElapsed).ToList();
        }

        public List<int> GetEncountersMissingPlayerRecords(int encounterLimit = 20)
        {
            string timeElapsed;
            return
                Query(q => q.Query<int>(MySQL.Encounter.GetEncounterIdsWithNoPlayerRecords, new { @limit = encounterLimit }, commandTimeout: 600), // 10 minutes
                    out timeElapsed).ToList();
        }

        public int EncountersMissingPlayerStatistics()
        {
            string timeElapsed;
            var result = Query(q => q.Query<long>(MySQL.Encounter.CountEncountersWithNoPlayerStatistics), out timeElapsed).SingleOrDefault();
            return Convert.ToInt32(result);
        }

        public int EncountersMissingBiggestHitStatistics()
        {
            string timeElapsed;
            var result = Query(q => q.Query<long>(MySQL.Encounter.CountEncounterIdsWithNoPlayerTopHits), out timeElapsed).SingleOrDefault();
            return Convert.ToInt32(result);
        }

        public RecordCounts CountBasicRecordsForEncounter(int encounterId)
        {
            string timeElapsed;
            return
                Query(q => q.Query<RecordCounts>(MySQL.Encounter.CountBasicRecordsForEncounter, new { encounterId }),
                    out timeElapsed).SingleOrDefault();
        }

        public List<int> GetEncountersIdsMissingPlayerStatistics(int encounterLimit = 20)
        {
            string timeElapsed;
            return
                Query(q => q.Query<int>(MySQL.Encounter.GetEncounterIdsWithNoPlayerStatistics, new { @limit = encounterLimit }),
                    out timeElapsed).ToList();
        }

        public List<Encounter> GetEncountersMissingPlayerStatistics(int encounterLimit = 20)
        {
            string timeElapsed;
            return
                Query(q => q.Query<Encounter>(MySQL.Encounter.GetEncountersWithNoPlayerStatisticsButValidSession, new { @limit = encounterLimit }),
                    out timeElapsed).ToList();
        }

        public List<Encounter> GetEncountersMissingBiggestHitStatistics(int encounterLimit = 20)
        {
            string timeElapsed;
            return
                Query(q => q.Query<Encounter>(MySQL.Encounter.GetEncountersWithNoPlayerTopHits, new { @limit = encounterLimit }, commandTimeout: 600), //10 minutes
                    out timeElapsed).ToList();
        }

        public async Task<List<Encounter>> GetEncountersMissingBurstStatistics(int encounterLimit = 20)
        {
            return (await QueryAsync(q => q.QueryAsync<Encounter>(MySQL.Encounter.GetEncountersWithNoPlayerBurstStatisticsButValidSession, new { @limit = encounterLimit }))).ToList();
        }

        public List<Encounter> GetEncountersMissingSingleTargetDpsStatistics(int encounterLimit = 100)
        {
            string timeElapsed;
            return
                Query(q => q.Query<Encounter>(MySQL.Encounter.GetEncountersWithNoSingleTargetDps, new { @limit = encounterLimit }, commandTimeout: 600), //10 minutes
                    out timeElapsed).ToList();
        }

        public List<PlayerIdSingleTargetDamage> GetPlayerSingleTargetDamageDone(int id, string targetName)
        {
            string timeElapsed;
            return
                Query(q => q.Query<PlayerIdSingleTargetDamage>(SQL.Encounter.Overview.Player.Damage.Done.TotalSingleTarget,
                    new { @encounterId = id, targetName }, commandTimeout: 300), //5 minutes
                    out timeElapsed).ToList();
        }

        private ReturnValue SingleSaveEncounterPlayerStatistics(List<EncounterPlayerStatistics> list)
        {
            var returnValue = new ReturnValue();
            try
            {
                DapperDb dapperDb = DapperDb.Init(OpenConnection(), 3, false);

                foreach (var stat in list)
                {
                    returnValue.Message = dapperDb.EncounterPlayerStatisticsTable.Insert(
                        new //EncounterPlayerStatistics
                        {
                            EncounterId = stat.EncounterId,
                            APS = stat.APS,
                            PlayerId = stat.PlayerId,
                            DPS = stat.DPS,
                            Deaths = stat.Deaths,
                            HPS = stat.HPS,
                            TopApsAbilityId = stat.TopApsAbilityId == 0 ? null : stat.TopApsAbilityId,
                            TopDpsAbilityId = stat.TopDpsAbilityId == 0 ? null : stat.TopDpsAbilityId,
                            TopHpsAbilityId = stat.TopHpsAbilityId == 0 ? null : stat.TopHpsAbilityId,
                            TopApsAbilityValue = stat.TopApsAbilityValue,
                            TopDpsAbilityValue = stat.TopDpsAbilityValue,
                            TopHpsAbilityValue = stat.TopHpsAbilityValue
                        }).ToString();
                }

                returnValue.Success = true;
                return returnValue;
            }
            catch (Exception ex)
            {
                returnValue.Message = ex.Message;
                return returnValue;
            }
        }

        public ReturnValue SaveEncounterPlayerStatistics(List<EncounterPlayerStatistics> list)
        {
            var returnValue = new ReturnValue();
            try
            {
                // Used for looping when something goes wrong during an INSERT
                var failCount = 0;

                while (true)
                {
                    if (failCount == 3)
                    {
                        returnValue.Message = "Saving statistics failed 3 times!";
                        return returnValue;
                    }

                    var result = SingleSaveEncounterPlayerStatistics(list);
                    if (!result.Success)
                    {
                        _logger.Debug(string.Format("There was an error while saving statistics for this encounter! {0}", result.Message));
                        _logger.Debug("Retrying stats save in 5 seconds...");
                        Thread.Sleep(5000); // Sleep 5 seconds and retry

                        failCount++;
                    }
                    else
                    {
                        var encounterId = Convert.ToInt32(result.Message);
                        if (failCount > 0)
                        {
                            _logger.Debug(string.Format("Encountered an earlier error adding stats for encounter {0}, but it has been successfully added now.", encounterId));
                        }
                        failCount = 0;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                returnValue.Message = ex.Message;
                return returnValue;
            }

            return returnValue;
        }

        public ReturnValue UpdateEncounterSingleTargetDpsStatistics(List<EncounterPlayerStatistics> list)
        {
            var returnValue = new ReturnValue();
            try
            {
                DapperDb dapperDb = DapperDb.Init(OpenConnection(), 3, false);

                var encId = 0;

                foreach (var stat in list)
                {
                    if (encId == 0)
                    {
                        encId = stat.EncounterId;
                    }
                    //UPDATE EncounterPlayerStatistics SET SingleTargetDPS = @dps WHERE EncounterId = @encounterId AND PlayerId = @playerId
                    dapperDb.Execute(MySQL.EncounterPlayerStatistics.UpdateSingleTargetDps,
                        new { @dps = stat.SingleTargetDps, @encounterId = stat.EncounterId, @playerId = stat.PlayerId });
                }

                // Now that we've updated all of the records we were given, set the rest to -1 so they don't get parsed again!
                if (encId != 0)
                {
                    dapperDb.Execute(MySQL.EncounterPlayerStatistics.NullifySingleTargetDps,
                        new { @encounterId = encId });
                }

                returnValue.Success = true;
                return returnValue;
            }
            catch (Exception ex)
            {
                _logger.Debug(string.Format("The following error occurred while updating stats: {0}", ex.Message));
                returnValue.Message = ex.Message;
                return returnValue;
            }
        }

        /// <summary>
        /// Get the events leading up to a player's death
        /// </summary>
        /// <param name="encounterId"></param>
        /// <param name="targetPlayerId"></param>
        /// <param name="minSeconds"></param>
        /// <param name="maxSeconds"></param>
        /// <returns></returns>
        public List<EncounterDeathEvent> GetEventsBeforeDeath(int encounterId, int targetPlayerId, int minSeconds, int maxSeconds)
        {
            string timeElapsed;
            return Query(q => q.Query<EncounterDeathEvent>(MySQL.Encounter.Character.Player.GetEventsBeforeDeath,
                new { encounterId, targetPlayerId, minSeconds, maxSeconds }), out timeElapsed).ToList();
        }


        public void RemoveEncounter(string email, int encounterId)
        {
            try
            {
                string timeElapsed;

                _logger.Debug(string.Format("Deleting records for encounter ID {0}", encounterId));
                string deleteQuery = @"DELETE FROM EncounterOverview WHERE EncounterId = @id;
                                           DELETE FROM EncounterBuffEvent WHERE EncounterId = @id;
                                           DELETE FROM EncounterBuffUptime WHERE EncounterId = @id;
                                           DELETE FROM EncounterBuffAction WHERE EncounterId = @id;
                                           DELETE FROM EncounterDebuffAction WHERE EncounterId = @id;
                                           DELETE FROM EncounterNpcCast WHERE EncounterId = @id;
                                           DELETE FROM EncounterDeath WHERE EncounterId = @id;
                                           DELETE FROM DamageDone WHERE EncounterId = @id;
                                           DELETE FROM HealingDone WHERE EncounterId = @id;
                                           DELETE FROM ShieldingDone WHERE EncounterId = @id;
                                           DELETE FROM SessionEncounter WHERE EncounterId = @id;
                                           DELETE FROM EncounterPlayerRole WHERE EncounterId = @id;
                                           DELETE FROM EncounterNpc WHERE EncounterId = @id;
                                           DELETE FROM EncounterPlayerStatistics WHERE EncounterId = @id;
                                           DELETE FROM SessionEncounter WHERE EncounterId = @id;
                                           DELETE FROM Encounter WHERE Id = @id;";
                var result = Execute(s => s.Execute(deleteQuery, new { @id = encounterId }, commandTimeout: 300), out timeElapsed);
                _logger.Debug(string.Format("Encounter ID {0} removed in {1}.", encounterId, timeElapsed));
            }
            catch (Exception ex)
            {
                _logger.Debug(string.Format("The following error occurred while trying to remove encounter {0}: {1}", encounterId, ex.Message));
            }
        }

        private void DeleteRecordsForEncounterInsideTransaction(string tableName, List<int> encounterIds,
            DbTransaction transaction, DbConnection connection, bool console = false)
        {
            Stopwatch individualTimer = new Stopwatch();
            individualTimer.Start();
            var result = connection.Execute(string.Format("DELETE FROM {0} WHERE EncounterId IN @ids", tableName), new { @ids = encounterIds }, transaction, 600); // 10 minutes
            individualTimer.Stop();
            if (console)
            {
                Console.WriteLine("{0} | {1} in {2}", tableName, result, individualTimer.Elapsed.ToString(@"mm\:ss"));
                //Console.WriteLine("{0} records removed from {1} in {2}", result, tableName, individualTimer.Elapsed);
            }
            else
            {
                _logger.Debug(string.Format("{0} records removed from {1} in {2}", result, tableName, individualTimer.Elapsed));
            }
        }

        private ReturnValue PerformRemoveRecordsForMarkedEncounters_Console(List<int> encounterIds, string email, string overrideConnectionString = null)
        {
            var returnValue = new ReturnValue();
            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                Stopwatch individualTimer = new Stopwatch();

                DbConnection conn;
                if (string.IsNullOrEmpty(overrideConnectionString))
                {
                    conn = OpenConnection();
                }
                else
                {
                    conn = new MySqlConnection(overrideConnectionString);
                    conn.Open();
                }

                //using (var conn = OpenConnection())
                //using (conn)
                //{
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        //Console.WriteLine("=== START ===");
                        individualTimer.Reset();
                        individualTimer.Start();
                        DeleteRecordsForEncounterInsideTransaction("EncounterOverview", encounterIds, trans, conn, true);
                        DeleteRecordsForEncounterInsideTransaction("EncounterBuffEvent", encounterIds, trans, conn, true);
                        DeleteRecordsForEncounterInsideTransaction("EncounterBuffUptime", encounterIds, trans, conn, true);
                        DeleteRecordsForEncounterInsideTransaction("EncounterBuffAction", encounterIds, trans, conn, true);
                        DeleteRecordsForEncounterInsideTransaction("EncounterDebuffAction", encounterIds, trans, conn, true);
                        DeleteRecordsForEncounterInsideTransaction("EncounterNpcCast", encounterIds, trans, conn, true);
                        DeleteRecordsForEncounterInsideTransaction("EncounterDeath", encounterIds, trans, conn, true);
                        DeleteRecordsForEncounterInsideTransaction("DamageDone", encounterIds, trans, conn, true);
                        DeleteRecordsForEncounterInsideTransaction("HealingDone", encounterIds, trans, conn, true);
                        DeleteRecordsForEncounterInsideTransaction("ShieldingDone", encounterIds, trans, conn, true);
                        DeleteRecordsForEncounterInsideTransaction("EncounterPlayerRole", encounterIds, trans, conn, true);
                        DeleteRecordsForEncounterInsideTransaction("EncounterNpc", encounterIds, trans, conn, true);
                        DeleteRecordsForEncounterInsideTransaction("EncounterPlayerStatistics", encounterIds, trans, conn, true);

                        conn.Execute("UPDATE Encounter SET Removed = 1 WHERE Id IN @ids", new { @ids = encounterIds }, trans, 60);

                        trans.Commit();
                        individualTimer.Stop();
                        //Console.WriteLine("Committed in {0}", individualTimer.Elapsed.ToString(@"mm\:ss"));
                        Console.WriteLine("=============");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: {0}", ex.Message);
                        Console.WriteLine("Rolling back transaction...");
                        trans.Rollback();
                    }
                }
                //}
                conn.Close();

                sw.Stop();

                Console.WriteLine("Records for {0} encounters removed in {1}", encounterIds.Count,
                    sw.Elapsed);
                returnValue.Success = true;
                return returnValue;
            }
            catch (Exception ex)
            {
                Console.WriteLine("The following error occurred while mass deleting encounters: {0}",
                    ex.Message);
                returnValue.Message = ex.Message;
                return returnValue;
            }
        }

        private async Task<ReturnValue> PerformRemoveRecordsForMarkedEncounters_ConsoleAsync(List<int> encounterIds, string email)
        {
            var returnValue = new ReturnValue();
            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                Stopwatch individualTimer = new Stopwatch();

                using (var conn = await OpenConnectionAsync())
                {
                    try
                    {
                        var tables = new List<string>()
                        {
                            "EncounterOverview",
                            "EncounterBuffEvent",
                            "EncounterBuffUptime"
                        };

                        foreach (var tableName in tables)
                        {
                            individualTimer.Restart();

                            Console.WriteLine("Deleting from {0}", tableName);

                            var result = await conn.ExecuteAsync(string.Format("DELETE FROM {0} WHERE EncounterId IN @ids", tableName), new { ids = encounterIds });

                            individualTimer.Stop();
                            if (result > 0)
                            {
                                Console.WriteLine("Deleted {1} in {0}", individualTimer.Elapsed, result);
                            }
                        }


                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: {0}", ex.Message);
                    }

                    //using (var trans = conn.BeginTransaction())
                    //{
                    //    try
                    //    {
                    //        individualTimer.Reset();
                    //        individualTimer.Start();
                    //        DeleteRecordsForEncounterInsideTransaction("EncounterOverview", encounterIds, trans, conn, true);
                    //        DeleteRecordsForEncounterInsideTransaction("EncounterBuffEvent", encounterIds, trans, conn, true);
                    //        DeleteRecordsForEncounterInsideTransaction("EncounterBuffUptime", encounterIds, trans, conn, true);
                    //        DeleteRecordsForEncounterInsideTransaction("EncounterBuffAction", encounterIds, trans, conn, true);
                    //        DeleteRecordsForEncounterInsideTransaction("EncounterDebuffAction", encounterIds, trans, conn, true);
                    //        DeleteRecordsForEncounterInsideTransaction("EncounterNpcCast", encounterIds, trans, conn, true);
                    //        DeleteRecordsForEncounterInsideTransaction("EncounterDeath", encounterIds, trans, conn, true);
                    //        DeleteRecordsForEncounterInsideTransaction("DamageDone", encounterIds, trans, conn, true);
                    //        DeleteRecordsForEncounterInsideTransaction("HealingDone", encounterIds, trans, conn, true);
                    //        DeleteRecordsForEncounterInsideTransaction("ShieldingDone", encounterIds, trans, conn, true);
                    //        DeleteRecordsForEncounterInsideTransaction("EncounterPlayerRole", encounterIds, trans, conn, true);
                    //        DeleteRecordsForEncounterInsideTransaction("EncounterNpc", encounterIds, trans, conn, true);
                    //        DeleteRecordsForEncounterInsideTransaction("EncounterPlayerStatistics", encounterIds, trans, conn, true);

                    //        conn.Execute("UPDATE Encounter SET Removed = 1 WHERE Id IN @ids", new { @ids = encounterIds }, trans, 60);

                    //        trans.Commit();
                    //        individualTimer.Stop();
                    //        //Console.WriteLine("Committed in {0}", individualTimer.Elapsed.ToString(@"mm\:ss"));
                    //        Console.WriteLine("=============");
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        Console.WriteLine("Error: {0}", ex.Message);
                    //        Console.WriteLine("Rolling back transaction...");
                    //        trans.Rollback();
                    //    }
                    //}
                }

                sw.Stop();

                Console.WriteLine("Records for {0} encounters removed in {1}", encounterIds.Count,
                    sw.Elapsed);
                returnValue.Success = true;
                return returnValue;
            }
            catch (Exception ex)
            {
                Console.WriteLine("The following error occurred while mass deleting encounters: {0}",
                    ex.Message);
                returnValue.Message = ex.Message;
                return returnValue;
            }
        }

        private ReturnValue PerformRemoveRecordsForMarkedEncounters(List<int> encounterIds, string email, bool console = false)
        {
            var returnValue = new ReturnValue();
            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                Stopwatch commitTimer = new Stopwatch();

                using (var connection = OpenConnection())
                {
                    if (console)
                    {
                        Console.WriteLine("Beginning transaction to remove records for {0} encounters",
                            encounterIds.Count);
                    }
                    else
                    {
                        _logger.Debug(string.Format("Beginning transaction to remove records for {0} encounters",
                            encounterIds.Count));
                    }

                    var transaction = connection.BeginTransaction();

                    DeleteRecordsForEncounterInsideTransaction("EncounterOverview", encounterIds, transaction, connection, console);
                    DeleteRecordsForEncounterInsideTransaction("EncounterBuffEvent", encounterIds, transaction, connection, console);
                    DeleteRecordsForEncounterInsideTransaction("EncounterBuffUptime", encounterIds, transaction, connection, console);
                    DeleteRecordsForEncounterInsideTransaction("EncounterBuffAction", encounterIds, transaction, connection, console);
                    DeleteRecordsForEncounterInsideTransaction("EncounterDebuffAction", encounterIds, transaction, connection, console);
                    DeleteRecordsForEncounterInsideTransaction("EncounterNpcCast", encounterIds, transaction, connection, console);
                    DeleteRecordsForEncounterInsideTransaction("EncounterDeath", encounterIds, transaction, connection, console);
                    DeleteRecordsForEncounterInsideTransaction("DamageDone", encounterIds, transaction, connection, console);
                    DeleteRecordsForEncounterInsideTransaction("HealingDone", encounterIds, transaction, connection, console);
                    DeleteRecordsForEncounterInsideTransaction("ShieldingDone", encounterIds, transaction, connection, console);
                    DeleteRecordsForEncounterInsideTransaction("EncounterPlayerRole", encounterIds, transaction, connection, console);
                    DeleteRecordsForEncounterInsideTransaction("EncounterNpc", encounterIds, transaction, connection, console);
                    DeleteRecordsForEncounterInsideTransaction("EncounterPlayerStatistics", encounterIds, transaction, connection, console);

                    commitTimer = new Stopwatch();
                    commitTimer.Start();
                    transaction.Commit();
                    commitTimer.Stop();
                    if (console)
                    {
                        Console.WriteLine("Commit finished in {0}", commitTimer.Elapsed.ToString(@"mm\:ss"));
                    }

                    var transaction2 = connection.BeginTransaction();
                    connection.Execute("UPDATE Encounter SET Removed = 1 WHERE Id IN @ids", new { @ids = encounterIds }, transaction2);
                    transaction2.Commit();

                    if (console)
                    {
                        Console.WriteLine("==================");
                    }
                }

                sw.Stop();

                if (console)
                {
                    Console.WriteLine("Records for {0} encounters removed in {1}", encounterIds.Count,
                        sw.Elapsed);
                }
                else
                {
                    _logger.Debug(string.Format("Records for {0} encounters removed in {1}", encounterIds.Count,
                        sw.Elapsed));
                }
                returnValue.Success = true;
                return returnValue;
            }
            catch (Exception ex)
            {
                if (console)
                {
                    Console.WriteLine("The following error occurred while mass deleting encounters: {0}",
                        ex.Message);
                }
                else
                {
                    _logger.Debug(string.Format("The following error occurred while mass deleting encounters: {0}",
                        ex.Message));
                }
                returnValue.Message = ex.Message;
                return returnValue;
            }
        }

        /// <summary>
        /// This isn't used anymore - see the async method. 20191126
        /// </summary>
        /// <param name="email"></param>
        /// <param name="console"></param>
        /// <param name="overrideConnectionString"></param>
        /// <returns></returns>
        public ReturnValue RemoveEncountersMarkedForDeletion(string email, bool console = false, string overrideConnectionString = null)
        {
            //_discord.Log($"Stepped into 'RemoveEncountersMarkedForDeletion' ({email})", "EncounterRepository", LogLevel.Debug).Wait();

            var returnValue = new ReturnValue();
            //int maxEncounters = 1;
            int maxEncounters = 1;
            int totalRemoved = 0;
            string timeElapsed;

            // Get marked encounters that haven't already been removed
            // temp disable this one and use the shortest one first
            //var removeList = Query(q => q.Query<Encounter>(MySQL.Encounter.GetMarkedForDeletion), out timeElapsed).ToList();
            var removeList = Query(q => q.Query<Encounter>(MySQL.Encounter.GetMarkedForDeletionShortestFirst), out timeElapsed).ToList();
            //var removeList = Query(q => q.Query<Encounter>(MySQL.Encounter.GetMarkedForDeletionLongestFirst), out timeElapsed).ToList();
            if (!removeList.Any())
            {
                returnValue.Success = true;
                return returnValue;
            }
            if (console)
            {
                Console.WriteLine("Found a total of {0} encounters to 'remove' from the database as they're marked for removal", removeList.Count);
            }
            else
            {
                //_discord.Log($"Second message ", "EncounterRepository", LogLevel.Debug).Wait();
                //_discord.Log($"Found a total of {removeList.Count} encounters to 'remove' from the database as they're marked for removal", "EncounterRepository", LogLevel.Debug).Wait();
                //_logger.Debug(
                //    string.Format(
                //        "Found a total of {0} encounters to 'remove' from the database as they're marked for removal",
                //        removeList.Count));
            }
            try
            {
                while (removeList.Any())
                {
                    var encounterList = removeList.Count > maxEncounters ? removeList.Take(maxEncounters).ToList() : removeList;
                    ReturnValue result = new ReturnValue();
                    if (console)
                    {
                        Console.WriteLine("Removing {0} of {1} encounters.", encounterList.Count, removeList.Count);
                        result =
                            PerformRemoveRecordsForMarkedEncounters_Console(encounterList.Select(e => e.Id).ToList(),
                                email, overrideConnectionString);
                    }
                    else
                    {
                        result = PerformRemoveRecordsForMarkedEncounters(encounterList.Select(e => e.Id).ToList(), email);
                    }

                    if (!result.Success)
                    {
                        break;
                    }
                    removeList.RemoveRange(0, encounterList.Count);
                }
            }
            catch (Exception ex)
            {
                if (console)
                {
                    Console.WriteLine(
                        "An error occurred while removing encounters marked for deletion: {0}",
                        ex.Message);
                }
                else
                {
                    _logger.Debug(string.Format("An error occurred while removing encounters marked for deletion: {0}",
                        ex.Message));
                }
                returnValue.Message = ex.Message;
            }

            return returnValue;
        }

        /// <summary>
        /// This is the new method to remove records from encounters marked for deletion.
        /// </summary>
        /// <remarks>Date implemented: 20191126</remarks>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<ReturnValue> RemoveEncountersMarkedForDeletionAsync(string email)
        {
            //await _discord.Log($"Stepped into 'RemoveEncountersMarkedForDeletionAsync' ({email})", "EncounterRepository", LogLevel.Debug);

            var returnValue = new ReturnValue();
            // The number of encounters to be removed at a time. This value is currently not implemented.
            int maxEncounters = 1;
            int totalRemoved = 0;

            // Get marked encounters that haven't already been removed
            var removeList = (await QueryAsync(q => q.QueryAsync<Encounter>(MySQL.Encounter.GetMarkedForDeletion))).ToList();
            if (!removeList.Any())
            {
                await _discord.Log($"There are no encounters marked for deletion.", "EncounterRepository", LogLevel.Information);
                returnValue.Success = true;
                return returnValue;
            }

            var totalEncounters = removeList.Count;

            await _discord.Log($"Found a total of {totalEncounters} encounters to 'remove' from the database as they're marked for removal.", "EncounterRepository", LogLevel.Debug);


            Stopwatch sw = new Stopwatch();

            try
            {
                while (removeList.Any())
                {
                    var thisIndex = totalRemoved + 1;

                    var encToRemove = removeList[0];
                    var nextToRemove = removeList[1];

                    using (var conn = await OpenConnectionAsync())
                    {
                        using (var trans = conn.BeginTransaction())
                        {
                            try
                            {
                                var recordCounts = (await conn.QueryAsync<EncounterTableRecords>(EncounterRemovalFrom.GetEncounterRecordCounts,
                                    new { id = encToRemove.Id }, trans)).SingleOrDefault();

                                if (recordCounts == null)
                                {
                                    var logMessage =
                                        $"The record count for encounter ID #{encToRemove.Id} came back null. Weird.";
                                    if (thisIndex < totalEncounters)
                                    {
                                        logMessage +=
                                            $"\n\nNext encounter to remove: Encounter ID #{nextToRemove.Id} ({thisIndex + 1}/{totalEncounters}). Encounter length: {encToRemove.Duration}";
                                    }

                                    //await _discord.Log(logMessage, "EncounterRepository", LogLevel.Warning);
                                    Debug.WriteLine(logMessage);
                                }
                                else if (recordCounts.HasRecords == false)
                                {
                                    // Wait for 500ms so that we're not rate limited
                                    await Task.Delay(550);
                                    await conn.ExecuteAsync(EncounterRemovalFrom.SetEncounterRemoved, new { id = encToRemove.Id }, trans);
                                    var logMessage =
                                        $"Encounter ID #{encToRemove.Id} ({thisIndex}/{totalEncounters}) has no records that need to be removed.";
                                    if (thisIndex < totalEncounters)
                                    {
                                        logMessage +=
                                            $"\n\nNext encounter to remove: Encounter ID #{nextToRemove.Id} ({thisIndex + 1}/{totalEncounters}). Encounter length: {encToRemove.Duration}";
                                    }

                                    //await _discord.Log(logMessage, "EncounterRepository", LogLevel.Debug);
                                    Debug.WriteLine(logMessage);
                                    trans.Commit();
                                }
                                else
                                {
                                    var logMessage = $"Removal of encounter ID #{encToRemove.Id} ({thisIndex + 1}/{totalEncounters}):";

                                    sw.Reset();
                                    sw.Start();

                                    logMessage += $"\n{await PerformDeleteForEncounter("EncounterOverview", encToRemove.Id, conn, trans, recordCounts)}";
                                    logMessage += $"\n{await PerformDeleteForEncounter("EncounterBuffEvent", encToRemove.Id, conn, trans, recordCounts)}";
                                    logMessage += $"\n{await PerformDeleteForEncounter("EncounterBuffUptime", encToRemove.Id, conn, trans, recordCounts)}";
                                    logMessage += $"\n{await PerformDeleteForEncounter("EncounterBuffAction", encToRemove.Id, conn, trans, recordCounts)}";
                                    logMessage += $"\n{await PerformDeleteForEncounter("EncounterDebuffAction", encToRemove.Id, conn, trans, recordCounts)}";
                                    logMessage += $"\n{await PerformDeleteForEncounter("EncounterNpcCast", encToRemove.Id, conn, trans, recordCounts)}";
                                    logMessage += $"\n{await PerformDeleteForEncounter("EncounterDeath", encToRemove.Id, conn, trans, recordCounts)}";
                                    logMessage += $"\n{await PerformDeleteForEncounter("DamageDone", encToRemove.Id, conn, trans, recordCounts)}";
                                    logMessage += $"\n{await PerformDeleteForEncounter("HealingDone", encToRemove.Id, conn, trans, recordCounts)}";
                                    logMessage += $"\n{await PerformDeleteForEncounter("ShieldingDone", encToRemove.Id, conn, trans, recordCounts)}";
                                    logMessage += $"\n{await PerformDeleteForEncounter("EncounterPlayerRole", encToRemove.Id, conn, trans, recordCounts)}";
                                    logMessage += $"\n{await PerformDeleteForEncounter("EncounterNpc", encToRemove.Id, conn, trans, recordCounts)}";
                                    logMessage += $"\n{await PerformDeleteForEncounter("EncounterPlayerStatistics", encToRemove.Id, conn, trans, recordCounts)}";

                                    sw.Stop();

                                    logMessage += $"\n\nCompleted in {sw.Elapsed}";
                                    if (thisIndex < totalEncounters)
                                    {
                                        logMessage +=
                                            $"\n\nNext encounter to remove: Encounter ID #{nextToRemove.Id} ({thisIndex + 1}/{totalEncounters}). Encounter length: {nextToRemove.Duration}";
                                    }

                                    // If this process took less than 500ms, wait a short period of time so that we're not rate limited
                                    if (sw.ElapsedMilliseconds < 550)
                                    {
                                        await Task.Delay(550 - (int)sw.ElapsedMilliseconds);
                                    }

                                    //await _discord.Log(logMessage, "EncounterRepository", LogLevel.Debug);
                                    Debug.WriteLine(logMessage);
                                    trans.Commit();
                                }
                            }
                            catch (Exception ex)
                            {
                                trans.Rollback();
                                await _discord.Log($"An exception occurred within the transaction: {ex.Message}",
                                    "EncounterRepository", LogLevel.Critical);
                                returnValue.Message = ex.Message;
                            }
                        }
                    }

                    totalRemoved++;
                    removeList.RemoveAt(0);
                }
            }
            catch (Exception ex)
            {
                _logger.Debug($"An error occurred while removing encounters marked for deletion: {ex.Message}");
                await _discord.Log($"An exception was thrown while removing encounters marked for deletion: {ex.Message}", "EncounterRepository", LogLevel.Critical);
                returnValue.Message = ex.Message;
            }

            await _discord.Log("Exiting the method for some reason", "EncounterRepository", LogLevel.Warning);
            return returnValue;
        }

        public async Task<ReturnValue> CheckForOrphanedEncountersAsync()
        {
            using (var conn = await OpenConnectionAsync())
            {
                // Update records for encounter stats
                var encountersMissingStats =
                    (await conn.QueryAsync<int>(OrphanedEncounter.EncounterIdsWithoutTableStats)).AsList();
                var totalEncountersToUpdate = encountersMissingStats.Count;
                Debug.WriteLine($"There are {totalEncountersToUpdate} encounters that don't have stats.");

                int i = 1;
                foreach (var encId in encountersMissingStats)
                {
                    var stats = (await conn.QueryAsync<EncounterTableRecords>(OrphanedEncounter.GetBasicEncounterStats,
                        new { id = encId })).SingleOrDefault();
                    //Debug.WriteLine($"Encounter {encId} DMG: {stats.Damage} HEAL: {stats.Healing} SHIELD: {stats.Shielding}");
                    var addResult = await conn.ExecuteAsync(OrphanedEncounter.InsertTableStatsForEncounter, new
                    {
                        id = encId,
                        damage = stats.Damage,
                        healing = stats.Healing,
                        shielding = stats.Shielding
                    });
                    Debug.WriteLine($"Updating stats for encounter #{encId} ({i}/{totalEncountersToUpdate}): {addResult}");
                    i++;
                }



                //var encounterIds = (await conn.QueryAsync<int>(MySQL.Encounter.GetAllEncounterIdsNotToBeDeleted)).AsList();

                //int i = 1;
                //foreach (var encId in encounterIds)
                //{
                //    if (i == 10)
                //    {
                //        break;
                //    }

                //    var recordCounts = (await conn.QueryAsync<EncounterTableRecords>(EncounterRemovalFrom.GetEncounterRecordCounts,new { id = encId })).SingleOrDefault();
                //    if (recordCounts == null)
                //    {
                //        Debug.WriteLine($"** Encounter Id #{encId} counts returned a null value");
                //    }
                //    else if (!recordCounts.HasRecords)
                //    {
                //        Debug.WriteLine($"** Encounter Id #{encId} appears to have been deleted - there are no records.");
                //    }
                //    else
                //    {
                //        Debug.WriteLine($"Encounter Id #{encId} - Damage {recordCounts.Damage}, Healing {recordCounts.Healing}");
                //    }

                //    // Step 1: Check that each encounter Id has matching rows in the tables that should have them (Damage/Healing)



                //    // SELECT COUNT(1) FROM HealingDone WHERE EncounterId > 1 AND EncounterId < 3

                //    // Step 2: Check Damage / Healing / Shielding / Buff / Debuff tables for encounter IDs that don't exist

                //    // Check healing done

                //    i++;
                //}
            }

            return new ReturnValue(false, "");
        }

        private async Task<string> PerformDeleteForEncounter(string tableName, long encounterId, DbConnection conn,
            DbTransaction trans, EncounterTableRecords counts)
        {
            int recordsDeleted = 0;
            int previousRecordCount = 0;

            switch (tableName)
            {
                case "EncounterOverview":
                    previousRecordCount = counts.Overview;
                    if (previousRecordCount > 0)
                    {
                        recordsDeleted = await conn.ExecuteAsync(EncounterRemovalFrom.EncounterOverview, new { id = encounterId }, trans);
                    }
                    break;
                case "EncounterBuffEvent":
                    previousRecordCount = counts.BuffEvent;
                    if (previousRecordCount > 0)
                    {
                        recordsDeleted = await conn.ExecuteAsync(EncounterRemovalFrom.EncounterBuffEvent, new { id = encounterId }, trans);
                    }
                    break;
                case "EncounterBuffUptime":
                    previousRecordCount = counts.BuffUptime;
                    if (previousRecordCount > 0)
                    {
                        recordsDeleted = await conn.ExecuteAsync(EncounterRemovalFrom.EncounterBuffUptime, new { id = encounterId }, trans);
                    }
                    break;
                case "EncounterBuffAction":
                    previousRecordCount = counts.BuffAction;
                    if (previousRecordCount > 0)
                    {
                        recordsDeleted = await conn.ExecuteAsync(EncounterRemovalFrom.EncounterBuffAction, new { id = encounterId }, trans);
                    }
                    break;
                case "EncounterDebuffAction":
                    previousRecordCount = counts.DebuffAction;
                    if (previousRecordCount > 0)
                    {
                        recordsDeleted = await conn.ExecuteAsync(EncounterRemovalFrom.EncounterDebuffAction, new { id = encounterId }, trans);
                    }
                    break;
                case "EncounterNpcCast":
                    previousRecordCount = counts.NpcCast;
                    if (previousRecordCount > 0)
                    {
                        recordsDeleted = await conn.ExecuteAsync(EncounterRemovalFrom.EncounterNpcCast, new { id = encounterId }, trans);
                    }
                    break;
                case "EncounterDeath":
                    previousRecordCount = counts.Death;
                    if (previousRecordCount > 0)
                    {
                        recordsDeleted = await conn.ExecuteAsync(EncounterRemovalFrom.EncounterDeath, new { id = encounterId }, trans);
                    }
                    break;
                case "DamageDone":
                    previousRecordCount = counts.Damage;
                    if (previousRecordCount > 0)
                    {
                        recordsDeleted = await conn.ExecuteAsync(EncounterRemovalFrom.DamageDone, new { id = encounterId }, trans);
                    }
                    break;
                case "HealingDone":
                    previousRecordCount = counts.Healing;
                    if (previousRecordCount > 0)
                    {
                        recordsDeleted = await conn.ExecuteAsync(EncounterRemovalFrom.HealingDone, new { id = encounterId }, trans);
                    }
                    break;
                case "ShieldingDone":
                    previousRecordCount = counts.Shielding;
                    if (previousRecordCount > 0)
                    {
                        recordsDeleted = await conn.ExecuteAsync(EncounterRemovalFrom.ShieldingDone, new { id = encounterId }, trans);
                    }
                    break;
                case "EncounterPlayerRole":
                    previousRecordCount = counts.PlayerRole;
                    if (previousRecordCount > 0)
                    {
                        recordsDeleted = await conn.ExecuteAsync(EncounterRemovalFrom.EncounterPlayerRole, new { id = encounterId }, trans);
                    }
                    break;
                case "EncounterNpc":
                    previousRecordCount = counts.Npc;
                    if (previousRecordCount > 0)
                    {
                        recordsDeleted = await conn.ExecuteAsync(EncounterRemovalFrom.EncounterNpc, new { id = encounterId }, trans);
                    }
                    break;
                case "EncounterPlayerStatistics":
                    previousRecordCount = counts.PlayerStatistics;
                    if (previousRecordCount > 0)
                    {
                        recordsDeleted = await conn.ExecuteAsync(EncounterRemovalFrom.EncounterPlayerStatistics, new { id = encounterId }, trans);
                    }
                    break;
                default:
                    return $"The table {tableName} was called for delete but was unhandled.";
            }

            if (previousRecordCount == 0)
            {
                return $"{tableName}: No existing records to delete.";
            }

            var recordText = previousRecordCount == 1 ? "record" : "records";

            return $"{tableName}: {recordsDeleted}/{previousRecordCount} {recordText} deleted.";
        }

        /// <summary>
        /// Marks encounters with the specified IDs as 'ToBeDeleted'
        /// </summary>
        /// <param name="encounterIds"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        public ReturnValue MarkEncountersForDeletion(List<int> encounterIds, string email)
        {
            ReturnValue returnValue = new ReturnValue();
            try
            {
                string timeElapsed;
                Execute(e => e.Execute(MySQL.Encounter.MarkForDeletion, new { @ids = encounterIds }), out timeElapsed);
                returnValue.Success = true;
                return returnValue;
            }
            catch (Exception ex)
            {
                returnValue.Message = ex.Message;
                return returnValue;
            }
        }

        public void UpdateDurationForEncounter(int encounterId, TimeSpan duration)
        {
            try
            {
                string timeElapsed;
                Execute(
                    e =>
                        e.Execute(MySQL.Encounter.UpdateDurationForEncounter,
                            new { @id = encounterId, @duration = duration.ToString() }), out timeElapsed);
            }
            catch (Exception ex)
            {
                _logger.Debug(string.Format("Error while updating the duration for encounter {0}: {1}", encounterId, ex.Message));
            }
        }

        // Async methods

        public async Task<Encounter> GetAsync(int id)
        {
            var result = await QueryAsync(q => q.QueryAsync<Encounter, Models.EncounterDifficulty, BossFight, Models.Instance, AuthUserCharacter, Shard, Guild, Encounter>
                (SQL.Encounter.GetSingle,
                    (e, ed, bf, i, auc, s, g) =>
                    {
                        e.EncounterDifficulty = ed;
                        bf.Instance = i;
                        e.BossFight = bf;
                        auc.Shard = s;
                        e.UploadCharacter = auc;
                        if (g != null)
                        {
                            e.UploadGuild = g;
                        }
                        return e;
                    }, new { id }));

            return result.SingleOrDefault();
        }

        public async Task<bool> EncounterNpcRecordsExistAsync(int id)
        {
            var result = await QueryAsync(q => q.QueryAsync<long>(MySQL.EncounterNpc.RecordsExistForEncounter, new { id }));
            return result.SingleOrDefault() == 1;
        }

        public async Task<List<EncounterNpc>> GetEncounterNpcsFromEncounterInfoAsync(int id)
        {
            return (await QueryAsync(q => q.QueryAsync<EncounterNpc>(MySQL.EncounterNpc.GetAllForEncounter, new { id }))).ToList();
        }

        public async Task<List<EncounterPlayerRole>> GetEncounterRoleRecordsAsync(int id)
        {
            return (await QueryAsync(q => q.QueryAsync<EncounterPlayerRole>(MySQL.Encounter.GetEncounterRoleRecords, new { id }))).ToList();
        }

        public async Task<List<PlayerRole>> GetPlayerRolesAsync(int id)
        {
            var roles = new List<PlayerRole>();

            var shieldRoles = new List<PlayerRole>();
            var healRoles = new List<PlayerRole>();
            var damageRoles = new List<PlayerRole>();

            using (var connection = await OpenConnectionAsync())
            {
                var shieldTask = connection.QueryAsync<PlayerRole>(SQL.Encounter.PlayerRoles.Shield, new { id });
                var healTask = connection.QueryAsync<PlayerRole>(SQL.Encounter.PlayerRoles.Heal, new { id });
                var damageTask = connection.QueryAsync<PlayerRole>(SQL.Encounter.PlayerRoles.Damage, new { id });

                await Task.WhenAll(shieldTask, healTask, damageTask);

                shieldRoles = shieldTask.Result.ToList();
                healRoles = healTask.Result.ToList();
                damageRoles = damageTask.Result.ToList();
            }

            roles.AddRange(shieldRoles);
            foreach (var healRole in healRoles.Where(healRole => roles.Any(r => r.Id == healRole.Id) == false))
            {
                roles.Add(healRole);
            }

            if (damageRoles.Any())
            {
                foreach (var dmgRole in damageRoles)
                {
                    var role = dmgRole;
                    if (!roles.Any(r => r.Id == role.Id))
                    {
                        roles.Add(dmgRole);
                    }
                }
            }

            return roles;
        }

        public async Task<long> GetEncounterDpsAsync(int id, int totalSeconds)
        {
            try
            {
                if (totalSeconds == 0) return 0;

                var result = (await QueryAsync(q => q.QueryAsync<EncounterTotalAverage>(SQL.Encounter.Overview.PlayerDpsAverage, new { id, totalSeconds }))).SingleOrDefault();
                return result == null ? 0 : result.Average;
            }
            catch (Exception ex)
            {
                _logger.Debug(string.Format("An error occurred while getting encounter DPS: {0}", ex.Message));
            }
            return 0;
        }

        public async Task<long> GetEncounterHpsAsync(int id, int totalSeconds)
        {
            try
            {
                if (totalSeconds == 0) return 0;

                var result = (await QueryAsync(q => q.QueryAsync<EncounterTotalAverage>(SQL.Encounter.Overview.PlayerHpsAverage, new { id, totalSeconds }))).SingleOrDefault();
                return result == null ? 0 : result.Average;
            }
            catch (Exception ex)
            {
                _logger.Debug(string.Format("An error occurred while getting encounter HPS: {0}", ex.Message));
            }
            return 0;
        }

        public async Task<long> GetEncounterApsAsync(int id, int totalSeconds)
        {
            try
            {
                if (totalSeconds == 0) return 0;

                var result = (await QueryAsync(q => q.QueryAsync<EncounterTotalAverage>(SQL.Encounter.Overview.PlayerApsAverage, new { id, totalSeconds }))).SingleOrDefault();
                return result == null ? 0 : result.Average;
            }
            catch (Exception ex)
            {
                _logger.Debug(string.Format("An error occurred while getting encounter APS: {0}", ex.Message));
            }
            return 0;
        }

        public async Task<List<EncounterDebuffAction>> GetDebuffActionsAsync(int id)
        {
            try
            {
                return (await QueryAsync(q => q.QueryAsync<EncounterDebuffAction>(SQL.Encounter.Overview.DebuffOverview, new { id }))).ToList();
            }
            catch (Exception ex)
            {
                _logger.Debug(string.Format("An error occurred while getting debuff overview: {0}", ex.Message));
            }
            return new List<EncounterDebuffAction>();
        }

        public async Task<List<EncounterBuffAction>> GetBuffActionsAsync(int id)
        {
            try
            {
                return (await QueryAsync(q => q.QueryAsync<EncounterBuffAction>(SQL.Encounter.Overview.BuffOverview, new { id }))).ToList();
            }
            catch (Exception ex)
            {
                _logger.Debug(string.Format("An error occurred while getting buff overview: {0}", ex.Message));
            }
            return new List<EncounterBuffAction>();
        }

        public async Task<List<EncounterNpcCast>> GetNpcCastsAsync(int id)
        {
            try
            {
                return (await QueryAsync(q => q.QueryAsync<EncounterNpcCast>(SQL.Encounter.Overview.NpcCast, new { id }))).ToList();
            }
            catch (Exception ex)
            {
                _logger.Debug(string.Format("An error occurred while getting npc casts: {0}", ex.Message));
            }
            return new List<EncounterNpcCast>();
        }

        public async Task<int> GetTotalPlayerDeathsAsync(int id)
        {
            return Convert.ToInt32((await QueryAsync(q => q.QueryAsync<long>(SQL.Encounter.Overview.TotalPlayerDeathsForEncounter, new { id }))).SingleOrDefault());
        }

        public async Task<List<DamageDone>> GetAllDamageDoneForEncounterAsync(int id)
        {
            return (await QueryAsync(q => q.QueryAsync<DamageDone>(MySQL.Encounter.GetAllDamageDoneForEncounter, new { id }))).ToList();
        }

        public async Task<List<HealingDone>> GetAllHealingDoneForEncounterAsync(int id)
        {
            return (await QueryAsync(q => q.QueryAsync<HealingDone>(MySQL.Encounter.GetAllHealingDoneForEncounter, new { id }))).ToList();
        }

        public async Task<List<ShieldingDone>> GetAllShieldingDoneForEncounterAsync(int id)
        {
            return (await QueryAsync(q => q.QueryAsync<ShieldingDone>(MySQL.Encounter.GetAllShieldingDoneForEncounter, new { id }))).ToList();
        }

        public async Task<List<CharacterInteractionPerSecond>> CharacterInteractionPerSecondAsync(int id, int playerId = -1, string npcId = "", bool outgoing = true,
            CharacterType characterType = CharacterType.Player, InteractionType interactionType = InteractionType.Damage,
            InteractionFilter filter = InteractionFilter.All, InteractionMode mode = InteractionMode.Ability)
        {
            try
            {
                List<CharacterInteractionPerSecond> results;

                switch (interactionType)
                {
                    case InteractionType.Damage:
                        #region Damage
                        switch (filter)
                        {
                            case InteractionFilter.Npcs:
                                results = (await QueryAsync(q => q.QueryAsync<CharacterInteractionPerSecond>
                                        (mode == InteractionMode.Ability
                                        ? outgoing // By ability
                                            ? characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Damage.Done.ByAbility.OnlyNpcs.PerSecond // 1 player -> many NPCs
                                                : MySQL.Encounter.Character.Npc.Damage.Done.ByAbility.OtherNpcs.PerSecond // 1 NPC -> many NPCs
                                            : characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Damage.Taken.ByAbility.OnlyNpcs.PerSecond // Many NPCs -> 1 player
                                                : MySQL.Encounter.Character.Npc.Damage.Taken.ByAbility.OtherNpcs.PerSecond // Many NPCs -> 1 NPC
                                        : outgoing // By source / target
                                            ? characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Damage.Done.ByTarget.OnlyNpcs.PerSecond // 1 player -> many NPCs
                                                : MySQL.Encounter.Character.Npc.Damage.Done.ByTarget.OtherNpcs.PerSecond // 1 NPC -> many NPCs
                                            : characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Damage.Taken.BySource.OnlyNpcs.PerSecond // Many NPCs -> 1 player
                                                : MySQL.Encounter.Character.Npc.Damage.Taken.BySource.OtherNpcs.PerSecond // Many NPCs -> 1 NPC
                                        , new { id, playerId, npcId }))).ToList();
                                break;
                            case InteractionFilter.Players:
                                results = (await QueryAsync(q => q.QueryAsync<CharacterInteractionPerSecond>
                                    (mode == InteractionMode.Ability
                                    ? outgoing // By ability
                                        ? characterType == CharacterType.Player
                                            ? MySQL.Encounter.Character.Player.Damage.Done.ByAbility.OtherPlayers.PerSecond // 1 player -> many players
                                            : MySQL.Encounter.Character.Npc.Damage.Done.ByAbility.OnlyPlayers.PerSecond // 1 NPC -> many players
                                        : characterType == CharacterType.Player
                                            ? MySQL.Encounter.Character.Player.Damage.Taken.ByAbility.OtherPlayers.PerSecond // Many players -> 1 player
                                            : MySQL.Encounter.Character.Npc.Damage.Taken.ByAbility.OnlyPlayers.PerSecond // Many players -> 1 NPC
                                    : outgoing // By source / target
                                        ? characterType == CharacterType.Player
                                            ? MySQL.Encounter.Character.Player.Damage.Done.ByTarget.OtherPlayers.PerSecond // 1 player -> many players
                                            : MySQL.Encounter.Character.Npc.Damage.Done.ByTarget.OnlyPlayers.PerSecond // 1 NPC -> many players
                                        : characterType == CharacterType.Player
                                            ? MySQL.Encounter.Character.Player.Damage.Taken.BySource.OtherPlayers.PerSecond // Many players -> 1 player
                                            : MySQL.Encounter.Character.Npc.Damage.Taken.BySource.OnlyPlayers.PerSecond // Many players -> 1 NPC
                                    , new { id, playerId, npcId }))).ToList();
                                break;
                            case InteractionFilter.Self: // Only has "by ability" - no point doing "by target" or "by source" on self!
                                results = mode == InteractionMode.Ability
                                    ? (await QueryAsync(q => q.QueryAsync<CharacterInteractionPerSecond>
                                        (outgoing
                                            ? characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Damage.Done.ByAbility.OnlySelf.PerSecond // 1 player
                                                : MySQL.Encounter.Character.Npc.Damage.Done.ByAbility.OnlySelf.PerSecond // 1 NPC
                                            : characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Damage.Taken.ByAbility.OnlySelf.PerSecond // 1 player
                                                : MySQL.Encounter.Character.Npc.Damage.Taken.ByAbility.OnlySelf.PerSecond // 1 NPC
                                            , new { id, playerId, npcId }))).ToList()
                                    : null;
                                break;
                            case InteractionFilter.All:
                                results = (await QueryAsync(q => q.QueryAsync<CharacterInteractionPerSecond>
                                        (mode == InteractionMode.Ability
                                        ? outgoing // By ability
                                            ? characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Damage.Done.ByAbility.AllTargets.PerSecond // 1 player -> many players and many NPCs
                                                : MySQL.Encounter.Character.Npc.Damage.Done.ByAbility.AllTargets.PerSecond // 1 npc -> many players and many NPCs
                                            : characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Damage.Taken.ByAbility.AllSources.PerSecond // Many players and many NPCs -> 1 player
                                                : MySQL.Encounter.Character.Npc.Damage.Taken.ByAbility.AllSources.PerSecond // Many players and many NPCs -> 1 NPC
                                        : outgoing // By source / target
                                            ? characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Damage.Done.ByTarget.AllTargets.PerSecond // 1 player -> many players and many NPCs
                                                : MySQL.Encounter.Character.Npc.Damage.Done.ByTarget.AllTargets.PerSecond // 1 npc -> many players and many NPCs
                                            : characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Damage.Taken.BySource.AllSources.PerSecond // Many players and many NPCs -> 1 player
                                                : MySQL.Encounter.Character.Npc.Damage.Taken.BySource.AllSources.PerSecond // Many players and many NPCs -> 1 NPC
                                        , new { id, playerId, npcId }))).ToList();
                                break;
                            default:
                                results = null;
                                break;
                        }
                        break;
                    #endregion
                    case InteractionType.Absorption:
                        #region Absorption
                        switch (filter)
                        {
                            case InteractionFilter.Npcs:
                                results = (await QueryAsync(q => q.QueryAsync<CharacterInteractionPerSecond>
                                        (mode == InteractionMode.Ability
                                        ? outgoing // By ability
                                            ? characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Shielding.Done.ByAbility.OnlyNpcs.PerSecond // 1 player -> many NPCs
                                                : MySQL.Encounter.Character.Npc.Shielding.Done.ByAbility.OtherNpcs.PerSecond // 1 NPC -> many NPCs
                                            : characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Shielding.Taken.ByAbility.OnlyNpcs.PerSecond // Many NPCs -> 1 player
                                                : MySQL.Encounter.Character.Npc.Shielding.Taken.ByAbility.OtherNpcs.PerSecond // Many NPCs -> 1 NPC
                                        : outgoing // By source / target
                                            ? characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Shielding.Done.ByTarget.OnlyNpcs.PerSecond // 1 player -> many NPCs
                                                : MySQL.Encounter.Character.Npc.Shielding.Done.ByTarget.OtherNpcs.PerSecond // 1 NPC -> many NPCs
                                            : characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Shielding.Taken.BySource.OnlyNpcs.PerSecond // Many NPCs -> 1 player
                                                : MySQL.Encounter.Character.Npc.Shielding.Taken.BySource.OtherNpcs.PerSecond // Many NPCs -> 1 NPC
                                        , new { id, playerId, npcId }))).ToList();
                                break;
                            case InteractionFilter.Players:
                                results = (await QueryAsync(q => q.QueryAsync<CharacterInteractionPerSecond>
                                    (mode == InteractionMode.Ability
                                    ? outgoing // By ability
                                        ? characterType == CharacterType.Player
                                            ? MySQL.Encounter.Character.Player.Shielding.Done.ByAbility.OtherPlayers.PerSecond // 1 player -> many players
                                            : MySQL.Encounter.Character.Npc.Shielding.Done.ByAbility.OnlyPlayers.PerSecond // 1 NPC -> many players
                                        : characterType == CharacterType.Player
                                            ? MySQL.Encounter.Character.Player.Shielding.Taken.ByAbility.OtherPlayers.PerSecond // Many players -> 1 player
                                            : MySQL.Encounter.Character.Npc.Shielding.Taken.ByAbility.OnlyPlayers.PerSecond // Many players -> 1 NPC
                                    : outgoing // By source / target
                                        ? characterType == CharacterType.Player
                                            ? MySQL.Encounter.Character.Player.Shielding.Done.ByTarget.OtherPlayers.PerSecond // 1 player -> many players
                                            : MySQL.Encounter.Character.Npc.Shielding.Done.ByTarget.OnlyPlayers.PerSecond // 1 NPC -> many players
                                        : characterType == CharacterType.Player
                                            ? MySQL.Encounter.Character.Player.Shielding.Taken.BySource.OtherPlayers.PerSecond // Many players -> 1 player
                                            : MySQL.Encounter.Character.Npc.Shielding.Taken.BySource.OnlyPlayers.PerSecond // Many players -> 1 NPC
                                    , new { id, playerId, npcId }))).ToList();
                                break;
                            case InteractionFilter.Self: // Only has "by ability" - no point doing "by target" or "by source" on self!
                                results = mode == InteractionMode.Ability
                                    ? (await QueryAsync(q => q.QueryAsync<CharacterInteractionPerSecond>
                                        (outgoing
                                            ? characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Shielding.Done.ByAbility.OnlySelf.PerSecond // 1 player
                                                : MySQL.Encounter.Character.Npc.Shielding.Done.ByAbility.OnlySelf.PerSecond // 1 NPC
                                            : characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Shielding.Taken.ByAbility.OnlySelf.PerSecond // 1 player
                                                : MySQL.Encounter.Character.Npc.Shielding.Taken.ByAbility.OnlySelf.PerSecond // 1 NPC
                                            , new { id, playerId, npcId }))).ToList()
                                    : null;
                                break;
                            case InteractionFilter.All:
                                results = (await QueryAsync(q => q.QueryAsync<CharacterInteractionPerSecond>
                                        (mode == InteractionMode.Ability
                                        ? outgoing // By ability
                                            ? characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Shielding.Done.ByAbility.AllTargets.PerSecond // 1 player -> many players and many NPCs
                                                : MySQL.Encounter.Character.Npc.Shielding.Done.ByAbility.AllTargets.PerSecond // 1 npc -> many players and many NPCs
                                            : characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Shielding.Taken.ByAbility.AllSources.PerSecond // Many players and many NPCs -> 1 player
                                                : MySQL.Encounter.Character.Npc.Shielding.Taken.ByAbility.AllSources.PerSecond // Many players and many NPCs -> 1 NPC
                                        : outgoing // By source / target
                                            ? characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Shielding.Done.ByTarget.AllTargets.PerSecond // 1 player -> many players and many NPCs
                                                : MySQL.Encounter.Character.Npc.Shielding.Done.ByTarget.AllTargets.PerSecond // 1 npc -> many players and many NPCs
                                            : characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Shielding.Taken.BySource.AllSources.PerSecond // Many players and many NPCs -> 1 player
                                                : MySQL.Encounter.Character.Npc.Shielding.Taken.BySource.AllSources.PerSecond // Many players and many NPCs -> 1 NPC
                                        , new { id, playerId, npcId }))).ToList();
                                break;
                            default:
                                results = null;
                                break;
                        }
                        break;
                    #endregion
                    case InteractionType.Healing:
                        #region Healing
                        switch (filter)
                        {
                            case InteractionFilter.Npcs:
                                results = (await QueryAsync(q => q.QueryAsync<CharacterInteractionPerSecond>
                                        (mode == InteractionMode.Ability
                                        ? outgoing // By ability
                                            ? characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Healing.Done.ByAbility.OnlyNpcs.PerSecond // 1 player -> many NPCs
                                                : MySQL.Encounter.Character.Npc.Healing.Done.ByAbility.OtherNpcs.PerSecond // 1 NPC -> many NPCs
                                            : characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Healing.Taken.ByAbility.OnlyNpcs.PerSecond // Many NPCs -> 1 player
                                                : MySQL.Encounter.Character.Npc.Healing.Taken.ByAbility.OtherNpcs.PerSecond // Many NPCs -> 1 NPC
                                        : outgoing // By source / target
                                            ? characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Healing.Done.ByTarget.OnlyNpcs.PerSecond // 1 player -> many NPCs
                                                : MySQL.Encounter.Character.Npc.Healing.Done.ByTarget.OtherNpcs.PerSecond // 1 NPC -> many NPCs
                                            : characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Healing.Taken.BySource.OnlyNpcs.PerSecond // Many NPCs -> 1 player
                                                : MySQL.Encounter.Character.Npc.Healing.Taken.BySource.OtherNpcs.PerSecond // Many NPCs -> 1 NPC
                                        , new { id, playerId, npcId }))).ToList();
                                break;
                            case InteractionFilter.Players:
                                results = (await QueryAsync(q => q.QueryAsync<CharacterInteractionPerSecond>
                                    (mode == InteractionMode.Ability
                                    ? outgoing // By ability
                                        ? characterType == CharacterType.Player
                                            ? MySQL.Encounter.Character.Player.Healing.Done.ByAbility.OtherPlayers.PerSecond // 1 player -> many players
                                            : MySQL.Encounter.Character.Npc.Healing.Done.ByAbility.OnlyPlayers.PerSecond // 1 NPC -> many players
                                        : characterType == CharacterType.Player
                                            ? MySQL.Encounter.Character.Player.Healing.Taken.ByAbility.OtherPlayers.PerSecond // Many players -> 1 player
                                            : MySQL.Encounter.Character.Npc.Healing.Taken.ByAbility.OnlyPlayers.PerSecond // Many players -> 1 NPC
                                    : outgoing // By source / target
                                        ? characterType == CharacterType.Player
                                            ? MySQL.Encounter.Character.Player.Healing.Done.ByTarget.OtherPlayers.PerSecond // 1 player -> many players
                                            : MySQL.Encounter.Character.Npc.Healing.Done.ByTarget.OnlyPlayers.PerSecond // 1 NPC -> many players
                                        : characterType == CharacterType.Player
                                            ? MySQL.Encounter.Character.Player.Healing.Taken.BySource.OtherPlayers.PerSecond // Many players -> 1 player
                                            : MySQL.Encounter.Character.Npc.Healing.Taken.BySource.OnlyPlayers.PerSecond // Many players -> 1 NPC
                                    , new { id, playerId, npcId }))).ToList();
                                break;
                            case InteractionFilter.Self: // Only has "by ability" - no point doing "by target" or "by source" on self!
                                results = mode == InteractionMode.Ability
                                    ? (await QueryAsync(q => q.QueryAsync<CharacterInteractionPerSecond>
                                        (outgoing
                                            ? characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Healing.Done.ByAbility.OnlySelf.PerSecond // 1 player
                                                : MySQL.Encounter.Character.Npc.Healing.Done.ByAbility.OnlySelf.PerSecond // 1 NPC
                                            : characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Healing.Taken.ByAbility.OnlySelf.PerSecond // 1 player
                                                : MySQL.Encounter.Character.Npc.Healing.Taken.ByAbility.OnlySelf.PerSecond // 1 NPC
                                            , new { id, playerId, npcId }))).ToList()
                                    : null;
                                break;
                            case InteractionFilter.All:
                                results = (await QueryAsync(q => q.QueryAsync<CharacterInteractionPerSecond>
                                        (mode == InteractionMode.Ability
                                        ? outgoing // By ability
                                            ? characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Healing.Done.ByAbility.AllTargets.PerSecond // 1 player -> many players and many NPCs
                                                : MySQL.Encounter.Character.Npc.Healing.Done.ByAbility.AllTargets.PerSecond // 1 npc -> many players and many NPCs
                                            : characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Healing.Taken.ByAbility.AllSources.PerSecond // Many players and many NPCs -> 1 player
                                                : MySQL.Encounter.Character.Npc.Healing.Taken.ByAbility.AllSources.PerSecond // Many players and many NPCs -> 1 NPC
                                        : outgoing // By source / target
                                            ? characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Healing.Done.ByTarget.AllTargets.PerSecond // 1 player -> many players and many NPCs
                                                : MySQL.Encounter.Character.Npc.Healing.Done.ByTarget.AllTargets.PerSecond // 1 npc -> many players and many NPCs
                                            : characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Healing.Taken.BySource.AllSources.PerSecond // Many players and many NPCs -> 1 player
                                                : MySQL.Encounter.Character.Npc.Healing.Taken.BySource.AllSources.PerSecond // Many players and many NPCs -> 1 NPC
                                        , new { id, playerId, npcId }))).ToList();
                                break;
                            default:
                                results = null;
                                break;
                        }
                        break;
                    #endregion
                    default:
                        results = null;
                        break;
                }
                return results;
            }
            catch (Exception ex)
            {
                _logger.Debug(string.Format("Error while getting encounter interactions: {0}", ex.Message));
                return new List<CharacterInteractionPerSecond>();
            }
        }

        public async Task<List<EncounterCharacterAbilityBreakdownDetail>> CharacterInteractionTotalsAsync(int id, int playerId = -1, string npcId = "", bool outgoing = true,
            CharacterType characterType = CharacterType.Player, InteractionType interactionType = InteractionType.Damage,
            InteractionFilter filter = InteractionFilter.All, InteractionMode mode = InteractionMode.Ability,
            int totalSeconds = 0)
        {
            try
            {
                List<EncounterCharacterAbilityBreakdownDetail> results;

                switch (interactionType)
                {
                    case InteractionType.Damage:
                        #region Damage
                        switch (filter)
                        {
                            case InteractionFilter.Npcs:
                                results = (await QueryAsync(q => q.QueryAsync<EncounterCharacterAbilityBreakdownDetail>
                                        (mode == InteractionMode.Ability
                                        ? outgoing // By ability
                                            ? characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Damage.Done.ByAbility.OnlyNpcs.Totals // 1 player -> many NPCs
                                                : MySQL.Encounter.Character.Npc.Damage.Done.ByAbility.OtherNpcs.Totals // 1 NPC -> many NPCs
                                            : characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Damage.Taken.ByAbility.OnlyNpcs.Totals // Many NPCs -> 1 player
                                                : MySQL.Encounter.Character.Npc.Damage.Taken.ByAbility.OtherNpcs.Totals // Many NPCs -> 1 NPC
                                        : outgoing // By source / target
                                            ? characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Damage.Done.ByTarget.OnlyNpcs.Totals // 1 player -> many NPCs
                                                : MySQL.Encounter.Character.Npc.Damage.Done.ByTarget.OtherNpcs.Totals // 1 NPC -> many NPCs
                                            : characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Damage.Taken.BySource.OnlyNpcs.Totals // Many NPCs -> 1 player
                                                : MySQL.Encounter.Character.Npc.Damage.Taken.BySource.OtherNpcs.Totals // Many NPCs -> 1 NPC
                                        , new { id, playerId, npcId, totalSeconds }))).ToList();
                                break;
                            case InteractionFilter.Players:
                                results = (await QueryAsync(q => q.QueryAsync<EncounterCharacterAbilityBreakdownDetail>
                                    (mode == InteractionMode.Ability
                                    ? outgoing // By ability
                                        ? characterType == CharacterType.Player
                                            ? MySQL.Encounter.Character.Player.Damage.Done.ByAbility.OtherPlayers.Totals // 1 player -> many players
                                            : MySQL.Encounter.Character.Npc.Damage.Done.ByAbility.OnlyPlayers.Totals // 1 NPC -> many players
                                        : characterType == CharacterType.Player
                                            ? MySQL.Encounter.Character.Player.Damage.Taken.ByAbility.OtherPlayers.Totals // Many players -> 1 player
                                            : MySQL.Encounter.Character.Npc.Damage.Taken.ByAbility.OnlyPlayers.Totals // Many players -> 1 NPC
                                    : outgoing // By source / target
                                        ? characterType == CharacterType.Player
                                            ? MySQL.Encounter.Character.Player.Damage.Done.ByTarget.OtherPlayers.Totals // 1 player -> many players
                                            : MySQL.Encounter.Character.Npc.Damage.Done.ByTarget.OnlyPlayers.Totals // 1 NPC -> many players
                                        : characterType == CharacterType.Player
                                            ? MySQL.Encounter.Character.Player.Damage.Taken.BySource.OtherPlayers.Totals // Many players -> 1 player
                                            : MySQL.Encounter.Character.Npc.Damage.Taken.BySource.OnlyPlayers.Totals // Many players -> 1 NPC
                                    , new { id, playerId, npcId, totalSeconds }))).ToList();
                                break;
                            case InteractionFilter.Self:// Only has "by ability" - no point doing "by target" or "by source" on self!
                                results = mode == InteractionMode.Ability
                                    ? (await QueryAsync(q => q.QueryAsync<EncounterCharacterAbilityBreakdownDetail>
                                        (outgoing
                                            ? characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Damage.Done.ByAbility.OnlySelf.Totals // 1 player
                                                : MySQL.Encounter.Character.Npc.Damage.Done.ByAbility.OnlySelf.Totals // 1 NPC
                                            : characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Damage.Taken.ByAbility.OnlySelf.Totals // 1 player
                                                : MySQL.Encounter.Character.Npc.Damage.Taken.ByAbility.OnlySelf.Totals // 1 NPC
                                            , new { id, playerId, npcId, totalSeconds }))).ToList()
                                    : null;
                                break;
                            case InteractionFilter.All:
                                results = (await QueryAsync(q => q.QueryAsync<EncounterCharacterAbilityBreakdownDetail>
                                        (mode == InteractionMode.Ability
                                        ? outgoing // By ability
                                            ? characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Damage.Done.ByAbility.AllTargets.Totals // 1 player -> many players and many NPCs
                                                : MySQL.Encounter.Character.Npc.Damage.Done.ByAbility.AllTargets.Totals // 1 npc -> many players and many NPCs
                                            : characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Damage.Taken.ByAbility.AllSources.Totals // Many players and many NPCs -> 1 player
                                                : MySQL.Encounter.Character.Npc.Damage.Taken.ByAbility.AllSources.Totals // Many players and many NPCs -> 1 NPC
                                        : outgoing // By source / target
                                            ? characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Damage.Done.ByTarget.AllTargets.Totals // 1 player -> many players and many NPCs
                                                : MySQL.Encounter.Character.Npc.Damage.Done.ByTarget.AllTargets.Totals // 1 npc -> many players and many NPCs
                                            : characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Damage.Taken.BySource.AllSources.Totals // Many players and many NPCs -> 1 player
                                                : MySQL.Encounter.Character.Npc.Damage.Taken.BySource.AllSources.Totals // Many players and many NPCs -> 1 NPC
                                        , new { id, playerId, npcId, totalSeconds }))).ToList();
                                break;
                            default:
                                results = null;
                                break;
                        }
                        #endregion
                        break;
                    case InteractionType.Absorption:
                        #region Absorption
                        switch (filter)
                        {
                            case InteractionFilter.Npcs:
                                results = (await QueryAsync(q => q.QueryAsync<EncounterCharacterAbilityBreakdownDetail>
                                        (mode == InteractionMode.Ability
                                        ? outgoing // By ability
                                            ? characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Shielding.Done.ByAbility.OnlyNpcs.Totals // 1 player -> many NPCs
                                                : MySQL.Encounter.Character.Npc.Shielding.Done.ByAbility.OtherNpcs.Totals // 1 NPC -> many NPCs
                                            : characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Shielding.Taken.ByAbility.OnlyNpcs.Totals // Many NPCs -> 1 player
                                                : MySQL.Encounter.Character.Npc.Shielding.Taken.ByAbility.OtherNpcs.Totals // Many NPCs -> 1 NPC
                                        : outgoing // By source / target
                                            ? characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Shielding.Done.ByTarget.OnlyNpcs.Totals // 1 player -> many NPCs
                                                : MySQL.Encounter.Character.Npc.Shielding.Done.ByTarget.OtherNpcs.Totals // 1 NPC -> many NPCs
                                            : characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Shielding.Taken.BySource.OnlyNpcs.Totals // Many NPCs -> 1 player
                                                : MySQL.Encounter.Character.Npc.Shielding.Taken.BySource.OtherNpcs.Totals // Many NPCs -> 1 NPC
                                        , new { id, playerId, npcId, totalSeconds }))).ToList();
                                break;
                            case InteractionFilter.Players:
                                results = (await QueryAsync(q => q.QueryAsync<EncounterCharacterAbilityBreakdownDetail>
                                    (mode == InteractionMode.Ability
                                    ? outgoing // By ability
                                        ? characterType == CharacterType.Player
                                            ? MySQL.Encounter.Character.Player.Shielding.Done.ByAbility.OtherPlayers.Totals // 1 player -> many players
                                            : MySQL.Encounter.Character.Npc.Shielding.Done.ByAbility.OnlyPlayers.Totals // 1 NPC -> many players
                                        : characterType == CharacterType.Player
                                            ? MySQL.Encounter.Character.Player.Shielding.Taken.ByAbility.OtherPlayers.Totals // Many players -> 1 player
                                            : MySQL.Encounter.Character.Npc.Shielding.Taken.ByAbility.OnlyPlayers.Totals // Many players -> 1 NPC
                                    : outgoing // By source / target
                                        ? characterType == CharacterType.Player
                                            ? MySQL.Encounter.Character.Player.Shielding.Done.ByTarget.OtherPlayers.Totals // 1 player -> many players
                                            : MySQL.Encounter.Character.Npc.Shielding.Done.ByTarget.OnlyPlayers.Totals // 1 NPC -> many players
                                        : characterType == CharacterType.Player
                                            ? MySQL.Encounter.Character.Player.Shielding.Taken.BySource.OtherPlayers.Totals // Many players -> 1 player
                                            : MySQL.Encounter.Character.Npc.Shielding.Taken.BySource.OnlyPlayers.Totals // Many players -> 1 NPC
                                    , new { id, playerId, npcId, totalSeconds }))).ToList();
                                break;
                            case InteractionFilter.Self:// Only has "by ability" - no point doing "by target" or "by source" on self!
                                results = mode == InteractionMode.Ability
                                    ? (await QueryAsync(q => q.QueryAsync<EncounterCharacterAbilityBreakdownDetail>
                                        (outgoing
                                            ? characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Shielding.Done.ByAbility.OnlySelf.Totals // 1 player
                                                : MySQL.Encounter.Character.Npc.Shielding.Done.ByAbility.OnlySelf.Totals // 1 NPC
                                            : characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Shielding.Taken.ByAbility.OnlySelf.Totals // 1 player
                                                : MySQL.Encounter.Character.Npc.Shielding.Taken.ByAbility.OnlySelf.Totals // 1 NPC
                                            , new { id, playerId, npcId, totalSeconds }))).ToList()
                                    : null;
                                break;
                            case InteractionFilter.All:
                                results = (await QueryAsync(q => q.QueryAsync<EncounterCharacterAbilityBreakdownDetail>
                                        (mode == InteractionMode.Ability
                                        ? outgoing // By ability
                                            ? characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Shielding.Done.ByAbility.AllTargets.Totals // 1 player -> many players and many NPCs
                                                : MySQL.Encounter.Character.Npc.Shielding.Done.ByAbility.AllTargets.Totals // 1 npc -> many players and many NPCs
                                            : characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Shielding.Taken.ByAbility.AllSources.Totals // Many players and many NPCs -> 1 player
                                                : MySQL.Encounter.Character.Npc.Shielding.Taken.ByAbility.AllSources.Totals // Many players and many NPCs -> 1 NPC
                                        : outgoing // By source / target
                                            ? characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Shielding.Done.ByTarget.AllTargets.Totals // 1 player -> many players and many NPCs
                                                : MySQL.Encounter.Character.Npc.Shielding.Done.ByTarget.AllTargets.Totals // 1 npc -> many players and many NPCs
                                            : characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Shielding.Taken.BySource.AllSources.Totals // Many players and many NPCs -> 1 player
                                                : MySQL.Encounter.Character.Npc.Shielding.Taken.BySource.AllSources.Totals // Many players and many NPCs -> 1 NPC
                                        , new { id, playerId, npcId, totalSeconds }))).ToList();
                                break;
                            default:
                                results = null;
                                break;
                        }
                        #endregion
                        break;
                    case InteractionType.Healing:
                        #region Healing
                        switch (filter)
                        {
                            case InteractionFilter.Npcs:
                                results = (await QueryAsync(q => q.QueryAsync<EncounterCharacterAbilityBreakdownDetail>
                                        (mode == InteractionMode.Ability
                                        ? outgoing // By ability
                                            ? characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Healing.Done.ByAbility.OnlyNpcs.Totals // 1 player -> many NPCs
                                                : MySQL.Encounter.Character.Npc.Healing.Done.ByAbility.OtherNpcs.Totals // 1 NPC -> many NPCs
                                            : characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Healing.Taken.ByAbility.OnlyNpcs.Totals // Many NPCs -> 1 player
                                                : MySQL.Encounter.Character.Npc.Healing.Taken.ByAbility.OtherNpcs.Totals // Many NPCs -> 1 NPC
                                        : outgoing // By source / target
                                            ? characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Healing.Done.ByTarget.OnlyNpcs.Totals // 1 player -> many NPCs
                                                : MySQL.Encounter.Character.Npc.Healing.Done.ByTarget.OtherNpcs.Totals // 1 NPC -> many NPCs
                                            : characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Healing.Taken.BySource.OnlyNpcs.Totals // Many NPCs -> 1 player
                                                : MySQL.Encounter.Character.Npc.Healing.Taken.BySource.OtherNpcs.Totals // Many NPCs -> 1 NPC
                                        , new { id, playerId, npcId, totalSeconds }))).ToList();
                                break;
                            case InteractionFilter.Players:
                                results = (await QueryAsync(q => q.QueryAsync<EncounterCharacterAbilityBreakdownDetail>
                                    (mode == InteractionMode.Ability
                                    ? outgoing // By ability
                                        ? characterType == CharacterType.Player
                                            ? MySQL.Encounter.Character.Player.Healing.Done.ByAbility.OtherPlayers.Totals // 1 player -> many players
                                            : MySQL.Encounter.Character.Npc.Healing.Done.ByAbility.OnlyPlayers.Totals // 1 NPC -> many players
                                        : characterType == CharacterType.Player
                                            ? MySQL.Encounter.Character.Player.Healing.Taken.ByAbility.OtherPlayers.Totals // Many players -> 1 player
                                            : MySQL.Encounter.Character.Npc.Healing.Taken.ByAbility.OnlyPlayers.Totals // Many players -> 1 NPC
                                    : outgoing // By source / target
                                        ? characterType == CharacterType.Player
                                            ? MySQL.Encounter.Character.Player.Healing.Done.ByTarget.OtherPlayers.Totals // 1 player -> many players
                                            : MySQL.Encounter.Character.Npc.Healing.Done.ByTarget.OnlyPlayers.Totals // 1 NPC -> many players
                                        : characterType == CharacterType.Player
                                            ? MySQL.Encounter.Character.Player.Healing.Taken.BySource.OtherPlayers.Totals // Many players -> 1 player
                                            : MySQL.Encounter.Character.Npc.Healing.Taken.BySource.OnlyPlayers.Totals // Many players -> 1 NPC
                                    , new { id, playerId, npcId, totalSeconds }))).ToList();
                                break;
                            case InteractionFilter.Self:// Only has "by ability" - no point doing "by target" or "by source" on self!
                                results = mode == InteractionMode.Ability
                                    ? (await QueryAsync(q => q.QueryAsync<EncounterCharacterAbilityBreakdownDetail>
                                        (outgoing
                                            ? characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Healing.Done.ByAbility.OnlySelf.Totals // 1 player
                                                : MySQL.Encounter.Character.Npc.Healing.Done.ByAbility.OnlySelf.Totals // 1 NPC
                                            : characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Healing.Taken.ByAbility.OnlySelf.Totals // 1 player
                                                : MySQL.Encounter.Character.Npc.Healing.Taken.ByAbility.OnlySelf.Totals // 1 NPC
                                            , new { id, playerId, npcId, totalSeconds }))).ToList()
                                    : null;
                                break;
                            case InteractionFilter.All:
                                results = (await QueryAsync(q => q.QueryAsync<EncounterCharacterAbilityBreakdownDetail>
                                        (mode == InteractionMode.Ability
                                        ? outgoing // By ability
                                            ? characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Healing.Done.ByAbility.AllTargets.Totals // 1 player -> many players and many NPCs
                                                : MySQL.Encounter.Character.Npc.Healing.Done.ByAbility.AllTargets.Totals // 1 npc -> many players and many NPCs
                                            : characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Healing.Taken.ByAbility.AllSources.Totals // Many players and many NPCs -> 1 player
                                                : MySQL.Encounter.Character.Npc.Healing.Taken.ByAbility.AllSources.Totals // Many players and many NPCs -> 1 NPC
                                        : outgoing // By source / target
                                            ? characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Healing.Done.ByTarget.AllTargets.Totals // 1 player -> many players and many NPCs
                                                : MySQL.Encounter.Character.Npc.Healing.Done.ByTarget.AllTargets.Totals // 1 npc -> many players and many NPCs
                                            : characterType == CharacterType.Player
                                                ? MySQL.Encounter.Character.Player.Healing.Taken.BySource.AllSources.Totals // Many players and many NPCs -> 1 player
                                                : MySQL.Encounter.Character.Npc.Healing.Taken.BySource.AllSources.Totals // Many players and many NPCs -> 1 NPC
                                        , new { id, playerId, npcId, totalSeconds }))).ToList();
                                break;
                            default:
                                results = null;
                                break;
                        }
                        #endregion
                        break;
                    default:
                        results = null;
                        break;
                }

                return results;
            }
            catch (Exception ex)
            {
                _logger.Debug(string.Format("Error while getting encounter interactions: {0}", ex.Message));
                return new List<EncounterCharacterAbilityBreakdownDetail>();
            }
        }

        // Command

        public async Task<ReturnValue> AddEncounterNpcsAsync(List<EncounterNpc> encounterNpcs)
        {
            var returnValue = new ReturnValue();

            try
            {
                using (var connection = await OpenConnectionAsync())
                {
                    foreach (var encounterNpc in encounterNpcs)
                    {
                        await connection.ExecuteAsync(MySQL.EncounterNpc.Add,
                            new
                            {
                                encounterId = encounterNpc.EncounterId,
                                npcName = encounterNpc.NpcName,
                                npcId = encounterNpc.NpcId
                            });
                    }
                }

                returnValue.Success = true;
            }
            catch (Exception ex)
            {
                returnValue.Message = ex.Message;
            }

            return returnValue;
        }

        public async Task<ReturnValue> AddPlayerEncounterRolesAsync(List<EncounterPlayerRole> playerRoles)
        {
            var returnValue = new ReturnValue();

            try
            {
                using (var connection = await OpenConnectionAsync())
                {
                    foreach (var playerRole in playerRoles)
                    {
                        //@encounterId,@playerId,@playerName,@playerRole,@playerClass
                        await connection.ExecuteAsync(MySQL.EncounterPlayerRole.Add,
                            new
                            {
                                encounterId = playerRole.EncounterId,
                                playerId = playerRole.PlayerId,
                                playerName = playerRole.Name,
                                playerRole = playerRole.Role,
                                playerClass = playerRole.Class
                            });
                    }
                }

                returnValue.Success = true;
            }
            catch (Exception ex)
            {
                returnValue.Message = ex.Message;
            }

            return returnValue;
        }

        public async Task<ReturnValue> UpdateEncounterBurstStatistics(List<EncounterPlayerStatistics> list)
        {
            var returnValue = new ReturnValue();

            try
            {
                using (var connection = await OpenConnectionAsync())
                {
                    var trans = connection.BeginTransaction();

                    foreach (var updateStat in list)
                    {
                        await connection.ExecuteAsync(MySQL.EncounterPlayerStatistics.UpdateBurstStatistics,
                            new
                            {
                                burstDamage1sValue = updateStat.BurstDamage1sValue,
                                burstDamage1sSecond = updateStat.BurstDamage1sSecond,
                                burstDamage5sValue = updateStat.BurstDamage5sValue,
                                burstDamage5sPerSecond = updateStat.BurstDamage5sPerSecond,
                                burstDamage5sStart = updateStat.BurstDamage5sStart,
                                burstDamage5sEnd = updateStat.BurstDamage5sEnd,
                                burstDamage15sValue = updateStat.BurstDamage15sValue,
                                burstDamage15sPerSecond = updateStat.BurstDamage15sPerSecond,
                                burstDamage15sStart = updateStat.BurstDamage15sStart,
                                burstDamage15sEnd = updateStat.BurstDamage15sEnd,
                                burstHealing1sValue = updateStat.BurstHealing1sValue,
                                burstHealing1sSecond = updateStat.BurstHealing1sSecond,
                                burstHealing5sValue = updateStat.BurstHealing5sValue,
                                burstHealing5sPerSecond = updateStat.BurstHealing5sPerSecond,
                                burstHealing5sStart = updateStat.BurstHealing5sStart,
                                burstHealing5sEnd = updateStat.BurstHealing5sEnd,
                                burstHealing15sValue = updateStat.BurstHealing15sValue,
                                burstHealing15sPerSecond = updateStat.BurstHealing15sPerSecond,
                                burstHealing15sStart = updateStat.BurstHealing15sStart,
                                burstHealing15sEnd = updateStat.BurstHealing15sEnd,
                                burstShielding1sValue = updateStat.BurstShielding1sValue,
                                burstShielding1sSecond = updateStat.BurstShielding1sSecond,
                                burstShielding5sValue = updateStat.BurstShielding5sValue,
                                burstShielding5sPerSecond = updateStat.BurstShielding5sPerSecond,
                                burstShielding5sStart = updateStat.BurstShielding5sStart,
                                burstShielding5sEnd = updateStat.BurstShielding5sEnd,
                                burstShielding15sValue = updateStat.BurstShielding15sValue,
                                burstShielding15sPerSecond = updateStat.BurstShielding15sPerSecond,
                                burstShielding15sStart = updateStat.BurstShielding15sStart,
                                burstShielding15sEnd = updateStat.BurstShielding15sEnd,
                                encounterId = updateStat.EncounterId,
                                playerId = updateStat.PlayerId
                            }, trans);
                    }

                    trans.Commit();
                }

                returnValue.Success = true;
            }
            catch (Exception ex)
            {
                returnValue.Message = ex.Message;
            }

            return returnValue;
        }
    }
}
