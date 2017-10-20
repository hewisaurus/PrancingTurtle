namespace Database.SQL
{
    public static class AuthUserCharacterGuildApplication 
    {
        public static string GetGuildNameForPendingApplication
        {
            get { return "SELECT G.Name FROM [AuthUserCharacterGuildApplication] AUCGA JOIN [Guild] G ON AUCGA.GuildId = G.Id WHERE AuthUserCharacterId = @authUserCharacterId"; }
        }

        public static string HasPendingApplicationForGuild
        {
            get
            {
                return "IF EXISTS(SELECT * FROM AuthUserCharacterGuildApplication WHERE AuthUserCharacterId = @authUserCharacterId AND GuildId = @guildId) SELECT 1";
            }
        }

        public static string GetAllPendingApplications
        {
            get
            {
                return "SELECT * FROM [AuthUserCharacterGuildApplication] AUCGA " +
                       "JOIN [AuthUserCharacter] AUC ON AUCGA.AuthUserCharacterId = AUC.Id " +
                       "JOIN [Guild] G ON AUCGA.GuildId = G.Id " +
                       "WHERE G.Id = @guildId " +
                       "ORDER BY AUC.CharacterName ASC";
            }
        }

        public static string GetPendingApplicationById
        {
            get
            {
                return "SELECT TOP 1 * FROM [AuthUserCharacterGuildApplication] AUCGA " +
                       "JOIN [AuthUserCharacter] AUC ON AUCGA.AuthUserCharacterId = AUC.Id " +
                       "JOIN [Guild] G ON AUCGA.GuildId = G.Id " +
                       "WHERE AUCGA.Id = @Id";
            }
        }

        public static string GetPendingApplicationByCharacterId
        {
            get
            {
                return "SELECT TOP 1 * FROM [AuthUserCharacterGuildApplication] AUCGA " +
                    "JOIN [AuthUserCharacter] AUC ON AUCGA.AuthUserCharacterId = AUC.Id " +
                    "JOIN [Guild] G ON AUCGA.GuildId = G.Id " +
                    "WHERE AUC.Id = @Id";
            }
        }
    }
}
