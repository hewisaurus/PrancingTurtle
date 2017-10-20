using System;
using System.Collections.Generic;
using Database.Models;
using DotNet.Highcharts;

namespace PrancingTurtle.Models.ViewModels
{
    public class EncounterDetailPlayersVm
    {
        public Highcharts RaidDps { get; set; }
        public Highcharts RaidHps { get; set; }
        public Highcharts RaidAps { get; set; }
        public Highcharts PlayerDps { get; set; }
        public Highcharts PlayerHps { get; set; }
        public Highcharts PlayerAps { get; set; }
        public Highcharts PlayerDeaths { get; set; }

        public int EncounterId { get; set; }
        public string EncounterName { get; set; }
        public string InstanceName { get; set; }
        public TimeSpan EncounterLength { get; set; }
        public DateTime EncounterStart { get; set; }
        public bool EncounterSuccess { get; set; }

        public List<TopDamageDone> TopPlayerDamageDoneRecords { get; set; }
        public List<EncounterDeath> DeathRecords { get; set; }
        public AbilityBreakdown PlayerAbilityBreakdown { get; set; }

        public Dictionary<Player, long> AverageDps { get; set; }
        public Dictionary<Player, long> AverageHps { get; set; }
        public Dictionary<Player, long> AverageAps { get; set; }

        public List<Player> Players { get; set; }

        public EncounterDetailPlayersVm()
        {
            AverageDps = new Dictionary<Player, long>();
            AverageHps = new Dictionary<Player, long>();
            AverageAps = new Dictionary<Player, long>();
            Players = new List<Player>();
            DeathRecords = new List<EncounterDeath>();

            DeathRecords = new List<EncounterDeath>();
            TopPlayerDamageDoneRecords = new List<TopDamageDone>();
        }

    }
}