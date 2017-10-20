namespace Database.MySQL
{
    public static class NpcDeath
    {
        public static string GetAllInList
        {
            get { return "SELECT * FROM NpcDeath WHERE Name IN @names"; }
        }

        public static string UpdateItem
        {
            get { return "UPDATE NpcDeath SET Deaths = @deaths WHERE Name = @name"; }
        }
    }
}
