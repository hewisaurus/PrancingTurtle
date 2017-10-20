using System;
using System.Collections.Generic;

namespace PrancingTurtle.Models.ViewModels
{
    public class MainIndexVM
    {
        public IEnumerable<Database.Models.Encounter> Encounters { get; set; }
        public string TimeZoneId { get; set; }

        public TimeSpan TimeZoneOffset
        {
            get { return TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId).GetUtcOffset(DateTime.UtcNow); }
        }

        public MainIndexVM()
        {
            // Default the timezone to UTC
            TimeZoneId = "UTC";
        }
    }
}