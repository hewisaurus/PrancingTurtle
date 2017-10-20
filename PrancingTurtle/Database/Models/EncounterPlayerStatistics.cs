// ReSharper disable InconsistentNaming
namespace Database.Models
{
    public class EncounterPlayerStatistics
    {
        public int Id { get; set; }
        public int EncounterId { get; set; }
        public int PlayerId { get; set; }
        public long DPS { get; set; }
        public long HPS { get; set; }
        public long APS { get; set; }
        public int Deaths { get; set; }
        public int? TopDpsAbilityId { get; set; }
        public int? TopHpsAbilityId { get; set; }
        public int? TopApsAbilityId { get; set; }
        public long TopDpsAbilityValue { get; set; }
        public long TopHpsAbilityValue { get; set; }
        public long TopApsAbilityValue { get; set; }
        public long SingleTargetDps { get; set; }
        // Burst damage
        public long BurstDamage1sValue { get; set; }
        public int BurstDamage1sSecond { get; set; }
        public long BurstDamage5sValue { get; set; }
        public long BurstDamage5sPerSecond { get; set; }
        public int BurstDamage5sStart { get; set; }
        public int BurstDamage5sEnd { get; set; }
        public long BurstDamage15sValue { get; set; }
        public long BurstDamage15sPerSecond { get; set; }
        public int BurstDamage15sStart { get; set; }
        public int BurstDamage15sEnd { get; set; }
        // Burst healing
        public long BurstHealing1sValue { get; set; }
        public int BurstHealing1sSecond { get; set; }
        public long BurstHealing5sValue { get; set; }
        public long BurstHealing5sPerSecond { get; set; }
        public int BurstHealing5sStart { get; set; }
        public int BurstHealing5sEnd { get; set; }
        public long BurstHealing15sValue { get; set; }
        public long BurstHealing15sPerSecond { get; set; }
        public int BurstHealing15sStart { get; set; }
        public int BurstHealing15sEnd { get; set; }
        // Burst shielding
        public long BurstShielding1sValue { get; set; }
        public int BurstShielding1sSecond { get; set; }
        public long BurstShielding5sValue { get; set; }
        public long BurstShielding5sPerSecond { get; set; }
        public int BurstShielding5sStart { get; set; }
        public int BurstShielding5sEnd { get; set; }
        public long BurstShielding15sValue { get; set; }
        public long BurstShielding15sPerSecond { get; set; }
        public int BurstShielding15sStart { get; set; }
        public int BurstShielding15sEnd { get; set; }

        public Encounter Encounter { get; set; }
        public Player Player { get; set; }
    }
}
