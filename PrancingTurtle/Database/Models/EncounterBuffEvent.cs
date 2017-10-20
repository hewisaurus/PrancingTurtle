namespace Database.Models
{
    public class EncounterBuffEvent
    {
        public int Id { get; set; }
        public int EncounterId { get; set; }
        public int BuffId { get; set; }
        public int PlayerId { get; set; }
        public int SecondBuffWentUp { get; set; }
        public int SecondBuffWentDown { get; set; }

        public string DisplayName
        {
            get
            {
                if (Player == null || Buff == null)
                {
                    return null;
                }
                return string.Format("{0} ({1})", Buff.Name, Player.Name);
            }
        }

        public Encounter Encounter { get; set; }
        public Buff Buff { get; set; }
        public Player Player { get; set; }
    }
}
