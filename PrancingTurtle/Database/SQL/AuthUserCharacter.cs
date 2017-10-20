namespace Database.SQL
{
    public static class AuthUserCharacter
    { 
        /// <summary>
        /// UPDATE AuthUserCharacter SET GuildRankId = @guildRankId WHERE Id = @authUserCharacterId AND GuildId = @guildId
        /// </summary>
        public static string UpdateGuildRankForCharacter
        {
            get { return "UPDATE AuthUserCharacter SET GuildRankId = @guildRankId WHERE Id = @authUserCharacterId AND GuildId = @guildId"; }
        }

        public static string AddCharacterToGuild
        {
            get { return "UPDATE AuthUserCharacter SET GuildId = @guildId, GuildRankId = @guildRankId WHERE Id = @authUserCharacterId"; }
        }
        /// <summary>
        /// Gets the ID of the guild that the user belongs to, if any. Requires the parameter @Id
        /// </summary>
        public static string GetGuildIdForCharacter
        {
            get { return "SELECT TOP 1 GuildId FROM AuthUserCharacter WHERE Id = @id"; }
        }
        /// <summary>
        /// Gets the query for retrieving a single AuthUserCharacter
        /// </summary>
        public static string GetSingle
        {
            get {  return "SELECT TOP 1 AUC.*, S.* FROM [AuthUser] AU JOIN [AuthUserCharacter] AUC ON AU.Id = AUC.AuthUserId JOIN [Shard] S ON AUC.ShardId = S.Id WHERE AU.Email = @email AND AUC.Id = @characterId"; }
        }

        
        /// <summary>
        /// Gets the query for returning all characters for an account
        /// </summary>
        public static string GetAllCharactersForEmail
        {
            get
            {
                return "SELECT AUC.*, S.*, G.*, GR.* FROM [AuthUser] AU " +
                       "JOIN [AuthUserCharacter] AUC ON AU.Id = AUC.AuthUserId " +
                       "JOIN [Shard] S ON AUC.ShardId = S.Id " +
                       "LEFT JOIN [Guild] G ON AUC.GuildId = G.Id " +
                       "LEFT JOIN [GuildRank] GR ON AUC.GuildRankId = GR.Id " +
                       "WHERE AU.Email = @email " +
                       "ORDER BY S.Name, AUC.CharacterName ASC";
            }
        }
        /// <summary>
        /// Gets the query for checking whether a user can join a character to a guild
        /// </summary>
        public static string CharacterCanJoinAGuild
        {
            get
            {
                return "SELECT COUNT(*) FROM [AuthUser] AU JOIN [AuthUserCharacter] AUC ON AU.Id = AUC.AuthUserId WHERE AU.Email = @email AND AUC.Id = @id AND AUC.GuildId IS NULL";
            }
        }
        /// <summary>
        /// Gets the query to count the number of members in a given guild
        /// </summary>
        public static string CountGuildMembers
        {
            get { return "SELECT COUNT(*) FROM [AuthUserCharacter] WHERE GuildId = @guildId"; }
        }
        /// <summary>
        /// Gets the query to return the character with the highest rank in a given guild
        /// </summary>
        public static string GetCharacterWithHighestGuildRank
        {
            get
            {
                return "SELECT TOP 1 * FROM [AuthUserCharacter] AUC " +
                       "JOIN [Guild] G ON AUC.GuildId = G.Id " +
                       "JOIN [GuildRank] GR ON AUC.GuildRankId = GR.Id " +
                       "WHERE AUC.GuildId = @guildId AND AUC.AuthUserId = @authUserId " +
                       "ORDER BY GR.RankPriority";
            }
        }
        /// <summary>
        /// Gets the query to return all guild members for a given guild
        /// </summary>
        public static string GetAllGuildMembers
        {
            get
            {
                return "SELECT * FROM [AuthUserCharacter] AUC " +
                       "JOIN [Guild] G ON AUC.GuildId = G.Id " +
                       "JOIN [GuildRank] GR ON AUC.GuildRankId = GR.Id " +
                       "WHERE AUC.GuildId = @guildId " +
                       "ORDER BY GR.RankPriority, AUC.CharacterName ASC";
            }
        }
        /// <summary>
        /// Gets the query that checks whether a character name exists on a given shard
        /// </summary>
        public static string CheckCharacterNameExists
        {
            get
            {
                return "IF EXISTS(SELECT * FROM [AuthUserCharacter] AUC " +
                       "JOIN [Shard] S ON AUC.ShardId = S.Id " +
                       "WHERE AUC.CharacterName = @characterName " +
                       "AND S.Id = @shardId) SELECT 1";
            }
        }

        public static string CheckMaxCharacterCountForAccount
        {
            get
            {
                return "SELECT COUNT(*) FROM [AuthUser] AU " +
                       "JOIN [AuthUserCharacter] AUC ON AU.Id = AUC.AuthUserId " +
                       "JOIN [Shard] S ON AUC.ShardId = S.Id " +
                       "WHERE AU.Email = @email AND S.Id = @shardId";
            }
        }
    }
}
