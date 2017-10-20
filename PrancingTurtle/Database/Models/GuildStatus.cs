namespace Database.Models
{
    public class GuildStatus
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Active { get; set; }
        public bool Approved { get; set; }
        public bool DefaultStatus { get; set; }
        public bool PlayersCanApply { get; set; }
        public bool HideFromSearch { get; set; }
        public bool HideFromRankings { get; set; }
        public bool HideFromLists { get; set; }
    }
}