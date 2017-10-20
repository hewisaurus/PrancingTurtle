namespace Database.QueryModels
{
    public class OverviewPlayerSomethingDone
    {
        public int PlayerId { get; set; }
        public string PlayerLogId { get; set; }
        public string PlayerName { get; set; }
        public string Class { get; set; }
        public long Total { get; set; }
        public string TotalDisplay { get { return GetDisplayForTotal(Total); } }
        public long TotalToNpcs { get; set; }
        public string TotalToNpcsDisplay { get { return GetDisplayForTotal(TotalToNpcs); } }
        public long TotalToOtherPlayers { get; set; }
        public string TotalToOtherPlayersDisplay { get { return GetDisplayForTotal(TotalToOtherPlayers); } }
        public long TotalToSelf { get; set; }
        public string TotalToSelfDisplay { get { return GetDisplayForTotal(TotalToSelf); } }
        
        // Calculated properties
        public long Average { get; set; }
        public decimal Percentage { get; set; }
        public string ProgressBarPercentage { get; set; }

        public string RoleIcon { get; set; }
        public string RoleName { get; set; }

        public string RowClass
        {
            get { return PlayerId == -1 ? "info" : ""; }
        }

        private string GetDisplayForTotal(long total)
        {
            if (total > 999999999)
            {
                return ((decimal)total / 1000000000).ToString("#.#") + "b";
            }
            if (total > 999999)
            {
                return ((decimal)total / 1000000).ToString("#.#") + "m";
            }
            if (total > 999)
            {
                return ((decimal)total / 1000).ToString("#.#") + "k";
            }
            return total.ToString();
        }

        public string DisplayColorClass
        {
            get
            {
                if (string.IsNullOrEmpty(Class))
                {
                    return "unknown";
                }

                return string.Format("classtype-{0}", Class.ToLower());
            }
        }
    }
}
