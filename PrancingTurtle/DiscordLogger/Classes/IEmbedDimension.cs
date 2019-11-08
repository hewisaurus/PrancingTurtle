using Newtonsoft.Json;

namespace DiscordLogger.Classes
{
    [JsonObject]
    public interface IEmbedDimension
    {
        [JsonProperty("height")]
        int Height { get; set; }
        [JsonProperty("width")]
        int Width { get; set; }
    }
}
