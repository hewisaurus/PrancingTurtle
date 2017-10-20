using Database.Models;

namespace Database.QueryModels
{
    public class CharacterGuild
    {
        public int Id { get; set; }
        public string CharacterName { get; set; }
        public int ShardId { get; set; }
        public string ShardName { get; set; }
        public int? GuildId { get; set; }
        public string GuildName { get; set; }
        public string RankName { get; set; }
        public string PendingApplicationGuildName { get; set; }

        public GuildRank GuildRank { get; set; }
    }
}
