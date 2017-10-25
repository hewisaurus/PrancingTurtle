namespace Database.Models
{
    public class Ability
    {
        public int Id { get; set; }
        public int SoulId { get; set; }
        public long AbilityId { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public int RequiredLevel { get; set; }
        public int MinimumPointsInSoul { get; set; }
        public string AddonId { get; set; }
        public string Description { get; set; }
        public string DamageType { get; set; }
        public int RankNumber { get; set; }
        public string RequiresWeapon { get; set; }
        public decimal Cooldown { get; set; }
        public decimal CastTime { get; set; }
        public bool Channel { get; set; }
        public decimal Duration { get; set; }
        public decimal Interval { get; set; }
        public decimal MinRange { get; set; }
        public decimal MaxRange { get; set; }
        public decimal CostManaPercent { get; set; }
        public int CostEnergy { get; set; }
        public int MinimumHeat { get; set; }

        public string DisplayClass
        {
            get
            {
                return string.IsNullOrEmpty(DamageType) ? "damagetype-unknown" : string.Format("damagetype-{0}", DamageType.ToLower());
            }
        }

        public string FullDisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(Name) || AbilityId == 0)
                {
                    return null;
                }
                return string.Format("{0} (ID {1}) Rank {2}", Name, AbilityId, RankNumber);
            }
        }

        public Soul Soul { get; set; }
    }
}
