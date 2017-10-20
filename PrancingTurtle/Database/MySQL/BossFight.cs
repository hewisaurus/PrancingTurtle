using System;
using Common;

namespace Database.MySQL
{
    public static class BossFight
    {
        public static string GetKillsWipesStatistics
        {
            get
            {
                return "SELECT SUM(CASE WHEN SuccessfulKill = 1 THEN 1 ELSE 0 END) AS Kills, " +
                       "SUM(CASE WHEN SuccessfulKill = 0 THEN 1 ELSE 0 END) AS Wipes " +
                       "FROM Encounter WHERE BossFightId = @bossFightId AND EncounterDifficultyId = @difficultyId";
            }
        }
        public static string SearchForEncounter
        {
            get { return "SELECT * FROM BossFight BF JOIN Instance I ON BF.InstanceId = I.Id WHERE BF.Name LIKE @searchTerm ORDER BY I.Name ASC, BF.Name ASC"; }
        }

        public const string GetAll = "SELECT * FROM BossFight BF JOIN Instance I ON BF.InstanceId = I.Id WHERE I.Visible = 1";

        public static string GetAllProgressionInstances
        {
            get { return "SELECT * FROM BossFight BF JOIN Instance I ON BF.InstanceId = I.Id WHERE I.Visible = 1 AND I.IncludeInProgression = 1"; }
        }

        [Obsolete]
        public static string GetAllInclDifficulty
        {
            get
            {
                return "SELECT * FROM BossFight BF " +
                       "JOIN Instance I ON BF.InstanceId = I.Id " +
                       "LEFT JOIN BossFightDifficulty BFD ON BF.Id = BFD.BossFightId " +
                       "LEFT JOIN EncounterDifficulty ED ON BFD.EncounterDifficultyId = ED.Id";
            }
        }

        /// <summary>
        /// Get the BossFight, Instance, and difficulty information
        /// </summary>
        public static string GetFullBossFightInfo
        {
            get
            {
                return "SELECT * FROM BossFightDifficulty BFD " +
                       "JOIN BossFight BF ON BFD.BossFightId = BF.Id " +
                       "JOIN EncounterDifficulty ED ON BFD.EncounterDifficultyId = ED.Id " +
                       "JOIN Instance I ON BF.InstanceId = I.Id";
            }
        }

        public const string Get = "SELECT * FROM BossFight BF JOIN Instance I ON BF.InstanceId = I.Id WHERE BF.Id = @id";

        public static string GetTopDamageHits
        {
            get
            {
                return "SELECT DISTINCT D1.SourcePlayerId AS PlayerId, D1.AbilityId AS TopDpsAbilityId, D1.TotalDamage AS TopDpsAbilityValue " +
                       "FROM DamageDone D1 JOIN (SELECT SourcePlayerId, MAX(TotalDamage) AS BiggestHit " +
                       "FROM DamageDone WHERE EncounterId = @id AND TargetNpcId IS NOT NULL GROUP BY SourcePlayerId) D2 " +
                       "ON D1.SourcePlayerId = D2.SourcePlayerId AND D1.TotalDamage = D2.BiggestHit " +
                       "WHERE D1.EncounterId = @id ORDER BY D1.TotalDamage DESC;";
            }
        }

        public static string GetTopHealHits
        {
            get
            {
                return "SELECT DISTINCT D1.SourcePlayerId AS PlayerId, D1.AbilityId AS TopHpsAbilityId, D1.EFfectiveHealing AS TopHpsAbilityValue " +
                       "FROM HealingDone D1 JOIN (SELECT SourcePlayerId, MAX(EffectiveHealing) AS BiggestHit " +
                       "FROM HealingDone WHERE EncounterId = @id AND (TargetPlayerId IS NOT NULL OR TargetPetName IS NOT NULL) GROUP BY SourcePlayerId) D2 " +
                       "ON D1.SourcePlayerId = D2.SourcePlayerId AND D1.EffectiveHealing = D2.BiggestHit " +
                       "WHERE D1.EncounterId = @id ORDER BY D1.EffectiveHealing DESC;";
            }
        }

        public static string GetTopShieldHits
        {
            get
            {
                return "SELECT DISTINCT D1.SourcePlayerId AS PlayerId, D1.AbilityId AS TopApsAbilityId, D1.ShieldValue AS TopApsAbilityValue " +
                       "FROM ShieldingDone D1 JOIN (SELECT SourcePlayerId, MAX(ShieldValue) AS BiggestHit " +
                       "FROM ShieldingDone WHERE EncounterId = @id AND (TargetPlayerId IS NOT NULL OR TargetPetName IS NOT NULL) GROUP BY SourcePlayerId) D2 " +
                       "ON D1.SourcePlayerId = D2.SourcePlayerId AND D1.ShieldValue = D2.BiggestHit " +
                       "WHERE D1.EncounterId = @id ORDER BY D1.ShieldValue DESC;";
            }
        }

        public static string GetSingleTopDPS
        {
            get
            {
                return "SELECT EPS.Id, EPS.EncounterId, EPS.PlayerId, EPS.DPS AS Value, " +
                       "EPR.Class, E.IsPublic AS EncounterPublic, P.*, UG.Id, UG.Name " +
                       "FROM EncounterPlayerStatistics EPS " +
                       "JOIN Encounter E ON EPS.EncounterId = E.Id " +
                       "JOIN Guild G ON E.GuildId = G.Id " +
                       "JOIN EncounterPlayerRole EPR ON EPS.EncounterId = EPR.EncounterId AND EPS.PlayerId = EPR.PlayerId " +
                       "JOIN Player P ON EPS.PlayerId = P.Id " +
                       "JOIN Shard S ON P.Shard = S.Name " +
                       "LEFT JOIN AuthUserCharacter AUC ON P.Name = AUC.CharacterName AND S.Id = AUC.ShardId " +
                       "LEFT JOIN Guild UG ON AUC.GuildId = UG.Id " +
                       "WHERE E.BossFightId = @id AND E.SuccessfulKill = 1 " +
                       "AND G.HideFromRankings = 0 ORDER BY Value DESC LIMIT 1;";
            }
        }

        public static string GetTopXDPS
        {
            get
            {
                return GetTopXPerSecond("Dps");
            }
        }

        public static string GetTopXDPSPerClass(string playerClass)
        {
            return GetTopXPerSecondPerClass("Dps", playerClass);
        }

        public static string GetSingleTopHPS
        {
            get
            {
                return "SELECT EPS.Id, EPS.EncounterId, EPS.PlayerId, EPS.HPS AS Value, " +
                       "EPR.Class, E.IsPublic AS EncounterPublic, P.*, UG.Id, UG.Name " +
                       "FROM EncounterPlayerStatistics EPS " +
                       "JOIN Encounter E ON EPS.EncounterId = E.Id " +
                       "JOIN Guild G ON E.GuildId = G.Id " +
                       "JOIN EncounterPlayerRole EPR ON EPS.EncounterId = EPR.EncounterId AND EPS.PlayerId = EPR.PlayerId " +
                       "JOIN Player P ON EPS.PlayerId = P.Id " +
                       "JOIN Shard S ON P.Shard = S.Name " +
                       "LEFT JOIN AuthUserCharacter AUC ON P.Name = AUC.CharacterName AND S.Id = AUC.ShardId " +
                       "LEFT JOIN Guild UG ON AUC.GuildId = UG.Id " +
                       "WHERE E.BossFightId = @id AND E.SuccessfulKill = 1 " +
                       "AND G.HideFromRankings = 0 ORDER BY Value DESC LIMIT 1;";
            }
        }

        public static string GetTopXHPS
        {
            get
            {
                return GetTopXPerSecond("HPS");
                //return "SELECT EPS1.Id, EPS1.EncounterId, EPS1.PlayerId, EPS1.HPS AS Value, EPR.Class, " +
                //       "E.IsPublic AS EncounterPublic, P.*, UG.Id, UG.Name " +
                //       "FROM EncounterPlayerStatistics EPS1 JOIN " +
                //       "(SELECT EPS.PlayerId, MAX(EPS.HPS) AS HPS " +
                //       "FROM EncounterPlayerStatistics EPS " +
                //       "JOIN Encounter E ON EPS.EncounterId = E.Id " +
                //       "JOIN Guild G ON E.GuildId = G.Id " +
                //       "WHERE E.BossFightId = @id  AND E.SuccessfulKill = 1 AND G.HideFromRankings = 0 " +
                //       "GROUP BY PlayerId) EPS2 " +
                //       "ON EPS1.PlayerId = EPS2.PlayerId AND EPS1.HPS = EPS2.HPS " +
                //       "JOIN Encounter E ON EPS1.EncounterId = E.Id " +
                //       "JOIN Guild G ON E.GuildId = G.Id " +
                //       "JOIN EncounterPlayerRole EPR ON EPS1.EncounterId = EPR.EncounterId AND EPS1.PlayerId = EPR.PlayerId " +
                //       "JOIN Player P ON EPS1.PlayerId = P.Id " +
                //       "JOIN Shard S ON P.Shard = S.Name " +
                //       "LEFT JOIN AuthUserCharacter AUC ON P.Name = AUC.CharacterName AND S.Id = AUC.ShardId " +
                //       "LEFT JOIN Guild UG ON AUC.GuildId = UG.Id " +
                //       "WHERE E.BossFightId = @id AND E.SuccessfulKill = 1 AND G.HideFromRankings = 0 " +
                //       "ORDER BY EPS1.HPS DESC LIMIT @limit";
                //return "SELECT EPS.Id, EPS.EncounterId, EPS.PlayerId, MAX(EPS.HPS) AS Value, EPR.Class, E.IsPublic AS EncounterPublic, " +
                //       "P.*, UG.Id, UG.Name " +
                //       "FROM EncounterPlayerStatistics EPS " +
                //       "JOIN Player P ON EPS.PlayerId = P.Id " +
                //       "JOIN EncounterPlayerRole EPR ON EPS.EncounterId = EPR.EncounterId AND EPS.PlayerId = EPR.PlayerId " +
                //       "JOIN Encounter E ON EPS.EncounterId = E.Id " +
                //       "JOIN Guild G ON E.GuildId = G.Id " +
                //       "JOIN GuildStatus GS ON G.GuildStatusId = GS.Id " +
                //       "LEFT JOIN Shard S ON P.Shard = S.Name " +
                //       "LEFT JOIN AuthUserCharacter AUC ON P.Name = AUC.CharacterName AND S.Id = AUC.ShardId " +
                //       "LEFT JOIN Guild UG ON AUC.GuildId = UG.Id " +
                //       "WHERE E.BossFightId = @id AND E.SuccessfulKill = 1 AND G.HideFromRankings = 0 AND GS.Approved = 1 " +
                //       "GROUP BY EPS.PlayerId " +
                //       "ORDER BY Value DESC LIMIT @limit";
            }
        }

        public static string GetTopXHPSPerClass(string playerClass)
        {
            return GetTopXPerSecondPerClass("Hps", playerClass);
        }

        public static string GetSingleTopAPS
        {
            get
            {
                return "SELECT EPS.Id, EPS.EncounterId, EPS.PlayerId, EPS.APS AS Value, " +
                       "EPR.Class, E.IsPublic AS EncounterPublic, P.*, UG.Id, UG.Name " +
                       "FROM EncounterPlayerStatistics EPS " +
                       "JOIN Encounter E ON EPS.EncounterId = E.Id " +
                       "JOIN Guild G ON E.GuildId = G.Id " +
                       "JOIN EncounterPlayerRole EPR ON EPS.EncounterId = EPR.EncounterId AND EPS.PlayerId = EPR.PlayerId " +
                       "JOIN Player P ON EPS.PlayerId = P.Id " +
                       "JOIN Shard S ON P.Shard = S.Name " +
                       "LEFT JOIN AuthUserCharacter AUC ON P.Name = AUC.CharacterName AND S.Id = AUC.ShardId " +
                       "LEFT JOIN Guild UG ON AUC.GuildId = UG.Id " +
                       "WHERE E.BossFightId = @id AND E.SuccessfulKill = 1 " +
                       "AND G.HideFromRankings = 0 ORDER BY Value DESC LIMIT 1;";
            }
        }

        public static string GetTopXAPS
        {
            get
            {
                return GetTopXPerSecond("APS");
                //return "SELECT EPS1.Id, EPS1.EncounterId, EPS1.PlayerId, EPS1.APS AS Value, EPR.Class, " +
                //       "E.IsPublic AS EncounterPublic, P.*, UG.Id, UG.Name " +
                //       "FROM EncounterPlayerStatistics EPS1 JOIN " +
                //       "(SELECT EPS.PlayerId, MAX(EPS.APS) AS APS " +
                //       "FROM EncounterPlayerStatistics EPS " +
                //       "JOIN Encounter E ON EPS.EncounterId = E.Id " +
                //       "JOIN Guild G ON E.GuildId = G.Id " +
                //       "WHERE E.BossFightId = @id  AND E.SuccessfulKill = 1 AND G.HideFromRankings = 0 " +
                //       "GROUP BY PlayerId) EPS2 " +
                //       "ON EPS1.PlayerId = EPS2.PlayerId AND EPS1.APS = EPS2.APS " +
                //       "JOIN Encounter E ON EPS1.EncounterId = E.Id " +
                //       "JOIN Guild G ON E.GuildId = G.Id " +
                //       "JOIN EncounterPlayerRole EPR ON EPS1.EncounterId = EPR.EncounterId AND EPS1.PlayerId = EPR.PlayerId " +
                //       "JOIN Player P ON EPS1.PlayerId = P.Id " +
                //       "JOIN Shard S ON P.Shard = S.Name " +
                //       "LEFT JOIN AuthUserCharacter AUC ON P.Name = AUC.CharacterName AND S.Id = AUC.ShardId " +
                //       "LEFT JOIN Guild UG ON AUC.GuildId = UG.Id " +
                //       "WHERE E.BossFightId = @id AND E.SuccessfulKill = 1 AND G.HideFromRankings = 0 " +
                //       "ORDER BY EPS1.APS DESC LIMIT @limit";
                //return "SELECT EPS.Id, EPS.EncounterId, EPS.PlayerId, MAX(EPS.APS) AS Value, EPR.Class, E.IsPublic AS EncounterPublic, " +
                //       "P.*, UG.Id, UG.Name " +
                //       "FROM EncounterPlayerStatistics EPS " +
                //       "JOIN Player P ON EPS.PlayerId = P.Id " +
                //       "JOIN EncounterPlayerRole EPR ON EPS.EncounterId = EPR.EncounterId AND EPS.PlayerId = EPR.PlayerId " +
                //       "JOIN Encounter E ON EPS.EncounterId = E.Id " +
                //       "JOIN Guild G ON E.GuildId = G.Id " +
                //       "JOIN GuildStatus GS ON G.GuildStatusId = GS.Id " +
                //       "LEFT JOIN Shard S ON P.Shard = S.Name " +
                //       "LEFT JOIN AuthUserCharacter AUC ON P.Name = AUC.CharacterName AND S.Id = AUC.ShardId " +
                //       "LEFT JOIN Guild UG ON AUC.GuildId = UG.Id " +
                //       "WHERE E.BossFightId = @id AND E.SuccessfulKill = 1 AND G.HideFromRankings = 0 AND GS.Approved = 1 " +
                //       "GROUP BY EPS.PlayerId " +
                //       "ORDER BY Value DESC LIMIT @limit";
            }
        }

        public static string GetTopXAPSPerClass(string playerClass)
        {
            return GetTopXPerSecondPerClass("Aps", playerClass);
        }

        public static string GetTopSingleTargetDps
        {
            get
            {
                return "SELECT EPS1.Id, EPS1.EncounterId, EPS1.PlayerId, EPS2.STDPS AS Value, " +
                       "'' AS AbilityIcon, '' AS AbilityName, 0 AS TopHit, " +
                       "EPR.Class, EPR.Role, E.IsPublic AS EncounterPublic, P.*, UG.Id, UG.Name " +
                       "FROM EncounterPlayerStatistics EPS1 " +
                       "JOIN (" +
                       "SELECT EPS.PlayerId, MAX(EPS.SingleTargetDPS) AS STDPS " +
                       "FROM EncounterPlayerStatistics EPS " +
                       "JOIN Encounter E ON EPS.EncounterId = E.Id " +
                       "JOIN Guild G ON E.GuildId = G.Id " +
                       "WHERE E.BossFightId = @id AND E.SuccessfulKill = 1 AND G.HideFromRankings = 0 " +
                       "AND E.EncounterDifficultyId = @difficultyId AND E.IsPublic = 1 " +
                       "AND E.ToBeDeleted = 0 AND E.Removed = 0 " +
                       "GROUP BY EPS.PlayerId ORDER BY STDPS DESC) EPS2 ON " +
                       "EPS1.PlayerId = EPS2.PlayerId AND EPS1.SingleTargetDPS = EPS2.STDPS " +
                       "JOIN Encounter E ON EPS1.EncounterId = E.Id " +
                       "JOIN Guild G ON E.GuildId = G.Id " +
                       "JOIN EncounterPlayerRole EPR ON EPS1.EncounterId = EPR.EncounterId AND EPS1.PlayerId = EPR.PlayerId " +
                       "JOIN Player P ON EPS1.PlayerId = P.Id " +
                       "JOIN Shard S ON P.Shard = S.Name " +
                       "LEFT JOIN AuthUserCharacter AUC ON P.Name = AUC.CharacterName AND S.Id = AUC.ShardId " +
                       "LEFT JOIN Guild UG ON AUC.GuildId = UG.Id " +
                       "WHERE E.BossFightId = @id AND E.SuccessfulKill = 1 AND G.HideFromRankings = 0 " +
                       "AND E.ToBeDeleted = 0 AND E.Removed = 0 " +
                       "AND E.EncounterDifficultyId = @difficultyId AND E.IsPublic = 1 AND EPS2.STDPS > 0 " +
                       "GROUP BY EPS1.PlayerId ORDER BY Value DESC LIMIT @limit";
            }
        }

        public static string GetTopSingleTargetDpsPerClass(string playerClass = "Warrior")
        {
            return string.Format(
                "SELECT EPS1.Id, EPS1.EncounterId, EPS1.PlayerId, EPS2.STDPS AS Value, " +
                   "'' AS AbilityIcon, '' AS AbilityName, 0 AS TopHit, " +
                   "EPR.Class, EPR.Role, E.IsPublic AS EncounterPublic, P.*, UG.Id, UG.Name " +
                   "FROM EncounterPlayerStatistics EPS1 " +
                   "JOIN (" +
                   "SELECT EPS.PlayerId, MAX(EPS.SingleTargetDPS) AS STDPS " +
                   "FROM EncounterPlayerStatistics EPS " +
                   "JOIN Encounter E ON EPS.EncounterId = E.Id " +
                   "JOIN Guild G ON E.GuildId = G.Id " +
                   "WHERE E.BossFightId = @id AND E.SuccessfulKill = 1 AND G.HideFromRankings = 0 " +
                   "AND E.EncounterDifficultyId = @difficultyId AND E.IsPublic = 1 " +
                   "GROUP BY EPS.PlayerId ORDER BY STDPS DESC) EPS2 ON " +
                   "EPS1.PlayerId = EPS2.PlayerId AND EPS1.SingleTargetDPS = EPS2.STDPS " +
                   "JOIN Encounter E ON EPS1.EncounterId = E.Id " +
                   "JOIN Guild G ON E.GuildId = G.Id " +
                   "JOIN EncounterPlayerRole EPR ON EPS1.EncounterId = EPR.EncounterId AND EPS1.PlayerId = EPR.PlayerId " +
                   "JOIN Player P ON EPS1.PlayerId = P.Id " +
                   "JOIN Shard S ON P.Shard = S.Name " +
                   "LEFT JOIN AuthUserCharacter AUC ON P.Name = AUC.CharacterName AND S.Id = AUC.ShardId " +
                   "LEFT JOIN Guild UG ON AUC.GuildId = UG.Id " +
                   "WHERE E.BossFightId = @id AND E.SuccessfulKill = 1 AND G.HideFromRankings = 0 " +
                   "AND E.EncounterDifficultyId = @difficultyId AND E.IsPublic = 1 AND EPS2.STDPS > 0 AND EPR.Class = '{0}' " +
                   "GROUP BY EPS1.PlayerId ORDER BY Value DESC LIMIT @limit", playerClass);
        }

        public static string GetTopXPerSecond(string type = "DPS")
        {
            return string.Format(
                "SELECT EPS1.Id, EPS1.EncounterId, EPS1.PlayerId, EPS2.m{0} AS Value, " +
                "A.Icon AS AbilityIcon, A.Name AS AbilityName, EPS1.Top{0}AbilityValue AS TopHit, " +
                "EPR.Class, EPR.Role, E.IsPublic AS EncounterPublic, P.*, UG.Id, UG.Name " +
                "FROM EncounterPlayerStatistics EPS1 " +
                "JOIN (" +
                //"SELECT EPS.PlayerId, EPS.EncounterId, MAX(EPS.{0}) AS m{0} " +
                "SELECT EPS.Id, MAX(EPS.{0}) AS m{0} " +
                "FROM EncounterPlayerStatistics EPS " +
                "JOIN Encounter E ON EPS.EncounterId = E.Id " +
                "JOIN Guild G ON E.GuildId = G.Id " +
                "WHERE E.BossFightId = @id AND E.SuccessfulKill = 1 AND G.HideFromRankings = 0 " +
                "AND E.ToBeDeleted = 0 AND E.Removed = 0 " +
                "AND E.EncounterDifficultyId = @difficultyId AND E.IsPublic = 1 " +
                //"GROUP BY EPS.PlayerId ORDER BY m{0} DESC) EPS2 ON EPS1.Id = EPS2.Id " +
                "GROUP BY EPS.Id ORDER BY m{0} DESC) EPS2 ON EPS1.Id = EPS2.Id " +
                "JOIN Encounter E ON EPS1.EncounterId = E.Id " +
                "JOIN Guild G ON E.GuildId = G.Id " +
                "JOIN EncounterPlayerRole EPR ON EPS1.EncounterId = EPR.EncounterId AND EPS1.PlayerId = EPR.PlayerId " +
                "JOIN Player P ON EPS1.PlayerId = P.Id " +
                "JOIN Shard S ON P.Shard = S.Name " +
                "JOIN Ability A ON EPS1.Top{0}AbilityId = A.Id " +
                "LEFT JOIN AuthUserCharacter AUC ON P.Name = AUC.CharacterName AND S.Id = AUC.ShardId " +
                "LEFT JOIN Guild UG ON AUC.GuildId = UG.Id " +
                "WHERE E.BossFightId = @id AND E.SuccessfulKill = 1 AND G.HideFromRankings = 0 " +
                "AND E.EncounterDifficultyId = @difficultyId AND E.IsPublic = 1 " +
                "AND E.ToBeDeleted = 0 AND E.Removed = 0 " +
                "GROUP BY EPS1.PlayerId ORDER BY Value DESC LIMIT @limit", type);
        }

        public static string GetTopBurstX(BurstFilter.Duration duration, BurstFilter.Type type)
        {
            var topColumn = "";
            var table = "";

            #region set filters
            switch (duration)
            {
                case BurstFilter.Duration.OneSecond:
                    topColumn = "B1s";
                    break;
                case BurstFilter.Duration.FiveSeconds:
                    topColumn = "B5s";
                    break;
                case BurstFilter.Duration.FifteenSeconds:
                    topColumn = "B15s";
                    break;
            }
            switch (type)
            {
                case BurstFilter.Type.Damage:
                    table = "Damage";
                    break;
                case BurstFilter.Type.Healing:
                    table = "Healing";
                    break;
                case BurstFilter.Type.Shielding:
                    table = "Shielding";
                    break;
            }
            var filterColumn = $"{topColumn.Insert(1, $"urst{table}")}Value";
            #endregion

            return string.Format(
                "SELECT EPS1.Id, EPS1.EncounterId, EPS1.PlayerId, EPS2.{0} AS Value, " +
                "'' AS AbilityIcon, '' AS AbilityName, 0 AS TopHit, " +
                "EPR.Class, EPR.Role, E.IsPublic AS EncounterPublic, P.*, UG.Id, UG.Name " +
                "FROM EncounterPlayerStatistics EPS1 " +
                "JOIN (" +
                "SELECT EPS.Id, EPS.{1} AS {0} " +
                "FROM EncounterPlayerStatistics EPS " +
                "JOIN Encounter E ON EPS.EncounterId = E.Id " +
                "JOIN Guild G ON E.GuildId = G.Id " +
                "WHERE E.BossFightId = @id AND E.SuccessfulKill = 1 AND G.HideFromRankings = 0 " +
                "AND E.EncounterDifficultyId = @difficultyId AND E.IsPublic = 1 " +
                "AND E.ToBeDeleted = 0 AND E.Removed = 0 " +
                "ORDER BY {0} DESC) EPS2 ON EPS1.Id = EPS2.Id " +
                "JOIN Encounter E ON EPS1.EncounterId = E.Id " +
                "JOIN Guild G ON E.GuildId = G.Id " +
                "JOIN EncounterPlayerRole EPR ON EPS1.EncounterId = EPR.EncounterId AND EPS1.PlayerId = EPR.PlayerId " +
                "JOIN Player P ON EPS1.PlayerId = P.Id " +
                "JOIN Shard S ON P.Shard = S.Name " +
                "LEFT JOIN AuthUserCharacter AUC ON P.Name = AUC.CharacterName AND S.Id = AUC.ShardId " +
                "LEFT JOIN Guild UG ON AUC.GuildId = UG.Id " +
                "WHERE E.BossFightId = @id AND E.SuccessfulKill = 1 AND G.HideFromRankings = 0 " +
                "AND E.EncounterDifficultyId = @difficultyId AND E.IsPublic = 1 " +
                "AND E.ToBeDeleted = 0 AND E.Removed = 0 " +
                "GROUP BY EPS1.PlayerId ORDER BY Value DESC LIMIT @limit", topColumn, filterColumn);
        }

        public static string GetTopXPerSecondPerClass(string type = "DPS", string playerClass = "Warrior")
        {
            return string.Format(
                "SELECT EPS1.Id, EPS1.EncounterId, EPS1.PlayerId, EPS2.m{0} AS Value, " +
                "A.Icon AS AbilityIcon, A.Name AS AbilityName, EPS1.Top{0}AbilityValue AS TopHit, " +
                "EPR.Class, E.IsPublic AS EncounterPublic, P.*, UG.Id, UG.Name " +
                "FROM EncounterPlayerStatistics EPS1 " +
                "JOIN (" +
                //"SELECT EPS.PlayerId, EPS.EncounterId, MAX(EPS.{0}) AS m{0} " +
                "SELECT EPS.Id, MAX(EPS.{0}) AS m{0} " +
                "FROM EncounterPlayerStatistics EPS " +
                "JOIN Encounter E ON EPS.EncounterId = E.Id " +
                "JOIN Guild G ON E.GuildId = G.Id " +
                "WHERE E.BossFightId = @id AND E.SuccessfulKill = 1 AND G.HideFromRankings = 0 " +
                "AND E.EncounterDifficultyId = @difficultyId AND E.IsPublic = 1 " +
                //"GROUP BY EPS.PlayerId ORDER BY m{0} DESC) EPS2 ON EPS1.Id = EPS2.Id " +
                "GROUP BY EPS.Id ORDER BY m{0} DESC) EPS2 ON EPS1.Id = EPS2.Id " +
                "JOIN Encounter E ON EPS1.EncounterId = E.Id " +
                "JOIN Guild G ON E.GuildId = G.Id " +
                "JOIN EncounterPlayerRole EPR ON EPS1.EncounterId = EPR.EncounterId AND EPS1.PlayerId = EPR.PlayerId " +
                "JOIN Player P ON EPS1.PlayerId = P.Id " +
                "JOIN Shard S ON P.Shard = S.Name " +
                "JOIN Ability A ON EPS1.Top{0}AbilityId = A.Id " +
                "LEFT JOIN AuthUserCharacter AUC ON P.Name = AUC.CharacterName AND S.Id = AUC.ShardId " +
                "LEFT JOIN Guild UG ON AUC.GuildId = UG.Id " +
                "WHERE E.BossFightId = @id AND E.SuccessfulKill = 1 AND G.HideFromRankings = 0 " +
                "AND E.EncounterDifficultyId = @difficultyId AND E.IsPublic = 1 AND EPR.Class = '{1}' " +
                "GROUP BY EPS1.PlayerId ORDER BY Value DESC LIMIT @limit", type, playerClass);
        }

        public static string GetTopXPerSecondPerRole(string type = "DPS")
        {
            return string.Format(
                "SELECT EPS1.Id, EPS1.EncounterId, EPS1.PlayerId, EPS2.m{0} AS Value, " +
                "A.Icon AS AbilityIcon, A.Name AS AbilityName, EPS1.Top{0}AbilityValue AS TopHit, " +
                "EPR.Class, EPR.Role, E.IsPublic AS EncounterPublic, P.*, UG.Id, UG.Name " +
                "FROM EncounterPlayerStatistics EPS1 " +
                "JOIN (" +
                //"SELECT EPS.PlayerId, EPS.EncounterId, MAX(EPS.{0}) AS m{0} " +
                "SELECT EPS.Id, MAX(EPS.{0}) AS m{0} " +
                "FROM EncounterPlayerStatistics EPS " +
                "JOIN Encounter E ON EPS.EncounterId = E.Id " +
                "JOIN Guild G ON E.GuildId = G.Id " +
                "WHERE E.BossFightId = @id AND E.SuccessfulKill = 1 AND G.HideFromRankings = 0 " +
                "AND E.EncounterDifficultyId = @difficultyId AND E.IsPublic = 1 " +
                "AND E.ToBeDeleted = 0 AND E.Removed = 0 " +
                //"GROUP BY EPS.PlayerId ORDER BY m{0} DESC) EPS2 ON EPS1.Id = EPS2.Id " +
                "GROUP BY EPS.Id ORDER BY m{0} DESC) EPS2 ON EPS1.Id = EPS2.Id " +
                "JOIN Encounter E ON EPS1.EncounterId = E.Id " +
                "JOIN Guild G ON E.GuildId = G.Id " +
                "JOIN EncounterPlayerRole EPR ON EPS1.EncounterId = EPR.EncounterId AND EPS1.PlayerId = EPR.PlayerId " +
                "JOIN Player P ON EPS1.PlayerId = P.Id " +
                "JOIN Shard S ON P.Shard = S.Name " +
                "JOIN Ability A ON EPS1.Top{0}AbilityId = A.Id " +
                "LEFT JOIN AuthUserCharacter AUC ON P.Name = AUC.CharacterName AND S.Id = AUC.ShardId " +
                "LEFT JOIN Guild UG ON AUC.GuildId = UG.Id " +
                "WHERE E.BossFightId = @id AND E.SuccessfulKill = 1 AND G.HideFromRankings = 0 " +
                "AND E.ToBeDeleted = 0 AND E.Removed = 0 " +
                "AND E.EncounterDifficultyId = @difficultyId AND E.IsPublic = 1 AND EPR.Role = @role " +
                "GROUP BY EPS1.PlayerId ORDER BY Value DESC LIMIT @limit", type);
        }
        
        public const string Create = "INSERT INTO BossFight (Name, InstanceId, DpsCheck, RequiresSpecialProcessing, PriorityIfDuplicate) " +
                                     "VALUES (@name, @instanceId, @dpsCheck, @requiresSpecialProcessing, @priorityIfDuplicate)";

        public const string Delete = "DELETE FROM BossFight WHERE Id = @id";


        public const string Update = "UPDATE BossFight " +
                                     "SET Name = @name, InstanceId = @instanceId, DpsCheck = @dpsCheck, RequiresSpecialProcessing = @requiresSpecialProcessing, " +
                                     "requiresSpecialProcessing = @priorityIfDuplicate WHERE Id = @id";

        public static class PagedQuery
        {
            public static string Base(string selectFrom, string selectAlias, string selectObject)
            {
                return string.Format(
                    "SELECT {0} FROM {1} {2} " +
                    "JOIN Instance {3} ON {2}.InstanceId = {3}.Id ",
                    selectObject, selectFrom, selectAlias, AliasTs.Instance.Alias);
            }

            public static string SelectAllFrom(AliasTs obj)
            {
                var selectObject = $"{obj.Alias}.*, {AliasTs.Instance.Alias}.*";
                return $"{Base(obj.Name, obj.Alias, selectObject)} /**where**/ GROUP BY {obj.Alias}.Id /**orderby**/ LIMIT @offset,@total";
            }

            public static string CountAllFrom(AliasTs obj)
            {
                return $"SELECT COUNT(1) FROM ({Base(obj.Name, obj.Alias, "1")} /**where**/ GROUP BY {obj.Alias}.Id) Q";
            }
        }
    }
}
