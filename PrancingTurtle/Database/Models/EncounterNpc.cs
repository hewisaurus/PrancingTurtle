namespace Database.Models
{
    public class EncounterNpc
    {
        public long Id { get; set; }
        public int EncounterId { get; set; }
        public string NpcName { get; set; }
        public string NpcId { get; set; }

        public Encounter Encounter { get; set; }
    }
}
