namespace Database.QueryModels
{
    public class CharacterDamageDoneAbilityPerSecond
    {
        public int SecondsElapsed { get; set; }
        public string AbilityName { get; set; }
        public long Total { get; set; }
        public long Effective { get; set; }
    }
}
