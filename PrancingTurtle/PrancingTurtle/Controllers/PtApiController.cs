using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Database.Repositories.Interfaces;
using PrancingTurtle.Models.API;

namespace PrancingTurtle.Controllers
{
    public class PtApiController : ApiController
    {
        private readonly IApiRepository _apiRepository;
        private readonly IGuildRepository _guildRepository;

        public PtApiController(IGuildRepository guildRepository, IApiRepository apiRepository)
        {
            _guildRepository = guildRepository;
            _apiRepository = apiRepository;
        }

        public GuildQuery Get()
        {
            // Instead of returning this, return a list of valid methods (documentation)
            return new GuildQuery()
            {
                Guilds = new List<Guild>()
                {
                    new Guild()
                    {
                        Id = 1234,
                        Name = "GuildName",
                        Shard = "GuildShard",
                        Region = "NAorEU"
                    }
                },
                StatusCode = HttpStatusCode.OK,
                Message = ""
            };
        }

        [HttpGet]
        public GuildQuery ListGuilds(string authKey)
        {
            if (!_apiRepository.ValidateAuthKey(authKey))
            {
                return new GuildQuery()
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    Guilds = new List<Guild>() {new Guild() {Name = "Unauthorized"}}
                };
            }

            var returnValue = new GuildQuery();

            var guilds = _guildRepository.GetApprovedGuilds();
            foreach (var guild in guilds)
            {
                returnValue.Guilds.Add(new Guild()
                {
                    Id = guild.Id,
                    Name = guild.Name,
                    Shard = guild.Shard.Name,
                    Region = guild.Shard.Region
                });
            }

            return returnValue;
        }

        [HttpGet]
        public GuildQuery Guild(string authKey, string name)
        {
            if (!_apiRepository.ValidateAuthKey(authKey))
            {
                return new GuildQuery()
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    Guilds = new List<Guild>() { new Guild() { Name = "Unauthorized" } }
                };
            }

            var returnValue = new GuildQuery();
            var guild = _guildRepository.Get(name);

            if (guild == null)
            {
                returnValue.Guilds.Add(new Guild()
                {
                    Id = 0,
                    Name = "GuildNotFound",
                    Shard = "N/A",
                    Region = "N/A"
                });
            }
            else
            {
                returnValue.Guilds.Add(new Guild()
                {
                    Id = guild.Id,
                    Name = guild.Name,
                    Shard = guild.Shard.Name,
                    Region = guild.Shard.Region
                });
            }
            return returnValue;
        }
    }
}
