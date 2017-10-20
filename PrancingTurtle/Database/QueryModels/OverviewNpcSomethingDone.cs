namespace Database.QueryModels
{
    public class OverviewNpcSomethingDone
    {
        public string NpcId { get; set; }
        public string NpcName { get; set; }
        public long Total { get; set; }
        public string TotalDisplay { get { return GetDisplayForTotal(Total); } }
        public long TotalToPlayers { get; set; }
        public string TotalToPlayersDisplay { get { return GetDisplayForTotal(TotalToPlayers); } }
        public long TotalToOtherNpcs { get; set; }
        public string TotalToOtherNpcsDisplay { get { return GetDisplayForTotal(TotalToOtherNpcs); } }
        public long TotalToSelf { get; set; }
        public string TotalToSelfDisplay { get { return GetDisplayForTotal(TotalToSelf); } }

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

        // Calculated properties
        public long Average { get; set; }
        public decimal Percentage { get; set; }
        public string ProgressBarPercentage { get; set; }

        public string RowClass
        {
            get { return string.IsNullOrEmpty(NpcId) ? "info" : ""; }
        }
    }
}
