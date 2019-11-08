using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DiscordLogger.Classes
{
    public class WebhookRepo : IWebhookRepo
    {
        private ulong _id;
        private string _token;
        private readonly HttpClient _httpClient;
        private readonly string _webhookUrl;

        public WebhookRepo(ulong id, string token)
        {
            _id = id;
            _token = token;
            _httpClient = new HttpClient();
            _webhookUrl = $"https://discordapp.com/api/webhooks/{id}/{token}";
        }

        public async Task<HttpResponseMessage> Send(WebhookMessage message)
        {
            var content = new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json");
            return await _httpClient.PostAsync(_webhookUrl, content);
        }
    }
}
