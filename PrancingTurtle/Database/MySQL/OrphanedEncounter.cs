namespace Database.MySQL
{
    public static class OrphanedEncounter
    {
        public const string EncounterIdsWithoutTableStats = 
            "SELECT E.Id FROM Encounter E " +
            "LEFT JOIN EncounterTableStats ETS ON E.Id = ETS.EncounterId " +
            "WHERE ETS.Id IS NULL " +
            "ORDER BY E.Id";

        public const string InsertTableStatsForEncounter =
            "INSERT INTO EncounterTableStats(EncounterId,DamageRecords,HealingRecords,ShieldingRecords)" +
            "VALUES(@id,@damage,@healing,@shielding)";

        public const string GetBasicEncounterStats =
            "SELECT * FROM ( " +
            "(SELECT COUNT(1) AS Damage FROM DamageDone WHERE EncounterId = @id) S1, " +
            "(SELECT COUNT(1) AS Healing FROM HealingDone WHERE EncounterId = @id) S2, " +
            "(SELECT COUNT(1) AS Shielding FROM ShieldingDone WHERE EncounterId = @id) S3 " +
            ")";

        public const string EncounterHasDamageRecords = "SELECT COUNT(1) FROM DamageDone WHERE EncounterId = @id";
        public const string EncounterHasHealingRecords = "SELECT COUNT(1) FROM HealingDone WHERE EncounterId = @id";
    }
}
