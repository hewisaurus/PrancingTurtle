using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace LogParserConcept.Models.ParsedStats
{
    public class DamageDoneStats
    {
        [JsonProperty("total")]
        public long Total { get; set; }
        [JsonProperty("events")]
        public int Events { get; set; }
    }
}
