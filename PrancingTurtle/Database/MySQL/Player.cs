namespace Database.MySQL
{
    public static class Player
    {
        public static string GetAll
        {
            get { return "SELECT * FROM Player"; }
        }

        public static string GetByIds
        {
            get { return "SELECT * FROM Player WHERE Id IN @playerIds"; }
        }

        public static string GetPlayerNameFromLogId
        {
            get { return "SELECT Name FROM Player P WHERE PlayerId = @logId LIMIT 0,1"; }
        }

        public static string GetNpcNameFromLogId
        {
            get
            {
                return "SELECT TargetNpcName FROM DamageDone D WHERE TargetNpcId = @logId LIMIT 0,1";
            }
        }

        public static string GetSinglePlayerSearchFromPlayerId
        {
            get { return "SELECT P.Id AS Id, P.Name AS Name, P.Shard AS Shard, P.Id AS PlayerId, " +
                         "P.Name AS CharacterName, P.Shard AS ShardName, G.Name AS GuildName, " +
                         "(CASE WHEN G.Id IS NULL THEN 0 ELSE G.ID END) AS GuildId, EPR.Class AS Class " +
                         "FROM Player P LEFT JOIN EncounterPlayerRole EPR ON P.Id = EPR.PlayerId " +
                         "LEFT JOIN Shard S ON P.Shard = S.Name " +
                         "LEFT JOIN AuthUserCharacter AUC ON P.Name = AUC.CharacterName AND AUC.ShardId = S.Id " +
                         "LEFT JOIN Guild G ON AUC.GuildId = G.Id " +
                         "WHERE P.Id = @playerId GROUP BY P.Id"; }
        }

        public static string SearchFromPlayerId
        {
            get
            {
                return "SELECT P.Id, P.PlayerId, AUC.CharacterName, S.Name AS ShardName, " +
                         "G.Id AS GuildId, G.Name AS GuildName, EPR.Class " +
                         "FROM AuthUserCharacter AUC " +
                         "JOIN Shard S ON AUC.ShardId = S.Id " +
                         "JOIN Player P ON P.Id = @playerId " +
                         "JOIN EncounterPlayerRole EPR ON P.Id = EPR.PlayerId " +
                         "LEFT JOIN Guild G ON AUC.GuildId = G.Id " +
                         "WHERE AUC.CharacterName = (SELECT Name FROM Player WHERE Id = @playerId) " +
                         "AND S.Name = (SELECT Shard FROM Player WHERE Id = @playerId) GROUP BY Id"; }
        }

        public static string SearchForPlayer
        {
            get
            {
                return "SELECT DISTINCT P.*, EPR.Class FROM Player P " +
                       "LEFT JOIN EncounterPlayerRole EPR ON P.Id = EPR.PlayerId " +
                       "LEFT JOIN " +
                       "(SELECT AUC.Id AS AuthUserCharacterId, AUC.CharacterName AS CharacterName, " +
                       "S.Name AS ShardName, G.Name AS GuildName, G.Id AS GuildId " +
                       "FROM AuthUserCharacter AUC " +
                       "JOIN Shard S ON AUC.ShardId = S.Id " +
                       "LEFT JOIN Guild G ON AUC.GuildId = G.Id " +
                       "WHERE AUC.CharacterName LIKE @searchTerm) R ON P.Name = R.CharacterName AND P.Shard = R.ShardName " +
                       "WHERE P.Name LIKE @searchTerm ORDER BY P.Name ASC, P.Shard ASC";
                //return "SELECT * FROM Player P LEFT JOIN " +
                //       "(SELECT AUC.Id AS AuthUserCharacterId, AUC.CharacterName AS CharacterName, S.Name AS ShardName, " +
                //       "G.Name AS GuildName, G.Id AS GuildId FROM AuthUserCharacter AUC " +
                //       "JOIN Shard S ON AUC.ShardId = S.Id LEFT JOIN Guild G ON AUC.GuildId = G.Id " +
                //       "WHERE AUC.CharacterName LIKE @searchTerm) R ON P.Name = R.CharacterName AND P.Shard = R.ShardName " +
                //       "WHERE P.Name LIKE @searchTerm ORDER BY P.Name ASC, P.Shard ASC";
                //return "SELECT * FROM Player WHERE Name LIKE @searchTerm ORDER BY Name ASC, Shard ASC";
            }
        }

        public static string PlayerWithShardNames
        {
            get { return "SELECT * FROM Player WHERE Name LIKE '%@%'"; }
        }

        public static string UpdatePlayerShardName
        {
            get { return "UPDATE Player SET Name = @name, Shard = @shard WHERE Id = @id"; }
        }

        public static string GetAllUniquePlayerIds
        {
            get { return "SELECT * FROM (" +
                         "SELECT SourcePlayerId FROM DamageDone WHERE SourcePlayerId IS NOT NULL GROUP BY SourcePlayerId " +
                         "UNION SELECT SourcePlayerId FROM HealingDone WHERE SourcePlayerId IS NOT NULL GROUP BY SourcePlayerId " +
                         "UNION SELECT SourcePlayerId FROM ShieldingDone WHERE SourcePlayerId IS NOT NULL GROUP BY SourcePlayerId " +
                         "UNION SELECT TargetPlayerId FROM DamageDone WHERE TargetPlayerId IS NOT NULL GROUP BY TargetPlayerId " +
                         "UNION SELECT TargetPlayerId FROM HealingDone WHERE TargetPlayerId IS NOT NULL GROUP BY TargetPlayerId " +
                         "UNION SELECT TargetPlayerId FROM ShieldingDone WHERE TargetPlayerId IS NOT NULL GROUP BY TargetPlayerId " +
                         "UNION SELECT TargetPlayerId FROM EncounterDeath WHERE TargetPlayerId IS NOT NULL GROUP BY TargetPlayerId " +
                         "UNION SELECT PlayerId FROM EncounterPlayerRole WHERE PlayerId IS NOT NULL GROUP BY PlayerId) C"; } 
        }

        public static string RemovePlayerById
        {
            get { return "DELETE FROM Player WHERE Id IN @Ids"; }
        }
    }
}
