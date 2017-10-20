using Database.Models;

namespace Database.QueryModels.Misc
{
    public class RankPlayerGuild
    {
        public int Id { get; set; }
        public int EncounterId { get; set; }
        public int PlayerId { get; set; }
        public long Value { get; set; }
        public string AbilityIcon { get; set; }
        public string AbilityName { get; set; }
        public long TopHit { get; set; }
        public string Class { get; set; }
        public string Role { get; set; }
        public bool EncounterPublic { get; set; }

        public string DisplayColorClass
        {
            get
            {
                if (string.IsNullOrEmpty(Class))
                {
                    return null;
                }

                return string.Format("classtype-{0}", Class.ToLower());
            }
        }
        
        public Player Player { get; set; }
        public Guild Guild { get; set; }
    }
}
