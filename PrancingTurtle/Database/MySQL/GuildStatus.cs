namespace Database.MySQL
{
    public static class GuildStatus
    {
        public static string GetDefaultApprovedStatus
        {
            get { return "SELECT * FROM GuildStatus WHERE Active = 1 AND Approved = 1 LIMIT 0,1"; }
        }

        public static string GetDefaultCreationStatus
        {
            get { return "SELECT * FROM GuildStatus WHERE DefaultStatus = 1 LIMIT 0,1"; }
        }
    }
}
