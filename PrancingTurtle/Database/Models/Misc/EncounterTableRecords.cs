namespace Database.Models.Misc
{
    public class EncounterTableRecords
    {
        public int Damage { get; set; }
        public int Healing { get; set; }
        public int Shielding { get; set; }
        public int Overview { get; set; }
        public int BuffEvent { get; set; }
        public int BuffUptime { get; set; }
        public int BuffAction { get; set; }
        public int DebuffAction { get; set; }
        public int NpcCast { get; set; }
        public int Death { get; set; }
        public int PlayerRole { get; set; }
        public int Npc { get; set; }
        public int PlayerStatistics { get; set; }

        public bool HasRecords =>
            Damage > 0 || Healing > 0 || Shielding > 0 || Overview > 0 ||
            BuffEvent > 0 || BuffUptime > 0 || BuffAction > 0 ||
            DebuffAction > 0 || NpcCast > 0 || Death > 0 || PlayerRole > 0 ||
            Npc > 0 || PlayerStatistics > 0;
    }
}
