using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace LogParserConcept.Models.ParsedStats
{
    public class DeathStats
    {
        [JsonProperty("players")]
        public List<CharacterDeath> PlayerDeaths { get; set; }
        [JsonProperty("npcs")]
        public List<CharacterDeath> NpcDeaths { get; set; }
    }
}
