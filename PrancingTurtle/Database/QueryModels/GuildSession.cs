using System;
using System.Collections.Generic;

namespace Database.QueryModels
{
    public class GuildSession
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Duration { get; set; }
        public long TotalPlayTime { get; set; }
        public string Name { get; set; }
        public int AuthUserCharacterId { get; set; }
        public string UploadToken { get; set; }
        public string Filename { get; set; }
        public bool EncountersPublic { get; set; }
        public long SessionSize { get; set; }
        public List<string> InstancesSeen { get; set; }
        public List<string> BossesSeen { get; set; }
        public List<string> BossesSeenNotKilled { get; set; }
        public string TimeZoneId { get; set; }

        public TimeSpan TimeZoneOffset
        {
            get { return TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId).GetUtcOffset(DateTime.UtcNow); }
        }
        public Models.Guild Guild { get; set; }

        public GuildSession()
        {
            // Default the timezone to UTC
            TimeZoneId = "UTC";
        }
    }
}
