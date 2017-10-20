using System.Collections.Generic;
using System.Linq;

namespace PrancingTurtle.Models
{
    public class AbilityBreakdown
    {
        public List<Ability> Abilities { get; set; }

        public long TotalHits
        {
            get
            {
                if (Abilities.Any())
                {
                    return Abilities.Sum(a => a.Statistics.Hits);
                }
                return 0;
            }
        }

        public long TotalCrits
        {
            get
            {
                if (Abilities.Any())
                {
                    return Abilities.Sum(a => a.Statistics.Crits);
                }
                return 0;
            }
        }

        public decimal TotalCritRate
        {
            get
            {
                if (TotalHits > 0 && TotalCrits > 0)
                {
                    return (decimal)TotalCrits / ((decimal)TotalHits + (decimal)TotalCrits) * 100;
                }
                return 0;
            }
        }

        public AbilityBreakdown()
        {
            Abilities = new List<Ability>();
        }
    }
}