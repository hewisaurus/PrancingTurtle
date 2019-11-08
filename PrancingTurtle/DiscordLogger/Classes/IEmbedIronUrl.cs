using Newtonsoft.Json;

namespace DiscordLogger.Classes
{
    [JsonObject]
    public interface IEmbedIconUrl
    {
        [JsonProperty("icon_url")]
        string IconUrl { get; set; }
    }
}
