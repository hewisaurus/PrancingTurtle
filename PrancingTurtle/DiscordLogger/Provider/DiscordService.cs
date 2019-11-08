using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using DiscordLogger.Classes;

namespace DiscordLogger.Provider
{
    public class DiscordService : IDiscordService
    {
        private readonly IWebhookRepo _discord;

        public DiscordService(IWebhookRepo discord)
        {
            _discord = discord;
        }

        public async Task Log(string message, string sender, LogLevel level)
        {
            await _discord.Send(BuildMessage(message, sender, level));
        }

        private WebhookMessage BuildMessage(string message, string sender, LogLevel level)
        {
            var embed = new Embed()
            {
                Title = $"{level.ToString()} from {sender}",
                Description = message,
                Footer = new EmbedFooter
                {
                    Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                }
            };

            switch (level)
            {
                case LogLevel.Critical:
                case LogLevel.Error:
                    embed.Color = Color.Red.ToRgb();
                    break;
                case LogLevel.Information:
                    embed.Color = Color.Gray.ToRgb();
                    break;
                case LogLevel.Debug:
                    embed.Color = Color.MediumPurple.ToRgb();
                    break;
                case LogLevel.Warning:
                    embed.Color = Color.Orange.ToRgb();
                    break;
                case LogLevel.Trace:
                    embed.Color = Color.White.ToRgb();
                    break;
                default:
                    embed.Color = Color.Pink.ToRgb();
                    break;
            }

            var msg = new WebhookMessage
            {
                Embeds = new List<Embed>
                {
                    embed
                }
            };

            return msg;
        }
    }
}
