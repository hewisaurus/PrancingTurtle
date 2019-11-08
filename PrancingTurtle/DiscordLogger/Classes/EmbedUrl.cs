using Newtonsoft.Json;

namespace DiscordLogger.Classes
{
    [JsonObject]
    public abstract class EmbedUrl : IEmbedUrl
    {
        public string Url { get; set; }
    }
}
