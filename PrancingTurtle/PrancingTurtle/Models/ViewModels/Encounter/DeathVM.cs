using System;
using System.Collections.Generic;
using System.Linq;
using Database.Models;

namespace PrancingTurtle.Models.ViewModels.Encounter
{
    public class DeathVM
    {
        public Database.Models.Encounter Encounter { get; set; }
        public Database.Models.Session Session { get; set; }

        public TimeSpan BuildTime { get; set; }
        public List<EncounterDeath> Deaths { get; set; }

        public List<string> DeathStrings
        {
            get
            {
                var returnValue = new List<string>();
                if (Deaths == null) return returnValue;
                if (Deaths.Count == 0) return returnValue;

                foreach (
                    var deathGroup in
                        Deaths.Where(d => d.TargetPlayerId != null)
                            .GroupBy(d => new {d.TargetPlayerId, d.TargetPlayer.Name})
                            .OrderByDescending(d => d.Count()))
                {
                    string grpLine = string.Format("{0} ({1}): ", deathGroup.Key.Name, deathGroup.Count());
                    foreach (var deathRecord in deathGroup)
                    {
                        grpLine += string.Format("{0} ({1})", deathRecord.Ability.Name, deathRecord.SecondsElapsed);
                    }
                    returnValue.Add(grpLine);
                }
                return returnValue;
            }
        }
        public string TimeZoneId { get; set; }

        public TimeSpan TimeZoneOffset
        {
            get { return TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId).GetUtcOffset(DateTime.UtcNow); }
        }

        public DeathVM()
        {
            // Default the timezone to UTC
            TimeZoneId = "UTC";
        }
    }
}