namespace Database.MySQL
{
    public static class EncounterDifficulty
    {
        public static string Default
        {
            get { return "SELECT * FROM EncounterDifficulty WHERE Name = 'Normal' LIMIT 1"; }
        }

        public static string GetById
        {
            get { return "SELECT * FROM EncounterDifficulty WHERE Id = @id"; }
        }
    }
}
