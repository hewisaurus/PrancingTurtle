using System.Collections.Generic;
using System.ComponentModel;

namespace PrancingTurtle.Models.ViewModels
{
    public class JoinGuildVM
    {
        public int AuthUserCharacterId { get; set; }
        [DisplayName("Guild")]
        public int GuildId { get; set; }
        public int ShardId { get; set; }
        public string Message { get; set; }

        public List<Database.Models.Guild> Guilds { get; set; }

        public JoinGuildVM()
        {
            
        }

        public JoinGuildVM(int characterId)
        {
            AuthUserCharacterId = characterId;
        }

        public JoinGuildVM(int characterId, int shardId)
        {
            AuthUserCharacterId = characterId;
            ShardId = shardId;
        }
    }
}