using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Database.Models
{
    public class AuthUserCharacter
    {
        public int Id { get; set; }
        public int AuthUserId { get; set; }
        [DisplayName("Shard")]
        public int ShardId { get; set; }
        [Required]
        [DisplayName("Character Name")]
        public string CharacterName { get; set; }
        public int GuildId { get; set; }
        public int? GuildRankId { get; set; }
        public bool Removed { get; set; }

        public string FullDisplayName
        {
            get {
                return string.Format("{0}@{1} <{2}>", CharacterName, Shard.Name, Guild.Name);
            }
        }

        public string DisplayName
        {
            get { return string.Format("{0}@{1}", CharacterName, Shard.Name); }
        }

        public string PendingApplicationGuildName { get; set; }

        public AuthUser AuthUser { get; set; }
        public Shard Shard { get; set; }
        public Guild Guild { get; set;}
        public GuildRank GuildRank { get; set; }

        // UI Properties
        public List<Shard> Shards { get; set; }
        /// <summary>
        /// This property is used to ensure that an admin can't accidentally demote themselves
        /// </summary>
        public bool CanBeModified { get; set; }

        public AuthUserCharacter()
        {
            GuildRank = new GuildRank();
            Guild = new Guild();
            CanBeModified = true;
        }
    }
}