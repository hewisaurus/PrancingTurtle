namespace Database.MySQL
{
    public static class Records
    {
        public static string GetTopXPSBossFightXDifficultyX(string type, string classRestriction)
        {
            if (classRestriction == "Warrior" || classRestriction == "Rogue" || classRestriction == "Cleric"
                || classRestriction == "Mage" || classRestriction == "Primalist")
            {
                return string.Format(
                       "SELECT EPS1.*, E.*, P.* " +
                       "FROM EncounterPlayerStatistics EPS1 " +
                       "JOIN (SELECT EPS.EncounterId, MAX({0}) AS m{0} FROM EncounterPlayerStatistics EPS " +
                       "JOIN Encounter E ON EPS.EncounterId = E.Id " +
                       "JOIN EncounterPlayerRole EPR ON EPS.EncounterId = EPR.EncounterId AND EPS.PlayerId = EPR.PlayerId " +
                       "WHERE E.BossFightId = @bossFightId AND E.EncounterDifficultyId = @difficultyId AND EPR.Class = '{1}' " +
                       "GROUP BY EPS.EncounterId ORDER BY E.Date ASC, EPS.DPS DESC) EPS2 ON EPS1.{0} = EPS2.m{0} AND EPS1.EncounterId = EPS2.EncounterId " +
                       "JOIN Encounter E ON EPS1.EncounterId = E.Id " +
                       "JOIN Player P ON EPS1.PlayerId = P.Id " +
                       "WHERE E.BossFightId = @bossFightId AND E.EncounterDifficultyId = @difficultyId ORDER BY E.Date ASC", type.ToUpper(), classRestriction);
            }
            return string.Format(
                   "SELECT EPS1.*, E.*, P.* " +
                   "FROM EncounterPlayerStatistics EPS1 " +
                   "JOIN (SELECT EPS.EncounterId, MAX({0}) AS m{0} FROM EncounterPlayerStatistics EPS " +
                   "JOIN Encounter E ON EPS.EncounterId = E.Id " +
                   "JOIN EncounterPlayerRole EPR ON EPS.EncounterId = EPR.EncounterId AND EPS.PlayerId = EPR.PlayerId " +
                   "WHERE E.BossFightId = @bossFightId AND E.EncounterDifficultyId = @difficultyId " +
                   "GROUP BY EPS.EncounterId ORDER BY E.Date ASC, EPS.DPS DESC) EPS2 ON EPS1.{0} = EPS2.m{0} AND EPS1.EncounterId = EPS2.EncounterId " +
                   "JOIN Encounter E ON EPS1.EncounterId = E.Id " +
                   "JOIN Player P ON EPS1.PlayerId = P.Id " +
                   "WHERE E.BossFightId = @bossFightId AND E.EncounterDifficultyId = @difficultyId ORDER BY E.Date ASC", type.ToUpper());

        }

        public static string GetPlayerStatsForBossFightXDifficultyXForGuild
        {
            get
            {
                return "SELECT EPS.*, E.*, P.* FROM EncounterPlayerStatistics EPS " +
                       "JOIN Encounter E ON EPS.EncounterId = E.Id JOIN Player P ON EPS.PlayerId = P.Id " +
                       "WHERE E.BossFightId = @bossFightId AND E.EncounterDifficultyId = @difficultyId AND E.GuildId = @guildId ORDER BY E.Date ASC;";
            }
        }

        public static string GetCalculatedGuildXpsOverTime
        {
            get
            {
                return "SELECT E.*, EO.AverageDps, EO.AverageHps, EO.AverageAps " +
                       "FROM EncounterOverview EO " +
                       "JOIN Encounter E ON EO.EncounterId = E.Id " +
                       "WHERE E.GuildId = @guildId AND E.EncounterDifficultyId = @difficultyId " +
                       "AND E.BossFightId = @bossFightId AND E.SuccessfulKill = 1 " +
                       "AND E.ValidForRanking = 1 " +
                       "ORDER BY E.Date;";
            }
        }

        public const string GetEncounterDurationOverTime = "SELECT E.* " +
                        "FROM Encounter E " +
                        "WHERE E.GuildId = @guildId AND E.EncounterDifficultyId = @difficultyId " +
                        "AND E.BossFightId = @bossFightId AND E.SuccessfulKill = 1 " +
                        "AND E.ValidForRanking = 1 " +
                        "ORDER BY E.Date;";

        /// <summary>
        /// Get the single top record for DPS / HPS / APS for bossfight X with difficulty X
        /// </summary>
        /// <param name="xpsType"></param>
        /// <returns></returns>
        public static string GetSingleTopXps(string xpsType = "DPS")
        {
            return string.Format(
                "SELECT EPS1.Id, EPS1.EncounterId, EPS1.PlayerId, EPS2.m{0} AS Value, A.Icon AS AbilityIcon, " +
                   "A.Name AS AbilityName, EPS1.Top{0}AbilityValue AS TopHit, EPR.Class, E.IsPublic AS EncounterPublic, P.*, UG.Id, UG.Name " +
                   "FROM EncounterPlayerStatistics EPS1 " +
                   "JOIN (SELECT EPS.Id, MAX(EPS.{0}) AS m{0} FROM EncounterPlayerStatistics EPS " +
                   "JOIN Encounter E ON EPS.EncounterId = E.Id JOIN Guild G ON E.GuildId = G.Id WHERE E.BossFightId = @bossFightId AND E.SuccessfulKill = 1 " +
                   "AND G.HideFromRankings = 0 AND E.EncounterDifficultyId = @difficultyId AND E.IsPublic = 1 " +
                   "GROUP BY EPS.Id ORDER BY m{0} DESC LIMIT 1) EPS2 ON EPS1.Id = EPS2.Id " +
                   "JOIN Encounter E ON EPS1.EncounterId = E.Id JOIN Guild G ON E.GuildId = G.Id " +
                   "JOIN EncounterPlayerRole EPR ON EPS1.EncounterId = EPR.EncounterId AND EPS1.PlayerId = EPR.PlayerId " +
                   "JOIN Player P ON EPS1.PlayerId = P.Id JOIN Shard S ON P.Shard = S.Name JOIN Ability A ON EPS1.Top{0}AbilityId = A.Id " +
                   "LEFT JOIN AuthUserCharacter AUC ON P.Name = AUC.CharacterName AND S.Id = AUC.ShardId LEFT JOIN Guild UG ON AUC.GuildId = UG.Id;", xpsType);
        }

        public static string GetSingleTopXpsGuild(string xpsType = "DPS")
        {
            return string.Format(
                "SELECT EO.Id, E.Id AS EncounterId, EO.Average{0} AS Value, G.* " +
                "FROM EncounterOverview EO " +
                "JOIN Encounter E ON EO.EncounterId = E.Id " +
                "JOIN Guild G ON E.GuildId = G.Id " +
                "WHERE E.EncounterDifficultyId = @difficultyId " +
                "AND E.BossFightId = @bossFightId AND E.SuccessfulKill = 1 " +
                "AND E.ValidForRanking = 1 AND G.HideFromRankings = 0 AND E.IsPublic = 1 " +
                "ORDER BY Average{0} DESC LIMIT 1", xpsType);
        }

        public static string GetAllGuildXps(string xpsType = "DPS")
        {
            return string.Format(
                "SELECT EPS1.Id, EPS1.EncounterId, EPS1.PlayerId, EPS2.m{0} AS Value, EPR.Class, P.* " +
                "FROM EncounterPlayerStatistics EPS1 JOIN " +
                "(SELECT EPS.PlayerId, MAX(EPS.{0}) AS m{0} FROM EncounterPlayerStatistics EPS " +
                "JOIN Encounter E ON EPS.EncounterId = E.Id " +
                "WHERE E.BossFightId = @bossFightId AND E.SuccessfulKill = 1 " +
                "AND E.EncounterDifficultyId = @difficultyId AND E.GuildId = @guildId " +
                "GROUP BY EPS.PlayerId ORDER BY m{0} DESC) EPS2 ON EPS1.{0} = EPS2.m{0} AND EPS1.PlayerId = EPS2.PlayerId " +
                "JOIN Encounter E ON EPS1.EncounterId = E.Id " +
                "JOIN Guild G ON E.GuildId = G.Id " +
                "JOIN EncounterPlayerRole EPR ON EPS1.EncounterId = EPR.EncounterId AND EPS1.PlayerId = EPR.PlayerId " +
                "JOIN Player P ON EPS1.PlayerId = P.Id JOIN Shard S ON P.Shard = S.Name " +
                "WHERE E.BossFightId = @bossFightId AND E.SuccessfulKill = 1 AND E.EncounterDifficultyId = @difficultyId " +
                "AND E.GuildId = @guildId AND EPS2.m{0} > 0 GROUP BY EPS1.PlayerId ORDER BY Value DESC;", xpsType);
        }
    }
}
