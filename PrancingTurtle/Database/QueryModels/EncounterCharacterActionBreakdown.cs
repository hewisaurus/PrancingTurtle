using Common;

namespace Database.QueryModels
{
    public class EncounterCharacterActionBreakdown
    {
        public string TargetId { get; set; }
        public string TargetName { get; set; }
        public long Total { get; set; }
        public long Average { get; set; }
        public CharacterType Type { get; set; }
    }
}
