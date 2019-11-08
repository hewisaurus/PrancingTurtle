using Microsoft.Extensions.Logging;

namespace DiscordLogger
{
    public class DiscordLoggerConfiguration
    {
        public LogLevel LogLevel { get; set; } = LogLevel.Information;
        public int EventId { get; set; } = 0;
        public string IncludedPrefixes { get; set; }
        public string ExcludedPrefixes { get; set; }
    }
}
