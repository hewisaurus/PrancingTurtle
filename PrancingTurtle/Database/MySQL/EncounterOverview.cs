namespace Database.MySQL
{
    public static class EncounterOverview
    {
        public static string GetEncountersMissingHPSorAPS
        {
            get
            {
                return "SELECT E.* FROM EncounterOverview EO JOIN Encounter E ON EO.EncounterId = E.Id WHERE EO.AverageHps = -1 OR EO.AverageAps = -1 LIMIT @limit";
            }
        }

        public static string UpdateHpsApsByEncounterId
        {
            get
            {
                return "UPDATE EncounterOverview SET AverageHps = @averageHps, AverageAps = @averageAps WHERE EncounterId = @encounterId";
            }
        }
    }
}
