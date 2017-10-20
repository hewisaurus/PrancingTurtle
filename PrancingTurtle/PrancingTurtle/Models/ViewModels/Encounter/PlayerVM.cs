using System;
using System.Collections.Generic;
using Database.Models;
using Database.QueryModels;

namespace PrancingTurtle.Models.ViewModels.Encounter
{
    public class PlayerVM
    {
        public Database.Models.Encounter Encounter { get; set; }
        public Player Player { get; set; }

        public List<EncounterPlayerDamageDone> PlayerDamageDone { get; set; }
        public List<EncounterPlayerDamageDoneDetail> PlayerDamageDoneDetail { get; set; }

        public TimeSpan BuildTime { get; set; }

        public string TimeZoneId { get; set; }

        public TimeSpan TimeZoneOffset
        {
            get { return TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId).GetUtcOffset(DateTime.UtcNow); }
        }

        public PlayerVM()
        {
            // Default the timezone to UTC
            TimeZoneId = "UTC";
        }
    }
}