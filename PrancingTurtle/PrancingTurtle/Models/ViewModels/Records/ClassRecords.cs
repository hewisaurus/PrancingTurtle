using System.Collections.Generic;
using Database.Models;
using Database.QueryModels.Misc;
using DotNet.Highcharts;

namespace PrancingTurtle.Models.ViewModels.Records
{
    public class ClassRecords
    {
        public string ClassName { get; set; }
        public EncounterDifficulty Difficulty { get; set; }
        public Database.Models.BossFight BossFight { get; set; }
        public Database.Models.Encounter SingleFastestKill { get; set; }
        public RankPlayerGuild SingleTopDps { get; set; }
        public List<RankPlayerGuild> Top10Dps { get; set; }
        public RankPlayerGuild SingleTopHps { get; set; }
        public List<RankPlayerGuild> Top10Hps { get; set; }
        public RankPlayerGuild SingleTopAps { get; set; }
        public List<RankPlayerGuild> Top10Aps { get; set; }
        public bool LoadImage { get; set; }

        public Highcharts DpsChart { get; set; }
        public Highcharts HpsChart { get; set; }
        public Highcharts ApsChart { get; set; }

        public ClassRecords()
        {
            BossFight = new Database.Models.BossFight();
            SingleFastestKill = new Database.Models.Encounter();
            SingleTopAps = new RankPlayerGuild();
            SingleTopDps = new RankPlayerGuild();
            SingleTopHps = new RankPlayerGuild();
        }
    }
}

