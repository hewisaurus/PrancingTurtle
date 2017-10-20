namespace Database.Models
{
    public class EncounterDifficulty
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public int Priority { get; set; }

        public string DisplayClass
        {
            get
            {
                if (string.IsNullOrEmpty(Name)) return null;
                switch (Name)
                {
                    case "Hard":
                        return "label label-danger";
                    case "Easy":
                        return "label label-success";
                }
                return null;
            }
        }
    }
}
