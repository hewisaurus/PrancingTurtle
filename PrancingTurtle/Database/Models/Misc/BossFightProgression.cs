using System;

namespace Database.Models.Misc
{
    public class BossFightProgression
    {
        public int InstanceId { get; set; }
        public string InstanceName { get; set; }
        public int BossFightId { get; set; }
        public string BossFightName { get; set; }
        public int DifficultyId { get; set; }
        public string DifficultyName { get; set; }
        public string DifficultyShortName { get; set; }
        public bool Killed { get; set; }
        public TimeSpan? BestTime { get; set; }
        public int Rank { get; set; }
    }
}
