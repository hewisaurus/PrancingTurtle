using System.Collections.Generic;
using System.Linq;
using Database.QueryModels;
using DotNet.Highcharts;

namespace PrancingTurtle.Models.ViewModels.Encounter
{
    public class InteractionVm
    {
        public string PageTitle { get; set; }
        public Highcharts Chart { get; set; }
        public Highcharts SplineChart { get; set; }
        public string BuildTime { get; set; }
        public string Mode { get; set; }
        public List<string> DebugBuildTime { get; set; }
        public Database.Models.Encounter Encounter { get; set; }
        public List<EncounterCharacterAbilityBreakdownDetail> Breakdown { get; set; }
        public string Type { get; set; }
        public bool Outgoing { get; set; }
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
        //Visibility Checks
        public bool ShowAbsorbed { get { return Breakdown.Any(b => b.Absorbed > 0); } }
        public bool ShowBlocked { get { return Breakdown.Any(b => b.Blocked > 0); } }
        public bool ShowIgnored { get { return Breakdown.Any(b => b.Ignored > 0); } }
        public bool ShowIntercepted { get { return Breakdown.Any(b => b.Intercepted > 0); } }
        public bool ShowOverkilled { get { return Breakdown.Any(b => b.Overkilled > 0); } }

        public void SetText()
        {
            if (string.IsNullOrEmpty(Type)) Type = "dps";

            switch (Type)
            {
                case "hps":
                    TotalHeader = "Total Healing";
                    TotalTooltip = "This total includes overhealing";
                    EffectiveHeader = "Effective Healing";
                    EffectiveTooltip = "This total only includes actual healing, and ignores overhealing";
                    TotalAverageHeader = "tHPS";
                    TotalAverageTooltip = "Total healing per second, including overhealing";
                    EffectiveAverageHeader = "eHPS";
                    EffectiveAverageTooltip = "Effective healing second, ignoring overhealing";
                    PercentageOverallHeader = "% HPS";
                    PercentageOverallTooltip = "Percentage of the total effective healing per second";
                    SwingsTooltip = "The number of times this ability hit the target";
                    break;
                case "aps":
                    TotalHeader = "Total absorption";
                    TotalTooltip = "Total absorption (shields) given";
                    TotalAverageHeader = "APS";
                    TotalAverageTooltip = "Absorption per second";
                    PercentageOverallHeader = "% APS";
                    PercentageOverallTooltip = "Percentage of the total absorption per second";
                    SwingsTooltip = "The number of times this ability hit the target";
                    break;
                default:
                    TotalHeader = "Total Damage";
                    TotalTooltip = "This total includes damage absorbed, blocked, ignored and intercepted";
                    EffectiveHeader = "Effective Damage";
                    EffectiveTooltip = "This total only includes actual damage, and ignores absorbed, blocked and intercepted damage";
                    TotalAverageHeader = "tDPS";
                    TotalAverageTooltip = "Total damage per second, including absorbed, blocked, ignored and intercepted damage";
                    EffectiveAverageHeader = "eDPS";
                    EffectiveAverageTooltip = "Effective damage per second, ignoring absorbed, blocked and intercepted damage";
                    PercentageOverallHeader = "% DPS";
                    PercentageOverallTooltip = "Percentage of the total damage per second";
                    SwingsTooltip = "The number of times this ability hit the target";
                    break;
            }
        }
    }
}