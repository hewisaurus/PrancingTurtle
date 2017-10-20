namespace Database.MySQL
{
    public static class EncounterNpc
    {
        public static string GetAllForEncounter
        {
            get { return "SELECT * FROM EncounterNpc WHERE EncounterId = @id"; }
        }

        public static string RecordsExistForEncounter
        {
            get { return "SELECT IF(EXISTS(SELECT * FROM EncounterNpc WHERE EncounterId = @id), 1, 0) AS RecordsExist"; }
        }

        public static string CalculateForEncounter
        {
            get
            {
                return "SELECT SourceNpcName AS NpcName, SourceNpcId AS NpcId FROM DamageDone WHERE EncounterId = @id AND SourceNpcId IS NOT NULL GROUP BY SourceNpcId UNION " +
                       "SELECT TargetNpcName AS NpcName, TargetNpcId AS NpcId FROM DamageDone WHERE EncounterId = @id AND TargetNpcId IS NOT NULL GROUP BY TargetNpcId UNION " +
                       "SELECT SourceNpcName AS NpcName, SourceNpcId AS NpcId FROM HealingDone WHERE EncounterId = @id AND SourceNpcId IS NOT NULL GROUP BY SourceNpcId UNION " +
                       "SELECT TargetNpcName AS NpcName, TargetNpcId AS NpcId FROM HealingDone WHERE EncounterId = @id AND TargetNpcId IS NOT NULL GROUP BY TargetNpcId UNION " +
                       "SELECT SourceNpcName AS NpcName, SourceNpcId AS NpcId FROM ShieldingDone WHERE EncounterId = @id AND SourceNpcId IS NOT NULL GROUP BY SourceNpcId UNION " +
                       "SELECT TargetNpcName AS NpcName, TargetNpcId AS NpcId FROM ShieldingDone WHERE EncounterId = @id AND TargetNpcId IS NOT NULL GROUP BY TargetNpcId";
            }
        }

        public static string GetNpcNameForId
        {
            get { return "SELECT NpcName FROM EncounterNpc WHERE EncounterId = @encounterId AND NpcId = @npcId LIMIT 1"; }
        }
        
        public const string Add = "INSERT INTO EncounterNpc(EncounterId,NpcName,NpcId) VALUES (@encounterId,@npcName,@npcId)";
    }
}
