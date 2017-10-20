using Database.Models;

namespace PrancingTurtle.Models
{
    public class PlayerComparison
    {
        public Player Player { get; set; }
        public AbilityBreakdown DamageBreakdown { get; set; }
        public AbilityBreakdown HealingBreakdown { get; set; }

        public PlayerComparison()
        {
            Player = new Player();
            DamageBreakdown = new AbilityBreakdown();
            HealingBreakdown = new AbilityBreakdown();
        }
    }
}