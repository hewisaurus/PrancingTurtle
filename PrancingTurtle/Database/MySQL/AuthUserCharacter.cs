namespace Database.MySQL
{
    public static class AuthUserCharacter
    {
        /// <summary>
        /// This query includes the shard, and requires @email and @characterId
        /// </summary>
        public static string GetSingle
        {
            get
            {
                return "SELECT AUC.*, S.* FROM AuthUser AU " +
                       "JOIN AuthUserCharacter AUC ON AU.Id = AUC.AuthUserId " +
                       "JOIN Shard S ON AUC.ShardId = S.Id " +
                       "WHERE AU.Email = @email AND AUC.Id = @characterId AND AUC.Removed = 0 " +
                       "LIMIT 0,1";
            }
        }

        public static string Get
        {
            get { return "SELECT * FROM AuthUserCharacter WHERE Id = @id AND Removed = 0 LIMIT 1"; }
        }

        public static string GetAllCharactersForEmail
        {
            get
            {
                return "SELECT AUC.Id As Id, AUC.CharacterName AS CharacterName, S.Id AS ShardId, S.Name AS ShardName, " +
                       "G.Id AS GuildId, G.Name AS GuildName " +
                       "FROM AuthUser AU " +
                       "JOIN AuthUserCharacter AUC ON AU.Id = AUC.AuthUserId " +
                       "JOIN Shard S ON AUC.ShardId = S.Id " +
                       "LEFT JOIN Guild G ON AUC.GuildId = G.Id " +
                       "WHERE AU.Email = @email AND AUC.Removed = 0 " +
                       "ORDER BY S.Name, AUC.CharacterName ASC";
                //return "SELECT AUC.Id As Id, AUC.CharacterName AS CharacterName, S.Id AS ShardId, S.Name AS ShardName, " +
                //       "GR.Name AS RankName, G.Id AS GuildId, G.Name AS GuildName, GR.* " +
                //       "FROM AuthUser AU " +
                //       "JOIN AuthUserCharacter AUC ON AU.Id = AUC.AuthUserId " +
                //       "JOIN Shard S ON AUC.ShardId = S.Id " +
                //       "LEFT JOIN Guild G ON AUC.GuildId = G.Id " +
                //       "LEFT JOIN GuildRank GR ON AUC.GuildRankId = GR.Id " +
                //       "WHERE AU.Email = @email AND AUC.Removed = 0 " +
                //       "ORDER BY S.Name, AUC.CharacterName ASC";
            }
        }

        public static string GetGuildIdsForEmail
        {
            get { return "SELECT DISTINCT G.Id FROM AuthUserCharacter AUC " +
                         "JOIN AuthUser AU ON AUC.AuthUserId = AU.Id " +
                         "JOIN Guild G ON AUC.GuildId = G.Id " +
                         "WHERE AU.Email = @email AND AUC.Removed = 0"; } 
        }
        
        public static string GetAllUploadersForEmail
        {
            get
            {
                return "SELECT AUC.*, S.*, G.*, GR.* " +
                       "FROM AuthUser AU JOIN AuthUserCharacter AUC ON AU.Id = AUC.AuthUserId " +
                       "JOIN Shard S ON AUC.ShardId = S.Id JOIN Guild G ON AUC.GuildId = G.Id " +
                       "JOIN GuildRank GR ON AUC.GuildRankId = GR.Id " +
                       "WHERE AU.Email = @email AND AUC.Removed = 0 AND GR.CanUploadLogs = 1 " +
                       "ORDER BY S.Name, AUC.CharacterName ASC";
            }
        }

        public static string CountGuildMembers
        {
            get { return "SELECT COUNT(1) FROM AuthUserCharacter WHERE GuildId = @guildId AND Removed = 0"; }
        }

        public static string GetCharacterWithHighestGuildRank
        {
            get
            {
                return "SELECT * FROM AuthUserCharacter AUC " +
                       "JOIN Guild G ON AUC.GuildId = G.Id " +
                       "JOIN GuildRank GR ON AUC.GuildRankId = GR.Id " +
                       "WHERE AUC.GuildId = @guildId AND AUC.AuthUserId = @authUserId AND AUC.Removed = 0 " +
                       "ORDER BY GR.RankPriority " +
                       "LIMIT 0,1";
            }
        }

        public static string GetAllGuildMembers
        {
            get
            {
                return "SELECT * FROM AuthUserCharacter AUC " +
                       "JOIN Guild G ON AUC.GuildId = G.Id " +
                       "JOIN GuildRank GR ON AUC.GuildRankId = GR.Id " +
                       "WHERE AUC.GuildId = @guildId AND AUC.Removed = 0 " +
                       "ORDER BY AUC.CharacterName, GR.RankPriority";
            }
        }

        public static string CharacterIsInGuild
        {
            get
            {
                return "SELECT IF (EXISTS(SELECT * FROM AuthUserCharacter WHERE Id = @authUserCharacterId AND GuildId = @guildId), 1, 0) AS InGuild";
            }
        }

        public static string UpdateGuildRankForCharacter
        {
            get { return "UPDATE AuthUserCharacter SET GuildRankId = @guildRankId WHERE Id = @authUserCharacterId AND GuildId = @guildId"; }
        }

        public static string CharacterExistsOnShard
        {
            get
            {
                return "SELECT IF (EXISTS(" +
                       "SELECT * FROM AuthUserCharacter AUC " +
                       "JOIN Shard S ON AUC.ShardId = S.Id " +
                       "WHERE AUC.CharacterName = @characterName AND AUC.Removed = 0 AND " +
                       "S.Id = @shardId), 1, 0) AS CharacterExists";
            }
        }
        public static string CheckMaxCharacterCountForAccount
        {
            get
            {
                return "SELECT COUNT(1) FROM AuthUser AU " +
                       "JOIN AuthUserCharacter AUC ON AU.Id = AUC.AuthUserId " +
                       "JOIN Shard S ON AUC.ShardId = S.Id " +
                       "WHERE AU.Email = @email AND AUC.Removed = 0 " +
                       "AND S.Id = @shardId";
            }
        }
        public static string AddCharacterToGuild
        {
            get { return "UPDATE AuthUserCharacter SET GuildId = @guildId, GuildRankId = @guildRankId WHERE Id = @authUserCharacterId"; }
        }
        public static string GetGuildIdForCharacter
        {
            get { return "SELECT GuildId FROM AuthUserCharacter WHERE Id = @id LIMIT 0,1"; }
        }

        public static string GetGuildRankForCharacter
        {
            get { return "SELECT GR.* FROM AuthUserCharacter AUC JOIN GuildRank GR ON GR.Id = AUC.GuildRankId WHERE AUC.Id = @id"; }
        }
        public static string CharacterCanJoinAGuild
        {
            get
            {
                return "SELECT IF(EXISTS(" +
                       "SELECT * FROM AuthUser AU " +
                       "JOIN AuthUserCharacter AUC ON AUC.AuthUserId = AU.Id " +
                       "WHERE AUC.Id = @userCharacterId AND " +
                       "AU.Email = @email " +
                       "AND AUC.GuildId IS NULL ), 1, 0) " +
                       "AS CanJoinAGuild";
            }
        }

        public static string MarkCharacterRemoved
        {
            get { return "UPDATE AuthUserCharacter SET Removed = 1 WHERE Id = @id"; }
        }
        public static string MarkCharacterRemovedIncludingGuildId
        {
            get { return "UPDATE AuthUserCharacter SET Removed = 1 WHERE Id = @id AND GuildId = @guildId"; }
        }
        public static string HasCreatedSessions
        {
            get
            {
                return "SELECT IF(EXISTS(SELECT * FROM Session WHERE AuthUserCharacterId = @id), 1, 0) AS CreatedSessions";
            }
        }

        public static string HasUploadedLogs
        {
            get
            {
                return "SELECT IF(EXISTS(SELECT * FROM SessionLog WHERE AuthUserCharacterId = @id), 1, 0) AS UploadedLogs";
            }
        }
    }
}