using System;
using System.Collections.Generic;
using Database.QueryModels;

namespace PrancingTurtle.Models.ViewModels
{
    public class ManageIndexVM
    {
        public DateTime LastLoggedIn { get; set;}
        public string TimeZoneId { get; set; }
        public string TimeZoneDisplay { get; set; }
        public string LastLoginAddress { get; set; }
        public bool ShortMenuFormat { get; set; }
        public bool ShowGuildMenu { get; set; }

        public List<CharacterGuild> Characters { get; set; }
        //public List<AuthUserCharacterGuild> Guilds { get; set; } 

        public ManageIndexVM()
        {
            Characters = new List<CharacterGuild>();
            //Guilds = new List<AuthUserCharacterGuild>();
        }

        public ManageIndexVM(List<CharacterGuild> characters)
        {
            Characters = new List<CharacterGuild>(characters);
            //Guilds = new List<AuthUserCharacterGuild>();
        }
    }
}