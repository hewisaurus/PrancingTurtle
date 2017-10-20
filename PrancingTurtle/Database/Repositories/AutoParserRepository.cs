using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Common;
using Dapper;
using Database.Models;
using Database.QueryModels.Misc;
using Database.QueryModels.Parser;
using Database.Repositories.Interfaces;
using Logging;
using MySql.Data.MySqlClient;

namespace Database.Repositories
{
    public class AutoParserRepository : DapperRepositoryBase, IAutoParserRepository
    {
        private readonly ILogger _logger;

        public AutoParserRepository(IConnectionFactory connectionFactory, ILogger logger)
            : base(connectionFactory)
        {
            _logger = logger;

        }

        public SessionLogInfo GetInfoByToken(string token)
        {
            string timeElapsed;
            return
                Query(q => q.Query<SessionLogInfo>(MySQL.AutoParser.SingleSessionLogInfo, new { token }), out timeElapsed)
                    .SingleOrDefault();
        }

        public IEnumerable<string> GetPlayerIdsNotInDb(IEnumerable<string> playerIds)
        {
            try
            {
                string timeElapsed;
                // Clear table
                Execute(e => e.Execute(MySQL.AutoParser.ClearTemporaryPlayerTable), out timeElapsed);
                string insertTempIds = playerIds.Aggregate("", (current, playerId) => current + string.Format(" INSERT INTO AutoParserPlayerChecking(PlayerId) VALUES('{0}');", playerId));
                // Execute the insert to temporary table
                Execute(e => e.Execute(insertTempIds), out timeElapsed);
                // Query the temporary table against Player table for the list
                var results =
                    Query(q => q.Query<string>(MySQL.AutoParser.ComparePlayerTableToTemporary), out timeElapsed)
                        .ToList();
                // Clear table
                Execute(e => e.Execute(MySQL.AutoParser.ClearTemporaryPlayerTable), out timeElapsed);

                return results;
            }
            catch (Exception ex)
            {
                _logger.Debug(string.Format("An error occurred while trying to get the player IDs missing from the database: {0}", ex.Message));
                return null;
            }
        }

        public IEnumerable<Player> GetPlayersById(IEnumerable<string> playerIds)
        {
            try
            {
                string timeElapsed;
                return Query(q => q.Query<Player>(MySQL.AutoParser.GetPlayersByIds,
                    new { @playerIds = playerIds.ToArray() }), out timeElapsed);
            }
            catch (Exception ex)
            {
                _logger.Debug(string.Format("An error occurred while trying to get players from the database: {0}", ex.Message));
                return null;
            }
        }

        public IEnumerable<ShortAbility> GetShortAbilities(IEnumerable<long> abilityIds)
        {
            try
            {
                string timeElapsed;
                return Query(q => q.Query<ShortAbility>(MySQL.AutoParser.GetShortAbilities,
                    new { @abilityIds = abilityIds.ToArray() }), out timeElapsed);
            }
            catch (Exception ex)
            {
                _logger.Debug(string.Format("An error occurred while trying to get the abilities from the database: {0}", ex.Message));
                return null;
            }
        }

        public IEnumerable<long> GetAbilityIdsNotInDb(IEnumerable<long> abilityIds)
        {
            try
            {
                string timeElapsed;
                // Clear table
                Execute(e => e.Execute(MySQL.AutoParser.ClearTemporaryAbilityTable), out timeElapsed);
                string insertTempIds = abilityIds.Aggregate("", (current, abilityId) => current + string.Format(" INSERT INTO AutoParserAbilityChecking(AbilityId) VALUES({0});", abilityId));
                // Execute the insert to temporary table
                Execute(e => e.Execute(insertTempIds), out timeElapsed);
                // Query the temporary table against Player table for the list
                var results =
                    Query(q => q.Query<long>(MySQL.AutoParser.CompareAbilityTableToTemporary), out timeElapsed)
                        .ToList();
                // Clear table
                Execute(e => e.Execute(MySQL.AutoParser.ClearTemporaryAbilityTable), out timeElapsed);

                return results;
            }
            catch (Exception ex)
            {
                _logger.Debug(string.Format("An error occurred while trying to get the ability IDs missing from the database: {0}", ex.Message));
                return null;
            }
        }

        public IEnumerable<long> GetAbilityIdsWithoutDamageTypes(List<long> abilityIds)
        {
            try
            {
                string timeElapsed;
                return Query(q => q.Query<long>(MySQL.AutoParser.GetAbilityIdsWithNoDamageTypeByAbilityIds,
                    new { @abilityIds = abilityIds.ToArray() }), out timeElapsed);
            }
            catch (Exception ex)
            {
                _logger.Debug(string.Format("An error occurred while trying to get the abilities with no damage type from the database: {0}", ex.Message));
                return null;
            }
        }

        public ReturnValue AddPlayers(List<Player> players)
        {
            var returnValue = new ReturnValue();
            try
            {
                DapperDb dapperDb = DapperDb.Init(OpenConnection(), 3, false);

                foreach (var player in players)
                {
                    dapperDb.PlayerTable.Insert(
                        new //Player()
                        {
                            Name = player.Name,
                            PlayerId = player.PlayerId,
                            Shard = player.Shard
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

        public ReturnValue UpdatePlayers(List<Player> players)
        {
            var returnValue = new ReturnValue();
            try
            {
                foreach (var player in players)
                {
                    string timeElapsed;
                    Execute(
                        q => q.Execute(MySQL.AutoParser.PlayerUpdater, new { @name = player.Name, @shard = player.Shard, @playerId = player.PlayerId }),
                        out timeElapsed);
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

        public ReturnValue AddAbilities(List<Ability> abilities)
        {
            var returnValue = new ReturnValue();
            try
            {
                DapperDb dapperDb = DapperDb.Init(OpenConnection(), 3, false);

                foreach (var ability in abilities)
                {
                    dapperDb.AbilityTable.Insert(
                        new //Ability()
                        {
                            Name = ability.Name,
                            AbilityId = ability.AbilityId,
                            Description = ability.Description,
                            DamageType = ability.DamageType
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

        public ReturnValue UpdateAbilityDamageTypes(Dictionary<long, string> abilities)
        {
            var returnValue = new ReturnValue();
            try
            {
                foreach (var ability in abilities)
                {
                    string timeElapsed;
                    var ability1 = ability;
                    Execute(
                        q => q.Execute(MySQL.AutoParser.AbilityDamageTypeUpdaer,
                            new { @abilityId = ability1.Key, @damageType = ability1.Value }),
                        out timeElapsed);
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

        public IEnumerable<BossFight> GetBossFights(string fightName)
        {
            try
            {
                string timeElapsed;

                return Query(s => s.Query<BossFight, Instance, BossFight>
                    (MySQL.AutoParser.GetBossFightsByName,
                    (bf, i) =>
                    {
                        bf.Instance = i;
                        return bf;
                    }, new { fightName }), out timeElapsed);
            }
            catch (Exception ex)
            {
                _logger.Debug(string.Format("An error occurred while trying to get the BossFight for this encounter: {0}", ex.Message));
                return null;
            }
        }

        //Added 20160125
        public Dictionary<int, string> GetBossFights()
        {
            try
            {
                string timeElapsed;
                var bossFights =
                    Query(q => q.Query<BossFight>(MySQL.AutoParser.ListBossFights), out timeElapsed).ToList();
                return bossFights.ToDictionary(bossFight => bossFight.Id, bossFight => bossFight.Name);
            }
            catch (Exception ex)
            {
                _logger.Debug(string.Format("An error occurred while retrieving boss fights: {0}", ex.Message));
                return new Dictionary<int, string>();
            }
        }

        public int GetEncounterId(int uploaderId, DateTime encounterDate, int bossFightId, TimeSpan duration)
        {
            try
            {
                string timeElapsed;

                return Query(s => s.Query<int>(MySQL.AutoParser.GetEncounterIdFromDetails,
                    new { @date = encounterDate, bossFightId, @duration = duration.ToString(), uploaderId }), out timeElapsed).SingleOrDefault();
            }
            catch (Exception ex)
            {
                _logger.Debug(string.Format("Error while getting encounter ID: {0}", ex.Message));
                return 0;
            }
        }

        public ReturnValue AddEncounter(Encounter encounter)
        {
            var returnValue = new ReturnValue();
            try
            {
                DapperDb dapperDb = DapperDb.Init(OpenConnection(), 3, false);

                var result = dapperDb.EncounterTable.Insert(
                    new //Models.Encounter()
                    {
                        BossFightId = encounter.BossFightId,
                        Date = encounter.Date,
                        Duration = encounter.Duration.ToString(),
                        SuccessfulKill = encounter.SuccessfulKill,
                        ValidForRanking = encounter.ValidForRanking,
                        UploaderId = encounter.UploaderId,
                        GuildId = encounter.GuildId,
                        IsPublic = encounter.IsPublic,
                        EncounterDifficultyId = encounter.EncounterDifficultyId
                    });

                returnValue.Message = result.ToString();
                returnValue.Success = true;
                return returnValue;
            }
            catch (Exception ex)
            {
                returnValue.Message = ex.Message;
                return returnValue;
            }
        }

        public EncounterInformation GetInfo(int encounterId)
        {
            try
            {
                var info = new EncounterInformation();

                using (var connection = OpenConnection())
                {
                    using (var result = connection.QueryMultiple(MySQL.AutoParser.GetEncounterInfo, new { @id = encounterId }))
                    {
                        info.HasOverviewRecords = result.Read<long>().SingleOrDefault() == 1;
                        info.HasBuffEventRecords = result.Read<long>().SingleOrDefault() == 1;
                        info.HasBuffUptimeRecords = result.Read<long>().SingleOrDefault() == 1;
                        info.HasDeathRecords = result.Read<long>().SingleOrDefault() == 1;
                        info.HasDamageRecords = result.Read<long>().SingleOrDefault() == 1;
                        info.HasHealingRecords = result.Read<long>().SingleOrDefault() == 1;
                        info.HasShieldingRecords = result.Read<long>().SingleOrDefault() == 1;
                        info.HasDebuffActionRecords = result.Read<long>().SingleOrDefault() == 1;
                        info.HasBuffActionRecords = result.Read<long>().SingleOrDefault() == 1;
                        info.HasNpcCastRecords = result.Read<long>().SingleOrDefault() == 1;
                    }
                }

                return info;
            }
            catch (Exception ex)
            {
                _logger.Debug(string.Format("Error while getting encounter information: {0}", ex.Message));
                return null;
            }
        }

        public List<int> GetEncounterIdsForSession(int sessionId)
        {
            string timeElapsed;
            return
                Query(q => q.Query<int>(MySQL.AutoParser.GetExistingEncounterIdsForSession, new { sessionId }),
                    out timeElapsed).ToList();
        }

        public List<string> GetIgnoredEncounterNames()
        {
            string timeElapsed;
            return Query(q => q.Query<string>(MySQL.AutoParser.GetIgnoredEncounters), out timeElapsed).ToList();
        }

        public DateTime GetFirstEncounterDateForSession(int sessionId)
        {
            string timeElapsed;
            return
                Query(q => q.Query<DateTime>(MySQL.AutoParser.GetFirstEncounterDateForSession, new { sessionId }),
                    out timeElapsed).SingleOrDefault();
        }

        public bool DifficultyRecordsExist(int bossFightId)
        {
            string timeElapsed;
            return Query(q => q.Query<long>(MySQL.BossFightDifficulty.DifficultyRecordsExist, new { bossFightId }), out timeElapsed).SingleOrDefault() > 0;
        }

        public List<BossFightDifficulty> GetDifficultySettings(int bossFightId)
        {
            string timeElapsed;
            return Query(q => q.Query<BossFightDifficulty, EncounterDifficulty, BossFightDifficulty>
                (MySQL.BossFightDifficulty.GetAll,
                    (bfd, ed) =>
                    {
                        bfd.EncounterDifficulty = ed;
                        return bfd;
                    }, new { bossFightId }), out timeElapsed).ToList();
        }

        public int GetDefaultDifficultyId()
        {
            string timeElapsed;
            var defaultDifficulty = Query(q => q.Query<EncounterDifficulty>(MySQL.EncounterDifficulty.Default), out timeElapsed).SingleOrDefault();
            if (defaultDifficulty == null) return 0;
            return defaultDifficulty.Id;
        }

        public List<NpcDeath> GetExistingNpcDeaths(List<string> npcNames)
        {
            string timeElapsed;
            return
                Query(q => q.Query<NpcDeath>(MySQL.NpcDeath.GetAllInList, new { @names = npcNames.ToArray() }),
                    out timeElapsed).ToList();
        }

        public ReturnValue AddDamageDone(List<DamageDone> damageDone)
        {
            var returnValue = new ReturnValue();

            try
            {
                // Create a CSV of the data, and then dump it into the database
                string randomFilename = AuthEncryption.RandomFilename() + ".csv";
                while (true)
                {
                    if (!File.Exists("databaseImport\\" + randomFilename))
                    {
                        break;
                    }
                    randomFilename = AuthEncryption.RandomFilename() + ".csv";
                }
                string filePath = "databaseImport\\" + randomFilename;
                using (var outFile = File.CreateText(filePath))
                {
                    outFile.WriteLine("SourcePlayerId,SourceNpcName,SourceNpcId,SourcePetName,TargetPlayerId,TargetNpcName,TargetNpcId,TargetPetName," +
                        "EncounterId,AbilityId,TotalDamage,EffectiveDamage,CriticalHit,SecondsElapsed,OrderWithinSecond,BlockedAmount,AbsorbedAmount, " +
                        "DeflectedAmount,IgnoredAmount,InterceptedAmount,OverkillAmount,Dodged");
                    foreach (var dmgEntry in damageDone)
                    {
                        List<object> lineList = new List<object>()
                    {
                        dmgEntry.SourcePlayerId, dmgEntry.SourceNpcName, dmgEntry.SourceNpcId, dmgEntry.SourcePetName,
                        dmgEntry.TargetPlayerId, dmgEntry.TargetNpcName, dmgEntry.TargetNpcId, dmgEntry.TargetPetName,
                        dmgEntry.EncounterId, dmgEntry.AbilityId, dmgEntry.TotalDamage, dmgEntry.EffectiveDamage, dmgEntry.CriticalHit,
                        dmgEntry.SecondsElapsed, dmgEntry.OrderWithinSecond, dmgEntry.BlockedAmount, dmgEntry.AbsorbedAmount,
                        dmgEntry.DeflectedAmount, dmgEntry.IgnoredAmount, dmgEntry.InterceptedAmount, dmgEntry.OverkillAmount, dmgEntry.Dodged
                    };

                        outFile.WriteLine(EncapLine(lineList));
                    }
                }

                using (var connection = new MySqlConnection(_connectionString))
                {
                    MySqlBulkLoader bulkLoader = new MySqlBulkLoader(connection)
                    {
                        TableName = "DamageDone",
                        FieldTerminator = ",",
                        LineTerminator = "\r\n",
                        FieldQuotationCharacter = '"',
                        FieldQuotationOptional = false,
                        FileName = filePath,
                        NumberOfLinesToSkip = 1,
                        Columns = { "SourcePlayerId", "SourceNpcName", "SourceNpcId", "SourcePetName", "TargetPlayerId", "TargetNpcName", "TargetNpcId", "TargetPetName",
                            "EncounterId", "AbilityId", "TotalDamage", "EffectiveDamage", "CriticalHit", "SecondsElapsed", "OrderWithinSecond", "BlockedAmount",
                            "AbsorbedAmount", "DeflectedAmount", "IgnoredAmount", "InterceptedAmount", "OverkillAmount", "Dodged" }
                    };

                    int count = bulkLoader.Load();
                    returnValue.Success = true;
                    returnValue.Message = count.ToString();
                }

                try
                {
                    File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    // Catch this?
                }

                return returnValue;
            }
            catch (Exception ex)
            {
                returnValue.Message = ex.Message;
                return returnValue;
            }
        }

        public ReturnValue AddHealingDone(List<HealingDone> healingDone)
        {
            var returnValue = new ReturnValue();

            try
            {
                // Create a CSV of the data, and then dump it into the database
                string randomFilename = AuthEncryption.RandomFilename() + ".csv";
                while (true)
                {
                    if (!File.Exists("databaseImport\\" + randomFilename))
                    {
                        break;
                    }
                    randomFilename = AuthEncryption.RandomFilename() + ".csv";
                }
                string filePath = "databaseImport\\" + randomFilename;
                using (var outFile = File.CreateText(filePath))
                {
                    outFile.WriteLine("SourcePlayerId,SourceNpcName,SourceNpcId,SourcePetName,TargetPlayerId,TargetNpcName,TargetNpcId,TargetPetName," +
                        "EncounterId,AbilityId,TotalHealing,EffectiveHealing,CriticalHit,SecondsElapsed,OrderWithinSecond,OverhealAmount");
                    foreach (var healEntry in healingDone)
                    {
                        List<object> lineList = new List<object>()
                    {
                        healEntry.SourcePlayerId, healEntry.SourceNpcName, healEntry.SourceNpcId, healEntry.SourcePetName,
                        healEntry.TargetPlayerId, healEntry.TargetNpcName, healEntry.TargetNpcId, healEntry.TargetPetName,
                        healEntry.EncounterId, healEntry.AbilityId, healEntry.TotalHealing, healEntry.EffectiveHealing, healEntry.CriticalHit,
                        healEntry.SecondsElapsed, healEntry.OrderWithinSecond, healEntry.OverhealAmount
                    };

                        outFile.WriteLine(EncapLine(lineList));
                    }
                }

                using (var connection = new MySqlConnection(_connectionString))
                {
                    MySqlBulkLoader bulkLoader = new MySqlBulkLoader(connection)
                    {
                        TableName = "HealingDone",
                        FieldTerminator = ",",
                        LineTerminator = "\r\n",
                        FieldQuotationCharacter = '"',
                        FieldQuotationOptional = false,
                        FileName = filePath,
                        NumberOfLinesToSkip = 1,
                        Columns = { "SourcePlayerId", "SourceNpcName", "SourceNpcId", "SourcePetName", "TargetPlayerId", "TargetNpcName", "TargetNpcId", "TargetPetName",
                            "EncounterId", "AbilityId", "TotalHealing", "EffectiveHealing", "CriticalHit", "SecondsElapsed", "OrderWithinSecond", "OverhealAmount" }
                    };

                    int count = bulkLoader.Load();
                    returnValue.Success = true;
                    returnValue.Message = count.ToString();
                }

                try
                {
                    File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    // Catch this?
                }

                return returnValue;
            }
            catch (Exception ex)
            {
                returnValue.Message = ex.Message;
                return returnValue;
            }
        }

        public ReturnValue AddShieldingDone(List<ShieldingDone> shieldingDone)
        {
            var returnValue = new ReturnValue();

            try
            {
                // Create a CSV of the data, and then dump it into the database
                string randomFilename = AuthEncryption.RandomFilename() + ".csv";
                while (true)
                {
                    if (!File.Exists("databaseImport\\" + randomFilename))
                    {
                        break;
                    }
                    randomFilename = AuthEncryption.RandomFilename() + ".csv";
                }
                string filePath = "databaseImport\\" + randomFilename;
                using (var outFile = File.CreateText(filePath))
                {
                    outFile.WriteLine("SourcePlayerId,SourceNpcName,SourceNpcId,SourcePetName,TargetPlayerId,TargetNpcName,TargetNpcId,TargetPetName," +
                        "EncounterId,AbilityId,ShieldValue,CriticalHit,SecondsElapsed,OrderWithinSecond");
                    foreach (var shieldEntry in shieldingDone)
                    {
                        List<object> lineList = new List<object>()
                    {
                        shieldEntry.SourcePlayerId, shieldEntry.SourceNpcName, shieldEntry.SourceNpcId, shieldEntry.SourcePetName,
                        shieldEntry.TargetPlayerId, shieldEntry.TargetNpcName, shieldEntry.TargetNpcId, shieldEntry.TargetPetName,
                        shieldEntry.EncounterId, shieldEntry.AbilityId, shieldEntry.ShieldValue, shieldEntry.CriticalHit,
                        shieldEntry.SecondsElapsed, shieldEntry.OrderWithinSecond
                    };

                        outFile.WriteLine(EncapLine(lineList));
                    }
                }

                using (var connection = new MySqlConnection(_connectionString))
                {
                    MySqlBulkLoader bulkLoader = new MySqlBulkLoader(connection)
                    {
                        TableName = "ShieldingDone",
                        FieldTerminator = ",",
                        LineTerminator = "\r\n",
                        FieldQuotationCharacter = '"',
                        FieldQuotationOptional = false,
                        FileName = filePath,
                        NumberOfLinesToSkip = 1,
                        Columns = { "SourcePlayerId", "SourceNpcName", "SourceNpcId", "SourcePetName", "TargetPlayerId", "TargetNpcName", "TargetNpcId", "TargetPetName",
                            "EncounterId", "AbilityId", "ShieldValue", "CriticalHit", "SecondsElapsed", "OrderWithinSecond"}
                    };

                    int count = bulkLoader.Load();
                    returnValue.Success = true;
                    returnValue.Message = count.ToString();
                }

                try
                {
                    File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    // Catch this?
                }

                return returnValue;
            }
            catch (Exception ex)
            {
                returnValue.Message = ex.Message;
                return returnValue;
            }
        }

        public ReturnValue AddEncounterDeaths(List<EncounterDeath> encounterDeaths)
        {
            var returnValue = new ReturnValue();

            try
            {
                // Create a CSV of the data, and then dump it into the database
                string randomFilename = AuthEncryption.RandomFilename() + ".csv";
                while (true)
                {
                    if (!File.Exists("databaseImport\\" + randomFilename))
                    {
                        break;
                    }
                    randomFilename = AuthEncryption.RandomFilename() + ".csv";
                }
                string filePath = "databaseImport\\" + randomFilename;
                using (var outFile = File.CreateText(filePath))
                {
                    outFile.WriteLine("SourcePlayerId,SourceNpcName,SourceNpcId,SourcePetName,TargetPlayerId,TargetNpcName,TargetNpcId,TargetPetName," +
                        "EncounterId,AbilityId,TotalDamage,OverkillValue,SecondsElapsed,OrderWithinSecond");
                    foreach (var death in encounterDeaths)
                    {
                        List<object> lineList = new List<object>()
                    {
                        death.SourcePlayerId, death.SourceNpcName, death.SourceNpcId, death.SourcePetName,
                        death.TargetPlayerId, death.TargetNpcName, death.TargetNpcId, death.TargetPetName,
                        death.EncounterId, death.AbilityId, death.TotalDamage, death.OverkillValue,
                        death.SecondsElapsed, death.OrderWithinSecond
                    };

                        outFile.WriteLine(EncapLine(lineList));
                    }
                }

                using (var connection = new MySqlConnection(_connectionString))
                {
                    MySqlBulkLoader bulkLoader = new MySqlBulkLoader(connection)
                    {
                        TableName = "EncounterDeath",
                        FieldTerminator = ",",
                        LineTerminator = "\r\n",
                        FieldQuotationCharacter = '"',
                        FieldQuotationOptional = false,
                        FileName = filePath,
                        NumberOfLinesToSkip = 1,
                        Columns = { "SourcePlayerId", "SourceNpcName", "SourceNpcId", "SourcePetName", "TargetPlayerId", "TargetNpcName", "TargetNpcId", "TargetPetName",
                            "EncounterId", "AbilityId", "TotalDamage", "OverkillValue", "SecondsElapsed", "OrderWithinSecond"}
                    };

                    int count = bulkLoader.Load();
                    returnValue.Success = true;
                    returnValue.Message = count.ToString();
                }

                try
                {
                    File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    // Catch this?
                }

                return returnValue;
            }
            catch (Exception ex)
            {
                returnValue.Message = ex.Message;
                return returnValue;
            }
        }

        public ReturnValue AddDebuffAction(List<EncounterDebuffAction> debuffActions)
        {
            var returnValue = new ReturnValue();

            try
            {
                // Create a CSV of the data, and then dump it into the database
                string randomFilename = AuthEncryption.RandomFilename() + ".csv";
                while (true)
                {
                    if (!File.Exists("databaseImport\\" + randomFilename))
                    {
                        break;
                    }
                    randomFilename = AuthEncryption.RandomFilename() + ".csv";
                }
                string filePath = "databaseImport\\" + randomFilename;
                using (var outFile = File.CreateText(filePath))
                {
                    outFile.WriteLine("EncounterId,AbilityId,DebuffName,SourceId,SourceName,SourceType,TargetId,TargetName,TargetType," +
                        "SecondDebuffWentUp,SecondDebuffWentDown");
                    foreach (var action in debuffActions)
                    {
                        List<object> lineList = new List<object>()
                    {
                        action.EncounterId, action.AbilityId, action.DebuffName, action.SourceId,
                        action.SourceName, action.SourceType, action.TargetId, action.TargetName,
                        action.TargetType, action.SecondDebuffWentUp, action.SecondDebuffWentDown
                    };

                        outFile.WriteLine(EncapLine(lineList));
                    }
                }

                using (var connection = new MySqlConnection(_connectionString))
                {
                    MySqlBulkLoader bulkLoader = new MySqlBulkLoader(connection)
                    {
                        TableName = "EncounterDebuffAction",
                        FieldTerminator = ",",
                        LineTerminator = "\r\n",
                        FieldQuotationCharacter = '"',
                        FieldQuotationOptional = false,
                        FileName = filePath,
                        NumberOfLinesToSkip = 1,
                        Columns = { "EncounterId", "AbilityId", "DebuffName", "SourceId", "SourceName", "SourceType", "TargetId", "TargetName",
                            "TargetType", "SecondDebuffWentUp", "SecondDebuffWentDown"}
                    };

                    int count = bulkLoader.Load();
                    returnValue.Success = true;
                    returnValue.Message = count.ToString();
                }

                try
                {
                    File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    // Catch this?
                }

                return returnValue;
            }
            catch (Exception ex)
            {
                returnValue.Message = ex.Message;
                return returnValue;
            }
        }

        public ReturnValue AddBuffAction(List<EncounterBuffAction> buffActions)
        {
            var returnValue = new ReturnValue();

            try
            {
                // Create a CSV of the data, and then dump it into the database
                string randomFilename = AuthEncryption.RandomFilename() + ".csv";
                while (true)
                {
                    if (!File.Exists("databaseImport\\" + randomFilename))
                    {
                        break;
                    }
                    randomFilename = AuthEncryption.RandomFilename() + ".csv";
                }
                string filePath = "databaseImport\\" + randomFilename;
                using (var outFile = File.CreateText(filePath))
                {
                    outFile.WriteLine("EncounterId,AbilityId,BuffName,SourceId,SourceName,SourceType,TargetId,TargetName,TargetType," +
                        "SecondBuffWentUp,SecondBuffWentDown");
                    foreach (var action in buffActions)
                    {
                        List<object> lineList = new List<object>()
                    {
                        action.EncounterId, action.AbilityId, action.BuffName, action.SourceId,
                        action.SourceName, action.SourceType, action.TargetId, action.TargetName,
                        action.TargetType, action.SecondBuffWentUp, action.SecondBuffWentDown
                    };

                        if (action.AbilityId == 0)
                        {
                            string something = "blah";
                        }

                        outFile.WriteLine(EncapLine(lineList));
                    }
                }

                using (var connection = new MySqlConnection(_connectionString))
                {
                    MySqlBulkLoader bulkLoader = new MySqlBulkLoader(connection)
                    {
                        TableName = "EncounterBuffAction",
                        FieldTerminator = ",",
                        LineTerminator = "\r\n",
                        FieldQuotationCharacter = '"',
                        FieldQuotationOptional = false,
                        FileName = filePath,
                        NumberOfLinesToSkip = 1,
                        Columns = { "EncounterId", "AbilityId", "BuffName", "SourceId", "SourceName", "SourceType", "TargetId", "TargetName",
                            "TargetType", "SecondBuffWentUp", "SecondBuffWentDown"}
                    };

                    int count = bulkLoader.Load();
                    returnValue.Success = true;
                    returnValue.Message = count.ToString();
                }

                try
                {
                    File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    // Catch this?
                }

                return returnValue;
            }
            catch (Exception ex)
            {
                returnValue.Message = ex.Message;
                return returnValue;
            }
        }

        public ReturnValue AddNpcCasts(List<EncounterNpcCast> npcCasts)
        {
            var returnValue = new ReturnValue();

            try
            {
                // Create a CSV of the data, and then dump it into the database
                string randomFilename = AuthEncryption.RandomFilename() + ".csv";
                while (true)
                {
                    if (!File.Exists("databaseImport\\" + randomFilename))
                    {
                        break;
                    }
                    randomFilename = AuthEncryption.RandomFilename() + ".csv";
                }
                string filePath = "databaseImport\\" + randomFilename;
                using (var outFile = File.CreateText(filePath))
                {
                    outFile.WriteLine("EncounterId,AbilityName,NpcId,NpcName");
                    foreach (var cast in npcCasts)
                    {
                        List<object> lineList = new List<object>() { cast.EncounterId, cast.AbilityName, cast.NpcId, cast.NpcName };
                        outFile.WriteLine(EncapLine(lineList));
                    }
                }

                using (var connection = new MySqlConnection(_connectionString))
                {
                    MySqlBulkLoader bulkLoader = new MySqlBulkLoader(connection)
                    {
                        TableName = "EncounterNpcCast",
                        FieldTerminator = ",",
                        LineTerminator = "\r\n",
                        FieldQuotationCharacter = '"',
                        FieldQuotationOptional = false,
                        FileName = filePath,
                        NumberOfLinesToSkip = 1,
                        Columns = { "EncounterId", "AbilityName", "NpcId", "NpcName" }
                    };

                    int count = bulkLoader.Load();
                    returnValue.Success = true;
                    returnValue.Message = count.ToString();
                }

                try
                {
                    File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    // Catch this?
                }

                return returnValue;
            }
            catch (Exception ex)
            {
                returnValue.Message = ex.Message;
                return returnValue;
            }
        }

        public ReturnValue AddSessionEncounters(List<SessionEncounter> sessionEncounters)
        {
            var returnValue = new ReturnValue();
            try
            {
                string timeElapsed;

                DapperDb dapperDb = DapperDb.Init(OpenConnection(), 3, false);

                foreach (var sessionEncounter in sessionEncounters)
                {
                    dapperDb.SessionEncounterTable.Insert(
                        new //SessionEncounter()
                        {
                            SessionId = sessionEncounter.SessionId,
                            EncounterId = sessionEncounter.EncounterId
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

        public ReturnValue AddNpcDeath(NpcDeath npcDeath)
        {
            var returnValue = new ReturnValue();
            try
            {
                DapperDb dapperDb = DapperDb.Init(OpenConnection(), 3, false);

                var newId = dapperDb.NpcDeathTable.Insert(
                    new //NpcDeath()
                    {
                        Name = npcDeath.Name,
                        Deaths = npcDeath.Deaths
                    });

                returnValue.Success = true;
                returnValue.Message = newId.ToString();
                return returnValue;
            }
            catch (Exception ex)
            {
                returnValue.Message = ex.Message;
                return returnValue;
            }
        }

        public ReturnValue UpdateSessionLogPostParse(int sessionLogId, long totalPlayedTime, long logSize, long totalLines)
        {
            var returnValue = new ReturnValue();
            try
            {
                string timeElapsed;

                var result = Execute(e => e.Execute(MySQL.AutoParser.SessionLogPostParseUpdater,
                    new { logSize, totalPlayedTime, sessionLogId, totalLines }), out timeElapsed);

                returnValue.Success = true;
                returnValue.Message = result.ToString();
                return returnValue;
            }
            catch (Exception ex)
            {
                returnValue.Message = ex.Message;
                return returnValue;
            }
        }

        public ReturnValue UpdateSessionPostParse(int sessionId)
        {
            // Get the date of the first encounter from SessionEncounter
            // Get the total log size and total played time from SessionLog
            string timeElapsed;
            var returnValue = new ReturnValue();
            try
            {
                var firstDate = GetFirstEncounterDateForSession(sessionId);
                var lastDate =
                    Query(q => q.Query<DateTime>(MySQL.AutoParser.GetLastEncounterDateForSession, new { sessionId }),
                        out timeElapsed).SingleOrDefault();
                var totals =
                    Query(
                        q => q.Query<SessionLogTotals>(MySQL.AutoParser.GetSessionTotalsFromSessionLogs, new { sessionId }),
                        out timeElapsed).Single();


                if (firstDate == new DateTime() || lastDate == new DateTime())
                {
                    // This shouldn't happen, but if it does, update the totals without modifying the date and duration
                    var result = Execute(e => e.Execute(MySQL.AutoParser.SessionPostParseUpdaterNoDate,
                        new
                        {
                            @sessionSize = totals.TotalLogSize,
                            @totalPlayTime = totals.TotalPlayedTime,
                            sessionId
                        }),
                        out timeElapsed);
                    if (result == 0)
                    {
                        returnValue.Message = string.Format("There was an error updating session {0}", sessionId);
                    }
                    else
                    {
                        returnValue.Success = true;
                    }
                }
                else
                {
                    var sessionDuration = lastDate - firstDate;
                    var result = Execute(e => e.Execute(MySQL.AutoParser.SessionPostParseUpdater,
                        new
                        {
                            @sessionSize = totals.TotalLogSize,
                            @totalPlayTime = totals.TotalPlayedTime,
                            @date = firstDate,
                            @duration = sessionDuration,
                            sessionId
                        }),
                        out timeElapsed);
                    if (result == 0)
                    {
                        returnValue.Message = string.Format("There was an error updating session {0}", sessionId);
                    }
                    else
                    {
                        returnValue.Success = true;
                    }
                }

                return returnValue;
            }
            catch (Exception ex)
            {
                returnValue.Message = ex.Message;
                return returnValue;
            }
        }

        public ReturnValue UpdateNpcDeath(NpcDeath npcDeath)
        {
            var returnValue = new ReturnValue();
            try
            {
                string timeElapsed;

                var result = Execute(e => e.Execute(MySQL.NpcDeath.UpdateItem,
                    new { @deaths = npcDeath.Deaths, @name = npcDeath.Name }), out timeElapsed);

                returnValue.Success = true;
                returnValue.Message = result.ToString();
                return returnValue;
            }
            catch (Exception ex)
            {
                returnValue.Message = ex.Message;
                return returnValue;
            }
        }

        private static string EncapLine(List<object> lineList)
        {
            List<string> lineListOut = new List<string>();

            foreach (var obj in lineList)
            {
                lineListOut.Add(obj == null
                    ? EncapField("\\N")
                    : string.IsNullOrEmpty(obj.ToString())
                        ? EncapField("\\N")
                        : EncapField(obj.ToString().Replace("True", "1").Replace("False", "0")));
            }

            return string.Join(",", lineListOut);
        }

        private static string EncapField(object field)
        {
            return string.Format("\"{0}\"", field);
        }
    }
}
