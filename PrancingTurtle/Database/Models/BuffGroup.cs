namespace Database.Models
{
    public class BuffGroup
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsABuff { get; set; }
        public bool TrackOnUI { get; set; }
    }
}
