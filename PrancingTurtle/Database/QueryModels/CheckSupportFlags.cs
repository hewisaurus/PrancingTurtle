namespace Database.QueryModels
{
    public class CheckSupportFlags
    {
        public int EncounterId { get; set; }
        public int PlayerId { get; set; }
        public string PlayerName { get; set; }
        public bool IsSupport { get; set; }
    }
}
