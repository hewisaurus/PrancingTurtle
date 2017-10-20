namespace Database.SQL
{
    public static class Guild
    {
        public static string GetAll { get { return "SELECT * FROM Guild ORDER BY Name"; } }
         
        public static string CharacterIsInGuild
        {
            get
            {
                return "IF EXISTS(SELECT * FROM AuthUserCharacter WHERE Id = @authUserCharacterId AND GuildId = @guildId) SELECT 1";
                //return "SELECT COUNT(*) FROM AuthUserCharacter WHERE Id = @authUserCharacterId AND GuildId = @guildId";
            }
        }
    }
}
