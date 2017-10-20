namespace Database.MySQL
{
    public static class BossFightDifficulty
    {
        public static string DifficultyRecordsExist
        {
            get { return "SELECT IF(EXISTS (SELECT * FROM BossFightDifficulty WHERE BossFightId = @bossFightId), 1, 0) RESULT"; }
        }

        public static string GetAll
        {
            get { return "SELECT * FROM BossFightDifficulty BFD JOIN EncounterDifficulty ED ON BFD.EncounterDifficultyId = ED.Id WHERE BossFightId = @bossFightId"; }
        }
    }
}
