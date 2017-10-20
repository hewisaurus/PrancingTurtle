using System.Collections.Generic;
using System.Linq;
using Database.QueryModels;

namespace PrancingTurtle.Models.ViewModels.Encounter
{
    public class PlayerSomethingDonePartialVM
    {
        public List<OverviewPlayerSomethingDone> Data { get; set; }
        public bool IsOverview { get; set; }
        public bool IsOutgoing { get; set; }
        public Database.Models.Encounter Encounter { get; set; }

        public string GraphType { get; set; }
        public string TotalText { get; set; }
        public string AverageText { get; set; }
        public bool ShowBuffs { get; set; }
        public bool ShowDebuffs { get; set; }

        public void CalculateAverageAndPercentage()
        {
            if (Data.Any())
            {
                long total = Data.Where(p => p.PlayerId != -1).Sum(d => d.Total);

                for (int i = 0; i < Data.Count; i++)
                {
                    var overview = Data[i];
                    overview.Average = long.Parse(((decimal) overview.Total/(decimal)Encounter.Duration.TotalSeconds).ToString("#"));
                    overview.Percentage = overview.Total / (decimal)total * 100;
                }
            }
        }

        public PlayerSomethingDonePartialVM()
        {
            
        }

        public PlayerSomethingDonePartialVM(List<OverviewPlayerSomethingDone> data, Database.Models.Encounter encounter, bool isOverview, string averageText,
            string graphType, string totalText, bool showBuffs, bool showDebuffs, bool isOutgoing)
        {
            Data = data;
            Encounter = encounter;
            IsOverview = isOverview;
            AverageText = averageText;
            GraphType = graphType;
            TotalText = averageText;
            ShowBuffs = showBuffs;
            ShowDebuffs = showDebuffs;
            IsOutgoing = isOutgoing;

            CalculateAverageAndPercentage();
        }
    }
}