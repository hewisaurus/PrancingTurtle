namespace Database.QueryModels
{
    public class CharacterDebuffAction
    {
        public string DebuffName { get; set; }
        public string SourceName { get; set; }
        public int SecondDebuffWentUp { get; set; }
        public int SecondDebuffWentDown { get; set; }
        public string Icon { get; set; }
        public string Role { get; set; }
    }
}
