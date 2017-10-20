using System;

namespace Database.Models
{
    public class NewsRecentChanges
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public DateTime ItemDate { get; set; }
        public bool Visible { get; set; }

        public string PrintedDateTime
        {
            get { return ItemDate.ToString("dd/MM/yyyy"); }
        }

        public string PrintedFullObject
        {
            get { return string.Format("{0} - {1}", PrintedDateTime, Description); }
        }
    }
}
