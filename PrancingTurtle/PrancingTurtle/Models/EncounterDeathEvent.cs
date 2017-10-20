namespace PrancingTurtle.Models
{
    public class EncounterDeathEvent
    {
        public string EventType { get; set; }
        public string Source { get; set; }
        public string Target { get; set; }
        public string Ability { get; set; }
        public long Total { get; set; }
        public long Overheal { get; set; }
        public long Overkill { get; set; }
        public int SecondsElapsed { get; set; }
        public int OrderWithinSecond { get; set; }
        public string RowClass
        {
            get
            {
                string returnValue = null;

                switch (EventType)
                {
                    case "Heal":
                        break;
                    case "Damage":
                        returnValue = "danger";
                        break;
                    case "Shield":
                        break;
                }

                return returnValue;
            }
        }

        public string TextClass
        {
            get
            {
                string returnValue = null;

                switch (EventType)
                {
                    case "Heal":
                        returnValue = "text-info";
                        break;
                    case "Damage":
                        returnValue = "text-warning";
                        break;
                    case "Shield":
                        returnValue = "text-success";
                        break;
                }

                return returnValue;
            }
        }
    }
}