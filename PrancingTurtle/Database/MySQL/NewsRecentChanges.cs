namespace Database.MySQL
{
    public static class NewsRecentChanges
    {
        public const string GetRecentChanges =
            "SELECT * FROM NewsRecentChanges WHERE Visible = 1 ORDER BY ItemDate DESC";
    }
}
