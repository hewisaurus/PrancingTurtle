namespace Database.Models
{
    public class BossFightDifficulty
    {
        public int Id { get; set; }
        public int BossFightId { get; set; }
        public int EncounterDifficultyId { get; set;}
        public long OverrideHitpoints { get; set; }
        public string OverrideHitpointTarget { get; set; }

        public BossFight BossFight { get; set; }
        public EncounterDifficulty EncounterDifficulty { get; set; }
    }
}
