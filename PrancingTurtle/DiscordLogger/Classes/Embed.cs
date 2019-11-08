using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DiscordLogger.Classes
{
    [JsonObject]
    public class Embed : IEmbedUrl
    {
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; } = "rich";
        [JsonProperty("description")]
        public string Description { get; set; }
        public string Url { get; set; }
        [JsonProperty("timestamp")]
        public DateTimeOffset? TimeStamp { get; set; }
        [JsonProperty("color")]
        public int Color { get; set; }
        [JsonProperty("footer")]
        public EmbedFooter Footer { get; set; }
        [JsonProperty("image")]
        public EmbedImage Image { get; set; }
        [JsonProperty("thumbnail")]
        public EmbedThumbnail Thumbnail { get; set; }
        [JsonProperty("video")]
        public EmbedVideo Video { get; set; }
        [JsonProperty("provider")]
        public EmbedProvider Provider { get; set; }
        [JsonProperty("author")]
        public EmbedAuthor Author { get; set; }
        [JsonProperty("fields")]
        public List<EmbedField> Fields { get; set; } = new List<EmbedField>();
    }
}
