namespace Database.MySQL
{
    public static class Shard
    {
        public static string GetAll
        {
            get { return "SELECT * FROM Shard ORDER BY Name ASC"; }
        }

        public static string GetSingle
        {
            get { return "SELECT * FROM Shard WHERE Id = @id LIMIT 0,1"; }
        }
    }
}
