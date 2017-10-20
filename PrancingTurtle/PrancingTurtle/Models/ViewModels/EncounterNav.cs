using System;

namespace PrancingTurtle.Models.ViewModels
{
    public class EncounterNav
    {
        public int Id { get; set; }
        public string EncounterName { get; set; }
        public string InstanceName { get; set; }
        public TimeSpan EncounterLength { get; set; }
        public Database.Models.Session Session { get; set; }
        public bool Success { get; set; }

        public string NavClass
        {
            get { return Success ? "navbark-flat navbar-kill" : "navbarw-flat navbar-wipe"; }
        }

        public EncounterNav()
        {
            Session = null;
        }

        public EncounterNav(int id, string encName, string instName, TimeSpan length, bool success)
        {
            Id = id;
            EncounterName = encName;
            InstanceName = instName;
            EncounterLength = length;
            Success = success;
            Session = null;
        }

        public EncounterNav(int id, string encName, string instName, TimeSpan length, bool success, Database.Models.Session session)
        {
            Id = id;
            EncounterName = encName;
            InstanceName = instName;
            EncounterLength = length;
            Success = success;
            Session = session;
        }
    }
}