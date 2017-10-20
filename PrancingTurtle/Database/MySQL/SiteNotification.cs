namespace Database.MySQL
{
    public static class SiteNotification
    {
        public const string GetNotification =
            "SELECT * FROM SiteNotification WHERE Visible = 1 ORDER BY Created DESC LIMIT 1";
    }
}
