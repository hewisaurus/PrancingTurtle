using System.Collections.Generic;

namespace Database.QueryModels
{
    public class EncounterFullOverview
    {
        // X Done
        public List<OverviewPlayerSomethingDone> PlayerDamageDone { get; set; }
        public List<OverviewPlayerSomethingDone> PlayerHealingDone { get; set; }
        public List<OverviewPlayerSomethingDone> PlayerShieldingDone { get; set; }
        public List<OverviewNpcSomethingDone> NpcDamageDone { get; set; }
        public List<OverviewNpcSomethingDone> NpcHealingDone { get; set; }
        public List<OverviewNpcSomethingDone> NpcShieldingDone { get; set; }
        // X Taken
        public List<OverviewPlayerSomethingDone> PlayerDamageTaken { get; set; }
        public List<OverviewPlayerSomethingDone> PlayerHealingTaken { get; set; }
        public List<OverviewPlayerSomethingDone> PlayerShieldingTaken { get; set; }
        public List<OverviewNpcSomethingDone> NpcDamageTaken { get; set; }
        public List<OverviewNpcSomethingDone> NpcHealingTaken { get; set; }
        public List<OverviewNpcSomethingDone> NpcShieldingTaken { get; set; }
    }
}
