using System;

namespace Database.Models
{
    public class SiteNotification
    {
        public int Id { get; set; }
        public string Header { get; set; }
        public string Body { get; set; }
        public string ColourClass { get; set; }
        public DateTime Created { get; set; }
        public bool Visible { get; set; }
    }
}
