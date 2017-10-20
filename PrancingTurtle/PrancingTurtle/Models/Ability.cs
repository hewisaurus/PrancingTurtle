using DotNet.Highcharts;

namespace PrancingTurtle.Models
{
    public class Ability
    {
        /// <summary>
        /// Source of the ability
        /// Generally null unless referring to NPCs
        /// </summary>
        public string Source { get; set; }
        public long AbilityId { get; set; }
        public string Name { get; set; }
        public string DamageType { get; set; }
        public bool IsPetAbility { get; set; }
        public string DisplayClass
        {
            get
            {
                return string.IsNullOrEmpty(DamageType) ? "damagetype-unknown" : string.Format("damagetype-{0}", DamageType.ToLower());
            }
        }
        public string CellClass
        {
            get
            {
                return string.IsNullOrEmpty(DamageType) ? "damagetype-unknown-cell" : string.Format("damagetype-{0}-cell", DamageType.ToLower());
            }
        }
        public string IconPath { get; set; }
        public long HighestTotalDamage { get; set; }
        public long TotalDamage { get; set; }
        public long HighestTotalHealing { get; set; }
        public long TotalHealing { get; set; }
        public long TotalEffectiveHealing { get; set; }
        public long DamagePerSecond { get; set; }
        public long HealingPerSecond { get; set; }
        public long EffectiveHealingPerSecond { get; set; }
        public decimal BreakdownPercentage { get; set; }
        public AbilityStatistics Statistics { get; set; }

        public Highcharts ComparisonChart { get; set; }

        public Ability()
        {
            Statistics = new AbilityStatistics();
        }

        /// <summary>
        /// Makes a new Ability by coping values from another
        /// </summary>
        /// <param name="ability"></param>
        public Ability(Ability ability)
        {
            Name = ability.Name;
            AbilityId = ability.AbilityId;
            Source = ability.Source;
            DamageType = ability.DamageType;
            IsPetAbility = ability.IsPetAbility;
            IconPath = ability.IconPath;
            TotalDamage = ability.TotalDamage;
            TotalHealing = ability.TotalHealing;
            TotalEffectiveHealing = ability.TotalEffectiveHealing;
            DamagePerSecond = ability.DamagePerSecond;
            HealingPerSecond = ability.HealingPerSecond;
            EffectiveHealingPerSecond = ability.EffectiveHealingPerSecond;
            BreakdownPercentage = ability.BreakdownPercentage;
            Statistics = new AbilityStatistics(ability.Statistics);
        }

    }
}