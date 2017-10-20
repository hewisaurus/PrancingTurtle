using System.Collections.Generic;
using Database.Models;

namespace Database.QueryModels.Misc
{
    public class SearchResult
    {
        public List<PlayerSearchResult> Players { get; set; }
        public List<BossFight> Encounters { get; set; }
        public List<Guild> Guilds { get; set; } 

        public SearchResult()
        {
            Players = new List<PlayerSearchResult>();
            Encounters = new List<BossFight>();
            Guilds = new List<Guild>();
        }
    }
}
