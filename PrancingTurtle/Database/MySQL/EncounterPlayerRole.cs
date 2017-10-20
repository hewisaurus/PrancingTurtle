namespace Database.MySQL
{
    public static class EncounterPlayerRole
    {
        public static string GetAllSorted
        {
            get { return "SELECT * FROM EncounterPlayerRole ORDER BY EncounterId, PlayerId, Id"; }
        }

        public static string DeleteMultiple
        {
            get { return "DELETE FROM EncounterPlayerRole WHERE Id IN @ids"; }
        }

        public static string GetAllForEncounter
        {
            get { return "SELECT * FROM EncounterPlayerRole WHERE EncounterId = @encounterId ORDER BY Name"; }
        }

        public static string GetEncountersWithClericSupport
        {
            get { return "SELECT EncounterId, PlayerId FROM EncounterPlayerRole WHERE Role = 'Support' AND Class = 'Cleric' GROUP BY EncounterId, PlayerId"; }
        }

        public static string SetRoleToDps
        {
            get { return "UPDATE EncounterPlayerRole SET Role = 'Damage' WHERE EncounterId = @encounterId AND PlayerId = @playerId"; }
        }

        public const string Add = "INSERT INTO EncounterPlayerRole(EncounterId,PlayerId,Name,Role,Class) VALUES (@encounterId,@playerId,@playerName,@playerRole,@playerClass)";
    }
}
