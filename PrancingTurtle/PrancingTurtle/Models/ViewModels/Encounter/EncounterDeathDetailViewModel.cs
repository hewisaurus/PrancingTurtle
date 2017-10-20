using System.Collections.Generic;
using Database.Models;

namespace PrancingTurtle.Models.ViewModels.Encounter
{
    public class EncounterDeathDetailViewModel
    {
        public Database.Models.Encounter Encounter { get; set; }
        public Database.Models.Session Session { get; set; }
        public string BuildTime { get; set; }


        //public int EncounterId { get; set; }
        public Player Player { get; set; }
        //public string EncounterName { get; set; }
        //public string InstanceName { get; set; }
        //public TimeSpan EncounterLength { get; set; }
        //public DateTime EncounterStart { get; set; }
        //public bool EncounterSuccess { get; set; }
        public int SecondsElapsed { get; set; }
        public List<Database.QueryModels.EncounterDeathEvent> DeathEvents { get; set; }

        public EncounterDeathDetailViewModel()
        {
            DeathEvents = new List<Database.QueryModels.EncounterDeathEvent>();
        }
    }
}