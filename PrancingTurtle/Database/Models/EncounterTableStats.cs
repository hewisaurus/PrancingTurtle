namespace Database.Models
{
    public class EncounterTableStats
    {
        public int Id { get; set; }
        public int EncounterId { get; set; }
        public int DamageRecords { get; set; }
        public int HealingRecords { get; set; }
        public int ShieldingRecords { get; set; }
    }
}
