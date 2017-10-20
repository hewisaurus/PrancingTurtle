namespace PrancingTurtle.Models.Misc
{
    public class PlayerRole
    {
        public string Name { get; set; }
        public string Role { get; set; }

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
    }
}