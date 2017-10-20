using System;

namespace Database.Models
{
    public class ScheduledTask
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime LastRun { get; set; }
        public int ScheduleMinutes { get; set; }
    }
}
