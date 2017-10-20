namespace PrancingTurtle.Models
{
    public class AbilityStatistics
    {
        public long MinHit { get; set; }
        public long MinCrit { get; set; }
        public long MaxHit { get; set; }
        public long MaxCrit { get; set; }
        public long AverageHit { get; set; }
        public long AverageCrit { get; set; }
        public decimal CritRate { get; set; }
        public int Swings { get; set; }
        public int Hits { get; set; }
        public int Crits { get; set; }

        public AbilityStatistics()
        {
            
        }

        /// <summary>
        /// Makes a new object by copying values from an existing one
        /// </summary>
        /// <param name="abilityStatistics"></param>
        public AbilityStatistics(AbilityStatistics abilityStatistics)
        {
            MinHit = abilityStatistics.MinHit;
            MinCrit = abilityStatistics.MinCrit;
            MaxHit = abilityStatistics.MaxHit;
            MaxCrit = abilityStatistics.MaxCrit;
            AverageCrit = abilityStatistics.AverageCrit;
            AverageHit = abilityStatistics.AverageHit;
            CritRate = abilityStatistics.CritRate;
            Swings = abilityStatistics.Swings;
            Hits = abilityStatistics.Hits;
            Crits = abilityStatistics.Crits;
        }
    }
}