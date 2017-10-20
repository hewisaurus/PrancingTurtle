using System;

namespace Database.QueryModels
{
    public class SiteStats
    {
        public long TotalUsers { get; set; }
        public long TotalCharacters { get; set; }
        public long TotalPlayers { get; set; }
        public long TotalGuilds { get; set; }
        public long TotalSessions { get; set; }
        public long TotalEncounters { get; set; }

        public long TotalDamageRecords { get; set; }
        public long AverageDamageRecords { get; set; }
        public long TotalHealingRecords { get; set; }
        public long AverageHealingRecords { get; set; }
        public long TotalShieldingRecords { get; set; }
        public long AverageShieldingRecords { get; set; }

        public long TotalLogSize { get; set; }
        public long TotalLogLines { get; set; }
        public long TotalPlayedTimeTicks { get; set; }

        public TimeSpan TotalTimeSpan { get; set; }

        //public SiteStatsSessionDuration SessionDuration { get; set; }
        public TimeSpan TotalDuration { get; set; }

        public TimeSpan AverageSessionDuration
        {
            get
            {
                //if (SessionDuration == null || TotalDuration == new TimeSpan()) return new TimeSpan();
                if (TotalDuration == new TimeSpan()) return new TimeSpan();

                return new TimeSpan(TotalDuration.Ticks / TotalSessions);
            }
        }
    }
}
