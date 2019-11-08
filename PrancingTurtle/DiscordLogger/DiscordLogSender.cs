using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordLogger.Provider;
using Microsoft.Extensions.Logging;

namespace DiscordLogger
{
    public class DiscordLogSender : ILogger
    {
        private readonly DiscordLoggerConfiguration _config;
        private readonly string _name;
        private readonly IDiscordService _discord;

        public DiscordLogSender(string name, DiscordLoggerConfiguration config, IDiscordService discord)
        {
            _name = name;
            _config = config;
            _discord = discord;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            // If we're whitelisting (included prefixes), ignore anything not included. Otherwise, check the blacklist (excluded)

            if (!string.IsNullOrEmpty(_config.IncludedPrefixes))
            {
                // Only log prefixes that match
                foreach (var prefix in _config.IncludedPrefixes.Split(','))
                {
                    if (_name.StartsWith(prefix))
                    {
                        _discord.Log(state.ToString(), _name, logLevel);
                        return;
                    }
                }
            }
            else if (!string.IsNullOrEmpty(_config.ExcludedPrefixes))
            {
                // Ignore prefixes that match
                foreach (var prefix in _config.ExcludedPrefixes.Split(','))
                {
                    if (_name.StartsWith(prefix))
                    {
                        return;
                    }
                }

                _discord.Log(state.ToString(), _name, logLevel);
            }
            else
            {
                // No included or excluded, so log everything.
                _discord.Log(state.ToString(), _name, logLevel);
            }
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel == _config.LogLevel;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }
    }
}
