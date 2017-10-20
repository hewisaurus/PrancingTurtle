namespace Database.QueryModels
{
    public class EncounterDeathEvent
    {
        public int SecondsElapsed { get; set; }
        public int OrderWithinSecond { get; set; }
        public string EventType { get; set; }
        public string Source { get; set; }
        public string Target { get; set; }
        public string Ability { get; set; }
        public long Total { get; set; }
        public long Absorbed { get; set; }
        public long Overheal { get; set; }
        public long Intercepted { get; set; }
        public long Overkill { get; set; }

        public string RowClass
        {
            get
            {
                string returnValue = null;

                switch (EventType)
                {
                    case "Heal":
                        returnValue = "success";
                        break;
                    case "Damage":
                        returnValue = "danger";
                        break;
                    case "Absorb":
                        returnValue = "warning";
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
                        returnValue = "text-center text-success";
                        break;
                    case "Damage":
                        returnValue = "text-center text-danger";
                        break;
                    case "Absorb":
                        returnValue = "text-center text-info";
                        break;
                }

                return returnValue;
            }
        }
    }
}
