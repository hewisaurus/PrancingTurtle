namespace Database.QueryModels
{
    public class CharacterBuffAction
    {
        //EBA.BuffName, EBA.SourceName, EBA.SecondBuffWentUp, EBA.SecondBuffWentDown, A.Icon, S.[Role] 
        public string BuffName { get; set; }
        public string SourceName { get; set; }
        public int SecondBuffWentUp { get; set; }
        public int SecondBuffWentDown { get; set; }
        public string Icon { get; set; }
        public string Role { get; set; }
    }
}
