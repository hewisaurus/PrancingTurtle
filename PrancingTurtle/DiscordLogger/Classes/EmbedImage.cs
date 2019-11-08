using Newtonsoft.Json;

namespace DiscordLogger.Classes
{
    [JsonObject]
    public class EmbedImage : EmbedProxyUrl, IEmbedDimension
    {
        public int Height { get; set; }
        public int Width { get; set; }
    }
}
