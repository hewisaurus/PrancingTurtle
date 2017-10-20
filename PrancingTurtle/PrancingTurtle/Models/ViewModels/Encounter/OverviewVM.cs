using System;
using System.Collections.Generic;
using Database.Models;

namespace PrancingTurtle.Models.ViewModels.Encounter
{
    public class OverviewVM
    {
        public Database.Models.Encounter Encounter { get; set; }
        public Database.Models.Session Session { get; set; }

        public long AverageDps { get; set; }
        public long AverageHps { get; set; }
        public long AverageAps { get; set; }
        
        public List<EncounterDebuffAction> DebuffActions { get; set; }
        public List<EncounterBuffAction> BuffActions { get; set; }
        public List<EncounterNpcCast> NpcCasts { get; set; }
        public int PlayerDeaths { get; set; }

        public List<EncounterPlayerRole> PlayerRoles { get; set; }

        public TimeSpan BuildTime { get; set; }

        public bool LoadImage { get; set; }

        public string TimeZoneId { get; set; }

        public TimeSpan TimeZoneOffset
        {
            get { return TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId).GetUtcOffset(DateTime.UtcNow); }
        }

        public OverviewVM()
        {
            // Default the timezone to UTC
            TimeZoneId = "UTC";
        }
    }
}