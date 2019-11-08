using System.Net.Http;
using System.Threading.Tasks;

namespace DiscordLogger.Classes
{
    public interface IWebhookRepo
    {
        Task<HttpResponseMessage> Send(WebhookMessage message);
    }
}
