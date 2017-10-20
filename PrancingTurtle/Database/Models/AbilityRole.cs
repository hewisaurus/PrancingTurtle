namespace Database.Models
{
    public class AbilityRole
    {
        public int Id { get; set; }
        public long AbilityLogId { get; set; }
        public string AbilityName { get; set; }
        public string Soul { get; set; }
        public int RoleIconId { get; set; }
        public int PlayerClassId { get; set; }

        public PlayerClass Class { get; set; }
        public RoleIcon Role { get; set; }
    }
}
