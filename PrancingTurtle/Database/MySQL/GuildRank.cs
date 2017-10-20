namespace Database.MySQL
{
    public static class GuildRank
    {
        public static class Default
        {
            public static string GuildCreator
            {
                get { return "SELECT * FROM GuildRank WHERE DefaultWhenCreated = 1 LIMIT 0,1"; }
            }
            public static string GuildAppApproved
            {
                get { return "SELECT * FROM GuildRank WHERE DefaultWhenApproved = 1 LIMIT 0,1"; }
            }
        }

        public static string GetAll
        {
            get { return "SELECT * FROM GuildRank ORDER BY Name"; }
        }

        public static string GetSingle
        {
            get { return "SELECT * FROM GuildRank WHERE Id = @id LIMIT 0,1"; }
        }
    }
}
