using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace DiscordLogger.Provider
{
    public interface IDiscordService
    {
        // Generic logging
        Task Log(string message, string sender, LogLevel level);
    }
}
