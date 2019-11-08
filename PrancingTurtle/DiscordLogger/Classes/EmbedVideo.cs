using Newtonsoft.Json;

namespace DiscordLogger.Classes
{
    [JsonObject]
    public class EmbedVideo : EmbedUrl, IEmbedDimension
    {
        public int Height { get; set; }
        public int Width { get; set; }
    }
}
