using System;
using System.Collections.Generic;
using System.Linq;
using Database.Models;

namespace PrancingTurtle.Models.ViewModels.Session
{
    public class SessionDetailVM
    {
        public bool DebugMode { get; set; }
        public Database.Models.Session Session { get; set; }
        public List<Database.Models.Encounter> Encounters { get; set; }
        public string TimeZoneId { get; set; }
        public bool CanBeRemoved { get; set; }
        public bool CanBeReimported { get; set; }
        public bool CanBeRenamed { get; set; }
        public int OriginalUploaderGuildId { get; set; }
        public SessionLog FirstLog { get; set; }
        public TimeSpan ActiveTime
        {
            get
            {
                if (Encounters == null || !Encounters.Any()) return new TimeSpan();

                TimeSpan activeTimeSpan = new TimeSpan();

                Encounters.ForEach(e => activeTimeSpan = activeTimeSpan.Add(e.Duration));
                
                return activeTimeSpan;
            }
        }
        public string ActiveTimeReadable
        {
            get
            {
                return ActiveTime.ToString();
            }
        }
        public string ActiveTimePercentage
        {
            get
            {
                if (Session == null) return "0%";
                if (Session.Duration.Ticks == 0 || ActiveTime.Ticks == 0) return "0%";

                return ((decimal)ActiveTime.Ticks / Session.Duration.Ticks).ToString("#.##%");
            }
        }

        //public string ActivePlayerTime
        //{
        //    get
        //    {
        //        if (Session == null) return "";
        //        if (Session.TotalPlayTime == 0) return "";
        //        return new TimeSpan(Session.TotalPlayTime).ToString();
        //    }
        //}

        public List<string> Uploaders { get; set; } 

        public string UploadFilename
        {
            get
            {
                if (Session == null) return "";

                return string.Format("{0}.zip", Session.UploadToken);
            }
        }

        public TimeSpan TimeZoneOffset
        {
            get { return TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId).GetUtcOffset(DateTime.UtcNow); }
        }

        public SessionDetailVM()
        {
            Encounters = new List<Database.Models.Encounter>();
            Session = new Database.Models.Session();
            // Default the timezone to UTC
            TimeZoneId = "UTC";
            CanBeRemoved = false;
            CanBeReimported = false;
            DebugMode = false;
            Uploaders = new List<string>();
        }
    }
}