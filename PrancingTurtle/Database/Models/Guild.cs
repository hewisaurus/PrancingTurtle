using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Database.Models
{
    public class Guild
    {
        public int Id { get; set; }
        [Required]
        [DisplayName("Guild Name")]
        public string Name { get; set; }
        [DisplayName("Shard")]
        public int ShardId { get; set; }
        public int GuildStatusId { get; set; }
        public DateTime Created { get; set; }

        // Additional Privacy
        public bool HideFromLists { get; set; }
        public bool HideFromSearch { get; set; }
        public bool HideFromRankings { get; set; }
        public bool HideSessions { get; set; }
        public bool HideRoster { get; set; }
        public bool HideProgression { get; set; }

        public Shard Shard { get; set; }
        public GuildStatus Status { get; set; }

        // Lists for UI
        public List<Shard> Shards { get; set; }

        public Guild()
        {
            HideFromLists = false;
            HideFromRankings = false;
            HideFromSearch = false;
            HideSessions = false;
            HideRoster = true;
            HideProgression = false;
        }
    }
}
