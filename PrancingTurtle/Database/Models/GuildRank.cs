namespace Database.Models
{
    public class GuildRank
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool CanUploadLogs { get; set; }
        public bool CanPromoteUsers { get; set; }
        public bool CanApproveUsers { get; set; }
        public bool CanModifyPrivacy { get; set; }
        public bool CanModifyAnySession { get; set; }
        public int RankPriority { get; set; }
        public bool DefaultWhenApproved { get; set; }
        public bool DefaultWhenCreated { get; set; }

        public GuildRank()
        {
            CanUploadLogs = false;
            CanPromoteUsers = false;
            CanApproveUsers = false;
            CanModifyAnySession = false;
            CanModifyPrivacy = false;
            RankPriority = 99;
            DefaultWhenApproved = false;
            DefaultWhenCreated = false;
        }
    }
}
