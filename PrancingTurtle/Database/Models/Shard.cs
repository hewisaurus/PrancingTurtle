namespace Database.Models
{
    public class Shard
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Region { get; set; }
        public string ShardType { get; set; }

        // UI Properties
        public string DisplayName
        {
            get { return string.IsNullOrEmpty(Name) ? null : string.Format("{0} ({1} {2})", Name, Region, ShardType); }
        }
    }
}
