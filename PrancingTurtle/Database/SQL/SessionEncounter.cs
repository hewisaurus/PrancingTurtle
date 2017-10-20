namespace Database.SQL
{
    public static class SessionEncounter
    {
        public static string EncounterIds { get { return "SELECT DISTINCT EncounterId FROM SessionEncounter"; } } 
    }
}
