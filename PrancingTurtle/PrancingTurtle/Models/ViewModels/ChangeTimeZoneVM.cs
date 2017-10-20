using System;
using System.Collections.Generic;
using System.Linq;

namespace PrancingTurtle.Models.ViewModels
{
    public class ChangeTimeZoneVM
    {
        public string SelectedTimeZoneId { get; set; }

        public List<TimeZoneInfo> TimeZones
        {
            get { return TimeZoneInfo.GetSystemTimeZones().ToList(); }
        }

        public ChangeTimeZoneVM()
        {
            SelectedTimeZoneId = "UTC";
        }

        public ChangeTimeZoneVM(string timeZoneId)
        {
            SelectedTimeZoneId = timeZoneId;
        }
    }
}