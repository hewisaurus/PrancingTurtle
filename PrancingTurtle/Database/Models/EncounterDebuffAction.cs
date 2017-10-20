namespace Database.Models
{
    public class EncounterDebuffAction
    {
        public int Id { get; set; }
        public int EncounterId { get; set; }
        public int AbilityId { get; set; }
        public string DebuffName { get; set; }
        public string SourceId { get; set; }
        public string SourceName { get; set; }
        public string SourceType { get; set; }
        public string TargetId { get; set; }
        public string TargetName { get; set; }
        public string TargetType { get; set; }
        public int SecondDebuffWentUp { get; set; }
        public int SecondDebuffWentDown { get; set; }

        public string TextColour
        {
            get
            {
                return SourceType.ToUpper() == "NPC"? "text-danger" : "text-info";
            }
        }

        public bool BoldText
        {
            get
            {
                return SourceType.ToUpper() == "NPC";
            }
        }
        

        public EncounterDebuffAction()
        {

        }

        public EncounterDebuffAction(EncounterDebuffAction actionToCopy)
        {
            EncounterId = actionToCopy.EncounterId;
            AbilityId = actionToCopy.AbilityId;
            SecondDebuffWentUp = actionToCopy.SecondDebuffWentUp;
            SecondDebuffWentDown = actionToCopy.SecondDebuffWentDown;
            DebuffName = actionToCopy.DebuffName;
            SourceId = actionToCopy.SourceId;
            SourceName = actionToCopy.SourceName;
            SourceType = actionToCopy.SourceType;
            TargetId = actionToCopy.TargetId;
            TargetName = actionToCopy.TargetName;
            TargetType = actionToCopy.TargetType;
        }
    }
}
