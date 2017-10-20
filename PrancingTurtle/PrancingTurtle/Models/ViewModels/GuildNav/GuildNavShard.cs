using System.Collections.Generic;

namespace PrancingTurtle.Models.ViewModels.GuildNav
{
    public class GuildNavShard
    {
        public string Name { get; set; }
        public List<GuildNavGuild> Guilds { get; set; }

        public GuildNavShard()
        {
            Guilds = new List<GuildNavGuild>();
        }
    }
}