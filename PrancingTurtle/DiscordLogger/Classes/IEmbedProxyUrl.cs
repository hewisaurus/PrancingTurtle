using Newtonsoft.Json;

namespace DiscordLogger.Classes
{
    [JsonObject]
    public interface IEmbedProxyUrl
    {
        [JsonProperty("proxy_url")]
        string ProxyUrl { get; set; }
    }
}
