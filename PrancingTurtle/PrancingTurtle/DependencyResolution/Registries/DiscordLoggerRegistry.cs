using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using DiscordLogger.Classes;
using DiscordLogger.Provider;
using StructureMap;

namespace PrancingTurtle.DependencyResolution.Registries
{
    public class DiscordLoggerRegistry : Registry
    {
        public DiscordLoggerRegistry()
        {
            var id = ulong.Parse(ConfigurationManager.AppSettings["DiscordId"]);
            var token = ConfigurationManager.AppSettings["DiscordToken"];

            For<IWebhookRepo>().Use<WebhookRepo>().Ctor<ulong>("id").Is(id).Ctor<string>("token").Is(token);
            For<IDiscordService>().Use<DiscordService>();
        }
    }
}