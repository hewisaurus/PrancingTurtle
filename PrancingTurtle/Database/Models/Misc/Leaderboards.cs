using System.Collections.Generic;
using Database.QueryModels.Misc;

namespace Database.Models.Misc
{
    public class Leaderboards
    {
        public List<Encounter> FastestKills { get; set; }
        public Encounter SingleFastestKill { get; set; }

        public List<RankPlayerGuild> TopAps { get; set; }
        public List<RankPlayerGuild> TopDps { get; set; }
        public List<RankPlayerGuild> TopHps { get; set; }
        public RankPlayerGuild SingleTopAps { get; set; }
        public RankPlayerGuild SingleTopDps { get; set; }
        public RankPlayerGuild SingleTopHps { get; set; }

        public List<RankPlayerGuild> WarriorTopAps { get; set; }
        public List<RankPlayerGuild> WarriorTopDps { get; set; }
        public List<RankPlayerGuild> WarriorTopHps { get; set; }
        public List<RankPlayerGuild> MageTopAps { get; set; }
        public List<RankPlayerGuild> MageTopDps { get; set; }
        public List<RankPlayerGuild> MageTopHps { get; set; }
        public List<RankPlayerGuild> ClericTopAps { get; set; }
        public List<RankPlayerGuild> ClericTopDps { get; set; }
        public List<RankPlayerGuild> ClericTopHps { get; set; }
        public List<RankPlayerGuild> RogueTopAps { get; set; }
        public List<RankPlayerGuild> RogueTopDps { get; set; }
        public List<RankPlayerGuild> RogueTopHps { get; set; }
        public List<RankPlayerGuild> PrimalistTopAps { get; set; }
        public List<RankPlayerGuild> PrimalistTopDps { get; set; }
        public List<RankPlayerGuild> PrimalistTopHps { get; set; }

        public List<RankPlayerGuild> TopSingleTargetDps { get; set; }
        public List<RankPlayerGuild> WarriorTopSingleTarget { get; set; }
        public List<RankPlayerGuild> MageTopSingleTarget { get; set; }
        public List<RankPlayerGuild> ClericTopSingleTarget { get; set; }
        public List<RankPlayerGuild> RogueTopSingleTarget { get; set; }
        public List<RankPlayerGuild> PrimalistTopSingleTarget { get; set; }

        public List<RankPlayerGuild> TopSupportDps { get; set; }
        public List<RankPlayerGuild> WarriorTopSupportDps { get; set; }
        public List<RankPlayerGuild> MageTopSupportDps { get; set; }
        public List<RankPlayerGuild> ClericTopSupportDps { get; set; }
        public List<RankPlayerGuild> RogueTopSupportDps { get; set; }
        public List<RankPlayerGuild> PrimalistTopSupportDps { get; set; }

        public List<RankPlayerGuild> TopBurstDamage1S { get; set; }
        
        public Leaderboards()
        {
            FastestKills = new List<Encounter>();
            SingleFastestKill = new Encounter();

            TopAps = new List<RankPlayerGuild>();
            TopDps = new List<RankPlayerGuild>();
            TopHps = new List<RankPlayerGuild>();
            SingleTopAps = new RankPlayerGuild();
            SingleTopDps = new RankPlayerGuild();
            SingleTopHps = new RankPlayerGuild();

            WarriorTopAps = new List<RankPlayerGuild>();
            WarriorTopDps = new List<RankPlayerGuild>();
            WarriorTopHps = new List<RankPlayerGuild>();
            MageTopAps = new List<RankPlayerGuild>();
            MageTopDps = new List<RankPlayerGuild>();
            MageTopHps = new List<RankPlayerGuild>();
            ClericTopAps = new List<RankPlayerGuild>();
            ClericTopDps = new List<RankPlayerGuild>();
            ClericTopHps = new List<RankPlayerGuild>();
            RogueTopAps = new List<RankPlayerGuild>();
            RogueTopDps = new List<RankPlayerGuild>();
            RogueTopHps = new List<RankPlayerGuild>();
            PrimalistTopAps = new List<RankPlayerGuild>();
            PrimalistTopDps = new List<RankPlayerGuild>();
            PrimalistTopHps = new List<RankPlayerGuild>();

            TopSingleTargetDps = new List<RankPlayerGuild>();

            WarriorTopSingleTarget = new List<RankPlayerGuild>();
            MageTopSingleTarget = new List<RankPlayerGuild>();
            ClericTopSingleTarget = new List<RankPlayerGuild>();
            RogueTopSingleTarget = new List<RankPlayerGuild>();
            PrimalistTopSingleTarget = new List<RankPlayerGuild>();

            TopSupportDps = new List<RankPlayerGuild>();
            WarriorTopSupportDps = new List<RankPlayerGuild>();
            MageTopSupportDps = new List<RankPlayerGuild>();
            ClericTopSupportDps = new List<RankPlayerGuild>();
            RogueTopSupportDps = new List<RankPlayerGuild>();
            PrimalistTopSupportDps = new List<RankPlayerGuild>();

            TopBurstDamage1S = new List<RankPlayerGuild>();
        }
    }
}
