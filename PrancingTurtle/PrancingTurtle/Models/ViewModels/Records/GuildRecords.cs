using System.Collections.Generic;
using Database.Models;
using Database.QueryModels.Misc;
using DotNet.Highcharts;

namespace PrancingTurtle.Models.ViewModels.Records
{
    public class GuildRecords
    {
        public bool LoadImage { get; set; }
        public string GuildName { get; set; }
        public EncounterDifficulty Difficulty { get; set; }
        public Database.Models.BossFight BossFight { get; set; }
        public Database.Models.Encounter SingleFastestKill { get; set; }
        // TOP Player-based
        public RankPlayerGuild SingleTopDps { get; set; }
        public RankPlayerGuild SingleTopHps { get; set; }
        public RankPlayerGuild SingleTopAps { get; set; }
        // TOP Guild-based
        public RankPlayerGuild SingleTopDpsGuild { get; set; }
        public RankPlayerGuild SingleTopHpsGuild { get; set; }
        public RankPlayerGuild SingleTopApsGuild { get; set; }
        // This-guild based
        public Highcharts GuildHybridXpsOverTimeChart { get; set; }
        public Highcharts GuildDurationOverTimeChart { get; set; }
        public Highcharts GuildDpsOverTimeChart { get; set; }
        public Highcharts GuildHpsOverTimeChart { get; set; }
        public Highcharts GuildApsOverTimeChart { get; set; }

        public List<RankPlayerGuild> GuildTopDps { get; set; }
        public List<RankPlayerGuild> GuildTopHps { get; set; }
        public List<RankPlayerGuild> GuildTopAps { get; set; }

        public Highcharts PlayerDpsOverTimeChart { get; set; }
        public Highcharts PlayerHpsOverTimeChart { get; set; }
        public Highcharts PlayerApsOverTimeChart { get; set; }

        public GuildRecords()
        {
            BossFight = new Database.Models.BossFight();
            SingleFastestKill = new Database.Models.Encounter();
            SingleTopAps = new RankPlayerGuild();
            SingleTopDps = new RankPlayerGuild();
            SingleTopHps = new RankPlayerGuild();
            SingleTopApsGuild = new RankPlayerGuild();
            SingleTopDpsGuild = new RankPlayerGuild();
            SingleTopHpsGuild = new RankPlayerGuild();

            GuildTopDps = new List<RankPlayerGuild>();
            GuildTopHps = new List<RankPlayerGuild>();
            GuildTopAps = new List<RankPlayerGuild>();
        }
    }
}