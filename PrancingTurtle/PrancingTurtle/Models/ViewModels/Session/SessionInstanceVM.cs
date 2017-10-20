using System;
using System.Collections.Generic;

namespace PrancingTurtle.Models.ViewModels.Session
{
    public class SessionInstanceVM
    {
        public Database.Models.Instance Instance { get; set; }
        public List<Database.QueryModels.GuildSession> Sessions { get; set; }
        public string TimeZoneId { get; set; }

        public TimeSpan TimeZoneOffset
        {
            get { return TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId).GetUtcOffset(DateTime.UtcNow); }
        }

        public SessionInstanceVM()
        {
            // Default the timezone to UTC
            TimeZoneId = "UTC";
        }
    }
}