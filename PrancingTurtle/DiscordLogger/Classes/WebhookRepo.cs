using System;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
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
            try
            {
                var result = await _httpClient.PostAsync(_webhookUrl, content);
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new HttpResponseMessage(HttpStatusCode.NotAcceptable);
            }
        }

        public async Task<HttpResponseMessage> SendAsync(WebhookMessage message)
        {
            var content = new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json");
            
            try
            {
                var response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);

                using (var client = new HttpClient())
                {
                    response = await client.PostAsync(_webhookUrl, content);
                }

                if (!response.IsSuccessStatusCode)
                {
                    var reason = response.ReasonPhrase;
                }

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new HttpResponseMessage(HttpStatusCode.NotAcceptable);
            }
        }
    }
}
