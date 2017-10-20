using System;

namespace Database.QueryModels.Parser
{
    public class SessionLogInfo
    {
        public DateTime SessionDate { get; set; }
        public int SessionId { get; set; }
        public string SessionName { get; set; }
        public int SessionLogId { get; set; }
        public int OwnerId { get; set; }
        public string OwnerName { get; set; }
        public string OwnerShard { get; set; }
        public string OwnerGuild { get; set; }
        public int UploaderId { get; set; }
        public string UploaderName { get; set; }
        public string UploaderShard { get; set; }
        public string UploaderGuild { get; set; }
        public int UploaderGuildId { get; set; }
        public string UploaderTimezone { get; set; }
        public bool PublicSession { get; set; }

        public string OwnerInfo
        {
            get { return string.Format("{0}@{1} <{2}>", OwnerName, OwnerShard, OwnerGuild); }
        }

        public string UploaderInfo
        {
            get { return string.Format("{0}@{1} <{2}>", UploaderName, UploaderShard, UploaderGuild); }
        }
    }
}
