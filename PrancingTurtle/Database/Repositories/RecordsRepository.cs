using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Database.Models;
using Database.QueryModels.Misc;
using Database.Repositories.Interfaces;
using Logging;

namespace Database.Repositories
{
    public class RecordsRepository : DapperRepositoryBase, IRecordsRepository
    {
        private readonly ILogger _logger;

        public RecordsRepository(IConnectionFactory connectionFactory, ILogger logger)
            : base(connectionFactory)
        {
            _logger = logger;
        }

        public List<EncounterPlayerStatistics> GetTopDpsOverTime(int bossFightId, int difficultyId, string playerClass = "NA")
        {
            string timeElapsed;
            
            var queryList = Query(q => q.Query<EncounterPlayerStatistics, Encounter, Player, EncounterPlayerStatistics>
                (MySQL.Records.GetTopXPSBossFightXDifficultyX("DPS", playerClass),
                    (eps, enc, pl) =>
                    {
                        eps.Encounter = enc;
                        eps.Player = pl;
                        return eps;
                    }, new { bossFightId, difficultyId }), out timeElapsed).ToList();

            var skipEncounterIds = new List<int>();
            var playerGroup = queryList.GroupBy(q => q.PlayerId);
            foreach (var playerEncounterList in playerGroup)
            {
                foreach (var playerEncounterListDate in playerEncounterList.GroupBy(e => e.Encounter.Date.ToShortDateString()))
                {
                    if (playerEncounterListDate.Count() > 1)
                    {
                        // There's more than one kill for this date, so figure out which is the correct one and which ones to skip
                        skipEncounterIds.AddRange(playerEncounterListDate.OrderByDescending(e => e.DPS).Skip(1).Select(skipEnc => skipEnc.EncounterId));
                    }
                }
            }

            return queryList.Where(e => !skipEncounterIds.Contains(e.EncounterId)).ToList();
        }

        public List<EncounterPlayerStatistics> GetTopDpsOverTime(int bossFightId, int difficultyId, int guildId)
        {
            string timeElapsed;

            var queryList = Query(q => q.Query<EncounterPlayerStatistics, Encounter, Player, EncounterPlayerStatistics>
                (MySQL.Records.GetPlayerStatsForBossFightXDifficultyXForGuild,
                    (eps, enc, pl) =>
                    {
                        eps.Encounter = enc;
                        eps.Player = pl;
                        return eps;
                    }, new { bossFightId, difficultyId, guildId }), out timeElapsed).ToList();

            // FOR NOW, DON'T FILTER OUT DUPLICATE ENCOUNTERS

            //var skipEncounterIds = new List<int>();
            //var playerGroup = queryList.GroupBy(q => q.PlayerId);
            //foreach (var playerEncounterList in playerGroup)
            //{
            //    foreach (var playerEncounterListDate in playerEncounterList.GroupBy(e => e.Encounter.Date.ToShortDateString()))
            //    {
            //        if (playerEncounterListDate.Count() > 1)
            //        {
            //            // There's more than one kill for this date, so figure out which is the correct one and which ones to skip
            //            skipEncounterIds.AddRange(playerEncounterListDate.OrderByDescending(e => e.DPS).Skip(1).Select(skipEnc => skipEnc.EncounterId));
            //        }
            //    }
            //}

            //return queryList.Where(e => !skipEncounterIds.Contains(e.EncounterId)).ToList();

            return queryList;
        }

        public List<EncounterPlayerStatistics> GetTopXpsOverTime(int bossFightId, int difficultyId, string playerClass = "NA")
        {
            throw new NotImplementedException();
        }

        public List<EncounterPlayerStatistics> GetTopXpsOverTime(int bossFightId, int difficultyId, int guildId, string xpsType)
        {
            string timeElapsed;

            var queryList = Query(q => q.Query<EncounterPlayerStatistics, Encounter, Player, EncounterPlayerStatistics>
                (MySQL.Records.GetPlayerStatsForBossFightXDifficultyXForGuild,
                    (eps, enc, pl) =>
                    {
                        eps.Encounter = enc;
                        eps.Player = pl;
                        return eps;
                    }, new { bossFightId, difficultyId, guildId }), out timeElapsed).ToList();

            var returnList = new List<EncounterPlayerStatistics>();
            foreach (var playerStatList in queryList.GroupBy(q => q.PlayerId))
            {
                foreach (var playerStatDate in playerStatList.GroupBy(e => e.Encounter.Date.ToShortDateString()))
                {
                    if (playerStatDate.Count() == 1)
                    {
                        returnList.Add(playerStatDate.First());
                    }
                    else
                    {
                        switch (xpsType)
                        {
                            case "APS":
                                returnList.Add(playerStatDate.OrderByDescending(e => e.APS).First());
                                break;
                            case "HPS":
                                returnList.Add(playerStatDate.OrderByDescending(e => e.HPS).First());
                                break;
                            default:
                            case "DPS":
                                returnList.Add(playerStatDate.OrderByDescending(e => e.DPS).First());
                                break;
                        }
                    }
                }
            }

            return returnList;

            //var skipEncounterIds = new List<int>();
            //var playerGroup = queryList.GroupBy(q => q.PlayerId);
            //foreach (var playerEncounterList in playerGroup)
            //{
            //    foreach (var playerEncounterListDate in playerEncounterList.GroupBy(e => e.Encounter.Date.ToShortDateString()))
            //    {
            //        if (playerEncounterListDate.Count() > 1)
            //        {
            //            // There's more than one kill for this date, so figure out which is the correct one and which ones to skip
            //            switch (xpsType)
            //            {
            //                case "APS":
            //                    skipEncounterIds.AddRange(playerEncounterListDate.OrderByDescending(e => e.APS).Skip(1).Select(skipEnc => skipEnc.Id));
            //                    break;
            //                case "HPS":
            //                    skipEncounterIds.AddRange(playerEncounterListDate.OrderByDescending(e => e.HPS).Skip(1).Select(skipEnc => skipEnc.Id));
            //                    break;
            //                default:
            //                case "DPS":
            //                    skipEncounterIds.AddRange(playerEncounterListDate.OrderByDescending(e => e.DPS).Skip(1).Select(skipEnc => skipEnc.Id));
            //                    break;
            //            }
            //        }
            //    }
            //}

            //var skipCount = skipEncounterIds.Count;
            //return queryList.Where(e => !skipEncounterIds.Contains(e.Id)).ToList();
        }

       

        public List<Encounter> GetGuildStatsOverTimeHybrid(int bossFightId, int difficultyId, int guildId)
        {
            string timeElapsed;

            return Query(q => q.Query<Encounter>
                (MySQL.Records.GetCalculatedGuildXpsOverTime, 
                new { guildId, bossFightId, difficultyId }), out timeElapsed).ToList();
        }

        public List<Encounter> GetEncounterDurationOverTime(int bossFightId, int difficultyId, int guildId)
        {
            string timeElapsed;

            return Query(q => q.Query<Encounter>
                (MySQL.Records.GetEncounterDurationOverTime,
                new { guildId, bossFightId, difficultyId }), out timeElapsed).ToList();
        }

        public RankPlayerGuild GetSingleTopDps(int bossFightId, int d = -1)
        {
            return GetSingleTopXps(bossFightId, d, "DPS");
        }

        public RankPlayerGuild GetSingleTopHps(int bossFightId, int d = -1)
        {
            return GetSingleTopXps(bossFightId, d, "HPS");
        }

        public RankPlayerGuild GetSingleTopAps(int bossFightId, int d = -1)
        {
            return GetSingleTopXps(bossFightId, d, "APS");
        }

        public RankPlayerGuild GetSingleTopDpsGuild(int bossFightId, int d = -1)
        {
            return GetSingleTopXpsGuild(bossFightId, d, "DPS");
        }

        public RankPlayerGuild GetSingleTopHpsGuild(int bossFightId, int d = -1)
        {
            return GetSingleTopXpsGuild(bossFightId, d, "HPS");
        }

        public RankPlayerGuild GetSingleTopApsGuild(int bossFightId, int d = -1)
        {
            return GetSingleTopXpsGuild(bossFightId, d, "APS");
        }

        public List<RankPlayerGuild> GetTopGuildDps(int bossFightId, int guildId, int d = -1)
        {
            return GetTopGuildXps(bossFightId, guildId, d, "DPS");
        }

        public List<RankPlayerGuild> GetTopGuildHps(int bossFightId, int guildId, int d = -1)
        {
            return GetTopGuildXps(bossFightId, guildId, d, "HPS");
        }

        public List<RankPlayerGuild> GetTopGuildAps(int bossFightId, int guildId, int d = -1)
        {
            return GetTopGuildXps(bossFightId, guildId, d, "APS");
        }

        private List<RankPlayerGuild> GetTopGuildXps(int bossFightId, int guildId, int d = -1, string xpsType = "DPS")
        {
            string timeElapsed;
            var difficultyId = d;
            if (d == -1)
            {
                var defaultDifficulty = Query(q => q.Query<EncounterDifficulty>(MySQL.EncounterDifficulty.Default), out timeElapsed).SingleOrDefault();
                if (defaultDifficulty != null)
                {
                    difficultyId = defaultDifficulty.Id;
                }
            }
            return Query(q => q.Query<RankPlayerGuild, Player, RankPlayerGuild>
                (MySQL.Records.GetAllGuildXps(xpsType),
                    (rpg, p) =>
                    {
                        rpg.Player = p;
                        return rpg;
                    }, new { bossFightId, difficultyId, guildId }), out timeElapsed).ToList();
        }

        private RankPlayerGuild GetSingleTopXps(int bossFightId, int d = -1, string xpsType = "DPS")
        {
            string timeElapsed;
            var difficultyId = d;
            if (d == -1)
            {
                var defaultDifficulty = Query(q => q.Query<EncounterDifficulty>(MySQL.EncounterDifficulty.Default), out timeElapsed).SingleOrDefault();
                if (defaultDifficulty != null)
                {
                    difficultyId = defaultDifficulty.Id;
                }
            }
            return Query(q => q.Query<RankPlayerGuild, Player, Guild, RankPlayerGuild>
                (MySQL.Records.GetSingleTopXps(xpsType),
                    (rpg, p, g) =>
                    {
                        rpg.Player = p;
                        if (g != null)
                        {
                            rpg.Guild = g;
                        }
                        return rpg;
                    }, new { bossFightId, difficultyId }), out timeElapsed).SingleOrDefault();
        }

        private RankPlayerGuild GetSingleTopXpsGuild(int bossFightId, int d = -1, string xpsType = "DPS")
        {
            string timeElapsed;
            var difficultyId = d;
            if (d == -1)
            {
                var defaultDifficulty = Query(q => q.Query<EncounterDifficulty>(MySQL.EncounterDifficulty.Default), out timeElapsed).SingleOrDefault();
                if (defaultDifficulty != null)
                {
                    difficultyId = defaultDifficulty.Id;
                }
            }
            return Query(q => q.Query<RankPlayerGuild, Guild, RankPlayerGuild>
                (MySQL.Records.GetSingleTopXpsGuild(xpsType),
                    (rpg, g) =>
                    {
                        if (g != null)
                        {
                            rpg.Guild = g;
                        }
                        return rpg;
                    }, new { bossFightId, difficultyId }), out timeElapsed).SingleOrDefault();
        }
    }
}
