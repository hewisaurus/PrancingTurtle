using Newtonsoft.Json;

namespace LogParserConcept.Models.ParsedStats
{
    public class HealingDoneStats
    {
        [JsonProperty("total")]
        public long Total { get; set; }
        [JsonProperty("events")]
        public int Events { get; set; }
    }
}
