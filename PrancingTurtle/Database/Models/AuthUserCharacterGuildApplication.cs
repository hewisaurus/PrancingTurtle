namespace Database.Models
{
    public class AuthUserCharacterGuildApplication
    {
        public int Id { get; set; }
        public int AuthUserCharacterId { get; set; }
        public int GuildId { get; set; }
        public string Message { get; set; }

        public Guild Guild { get; set; }
        public AuthUserCharacter Character { get; set; }
    }
}
