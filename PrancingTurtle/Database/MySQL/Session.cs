namespace Database.MySQL
{
    public static class Session
    {
        public static string GetRecentSessionsNoAuth
        {
            get
            {
                return "SELECT S.*, AUC.*, SH.*, G.* FROM Session S " +
                    "JOIN AuthUserCharacter AUC ON S.AuthUserCharacterId = AUC.Id " +
                    "JOIN Shard SH ON AUC.ShardId = SH.Id " +
                    "JOIN Guild G ON AUC.GuildId = G.Id " +
                    "JOIN GuildStatus GS ON G.GuildStatusId = GS.Id " +
                    "WHERE G.HideSessions = 0 AND S.Duration > '00:01:00' AND GS.Approved = 1 AND S.EncountersPublic = 1 " +
                    "ORDER BY Date DESC LIMIT 0, @sessionCount";
                //return "SELECT * FROM Session S " +
                //       "JOIN AuthUserCharacter AUC ON S.AuthUserCharacterId = AUC.Id " +
                //       "JOIN Shard SH ON AUC.ShardId = SH.Id " +
                //       "LEFT JOIN Guild G ON AUC.GuildId = G.Id " +
                //       "WHERE S.Duration > '00:01:00' " +
                //       "ORDER BY S.Date DESC " +
                //       "LIMIT 0, @sessionCount";
            }
        }

        public static string GetRecentSessionsAuthenticated
        {
            get
            {
                return "SELECT S.*, AUC.*, SH.*, G.* FROM Session S " +
                         "JOIN AuthUserCharacter AUC ON S.AuthUserCharacterId = AUC.Id " +
                         "JOIN Shard SH ON AUC.ShardId = SH.Id " +
                         "JOIN Guild G ON AUC.GuildId = G.Id " +
                         "JOIN GuildStatus GS ON G.GuildStatusId = GS.Id " +
                         "WHERE G.HideSessions = 0 AND S.Duration > '00:01:00' AND GS.Approved = 1 AND S.EncountersPublic = 1 " +
                         "UNION " +
                         "SELECT S.*, AUC.*, SH.*, G.* FROM Session S " +
                         "JOIN AuthUserCharacter AUC ON S.AuthUserCharacterId = AUC.Id " +
                         "JOIN Shard SH ON AUC.ShardId = SH.Id " +
                         "JOIN Guild G ON AUC.GuildId = G.Id " +
                         "WHERE G.HideSessions = 1 AND S.Duration > '00:01:00' AND G.Id IN " +
                         "(SELECT DISTINCT AUC.GuildId FROM AuthUserCharacter AUC " +
                         "JOIN AuthUser AU ON AUC.AuthUserId = AU.Id " +
                         "WHERE AU.Email = @email AND Removed = 0 " +
                         "AND AUC.GuildId IS NOT NULL) ORDER BY Date DESC LIMIT 0, @sessionCount";
            }
        }

        public static string GetSession
        {
            get
            {
                return "SELECT * FROM Session S " +
                       "JOIN AuthUserCharacter AUC ON S.AuthUserCharacterId = AUC.Id " +
                       "JOIN Shard SH ON AUC.ShardId = SH.Id " +
                       "LEFT JOIN Guild G ON AUC.GuildId = G.Id " +
                       "WHERE S.Id = @Id LIMIT 0,1";
            }
        }

        public static string GetEncounters
        {
            get
            {
                return "SELECT E.*, ED.*, EO.Id AS EncounterOverviewId, BF.*, I.*, EO.* " +
                       "FROM SessionEncounter SE " +
                       "JOIN Encounter E ON SE.EncounterId = E.Id " +
                       "JOIN BossFight BF ON E.BossFightId = BF.Id " +
                       "JOIN Instance I ON BF.InstanceId = I.Id " +
                       "JOIN EncounterDifficulty ED ON E.EncounterDifficultyId = ED.Id " +
                       "LEFT JOIN EncounterOverview EO ON E.Id = EO.EncounterId " +
                       "WHERE SE.SessionId = @id ORDER BY E.Date ASC";
            }
        }

        public static string GetEncountersWithKillTimeRank
        {
            get
            {
                return "SELECT E.*, (SELECT (IF(E.SuccessfulKill = 1 AND E.ValidForRanking = 1, " +
                       "(SELECT COUNT(1) FROM Encounter " +
                       "WHERE Duration <= E.Duration AND SuccessfulKill = 1 AND ValidForRanking = 1 " +
                       "AND BossFightId = E.BossFightId AND EncounterDifficultyId = E.EncounterDifficultyId), " +
                       "0)))AS KillTimeRank, ED.*, EO.Id AS EncounterOverviewId, BF.*, I.*, EO.* " +
                       "FROM SessionEncounter SE " +
                       "JOIN Encounter E ON SE.EncounterId = E.Id " +
                       "JOIN BossFight BF ON E.BossFightId = BF.Id " +
                       "JOIN Instance I ON BF.InstanceId = I.Id " +
                       "JOIN EncounterDifficulty ED ON E.EncounterDifficultyId = ED.Id " +
                       "LEFT JOIN EncounterOverview EO ON E.Id = EO.EncounterId " +
                       "WHERE SE.SessionId = @id ORDER BY E.Date ASC";
            }
        }

        public static string GetEncountersWithKillTimeAndDpsRank
        {
            get
            {
                return "SELECT E.*, (SELECT (IF(E.SuccessfulKill = 1 AND E.ValidForRanking = 1, " +
                       "(SELECT COUNT(1) FROM Encounter " +
                       "WHERE Duration <= E.Duration AND SuccessfulKill = 1 AND ValidForRanking = 1 " +
                       "AND BossFightId = E.BossFightId AND EncounterDifficultyId = E.EncounterDifficultyId), " +
                       "0))) AS KillTimeRank, " +
                       "(SELECT (IF(E.SuccessfulKill = 1 AND E.ValidForRanking = 1, " +
                       "(SELECT COUNT(1) FROM EncounterOverview EO2 " +
                       "JOIN Encounter E2 ON EO2.EncounterId = E2.Id " +
                       "WHERE AverageDPS >= EO.AverageDPS AND E2.SuccessfulKill = 1 AND E2.ValidForRanking = 1 " +
                       "AND E2.BossFightId = E.BossFightId AND E2.EncounterDifficultyId = E.EncounterDifficultyId), 0))) AS DPSRank, " +
                       "ED.*, EO.Id AS EncounterOverviewId, BF.*, I.*, EO.* " +
                       "FROM SessionEncounter SE " +
                       "JOIN Encounter E ON SE.EncounterId = E.Id " +
                       "JOIN BossFight BF ON E.BossFightId = BF.Id " +
                       "JOIN Instance I ON BF.InstanceId = I.Id " +
                       "JOIN EncounterDifficulty ED ON E.EncounterDifficultyId = ED.Id " +
                       "LEFT JOIN EncounterOverview EO ON E.Id = EO.EncounterId " +
                       "WHERE SE.SessionId = @id ORDER BY E.Date ASC";
            }
        }

        public static string GetEncountersWithKillTimeAndDpsRankHideDeleted
        {
            get
            {
                return "SELECT E.*, (SELECT (IF(E.SuccessfulKill = 1 AND E.ValidForRanking = 1, " +
                       "(SELECT COUNT(1) FROM Encounter " +
                       "WHERE Duration <= E.Duration AND SuccessfulKill = 1 AND ValidForRanking = 1 " +
                       "AND BossFightId = E.BossFightId AND EncounterDifficultyId = E.EncounterDifficultyId), " +
                       "0))) AS KillTimeRank, " +
                       "(SELECT (IF(E.SuccessfulKill = 1 AND E.ValidForRanking = 1, " +
                       "(SELECT COUNT(1) FROM EncounterOverview EO2 " +
                       "JOIN Encounter E2 ON EO2.EncounterId = E2.Id " +
                       "WHERE AverageDPS >= EO.AverageDPS AND E2.SuccessfulKill = 1 AND E2.ValidForRanking = 1 " +
                       "AND E2.BossFightId = E.BossFightId AND E2.EncounterDifficultyId = E.EncounterDifficultyId), 0))) AS DPSRank, " +
                       "ED.*, EO.Id AS EncounterOverviewId, BF.*, I.*, EO.* " +
                       "FROM SessionEncounter SE " +
                       "JOIN Encounter E ON SE.EncounterId = E.Id " +
                       "JOIN BossFight BF ON E.BossFightId = BF.Id " +
                       "JOIN Instance I ON BF.InstanceId = I.Id " +
                       "JOIN EncounterDifficulty ED ON E.EncounterDifficultyId = ED.Id " +
                       "LEFT JOIN EncounterOverview EO ON E.Id = EO.EncounterId " +
                       "WHERE SE.SessionId = @id AND E.ToBeDeleted = 0 ORDER BY E.Date ASC";
            }
        }

        public static string GetGuildSessions
        {
            get
            {
                return "SELECT * FROM Session S " +
                       "JOIN AuthUserCharacter AUC ON S.AuthUserCharacterId = AUC.Id " +
                       "JOIN Shard SH ON AUC.ShardId = SH.Id " +
                       "LEFT JOIN Guild G ON AUC.GuildId = G.Id " +
                       "WHERE AUC.GuildId = @id " +
                       "ORDER BY S.Date DESC";
            }
        }

        public static string GetPlayerSessionsNoAuth
        { 
            get
            {
                return "SELECT S.* FROM SessionEncounter SE " +
                       "JOIN EncounterPlayerRole EPR ON SE.EncounterId = EPR.EncounterId " +
                       "JOIN Session S ON SE.SessionId = S.Id " +
                       "JOIN AuthUserCharacter AUC ON S.AuthUserCharacterId = AUC.Id " +
                       "JOIN Guild G ON AUC.GuildId = G.Id " +
                       "JOIN GuildStatus GS ON G.GuildStatusId = GS.Id " +
                       "WHERE EPR.PlayerId = @playerId AND G.HideSessions = 0 " +
                       "AND GS.Approved = 1  " +
                       "GROUP BY SessionId ORDER BY Date DESC;";
            }
        }

        public static string GetPlayerSessionsAuthenticated
        {
            get
            {
                return "SELECT S.* FROM SessionEncounter SE " +
                       "JOIN EncounterPlayerRole EPR ON SE.EncounterId = EPR.EncounterId " +
                       "JOIN Session S ON SE.SessionId = S.Id " +
                       "JOIN AuthUserCharacter AUC ON S.AuthUserCharacterId = AUC.Id " +
                       "JOIN Guild G ON AUC.GuildId = G.Id " +
                       "JOIN GuildStatus GS ON G.GuildStatusId = GS.Id " +
                       "WHERE EPR.PlayerId = @playerId AND G.HideSessions = 1 " +
                       "AND GS.Active = 1 AND G.Id IN " +
                       "(SELECT DISTINCT AUC.GuildId FROM AuthUserCharacter AUC " +
                       "JOIN AuthUser AU ON AUC.AuthUserId = AU.Id " +
                       "WHERE AU.Email = @email AND Removed = 0  AND AUC.GuildId IS NOT NULL) " +
                       "GROUP BY SessionId " +
                       "UNION " +
                       "SELECT S.* FROM SessionEncounter SE " +
                       "JOIN EncounterPlayerRole EPR ON SE.EncounterId = EPR.EncounterId " +
                       "JOIN Session S ON SE.SessionId = S.Id " +
                       "JOIN AuthUserCharacter AUC ON S.AuthUserCharacterId = AUC.Id " +
                       "JOIN Guild G ON AUC.GuildId = G.Id " +
                       "JOIN GuildStatus GS ON G.GuildStatusId = GS.Id " +
                       "WHERE EPR.PlayerId = @playerId AND G.HideSessions = 0 " +
                       "AND GS.Approved = 1 " +
                       "GROUP BY SessionId " +
                       "ORDER BY Date DESC;";
            }
        }

        public static string CountGuildSessions
        {
            get
            {
                return "SELECT COUNT(*) FROM Session S JOIN AuthUserCharacter AUC ON S.AuthUserCharacterId = AUC.Id WHERE AUC.GuildId = @guildId";
            }
        }

        public static string GetAllSessionsUnauthenticated
        {
            get
            {
                return "SELECT * FROM Session S " +
                    "JOIN AuthUserCharacter AUC ON S.AuthUserCharacterId = AUC.Id " +
                    "JOIN Shard SH ON AUC.ShardId = SH.Id " +
                    "JOIN Guild G ON AUC.GuildId = G.Id " +
                    "WHERE G.HideSessions = 0 " +
                    "ORDER BY S.Date DESC";
            }
        }

        public static string GetAllSessionsAuthenticated
        {
            get
            {
                return "SELECT * FROM Session S " +
                       "JOIN AuthUserCharacter AUC ON S.AuthUserCharacterId = AUC.Id " +
                       "JOIN Shard SH ON AUC.ShardId = SH.Id " +
                       "JOIN Guild G ON AUC.GuildId = G.Id " +
                       "WHERE G.HideSessions = 0 " +
                       "UNION " +
                       "SELECT * FROM Session S " +
                       "JOIN AuthUserCharacter AUC ON S.AuthUserCharacterId = AUC.Id " +
                       "JOIN Shard SH ON AUC.ShardId = SH.Id " +
                       "JOIN Guild G ON AUC.GuildId = G.Id " +
                       "WHERE G.HideSessions = 1 AND G.Id IN " +
                       "(SELECT DISTINCT AUC.GuildId FROM AuthUserCharacter AUC " +
                       "JOIN AuthUser AU ON AUC.AuthUserId = AU.Id " +
                       "WHERE AU.Email = @email AND Removed = 0 " +
                       "AND AUC.GuildId IS NOT NULL) ORDER BY Date DESC";
            }
        }

        public static string GetInstancesSeen
        {
            get { return "SELECT I.* FROM SessionEncounter SE " +
                         "JOIN Encounter E ON SE.EncounterId = E.Id " +
                         "JOIN BossFight BF ON E.BossFightId = BF.Id " +
                         "JOIN Instance I ON BF.InstanceId = I.Id " +
                         "WHERE SE.SessionId = @sessionId GROUP BY I.Name"; }
        }

        public static string GetDifficultiesSeen
        {
            get
            {
                return "SELECT ED.ShortName FROM SessionEncounter SE " +
                       "JOIN Encounter E ON SE.EncounterId = E.Id " +
                       "JOIN EncounterDifficulty ED ON E.EncounterDifficultyId = ED.Id " +
                       "JOIN BossFight BF ON E.BossFightId = BF.Id " +
                       "JOIN BossFightDifficulty BFD ON BF.Id = BFD.BossFightId " +
                       "WHERE SE.SessionId = @sessionId AND BF.InstanceId = @instanceId AND E.SuccessfulKill = 1 " +
                       "GROUP BY ED.Id " +
                       "ORDER BY ED.Priority DESC";
            }
        }

        public static string ListBossesKilled
        {
            get
            {
                return "SELECT IF(BFD.ID IS NULL, BF.Name, CONCAT(BF.Name, ' (', ED.ShortName, ')')) AS Name " +
                         "FROM SessionEncounter SE " +
                         "JOIN Encounter E ON SE.EncounterId = E.Id " +
                         "JOIN BossFight BF ON E.BossFightId = BF.Id " +
                         "JOIN EncounterDifficulty ED ON E.EncounterDifficultyId = ED.Id " +
                         "LEFT JOIN BossFightDifficulty BFD ON BF.Id = BFD.BossFightId " +
                         "WHERE SE.SessionId = @sessionId AND E.SuccessfulKill = 1 AND E.Removed = 0 " +
                         "GROUP BY BF.Name ORDER BY E.Id";
            }
        }

        public static string ListBossesSeenButNotKilled
        {
            get { return "SELECT BF.Name FROM SessionEncounter SE " +
                         "JOIN Encounter E ON SE.EncounterId = E.Id " +
                         "JOIN BossFight BF ON E.BossFightId = BF.Id " +
                         "WHERE SE.SessionId = @sessionId AND E.SuccessfulKill = 0 AND E.Removed = 0 " +
                         "AND BF.Name NOT IN" +
                         "(SELECT BF.Name FROM SessionEncounter SE " +
                         "JOIN Encounter E ON SE.EncounterId = E.Id " +
                         "JOIN BossFight BF ON E.BossFightId = BF.Id " +
                         "WHERE SE.SessionId = @sessionId AND E.SuccessfulKill = 1 AND E.Removed = 0 " +
                         "GROUP BY BF.Name) GROUP BY Name"; }
        }

        public static string GetSessionsForInstanceNoAuth
        {
            get { return "SELECT S.*, G.* FROM SessionEncounter SE " +
                         "JOIN Session S ON SE.SessionId = S.Id " +
                         "JOIN Encounter E ON SE.EncounterId = E.Id " +
                         "JOIN BossFight BF ON E.BossFightId = BF.Id " +
                         "JOIN AuthUserCharacter AUC ON S.AuthUserCharacterId = AUC.Id " +
                         "JOIN Guild G ON AUC.GuildId = G.Id " +
                         "JOIN GuildStatus GS ON G.GuildStatusId = GS.Id " +
                         "WHERE BF.InstanceId = @instanceId AND G.HideSessions = 0 " +
                         "AND GS.Active = 1 AND GS.Approved = 1 " +
                         "GROUP BY S.Id ORDER BY S.Date DESC"; }
        }

        public static string GetSessionsForInstanceWithAuth
        {
            get { return "SELECT S.*, G.* FROM SessionEncounter SE " +
                         "JOIN Session S ON SE.SessionId = S.Id " +
                         "JOIN Encounter E ON SE.EncounterId = E.Id " +
                         "JOIN BossFight BF ON E.BossFightId = BF.Id " +
                         "JOIN AuthUserCharacter AUC ON S.AuthUserCharacterId = AUC.Id " +
                         "JOIN Guild G ON AUC.GuildId = G.Id " +
                         "JOIN GuildStatus GS ON G.GuildStatusId = GS.Id " +
                         "WHERE BF.InstanceId = @instanceId AND G.HideSessions = 0 " +
                         "AND GS.Active = 1 AND GS.Approved = 1 " +
                         "GROUP BY S.Id " +
                         "UNION " +
                         "SELECT S.*, G.* FROM SessionEncounter SE " +
                         "JOIN Session S ON SE.SessionId = S.Id " +
                         "JOIN Encounter E ON SE.EncounterId = E.Id " +
                         "JOIN BossFight BF ON E.BossFightId = BF.Id " +
                         "JOIN AuthUserCharacter AUC ON S.AuthUserCharacterId = AUC.Id " +
                         "JOIN AuthUser AU ON AUC.AuthUserId = AU.Id " +
                         "JOIN Guild G ON AUC.GuildId = G.Id " +
                         "JOIN GuildStatus GS ON G.GuildStatusId = GS.Id " +
                         "WHERE BF.InstanceId = @instanceId AND G.Id IN(" +
                         "SELECT DISTINCT G.Id FROM AuthUserCharacter AUC " +
                         "JOIN AuthUser AU ON AUC.AuthUserId = AU.Id " +
                         "JOIN Guild G ON AUC.GuildId = G.Id " +
                         "WHERE AU.Email = @email AND AUC.Removed = 0) " +
                         "AND GS.Active = 1 AND GS.Approved = 1 " +
                         "GROUP BY S.Id ORDER BY Date DESC"; }
        }

        public static string GetAllSessionsForInstance
        {
            get
            {
                return "SELECT S.*, G.* FROM SessionEncounter SE " +
                       "JOIN Session S ON SE.SessionId = S.Id " +
                       "JOIN Encounter E ON SE.EncounterId = E.Id " +
                       "JOIN BossFight BF ON E.BossFightId = BF.Id " +
                       "JOIN AuthUserCharacter AUC ON S.AuthUserCharacterId = AUC.Id " +
                       "JOIN Guild G ON AUC.GuildId = G.Id " +
                       "JOIN GuildStatus GS ON G.GuildStatusId = GS.Id " +
                       "WHERE BF.InstanceId = @instanceId AND GS.Active = 1 " +
                       "AND GS.Approved = 1 GROUP BY S.Id ORDER BY S.Date DESC";
            }
        }

        public static string GetSessionsForBossFightNoAuth
        {
            get
            {
                return "SELECT S.*, G.* FROM SessionEncounter SE " +
                       "JOIN Session S ON SE.SessionId = S.Id " +
                       "JOIN Encounter E ON SE.EncounterId = E.Id " +
                       "JOIN AuthUserCharacter AUC ON S.AuthUserCharacterId = AUC.Id " +
                       "JOIN Guild G ON AUC.GuildId = G.Id " +
                       "JOIN GuildStatus GS ON G.GuildStatusId = GS.Id " +
                       "WHERE E.BossFightId = @bossFightId AND G.HideSessions = 0 " +
                       "AND GS.Active = 1 AND GS.Approved = 1 " +
                       "GROUP BY S.Id ORDER BY S.Date DESC";
            }
        }

        public static string GetSessionsForBossFightNoAuthWithDifficulty
        {
            get
            {
                return "SELECT S.*, G.* FROM SessionEncounter SE " +
                       "JOIN Session S ON SE.SessionId = S.Id " +
                       "JOIN Encounter E ON SE.EncounterId = E.Id " +
                       "JOIN AuthUserCharacter AUC ON S.AuthUserCharacterId = AUC.Id " +
                       "JOIN Guild G ON AUC.GuildId = G.Id " +
                       "JOIN GuildStatus GS ON G.GuildStatusId = GS.Id " +
                       "WHERE E.BossFightId = @bossFightId AND G.HideSessions = 0 " +
                       "AND GS.Active = 1 AND GS.Approved = 1 AND E.EncounterDifficultyId = @difficultyId " +
                       "GROUP BY S.Id ORDER BY S.Date DESC";
            }
        }

        public static string GetSessionsForBossFightWithAuth
        {
            get
            {
                return "SELECT S.*, G.* FROM SessionEncounter SE " +
                       "JOIN Session S ON SE.SessionId = S.Id " +
                       "JOIN Encounter E ON SE.EncounterId = E.Id " +
                       "JOIN AuthUserCharacter AUC ON S.AuthUserCharacterId = AUC.Id " +
                       "JOIN Guild G ON AUC.GuildId = G.Id " +
                       "JOIN GuildStatus GS ON G.GuildStatusId = GS.Id " +
                       "WHERE E.BossFightId = @bossFightId AND G.HideSessions = 0 " +
                       "AND GS.Active = 1 AND GS.Approved = 1 " +
                       "GROUP BY S.Id " +
                       "UNION " +
                       "SELECT S.*, G.* FROM SessionEncounter SE " +
                       "JOIN Session S ON SE.SessionId = S.Id " +
                       "JOIN Encounter E ON SE.EncounterId = E.Id " +
                       "JOIN AuthUserCharacter AUC ON S.AuthUserCharacterId = AUC.Id " +
                       "JOIN AuthUser AU ON AUC.AuthUserId = AU.Id " +
                       "JOIN Guild G ON AUC.GuildId = G.Id " +
                       "JOIN GuildStatus GS ON G.GuildStatusId = GS.Id " +
                       "WHERE E.BossFightId = @bossFightId AND G.Id IN(" +
                       "SELECT DISTINCT G.Id FROM AuthUserCharacter AUC " +
                       "JOIN AuthUser AU ON AUC.AuthUserId = AU.Id " +
                       "JOIN Guild G ON AUC.GuildId = G.Id " +
                       "WHERE AU.Email = @email AND AUC.Removed = 0) " +
                       "AND GS.Active = 1 AND GS.Approved = 1 " +
                       "GROUP BY S.Id ORDER BY Date DESC";
            }
        }

        public static string GetSessionsForBossFightWithAuthWithDifficulty
        {
            get
            {
                return "SELECT S.*, G.* FROM SessionEncounter SE " +
                       "JOIN Session S ON SE.SessionId = S.Id " +
                       "JOIN Encounter E ON SE.EncounterId = E.Id " +
                       "JOIN AuthUserCharacter AUC ON S.AuthUserCharacterId = AUC.Id " +
                       "JOIN Guild G ON AUC.GuildId = G.Id " +
                       "JOIN GuildStatus GS ON G.GuildStatusId = GS.Id " +
                       "WHERE E.BossFightId = @bossFightId AND G.HideSessions = 0 " +
                       "AND GS.Active = 1 AND GS.Approved = 1 AND E.EncounterDifficultyId = @difficultyId " +
                       "GROUP BY S.Id " +
                       "UNION " +
                       "SELECT S.*, G.* FROM SessionEncounter SE " +
                       "JOIN Session S ON SE.SessionId = S.Id " +
                       "JOIN Encounter E ON SE.EncounterId = E.Id " +
                       "JOIN AuthUserCharacter AUC ON S.AuthUserCharacterId = AUC.Id " +
                       "JOIN AuthUser AU ON AUC.AuthUserId = AU.Id " +
                       "JOIN Guild G ON AUC.GuildId = G.Id " +
                       "JOIN GuildStatus GS ON G.GuildStatusId = GS.Id " +
                       "WHERE E.BossFightId = @bossFightId AND G.Id IN(" +
                       "SELECT DISTINCT G.Id FROM AuthUserCharacter AUC " +
                       "JOIN AuthUser AU ON AUC.AuthUserId = AU.Id " +
                       "JOIN Guild G ON AUC.GuildId = G.Id " +
                       "WHERE AU.Email = @email AND AUC.Removed = 0) " +
                       "AND GS.Active = 1 AND GS.Approved = 1 AND E.EncounterDifficultyId = @difficultyId " +
                       "GROUP BY S.Id ORDER BY Date DESC";
            }
        }

        public static string GetAllSessionsForBossFight
        {
            get
            {
                return "SELECT S.*, G.* FROM SessionEncounter SE " +
                       "JOIN Session S ON SE.SessionId = S.Id " +
                       "JOIN Encounter E ON SE.EncounterId = E.Id " +
                       "JOIN AuthUserCharacter AUC ON S.AuthUserCharacterId = AUC.Id " +
                       "JOIN Guild G ON AUC.GuildId = G.Id " +
                       "JOIN GuildStatus GS ON G.GuildStatusId = GS.Id " +
                       "WHERE E.BossFightId = @bossFightId AND GS.Active = 1 " +
                       "AND GS.Approved = 1 GROUP BY S.Id ORDER BY S.Date DESC";
            }
        }

        public static string GetAllSessionsForBossFightWithDifficulty
        {
            get
            {
                return "SELECT S.*, G.* FROM SessionEncounter SE " +
                       "JOIN Session S ON SE.SessionId = S.Id " +
                       "JOIN Encounter E ON SE.EncounterId = E.Id " +
                       "JOIN AuthUserCharacter AUC ON S.AuthUserCharacterId = AUC.Id " +
                       "JOIN Guild G ON AUC.GuildId = G.Id " +
                       "JOIN GuildStatus GS ON G.GuildStatusId = GS.Id " +
                       "WHERE E.BossFightId = @bossFightId AND GS.Active = 1 " +
                       "AND GS.Approved = 1 AND E.EncounterDifficultyId = @difficultyId " +
                       "GROUP BY S.Id ORDER BY S.Date DESC";
            }
        }
    }
}
