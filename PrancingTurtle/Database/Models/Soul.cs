namespace Database.Models
{
    public class Soul
    {
        public int Id { get; set; }
        public int PlayerClassId { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Role { get; set; }

        public PlayerClass Class { get; set; }
    }
}
