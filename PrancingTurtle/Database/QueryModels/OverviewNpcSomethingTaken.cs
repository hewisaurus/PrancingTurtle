namespace Database.QueryModels
{
    public class OverviewNpcSomethingTaken
    {
        public string NpcId { get; set; }
        public string NpcName { get; set; }
        public long Total { get; set; }
        public string TotalDisplay { get { return GetDisplayForTotal(Total); } }
        public long TotalFromPlayers { get; set; }
        public string TotalFromPlayersDisplay { get { return GetDisplayForTotal(TotalFromPlayers); } }
        public long TotalFromOtherNpcs { get; set; }
        public string TotalFromOtherNpcsDisplay { get { return GetDisplayForTotal(TotalFromOtherNpcs); } }
        public long TotalFromSelf { get; set; }
        public string TotalFromSelfDisplay { get { return GetDisplayForTotal(TotalFromSelf); } }

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
