namespace Database.QueryModels.Misc
{
    public class EncounterInformation
    {
        public bool HasBuffEventRecords { get; set; }
        public bool HasBuffUptimeRecords { get; set; }
        public bool HasOverviewRecords { get; set; }
        public bool HasDamageRecords { get; set; }
        public bool HasHealingRecords { get; set; }
        public bool HasShieldingRecords { get; set; }
        public bool HasDeathRecords { get; set; }
        public bool HasDebuffActionRecords { get; set; }
        public bool HasBuffActionRecords { get; set; }
        public bool HasNpcCastRecords { get; set; }
    }
}
