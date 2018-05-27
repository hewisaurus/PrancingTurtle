using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Models
{
    public class GameEdition
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string DisplayClass { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
    }
}
