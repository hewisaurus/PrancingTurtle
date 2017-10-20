using System.Collections.Generic;
using Database.QueryModels;

namespace PrancingTurtle.Models.ViewModels.Encounter
{
    public class PlayerDamageDonePartialVM
    {
        public List<OverviewPlayerSomethingDone> Data { get; set; }
        public bool IsOverview { get; set; }
        public Database.Models.Encounter Encounter { get; set; }
    }
}