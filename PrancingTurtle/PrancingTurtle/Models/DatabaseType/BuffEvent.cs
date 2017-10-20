using System.Collections.Generic;
using Database.Models;

namespace PrancingTurtle.Models.DatabaseType
{
    public class BuffEvent
    {
        public Player Player { get; set; }
        public Buff Buff { get; set; }
        public List<int> SecondsUp { get; set; }

        public string DisplayName
        {
            get
            {
                if (Player == null || Buff == null)
                {
                    return null;
                }
                return string.Format("{0} ({1})", Buff.Name, Player.Name);
            }
        }

        public BuffEvent()
        {
            SecondsUp = new List<int>();
        }
    }
}