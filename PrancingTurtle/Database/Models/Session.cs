using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Database.Models
{
    public class Session
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Duration { get; set; }
        public long TotalPlayTime { get; set; } //Stored in ticks
        [Required]
        [MaxLength(50, ErrorMessage = "The session name must be 50 characters or less")]
        public string Name { get; set; }
        public int AuthUserCharacterId { get; set; }
        public string UploadToken { get; set; }
        public string Filename { get; set; }
        public bool EncountersPublic { get; set; }
        public long SessionSize { get; set; }
        public List<Instance> InstancesSeen { get; set; }
        public List<string> BossesSeen { get; set; }
        public List<string> BossesSeenNotKilled { get; set; } 

        public string SessionSizeReadable
        {
            get
            {
                if (SessionSize < 1024) return string.Format("{0}b", SessionSize);
                else if (SessionSize < 1048576) return string.Format("{0}kb", SessionSize / 1024);
                else if (SessionSize < 1073741824) return string.Format("{0}mb", SessionSize / 1048576);
                else return string.Format("{0}gb", SessionSize / 1073741824);
            }
        }

        public string TotalPlayTimeReadable
        {
            get
            {
                TimeSpan totalPlayTimeSpan = new TimeSpan(TotalPlayTime);
                return totalPlayTimeSpan.ToString();
            }
        }

        public AuthUserCharacter AuthUserCharacter { get; set; }

        public Session()
        {
            InstancesSeen = new List<Instance>();
            BossesSeen = new List<string>();
            BossesSeenNotKilled = new List<string>();
        }
    }
}
