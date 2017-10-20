using System;
using System.Collections.Generic;
using Database.QueryModels.Misc;

namespace PrancingTurtle.Models.ViewModels.Session
{
    public class SessionPlayerVM
    {
        public PlayerSearchResult Player { get; set; }
        public List<Database.Models.Session> Sessions { get; set; }
        public string TimeZoneId { get; set; }

        public TimeSpan TimeZoneOffset
        {
            get { return TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId).GetUtcOffset(DateTime.UtcNow); }
        }

        public SessionPlayerVM()
        {
            // Default the timezone to UTC
            TimeZoneId = "UTC";
        }
    }
}