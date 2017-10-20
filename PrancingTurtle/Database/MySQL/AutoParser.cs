namespace Database.MySQL
{
    public static class AutoParser
    {
        public static string ListAllSessionLogInfo
        {
            get
            {
                return "SELECT S.Date AS SessionDate, S.Id AS SessionId, S.Name AS SessionName, " +
                       "SL.Id AS SessionLogId, AUC2.Id AS OwnerId, AUC2.CharacterName AS OwnerName, " +
                       "SH2.Name AS OwnerShard, G2.Name AS OwnerGuild, AUC1.Id AS UploaderId, " +
                       "AUC1.CharacterName AS UploaderName, SH1.Name AS UploaderShard, G1.Name AS UploaderGuild, " +
                       "AU.TimeZone AS UploaderTimezone, S.EncountersPublic AS PublicSession " +
                       "FROM SessionLog SL " +
                       "JOIN Session S ON SL.SessionId = S.Id " +
                       "JOIN AuthUserCharacter AUC1 ON SL.AuthUserCharacterId = AUC1.Id /* UPLOADER */ " +
                       "JOIN Shard SH1 ON AUC1.ShardId = SH1.Id " +
                       "JOIN Guild G1 ON AUC1.GuildId = G1.Id " +
                       "JOIN AuthUserCharacter AUC2 ON S.AuthUserCharacterId = AUC2.Id /* OWNER */ " +
                       "JOIN Shard SH2 ON AUC2.ShardId = SH2.Id " +
                       "JOIN Guild G2 ON AUC2.GuildId = G2.Id " +
                       "JOIN AuthUser AU ON AUC1.AuthUserId = AU.Id " +
                       "ORDER BY S.Id";
            }
        }

        public static string SingleSessionLogInfo
        {
            get
            {
                return "SELECT S.Date AS SessionDate, S.Id AS SessionId, S.Name AS SessionName, " +
                       "SL.Id AS SessionLogId, AUC2.Id AS OwnerId, AUC2.CharacterName AS OwnerName, " +
                       "SH2.Name AS OwnerShard, G2.Name AS OwnerGuild, AUC1.Id AS UploaderId, " +
                       "AUC1.CharacterName AS UploaderName, SH1.Name AS UploaderShard, G1.Name AS UploaderGuild, " +
                       "G1.Id AS UploaderGuildId, AU.TimeZone AS UploaderTimezone, S.EncountersPublic AS PublicSession " +
                       "FROM SessionLog SL " +
                       "JOIN Session S ON SL.SessionId = S.Id " +
                       "JOIN AuthUserCharacter AUC1 ON SL.AuthUserCharacterId = AUC1.Id /* UPLOADER */ " +
                       "JOIN Shard SH1 ON AUC1.ShardId = SH1.Id " +
                       "JOIN Guild G1 ON AUC1.GuildId = G1.Id " +
                       "JOIN AuthUserCharacter AUC2 ON S.AuthUserCharacterId = AUC2.Id /* OWNER */ " +
                       "JOIN Shard SH2 ON AUC2.ShardId = SH2.Id " +
                       "JOIN Guild G2 ON AUC2.GuildId = G2.Id " +
                       "JOIN AuthUser AU ON AUC1.AuthUserId = AU.Id " +
                       "WHERE SL.Token = @token LIMIT 0,1";
            }
        }

        public static string ClearTemporaryPlayerTable
        {
            get { return "DELETE FROM AutoParserPlayerChecking"; }
        }

        public static string ClearTemporaryAbilityTable
        {
            get { return "DELETE FROM AutoParserAbilityChecking"; }
        }

        public static string ComparePlayerTableToTemporary
        {
            get { return "SELECT AP.PlayerId FROM AutoParserPlayerChecking AP LEFT JOIN Player P ON AP.PlayerId = P.PlayerId WHERE P.PlayerId IS NULL"; }
        }

        public static string CompareAbilityTableToTemporary
        {
            get { return "SELECT AP.AbilityId FROM AutoParserAbilityChecking AP LEFT JOIN Ability A ON AP.AbilityId = A.AbilityId WHERE A.AbilityId IS NULL"; }
        }

        public static string GetPlayersByIds
        {
            get { return "SELECT * FROM Player WHERE PlayerId IN @playerIds"; }
        }

        public static string GetAbilityIdsWithNoDamageTypeByAbilityIds
        {
            get { return "SELECT AbilityId FROM Ability WHERE AbilityId IN @abilityIds AND DamageType IS NULL"; }
        }

        public static string GetShortAbilities
        {
            get { return "SELECT Id, AbilityId FROM Ability WHERE AbilityId IN @abilityIds"; }
        }

        public static string PlayerUpdater
        {
            get { return "UPDATE Player SET Name = @name, Shard = @shard WHERE PlayerId = @playerId"; }
        }

        public static string AbilityDamageTypeUpdaer
        {
            get { return "UPDATE Ability SET DamageType = @damageType WHERE AbilityId = @abilityId"; }
        }

        public static string GetBossFightsByName
        {
            get { return "SELECT * FROM BossFight BF " +
                         "JOIN Instance I ON BF.InstanceId = I.Id " +
                         "WHERE BF.Name = @fightName " +
                         "ORDER BY BF.UniqueAbilityName DESC"; }
        }
        public static string ListBossFights
        {
            get
            {
                return "SELECT * FROM PrancingTurtle.BossFight GROUP BY Name ORDER BY Name";
            }
        }

        public static string GetEncounterIdFromDetails
        {
            get 
            { 
                return "SELECT Id FROM Encounter WHERE Date = @date AND BossFightId = @bossFightId " +
                         "AND Duration = @duration AND ToBeDeleted = 0 AND UploaderId = @uploaderId LIMIT 0,1";
            }
        }

        public static string GetEncounterInfo
        {
            get
            {
                return "SELECT IF(EXISTS(SELECT * FROM EncounterOverview WHERE EncounterId = @id), 1, 0) As EncounterOverview; " +
                       "SELECT IF(EXISTS(SELECT * FROM EncounterBuffEvent WHERE EncounterId = @id), 1, 0) As EncounterBuffEvent; " +
                       "SELECT IF(EXISTS(SELECT * FROM EncounterBuffUptime WHERE EncounterId = @id), 1, 0) As EncounterBuffUptime; " +
                       "SELECT IF(EXISTS(SELECT * FROM EncounterDeath WHERE EncounterId = @id), 1, 0) As EncounterDeath; " +
                       "SELECT IF(EXISTS(SELECT * FROM DamageDone WHERE EncounterId = @id), 1, 0) As DamageDone; " +
                       "SELECT IF(EXISTS(SELECT * FROM HealingDone WHERE EncounterId = @id), 1, 0) As HealingDone; " +
                       "SELECT IF(EXISTS(SELECT * FROM ShieldingDone WHERE EncounterId = @id), 1, 0) As ShieldingDone; " +
                       "SELECT IF(EXISTS(SELECT * FROM EncounterDebuffAction WHERE EncounterId = @id), 1, 0) As EncounterDebuffAction; " +
                       "SELECT IF(EXISTS(SELECT * FROM EncounterBuffAction WHERE EncounterId = @id), 1, 0) As EncounterBuffAction; " +
                       "SELECT IF(EXISTS(SELECT * FROM EncounterNpcCast WHERE EncounterId = @id), 1, 0) As EncounterNpcCast;"; 
            }
        }

        public static string SessionPostParseUpdaterNoDate
        {
            get { return "UPDATE Session SET SessionSize = @sessionSize, TotalPlayTime = @totalPlayTime WHERE Id = @sessionId"; }
        }

        public static string SessionPostParseUpdater
        {
            get { return "UPDATE Session SET Date = @date, Duration = @duration, SessionSize = @sessionSize, TotalPlayTime = @totalPlayTime WHERE Id = @sessionId"; }
        }

        public static string SessionLogPostParseUpdater
        {
            get { return "UPDATE SessionLog SET LogSize = @logSize, TotalPlayedTime = @totalPlayedTime, LogLines = @totalLines WHERE Id = @sessionLogId"; }
        }

        public static string GetExistingEncounterIdsForSession
        {
            get { return "SELECT EncounterId FROM SessionEncounter WHERE SessionId = @sessionId ORDER BY EncounterId ASC"; }
        }

        public static string GetFirstEncounterDateForSession
        {
            get { return "SELECT E.Date FROM SessionEncounter SE JOIN Encounter E ON SE.EncounterId = E.Id WHERE SessionId = @sessionId ORDER BY E.Date ASC LIMIT 0,1"; }
        }

        public static string GetLastEncounterDateForSession
        {
            get
            {
                return "SELECT DATE_ADD(E.Date, INTERVAL E.Duration HOUR_SECOND) AS EndDate " +
                       "FROM SessionEncounter SE JOIN Encounter E ON SE.EncounterId = E.Id WHERE SessionId = @sessionId ORDER BY EndDate DESC LIMIT 0,1;";
            }
        }

        public static string GetSessionTotalsFromSessionLogs
        {
            get { return "SELECT COALESCE(SUM(LogSize), 0) AS TotalLogSize, COALESCE(SUM(TotalPlayedTime), 0) AS TotalPlayedTime FROM SessionLog WHERE SessionId = @sessionId"; }
        }

        public static string GetIgnoredEncounters
        {
            get { return "SELECT Name FROM AutoParserIgnoredEncounters ORDER BY Name ASC"; }
        }
    }
}
