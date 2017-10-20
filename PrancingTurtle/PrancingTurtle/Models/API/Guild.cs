using System.Collections.Generic;
using System.Net;

namespace PrancingTurtle.Models.API
{
    public class Guild
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Shard { get; set; }
        public string Region { get; set; }
    }

    public class GuildQuery
    {
        public List<Guild> Guilds;
        public HttpStatusCode StatusCode;
        public string Message;

        public GuildQuery()
        {
            Guilds = new List<Guild>();
            StatusCode = HttpStatusCode.OK;
        }
    }
}