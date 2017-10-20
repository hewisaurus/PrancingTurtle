using System;
using System.Collections.Generic;
using DotNet.Highcharts;

namespace PrancingTurtle.Models.ViewModels.Encounter
{
    public class CharacterBuffs
    {
        //public Player Player { get; set; }
        public string TargetName { get; set; }
        public Highcharts Graph { get; set; }
        public Database.Models.Encounter Encounter { get; set; }
        public TimeSpan BuildTime { get; set; }
        public List<string> DebugBuildTime { get; set; } 

        public string TimeZoneId { get; set; }

        public TimeSpan TimeZoneOffset
        {
            get { return TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId).GetUtcOffset(DateTime.UtcNow); }
        }

        public CharacterBuffs()
        {
            // Default the timezone to UTC
            TimeZoneId = "UTC";
        }
    }
}