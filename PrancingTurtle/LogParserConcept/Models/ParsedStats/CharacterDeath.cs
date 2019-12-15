using Newtonsoft.Json;

namespace LogParserConcept.Models.ParsedStats
{
    public class CharacterDeath
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("deaths")]
        public int Deaths { get; set; }
    }
}
