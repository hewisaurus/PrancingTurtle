using System;

namespace Database.SQL
{
    public static class Player
    {
        public static string GetAll
        {
            get { return "SELECT * FROM Player ORDER BY Name ASC"; }
        }

        /// <summary>
        /// The query to return the Player ID for a given Player Log ID
        /// Requires the parameter @logId for PlayerId
        /// </summary>
        [Obsolete("Use GetNpcNameFromLogId or GetPlayerNameFromLogId")]
        public static string GetPlayerIdFromPlayerLogId
        {
            get { return "SELECT TOP 1 Id FROM [Player] P WHERE PlayerId = @logId"; }
        }

        public static string GetPlayerNameFromLogId
        {
            get { return "SELECT TOP 1 Name FROM [Player] P WHERE PlayerId = @logId"; }
        }

        public static string GetNpcNameFromLogId
        {
            get
            {
                return "SELECT TOP 1 TargetNpcName FROM [DamageDone] D WHERE TargetNpcId = @logId";
            }
        }

        public static class Class
        {
            public static string FromShieldingDone {
                get { return @"SELECT A.ClassName, Count(A.ClassName) AS ClassCount FROM
                               (
                               SELECT DISTINCT A.Name AS AbilityName, S.Name AS SoulName, PC.Name AS ClassName
                               FROM [ShieldingDone] D
                               JOIN [Ability] A ON D.AbilityId = A.Id
                               JOIN [Soul] S ON A.SoulId = S.Id
                               JOIN [PlayerClass] PC ON S.PlayerClassId = PC.Id
                               WHERE D.SourcePlayerId = @playerId AND A.MinimumPointsInSoul > @minPoints
                               ) AS A
                               GROUP BY A.ClassName
                               ORDER BY ClassCount DESC";
                } 
            }

            public static string FromHealingDone
            {
                get
                {
                    return @"SELECT A.ClassName, Count(A.ClassName) AS ClassCount FROM
                               (
                               SELECT DISTINCT A.Name AS AbilityName, S.Name AS SoulName, PC.Name AS ClassName
                               FROM [HealingDone] D
                               JOIN [Ability] A ON D.AbilityId = A.Id
                               JOIN [Soul] S ON A.SoulId = S.Id
                               JOIN [PlayerClass] PC ON S.PlayerClassId = PC.Id
                               WHERE D.SourcePlayerId = @playerId AND A.MinimumPointsInSoul > @minPoints
                               ) AS A
                               GROUP BY A.ClassName
                               ORDER BY ClassCount DESC";
                }
            }

            public static string FromDamageDone
            {
                get
                {
                    return @"SELECT A.ClassName, Count(A.ClassName) AS ClassCount FROM
                               (
                               SELECT DISTINCT A.Name AS AbilityName, S.Name AS SoulName, PC.Name AS ClassName
                               FROM [DamageDone] D
                               JOIN [Ability] A ON D.AbilityId = A.Id
                               JOIN [Soul] S ON A.SoulId = S.Id
                               JOIN [PlayerClass] PC ON S.PlayerClassId = PC.Id
                               WHERE D.SourcePlayerId = @playerId AND A.MinimumPointsInSoul > @minPoints
                               ) AS A
                               GROUP BY A.ClassName
                               ORDER BY ClassCount DESC";
                }
            }
        }
    }
}
