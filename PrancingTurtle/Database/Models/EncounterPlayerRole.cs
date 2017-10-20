namespace Database.Models
{
    public class EncounterPlayerRole
    {
        public long Id { get; set; }
        public int EncounterId { get; set; }
        public int PlayerId { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public string Class { get; set; }

        public string Icon
        {
            get
            {
                switch (Role)
                {
                    case "Tank":
                        return "raid_icon_role_tank.png";
                        break;
                    case "Healing":
                        return "raid_icon_role_heal.png";
                        break;
                    case "Support":
                        return "raid_icon_role_support.png";
                        break;
                    case "Damage":
                        return "raid_icon_role_dps.png";
                        break;
                    default:
                        return "raid_icon_role_dps.png";
                        break;

                }
            }
        }

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
    }
}
