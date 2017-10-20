using System.Collections.Generic;

namespace Database.Models
{
    public class Buff
    {
        public int Id { get; set; }
        public int BuffGroupId { get; set; }
        public bool IsPrimary { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public bool ShowByDefault { get; set; }

        public BuffGroup BuffGroup { get; set; }

        public List<BuffGroup> BuffGroups { get; set; } 
    }
}
