namespace Database.QueryModels
{
    public class EncounterCharacterAbilityBreakdownDetail
    {
        public string AbilityName { get; set; }
        public string SourceName { get; set; }
        public string TargetName { get; set; }
        public string DamageType { get; set; }
        public string Icon { get; set; }
        public int Crits { get; set; }
        public int Hits { get; set; }
        public long BiggestHit { get; set; }
        public long AverageHit { get; set; }
        public long Total { get; set; }
        public long Blocked { get; set; }
        public string BlockedPercentage
        {
            get
            {
                if (Blocked == 0 || Total == 0) return "0%";
                return ((decimal)Blocked / Total).ToString("#.##%");
            }
        }
        public long Intercepted { get; set; }
        public string InterceptedPercentage
        {
            get
            {
                if (Intercepted == 0 || Total == 0) return "0%";
                return ((decimal)Intercepted / Total).ToString("#.##%");
            }
        }
        public long Ignored { get; set; }
        public string IgnoredPercentage
        {
            get
            {
                if (Ignored == 0 || Total == 0) return "0%";
                return ((decimal)Ignored / Total).ToString("#.##%");
            }
        }
        public long Absorbed { get; set; }
        public string AbsorbedPercentage
        {
            get
            {
                if (Absorbed == 0 || Total == 0) return "0%";
                return ((decimal) Absorbed/Total).ToString("#.##%");
            }
        }
        public long Effective { get; set; }
        public string EffectivePercentage
        {
            get
            {
                if (Effective == 0 || Total == 0) return "0%";
                return ((decimal)Effective / Total).ToString("#.##%");
            }
        }
        public long Average { get; set; }
        public long AverageEffective { get; set; }
        public decimal Percentage { get; set; }
        public long Overhealing { get; set; }
        public string OverhealingPercentage
        {
            get
            {
                if (Overhealing == 0 || Total == 0) return "0%";
                return ((decimal)Overhealing / Total).ToString("#.##%");
            }
        }
        public long Overkilled { get; set; }
        public int Swings
        {
            get { return Crits + Hits; }
        }
        
        public string RowClass
        {
            get
            {
                {
                    if (string.IsNullOrEmpty(DamageType)) return "";
                    return DamageType == "NA" ? "success" : "";
                }
            }
        }

        public string Displayclass
        {
            get
            {
                return string.IsNullOrEmpty(DamageType) ? "damagetype-unknown" : string.Format("damagetype-{0}", DamageType.ToLower());
            }
        }
        public string CellClass
        {
            get
            {
                return string.IsNullOrEmpty(DamageType) ? "damagetype-unknown-cell" : string.Format("damagetype-{0}-cell", DamageType.ToLower());
            }
        }
        public string ProgressBarPercentage { get; set; }
        public bool TopRecord { get; set; }
    }
}
