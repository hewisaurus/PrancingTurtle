using Newtonsoft.Json;

namespace DiscordLogger.Classes
{
    [JsonObject]
    public interface IEmbedUrl
    {
        [JsonProperty("url")]
        string Url { get; set; }
    }
}
