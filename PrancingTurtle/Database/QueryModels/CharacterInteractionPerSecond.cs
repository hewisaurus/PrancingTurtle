namespace Database.QueryModels
{
    /// <summary>
    /// The class to hold all incoming and outgoing dps/hps/aps for graphs
    /// </summary>
    public class CharacterInteractionPerSecond
    {
        public int SecondsElapsed { get; set; }
        public string SourceId { get; set; }
        public string SourceName { get; set; }
        public string TargetId { get; set; }
        public string TargetName { get; set; }
        public string AbilityName { get; set; }
        public long Total { get; set; }
        public long Effective { get; set; } 
    }
}
