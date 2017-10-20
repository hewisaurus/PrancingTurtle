using Common;

namespace Database.QueryModels
{
    public class EncounterPlayerDamageDoneDetail
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

        public string AbilityName { get; set; }
        public string Icon { get; set; }
        public string DamageType { get; set; }

        public int Crits { get; set; }
        public int Hits { get; set; }
        public int Swings
        {
            get { return Crits + Hits; }
        }

        public long BiggestHit { get; set; }
        public long AverageHit { get; set; }

        public string RowClass
        {
            get
            {
                {
                    if (string.IsNullOrEmpty(DamageType)) return "";
                    return DamageType == "NA" ? "info" : "";
                }
            }
        }

        public string Displayclass
        {
            get
            {
                return string.IsNullOrEmpty(DamageType) ? "damagetype-unknown" : string.Format("damagetype-{0}", DamageType.ToLower());
            }
        }
        public string CellClass
        {
            get
            {
                return string.IsNullOrEmpty(DamageType) ? "damagetype-unknown-cell" : string.Format("damagetype-{0}-cell", DamageType.ToLower());
            }
        }
    }
}
