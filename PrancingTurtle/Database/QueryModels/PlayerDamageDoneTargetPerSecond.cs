namespace Database.QueryModels
{
    /// <summary>
    /// TODO: THIS CLASS NEEDS TO HAVE ITS SOURCE FILE RENAMED AND CLASS RENAMED TO CharacterSomethingDoneTargetPerSecond
    /// </summary>
    public class CharacterDamageDoneTargetPerSecond
    {
        public int SecondsElapsed { get; set; }
        public string TargetId { get; set; }
        public string TargetName { get; set; }
        public long Total { get; set; }
        public long Effective { get; set; } 
    }
}
