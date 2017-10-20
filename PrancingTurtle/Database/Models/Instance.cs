using System.Collections.Generic;
using System.ComponentModel;

namespace Database.Models
{
    public class Instance
    {
        public int Id { get; set; }
        public string Name { get; set; }

        [DisplayName("Max Raid Size")]
        public int MaxRaidSize { get; set; }
        public bool Visible { get; set; }

        [DisplayName("Include in Progression")]
        public bool IncludeInProgression { get; set; }

        [DisplayName("Include in Lists")]
        public bool IncludeInLists { get; set; }

        [DisplayName("Short Name")]
        public string ShortName { get; set; }

        public string ImageFilename { get; set; }
        public int TierNumber { get; set; }
        public int GameVersion { get; set; }
        public bool ForceShowTier { get; set; }

        // UI Lists
        public List<string> DifficultiesSeen { get; set; }
    }
}
