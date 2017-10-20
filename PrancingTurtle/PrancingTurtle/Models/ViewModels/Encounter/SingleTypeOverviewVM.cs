using System.Collections.Generic;
using Database.QueryModels;

namespace PrancingTurtle.Models.ViewModels.Encounter
{
    public class SingleTypeOverviewVM
    {
        public int EncounterId { get; set; }
        public List<OverviewPlayerSomethingDone> Records { get; set; }
    }
}