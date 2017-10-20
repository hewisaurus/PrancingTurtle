using System.Collections.Generic;

namespace Database.Models
{
    public class Player
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Shard { get; set; }

        public string DisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(Shard))
                {
                    return null;
                }
                return Name.Contains("@") ? Name : string.Format("{0}@{1}", Name, Shard);
            }
        }
        public string PlayerId { get; set; }
        public List<Soul> Souls { get; set; }

        public string DisplayColorClass
        {
            get
            {
                if (PlayerClassId == null)
                {
                    return null;
                }

                return string.Format("classtype-{0}", PlayerClass.Name.ToLower());
            }
        }
        public int? PlayerClassId { get; set; }

        public PlayerClass PlayerClass { get; set; }

        public Player()
        {
            Souls = new List<Soul>();
        }
    }
}
