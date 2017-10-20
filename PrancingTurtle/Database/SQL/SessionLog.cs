namespace Database.SQL
{
    public static class SessionLog
    {
        /// <summary>
        /// Get SessionLog IDs for sessionLogs that have no TotalPlayTime
        /// </summary>
        public static string SessionLogsWithNoTotalPlayTime 
        {
            get { return "SELECT * FROM SessionLog WHERE TotalPlayedTime = 0"; }
        }
    }
}
