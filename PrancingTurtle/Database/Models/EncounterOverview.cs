namespace Database.Models
{
    public class EncounterOverview
    {
        public long Id { get; set; }

        public int EncounterId { get; set; }
        public long AverageDps { get; set; }
        public int PlayerDeaths { get; set; }
        public long AverageHps { get; set; }
        public long AverageAps { get; set; }
    }
}
