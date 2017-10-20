namespace Database.MySQL
{
    public static class ScheduledTask
    {
        public static string GetByName
        {
            get { return "SELECT * FROM ScheduledTask WHERE Name = @name LIMIT 1"; }
        }

        public static string UpdateRunTime
        {
            get { return "UPDATE ScheduledTask SET LastRun = @lastRun WHERE Id = @id"; }
        }
    }
}
