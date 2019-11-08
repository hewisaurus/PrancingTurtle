using Newtonsoft.Json;

namespace DiscordLogger.Classes
{
    [JsonObject]
    public class EmbedProvider : EmbedUrl
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
