using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.SQL
{
    public static class EncounterRemovalFrom
    {
        public const string EncounterOverview = "DELETE FROM EncounterOverview WHERE EncounterId = @id";
        public const string DamageDone = "DELETE FROM DamageDone WHERE EncounterId = @id";
        public const string HealingDone = "DELETE FROM HealingDone WHERE EncounterId = @id";
        public const string ShieldingDone = "DELETE FROM ShieldingDone WHERE EncounterId = @id";
        public const string EncounterBuffEvent = "DELETE FROM EncounterBuffEvent WHERE EncounterId = @id";
        public const string EncounterBuffUptime = "DELETE FROM EncounterBuffUptime WHERE EncounterId = @id";
        public const string EncounterBuffAction = "DELETE FROM EncounterBuffAction WHERE EncounterId = @id";
        public const string EncounterDebuffAction = "DELETE FROM EncounterDebuffAction WHERE EncounterId = @id";
        public const string EncounterNpcCast = "DELETE FROM EncounterNpcCast WHERE EncounterId = @id";
        public const string EncounterDeath = "DELETE FROM EncounterDeath WHERE EncounterId = @id";
        public const string EncounterPlayerRole = "DELETE FROM EncounterPlayerRole WHERE EncounterId = @id";
        public const string EncounterNpc = "DELETE FROM EncounterNpc WHERE EncounterId = @id";
        public const string EncounterPlayerStatistics = "DELETE FROM EncounterPlayerStatistics WHERE EncounterId = @id";

        public const string GetEncounterRecordCounts = 
            "SELECT * FROM ( " +
            "(SELECT COUNT(1) AS Damage FROM DamageDone WHERE EncounterId = @id) S1, " +
            "(SELECT COUNT(1) AS Healing FROM HealingDone WHERE EncounterId = @id) S2, " +
            "(SELECT COUNT(1) AS Shielding FROM ShieldingDone WHERE EncounterId = @id) S3, " +
            "(SELECT COUNT(1) AS Overview FROM EncounterOverview WHERE EncounterId = @id) S4, " +
            "(SELECT COUNT(1) AS BuffEvent FROM EncounterBuffEvent WHERE EncounterId = @id) S5, " +
            "(SELECT COUNT(1) AS BuffUptime FROM EncounterBuffUptime WHERE EncounterId = @id) S6, " +
            "(SELECT COUNT(1) AS BuffAction FROM EncounterBuffAction WHERE EncounterId = @id) S7, " +
            "(SELECT COUNT(1) AS DebuffAction FROM EncounterDebuffAction WHERE EncounterId = @id) S8, " +
            "(SELECT COUNT(1) AS NpcCast FROM EncounterNpcCast WHERE EncounterId = @id) S9, " +
            "(SELECT COUNT(1) AS Death FROM EncounterDeath WHERE EncounterId = @id) S10, " +
            "(SELECT COUNT(1) AS PlayerRole FROM EncounterPlayerRole WHERE EncounterId = @id) S11, " +
            "(SELECT COUNT(1) AS Npc FROM EncounterNpc WHERE EncounterId = @id) S12, " +
            "(SELECT COUNT(1) AS PlayerStatistics FROM EncounterPlayerStatistics WHERE EncounterId = @id) S13 " +
            ")";

        public const string SetEncounterRemoved = "UPDATE Encounter SET Removed = 1 WHERE Id = @id";
    }
}
