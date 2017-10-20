using System.Collections.Generic;
using Database.QueryModels.Misc;

namespace PrancingTurtle.Models.ViewModels.BossFight
{
    public class RankPlayerGuildType
    {
        public List<RankPlayerGuild> Players { get; set; }
        public string DestinationAction { get; set; }
        public bool ShowShardName { get; set; }

        public RankPlayerGuildType()
        {
            Players = new List<RankPlayerGuild>();
            DestinationAction = "PlayerDamageDone";
            ShowShardName = true;
        }
    }
}