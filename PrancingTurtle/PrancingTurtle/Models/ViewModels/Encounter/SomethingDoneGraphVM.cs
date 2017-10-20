using System.Collections.Generic;
using Database.QueryModels;
using DotNet.Highcharts;

namespace PrancingTurtle.Models.ViewModels.Encounter
{
    public class SomethingDoneGraphVM
    {
        public string Mode { get; set; }
        public Highcharts Chart { get; set; }
        public string BuildTime { get; set; }
        public List<string> DebugBuildTime { get; set; } 
        public Database.Models.Encounter Encounter { get; set; }
        public List<EncounterCharacterAbilityBreakdownDetail> AbilityBreakdown { get; set; }
        public string Type { get; set; }

        // Crit rate
        public int TotalCrits { get; set; }
        public int TotalHits { get; set; }
        public int TotalSwings
        {
            get { return TotalCrits + TotalHits; }
        }
        // Biggest / average hit
        public long TopBiggestHit { get; set; }
        public long TopAverageHit { get; set; }

        // Text
        public string TotalHeader { get; set; }
        public string TotalTooltip { get; set; }
        public string EffectiveHeader { get; set; }
        public string EffectiveTooltip { get; set; }
        public string TotalAverageHeader { get; set; }
        public string TotalAverageTooltip { get; set; }
        public string EffectiveAverageHeader { get; set; }
        public string EffectiveAverageTooltip { get; set; }
        public string PercentageOverallHeader { get; set; }
        public string PercentageOverallTooltip { get; set; }
        public string SwingsTooltip { get; set; }
    }
}