namespace Database.Models
{
    public class EncounterBuffAction
    {
        public int Id { get; set; }
        public int EncounterId { get; set; }
        public int AbilityId { get; set; }
        public string BuffName { get; set; }
        public string SourceId { get; set; }
        public string SourceName { get; set; }
        public string SourceType { get; set; }
        public string TargetId { get; set; }
        public string TargetName { get; set; }
        public string TargetType { get; set; }
        public int SecondBuffWentUp { get; set; }
        public int SecondBuffWentDown { get; set; }

        public string TextColour
        {
            get
            {
                return SourceType.ToUpper() == "NPC" ? "text-danger" : "text-info";
            }
        }
        public bool BoldText
        {
            get
            {
                return SourceType.ToUpper() == "NPC";
            }
        }

        public EncounterBuffAction()
        {

        }

        public EncounterBuffAction(EncounterBuffAction actionToCopy)
        {
            EncounterId = actionToCopy.EncounterId;
            AbilityId = actionToCopy.AbilityId;
            SecondBuffWentUp = actionToCopy.SecondBuffWentUp;
            SecondBuffWentDown = actionToCopy.SecondBuffWentDown;
            BuffName = actionToCopy.BuffName;
            SourceId = actionToCopy.SourceId;
            SourceName = actionToCopy.SourceName;
            SourceType = actionToCopy.SourceType;
            TargetId = actionToCopy.TargetId;
            TargetName = actionToCopy.TargetName;
            TargetType = actionToCopy.TargetType;
        }
    }
}
