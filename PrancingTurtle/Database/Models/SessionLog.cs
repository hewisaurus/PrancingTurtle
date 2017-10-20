
using System;

namespace Database.Models
{
    public class SessionLog
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public int AuthUserCharacterId { get; set; }
        public int GuildId { get; set; }
        public string Token { get; set; }
        public string Filename { get; set; }
        public long LogSize { get; set; }
        public long TotalPlayedTime { get; set; }
        public long LogLines { get; set; }
        public DateTime CreationDate { get; set; }

        public Guild Guild { get; set; }
    }
}
