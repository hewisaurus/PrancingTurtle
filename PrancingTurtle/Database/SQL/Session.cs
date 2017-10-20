namespace Database.SQL
{
    public static class Session
    {
        /// <summary>
        /// Get Session IDs for sessions longer than 1 minute that have no TotalPlayTime
        /// </summary> 
        public static string SessionsWithNoTotalPlayTime
        {
            get { return "SELECT * FROM Session WHERE TotalPlayTime IS NULL AND ((DATEPART(HOUR, Duration) * 3600) + (DATEPART(MINUTE, Duration) * 60)) > 0"; }
        }

        public static string EncounterDateDuration
        {
            get { return "SELECT E.Date, E.Duration FROM SessionEncounter SE JOIN Encounter E ON SE.EncounterId = E.Id WHERE SE.SessionId = @sessionId ORDER BY E.Date"; }
        }

        public static string SessionTokenExists
        {
            get { return "SELECT COUNT(*) FROM Session WHERE UploadToken = @uploadToken"; }
        }

        public static string GetSessionsWithNoLog
        {
            get { return "SELECT S.* FROM Session S " +
                         "JOIN AuthUserCharacter AUC ON S.AuthUserCharacterId = AUC.Id " +
                         "LEFT JOIN SessionLog SL ON SL.SessionId = S.Id WHERE SL.Id IS NULL " +
                         "AND S.Filename IS NOT NULL AND AUC.GuildId IS NOT NULL";
            }
        }

        public static string GetSessionLogs
        {
            get { return "SELECT * FROM SessionLog WHERE SessionId = @sessionId"; }
        }
    }
}
