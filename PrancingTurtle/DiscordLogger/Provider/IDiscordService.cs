using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace DiscordLogger.Provider
{
    public interface IDiscordService
    {
        // Generic logging
        Task Log(string message, string sender, LogLevel level);
        // Multiple line logging
        Task Log(List<string> messageLines, string sender, LogLevel level);
    }
}
