using System;
using System.Collections.Generic;
using Database.QueryModels;

namespace PrancingTurtle.Models.ViewModels.Encounter
{
    public class CharacterTargetBreakdownVM
    {
        public List<EncounterCharacterActionBreakdown> Data { get; set; }
        public Database.Models.Encounter Encounter { get; set; }
        public string CharacterName { get; set; }
        public TimeSpan BuildTime { get; set; }
        public string TimeZoneId { get; set; }

        public string TotalText { get; set; }
        public string AverageText { get; set; }

        public TimeSpan TimeZoneOffset
        {
            get { return TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId).GetUtcOffset(DateTime.UtcNow); }
        }

        public CharacterTargetBreakdownVM()
        {
            // Default the timezone to UTC
            TimeZoneId = "UTC";
        }
    }
}