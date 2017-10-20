namespace Database.MySQL
{
    public static class AuthUserCharacterGuildApplication
    {
        public static string GetAllGuildApps
        {
            get
            {
                return "SELECT * FROM AuthUserCharacterGuildApplication AUCGA " +
                       "JOIN AuthUserCharacter AUC ON AUCGA.AuthUserCharacterId = AUC.Id " +
                       "JOIN Guild G ON AUCGA.GuildId = G.Id " +
                       "WHERE G.Id = @guildId " +
                       "ORDER BY AUC.CharacterName ASC";
            }
        }

        public static string GetGuildNameForPendingApplication
        {
            get
            {
                return "SELECT G.Name FROM AuthUserCharacterGuildApplication AUCGA " +
                       "JOIN Guild G ON AUCGA.GuildId = G.Id " +
                       "WHERE AuthUserCharacterId = @authUserCharacterId " +
                       "LIMIT 0,1";
            }
        }

        public static string HasPendingApplicationForGuild
        {
            get
            {
                return "SELECT IF (EXISTS(" +
                       "SELECT * FROM AuthUserCharacterGuildApplication " +
                       "WHERE AuthUserCharacterId = @authUserCharacterId " +
                       "AND GuildId = @guildId), 1, 0) AS HasPending";
            }
        }

        public static string GetPendingApplicationById
        {
            get
            {
                return "SELECT * FROM AuthUserCharacterGuildApplication AUCGA " +
                       "JOIN AuthUserCharacter AUC ON AUCGA.AuthUserCharacterId = AUC.Id " +
                       "JOIN Guild G ON AUCGA.GuildId = G.Id " +
                       "WHERE AUCGA.Id = @Id " +
                       "LIMIT 0,1";
            }
        }

        public static string GetPendingApplicationByCharacterId
        {
            get
            {
                return "SELECT * FROM AuthUserCharacterGuildApplication AUCGA " +
                    "JOIN AuthUserCharacter AUC ON AUCGA.AuthUserCharacterId = AUC.Id " +
                    "JOIN Guild G ON AUCGA.GuildId = G.Id " +
                    "WHERE AUC.Id = @Id LIMIT 0,1";
            }
        }

        public static string CharacterHasAPendingApplication
        {
            get
            {
                return "SELECT IF(EXISTS(SELECT * FROM AuthUserCharacterGuildApplication WHERE AuthUserCharacterId = @authUserCharacterId), 1, 0) AS HasExistingApplication";
            }
        }
    }
}
