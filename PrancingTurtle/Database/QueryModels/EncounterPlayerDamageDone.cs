using Common;

namespace Database.QueryModels
{
    public class EncounterPlayerDamageDone
    {
        public string TargetNpcId { get; set; }
        public string TargetNpcName { get; set; }
        public int TargetPlayerId { get; set; }
        public string TargetPlayerName { get; set; }
        public string TargetPetName { get; set; }

        public CharacterType TargetType
        {
            get
            {
                if (!string.IsNullOrEmpty(TargetNpcId)) return CharacterType.Npc;
                if (!string.IsNullOrEmpty(TargetPlayerName)) return CharacterType.Player;
                if (!string.IsNullOrEmpty(TargetPetName)) return CharacterType.Pet;
                return CharacterType.Unknown;
            }
        }

        public long Total { get; set; }
    }
}
