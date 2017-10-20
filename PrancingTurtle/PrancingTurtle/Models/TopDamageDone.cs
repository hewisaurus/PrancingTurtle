namespace PrancingTurtle.Models
{
    public class TopDamageDone
    {
        public string AbilityName { get; set; }

        public string DisplayClass
        {
            get
            {
                return string.IsNullOrEmpty(DamageType) ? "damagetype-unknown" : string.Format("damagetype-{0}", DamageType.ToLower());
            }
        }
        public string AttackerName { get; set; }
        public string TargetName { get; set; }
        public long Value { get; set; }
        public string DamageType { get; set; }
        public string IconPath { get; set; }
    }
}