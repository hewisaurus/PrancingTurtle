using System;

namespace Database.MySQL
{
    public static class Guild
    {
        public static string CountMembersSessionsEncountersForGuild
        {
            get
            {
                return "SELECT * FROM " +
                       "(SELECT COUNT(1) AS MemberCount FROM AuthUserCharacter WHERE GuildId = @guildId AND Removed = 0) C1, " +
                       "(SELECT COUNT(1) AS SessionCount FROM Session S JOIN AuthUserCharacter AUC ON S.AuthUserCharacterId = AUC.Id WHERE AUC.GuildId = @guildId) C2, " +
                       "(SELECT COUNT(1) AS EncounterCount FROM Encounter WHERE GuildId = @guildId) C3";
            }
        }
        public static string GetGuildNavigation
        {
            get
            {
                return "SELECT * FROM Guild G JOIN Shard S ON G.ShardId = S.Id ORDER BY S.Name, G.Name ASC";
            }
        }

        public static string GetGuildById
        {
            get
            {
                return "SELECT * FROM Guild G JOIN Shard S ON G.ShardId = S.Id JOIN GuildStatus GS ON G.GuildStatusId = GS.Id WHERE G.Id = @id";
            }
        }

        public static string GetGuildByName
        {
            get
            {
                return "SELECT * FROM Guild G JOIN Shard S ON G.ShardId = S.Id JOIN GuildStatus GS ON G.GuildStatusId = GS.Id WHERE G.Name = @name LIMIT 1";
            }
        }

        public static string GuildHasMembers
        {
            get { return "SELECT IF(EXISTS(SELECT * FROM AuthUserCharacter WHERE GuildId = @guildId), 1, 0) AS GuildHasMembers"; }
        }

        public static string RemoveGuild
        {
            get { return "DELETE FROM Guild WHERE Id = @guildId"; }
        }

        public static string Approve
        {
            get { return "UPDATE Guild SET GuildStatusId = @guildStatusId WHERE Id = @guildId"; }
        }

        public static string ExistsOnShard
        {
            get { return "SELECT IF (EXISTS(SELECT * FROM Guild WHERE Name = @guildName AND ShardId = @shardId), 1, 0) AS GuildExists"; }
        }

        public static string GuildsAcceptingApplications
        {
            get 
            { 
                return "SELECT G.* FROM Guild G " +
                    "JOIN Shard S ON G.ShardId = S.Id " +
                    "JOIN GuildStatus GS ON G.GuildStatusId = GS.Id " +
                    "WHERE S.Id = @shardId AND " +
                    "GS.PlayersCanApply = 1 ORDER BY G.Name ASC"; 
            }
        }

        public static string AllGuildsOnShard
        {
            get
            {
                return "SELECT G.* FROM Guild G " +
                    "JOIN Shard S ON G.ShardId = S.Id " +
                    "JOIN GuildStatus GS ON G.GuildStatusId = GS.Id " +
                    "WHERE S.Id = @shardId ORDER BY G.Name ASC";
            }
        }

        public static string SearchForAllGuilds
        {
            get
            {
                return "SELECT * FROM Guild G JOIN Shard S ON G.ShardId = S.Id " +
                       "WHERE G.Name LIKE @searchTerm ORDER BY G.Name ASC, S.Name ASC";
            }
        }

        public static string SearchForGuildNoAuth
        {
            get 
            { 
                return 
                    "SELECT * FROM Guild G JOIN Shard S ON G.ShardId = S.Id " +
                    "WHERE G.Name LIKE @searchTerm AND G.HideFromSearch = 0 ORDER BY G.Name ASC, S.Name ASC";
            }
        }

        public static string SearchForGuildWithAuth
        {
            get
            {
                return "SELECT G.*, S.* FROM Guild G " +
                       "JOIN Shard S ON G.ShardId = S.Id " +
                       "JOIN GuildStatus GS ON G.GuildStatusId = GS.Id " +
                       "WHERE G.Name LIKE @searchTerm AND G.HideFromSearch = 0 " +
                       "AND GS.Approved = 1 " +
                       "UNION " +
                       "SELECT G.*, S.* FROM Guild G " +
                       "JOIN Shard S ON G.ShardId = S.Id " +
                       "JOIN GuildStatus GS ON G.GuildStatusId = GS.Id " +
                       "JOIN AuthUserCharacter AUC ON AUC.GuildId = G.Id " +
                       "JOIN AuthUser AU ON AUC.AuthUserId = AU.Id " +
                       "WHERE G.Name LIKE @searchTerm AND GS.Approved = 1 " +
                       "AND AU.Email = @email AND AUC.Removed = 0";
            }
        }
        #region Guild visibility controls
        public static string SetRosterPrivate
        {
            get { return "UPDATE Guild SET HideRoster = 1 WHERE Id = @guildId"; }
        }

        public static string SetRosterPublic
        {
            get { return "UPDATE Guild SET HideRoster = 0 WHERE Id = @guildId"; }
        }

        public static string SetListsPrivate
        {
            get { return "UPDATE Guild SET HideFromLists = 1 WHERE Id = @guildId"; }
        }

        public static string SetListsPublic
        {
            get { return "UPDATE Guild SET HideFromLists = 0 WHERE Id = @guildId"; }
        }

        public static string SetSearchPrivate
        {
            get { return "UPDATE Guild SET HideFromSearch = 1 WHERE Id = @guildId"; }
        }

        public static string SetSearchPublic
        {
            get { return "UPDATE Guild SET HideFromSearch = 0 WHERE Id = @guildId"; }
        }

        public static string SetRankingsPrivate
        {
            get { return "UPDATE Guild SET HideFromRankings = 1 WHERE Id = @guildId"; }
        }

        public static string SetRankingsPublic
        {
            get { return "UPDATE Guild SET HideFromRankings = 0 WHERE Id = @guildId"; }
        }

        public static string SetSessionsPrivate
        {
            get { return "UPDATE Guild SET HideSessions = 1 WHERE Id = @guildId"; }
        }

        public static string SetSessionsPublic
        {
            get { return "UPDATE Guild SET HideSessions = 0 WHERE Id = @guildId"; }
        }

        public static string SetProgressionPrivate
        {
            get { return "UPDATE Guild SET HideProgression = 1 WHERE Id = @guildId"; }
        }

        public static string SetProgressionPublic
        {
            get { return "UPDATE Guild SET HideProgression = 0 WHERE Id = @guildId"; }
        }
        #endregion

        public static string ListVisibleGuildsNoAuth
        {
            get
            {
                return "SELECT G.* FROM Guild G " +
                       "JOIN GuildStatus GS ON G.GuildStatusId = GS.Id " +
                       "JOIN Shard S ON G.ShardId = S.Id " +
                       "WHERE GS.Active = 1 AND GS.Approved = 1 " +
                       "AND G.HideFromLists = 0";
            }
        }

        public static string ListAllApprovedGuilds
        {
            get
            {
                return "SELECT G.*, S.* FROM Guild G " +
                       "JOIN GuildStatus GS ON G.GuildStatusId = GS.Id " +
                       "JOIN Shard S ON G.ShardId = S.Id " +
                       "WHERE GS.Active = 1 AND GS.Approved = 1 ORDER BY G.Name";
            }
        }

        public static string ListVisibleGuildsWithAuth
        {
            get { return "SELECT G.* FROM Guild G " +
                         "JOIN GuildStatus GS ON G.GuildStatusId = GS.Id " +
                         "JOIN Shard S ON G.ShardId = S.Id " +
                         "WHERE GS.Active = 1 AND GS.Approved = 1 " +
                         "AND G.HideFromLists = 0 " +
                         "UNION " +
                         "SELECT DISTINCT G.* FROM AuthUserCharacter AUC " +
                         "JOIN AuthUser AU ON AUC.AuthUserId = AU.Id " +
                         "JOIN Guild G ON AUC.GuildId = G.Id JOIN " +
                         "GuildStatus GS ON G.GuildStatusId = GS.Id " +
                         "WHERE AU.Email = @email AND AUC.Removed = 0 " +
                         "AND GS.Active = 1 AND GS.Approved = 1"; }
        }

        [Obsolete]
        public static string GetBossFightsCleared
        {
            get
            {
                return "SELECT BossFightId FROM Encounter WHERE GuildId = @guildId AND SuccessfulKill = 1 GROUP BY BossFightId";
            }
        }

        public static string GetGuildBossFightProgression
        {
            get
            {
                return "SELECT I.Id AS InstanceId, I.Name AS InstanceName, BF.Id AS BossFightId, " +
                       "BF.Name AS BossFightName, ED.Id AS DifficultyId, ED.Name AS DifficultyName, ED.ShortName AS DifficultyShortName, " +
                       "(IF(SUM(E.SuccessfulKill) > 0, 1, 0)) AS Killed, " +
                       "(SELECT MIN(Duration) FROM Encounter WHERE BossFightId = BFD.BossFightId AND EncounterDifficultyId = BFD.EncounterDifficultyId " +
                       "AND SuccessfulKill = 1 AND ValidForRanking = 1 AND GuildId = @guildId) AS BestTime, " +
                       "(SELECT (IF(SUM(E.SuccessfulKill) > 0, " +
                       "(SELECT COUNT(DISTINCT(Duration)) FROM Encounter " +
                       "WHERE Duration <= " +
                       "(SELECT MIN(Duration) FROM Encounter WHERE BossFightId = BFD.BossFightId AND EncounterDifficultyId = BFD.EncounterDifficultyId " +
                       "AND SuccessfulKill = 1 AND ValidForRanking = 1 AND GuildId = @guildId) AND SuccessfulKill = 1 AND ValidForRanking = 1 " +
                       "AND BossFightId = BFD.BossFightId AND EncounterDifficultyId = BFD.EncounterDifficultyId), " +
                       "0))) AS Rank " +
                       "FROM BossFightDifficulty BFD " +
                       "JOIN BossFight BF ON BFD.BossFightId = BF.Id " +
                       "JOIN EncounterDifficulty ED ON BFD.EncounterDifficultyId = ED.Id " +
                       "JOIN Instance I ON BF.InstanceId = I.Id " +
                       "LEFT JOIN Encounter E ON E.BossFightId = BFD.BossFightId AND E.EncounterDifficultyId = BFD.EncounterDifficultyId AND E.GuildId = @guildId " +
                       "WHERE I.IncludeInProgression = 1 " +
                       "GROUP BY BFD.BossFightId, BFD.EncounterDifficultyId ORDER BY I.Name, BF.Name, ED.Priority DESC";
            }
        }
    }
}
