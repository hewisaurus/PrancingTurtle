using System.Collections.Generic;
using Database.Models;

namespace PrancingTurtle.Models.ViewModels.Navigation
{
    public class MainNavigation
    {
        public bool LoggedIn { get; set; }
        public bool ShortMenuFormat { get; set; }
        public bool ShowGuildMenu { get; set; }

        public List<Database.Models.Guild> Guilds { get; set; }
        public List<BossFightDifficulty> BossFightDifficultyRecords { get; set; }

        public MainNavigation()
        {
            Guilds = new List<Database.Models.Guild>();
            BossFightDifficultyRecords = new List<BossFightDifficulty>();
        }
    }
}