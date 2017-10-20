using System.Collections.Generic;

namespace PrancingTurtle.Models.ViewModels.Session
{
    public class RemoveEncountersVM
    {
        public int SessionId { get; set; }
        public List<int> EncounterIds { get; set; }

        public RemoveEncountersVM()
        {
            EncounterIds = new List<int>();
        }
    }
}