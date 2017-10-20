
using Database.Models;

namespace PrancingTurtle.Models.Misc
{
    public class BossFightProgressionOLD
    {
        public BossFight BossFight { get; set; }
        public bool Killed { get; set; }

        public BossFightProgressionOLD()
        {
            BossFight = new BossFight();
            Killed = false;
        }

        public BossFightProgressionOLD(BossFight bossFight, bool killed)
        {
            BossFight = bossFight;
            Killed = killed;
        }
    }
}