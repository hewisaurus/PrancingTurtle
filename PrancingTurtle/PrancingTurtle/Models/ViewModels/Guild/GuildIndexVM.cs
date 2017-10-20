using System.Collections.Generic;
using Database.Models;
using Database.Models.Misc;

namespace PrancingTurtle.Models.ViewModels.Guild
{
    public class GuildIndexVM
    {
        public Database.Models.Guild Guild { get; set; }
        public bool IsMember { get; set; }
        public GuildRank GuildRank { get; set; }
        public int Members { get; set; }
        public bool CanLinkToSessions { get; set; }
        public int Sessions { get; set; }
        public int Encounters { get; set; }
        public List<AuthUserCharacter> MemberList { get; set; }
        public List<GuildRank> AvailableRanks { get; set; }
        public int CurrentUserId { get; set; }
        public List<AuthUserCharacterGuildApplication> Applications { get; set; }
        public bool CanBeApproved { get; set; }
        public bool CanBeRemoved { get; set; }
        //[Obsolete]
        //public List<BossFightProgressionOLD> BossFightProgressionOld { get; set; }
        public List<BossFightProgression> BossFightProgression { get; set; } 

        public GuildIndexVM()
        {
            CanLinkToSessions = false;
            CanBeApproved = false;
            CanBeRemoved = false;
            IsMember = false;
            Members = 0;
            MemberList = new List<AuthUserCharacter>();
            Applications = new List<AuthUserCharacterGuildApplication>();
            //BossFightProgressionOld = new List<BossFightProgressionOLD>();
            BossFightProgression = new List<BossFightProgression>();
        }
    }
}