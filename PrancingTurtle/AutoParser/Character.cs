using Common;

namespace AutoParser
{
    public class Character
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Shard { get; set; }
        public CharacterType Type { get; set; }

        public Character()
        {
            Type = CharacterType.Player;
        }
    }
}
