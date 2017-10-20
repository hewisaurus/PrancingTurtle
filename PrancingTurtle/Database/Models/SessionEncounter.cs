namespace Database.Models
{
    public class SessionEncounter
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public int EncounterId { get; set; }
    }
}
