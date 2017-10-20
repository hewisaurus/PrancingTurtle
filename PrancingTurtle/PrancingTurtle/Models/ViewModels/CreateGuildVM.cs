using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PrancingTurtle.Models.ViewModels
{
    public class CreateGuildVM
    {
        public int AuthUserCharacterId { get; set; }
        [Required]
        [DisplayName("Guild Name")]
        public string Name { get; set; }
        public int ShardId { get; set; }
        public string ShardName { get; set; }

        //public List<Shard> Shards { get; set; } 

    }
}