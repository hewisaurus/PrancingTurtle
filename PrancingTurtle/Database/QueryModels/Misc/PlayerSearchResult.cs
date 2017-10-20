namespace Database.QueryModels.Misc
{
    public class PlayerSearchResult
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Shard { get; set; }
        public string PlayerId { get; set; }
        public int AuthUserCharacterId { get; set; }
        public string CharacterName { get; set; }
        public string ShardName { get; set; }
        public string GuildName { get; set; }
        public int GuildId { get; set; }
        public string Class { get; set; }

        public string DisplayNameShort
        {
            get { return string.Format("{0}@{1}", Name, Shard); }
        }

        public string DisplayName
        {
            get
            {
                return string.IsNullOrEmpty(GuildName) 
                    ? string.Format("{0}@{1}", Name, Shard) 
                    : string.Format("{0}@{1} <{2}>", Name, Shard, GuildName);
            }
        }

        public string DisplayColorClass
        {
            get
            {
                if (string.IsNullOrEmpty(Class))
                {
                    return "text-info";
                }

                return string.Format("classtype-{0}", Class.ToLower());
            }
        }
    }
}
