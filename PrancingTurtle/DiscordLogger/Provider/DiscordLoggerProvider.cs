using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;

namespace DiscordLogger.Provider
{
    public class DiscordLoggerProvider : IDiscordLoggerProvider
    {
        private readonly IDiscordService _discord;
        private readonly ConcurrentDictionary<string, DiscordLogSender> _loggers = new ConcurrentDictionary<string, DiscordLogSender>();
        private readonly DiscordLoggerConfiguration _config;

        public DiscordLoggerProvider(DiscordLoggerConfiguration config, IDiscordService discord, 
            string includedPrefixes = "", string excludedPrefixes = "")
        {
            _config = config;
            _discord = discord;
            _config.IncludedPrefixes = includedPrefixes;
            _config.ExcludedPrefixes = excludedPrefixes;
        }

        public void Dispose()
        {
            _loggers.Clear();
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, name => new DiscordLogSender(name, _config, _discord));
        }
    }
}
