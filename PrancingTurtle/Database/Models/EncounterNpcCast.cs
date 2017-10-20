namespace Database.Models
{
    public class EncounterNpcCast
    {
        public int Id { get; set; }
        public int EncounterId { get; set; }
        public string AbilityName { get; set; }
        public string NpcId { get; set; }
        public string NpcName { get; set; }
    }
}
