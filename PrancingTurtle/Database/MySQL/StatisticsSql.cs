using System;

namespace Database.MySQL
{
    public static class StatisticsSql
    {
        public static string TotalUsers { get { return "SELECT COUNT(1) AS TotalUsers FROM AuthUser"; } }
        public static string TotalCharacters { get { return "SELECT COUNT(1) AS TotalCharacters FROM AuthUserCharacter"; } }
        public static string TotalGuilds { get { return "SELECT COUNT(DISTINCT GuildId) AS TotalGuilds FROM AuthUserCharacter WHERE GuildId IS NOT NULL"; } }
        public static string TotalSessions { get { return "SELECT COUNT(1) AS TotalSessions FROM Session"; } }
        public static string TotalEncounters { get { return "SELECT COUNT(1) AS TotalEncounters FROM Encounter"; } }
        public static string TotalEncountersNotRemoved { get { return "SELECT COUNT(1) AS TotalEncounters FROM Encounter WHERE Removed = 0"; } }

        public static string TotalPlayedTimeTicks
        {
            get { return "SELECT SUM(TotalPlayedTime) TotalTicks FROM SessionLog"; }
        }
        public static string TotalDurationNotSum
        {
            get
            {
                return "SELECT Duration FROM Session";
                //return "SELECT SEC_TO_TIME(SUM(TIME_TO_SEC(Duration))) AS TotalDuration FROM Session";
            }
        }
        [Obsolete]
        public static string TotalDamageRecords
        {
            get
            {
                return
                    "SELECT COUNT(1) AS Total, (COUNT(1) / (SELECT COUNT(1) AS TotalEncounters FROM Encounter)) AS Average FROM DamageDone";
            }
        }

        public static string TotalDamageRows
        {
            get { return "SELECT CAST(TABLE_ROWS as unsigned integer) AS Total FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'DamageDone'"; }
        }
        [Obsolete]
        public static string TotalHealingRecords
        {
            get
            {
                return
                    "SELECT COUNT(1) AS Total, (COUNT(1) / (SELECT COUNT(1) AS TotalEncounters FROM Encounter)) AS Average FROM HealingDone";
            }
        }
        public static string TotalHealingRows
        {
            get { return "SELECT CAST(TABLE_ROWS as unsigned integer) AS Total FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'HealingDone'"; }
        }
        [Obsolete]
        public static string TotalShieldingRecords
        {
            get
            {
                return
                    "SELECT COUNT(1) AS Total, (COUNT(1) / (SELECT COUNT(1) AS TotalEncounters FROM Encounter)) AS Average FROM ShieldingDone";
            }
        }

        public static string TotalShieldingRows
        {
            get { return "SELECT CAST(TABLE_ROWS as unsigned integer) AS Total FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ShieldingDone'"; }
        }
        public static string TotalLogSize { get { return "SELECT SUM(LogSize) AS LogSize FROM SessionLog"; } }
        public static string TotalLogLines { get { return "SELECT SUM(LogLines) AS TotalLines FROM SessionLog"; } }
        public static string TotalPlayers { get { return "SELECT COUNT(1) FROM Player"; } }

        public const string DamageRecords = "SELECT TABLE_ROWS AS Total FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'DamageDone'";
        public const string HealingRecords = "SELECT TABLE_ROWS AS Total FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'HealingDone'";
        public const string ShieldingRecords = "SELECT TABLE_ROWS AS Total FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ShieldingDone'";
        /// <summary>
        /// Combines all 3 tables into one query
        /// </summary>
        public const string RelevantRecords = "SELECT TABLE_NAME AS Name, TABLE_ROWS AS Rows FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME IN('DamageDone','HealingDone','ShieldingDone')";

        public const string Insert =
            "INSERT INTO DailyStats(Date,DamageRecords,HealingRecords,ShieldingRecords)VALUES(@date,@damageRecords,@healingRecords,@shieldingRecords)";
    }

}
