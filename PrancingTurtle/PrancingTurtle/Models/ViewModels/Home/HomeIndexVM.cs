using System;
using System.Collections.Generic;
using Database.Models;

namespace PrancingTurtle.Models.ViewModels.Home
{
    public class HomeIndexVM
    {
        public List<Database.Models.Session> Sessions { get; set; }
        public string TimeZoneId { get; set; }
        public List<NewsRecentChanges> RecentChanges { get; set; }
        public SiteNotification SiteNotification { get; set; }
        public bool DisplayNotification { get; set; }

        public TimeSpan TimeZoneOffset
        {
            get { return TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId).GetUtcOffset(DateTime.UtcNow); }
        }

        public HomeIndexVM()
        {
            Sessions = new List<Database.Models.Session>();
            // Default the timezone to UTC
            TimeZoneId = "UTC";
            RecentChanges = new List<NewsRecentChanges>();
            SiteNotification = new SiteNotification();
            DisplayNotification = false;
        }
    }
}