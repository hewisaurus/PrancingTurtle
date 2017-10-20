using System;
using System.Collections.Generic;
using Database.Models;
using DotNet.Highcharts;

namespace PrancingTurtle.Models.ViewModels
{
    public class EncounterOverviewViewModel
    {
        public Highcharts RaidDps { get; set; }
        public Highcharts RaidHps { get; set; }
        public Highcharts RaidAps { get; set; }
        public Highcharts NpcDps { get; set; }
        public Highcharts NpcHps { get; set; }
        public Highcharts NpcAps { get; set; }
        public Highcharts PlayerDeaths { get; set; }

        //public long AverageDps { get; set; }
        //public long AverageHps { get; set; }
        //public long AverageAps { get; set; }

        public int EncounterId { get; set; }
        public string EncounterName { get; set; }
        public string InstanceName { get; set; }
        public bool EncounterSuccess { get; set; }
        public TimeSpan EncounterLength { get; set; }
        public DateTime EncounterStart { get; set; }

        public List<Player> Players { get; set; }
        public List<EncounterDeath> DeathRecords { get; set; }

        public TimeSpan BuildTime { get; set; }

        public EncounterOverviewViewModel()
        {
            Players = new List<Player>();
            DeathRecords = new List<EncounterDeath>();
        }
    }
}