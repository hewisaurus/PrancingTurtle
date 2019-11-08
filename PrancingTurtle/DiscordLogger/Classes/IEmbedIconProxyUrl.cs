using Newtonsoft.Json;

namespace DiscordLogger.Classes
{
    [JsonObject]
    public interface IEmbedIconProxyUrl
    {
        [JsonProperty("proxy_icon_url")]
        string ProxyIconUrl { get; set; }
    }
}
