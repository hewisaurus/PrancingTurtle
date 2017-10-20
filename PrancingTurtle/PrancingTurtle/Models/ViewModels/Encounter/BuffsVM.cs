using System;
using System.Collections.Generic;
using DotNet.Highcharts;

namespace PrancingTurtle.Models.ViewModels.Encounter
{
    public class BuffsVM
    {
        public Database.Models.Encounter Encounter { get; set; }
        public Highcharts BuffTimers { get; set; }

        public TimeSpan BuildTime { get; set; }
        public List<string> DebugBuildTime { get; set; } 
    }
}