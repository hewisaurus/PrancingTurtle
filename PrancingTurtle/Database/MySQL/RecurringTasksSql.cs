namespace Database.MySQL
{
    public static class RecurringTasksSql
    {
        public const string CountEncounterRequiringDeletion =
            "SELECT COUNT(1) AS Count FROM Encounter WHERE ToBeDeleted = 1 AND Removed = 0";
        public const string GetNextEncounterToDelete =
            "SELECT * FROM Encounter WHERE ToBeDeleted = 1 AND Removed = 0 ORDER BY DATE ASC LIMIT 1";

        public const string GetAllCountersRequiringDeletion =
            "SELECT * FROM Encounter WHERE ToBeDeleted = 1 AND Removed = 0";

        public const string DeleteEncounterFromOverview = "DELETE FROM EncounterOverview WHERE EncounterId = @id";
        public const string DeleteEncounterFromBuffEvent = "DELETE FROM EncounterBuffEvent WHERE EncounterId = @id";
        public const string DeleteEncounterFromBuffUptime = "DELETE FROM EncounterBuffUptime WHERE EncounterId = @id";
        public const string DeleteEncounterFromBuffAction = "DELETE FROM EncounterBuffAction WHERE EncounterId = @id";
        public const string DeleteEncounterFromDebuffAction = "DELETE FROM EncounterDebuffAction WHERE EncounterId = @id";
        public const string DeleteEncounterFromNpcCast = "DELETE FROM EncounterNpcCast WHERE EncounterId = @id";
        public const string DeleteEncounterFromDeath = "DELETE FROM EncounterDeath WHERE EncounterId = @id";
        public const string DeleteEncounterFromDamageDone = "DELETE FROM DamageDone WHERE EncounterId = @id";
        public const string DeleteEncounterFromHealingDone = "DELETE FROM HealingDone WHERE EncounterId = @id";
        public const string DeleteEncounterFromShieldingDone = "DELETE FROM ShieldingDone WHERE EncounterId = @id";
        public const string DeleteEncounterFromPlayerRole = "DELETE FROM EncounterPlayerRole WHERE EncounterId = @id";
        public const string DeleteEncounterFromNpc = "DELETE FROM EncounterNpc WHERE EncounterId = @id";
        public const string DeleteEncounterFromPlayerStatistics = "DELETE FROM EncounterPlayerStatistics WHERE EncounterId = @id";

        public const string MarkEncounterAsRemoved = "UPDATE Encounter SET Removed = 1 WHERE Id = @id";
    }
}
