using System;
using System.Collections.Generic;
using Database.QueryModels;
using DotNet.Highcharts;

namespace PrancingTurtle.Models.ViewModels.Encounter
{
    public class NpcSomethingTaken
    {
        public Highcharts Graph { get; set; }
        public Highcharts SplineGraph { get; set; }
        public List<OverviewNpcSomethingTaken> Data { get; set; }
        public Database.Models.Encounter Encounter { get; set; }
        public Database.Models.Session Session { get; set; }
        public TimeSpan BuildTime { get; set; }
        public List<string> DebugBuildTime { get; set; }
        public string TimeZoneId { get; set; }
        public string ExportText { get; set; }

        public TimeSpan TimeZoneOffset
        {
            get { return TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId).GetUtcOffset(DateTime.UtcNow); }
        }

        public string PageTitle { get; set; }
        public string TotalText { get; set; }
        public string AverageText { get; set; }
        public string GraphType { get; set; }
        public bool IsOutgoing { get; set; }

        public string TotalNpcText
        {
            get { return IsOutgoing ? "Total to NPCs" : "Total from NPCs"; }
        }

        public string TotalPlayersText
        {
            get { return IsOutgoing ? "Total to players" : "Total from players"; }
        }

        public string TotalSelfText
        {
            get { return IsOutgoing ? "Total to self" : "Total from self"; }
        }

        public NpcSomethingTaken()
        {
            // Default the timezone to UTC
            TimeZoneId = "UTC";
        }
    }
}