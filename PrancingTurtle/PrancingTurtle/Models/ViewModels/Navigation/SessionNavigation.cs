using System.Collections.Generic;
using Database.Models;

namespace PrancingTurtle.Models.ViewModels.Navigation
{
    public class SessionNavigation
    {
        public List<Database.Models.Guild> Guilds { get; set; }
        public List<Database.Models.BossFight> BossFights { get; set; }
        public List<BossFightDifficulty> BossFightDifficultyRecords { get; set; } // New method

        public SessionNavigation()
        {
            Guilds = new List<Database.Models.Guild>();
            BossFights = new List<Database.Models.BossFight>();
            BossFightDifficultyRecords = new List<BossFightDifficulty>();
        }
    }
}