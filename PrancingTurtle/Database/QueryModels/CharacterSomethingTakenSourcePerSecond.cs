namespace Database.QueryModels
{
    public class CharacterSomethingTakenSourcePerSecond
    {
        public int SecondsElapsed { get; set; }
        public string SourceId { get; set; }
        public string SourceName { get; set; }
        public long Total { get; set; }
        public long Effective { get; set; } 
    }
}
