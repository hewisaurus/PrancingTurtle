using System;
using System.Collections.Generic;
using System.Linq;
using Database.Models;
using DotNet.Highcharts;

namespace PrancingTurtle.Models.ViewModels
{
    public class EncounterSupportVm
    {
        public Highcharts PlayerDeaths { get; set; }
        public Highcharts BuffEvents { get; set; }
        public Highcharts BuffEventBars { get; set; }

        public int EncounterId { get; set; }
        public string EncounterName { get; set; }
        public string InstanceName { get; set; }
        public TimeSpan EncounterLength { get; set; }
        public DateTime EncounterStart { get; set; }
        public bool EncounterSuccess { get; set; }

        public List<EncounterBuffUptime> BuffUptimes { get; set; }

        public List<BuffGroup> BuffGroups
        {
            get
            {
                var groups = new List<BuffGroup>();

                if (BuffUptimes != null)
                {
                    foreach (var uptime in BuffUptimes)
                    {
                        if (!groups.Any(g => g.Name == uptime.Buff.BuffGroup.Name))
                        {
                            groups.Add(uptime.Buff.BuffGroup);
                        }
                    }
                }

                return groups;
            }
        }

        public TimeSpan BuildTime { get; set; }
    }
}