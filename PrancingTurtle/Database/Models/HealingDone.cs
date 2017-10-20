namespace Database.Models
{
    public class HealingDone
    {
        public long Id { get; set; }
        public int? SourcePlayerId { get; set; }
        public string SourceNpcName { get; set; }
        public string SourceNpcId { get; set; }
        public string SourcePetName { get; set; }
        public int? TargetPlayerId { get; set; }
        public string TargetNpcName { get; set; }
        public string TargetNpcId { get; set; }
        public string TargetPetName { get; set; }
        public int EncounterId { get; set; }
        public int AbilityId { get; set; }
        public long TotalHealing { get; set; }
        public long EffectiveHealing { get; set; }
        public bool CriticalHit { get; set; }
        public int SecondsElapsed { get; set; }
        public int OrderWithinSecond { get; set; }
        public long OverhealAmount { get; set; }

        public Player SourcePlayer { get; set; }
        public Player TargetPlayer { get; set; }
        public Encounter Encounter { get; set; }
        public Ability Ability { get; set; }
    }
}
