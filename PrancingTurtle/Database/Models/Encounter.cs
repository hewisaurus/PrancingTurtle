using System;

namespace Database.Models
{
    public class Encounter
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public DateTime DateUploaded { get; set; }
        public int BossFightId { get; set; }
        public bool SuccessfulKill { get; set; }
        public bool ValidForRanking { get; set; }
        public string Hash { get; set; }
        public TimeSpan Duration { get; set; }
        public BossFight BossFight { get; set; }
        public bool IsPublic { get; set; }
        public int? GuildId { get; set; }
        public int? UploaderId { get; set; }
        public int EncounterDifficultyId { get; set; }
        public bool ToBeDeleted { get; set; }
        public bool Removed { get; set; }

        public int KillTimeRank { get; set; }
        public int DPSRank { get; set; }

        public string OverallKillTimeRank
        {
            get
            {
                if (KillTimeRank > 0)
                {
                    return string.Format("#{0}", KillTimeRank);
                }
                return "-";
            }
        }
        public string OverallDPSRank
        {
            get
            {
                if (DPSRank > 0)
                {
                    return string.Format("#{0}", DPSRank);
                }
                return "-";
            }
        }

        public EncounterOverview Overview { get; set; }

        // Used for single target stats
        public string TargetName { get; set; }

        // UI Properties
        public long AverageDps { get; set; }
        public long AverageHps { get; set; }
        public long AverageAps { get; set; }

        public int PlayerDeaths { get; set; }
        public bool IsViewable { get; set; }
        public bool CanModifyPrivacy { get; set; }
        public AuthUserCharacter UploadCharacter { get; set; }
        public Guild UploadGuild { get; set; }
        public EncounterDifficulty EncounterDifficulty { get; set; }

        public string SuccessClass
        {
            get { return SuccessfulKill ? "success" : "danger"; }
        }
    }
}
