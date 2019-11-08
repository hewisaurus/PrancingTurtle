using Newtonsoft.Json;

namespace DiscordLogger.Classes
{
    [JsonObject]
    public abstract class EmbedProxyUrl : EmbedUrl, IEmbedProxyUrl
    {
        public string ProxyUrl { get; set; }
    }
}
