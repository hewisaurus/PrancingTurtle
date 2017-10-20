using System;

namespace Database.Models
{
    public class EncounterBuffUptime
    {
        public int Id { get; set; }
        public int EncounterId { get; set; }
        public int BuffId { get; set; }
        public Decimal Uptime { get; set; }

        public Encounter Encounter { get; set; }
        public Buff Buff { get; set; }
    }
}
