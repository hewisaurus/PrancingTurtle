namespace Database.SQL
{
    public static class Stat
    {
        public static string TotalUsers { get { return "SELECT COUNT(*) AS TotalUsers FROM AuthUser"; } }
        public static string TotalCharacters { get { return "SELECT COUNT(*) AS TotalCharacters FROM AuthUserCharacter"; } }
        public static string TotalGuilds { get { return "SELECT COUNT(DISTINCT GuildId) AS TotalGuilds FROM AuthUserCharacter WHERE GuildId IS NOT NULL"; } }
        public static string TotalSessions { get { return "SELECT COUNT(*) AS TotalSessions FROM Session"; } }
        public static string TotalEncounters { get { return "SELECT COUNT(*) AS TotalEncounters FROM Encounter"; } }
         
        public static string TotalPlayedTimeTicks
        {
            get { return "SELECT SUM(TotalPlayedTime) AS TotalTicks FROM SessionLog"; }
        }

        public static string TotalDuration
        {
            get { return "SELECT SUM(DATEPART(HOUR, Duration)) AS TotalHours, " +
                         "SUM(DATEPART(MINUTE, Duration)) AS TotalMinutes, " +
                         "SUM(DATEPART(SECOND, Duration)) AS TotalSeconds FROM Session"; }
        }

        public static string TotalDamageRecords
        {
            get
            {
                return
                    "SELECT COUNT(*) AS Total, (COUNT(*) / (SELECT COUNT(*) AS TotalEncounters FROM Encounter)) AS Average FROM DamageDone";
            }
        }

        public static string TotalHealingRecords
        {
            get
            {
                return
                    "SELECT COUNT(*) AS Total, (COUNT(*) / (SELECT COUNT(*) AS TotalEncounters FROM Encounter)) AS Average FROM HealingDone";
            }
        }

        public static string TotalShieldingRecords
        {
            get
            {
                return
                    "SELECT COUNT(*) AS Total, (COUNT(*) / (SELECT COUNT(*) AS TotalEncounters FROM Encounter)) AS Average FROM ShieldingDone";
            }
        }

        public static string TotalLogSize { get { return "SELECT SUM(LogSize) FROM SessionLog"; } } 
    }
}
