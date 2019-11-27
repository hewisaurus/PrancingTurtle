namespace Database.MySQL
{
    public static class Encounter
    {
        #region Comparison

        public static string GetMultiplePlayerDamageForEncounter
        {
            get { return "SELECT * FROM DamageDone D " +
                         //"JOIN Player P1 ON D.SourcePlayerId = P1.Id " +
                         "JOIN Ability A ON D.AbilityId = A.Id " +
                         "WHERE D.EncounterId = @id AND D.SourcePlayerId IN @playerIds " +
                         "AND D.TargetNpcId IS NOT NULL"; }
        }

        public static string GetMultiplePlayerHealingForEncounter
        {
            get
            {
                return "SELECT * FROM HealingDone D " +
                       "JOIN Ability A ON D.AbilityId = A.Id " +
                       "WHERE D.EncounterId = @id AND D.SourcePlayerId IN @playerIds " +
                       "AND (D.TargetPlayerId IS NOT NULL OR D.TargetPetName IS NOT NULL)";
            }
        }

        public static string GetMultiplePlayerShieldingForEncounter
        {
            get
            {
                return "SELECT * FROM ShieldingDone D " +
                       "JOIN Ability A ON D.AbilityId = A.Id " +
                       "WHERE D.EncounterId = @id AND D.SourcePlayerId IN @playerIds " +
                       "AND (D.TargetPlayerId IS NOT NULL OR D.TargetPetName IS NOT NULL)";
            }
        }
        #endregion
        public static string CountGuildEncounters
        {
            get { return "SELECT COUNT(*) FROM Encounter WHERE GuildId = @guildId"; }
        }

        public static string GetEncounterRoleRecords
        {
            get { return "SELECT * FROM EncounterPlayerRole WHERE EncounterId = @id"; }
        }

        public static string SearchEncounterDuration { get { return "@totalSeconds"; } }

        public static string GetEncounterIdsWithNoDuration
        {
            get { return "SELECT Id FROM Encounter WHERE Duration = '00:00:00' AND Removed = 0"; }
        }

        public static string GetTotalSecondsFromDamageDone
        {
            get
            {
                //return "SELECT MAX(SecondsElapsed) AS Seconds FROM DamageDone WHERE EncounterId = @id";
                return "SELECT IF((SELECT COUNT(1) FROM DamageDone WHERE EncounterId = @id) > 0, (SELECT MAX(SecondsElapsed) AS Seconds FROM DamageDone WHERE EncounterId = @id), 0) RESULT;";
            }
        }

        public static string UpdateDurationForEncounter
        {
            get { return "UPDATE Encounter SET Duration = @duration WHERE Id = @id"; }
        }

        public static string GetEncountersWithNoNpcRecords
        {
            get { return "SELECT * FROM Encounter E LEFT JOIN EncounterNpc EN ON EN.EncounterId = E.Id WHERE EN.Id IS NULL AND E.Removed = 0 AND E.ToBeDeleted = 0 LIMIT @limit"; }
        }

        public static string GetEncounterIdsWithNoPlayerRecords
        {
            get { return "SELECT E.Id FROM Encounter E " +
                         "LEFT JOIN EncounterPlayerRole EPR ON E.Id = EPR.EncounterId " +
                         "JOIN BossFight BF ON E.BossFightId = BF.Id " +
                         "WHERE EPR.Id IS NULL AND E.Duration > '00:00:10' AND E.Removed = 0 AND E.ToBeDeleted = 0 AND BF.InstanceId != '14' LIMIT @limit";
            }
        }

        public static string CountBasicRecordsForEncounter
        {
            get
            {
                return "SELECT (SELECT COUNT(1) FROM DamageDone WHERE EncounterId = @encounterId) AS DamageCount, " +
                       "(SELECT COUNT(1) FROM HealingDone WHERE EncounterId = @encounterId) AS HealingCount, " +
                       "(SELECT COUNT(1) FROM ShieldingDone WHERE EncounterId = @encounterId) AS ShieldCount";
            }
        }

        public static string CountEncountersWithNoPlayerStatistics
        {
            get { return "SELECT COUNT(1) FROM Encounter E LEFT JOIN EncounterPlayerStatistics EPS ON E.Id = EPS.EncounterId WHERE EPS.Id IS NULL AND E.Removed = 0 AND E.ToBeDeleted = 0"; }
        }

        public static string GetEncounterIdsWithNoPlayerStatistics
        {
            get { return "SELECT E.Id FROM Encounter E LEFT JOIN EncounterPlayerStatistics EPS ON E.Id = EPS.EncounterId " +
                         "WHERE EPS.Id IS NULL AND E.SuccessfulKill = 1 AND E.ValidForRanking = 1 AND E.Removed = 0 AND E.ToBeDeleted = 0 LIMIT @limit";
            }
        }

        public static string GetEncountersWithNoPlayerStatistics
        {
            get { return "SELECT * FROM Encounter E LEFT JOIN EncounterPlayerStatistics EPS ON E.Id = EPS.EncounterId " +
                         "WHERE EPS.Id IS NULL AND E.SuccessfulKill = 1 AND E.ValidForRanking = 1 AND E.Removed = 0 AND E.ToBeDeleted = 0 LIMIT @limit";
            }
        }

        public static string GetEncountersWithNoSingleTargetDps
        {
            get
            {
                return "SELECT E.*, BFSTD.TargetName FROM Encounter E " +
                       "JOIN EncounterPlayerStatistics EPS ON E.Id = EPS.EncounterId " +
                       "JOIN BossFightSingleTargetDetail BFSTD ON E.BossFightId = BFSTD.BossFightId " +
                       "WHERE EPS.SingleTargetDPS = 0 AND E.Removed = 0 AND E.ToBeDeleted = 0 " +
                       "GROUP BY E.Id LIMIT @limit";
            }
        }

        public static string GetEncountersWithNoPlayerStatisticsButValidSession
        {
            get
            {
                return "SELECT E.* FROM Encounter E " +
                       "LEFT JOIN EncounterPlayerStatistics EPS ON E.Id = EPS.EncounterId " +
                       "LEFT JOIN SessionEncounter SE ON E.Id = SE.EncounterId " +
                       "WHERE EPS.Id IS NULL AND SE.Id IS NOT NULL " +
                       "AND E.SuccessfulKill = 1 AND E.ValidForRanking = 1 AND E.Removed = 0 AND E.ToBeDeleted = 0 LIMIT @limit";
            }
        }

        public static string GetEncountersWithNoPlayerBurstStatisticsButValidSession
        {
            get
            {
                return "SELECT E.* " +
                       "FROM Encounter E " +
                       "JOIN EncounterPlayerStatistics EPS ON E.Id = EPS.EncounterId " +
                       "JOIN SessionEncounter SE ON E.Id = SE.EncounterId " +
                       "WHERE E.SuccessfulKill = 1 AND E.ValidForRanking = 1 " +
                       "AND E.Removed = 0 AND E.ToBeDeleted = 0 " +
                       "GROUP BY EPS.EncounterId HAVING SUM(EPS.BurstDamage1sValue) = 0 LIMIT @limit";
            }
        }

        public static string CountEncounterIdsWithNoPlayerTopHits
        {
            get { return "SELECT COUNT(DISTINCT EncounterId) AS Count FROM EncounterPlayerStatistics EPS JOIN Encounter E ON EPS.EncounterId = E.Id WHERE EPS.TopApsAbilityValue = 0 AND EPS.TopDpsAbilityValue = 0 AND EPS.TopHpsAbilityValue = 0 AND E.Removed = 0 AND E.ToBeDeleted = 0"; }
        }

        public static string GetEncountersWithNoPlayerTopHits
        {
            get
            {
                return "SELECT E.* FROM Encounter E JOIN " +
                       "(SELECT EncounterId FROM EncounterPlayerStatistics EPS " +
                       "JOIN Encounter E ON EPS.EncounterId = E.Id " +
                       "WHERE EPS.TopApsAbilityValue = 0 AND EPS.TopDpsAbilityValue = 0 " +
                       "AND EPS.TopHpsAbilityValue = 0 AND E.Removed = 0 AND E.ToBeDeleted = 0 GROUP BY EncounterId) E2 " +
                       "ON E.Id = E2.EncounterId LIMIT @limit";
            }
        }
        #region Update Encounter Top Hits
        public static string UpdateEncounterWithPlayerTopHitsAllThree
        {
            get
            {
                return "UPDATE EncounterPlayerStatistics " +
                       "SET TopApsAbilityId = @apsAbilityId, TopApsAbilityValue = @topApsAbilityValue, " +
                       "TopHpsAbilityId = @hpsAbilityId, TopHpsAbilityValue = @topHpsAbilityValue, " +
                       "TopDpsAbilityId = @dpsAbilityId, TopDpsAbilityValue = @topDpsAbilityValue " +
                       "WHERE EncounterId = @encounterId AND PlayerId = @playerId";
            }
        }

        public static string UpdateEncounterWithPlayerTopHitsAps
        {
            get
            {
                return "UPDATE EncounterPlayerStatistics " +
                       "SET TopApsAbilityId = @apsAbilityId, TopApsAbilityValue = @topApsAbilityValue " +
                       "WHERE EncounterId = @encounterId AND PlayerId = @playerId";
            }
        }

        public static string UpdateEncounterWithPlayerTopHitsHps
        {
            get
            {
                return "UPDATE EncounterPlayerStatistics " +
                       "SET TopHpsAbilityId = @hpsAbilityId, TopHpsAbilityValue = @topHpsAbilityValue " +
                       "WHERE EncounterId = @encounterId AND PlayerId = @playerId";
            }
        }

        public static string UpdateEncounterWithPlayerTopHitsDps
        {
            get
            {
                return "UPDATE EncounterPlayerStatistics " +
                       "SET TopDpsAbilityId = @dpsAbilityId, TopDpsAbilityValue = @topDpsAbilityValue " +
                       "WHERE EncounterId = @encounterId AND PlayerId = @playerId";
            }
        }

        public static string UpdateEncounterWithPlayerTopHitsApsHps
        {
            get
            {
                return "UPDATE EncounterPlayerStatistics " +
                       "SET TopApsAbilityId = @apsAbilityId, TopApsAbilityValue = @topApsAbilityValue, " +
                       "TopHpsAbilityId = @hpsAbilityId, TopHpsAbilityValue = @topHpsAbilityValue " +
                       "WHERE EncounterId = @encounterId AND PlayerId = @playerId";
            }
        }

        public static string UpdateEncounterWithPlayerTopHitsApsDps
        {
            get
            {
                return "UPDATE EncounterPlayerStatistics " +
                       "SET TopApsAbilityId = @apsAbilityId, TopApsAbilityValue = @topApsAbilityValue, " +
                       "TopDpsAbilityId = @dpsAbilityId, TopDpsAbilityValue = @topDpsAbilityValue " +
                       "WHERE EncounterId = @encounterId AND PlayerId = @playerId";
            }
        }

        public static string UpdateEncounterWithPlayerTopHitsHpsDps
        {
            get
            {
                return "UPDATE EncounterPlayerStatistics " +
                       "SET TopHpsAbilityId = @hpsAbilityId, TopHpsAbilityValue = @topHpsAbilityValue, " +
                       "TopDpsAbilityId = @dpsAbilityId, TopDpsAbilityValue = @topDpsAbilityValue " +
                       "WHERE EncounterId = @encounterId AND PlayerId = @playerId";
            }
        }
        #endregion
        public static string GetEncounterIdsWithNoPlayerTopHits
        {
            get { return "SELECT EPS.EncounterId FROM EncounterPlayerStatistics EPS JOIN Encounter E ON EPS.EncounterId = E.Id WHERE EPS.TopApsAbilityValue = 0 AND EPS.TopDpsAbilityValue = 0 AND EPS.TopHpsAbilityValue = 0 AND E.Removed = 0 AND E.ToBeDeleted = 0 GROUP BY EPS.EncounterId;"; }
        }

        public static string CountDeathsPerPlayer
        {
            get { return "SELECT TargetPlayerId AS PlayerId, COUNT(1) AS Deaths FROM EncounterDeath WHERE EncounterId = @encounterId AND TargetPlayerId IS NOT NULL GROUP BY TargetPlayerId"; }
        }

        public static string GetFastestKills
        {
            get
            {
                return "SELECT E.*, G.*, S.* FROM Encounter E " +
                       "JOIN Guild G ON E.GuildId = G.Id " +
                       "JOIN GuildStatus GS ON G.GuildStatusId = GS.Id " +
                       "JOIN Shard S ON G.ShardId = S.Id " +
                       "WHERE E.BossFightId = @id AND E.SuccessfulKill = 1 AND E.IsPublic = 1 " +
                       "AND E.ValidForRanking = 1 AND G.HideFromRankings = 0 AND E.Removed = 0 AND E.ToBeDeleted = 0 " +
                       "AND GS.Approved = 1 AND E.EncounterDifficultyId = @difficultyId ORDER BY Duration ASC";
            }
        }

        public static string GetFastestKill
        {
            get
            {
                return "SELECT E.*, G.* FROM Encounter E " +
                       "JOIN Guild G ON E.GuildId = G.Id " +
                       "JOIN GuildStatus GS ON G.GuildStatusId = GS.Id " +
                       "WHERE E.BossFightId = @id AND E.SuccessfulKill = 1 " +
                       "AND E.ValidForRanking = 1 AND G.HideFromRankings = 0 AND E.Removed = 0 AND E.ToBeDeleted = 0 " +
                       "AND GS.Approved = 1 ORDER BY Duration ASC LIMIT 1";
                //return "SELECT * FROM Encounter E JOIN Guild G ON E.GuildId = G.Id " +
                //       "JOIN GuildStatus GS ON G.GuildStatusId = GS.Id " +
                //       "WHERE BossFightId = @id AND SuccessfulKill = 1 AND G.HideFromRankings = 0 " +
                //       "AND GS.Approved = 1 ORDER BY Duration ASC LIMIT 1";
            }
        }

        public static string GetDateSortedKills
        {
            get
            {
                return "SELECT E.*, G.* FROM Encounter E " +
                       "JOIN Guild G ON E.GuildId = G.Id " +
                       "JOIN GuildStatus GS ON G.GuildStatusId = GS.Id " +
                       "WHERE E.BossFightId = @id AND E.EncounterDifficultyId = @difficultyId AND SuccessfulKill = 1 " +
                       "AND ValidForRanking = 1 AND G.HideFromRankings = 0 AND E.IsPublic = 1 AND GS.Approved = 1 AND E.Removed = 0 AND E.ToBeDeleted = 0 " +
                       "ORDER BY E.Date, G.Name";
            }
        }

        public static string MarkForDeletion
        {
            get { return "UPDATE Encounter SET ToBeDeleted = 1 WHERE Id IN @ids"; }
        }

        public static string GetMarkedForDeletion => "SELECT * FROM Encounter WHERE ToBeDeleted = 1 AND Removed = 0 ORDER BY Duration DESC";

        public static string GetMarkedForDeletionShortestFirst
        {
            get { return "SELECT * FROM Encounter WHERE ToBeDeleted = 1 AND Removed = 0 ORDER BY Duration ASC"; }
        }

        public static string GetMarkedForDeletionLongestFirst
        {
            get { return "SELECT * FROM Encounter WHERE ToBeDeleted = 1 AND Removed = 0 ORDER BY Duration DESC"; }
        }

        public static string GetAllSuccessfulEncounterIds
        {
            get { return "SELECT Id FROM Encounter WHERE SuccessfulKill = 1 AND ValidForRanking = 1 AND ToBeDeleted = 0 AND Removed = 0 ORDER BY ID Desc LIMIT 100000"; }
        }

        public const string GetAllDamageDoneForEncounter = "SELECT * FROM DamageDone WHERE EncounterId = @id ORDER BY SecondsElapsed, OrderWithinSecond";
        public const string GetAllHealingDoneForEncounter = "SELECT * FROM HealingDone WHERE EncounterId = @id ORDER BY SecondsElapsed, OrderWithinSecond";
        public const string GetAllShieldingDoneForEncounter = "SELECT * FROM ShieldingDone WHERE EncounterId = @id ORDER BY SecondsElapsed, OrderWithinSecond";

        public static class Character
        {
            public static class Player
            {
                public static string GetEventsBeforeDeath
                {
                    get
                    {
                        return "SELECT D.SecondsElapsed, D.OrderWithinSecond, 'Damage' AS EventType, " +
                               "IF(D.SourceNpcId IS NULL, P2.Name, D.SourceNpcName) AS Source, " +
                               "P.Name AS Target, A.Name AS Ability, D.TotalDamage AS Total, " +
                               "0 AS Overheal, D.InterceptedAmount AS Intercepted, D.AbsorbedAmount AS Absorbed, D.OverkillAmount AS Overkill " +
                               "FROM DamageDone D JOIN Player P ON D.TargetPlayerId = P.Id " +
                               "JOIN Ability A ON D.AbilityId = A.Id LEFT JOIN Player P2 ON D.SourcePlayerId = P2.Id " +
                               "WHERE D.EncounterId = @encounterId AND D.TargetPlayerId = @targetPlayerId " +
                               "AND D.SecondsElapsed <= @maxSeconds AND D.SecondsElapsed >= @minSeconds " +
                               "UNION " +
                               "SELECT D.SecondsElapsed, D.OrderWithinSecond, 'Heal' AS EventType, " +
                               "IF(D.SourceNpcId IS NULL, P2.Name, D.SourceNpcName) AS Source, " +
                               "P.Name AS Target, A.Name AS Ability, D.TotalHealing AS Total, " +
                               "D.OverhealAmount AS Overheal, 0 AS Intercepted, 0 AS Absorbed, 0 AS Overkill " +
                               "FROM HealingDone D JOIN Player P ON D.TargetPlayerId = P.Id " +
                               "JOIN Ability A ON D.AbilityId = A.Id LEFT JOIN Player P2 ON D.SourcePlayerId = P2.Id " +
                               "WHERE D.EncounterId = @encounterId AND D.TargetPlayerId = @targetPlayerId " +
                               "AND D.SecondsElapsed <= @maxSeconds AND D.SecondsElapsed >= @minSeconds " +
                               "UNION " +
                               "SELECT D.SecondsElapsed, D.OrderWithinSecond, 'Absorb' AS EventType, " +
                               "IF(D.SourceNpcId IS NULL, P2.Name, D.SourceNpcName) AS Source, " +
                               "P.Name AS Target, A.Name AS Ability, D.ShieldValue AS Total, 0 AS Overheal, " +
                               "0 AS Intercepted, 0 AS Absorbed, 0 AS Overkill FROM ShieldingDone D JOIN Player P ON D.TargetPlayerId = P.Id " +
                               "JOIN Ability A ON D.AbilityId = A.Id LEFT JOIN Player P2 ON D.SourcePlayerId = P2.Id " +
                               "WHERE D.EncounterId = @encounterId AND D.TargetPlayerId = @targetPlayerId " +
                               "AND D.SecondsElapsed <= @maxSeconds AND D.SecondsElapsed >= @minSeconds " +
                               "ORDER BY SecondsElapsed, OrderWithinSecond ASC";
                    }
                }

                public static string AllPlayerDeathsForEncounter
                {
                    get { return "SELECT SecondsElapsed FROM EncounterDeath WHERE EncounterId = @id AND TargetPlayerId IS NOT NULL ORDER BY SecondsElapsed"; }
                }

                public static string AllDeathsForPlayerForEncounter
                {
                    get { return "SELECT SecondsElapsed FROM EncounterDeath WHERE EncounterId = @id AND TargetPlayerId = @playerId ORDER BY SecondsElapsed"; }
                }

                public static string PlayerIsOracle
                {
                    get { return "SELECT IF(EXISTS(SELECT COUNT(1) " +
                                 "FROM DamageDone DD JOIN Ability A ON DD.AbilityId = A.Id " +
                                 "WHERE EncounterId = @encounterId AND SourcePlayerId = @playerId " +
                                 "AND A.Name IN('Emblem of Ice','Emblem of Pain','Insignia of Blood') " +
                                 "GROUP BY DD.AbilityId), 1, 0) AS IsOracle"; }
                }

                public static string CheckAllSupportFlags
                {
                    get
                    {
                        return "SELECT X.EncounterId, X.PlayerId, X.Name AS PlayerName, IF(SUM(CASE WHEN AR.RoleIconId = 3 THEN 1 ELSE 0 END) > 0, 1, 0) AS IsSupport " +
                               "FROM (SELECT * FROM EncounterPlayerRole WHERE EncounterId = @encounterId AND Role = 'Support') X " +
                               "JOIN DamageDone DD ON X.EncounterId = DD.EncounterId AND X.PlayerId = DD.SourcePlayerId " +
                               "JOIN Ability A ON DD.AbilityId = A.Id " +
                               "JOIN AbilityRole AR ON A.AbilityId = AR.AbilityLogId " +
                               "GROUP BY X.EncounterId, X.PlayerId";
                    }
                }

                public static string CheckSupportFlagForPlayer
                {
                    get
                    {
                        return "SELECT IF((SELECT COUNT(1) " +
                               "FROM DamageDone DD " +
                               "JOIN Ability A ON DD.AbilityId = A.Id " +
                               "JOIN AbilityRole AR ON A.AbilityId = AR.AbilityLogId " +
                               "WHERE DD.EncounterId = @encounterId AND DD.SourcePlayerId = @playerId AND AR.RoleIconId = 3) > 0, 1, 0) AS IsSupport";
                    }
                }

                public static class Damage
                {
                    public static string RecordTable { get { return "DamageDone"; } }
                    public static string RecordTableAlias { get { return "DD"; } }
                    public static string SearchCharacter { get { return "Player"; } }
                    public static string SearchEncounterId { get { return "@id"; } }
                    public static string SearchCharacterId { get { return "@playerId"; } }
                    public static string SearchField { get { return "TotalDamage"; } }

                    public static string InteractionBaseQueryPerSecond
                    {
                        get
                        {
                            return string.Format("(SUM({0}.EffectiveDamage) + SUM({0}.AbsorbedAmount) + SUM({0}.BlockedAmount) + " +
                                   "SUM({0}.InterceptedAmount) + SUM({0}.IgnoredAmount)) AS Total, " +
                                   "SUM({0}.EffectiveDamage) AS Effective ", RecordTableAlias);
                        }
                    }
                    public static string InteractionBaseQueryTotals
                    {
                        get
                        {
                            return string.Format("COUNT(CASE WHEN {0}.CriticalHit = 1 THEN 1 END) AS Crits, " +
                                    "count(CASE WHEN {0}.CriticalHit = 0 THEN 1 END) AS Hits, " +
                                    "MAX({0}.TotalDamage) AS BiggestHit, AVG({0}.TotalDamage) AS AverageHit, " +
                                    "(SUM({0}.EffectiveDamage) + SUM({0}.AbsorbedAmount) + SUM({0}.BlockedAmount) " +
                                    "+ SUM({0}.InterceptedAmount) + SUM({0}.IgnoredAmount)) AS Total, " +
                                    "SUM({0}.BlockedAmount) AS Blocked, " +
                                    "SUM({0}.IgnoredAmount) AS Ignored, " +
                                    "SUM({0}.InterceptedAmount) AS Intercepted, " +
                                    "SUM({0}.AbsorbedAmount) AS Absorbed, " +
                                    "SUM({0}.EffectiveDamage) AS Effective, " +
                                    "((SUM(EffectiveDamage) + SUM(AbsorbedAmount) + SUM(BlockedAmount) " +
                                    "+ SUM(InterceptedAmount) + SUM(IgnoredAmount)) / {1}) AS Average, " +
                                    "(SUM(EffectiveDamage) / {1}) AS AverageEffective, " +
                                    "((100.0 * (SUM({0}.EffectiveDamage) + SUM({0}.AbsorbedAmount)) /  " +
                                    "(SELECT (SUM(EffectiveDamage) + SUM(AbsorbedAmount)) ",
                                    RecordTableAlias, SearchEncounterDuration);
                        }
                    }
                    public static string InteractionBaseQueryTotalsNoPercentage
                    {
                        get
                        {
                            return string.Format("COUNT(CASE WHEN {0}.CriticalHit = 1 THEN 1 END) AS Crits, " +
                                    "count(CASE WHEN {0}.CriticalHit = 0 THEN 1 END) AS Hits, " +
                                    "MAX({0}.TotalDamage) AS BiggestHit, AVG({0}.TotalDamage) AS AverageHit, " +
                                    "(SUM({0}.EffectiveDamage) + SUM({0}.AbsorbedAmount) + SUM({0}.BlockedAmount) " +
                                    "+ SUM({0}.InterceptedAmount) + SUM({0}.IgnoredAmount)) AS Total, " +
                                    "SUM({0}.BlockedAmount) AS Blocked, " +
                                    "SUM({0}.IgnoredAmount) AS Ignored, " +
                                    "SUM({0}.InterceptedAmount) AS Intercepted, " +
                                    "SUM({0}.AbsorbedAmount) AS Absorbed, " +
                                    "SUM({0}.EffectiveDamage) AS Effective, " +
                                    "((SUM(EffectiveDamage) + SUM(AbsorbedAmount) + SUM(BlockedAmount) " +
                                    "+ SUM(InterceptedAmount) + SUM(IgnoredAmount)) / {1}) AS Average, " +
                                    "(SUM(EffectiveDamage) / {1}) AS AverageEffective ",
                                    RecordTableAlias, SearchEncounterDuration);
                        }
                    }
                    public static string InteractionJoinSourcePlayer
                    {
                        get
                        {
                            return string.Format("FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                   "JOIN Encounter E ON {1}.EncounterId = E.Id JOIN Player P ON {1}.SourcePlayerId = P.Id ", RecordTable, RecordTableAlias);
                        }
                    }
                    public static string InteractionJoinTargetPlayer
                    {
                        get
                        {
                            return string.Format("FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                   "JOIN Encounter E ON {1}.EncounterId = E.Id JOIN Player P ON {1}.TargetPlayerId = P.Id ", RecordTable, RecordTableAlias);
                        }
                    }
                    public static string InteractionJoinNoPlayer
                    {
                        get
                        {
                            return string.Format("FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                   "JOIN Encounter E ON {1}.EncounterId = E.Id ", RecordTable, RecordTableAlias);
                        }
                    }
                    public static string BaseWhereIncoming
                    {
                        get
                        {
                            return string.Format("WHERE {0}.EncounterId = {1} AND {0}.Target{2}Id = {3} AND {4} > 0 ",
                                RecordTableAlias, SearchEncounterId, SearchCharacter, SearchCharacterId, SearchField);
                        }
                    }
                    public static string BaseWhereOutgoing
                    {
                        get
                        {
                            return string.Format("WHERE {0}.EncounterId = {1} AND {0}.Source{2}Id = {3} AND {4} > 0 ",
                                   RecordTableAlias, SearchEncounterId, SearchCharacter, SearchCharacterId, SearchField);
                        }
                    }

                    public static class Done
                    {
                        public static class ByAbility
                        {
                            public static class OnlyNpcs
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {0}.SecondsElapsed, A.Name AS AbilityName, " +
                                                             InteractionBaseQueryPerSecond +
                                                             "FROM {1} {0} JOIN Ability A ON {0}.AbilityId = A.Id " +
                                                             BaseWhereOutgoing + "AND {0}.TargetNpcId IS NOT NULL " +
                                                             "GROUP BY {0}.SecondsElapsed, A.Name " +
                                                             "ORDER BY {0}.SecondsElapsed, A.Name",
                                                             RecordTableAlias, RecordTable);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT A.Name AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
                                                             "FROM {0} WHERE EncounterId = {2} AND SourcePlayerId = {3} AND TargetNpcId IS NOT NULL))) AS Percentage " +
                                                             InteractionJoinNoPlayer + BaseWhereOutgoing + " AND {1}.TargetNpcId IS NOT NULL " +
                                                             "GROUP BY A.Name, A.DamageType, A.Icon, E.Duration ORDER BY Total DESC",
                                                             RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            public static class OtherPlayers
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, A.Name AS AbilityName, " +
                                            InteractionBaseQueryPerSecond +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.TargetPlayerId = P.Id " +
                                                 BaseWhereOutgoing + "AND {1}.TargetPlayerId <> {2} GROUP BY {1}.SecondsElapsed, A.Name " +
                                                 "ORDER BY {1}.SecondsElapsed, A.Name", RecordTable, RecordTableAlias, SearchCharacterId);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT A.Name AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
                                                "FROM {0} WHERE EncounterId = {1} AND SourcePlayerId = {2} AND TargetPlayerId <> {2}))) AS Percentage " +
                                                InteractionJoinTargetPlayer + BaseWhereOutgoing + " AND {3}.TargetPlayerId <> {2} " +
                                               "GROUP BY A.Name, A.DamageType, A.Icon, E.Duration ORDER BY Total DESC",
                                               RecordTable, SearchEncounterId, SearchCharacterId, RecordTableAlias);
                                    }
                                }
                            }

                            public static class AllPlayers
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, A.Name AS AbilityName, " +
                                                InteractionBaseQueryPerSecond +
                                                "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.TargetPlayerId = P.Id " +
                                                BaseWhereOutgoing + "AND {1}.TargetPlayerId IS NOT NULL GROUP BY {1}.SecondsElapsed, A.Name " +
                                                "ORDER BY {1}.SecondsElapsed, AbilityName",
                                                RecordTable, RecordTableAlias);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT A.Name AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {1} AND SourcePlayerId = {2} AND TargetPlayerId IS NOT NULL))) AS Percentage " +
                                               InteractionJoinTargetPlayer + BaseWhereOutgoing + " AND {3}.TargetPlayerId IS NOT NULL " +
                                               "GROUP BY A.Name, A.DamageType, A.Icon, E.Duration ORDER BY Total DESC",
                                               RecordTable, SearchEncounterId, SearchCharacterId, RecordTableAlias);
                                    }
                                }
                            }

                            public static class OnlySelf
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, A.Name AS AbilityName, " +
                                            InteractionBaseQueryPerSecond +
                                            "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                            BaseWhereOutgoing + "AND {1}.TargetPlayerId = {2} GROUP BY {1}.SecondsElapsed, A.Name " +
                                            "ORDER BY {1}.SecondsElapsed, AbilityName",
                                            RecordTable, RecordTableAlias, SearchCharacterId);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT A.Name AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
                                                "FROM {0} WHERE EncounterId = {1} AND TargetPlayerId = {2} AND SourcePlayerId = {2}))) AS Percentage " +
                                                InteractionJoinSourcePlayer + BaseWhereOutgoing + " AND {3}.TargetPlayerId = {2} " +
                                               "GROUP BY A.Name, A.DamageType, A.Icon, E.Duration ORDER BY Total DESC",
                                               RecordTable, SearchEncounterId, SearchCharacterId, RecordTableAlias);
                                    }
                                }
                            }

                            public static class AllTargets
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format(
                                            "SELECT X.SecondsElapsed, X.AbilityName, " +
                                            "SUM(X.Total) AS Total, SUM(X.Effective) AS Effective " +
                                            "FROM " +
                                            "(SELECT {1}.SecondsElapsed, A.Name AS AbilityName, " +
                                            InteractionBaseQueryPerSecond +
                                            "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                            BaseWhereOutgoing + "AND {1}.TargetNpcId IS NOT NULL " +
                                            "GROUP BY {1}.SecondsElapsed, A.Name " +
                                            "UNION ALL " +
                                            "SELECT {1}.SecondsElapsed, A.Name AS AbilityName, " +
                                            InteractionBaseQueryPerSecond +
                                            "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                            "JOIN Player P ON {1}.TargetPlayerId = P.Id " +
                                            BaseWhereOutgoing + "AND {1}.TargetPlayerId IS NOT NULL " +
                                            "GROUP BY {1}.SecondsElapsed, A.Name) AS X " +
                                            "GROUP BY SecondsElapsed, AbilityName " +
                                            "ORDER BY SecondsElapsed, AbilityName",
                                            RecordTable, RecordTableAlias);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format(
                                              "SELECT X.AbilityName, X.DamageType, X.Icon, " +
                                              "SUM(X.Crits) AS Crits, SUM(X.Hits) AS Hits, MAX(X.BiggestHit) AS BiggestHit, " +
                                              "(SUM(X.Total) / (SUM(X.Crits) + SUM(X.Hits))) AS AverageHit, SUM(X.Total) AS Total, " +
                                              "SUM(X.Blocked) AS Blocked, SUM(X.Ignored) AS Ignored, SUM(X.Intercepted) AS Intercepted, SUM(X.Absorbed) AS Absorbed, " +
                                              "SUM(X.Effective) AS Effective, " +
                                              "CAST((SUM(X.Total) / {4}) AS unsigned integer) AS Average, " +
                                              "CAST((SUM(X.Effective) / {4}) AS unsigned integer) AS AverageEffective, " +
                                              "((100.0 * (SUM(X.Effective) + SUM(X.Absorbed)) / (SELECT (SUM(EffectiveDamage) + SUM(AbsorbedAmount)) " +
                                              "FROM {0} WHERE EncounterId = {2} AND SourcePlayerId = {3} AND TargetNpcId IS NOT NULL))) AS Percentage " +
                                              "FROM ( " +
                                              "SELECT E.Duration, A.Name AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotalsNoPercentage +
                                                 InteractionJoinNoPlayer + BaseWhereOutgoing + " AND {1}.TargetNpcId IS NOT NULL " +
                                                 "GROUP BY E.Duration, A.Name, A.DamageType, A.Icon " +
                                                 "UNION " +
                                                 "SELECT E.Duration, A.Name AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotalsNoPercentage +
                                                 InteractionJoinTargetPlayer + BaseWhereOutgoing + " AND {1}.TargetPlayerId IS NOT NULL " +
                                                 "GROUP BY E.Duration, A.Name, A.DamageType, A.Icon" +
                                                 ") AS X " +
                                                 "GROUP BY Duration, AbilityName, DamageType, Icon " +
                                                 "ORDER BY Total DESC",
                                                 RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId, SearchEncounterDuration);
                                    }
                                }
                            }
                        }

                        public static class ByTarget
                        {
                            public static class OnlyNpcs
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, {1}.TargetNpcName AS TargetName, {1}.TargetNpcId AS TargetId, " +
                                            "(SUM({1}.EffectiveDamage) + SUM({1}.AbsorbedAmount) + SUM({1}.BlockedAmount) + " +
                                            "SUM({1}.InterceptedAmount) + SUM({1}.IgnoredAmount)) AS Total, " +
                                            "SUM({1}.EffectiveDamage) AS Effective " +
                                            "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                            "WHERE {1}.EncounterId = {2} AND {1}.SourcePlayerId = {3} AND {1}.{4} > 0 AND {1}.TargetNpcId IS NOT NULL " +
                                            "GROUP BY {1}.SecondsElapsed, {1}.TargetNpcName, {1}.TargetNpcId " +
                                            "ORDER BY {1}.SecondsElapsed, {1}.TargetNpcName, {1}.TargetNpcId",
                                            RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId, SearchField);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.TargetNpcName AS TargetName," + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND SourcePlayerId = {3} AND TargetNpcId IS NOT NULL))) AS Percentage " +
                                               InteractionJoinNoPlayer + BaseWhereOutgoing + " AND {1}.TargetNpcId IS NOT NULL " +
                                               "GROUP BY {1}.TargetNpcName, {1}.TargetNpcId, E.Duration ORDER BY Total DESC",
                                               RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            public static class OtherPlayers
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, P.Name AS TargetName, P.Id AS TargetId, " +
                                                 "(SUM({1}.EffectiveDamage) + SUM({1}.AbsorbedAmount) + SUM({1}.BlockedAmount) + " +
                                                 "SUM({1}.InterceptedAmount) + SUM({1}.IgnoredAmount)) AS Total, " +
                                                 "SUM({1}.EffectiveDamage) AS Effective " +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.TargetPlayerId = P.Id " +
                                                 "WHERE {1}.EncounterId = {2} AND {1}.SourcePlayerId = {3} AND {1}.{4} > 0 AND {1}.TargetPlayerId <> {3} " +
                                                 "GROUP BY {1}.SecondsElapsed, P.Name, P.Id " +
                                                 "ORDER BY {1}.SecondsElapsed, P.Name",
                                                 RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId, SearchField);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT P.Name AS TargetName," + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND SourcePlayerId = {3} AND TargetPlayerId <> {3}))) AS Percentage " +
                                               InteractionJoinTargetPlayer + BaseWhereOutgoing + " AND {1}.TargetPlayerId <> {3} " +
                                               "GROUP BY P.Name, P.Id, E.Duration ORDER BY Effective DESC",
                                               RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            public static class AllPlayers
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, P.Name AS TargetName, P.Id AS TargetId, " +
                                                 "(SUM({1}.EffectiveDamage) + SUM({1}.AbsorbedAmount) + SUM({1}.BlockedAmount) + " +
                                                 "SUM({1}.InterceptedAmount) + SUM({1}.IgnoredAmount)) AS Total, " +
                                                 "SUM({1}.EffectiveDamage) AS Effective " +
                                                 InteractionJoinTargetPlayer +
                                                 "WHERE {1}.EncounterId = {2} AND {1}.SourcePlayerId = {3} AND {1}.{4} > 0 AND {1}.TargetPlayerId IS NOT NULL " +
                                                 "GROUP BY {1}.SecondsElapsed, P.Name, P.Id " +
                                                 "ORDER BY {1}.SecondsElapsed, P.Name",
                                                 RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId, SearchField);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT P.Name AS TargetName," + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND SourcePlayerId = {3} AND TargetPlayerId IS NOT NULL))) AS Percentage " +
                                               InteractionJoinTargetPlayer + BaseWhereOutgoing + " AND {1}.TargetPlayerId IS NOT NULL " +
                                               "GROUP BY P.Name, P.Id, E.Duration ORDER BY Total DESC",
                                               RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            public static class AllTargets
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, {1}.TargetNpcName AS TargetName, {1}.TargetNpcId AS TargetId, " +
                                                 "(SUM({1}.EffectiveDamage) + SUM({1}.AbsorbedAmount) + SUM({1}.BlockedAmount) + SUM({1}.InterceptedAmount) + SUM({1}.IgnoredAmount)) AS Total, " +
                                                 "SUM({1}.EffectiveDamage) AS Effective " +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                                 "WHERE {1}.EncounterId = {2} AND {1}.SourcePlayerId = {3} AND {1}.{4} > 0 AND {1}.TargetNpcId IS NOT NULL " +
                                                 "GROUP BY {1}.SecondsElapsed, {1}.TargetNpcName, {1}.TargetNpcId " +
                                                 "UNION " +
                                                 "SELECT {1}.SecondsElapsed, P.Name AS TargetName, CAST(P.Id AS CHAR(50)) AS TargetId, " +
                                                 "(SUM({1}.EffectiveDamage) + SUM({1}.AbsorbedAmount) + SUM({1}.BlockedAmount) + SUM({1}.InterceptedAmount) + SUM({1}.IgnoredAmount)) AS Total, " +
                                                 "SUM({1}.EffectiveDamage) AS Effective " +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.TargetPlayerId = P.Id " +
                                                 "WHERE {1}.EncounterId = {2} AND {1}.SourcePlayerId = {3} AND {1}.{4} > 0 AND {1}.TargetPlayerId IS NOT NULL " +
                                                 "GROUP BY {1}.SecondsElapsed, P.Name, P.Id " +
                                                 "ORDER BY SecondsElapsed, TargetName",
                                                 RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId, SearchField);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.TargetNpcName AS TargetName," + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND SourcePlayerId = {3}))) AS Percentage " +
                                               InteractionJoinNoPlayer + BaseWhereOutgoing + " AND {1}.TargetNpcId IS NOT NULL " +
                                               "GROUP BY {1}.TargetNpcName, {1}.TargetNpcId, E.Duration " +
                                               "UNION " +
                                               "SELECT P.Name AS TargetName," + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND SourcePlayerId = {3}))) AS Percentage " +
                                               InteractionJoinTargetPlayer + BaseWhereOutgoing + " AND {1}.TargetPlayerId IS NOT NULL " +
                                               "GROUP BY P.Name, P.Id, E.Duration ORDER BY Total DESC",
                                               RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }
                        }
                    }

                    public static class Taken
                    {
                        public static class ByAbility
                        {
                            public static class OnlyNpcs
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {0}.SecondsElapsed, CONCAT(A.Name, ' (', {0}.SourceNpcName, ')') AS AbilityName, " +
                                                             InteractionBaseQueryPerSecond +
                                                             "FROM {1} {0} JOIN Ability A ON {0}.AbilityId = A.Id " +
                                                             BaseWhereIncoming + "AND {0}.SourceNpcId IS NOT NULL " +
                                                             "GROUP BY {0}.SecondsElapsed, A.Name, {0}.SourceNpcName, {0}.SourceNpcId " +
                                                             "ORDER BY {0}.SecondsElapsed, A.Name, {0}.SourceNpcName, {0}.SourceNpcId",
                                                             RecordTableAlias, RecordTable);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT CONCAT(A.Name, ' (', {1}.SourceNpcName, ')') AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND TargetPlayerId = {3} AND SourceNpcId IS NOT NULL))) AS Percentage " +
                                               InteractionJoinNoPlayer + BaseWhereIncoming + " AND {1}.SourceNpcId IS NOT NULL " +
                                               "GROUP BY {1}.SourceNpcName, {1}.SourceNpcId, A.Name, A.DamageType, A.Icon, E.Duration ORDER BY Effective DESC",
                                               RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            public static class OtherPlayers
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, CONCAT(A.Name, ' (', P.Name, ')') AS AbilityName, " +
                                            InteractionBaseQueryPerSecond +
                                            "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.SourcePlayerId = P.Id " +
                                            BaseWhereIncoming + "AND {1}.SourcePlayerId <> {2} " +
                                            "GROUP BY {1}.SecondsElapsed, A.Name, P.Name, P.Id " +
                                            "ORDER BY {1}.SecondsElapsed, A.Name, P.Name, P.Id",
                                            RecordTable, RecordTableAlias, SearchCharacterId);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT CONCAT(A.Name, ' (', P.Name, ')') AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
                                                "FROM {0} WHERE EncounterId = {2} AND TargetPlayerId = {3} AND SourcePlayerId <> {3}))) AS Percentage " +
                                                InteractionJoinSourcePlayer + BaseWhereIncoming + " AND {1}.SourcePlayerId <> {3} " +
                                                "GROUP BY P.Name, P.Id, A.Name, A.DamageType, A.Icon, E.Duration ORDER BY Effective DESC",
                                                RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            public static class AllPlayers
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, CONCAT(A.Name, ' (', P.Name, ')') AS AbilityName, " +
                                            InteractionBaseQueryPerSecond +
                                            "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.SourcePlayerId = P.Id " +
                                            BaseWhereIncoming + "AND {1}.SourcePlayerId IS NOT NULL " +
                                            "GROUP BY {1}.SecondsElapsed, A.Name, P.Name, P.Id " +
                                            "ORDER BY {1}.SecondsElapsed, A.Name, P.Name, P.Id",
                                            RecordTable, RecordTableAlias);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {

                                        return string.Format("SELECT CONCAT(A.Name, ' (', P.Name, ')') AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
                                                "FROM {0} WHERE EncounterId = {2} AND TargetPlayerId = {3} AND SourcePlayerId IS NOT NULL))) AS Percentage " +
                                                InteractionJoinSourcePlayer + BaseWhereIncoming + " AND {1}.SourcePlayerId IS NOT NULL " +
                                                "GROUP BY P.Name, P.Id, A.Name, A.DamageType, A.Icon, E.Duration ORDER BY Effective DESC",
                                                RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            public static class OnlySelf
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, A.Name AS AbilityName, " +
                                                 InteractionBaseQueryPerSecond +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                                 BaseWhereIncoming + "AND {1}.SourcePlayerId = {2} GROUP BY {1}.SecondsElapsed, A.Name " +
                                                 "ORDER BY {1}.SecondsElapsed, AbilityName",
                                                 RecordTable, RecordTableAlias, SearchCharacterId);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT A.Name AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
                                                "FROM {0} WHERE EncounterId = {2} AND TargetPlayerId = {3} AND SourcePlayerId = {3}))) AS Percentage " +
                                                InteractionJoinSourcePlayer + BaseWhereIncoming + " AND {1}.SourcePlayerId = {3} " +
                                                "GROUP BY A.Name, A.DamageType, A.Icon, E.Duration ORDER BY Effective DESC",
                                                RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            public static class AllSources
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, CONCAT(A.Name, ' (', {1}.SourceNpcName, ')') AS AbilityName, " +
                                                InteractionBaseQueryPerSecond +
                                                "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                                BaseWhereIncoming + "AND {1}.SourceNpcId IS NOT NULL " +
                                                "GROUP BY {1}.SecondsElapsed, A.Name, {1}.SourceNpcName, {1}.SourceNpcId " +
                                                "UNION " +
                                                "SELECT {1}.SecondsElapsed, CONCAT(A.Name, ' (', P.Name, ')') AS AbilityName, " +
                                                InteractionBaseQueryPerSecond +
                                                "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.SourcePlayerId = P.Id " +
                                                BaseWhereIncoming + "AND {1}.SourcePlayerId IS NOT NULL " +
                                                "GROUP BY {1}.SecondsElapsed, A.Name, P.Name, P.Id " +
                                                "ORDER BY SecondsElapsed, AbilityName",
                                                RecordTable, RecordTableAlias);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT CONCAT(A.Name, ' (', {1}.SourceNpcName, ')') AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND TargetPlayerId = {3} AND SourceNpcId IS NOT NULL))) AS Percentage " +
                                               InteractionJoinNoPlayer + BaseWhereIncoming + " AND {1}.SourceNpcId IS NOT NULL " +
                                               "GROUP BY {1}.SourceNpcName, {1}.SourceNpcId, A.Name, A.DamageType, A.Icon, E.Duration " +
                                               "UNION " +
                                               "SELECT CONCAT(A.Name, ' (', P.Name, ')') AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND TargetPlayerId = {3} AND SourcePlayerId IS NOT NULL))) AS Percentage " +
                                               InteractionJoinSourcePlayer + BaseWhereIncoming + " AND {1}.SourcePlayerId IS NOT NULL " +
                                               "GROUP BY P.Name, P.Id, A.Name, A.DamageType, A.Icon, E.Duration ORDER BY Effective DESC",
                                                RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }
                        }

                        public static class BySource
                        {
                            public static class OnlyNpcs
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, {1}.SourceNpcName AS SourceName, {1}.SourceNpcId AS SourceId, " +
                                                 "(SUM({1}.EffectiveDamage) + SUM({1}.AbsorbedAmount) + SUM({1}.BlockedAmount) + " +
                                                 "SUM({1}.InterceptedAmount) + SUM({1}.IgnoredAmount)) AS Total, " +
                                                 "SUM({1}.EffectiveDamage) AS Effective " +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                                 "WHERE {1}.EncounterId = {2} AND {1}.TargetPlayerId = {3} AND {1}.{4} > 0 AND {1}.SourceNpcId IS NOT NULL " +
                                                 "GROUP BY {1}.SecondsElapsed, {1}.SourceNpcName, {1}.SourceNpcId " +
                                                 "ORDER BY {1}.SecondsElapsed, {1}.SourceNpcName",
                                                RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId, SearchField);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SourceNpcName AS SourceName," + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND TargetPlayerId = {3} AND SourceNpcId IS NOT NULL))) AS Percentage " +
                                               InteractionJoinNoPlayer + BaseWhereIncoming + " AND {1}.SourceNpcId IS NOT NULL " +
                                               "GROUP BY {1}.SourceNpcName, {1}.SourceNpcId, E.Duration ORDER BY Effective DESC",
                                                RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            public static class OtherPlayers
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, P.Name AS SourceName, P.Id AS SourceId, " +
                                                 "(SUM({1}.EffectiveDamage) + SUM({1}.AbsorbedAmount) + SUM({1}.BlockedAmount) + " +
                                                 "SUM({1}.InterceptedAmount) + SUM({1}.IgnoredAmount)) AS Total, " +
                                                 "SUM({1}.EffectiveDamage) AS Effective " +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.SourcePlayerId = P.Id " +
                                                 "WHERE {1}.EncounterId = {2} AND {1}.TargetPlayerId = {3} AND {1}.{4} > 0 AND {1}.SourcePlayerId <> {3} " +
                                                 "GROUP BY {1}.SecondsElapsed, P.Name, P.Id " +
                                                 "ORDER BY {1}.SecondsElapsed, SourceName",
                                                RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId, SearchField);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT P.Name AS SourceName," + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND TargetPlayerId = {3} AND SourcePlayerId <> {3}))) AS Percentage " +
                                               InteractionJoinSourcePlayer + BaseWhereIncoming + " AND {1}.SourcePlayerId <> {3} " +
                                               "GROUP BY P.Name, P.Id, E.Duration ORDER BY Effective DESC",
                                                RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            public static class AllPlayers
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, P.Name AS SourceName, P.Id AS SourceId, " +
                                                 "(SUM({1}.EffectiveDamage) + SUM({1}.AbsorbedAmount) + SUM({1}.BlockedAmount) + " +
                                                 "SUM({1}.InterceptedAmount) + SUM({1}.IgnoredAmount)) AS Total, " +
                                                 "SUM({1}.EffectiveDamage) AS Effective " +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.SourcePlayerId = P.Id " +
                                                 "WHERE {1}.EncounterId = {2} AND {1}.TargetPlayerId = {3} AND {1}.{4} > 0 AND {1}.SourcePlayerId IS NOT NULL " +
                                                 "GROUP BY {1}.SecondsElapsed, P.Name, P.Id " +
                                                 "ORDER BY {1}.SecondsElapsed, SourceName",
                                                RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId, SearchField);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT P.Name AS SourceName," + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND TargetPlayerId = {3} AND SourcePlayerId IS NOT NULL))) AS Percentage " +
                                               InteractionJoinSourcePlayer + BaseWhereIncoming + " AND {1}.SourcePlayerId IS NOT NULL " +
                                               "GROUP BY P.Name, P.Id, E.Duration ORDER BY Effective DESC",
                                                RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            public static class AllSources
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, {1}.SourceNpcName AS SourceName, {1}.SourceNpcId AS SourceId, " +
                                                 "(SUM({1}.EffectiveDamage) + SUM({1}.AbsorbedAmount) + SUM({1}.BlockedAmount) + " +
                                                 "SUM({1}.InterceptedAmount) + SUM({1}.IgnoredAmount)) AS Total, " +
                                                 "SUM({1}.EffectiveDamage) AS Effective " +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                                 "WHERE {1}.EncounterId = {2} AND {1}.TargetPlayerId = {3} AND {1}.{4} > 0 AND {1}.SourceNpcId IS NOT NULL " +
                                                 "GROUP BY {1}.SecondsElapsed, {1}.SourceNpcName, {1}.SourceNpcId " +
                                                 "UNION " +
                                                 "SELECT {1}.SecondsElapsed, P.Name AS SourceName, CAST(P.Id AS CHAR(50)) AS SourceId, " +
                                                 "(SUM({1}.EffectiveDamage) + SUM({1}.AbsorbedAmount) + SUM({1}.BlockedAmount) + " +
                                                 "SUM({1}.InterceptedAmount) + SUM({1}.IgnoredAmount)) AS Total, " +
                                                 "SUM({1}.EffectiveDamage) AS Effective " +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.SourcePlayerId = P.Id " +
                                                 "WHERE {1}.EncounterId = {2} AND {1}.TargetPlayerId = {3} AND {1}.{4} > 0 AND {1}.SourcePlayerId IS NOT NULL " +
                                                 "GROUP BY {1}.SecondsElapsed, P.Name, P.Id " +
                                                 "ORDER BY SecondsElapsed, SourceName",
                                                RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId, SearchField);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SourceNpcName AS SourceName," + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND TargetPlayerId = {3}))) AS Percentage " +
                                               InteractionJoinNoPlayer + BaseWhereIncoming + " AND {1}.SourceNpcId IS NOT NULL " +
                                               "GROUP BY {1}.SourceNpcName, {1}.SourceNpcId, E.Duration " +
                                               "UNION " +
                                               "SELECT P.Name AS SourceName," + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND TargetPlayerId = {3}))) AS Percentage " +
                                               InteractionJoinSourcePlayer + BaseWhereIncoming + " AND {1}.SourcePlayerId IS NOT NULL " +
                                               "GROUP BY P.Name, P.Id, E.Duration ORDER BY Effective DESC",
                                                RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }
                        }
                    }
                }

                public static class Healing
                {
                    public static string RecordTable { get { return "HealingDone"; } }
                    public static string RecordTableAlias { get { return "HD"; } }
                    public static string SearchCharacter { get { return "Player"; } }
                    public static string SearchEncounterId { get { return "@Id"; } }
                    public static string SearchCharacterId { get { return "@playerId"; } }
                    public static string SearchField { get { return "TotalHealing"; } }
                    #region 'Base' queries
                    public static string InteractionJoinSourcePlayer
                    {
                        get
                        {
                            return string.Format("FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                   "JOIN Encounter E ON {1}.EncounterId = E.Id JOIN Player P ON {1}.SourcePlayerId = P.Id ", RecordTable, RecordTableAlias);
                        }
                    }
                    public static string InteractionJoinTargetPlayer
                    {
                        get
                        {
                            return string.Format("FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                   "JOIN Encounter E ON {1}.EncounterId = E.Id JOIN Player P ON {1}.TargetPlayerId = P.Id ", RecordTable, RecordTableAlias);
                        }
                    }
                    public static string InteractionJoinNoPlayer
                    {
                        get
                        {
                            return string.Format("FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                   "JOIN Encounter E ON {1}.EncounterId = E.Id ", RecordTable, RecordTableAlias);
                        }
                    }
                    public static string InteractionBaseQueryTotals
                    {
                        get
                        {
                            return string.Format("COUNT(CASE WHEN {0}.CriticalHit = 1 THEN 1 END) AS Crits, " +
                                    "count(CASE WHEN {0}.CriticalHit = 0 THEN 1 END) AS Hits, " +
                                    "MAX({0}.TotalHealing) AS BiggestHit, AVG({0}.TotalHealing) AS AverageHit, " +
                                    "SUM({0}.TotalHealing) AS Total, " +
                                    "SUM({0}.EffectiveHealing) AS Effective, " +
                                    "SUM({0}.OverhealAmount) AS Overhealing, " +
                                    "(SUM(TotalHealing) / {1}) AS Average, " +
                                    "(SUM(EffectiveHealing) / {1}) AS AverageEffective, " +
                                    "((100.0 * (SUM({0}.EffectiveHealing)) / (SELECT (SUM(EffectiveHealing)) ",
                                    RecordTableAlias, SearchEncounterDuration);
                        }
                    }
                    public static string InteractionBaseQueryPerSecond
                    {
                        get
                        {
                            return string.Format("SUM({0}.TotalHealing) AS Total, SUM({0}.EffectiveHealing) AS Effective ", RecordTableAlias);
                        }
                    }
                    public static string BaseWhereIncoming
                    {
                        get
                        {
                            return string.Format("WHERE {0}.EncounterId = {1} AND {0}.Target{2}Id = {3} AND {4} > 0 ",
                                RecordTableAlias, SearchEncounterId, SearchCharacter, SearchCharacterId, SearchField);
                        }
                    }
                    public static string BaseWhereOutgoing
                    {
                        get
                        {
                            return string.Format("WHERE {0}.EncounterId = {1} AND {0}.Source{2}Id = {3} AND {4} > 0 ",
                                   RecordTableAlias, SearchEncounterId, SearchCharacter, SearchCharacterId, SearchField);
                        }
                    }
                    #endregion

                    public static class Done
                    {
                        public static class ByAbility
                        {
                            public static class OnlyNpcs
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {0}.SecondsElapsed, A.Name AS AbilityName, " +
                                                             InteractionBaseQueryPerSecond +
                                                             "FROM {1} {0} JOIN Ability A ON {0}.AbilityId = A.Id " +
                                                             BaseWhereOutgoing + "AND {0}.TargetNpcId IS NOT NULL " +
                                                             "GROUP BY {0}.SecondsElapsed, A.Name " +
                                                             "ORDER BY {0}.SecondsElapsed, A.Name",
                                                             RecordTableAlias, RecordTable);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT A.Name AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
                                                             "FROM {0} WHERE EncounterId = {2} AND SourcePlayerId = {3} AND TargetNpcId IS NOT NULL))) AS Percentage " +
                                                             InteractionJoinNoPlayer + BaseWhereOutgoing + " AND {1}.TargetNpcId IS NOT NULL " +
                                                             "GROUP BY A.Name, A.DamageType, A.Icon, E.Duration ORDER BY Effective DESC",
                                                             RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            public static class OtherPlayers
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, A.Name AS AbilityName, " +
                                            InteractionBaseQueryPerSecond +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.TargetPlayerId = P.Id " +
                                                 BaseWhereOutgoing + "AND {1}.TargetPlayerId <> {2} GROUP BY {1}.SecondsElapsed, A.Name " +
                                                 "ORDER BY {1}.SecondsElapsed, A.Name", RecordTable, RecordTableAlias, SearchCharacterId);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT A.Name AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
                                                "FROM {0} WHERE EncounterId = {1} AND SourcePlayerId = {2} AND TargetPlayerId <> {2}))) AS Percentage " +
                                                InteractionJoinSourcePlayer + BaseWhereOutgoing + " AND {3}.TargetPlayerId <> {2} " +
                                               "GROUP BY A.Name, A.DamageType, A.Icon, E.Duration ORDER BY Effective DESC",
                                               RecordTable, SearchEncounterId, SearchCharacterId, RecordTableAlias);
                                    }
                                }
                            }

                            public static class AllPlayers
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, A.Name AS AbilityName, " +
                                                InteractionBaseQueryPerSecond +
                                                "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.TargetPlayerId = P.Id " +
                                                BaseWhereOutgoing + "AND {1}.TargetPlayerId IS NOT NULL GROUP BY {1}.SecondsElapsed, A.Name " +
                                                "ORDER BY {1}.SecondsElapsed, AbilityName",
                                                RecordTable, RecordTableAlias);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT A.Name AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {1} AND SourcePlayerId = {2} AND TargetPlayerId IS NOT NULL))) AS Percentage " +
                                               InteractionJoinSourcePlayer + BaseWhereOutgoing + " AND {3}.TargetPlayerId IS NOT NULL " +
                                               "GROUP BY A.Name, A.DamageType, A.Icon, E.Duration ORDER BY Effective DESC",
                                               RecordTable, SearchEncounterId, SearchCharacterId, RecordTableAlias);
                                    }
                                }
                            }

                            public static class OnlySelf
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, A.Name AS AbilityName, " +
                                            InteractionBaseQueryPerSecond +
                                            "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                            BaseWhereOutgoing + "AND {1}.TargetPlayerId = {2} GROUP BY {1}.SecondsElapsed, A.Name " +
                                            "ORDER BY {1}.SecondsElapsed, AbilityName",
                                            RecordTable, RecordTableAlias, SearchCharacterId);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT A.Name AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
                                                "FROM {0} WHERE EncounterId = {1} AND TargetPlayerId = {2} AND SourcePlayerId = {2}))) AS Percentage " +
                                                InteractionJoinSourcePlayer + BaseWhereOutgoing + " AND {3}.TargetPlayerId = {2} " +
                                               "GROUP BY A.Name, A.DamageType, A.Icon, E.Duration ORDER BY Effective DESC",
                                               RecordTable, SearchEncounterId, SearchCharacterId, RecordTableAlias);
                                    }
                                }
                            }

                            public static class AllTargets
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, A.Name AS AbilityName, " +
                                               InteractionBaseQueryPerSecond +
                                               "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                               BaseWhereOutgoing + "AND {1}.TargetNpcId IS NOT NULL " +
                                               "GROUP BY {1}.SecondsElapsed, A.Name " +
                                               "UNION " +
                                               "SELECT {1}.SecondsElapsed, A.Name AS AbilityName, " +
                                               InteractionBaseQueryPerSecond +
                                               "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.TargetPlayerId = P.Id " +
                                               BaseWhereOutgoing + "AND {1}.TargetPlayerId IS NOT NULL " +
                                               "GROUP BY {1}.SecondsElapsed, A.Name " +
                                               "ORDER BY SecondsElapsed, AbilityName",
                                               RecordTable, RecordTableAlias);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT A.Name AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND SourcePlayerId = {3} AND TargetNpcId IS NOT NULL))) AS Percentage " +
                                               InteractionJoinNoPlayer + BaseWhereOutgoing + " AND {1}.TargetNpcId IS NOT NULL " +
                                               "GROUP BY A.Name, A.DamageType, A.Icon, E.Duration " +
                                               "UNION " +
                                               "SELECT A.Name AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND SourcePlayerId = {3} AND TargetPlayerId IS NOT NULL))) AS Percentage " +
                                               InteractionJoinSourcePlayer + BaseWhereOutgoing + " AND {1}.TargetPlayerId IS NOT NULL " +
                                               "GROUP BY A.Name, A.DamageType, A.Icon, E.Duration ORDER BY Effective DESC",
                                               RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }
                        }

                        public static class ByTarget
                        {
                            public static class OnlyNpcs
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, {1}.TargetNpcName AS TargetName, {1}.TargetNpcId AS TargetId, " +
                                            InteractionBaseQueryPerSecond +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                                 BaseWhereOutgoing + "AND {1}.TargetNpcId IS NOT NULL GROUP BY {1}.SecondsElapsed, {1}.TargetNpcName, {1}.TargetNpcId " +
                                                 "ORDER BY {1}.SecondsElapsed, {1}.TargetNpcName", RecordTable, RecordTableAlias, SearchCharacterId);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.TargetNpcName AS TargetName," + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND SourcePlayerId = {3} AND TargetNpcId IS NOT NULL))) AS Percentage " +
                                               InteractionJoinNoPlayer + BaseWhereOutgoing + " AND {1}.TargetNpcId IS NOT NULL " +
                                               "GROUP BY {1}.TargetNpcName, {1}.TargetNpcId, E.Duration ORDER BY Effective DESC",
                                               RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            public static class OtherPlayers
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, P.Name AS TargetName, P.Id AS TargetId, " +
                                            InteractionBaseQueryPerSecond +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.TargetPlayerId = P.Id " +
                                                 BaseWhereOutgoing + "AND {1}.TargetPlayerId <> {2} GROUP BY {1}.SecondsElapsed, P.Name, P.Id " +
                                                 "ORDER BY {1}.SecondsElapsed, P.Name", RecordTable, RecordTableAlias, SearchCharacterId);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT P.Name AS TargetName," + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND SourcePlayerId = {3} AND TargetPlayerId <> {3}))) AS Percentage " +
                                               InteractionJoinTargetPlayer + BaseWhereOutgoing + " AND {1}.TargetPlayerId <> {3} " +
                                               "GROUP BY P.Name, P.Id, E.Duration ORDER BY Effective DESC",
                                               RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            public static class AllPlayers
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, P.Name AS TargetName, P.Id AS TargetId, " +
                                            InteractionBaseQueryPerSecond +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.TargetPlayerId = P.Id " +
                                                 BaseWhereOutgoing + "AND {1}.TargetPlayerId IS NOT NULL GROUP BY {1}.SecondsElapsed, P.Name, P.Id " +
                                                 "ORDER BY {1}.SecondsElapsed, P.Name", RecordTable, RecordTableAlias, SearchCharacterId);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT P.Name AS TargetName," + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND SourcePlayerId = {3} AND TargetPlayerId IS NOT NULL))) AS Percentage " +
                                               InteractionJoinTargetPlayer + BaseWhereOutgoing + " AND {1}.TargetPlayerId IS NOT NULL " +
                                               "GROUP BY P.Name, P.Id, E.Duration ORDER BY Effective DESC",
                                               RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            public static class AllTargets
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, {1}.TargetNpcName AS TargetName, {1}.TargetNpcId AS TargetId, " +
                                                 "SUM({1}.TotalHealing) AS Total, " +
                                                 "SUM({1}.EffectiveHealing) AS Effective, " +
                                                 "SUM({1}.OverhealAmount) AS Overhealing " +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                                 "WHERE {1}.EncounterId = {2} AND {1}.SourcePlayerId = {3} AND {1}.{4} > 0 AND {1}.TargetNpcId IS NOT NULL " +
                                                 "GROUP BY {1}.SecondsElapsed, {1}.TargetNpcName, {1}.TargetNpcId " +
                                                 "UNION " +
                                                 "SELECT {1}.SecondsElapsed, P.Name AS TargetName, CAST(P.Id AS CHAR(50)) AS TargetId, " +
                                                 "SUM({1}.TotalHealing) AS Total, " +
                                                 "SUM({1}.EffectiveHealing) AS Effective, " +
                                                 "SUM({1}.OverhealAmount) AS Overhealing " +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.TargetPlayerId = P.Id " +
                                                 "WHERE {1}.EncounterId = {2} AND {1}.SourcePlayerId = {3} AND {1}.{4} > 0 AND {1}.TargetPlayerId IS NOT NULL " +
                                                 "GROUP BY {1}.SecondsElapsed, P.Name, P.Id " +
                                                 "ORDER BY SecondsElapsed, TargetName",
                                                 RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId, SearchField);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.TargetNpcName AS TargetName," + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND SourcePlayerId = {3}))) AS Percentage " +
                                               InteractionJoinNoPlayer + BaseWhereOutgoing + " AND {1}.TargetNpcId IS NOT NULL " +
                                               "GROUP BY {1}.TargetNpcName, {1}.TargetNpcId, E.Duration " +
                                               "UNION " +
                                               "SELECT P.Name AS TargetName," + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND SourcePlayerId = {3}))) AS Percentage " +
                                               InteractionJoinTargetPlayer + BaseWhereOutgoing + " AND {1}.TargetPlayerId IS NOT NULL " +
                                               "GROUP BY P.Name, P.Id, E.Duration ORDER BY Effective DESC",
                                               RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }
                        }
                    }

                    public static class Taken
                    {
                        public static class ByAbility
                        {
                            public static class OnlyNpcs
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {0}.SecondsElapsed, CONCAT(A.Name, ' (', {0}.SourceNpcName, ')') AS AbilityName, " +
                                                             InteractionBaseQueryPerSecond +
                                                             "FROM {1} {0} JOIN Ability A ON {0}.AbilityId = A.Id " +
                                                             BaseWhereIncoming + "AND {0}.SourceNpcId IS NOT NULL " +
                                                             "GROUP BY {0}.SecondsElapsed, A.Name, {0}.SourceNpcName, {0}.SourceNpcId " +
                                                             "ORDER BY {0}.SecondsElapsed, A.Name, {0}.SourceNpcName, {0}.SourceNpcId",
                                                             RecordTableAlias, RecordTable);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT CONCAT(A.Name, ' (', {1}.SourceNpcName, ')') AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND TargetPlayerId = {3} AND SourceNpcId IS NOT NULL))) AS Percentage " +
                                               InteractionJoinNoPlayer + BaseWhereIncoming + " AND {1}.SourceNpcId IS NOT NULL " +
                                               "GROUP BY {1}.SourceNpcName, {1}.SourceNpcId, A.Name, A.DamageType, A.Icon, E.Duration ORDER BY Effective DESC",
                                               RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            public static class OtherPlayers
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, CONCAT(A.Name, ' (', P.Name, ')') AS AbilityName, " +
                                            InteractionBaseQueryPerSecond +
                                            "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.SourcePlayerId = P.Id " +
                                            BaseWhereIncoming + "AND {1}.SourcePlayerId <> {2} " +
                                            "GROUP BY {1}.SecondsElapsed, A.Name, P.Name, P.Id " +
                                            "ORDER BY {1}.SecondsElapsed, A.Name, P.Name, P.Id",
                                            RecordTable, RecordTableAlias, SearchCharacterId);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT CONCAT(A.Name, ' (', P.Name, ')') AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
                                                "FROM {0} WHERE EncounterId = {2} AND TargetPlayerId = {3} AND SourcePlayerId <> {3}))) AS Percentage " +
                                                InteractionJoinSourcePlayer + BaseWhereIncoming + " AND {1}.SourcePlayerId <> {3} " +
                                                "GROUP BY P.Name, P.Id, A.Name, A.DamageType, A.Icon, E.Duration ORDER BY Effective DESC",
                                                RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            public static class AllPlayers
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, CONCAT(A.Name, ' (', P.Name, ')') AS AbilityName, " +
                                            InteractionBaseQueryPerSecond +
                                            "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.SourcePlayerId = P.Id " +
                                            BaseWhereIncoming + "AND {1}.SourcePlayerId IS NOT NULL " +
                                            "GROUP BY {1}.SecondsElapsed, A.Name, P.Name, P.Id " +
                                            "ORDER BY {1}.SecondsElapsed, A.Name, P.Name, P.Id",
                                            RecordTable, RecordTableAlias);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {

                                        return string.Format("SELECT CONCAT(A.Name, ' (', P.Name, ')') AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
                                                "FROM {0} WHERE EncounterId = {2} AND TargetPlayerId = {3} AND SourcePlayerId IS NOT NULL))) AS Percentage " +
                                                InteractionJoinSourcePlayer + BaseWhereIncoming + " AND {1}.SourcePlayerId IS NOT NULL " +
                                                "GROUP BY P.Name, P.Id, A.Name, A.DamageType, A.Icon, E.Duration ORDER BY Effective DESC",
                                                RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            public static class OnlySelf
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, A.Name AS AbilityName, " +
                                                 InteractionBaseQueryPerSecond +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                                 BaseWhereIncoming + "AND {1}.SourcePlayerId = {2} GROUP BY {1}.SecondsElapsed, A.Name " +
                                                 "ORDER BY {1}.SecondsElapsed, AbilityName",
                                                 RecordTable, RecordTableAlias, SearchCharacterId);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT A.Name AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
                                                "FROM {0} WHERE EncounterId = {2} AND TargetPlayerId = {3} AND SourcePlayerId = {3}))) AS Percentage " +
                                                InteractionJoinSourcePlayer + BaseWhereIncoming + " AND {1}.SourcePlayerId = {3} " +
                                                "GROUP BY P.Name, P.Id, A.Name, A.DamageType, A.Icon, E.Duration ORDER BY Effective DESC",
                                                RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            public static class AllSources
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, CONCAT(A.Name, ' (', {1}.SourceNpcName, ')') AS AbilityName, " +
                                                InteractionBaseQueryPerSecond +
                                                "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                                BaseWhereIncoming + "AND {1}.SourceNpcId IS NOT NULL " +
                                                "GROUP BY {1}.SecondsElapsed, A.Name, {1}.SourceNpcName, {1}.SourceNpcId " +
                                                "UNION " +
                                                "SELECT {1}.SecondsElapsed, CONCAT(A.Name, ' (', P.Name, ')') AS AbilityName, " +
                                                InteractionBaseQueryPerSecond +
                                                "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.SourcePlayerId = P.Id " +
                                                BaseWhereIncoming + "AND {1}.SourcePlayerId IS NOT NULL " +
                                                "GROUP BY {1}.SecondsElapsed, A.Name, P.Name, P.Id " +
                                                "ORDER BY SecondsElapsed, AbilityName",
                                                RecordTable, RecordTableAlias);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT CONCAT(A.Name, ' (', {1}.SourceNpcName, ')') AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND TargetPlayerId = {3} AND SourceNpcId IS NOT NULL))) AS Percentage " +
                                               InteractionJoinNoPlayer + BaseWhereIncoming + " AND {1}.SourceNpcId IS NOT NULL " +
                                               "GROUP BY {1}.SourceNpcName, {1}.SourceNpcId, A.Name, A.DamageType, A.Icon, E.Duration " +
                                               "UNION " +
                                               "SELECT CONCAT(A.Name, ' (', P.Name, ')') AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND TargetPlayerId = {3} AND SourcePlayerId IS NOT NULL))) AS Percentage " +
                                               InteractionJoinSourcePlayer + BaseWhereIncoming + " AND {1}.SourcePlayerId IS NOT NULL " +
                                               "GROUP BY P.Name, P.Id, A.Name, A.DamageType, A.Icon, E.Duration ORDER BY Effective DESC",
                                                RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }
                        }

                        public static class BySource
                        {
                            public static class OnlyNpcs
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, {1}.SourceNpcName AS SourceName, {1}.SourceNpcId AS SourceId, " +
                                            InteractionBaseQueryPerSecond +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                                 BaseWhereIncoming + "AND {1}.SourceNpcId IS NOT NULL GROUP BY {1}.SecondsElapsed, {1}.SourceNpcName, {1}.SourceNpcId " +
                                                 "ORDER BY {1}.SecondsElapsed, {1}.SourceNpcName", RecordTable, RecordTableAlias, SearchCharacterId);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SourceNpcName AS SourceName," + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND TargetPlayerId = {3} AND SourceNpcId IS NOT NULL))) AS Percentage " +
                                               InteractionJoinNoPlayer + BaseWhereIncoming + " AND {1}.SourceNpcId IS NOT NULL " +
                                               "GROUP BY {1}.SourceNpcName, {1}.SourceNpcId, E.Duration ORDER BY Effective DESC",
                                                RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            public static class OtherPlayers
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, P.Name AS SourceName, P.Id AS SourceId, " +
                                            InteractionBaseQueryPerSecond +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.SourcePlayerId = P.Id " +
                                                 BaseWhereIncoming + "AND {1}.SourcePlayerId <> {2} GROUP BY {1}.SecondsElapsed, P.Name, P.Id " +
                                                 "ORDER BY {1}.SecondsElapsed, P.Name", RecordTable, RecordTableAlias, SearchCharacterId);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT P.Name AS SourceName," + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND TargetPlayerId = {3} AND SourcePlayerId <> {3}))) AS Percentage " +
                                               InteractionJoinSourcePlayer + BaseWhereIncoming + " AND {1}.SourcePlayerId <> {3} " +
                                               "GROUP BY P.Name, P.Id, E.Duration ORDER BY Effective DESC",
                                                RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            public static class AllPlayers
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, P.Name AS SourceName, P.Id AS SourceId, " +
                                            InteractionBaseQueryPerSecond +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.SourcePlayerId = P.Id " +
                                                 BaseWhereIncoming + "AND {1}.SourcePlayerId IS NOT NULL GROUP BY {1}.SecondsElapsed, P.Name, P.Id " +
                                                 "ORDER BY {1}.SecondsElapsed, P.Name", RecordTable, RecordTableAlias, SearchCharacterId);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT P.Name AS SourceName," + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND TargetPlayerId = {3} AND SourcePlayerId IS NOT NULL))) AS Percentage " +
                                               InteractionJoinSourcePlayer + BaseWhereIncoming + " AND {1}.SourcePlayerId IS NOT NULL " +
                                               "GROUP BY P.Name, P.Id, E.Duration ORDER BY Effective DESC",
                                                RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            public static class AllSources
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format(
                                            "SELECT {1}.SecondsElapsed, {1}.SourceNpcName AS SourceName, {1}.SourceNpcId AS SourceId, " +
                                            InteractionBaseQueryPerSecond +
                                            "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                            BaseWhereIncoming + "AND {1}.SourceNpcId IS NOT NULL GROUP BY {1}.SecondsElapsed, {1}.SourceNpcName, {1}.SourceNpcId " +
                                            "UNION " +
                                            "SELECT {1}.SecondsElapsed, P.Name AS SourceName, CAST(P.Id AS CHAR(50)) AS SourceId, " + InteractionBaseQueryPerSecond +
                                            "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.SourcePlayerId = P.Id " +
                                            BaseWhereIncoming + "AND {1}.SourcePlayerId IS NOT NULL GROUP BY {1}.SecondsElapsed, P.Name, P.Id " +
                                            "ORDER BY SecondsElapsed, SourceName",
                                            RecordTable, RecordTableAlias, SearchCharacterId);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SourceNpcName AS SourceName," + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND TargetPlayerId = {3}))) AS Percentage " +
                                               InteractionJoinNoPlayer + BaseWhereIncoming + " AND {1}.SourceNpcId IS NOT NULL " +
                                               "GROUP BY {1}.SourceNpcName, {1}.SourceNpcId, E.Duration " +
                                               "UNION " +
                                               "SELECT P.Name AS SourceName," + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND TargetPlayerId = {3}))) AS Percentage " +
                                               InteractionJoinSourcePlayer + BaseWhereIncoming + " AND {1}.SourcePlayerId IS NOT NULL " +
                                               "GROUP BY P.Name, P.Id, E.Duration ORDER BY Effective DESC",
                                                RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }
                        }
                    }
                }

                public static class Shielding
                {
                    public static string RecordTable { get { return "ShieldingDone"; } }
                    public static string RecordTableAlias { get { return "SD"; } }
                    public static string SearchCharacter { get { return "Player"; } }
                    public static string SearchEncounterId { get { return "@Id"; } }
                    public static string SearchCharacterId { get { return "@playerId"; } }
                    public static string SearchField { get { return "ShieldValue"; } }
                    #region 'Base' queries
                    public static string InteractionJoinSourcePlayer
                    {
                        get
                        {
                            return string.Format("FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                   "JOIN Encounter E ON {1}.EncounterId = E.Id JOIN Player P ON {1}.SourcePlayerId = P.Id ", RecordTable, RecordTableAlias);
                        }
                    }
                    public static string InteractionJoinTargetPlayer
                    {
                        get
                        {
                            return string.Format("FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                   "JOIN Encounter E ON {1}.EncounterId = E.Id JOIN Player P ON {1}.TargetPlayerId = P.Id ", RecordTable, RecordTableAlias);
                        }
                    }
                    public static string InteractionJoinNoPlayer
                    {
                        get
                        {
                            return string.Format("FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                   "JOIN Encounter E ON {1}.EncounterId = E.Id ", RecordTable, RecordTableAlias);
                        }
                    }
                    public static string InteractionBaseQueryTotals
                    {
                        get
                        {
                            return string.Format("COUNT(CASE WHEN {0}.CriticalHit = 1 THEN 1 END) AS Crits, " +
                                    "count(CASE WHEN {0}.CriticalHit = 0 THEN 1 END) AS Hits, " +
                                    "MAX({0}.ShieldValue) AS BiggestHit, AVG({0}.ShieldValue) AS AverageHit, " +
                                    "SUM({0}.ShieldValue) AS Total, " +
                                    "(SUM(ShieldValue) / {1}) AS Average, " +
                                    "((100.0 * (SUM({0}.ShieldValue)) / (SELECT (SUM(ShieldValue)) ",
                                    RecordTableAlias, SearchEncounterDuration);
                        }
                    }
                    public static string InteractionBaseQueryPerSecond
                    {
                        get
                        {
                            return string.Format("SUM({0}.ShieldValue) AS Total ", RecordTableAlias);
                        }
                    }
                    public static string BaseWhereIncoming
                    {
                        get
                        {
                            return string.Format("WHERE {0}.EncounterId = {1} AND {0}.Target{2}Id = {3} AND {4} > 0 ",
                                RecordTableAlias, SearchEncounterId, SearchCharacter, SearchCharacterId, SearchField);
                        }
                    }
                    public static string BaseWhereOutgoing
                    {
                        get
                        {
                            return string.Format("WHERE {0}.EncounterId = {1} AND {0}.Source{2}Id = {3} AND {4} > 0 ",
                                   RecordTableAlias, SearchEncounterId, SearchCharacter, SearchCharacterId, SearchField);
                        }
                    }
                    #endregion

                    /// <summary>
                    /// Outgoing shielding
                    /// </summary>
                    public static class Done
                    {
                        /// <summary>
                        /// Outgoing shielding, grouped by ability
                        /// </summary>
                        public static class ByAbility
                        {
                            /// <summary>
                            /// Outgoing shielding, grouped by ability, targeted at NPCs
                            /// </summary>
                            public static class OnlyNpcs
                            {
                                /// <summary>
                                /// Outgoing shielding per second, grouped by ability, targeted at NPCs (for graphs)
                                /// </summary>
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {0}.SecondsElapsed, A.Name AS AbilityName, " +
                                                             InteractionBaseQueryPerSecond +
                                                             "FROM {1} {0} JOIN Ability A ON {0}.AbilityId = A.Id " +
                                                             BaseWhereOutgoing + "AND {0}.TargetNpcId IS NOT NULL " +
                                                             "GROUP BY {0}.SecondsElapsed, A.Name " +
                                                             "ORDER BY {0}.SecondsElapsed, A.Name",
                                                             RecordTableAlias, RecordTable);
                                    }
                                }
                                /// <summary>
                                /// Outgoing shielding summary, grouped by ability, targeted at NPCs (for tables)
                                /// </summary>
                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT A.Name AS AbilityName, A.Icon, " + InteractionBaseQueryTotals +
                                                             "FROM {0} WHERE EncounterId = {2} AND SourcePlayerId = {3} AND TargetNpcId IS NOT NULL))) AS Percentage " +
                                                             InteractionJoinNoPlayer + BaseWhereOutgoing + " AND {1}.TargetNpcId IS NOT NULL " +
                                                             "GROUP BY A.Name, A.Icon, E.Duration ORDER BY ShieldValue DESC",
                                                             RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }
                            /// <summary>
                            /// Outgoing shielding, grouped by ability, targeted at players other than the source player
                            /// </summary>
                            public static class OtherPlayers
                            {
                                /// <summary>
                                /// Outgoing shielding per second, grouped by ability, targeted at players other than the source player (for graphs)
                                /// </summary>
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, A.Name AS AbilityName, " +
                                            InteractionBaseQueryPerSecond +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.TargetPlayerId = P.Id " +
                                                 BaseWhereOutgoing + "AND {1}.TargetPlayerId <> {2} GROUP BY {1}.SecondsElapsed, A.Name " +
                                                 "ORDER BY {1}.SecondsElapsed, A.Name", RecordTable, RecordTableAlias, SearchCharacterId);
                                    }
                                }
                                /// <summary>
                                /// Outgoing shielding summary, grouped by ability, targeted at players other than the source player (for graphs)
                                /// </summary>
                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT A.Name AS AbilityName, A.Icon, " + InteractionBaseQueryTotals +
                                                "FROM {0} WHERE EncounterId = {1} AND SourcePlayerId = {2} AND TargetPlayerId <> {2}))) AS Percentage " +
                                                InteractionJoinSourcePlayer + BaseWhereOutgoing + " AND {3}.TargetPlayerId <> {2} " +
                                               "GROUP BY A.Name, A.Icon, E.Duration ORDER BY Total DESC",
                                               RecordTable, SearchEncounterId, SearchCharacterId, RecordTableAlias);
                                    }
                                }
                            }

                            public static class AllPlayers
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, A.Name AS AbilityName, " +
                                                InteractionBaseQueryPerSecond +
                                                "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.TargetPlayerId = P.Id " +
                                                BaseWhereOutgoing + "AND {1}.TargetPlayerId IS NOT NULL GROUP BY {1}.SecondsElapsed, A.Name " +
                                                "ORDER BY {1}.SecondsElapsed, AbilityName",
                                                RecordTable, RecordTableAlias);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT A.Name AS AbilityName, A.Icon, " + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {1} AND SourcePlayerId = {2} AND TargetPlayerId IS NOT NULL))) AS Percentage " +
                                               InteractionJoinSourcePlayer + BaseWhereOutgoing + " AND {3}.TargetPlayerId IS NOT NULL " +
                                               "GROUP BY A.Name, A.Icon, E.Duration ORDER BY Total DESC",
                                               RecordTable, SearchEncounterId, SearchCharacterId, RecordTableAlias);
                                    }
                                }
                            }

                            public static class OnlySelf
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, A.Name AS AbilityName, " +
                                            InteractionBaseQueryPerSecond +
                                            "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                            BaseWhereOutgoing + "AND {1}.TargetPlayerId = {2} GROUP BY {1}.SecondsElapsed, A.Name " +
                                            "ORDER BY {1}.SecondsElapsed, AbilityName",
                                            RecordTable, RecordTableAlias, SearchCharacterId);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT A.Name AS AbilityName, A.Icon, " + InteractionBaseQueryTotals +
                                                "FROM {0} WHERE EncounterId = {1} AND TargetPlayerId = {2} AND SourcePlayerId = {2}))) AS Percentage " +
                                                InteractionJoinSourcePlayer + BaseWhereOutgoing + " AND {3}.TargetPlayerId = {2} " +
                                               "GROUP BY A.Name, A.Icon, E.Duration ORDER BY Total DESC",
                                               RecordTable, SearchEncounterId, SearchCharacterId, RecordTableAlias);
                                    }
                                }
                            }

                            public static class AllTargets
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, A.Name AS AbilityName, " +
                                               InteractionBaseQueryPerSecond +
                                               "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                               BaseWhereOutgoing + "AND {1}.TargetNpcId IS NOT NULL " +
                                               "GROUP BY {1}.SecondsElapsed, A.Name " +
                                               "UNION " +
                                               "SELECT {1}.SecondsElapsed, A.Name AS AbilityName, " +
                                               InteractionBaseQueryPerSecond +
                                               "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.TargetPlayerId = P.Id " +
                                               BaseWhereOutgoing + "AND {1}.TargetPlayerId IS NOT NULL " +
                                               "GROUP BY {1}.SecondsElapsed, A.Name " +
                                               "ORDER BY SecondsElapsed, AbilityName",
                                               RecordTable, RecordTableAlias);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT A.Name AS AbilityName, A.Icon, " + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND SourcePlayerId = {3} AND TargetNpcId IS NOT NULL))) AS Percentage " +
                                               InteractionJoinNoPlayer + BaseWhereOutgoing + " AND {1}.TargetNpcId IS NOT NULL " +
                                               "GROUP BY A.Name, A.Icon, E.Duration " +
                                               "UNION " +
                                               "SELECT A.Name AS AbilityName, A.Icon, " + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND SourcePlayerId = {3} AND TargetPlayerId IS NOT NULL))) AS Percentage " +
                                               InteractionJoinSourcePlayer + BaseWhereOutgoing + " AND {1}.TargetPlayerId IS NOT NULL " +
                                               "GROUP BY A.Name, A.Icon, E.Duration ORDER BY Total DESC",
                                               RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }
                        }

                        public static class ByTarget
                        {
                            public static class OnlyNpcs
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, {1}.TargetNpcName AS TargetName, {1}.TargetNpcId AS TargetId, " +
                                            InteractionBaseQueryPerSecond +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                                 BaseWhereOutgoing + "AND {1}.TargetNpcId = {2} GROUP BY {1}.SecondsElapsed, {1}.TargetNpcName, {1}.TargetNpcId " +
                                                 "ORDER BY {1}.SecondsElapsed, {1}.TargetNpcName", RecordTable, RecordTableAlias, SearchCharacterId);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.TargetNpcName AS TargetName," + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND SourcePlayerId = {3} AND TargetNpcId IS NOT NULL))) AS Percentage " +
                                               InteractionJoinNoPlayer + BaseWhereOutgoing + " AND {1}.TargetNpcId IS NOT NULL " +
                                               "GROUP BY {1}.TargetNpcName, {1}.TargetNpcId, E.Duration ORDER BY Total DESC",
                                               RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            public static class OtherPlayers
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, P.Name AS TargetName, P.Id AS TargetId, " +
                                            InteractionBaseQueryPerSecond +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.TargetPlayerId = P.Id " +
                                                 BaseWhereOutgoing + "AND {1}.TargetPlayerId <> {2} GROUP BY {1}.SecondsElapsed, P.Name, P.Id " +
                                                 "ORDER BY {1}.SecondsElapsed, P.Name", RecordTable, RecordTableAlias, SearchCharacterId);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT P.Name AS TargetName," + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND SourcePlayerId = {3} AND TargetPlayerId <> {3}))) AS Percentage " +
                                               InteractionJoinTargetPlayer + BaseWhereOutgoing + " AND {1}.TargetPlayerId <> {3} " +
                                               "GROUP BY P.Name, P.Id, E.Duration ORDER BY Total DESC",
                                               RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            public static class AllPlayers
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, P.Name AS TargetName, P.Id AS TargetId, " +
                                            InteractionBaseQueryPerSecond +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.TargetPlayerId = P.Id " +
                                                 BaseWhereOutgoing + "AND {1}.TargetPlayerId IS NOT NULL GROUP BY {1}.SecondsElapsed, P.Name, P.Id " +
                                                 "ORDER BY {1}.SecondsElapsed, P.Name", RecordTable, RecordTableAlias, SearchCharacterId);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT P.Name AS TargetName," + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND SourcePlayerId = {3} AND TargetPlayerId IS NOT NULL))) AS Percentage " +
                                               InteractionJoinTargetPlayer + BaseWhereOutgoing + " AND {1}.TargetPlayerId IS NOT NULL " +
                                               "GROUP BY P.Name, P.Id, E.Duration ORDER BY Total DESC",
                                               RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            public static class AllTargets
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, {1}.TargetNpcName AS TargetName, {1}.TargetNpcId AS TargetId, " +
                                                 "SUM({1}.ShieldValue) AS Total " +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                                 "WHERE {1}.EncounterId = {2} AND {1}.SourcePlayerId = {3} AND {1}.{4} > 0 AND {1}.TargetNpcId IS NOT NULL " +
                                                 "GROUP BY {1}.SecondsElapsed, {1}.TargetNpcName, {1}.TargetNpcId " +
                                                 "UNION " +
                                                 "SELECT {1}.SecondsElapsed, P.Name AS TargetName, CAST(P.Id AS CHAR(50)) AS TargetId, " +
                                                 "SUM({1}.ShieldValue) AS Total " +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.TargetPlayerId = P.Id " +
                                                 "WHERE {1}.EncounterId = {2} AND {1}.SourcePlayerId = {3} AND {1}.{4} > 0 AND {1}.TargetPlayerId IS NOT NULL " +
                                                 "GROUP BY {1}.SecondsElapsed, P.Name, P.Id " +
                                                 "ORDER BY SecondsElapsed, TargetName",
                                                 RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId, SearchField);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.TargetNpcName AS TargetName," + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND SourcePlayerId = {3}))) AS Percentage " +
                                               InteractionJoinNoPlayer + BaseWhereOutgoing + " AND {1}.TargetNpcId IS NOT NULL " +
                                               "GROUP BY {1}.TargetNpcName, {1}.TargetNpcId, E.Duration " +
                                               "UNION " +
                                               "SELECT P.Name AS TargetName," + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND SourcePlayerId = {3}))) AS Percentage " +
                                               InteractionJoinTargetPlayer + BaseWhereOutgoing + " AND {1}.TargetPlayerId IS NOT NULL " +
                                               "GROUP BY P.Name, P.Id, E.Duration ORDER BY Total DESC",
                                               RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }
                        }
                    }

                    public static class Taken
                    {
                        public static class ByAbility
                        {
                            public static class OnlyNpcs
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {0}.SecondsElapsed, CONCAT(A.Name, ' (', {0}.SourceNpcName, ')') AS AbilityName, " +
                                                             InteractionBaseQueryPerSecond +
                                                             "FROM {1} {0} JOIN Ability A ON {0}.AbilityId = A.Id " +
                                                             BaseWhereIncoming + "AND {0}.SourceNpcId IS NOT NULL " +
                                                             "GROUP BY {0}.SecondsElapsed, A.Name, {0}.SourceNpcName, {0}.SourceNpcId " +
                                                             "ORDER BY {0}.SecondsElapsed, A.Name, {0}.SourceNpcName, {0}.SourceNpcId",
                                                             RecordTableAlias, RecordTable);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT CONCAT(A.Name, ' (', {1}.SourceNpcName, ')') AS AbilityName, A.Icon, " + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND TargetPlayerId = {3} AND SourceNpcId IS NOT NULL))) AS Percentage " +
                                               InteractionJoinNoPlayer + BaseWhereIncoming + " AND {1}.SourceNpcId IS NOT NULL " +
                                               "GROUP BY {1}.SourceNpcName, {1}.SourceNpcId, A.Name, A.Icon, E.Duration ORDER BY Total DESC",
                                               RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            public static class OtherPlayers
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, CONCAT(A.Name, ' (', P.Name, ')') AS AbilityName, " +
                                            InteractionBaseQueryPerSecond +
                                            "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.SourcePlayerId = P.Id " +
                                            BaseWhereIncoming + "AND {1}.SourcePlayerId <> {2} " +
                                            "GROUP BY {1}.SecondsElapsed, A.Name, P.Name, P.Id " +
                                            "ORDER BY {1}.SecondsElapsed, A.Name, P.Name, P.Id",
                                            RecordTable, RecordTableAlias, SearchCharacterId);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT CONCAT(A.Name, ' (', P.Name, ')') AS AbilityName, A.Icon, " + InteractionBaseQueryTotals +
                                                "FROM {0} WHERE EncounterId = {2} AND TargetPlayerId = {3} AND SourcePlayerId <> {3}))) AS Percentage " +
                                                InteractionJoinSourcePlayer + BaseWhereIncoming + " AND {1}.SourcePlayerId <> {3} " +
                                                "GROUP BY P.Name, P.Id, A.Name, A.Icon, E.Duration ORDER BY Total DESC",
                                                RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            public static class AllPlayers
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, CONCAT(A.Name, ' (', P.Name, ')') AS AbilityName, " +
                                            InteractionBaseQueryPerSecond +
                                            "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.SourcePlayerId = P.Id " +
                                            BaseWhereIncoming + "AND {1}.SourcePlayerId IS NOT NULL " +
                                            "GROUP BY {1}.SecondsElapsed, A.Name, P.Name, P.Id " +
                                            "ORDER BY {1}.SecondsElapsed, A.Name, P.Name, P.Id",
                                            RecordTable, RecordTableAlias);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {

                                        return string.Format("SELECT CONCAT(A.Name, ' (', P.Name, ')') AS AbilityName, A.Icon, " + InteractionBaseQueryTotals +
                                                "FROM {0} WHERE EncounterId = {2} AND TargetPlayerId = {3} AND SourcePlayerId IS NOT NULL))) AS Percentage " +
                                                InteractionJoinSourcePlayer + BaseWhereIncoming + " AND {1}.SourcePlayerId IS NOT NULL " +
                                                "GROUP BY P.Name, P.Id, A.Name, A.Icon, E.Duration ORDER BY Total DESC",
                                                RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            public static class OnlySelf
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, A.Name AS AbilityName, " +
                                                 InteractionBaseQueryPerSecond +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                                 BaseWhereIncoming + "AND {1}.SourcePlayerId = {2} GROUP BY {1}.SecondsElapsed, A.Name " +
                                                 "ORDER BY {1}.SecondsElapsed, AbilityName",
                                                 RecordTable, RecordTableAlias, SearchCharacterId);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT A.Name AS AbilityName, A.Icon, " + InteractionBaseQueryTotals +
                                                "FROM {0} WHERE EncounterId = {2} AND TargetPlayerId = {3} AND SourcePlayerId = {3}))) AS Percentage " +
                                                InteractionJoinSourcePlayer + BaseWhereIncoming + " AND {1}.SourcePlayerId = {3} " +
                                                "GROUP BY P.Name, P.Id, A.Name, A.Icon, E.Duration ORDER BY Total DESC",
                                                RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            public static class AllSources
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, CONCAT(A.Name, ' (', {1}.SourceNpcName, ')') AS AbilityName, " +
                                                InteractionBaseQueryPerSecond +
                                                "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                                BaseWhereIncoming + "AND {1}.SourceNpcId IS NOT NULL " +
                                                "GROUP BY {1}.SecondsElapsed, A.Name, {1}.SourceNpcName, {1}.SourceNpcId " +
                                                "UNION " +
                                                "SELECT {1}.SecondsElapsed, CONCAT(A.Name, ' (', P.Name, ')') AS AbilityName, " +
                                                InteractionBaseQueryPerSecond +
                                                "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.SourcePlayerId = P.Id " +
                                                BaseWhereIncoming + "AND {1}.SourcePlayerId IS NOT NULL " +
                                                "GROUP BY {1}.SecondsElapsed, A.Name, P.Name, P.Id " +
                                                "ORDER BY SecondsElapsed, AbilityName",
                                                RecordTable, RecordTableAlias);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT CONCAT(A.Name, ' (', {1}.SourceNpcName, ')') AS AbilityName, A.Icon, " + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND TargetPlayerId = {3} AND SourceNpcId IS NOT NULL))) AS Percentage " +
                                               InteractionJoinNoPlayer + BaseWhereIncoming + " AND {1}.SourceNpcId IS NOT NULL " +
                                               "GROUP BY {1}.SourceNpcName, {1}.SourceNpcId, A.Name, A.Icon, E.Duration " +
                                               "UNION " +
                                               "SELECT CONCAT(A.Name, ' (', P.Name, ')') AS AbilityName, A.Icon, " + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND TargetPlayerId = {3} AND SourcePlayerId IS NOT NULL))) AS Percentage " +
                                               InteractionJoinSourcePlayer + BaseWhereIncoming + " AND {1}.SourcePlayerId IS NOT NULL " +
                                               "GROUP BY P.Name, P.Id, A.Name, A.Icon, E.Duration ORDER BY Total DESC",
                                                RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }
                        }

                        public static class BySource
                        {
                            public static class OnlyNpcs
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, {1}.SourceNpcName AS SourceName, {1}.SourceNpcId AS SourceId, " +
                                            InteractionBaseQueryPerSecond +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                                 BaseWhereIncoming + "AND {1}.SourceNpcId IS NOT NULL GROUP BY {1}.SecondsElapsed, {1}.SourceNpcName, {1}.SourceNpcId " +
                                                 "ORDER BY {1}.SecondsElapsed, {1}.SourceNpcName", RecordTable, RecordTableAlias, SearchCharacterId);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SourceNpcName AS SourceName," + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND TargetPlayerId = {3} AND SourceNpcId IS NOT NULL))) AS Percentage " +
                                               InteractionJoinNoPlayer + BaseWhereIncoming + " AND {1}.SourceNpcId IS NOT NULL " +
                                               "GROUP BY {1}.SourceNpcName, {1}.SourceNpcId, E.Duration ORDER BY Total DESC",
                                                RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            public static class OtherPlayers
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, P.Name AS SourceName, P.Id AS SourceId, " +
                                            InteractionBaseQueryPerSecond +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.SourcePlayerId = P.Id " +
                                                 BaseWhereIncoming + "AND {1}.SourcePlayerId <> {2} GROUP BY {1}.SecondsElapsed, P.Name, P.Id " +
                                                 "ORDER BY {1}.SecondsElapsed, P.Name", RecordTable, RecordTableAlias, SearchCharacterId);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT P.Name AS SourceName," + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND TargetPlayerId = {3} AND SourcePlayerId <> {3}))) AS Percentage " +
                                               InteractionJoinSourcePlayer + BaseWhereIncoming + " AND {1}.SourcePlayerId <> {3} " +
                                               "GROUP BY P.Name, P.Id, E.Duration ORDER BY Total DESC",
                                                RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }
                            /// <summary>
                            /// Incoming shielding, from all players, sorted by source
                            /// </summary>
                            public static class AllPlayers
                            {
                                /// <summary>
                                /// Incoming shielding per second, from all players, sorted by source (for graphs)
                                /// </summary>
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, P.Name AS SourceName, P.Name AS SourceId, " +
                                            InteractionBaseQueryPerSecond +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.SourcePlayerId = P.Id " +
                                                 BaseWhereIncoming + "AND {1}.SourcePlayerId IS NOT NULL GROUP BY {1}.SecondsElapsed, P.Name, P.Id " +
                                                 "ORDER BY {1}.SecondsElapsed, P.Name", RecordTable, RecordTableAlias, SearchCharacterId);
                                    }
                                }
                                /// <summary>
                                /// Incoming shielding summary, from all players, sorted by source (for tables)
                                /// </summary>
                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT P.Name AS SourceName," + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND TargetPlayerId = {3} AND SourcePlayerId IS NOT NULL))) AS Percentage " +
                                               InteractionJoinSourcePlayer + BaseWhereIncoming + " AND {1}.SourcePlayerId IS NOT NULL " +
                                               "GROUP BY P.Name, P.Id, E.Duration ORDER BY ShieldValue DESC",
                                                RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            /// <summary>
                            /// Incoming shielding, from all sources, grouped by source
                            /// </summary>
                            public static class AllSources
                            {
                                /// <summary>
                                /// Incoming shielding per second, from all sources, grouped by source (for graphs)
                                /// </summary>
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format(
                                            "SELECT {1}.SecondsElapsed, {1}.SourceNpcName AS SourceName, {1}.SourceNpcId AS SourceId, " + InteractionBaseQueryPerSecond +
                                            "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                            BaseWhereIncoming + "AND {1}.SourceNpcId IS NOT NULL GROUP BY {1}.SecondsElapsed, {1}.SourceNpcName, {1}.SourceNpcId " +
                                            "UNION " +
                                            "SELECT {1}.SecondsElapsed, P.Name AS SourceName, CAST(P.Id AS CHAR(50)) AS SourceId, " + InteractionBaseQueryPerSecond +
                                            "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.SourcePlayerId = P.Id " +
                                            BaseWhereIncoming + "AND {1}.SourcePlayerId IS NOT NULL GROUP BY {1}.SecondsElapsed, P.Name, P.Id " +
                                            "ORDER BY SecondsElapsed, SourceName",
                                            RecordTable, RecordTableAlias, SearchCharacterId);
                                    }
                                }
                                /// <summary>
                                /// Incoming shielding summary, from all sources, grouped by source (for tables)
                                /// </summary>
                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SourceNpcName AS SourceName," + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND TargetPlayerId = {3}))) AS Percentage " +
                                               InteractionJoinNoPlayer + BaseWhereIncoming + " AND {1}.SourceNpcId IS NOT NULL " +
                                               "GROUP BY {1}.SourceNpcName, {1}.SourceNpcId, E.Duration " +
                                               "UNION " +
                                               "SELECT P.Name AS SourceName," + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND TargetPlayerId = {3}))) AS Percentage " +
                                               InteractionJoinSourcePlayer + BaseWhereIncoming + " AND {1}.SourcePlayerId IS NOT NULL " +
                                               "GROUP BY P.Name, P.Id, E.Duration ORDER BY Total DESC",
                                                RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            public static class Npc
            {
                public static string AllNpcDeathsForEncounter
                {
                    get { return "SELECT SecondsElapsed FROM EncounterDeath WHERE EncounterId = @id AND TargetNpcId IS NOT NULL ORDER BY SecondsElapsed"; }
                }

                public static string AllDeathsForNpcForEncounter
                {
                    get { return "SELECT SecondsElapsed FROM EncounterDeath WHERE EncounterId = @id AND TargetNpcId = @npcId ORDER BY SecondsElapsed"; }
                }

                public static string GetNameFromIdDamageDone
                {
                    get { return "SELECT SourceNpcName FROM DamageDone WHERE SourceNpcId = @npcId AND EncounterId = @encounterId LIMIT 1"; }
                }

                public static string GetNameFromIdDamageTaken
                {
                    get { return "SELECT TargetNpcName FROM DamageDone WHERE TargetNpcId = @npcId AND EncounterId = @encounterId LIMIT 1"; }
                }

                public static string GetNameFromIdHealingDone
                {
                    get { return "SELECT SourceNpcName FROM HealingDone WHERE SourceNpcId = @npcId AND EncounterId = @encounterId LIMIT 1"; }
                }

                public static string GetNameFromIdHealingTaken
                {
                    get { return "SELECT TargetNpcName FROM HealingDone WHERE TargetNpcId = @npcId AND EncounterId = @encounterId LIMIT 1"; }
                }

                public static string GetNameFromIdShieldingDone
                {
                    get { return "SELECT SourceNpcName FROM ShieldingDone WHERE SourceNpcId = @npcId AND EncounterId = @encounterId LIMIT 1"; }
                }

                public static string GetNameFromIdShieldingTaken
                {
                    get { return "SELECT TargetNpcName FROM ShieldingDone WHERE TargetNpcId = @npcId AND EncounterId = @encounterId LIMIT 1"; }
                }

                public static class Damage
                {
                    public static string RecordTable { get { return "DamageDone"; } }
                    public static string RecordTableAlias { get { return "DD"; } }
                    public static string SearchCharacter { get { return "Npc"; } }
                    public static string SearchEncounterId { get { return "@Id"; } }
                    public static string SearchCharacterId { get { return "@npcId"; } }
                    public static string SearchField { get { return "TotalDamage"; } }
                    #region 'Base' queries
                    public static string InteractionJoinSourcePlayer
                    {
                        get
                        {
                            return string.Format("FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                   "JOIN Encounter E ON {1}.EncounterId = E.Id JOIN Player P ON {1}.SourcePlayerId = P.Id ", RecordTable, RecordTableAlias);
                        }
                    }
                    public static string InteractionJoinTargetPlayer
                    {
                        get
                        {
                            return string.Format("FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                   "JOIN Encounter E ON {1}.EncounterId = E.Id JOIN Player P ON {1}.TargetPlayerId = P.Id ", RecordTable, RecordTableAlias);
                        }
                    }
                    public static string InteractionJoinNoPlayer
                    {
                        get
                        {
                            return string.Format("FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                   "JOIN Encounter E ON {1}.EncounterId = E.Id ", RecordTable, RecordTableAlias);
                        }
                    }
                    public static string InteractionBaseQueryTotals
                    {
                        get
                        {
                            return string.Format("COUNT(CASE WHEN {0}.CriticalHit = 1 THEN 1 END) AS Crits, " +
                                    "count(CASE WHEN {0}.CriticalHit = 0 THEN 1 END) AS Hits, " +
                                    "MAX({0}.TotalDamage) AS BiggestHit, AVG({0}.TotalDamage) AS AverageHit, " +
                                    "(SUM({0}.EffectiveDamage) + SUM({0}.AbsorbedAmount) + SUM({0}.BlockedAmount) " +
                                    "+ SUM({0}.InterceptedAmount) + SUM({0}.IgnoredAmount)) AS Total, " +
                                    "SUM({0}.BlockedAmount) AS Blocked, " +
                                    "SUM({0}.IgnoredAmount) AS Ignored, " +
                                    "SUM({0}.InterceptedAmount) AS Intercepted, " +
                                    "SUM({0}.AbsorbedAmount) AS Absorbed, " +
                                    "SUM({0}.OverkillAmount) AS Overkilled, " +
                                    "SUM({0}.EffectiveDamage) AS Effective, " +
                                    "((SUM(EffectiveDamage) + SUM(AbsorbedAmount) + SUM(BlockedAmount) " +
                                    "+ SUM(InterceptedAmount) + SUM(IgnoredAmount)) / {1}) AS Average, " +
                                    "(SUM(EffectiveDamage) / {1}) AS AverageEffective, " +
                                    "((100.0 * (SUM({0}.EffectiveDamage) + SUM({0}.AbsorbedAmount)) /  " +
                                    "(SELECT (SUM(EffectiveDamage) + SUM(AbsorbedAmount)) ",
                                    RecordTableAlias, SearchEncounterDuration);
                        }
                    }
                    public static string InteractionBaseQueryPerSecond
                    {
                        get
                        {
                            return string.Format("(SUM({0}.EffectiveDamage) + SUM({0}.AbsorbedAmount) + SUM({0}.BlockedAmount) + " +
                                   "SUM({0}.InterceptedAmount) + SUM({0}.IgnoredAmount)) AS Total, " +
                                   "SUM({0}.EffectiveDamage) AS Effective ", RecordTableAlias);
                        }
                    }
                    public static string BaseWhereIncoming
                    {
                        get
                        {
                            return string.Format("WHERE {0}.EncounterId = {1} AND {0}.Target{2}Id = {3} AND {4} > 0 ",
                                RecordTableAlias, SearchEncounterId, SearchCharacter, SearchCharacterId, SearchField);
                        }
                    }
                    public static string BaseWhereOutgoing
                    {
                        get
                        {
                            return string.Format("WHERE {0}.EncounterId = {1} AND {0}.Source{2}Id = {3} AND {4} > 0 ",
                                   RecordTableAlias, SearchEncounterId, SearchCharacter, SearchCharacterId, SearchField);
                        }
                    }
                    #endregion

                    public static class Done
                    {
                        public static class ByAbility
                        {
                            public static class OnlyPlayers
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {0}.SecondsElapsed, A.Name AS AbilityName, " +
                                                             InteractionBaseQueryPerSecond +
                                                             "FROM {1} {0} JOIN Ability A ON {0}.AbilityId = A.Id " +
                                                             BaseWhereOutgoing + "AND {0}.TargetPlayerId IS NOT NULL " +
                                                             "GROUP BY {0}.SecondsElapsed, A.Name " +
                                                             "ORDER BY {0}.SecondsElapsed, A.Name",
                                                             RecordTableAlias, RecordTable);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT A.Name AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
                                                             "FROM {0} WHERE EncounterId = {2} AND SourceNpcId = {3} AND TargetPlayerId IS NOT NULL))) AS Percentage " +
                                                             InteractionJoinNoPlayer + BaseWhereOutgoing + " AND {1}.TargetPlayerId IS NOT NULL " +
                                                             "GROUP BY A.Name, A.DamageType, A.Icon, E.Duration ORDER BY Effective DESC",
                                                             RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            public static class OtherNpcs
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, A.Name AS AbilityName, " +
                                            InteractionBaseQueryPerSecond +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                                 BaseWhereOutgoing + "AND {1}.TargetNpcId <> {2} GROUP BY {1}.SecondsElapsed, A.Name " +
                                                 "ORDER BY {1}.SecondsElapsed, A.Name", RecordTable, RecordTableAlias, SearchCharacterId);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT A.Name AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
                                                "FROM {0} WHERE EncounterId = {1} AND SourceNpcId = {2} AND TargetNpcId <> {2}))) AS Percentage " +
                                                InteractionJoinNoPlayer + BaseWhereOutgoing + " AND {3}.TargetNpcId <> {2} " +
                                               "GROUP BY A.Name, A.DamageType, A.Icon, E.Duration ORDER BY Effective DESC",
                                               RecordTable, SearchEncounterId, SearchCharacterId, RecordTableAlias);
                                    }
                                }
                            }

                            public static class OnlySelf
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, A.Name AS AbilityName, " +
                                            InteractionBaseQueryPerSecond +
                                            "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                            BaseWhereOutgoing + "AND {1}.TargetNpcId = {2} GROUP BY {1}.SecondsElapsed, A.Name " +
                                            "ORDER BY {1}.SecondsElapsed, AbilityName",
                                            RecordTable, RecordTableAlias, SearchCharacterId);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT A.Name AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
                                                "FROM {0} WHERE EncounterId = {1} AND TargetNpcId = {2} AND SourceNpcId = {2}))) AS Percentage " +
                                                InteractionJoinNoPlayer + BaseWhereOutgoing + " AND {3}.TargetNpcId = {2} " +
                                               "GROUP BY A.Name, A.DamageType, A.Icon, E.Duration ORDER BY Effective DESC",
                                               RecordTable, SearchEncounterId, SearchCharacterId, RecordTableAlias);
                                    }
                                }
                            }

                            public static class AllTargets
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, A.Name AS AbilityName, " +
                                               InteractionBaseQueryPerSecond +
                                               "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                               BaseWhereOutgoing + "AND {1}.TargetNpcId IS NOT NULL " +
                                               "GROUP BY {1}.SecondsElapsed, A.Name " +
                                               "UNION " +
                                               "SELECT {1}.SecondsElapsed, A.Name AS AbilityName, " +
                                               InteractionBaseQueryPerSecond +
                                               "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.TargetPlayerId = P.Id " +
                                               BaseWhereOutgoing + "AND {1}.TargetPlayerId IS NOT NULL " +
                                               "GROUP BY {1}.SecondsElapsed, A.Name " +
                                               "ORDER BY SecondsElapsed, AbilityName",
                                               RecordTable, RecordTableAlias);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT A.Name AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND SourceNpcId = {3} AND TargetNpcId IS NOT NULL))) AS Percentage " +
                                               InteractionJoinNoPlayer + BaseWhereOutgoing + " AND {1}.TargetNpcId IS NOT NULL " +
                                               "GROUP BY A.Name, A.DamageType, A.Icon, E.Duration " +
                                               "UNION " +
                                               "SELECT A.Name AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND SourceNpcId = {3} AND TargetPlayerId IS NOT NULL))) AS Percentage " +
                                               InteractionJoinTargetPlayer + BaseWhereOutgoing + " AND {1}.TargetPlayerId IS NOT NULL " +
                                               "GROUP BY A.Name, A.DamageType, A.Icon, E.Duration ORDER BY Effective DESC",
                                               RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }
                        }

                        public static class ByTarget
                        {
                            public static class OnlyPlayers
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, P.Name AS TargetName, P.Id AS TargetId, " +
                                            "(SUM({1}.EffectiveDamage) + SUM({1}.AbsorbedAmount) + SUM({1}.BlockedAmount) + " +
                                            "SUM({1}.InterceptedAmount) + SUM({1}.IgnoredAmount)) AS Total, " +
                                            "SUM({1}.EffectiveDamage) AS Effective " +
                                            InteractionJoinTargetPlayer +
                                            "WHERE {1}.EncounterId = {2} AND {1}.SourceNpcId = {3} AND {1}.{4} > 0 AND {1}.TargetPlayerId IS NOT NULL " +
                                            "GROUP BY {1}.SecondsElapsed, P.Name, P.Id " +
                                            "ORDER BY {1}.SecondsElapsed, P.Name",
                                            RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId, SearchField);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT P.Name AS TargetName," + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND SourceNpcId = {3} AND TargetPlayerId IS NOT NULL))) AS Percentage " +
                                               InteractionJoinTargetPlayer + BaseWhereOutgoing + " AND {1}.TargetPlayerId IS NOT NULL " +
                                               "GROUP BY P.Name, P.Id, E.Duration ORDER BY Effective DESC",
                                               RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            public static class OtherNpcs
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, {1}.TargetNpcName AS TargetName, {1}.TargetNpcId AS TargetId, " +
                                                 "(SUM({1}.EffectiveDamage) + SUM({1}.AbsorbedAmount) + SUM({1}.BlockedAmount) + " +
                                                 "SUM({1}.InterceptedAmount) + SUM({1}.IgnoredAmount)) AS Total, " +
                                                 "SUM({1}.EffectiveDamage) AS Effective " +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                                 "WHERE {1}.EncounterId = {2} AND {1}.SourceNpcId = {3} AND {1}.{4} > 0 AND {1}.TargetNpcId <> {3} " +
                                                 "GROUP BY {1}.SecondsElapsed, {1}.TargetNpcName, {1}.TargetNpcId " +
                                                 "ORDER BY {1}.SecondsElapsed, {1}.TargetNpcName",
                                                 RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId, SearchField);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.TargetNpcName AS TargetName," + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND SourceNpcId = {3} AND TargetNpcId IS NOT NULL))) AS Percentage " +
                                               InteractionJoinNoPlayer + BaseWhereOutgoing + " AND {1}.TargetNpcId <> {3} " +
                                               "GROUP BY {1}.TargetNpcName, {1}.TargetNpcId, E.Duration ORDER BY Effective DESC",
                                               RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            public static class AllTargets
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, {1}.TargetNpcName AS TargetName, {1}.TargetNpcId AS TargetId, " +
                                                 "(SUM({1}.EffectiveDamage) + SUM({1}.AbsorbedAmount) + SUM({1}.BlockedAmount) + SUM({1}.InterceptedAmount) + SUM({1}.IgnoredAmount)) AS Total, " +
                                                 "SUM({1}.EffectiveDamage) AS Effective " +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                                 "WHERE {1}.EncounterId = {2} AND {1}.SourceNpcId = {3} AND {1}.{4} > 0 AND {1}.TargetNpcId IS NOT NULL " +
                                                 "GROUP BY {1}.SecondsElapsed, {1}.TargetNpcName, {1}.TargetNpcId " +
                                                 "UNION " +
                                                 "SELECT {1}.SecondsElapsed, P.Name AS TargetName, CAST(P.Id AS CHAR(50)) AS TargetId, " +
                                                 "(SUM({1}.EffectiveDamage) + SUM({1}.AbsorbedAmount) + SUM({1}.BlockedAmount) + SUM({1}.InterceptedAmount) + SUM({1}.IgnoredAmount)) AS Total, " +
                                                 "SUM({1}.EffectiveDamage) AS Effective " +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.TargetPlayerId = P.Id " +
                                                 "WHERE {1}.EncounterId = {2} AND {1}.SourceNpcId = {3} AND {1}.{4} > 0 AND {1}.TargetPlayerId IS NOT NULL " +
                                                 "GROUP BY {1}.SecondsElapsed, P.Name, P.Id " +
                                                 "ORDER BY SecondsElapsed, TargetName",
                                                 RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId, SearchField);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.TargetNpcName AS TargetName," + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND SourceNpcId = {3}))) AS Percentage " +
                                               InteractionJoinNoPlayer + BaseWhereOutgoing + " AND {1}.TargetNpcId IS NOT NULL " +
                                               "GROUP BY {1}.TargetNpcName, {1}.TargetNpcId, E.Duration " +
                                               "UNION " +
                                               "SELECT P.Name AS TargetName," + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND SourceNpcId = {3}))) AS Percentage " +
                                               InteractionJoinTargetPlayer + BaseWhereOutgoing + " AND {1}.TargetPlayerId IS NOT NULL " +
                                               "GROUP BY P.Name, P.Id, E.Duration ORDER BY Effective DESC",
                                               RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }
                        }
                    }

                    public static class Taken
                    {
                        public static class ByAbility
                        {
                            public static class OnlyPlayers
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {0}.SecondsElapsed,  CONCAT(A.Name, ' (', P.Name, ')') AS AbilityName, " +
                                                             InteractionBaseQueryPerSecond +
                                                             "FROM {1} {0} JOIN Ability A ON {0}.AbilityId = A.Id JOIN Player P ON {0}.SourcePlayerId = P.Id " +
                                                             BaseWhereIncoming + "AND {0}.SourcePlayerId IS NOT NULL " +
                                                             "GROUP BY {0}.SecondsElapsed, A.Name, P.Name, P.Id " +
                                                             "ORDER BY {0}.SecondsElapsed, A.Name, P.Name, P.Id",
                                                             RecordTableAlias, RecordTable);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT  CONCAT(A.Name, ' (', P.Name, ')') AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND TargetNpcId = {3} AND SourcePlayerId IS NOT NULL))) AS Percentage " +
                                               InteractionJoinSourcePlayer + BaseWhereIncoming + " AND {1}.SourcePlayerId IS NOT NULL " +
                                               "GROUP BY P.Name, P.Id, A.Name, A.DamageType, A.Icon, E.Duration ORDER BY Total DESC",
                                               RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            public static class OtherNpcs
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, CONCAT(A.Name, ' (', {1}.SourceNpcName, ')') AS AbilityName, " +
                                            InteractionBaseQueryPerSecond +
                                            "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                            BaseWhereIncoming + "AND {1}.SourceNpcId <> {2} " +
                                            "GROUP BY {1}.SecondsElapsed, A.Name, {1}.SourceNpcName, {1}.SourceNpcId " +
                                            "ORDER BY {1}.SecondsElapsed, A.Name, {1}.SourceNpcName, {1}.SourceNpcId",
                                            RecordTable, RecordTableAlias, SearchCharacterId);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT CONCAT(A.Name, ' (', {1}.SourceNpcName, ')') AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
                                                "FROM {0} WHERE EncounterId = {2} AND TargetNpcId = {3} AND SourceNpcId <> {3}))) AS Percentage " +
                                                InteractionJoinNoPlayer + BaseWhereIncoming + " AND {1}.SourceNpcId <> {3} " +
                                                "GROUP BY {1}.SourceNpcName, {1}.SourceNpcId, A.Name, A.DamageType, A.Icon, E.Duration ORDER BY Total DESC",
                                                RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            public static class OnlySelf
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, A.Name AS AbilityName, " +
                                                 InteractionBaseQueryPerSecond +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                                 BaseWhereIncoming + "AND {1}.SourceNpcId = {2} GROUP BY {1}.SecondsElapsed, A.Name " +
                                                 "ORDER BY {1}.SecondsElapsed, AbilityName",
                                                 RecordTable, RecordTableAlias, SearchCharacterId);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT A.Name AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
                                                "FROM {0} WHERE EncounterId = {2} AND TargetNpcId = {3} AND SourceNpcId = {3}))) AS Percentage " +
                                                InteractionJoinNoPlayer + BaseWhereIncoming + " AND {1}.SourceNpcId = {3} " +
                                                "GROUP BY A.Name, A.DamageType, A.Icon, E.Duration ORDER BY Total DESC",
                                                RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            public static class AllSources
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, CONCAT(A.Name, ' (', {1}.SourceNpcName, ')') AS AbilityName, " +
                                                InteractionBaseQueryPerSecond +
                                                "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                                BaseWhereIncoming + "AND {1}.SourceNpcId IS NOT NULL " +
                                                "GROUP BY {1}.SecondsElapsed, A.Name, {1}.SourceNpcName, {1}.SourceNpcId " +
                                                "UNION " +
                                                "SELECT {1}.SecondsElapsed,  CONCAT(A.Name, ' (', P.Name, ')') AS AbilityName, " +
                                                InteractionBaseQueryPerSecond +
                                                "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.SourcePlayerId = P.Id " +
                                                BaseWhereIncoming + "AND {1}.SourcePlayerId IS NOT NULL " +
                                                "GROUP BY {1}.SecondsElapsed, A.Name, P.Name, P.Id " +
                                                "ORDER BY SecondsElapsed, AbilityName",
                                                RecordTable, RecordTableAlias);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT CONCAT(A.Name, ' (', {1}.SourceNpcName, ')') AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND TargetNpcId = {3} AND SourceNpcId IS NOT NULL))) AS Percentage " +
                                               InteractionJoinNoPlayer + BaseWhereIncoming + " AND {1}.SourceNpcId IS NOT NULL " +
                                               "GROUP BY {1}.SourceNpcName, {1}.SourceNpcId, A.Name, A.DamageType, A.Icon, E.Duration " +
                                               "UNION " +
                                               "SELECT  CONCAT(A.Name, ' (', P.Name, ')') AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND TargetNpcId = {3} AND SourcePlayerId IS NOT NULL))) AS Percentage " +
                                               InteractionJoinSourcePlayer + BaseWhereIncoming + " AND {1}.SourcePlayerId IS NOT NULL " +
                                               "GROUP BY P.Name, P.Id, A.Name, A.DamageType, A.Icon, E.Duration ORDER BY Total DESC",
                                                RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }
                        }

                        public static class BySource
                        {
                            public static class OnlyPlayers
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, P.Name AS SourceName, P.Id AS SourceId, " +
                                                 "(SUM({1}.EffectiveDamage) + SUM({1}.AbsorbedAmount) + SUM({1}.BlockedAmount) + " +
                                                 "SUM({1}.InterceptedAmount) + SUM({1}.IgnoredAmount)) AS Total, " +
                                                 "SUM({1}.EffectiveDamage) AS Effective " +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.SourcePlayerId = P.Id " +
                                                 "WHERE {1}.EncounterId = {2} AND {1}.TargetNpcId = {3} AND {1}.{4} > 0 AND {1}.SourcePlayerId IS NOT NULL " +
                                                 "GROUP BY {1}.SecondsElapsed, P.Name, P.Id " +
                                                 "ORDER BY {1}.SecondsElapsed, P.Name",
                                                RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId, SearchField);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT P.Name AS SourceName, P.Id AS SourceId, " + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND TargetNpcId = {3} AND SourcePlayerId IS NOT NULL))) AS Percentage " +
                                               InteractionJoinSourcePlayer + BaseWhereIncoming + " AND {1}.SourcePlayerId IS NOT NULL " +
                                               "GROUP BY P.Name, P.Id, E.Duration ORDER BY Effective DESC",
                                                RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            public static class OtherNpcs
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, {1}.SourceNpcName AS SourceName, {1}.SourceNpcId AS SourceId, " +
                                                 "(SUM({1}.EffectiveDamage) + SUM({1}.AbsorbedAmount) + SUM({1}.BlockedAmount) + SUM({1}.InterceptedAmount) + SUM({1}.IgnoredAmount)) AS Total, " +
                                                 "SUM({1}.EffectiveDamage) AS Effective " +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                                 "WHERE {1}.EncounterId = {2} AND {1}.TargetNpcId = {3} AND {1}.{4} > 0 AND {1}.SourceNpcId <> {3} " +
                                                 "GROUP BY {1}.SecondsElapsed, {1}.SourceNpcName, {1}.SourceNpcId " +
                                                 "ORDER BY {1}.SecondsElapsed, {1}.SourceNpcName",
                                                RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId, SearchField);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SourceNpcName AS SourceName, {1}.SourceNpcId AS SourceId, " + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND TargetNpcId = {3} AND SourceNpcId <> {3}))) AS Percentage " +
                                               InteractionJoinNoPlayer + BaseWhereIncoming + " AND {1}.SourceNpcId <> {3} " +
                                               "GROUP BY {1}.SourceNpcName, {1}.SourceNpcId, E.Duration ORDER BY Effective DESC",
                                                RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            public static class AllSources
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, {1}.SourceNpcName AS SourceName, {1}.SourceNpcId AS SourceId, " +
                                                 "(SUM({1}.EffectiveDamage) + SUM({1}.AbsorbedAmount) + SUM({1}.BlockedAmount) + " +
                                                 "SUM({1}.InterceptedAmount) + SUM({1}.IgnoredAmount)) AS Total, " +
                                                 "SUM({1}.EffectiveDamage) AS Effective " +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                                 "WHERE {1}.EncounterId = {2} AND {1}.TargetNpcId = {3} AND {1}.{4} > 0 AND {1}.SourceNpcId IS NOT NULL " +
                                                 "GROUP BY {1}.SecondsElapsed, {1}.SourceNpcName, {1}.SourceNpcId " +
                                                 "UNION " +
                                                 "SELECT {1}.SecondsElapsed, P.Name AS SourceName, CAST(P.Id AS CHAR(50)) AS SourceId, " +
                                                 "(SUM({1}.EffectiveDamage) + SUM({1}.AbsorbedAmount) + SUM({1}.BlockedAmount) + " +
                                                 "SUM({1}.InterceptedAmount) + SUM({1}.IgnoredAmount)) AS Total, " +
                                                 "SUM({1}.EffectiveDamage) AS Effective " +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.SourcePlayerId = P.Id " +
                                                 "WHERE {1}.EncounterId = {2} AND {1}.TargetNpcId = {3} AND {1}.{4} > 0 AND {1}.SourcePlayerId IS NOT NULL " +
                                                 "GROUP BY {1}.SecondsElapsed, P.Name, P.Id " +
                                                 "ORDER BY SecondsElapsed, SourceName",
                                                RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId, SearchField);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SourceNpcName AS SourceName, {1}.SourceNpcId AS SourceId," + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND TargetNpcId = {3}))) AS Percentage " +
                                               InteractionJoinNoPlayer + BaseWhereIncoming + " AND {1}.SourceNpcId IS NOT NULL " +
                                               "GROUP BY {1}.SourceNpcName, {1}.SourceNpcId, E.Duration " +
                                               "UNION " +
                                               "SELECT P.Name AS SourceName, CAST(P.Id AS CHAR(50)) AS SourceId," + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND TargetNpcId = {3}))) AS Percentage " +
                                               InteractionJoinSourcePlayer + BaseWhereIncoming + " AND {1}.SourcePlayerId IS NOT NULL " +
                                               "GROUP BY P.Name, P.Id, E.Duration ORDER BY Effective DESC",
                                                RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }
                        }

                        public static string Top25AbilitiesPlayers
                        {
                            get
                            {
                                return "SELECT CONCAT(A.Name, ' (', P.Name, ')') AS AbilityName " +
                                       "FROM DamageDone DD JOIN Ability A ON DD.AbilityId = A.Id JOIN Player P ON DD.SourcePlayerId = P.Id " +
                                       "WHERE DD.EncounterId = @Id AND DD.TargetNpcId = @npcId " +
                                       "AND DD.SourcePlayerId IS NOT NULL " +
                                       "GROUP BY A.Name, P.Name " +
                                       "ORDER BY SUM(DD.TotalDamage) DESC LIMIT 0,25";
                            }
                        }

                        public static string Top25AbilitiesNpcs
                        {
                            get
                            {
                                return "SELECT CONCAT(A.Name, ' (', DD.SourceNpcName, ')') AS AbilityName " +
                                       "FROM DamageDone DD JOIN Ability A ON DD.AbilityId = A.Id " +
                                       "WHERE DD.EncounterId = @Id AND DD.TargetNpcId = @npcId " +
                                       "AND DD.SourceNpcId IS NOT NULL " +
                                       "GROUP BY A.Name, DD.SourceNpcName " +
                                       "ORDER BY SUM(DD.TotalDamage) DESC LIMIT 0,25";
                            }
                        }

                        public static string Top25AbilitiesAllSources
                        {
                            get
                            {
                                return "SELECT X.AbilityName FROM (SELECT CONCAT(A.Name, ' (', P.Name, ')') AS AbilityName, " +
                                       "SUM(DD.TotalDamage) AS Total FROM DamageDone DD JOIN Ability A ON DD.AbilityId = A.Id " +
                                       "JOIN Player P ON DD.SourcePlayerId = P.Id " +
                                       "WHERE DD.EncounterId = @Id AND DD.TargetNpcId = @npcId " +
                                       "AND DD.SourcePlayerId IS NOT NULL " +
                                       "GROUP BY A.Name, P.Name " +
                                       "UNION " +
                                       "SELECT CONCAT(A.Name, ' (', DD.SourceNpcName, ')') AS AbilityName, " +
                                       "SUM(DD.TotalDamage) AS Total " +
                                       "FROM DamageDone DD JOIN Ability A ON DD.AbilityId = A.Id " +
                                       "WHERE DD.EncounterId = @Id AND DD.TargetNpcId = @npcId " +
                                       "AND DD.SourceNpcId IS NOT NULL GROUP BY A.Name, DD.SourceNpcName) X " +
                                       "ORDER BY X.Total DESC LIMIT 0,25";
                            }
                        }
                    }
                }

                public static class Healing
                {
                    public static string RecordTable { get { return "HealingDone"; } }
                    public static string RecordTableAlias { get { return "HD"; } }
                    public static string SearchCharacter { get { return "Npc"; } }
                    public static string SearchEncounterId { get { return "@Id"; } }
                    public static string SearchCharacterId { get { return "@npcId"; } }
                    public static string SearchField { get { return "TotalHealing"; } }
                    #region 'Base' queries
                    public static string InteractionJoinSourcePlayer
                    {
                        get
                        {
                            return string.Format("FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                   "JOIN Encounter E ON {1}.EncounterId = E.Id JOIN Player P ON {1}.SourcePlayerId = P.Id ", RecordTable, RecordTableAlias);
                        }
                    }
                    public static string InteractionJoinTargetPlayer
                    {
                        get
                        {
                            return string.Format("FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                   "JOIN Encounter E ON {1}.EncounterId = E.Id JOIN Player P ON {1}.TargetPlayerId = P.Id ", RecordTable, RecordTableAlias);
                        }
                    }
                    public static string InteractionJoinNoPlayer
                    {
                        get
                        {
                            return string.Format("FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                   "JOIN Encounter E ON {1}.EncounterId = E.Id ", RecordTable, RecordTableAlias);
                        }
                    }
                    public static string InteractionBaseQueryTotals
                    {
                        get
                        {
                            return string.Format("COUNT(CASE WHEN {0}.CriticalHit = 1 THEN 1 END) AS Crits, " +
                                    "count(CASE WHEN {0}.CriticalHit = 0 THEN 1 END) AS Hits, " +
                                    "MAX({0}.TotalHealing) AS BiggestHit, AVG({0}.TotalHealing) AS AverageHit, " +
                                    "SUM({0}.TotalHealing) AS Total, " +
                                    "SUM({0}.EffectiveHealing) AS Effective, " +
                                    "SUM({0}.OverhealAmount) AS Overhealing, " +
                                    "(SUM(TotalHealing) / {1}) AS Average, " +
                                    "(SUM(EffectiveHealing) / {1}) AS AverageEffective, " +
                                    "((100.0 * (SUM({0}.EffectiveHealing)) / (SELECT (SUM(EffectiveHealing)) ",
                                    RecordTableAlias, SearchEncounterDuration);
                        }
                    }
                    public static string InteractionBaseQueryPerSecond
                    {
                        get
                        {
                            return string.Format("SUM({0}.TotalHealing) AS Total, SUM({0}.EffectiveHealing) AS Effective ", RecordTableAlias);
                        }
                    }
                    public static string BaseWhereIncoming
                    {
                        get
                        {
                            return string.Format("WHERE {0}.EncounterId = {1} AND {0}.Target{2}Id = {3} AND {4} > 0 ",
                                RecordTableAlias, SearchEncounterId, SearchCharacter, SearchCharacterId, SearchField);
                        }
                    }
                    public static string BaseWhereOutgoing
                    {
                        get
                        {
                            return string.Format("WHERE {0}.EncounterId = {1} AND {0}.Source{2}Id = {3} AND {4} > 0 ",
                                   RecordTableAlias, SearchEncounterId, SearchCharacter, SearchCharacterId, SearchField);
                        }
                    }
                    #endregion

                    public static class Done
                    {
                        public static class ByAbility
                        {
                            public static class OnlyPlayers
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {0}.SecondsElapsed, A.Name AS AbilityName, " +
                                                             InteractionBaseQueryPerSecond +
                                                             "FROM {1} {0} JOIN Ability A ON {0}.AbilityId = A.Id " +
                                                             BaseWhereOutgoing + "AND {0}.TargetPlayerId IS NOT NULL " +
                                                             "GROUP BY {0}.SecondsElapsed, A.Name " +
                                                             "ORDER BY {0}.SecondsElapsed, A.Name",
                                                             RecordTableAlias, RecordTable);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT A.Name AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
                                                             "FROM {0} WHERE EncounterId = {2} AND SourceNpcId = {3} AND TargetPlayerId IS NOT NULL))) AS Percentage " +
                                                             InteractionJoinNoPlayer + BaseWhereOutgoing + " AND {1}.TargetPlayerId IS NOT NULL " +
                                                             "GROUP BY A.Name, A.DamageType, A.Icon, E.Duration ORDER BY Effective DESC",
                                                             RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            public static class OtherNpcs
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, A.Name AS AbilityName, " +
                                            InteractionBaseQueryPerSecond +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                                 BaseWhereOutgoing + "AND {1}.TargetNpcId <> {2} GROUP BY {1}.SecondsElapsed, A.Name " +
                                                 "ORDER BY {1}.SecondsElapsed, A.Name", RecordTable, RecordTableAlias, SearchCharacterId);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT A.Name AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
                                                "FROM {0} WHERE EncounterId = {1} AND SourceNpcId = {2} AND TargetNpcId <> {2}))) AS Percentage " +
                                                InteractionJoinNoPlayer + BaseWhereOutgoing + " AND {3}.TargetNpcId <> {2} " +
                                               "GROUP BY A.Name, A.DamageType, A.Icon, E.Duration ORDER BY Effective DESC",
                                               RecordTable, SearchEncounterId, SearchCharacterId, RecordTableAlias);
                                    }
                                }
                            }

                            public static class OnlySelf
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, A.Name AS AbilityName, " +
                                            InteractionBaseQueryPerSecond +
                                            "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                            BaseWhereOutgoing + "AND {1}.TargetNpcId = {2} GROUP BY {1}.SecondsElapsed, A.Name " +
                                            "ORDER BY {1}.SecondsElapsed, AbilityName",
                                            RecordTable, RecordTableAlias, SearchCharacterId);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT A.Name AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
                                                "FROM {0} WHERE EncounterId = {1} AND TargetNpcId = {2} AND SourceNpcId = {2}))) AS Percentage " +
                                                InteractionJoinNoPlayer + BaseWhereOutgoing + " AND {3}.TargetNpcId = {2} " +
                                               "GROUP BY A.Name, A.DamageType, A.Icon, E.Duration ORDER BY Effective DESC",
                                               RecordTable, SearchEncounterId, SearchCharacterId, RecordTableAlias);
                                    }
                                }
                            }

                            public static class AllTargets
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, A.Name AS AbilityName, " +
                                               InteractionBaseQueryPerSecond +
                                               "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                               BaseWhereOutgoing + "AND {1}.TargetNpcId IS NOT NULL " +
                                               "GROUP BY {1}.SecondsElapsed, A.Name " +
                                               "UNION " +
                                               "SELECT {1}.SecondsElapsed, A.Name AS AbilityName, " +
                                               InteractionBaseQueryPerSecond +
                                               "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                               BaseWhereOutgoing + "AND {1}.TargetPlayerId IS NOT NULL " +
                                               "GROUP BY {1}.SecondsElapsed, A.Name " +
                                               "ORDER BY SecondsElapsed, AbilityName",
                                               RecordTable, RecordTableAlias);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT A.Name AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND SourceNpcId = {3} AND TargetNpcId IS NOT NULL))) AS Percentage " +
                                               InteractionJoinNoPlayer + BaseWhereOutgoing + " AND {1}.TargetNpcId IS NOT NULL " +
                                               "GROUP BY A.Name, A.DamageType, A.Icon, E.Duration " +
                                               "UNION " +
                                               "SELECT A.Name AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND SourceNpcId = {3} AND TargetPlayerId IS NOT NULL))) AS Percentage " +
                                               InteractionJoinSourcePlayer + BaseWhereOutgoing + " AND {1}.TargetPlayerId IS NOT NULL " +
                                               "GROUP BY A.Name, A.DamageType, A.Icon, E.Duration ORDER BY Effective DESC",
                                               RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }
                        }

                        public static class ByTarget
                        {
                            public static class OtherNpcs
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, {1}.TargetNpcName AS TargetName, {1}.TargetNpcId AS TargetId, " +
                                            InteractionBaseQueryPerSecond +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                                 BaseWhereOutgoing + "AND {1}.TargetNpcId <> {2} GROUP BY {1}.SecondsElapsed, {1}.TargetNpcName, {1}.TargetNpcId " +
                                                 "ORDER BY {1}.SecondsElapsed, {1}.TargetNpcName", RecordTable, RecordTableAlias, SearchCharacterId);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.TargetNpcName AS TargetName," + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND SourceNpcId = {3} AND TargetNpcId <> {3}))) AS Percentage " +
                                               InteractionJoinNoPlayer + BaseWhereOutgoing + " AND {1}.TargetNpcId <> {3} " +
                                               "GROUP BY {1}.TargetNpcName, {1}.TargetNpcId, E.Duration ORDER BY Effective DESC",
                                               RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            public static class OnlyPlayers
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, P.Name AS TargetName, P.Id AS TargetId, " +
                                            InteractionBaseQueryPerSecond +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.TargetPlayerId = P.Id " +
                                                 BaseWhereOutgoing + "AND {1}.TargetPlayerId IS NOT NULL {2} GROUP BY {1}.SecondsElapsed, P.Name, P.Id " +
                                                 "ORDER BY {1}.SecondsElapsed, P.Name", RecordTable, RecordTableAlias, SearchCharacterId);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT P.Name AS TargetName," + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND SourceNpcId = {3} AND TargetPlayerId IS NOT NULL))) AS Percentage " +
                                               InteractionJoinTargetPlayer + BaseWhereOutgoing + " AND {1}.TargetPlayerId IS NOT NULL " +
                                               "GROUP BY P.Name, P.Id, E.Duration ORDER BY Effective DESC",
                                               RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            public static class AllTargets
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, {1}.TargetNpcName AS TargetName, {1}.TargetNpcId AS TargetId, " +
                                                 "SUM({1}.TotalHealing) AS Total, " +
                                                 "SUM({1}.EffectiveHealing) AS Effective, " +
                                                 "SUM({1}.OverhealAmount) AS Overhealing " +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                                 "WHERE {1}.EncounterId = {2} AND {1}.SourceNpcId = {3} AND {1}.{4} > 0 AND {1}.TargetNpcId IS NOT NULL " +
                                                 "GROUP BY {1}.SecondsElapsed, {1}.TargetNpcName, {1}.TargetNpcId " +
                                                 "UNION " +
                                                 "SELECT {1}.SecondsElapsed, P.Name AS TargetName, CAST(P.Id AS CHAR(50)) AS TargetId, " +
                                                 "SUM({1}.TotalHealing) AS Total, " +
                                                 "SUM({1}.EffectiveHealing) AS Effective, " +
                                                 "SUM({1}.OverhealAmount) AS Overhealing " +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.TargetPlayerId = P.Id " +
                                                 "WHERE {1}.EncounterId = {2} AND {1}.SourceNpcId = {3} AND {1}.{4} > 0 AND {1}.TargetPlayerId IS NOT NULL " +
                                                 "GROUP BY {1}.SecondsElapsed, P.Name, P.Id " +
                                                 "ORDER BY SecondsElapsed, TargetName",
                                                 RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId, SearchField);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.TargetNpcName AS TargetName," + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND SourceNpcId = {3}))) AS Percentage " +
                                               InteractionJoinNoPlayer + BaseWhereOutgoing + " AND {1}.TargetNpcId IS NOT NULL " +
                                               "GROUP BY {1}.TargetNpcName, {1}.TargetNpcId, E.Duration " +
                                               "UNION " +
                                               "SELECT P.Name AS TargetName," + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND SourceNpcId = {3}))) AS Percentage " +
                                               InteractionJoinTargetPlayer + BaseWhereOutgoing + " AND {1}.TargetPlayerId IS NOT NULL " +
                                               "GROUP BY P.Name, P.Id, E.Duration ORDER BY Effective DESC",
                                               RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                        }
                    }

                    public static class Taken
                    {
                        public static class ByAbility
                        {
                            public static class OtherNpcs
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {0}.SecondsElapsed, CONCAT(A.Name, ' (', {0}.SourceNpcName, ')') AS AbilityName, " +
                                                             InteractionBaseQueryPerSecond +
                                                             "FROM {1} {0} JOIN Ability A ON {0}.AbilityId = A.Id " +
                                                             BaseWhereIncoming + "AND {0}.SourceNpcId <> {2} " +
                                                             "GROUP BY {0}.SecondsElapsed, A.Name, {0}.SourceNpcName, {0}.SourceNpcId " +
                                                             "ORDER BY {0}.SecondsElapsed, A.Name, {0}.SourceNpcName, {0}.SourceNpcId",
                                                             RecordTableAlias, RecordTable, SearchCharacterId);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT CONCAT(A.Name, ' (', {1}.SourceNpcName, ')') AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND TargetNpcId = {3} AND SourceNpcId <> {3}))) AS Percentage " +
                                               InteractionJoinNoPlayer + BaseWhereIncoming + " AND {1}.SourceNpcId IS NOT NULL " +
                                               "GROUP BY {1}.SourceNpcName, {1}.SourceNpcId, A.Name, A.DamageType, A.Icon, E.Duration ORDER BY Effective DESC",
                                               RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            public static class OnlyPlayers
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, CONCAT(A.Name, ' (', P.Name, ')') AS AbilityName, " +
                                            InteractionBaseQueryPerSecond +
                                            "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.SourcePlayerId = P.Id " +
                                            BaseWhereIncoming + "AND {1}.SourcePlayerId IS NOT NULL " +
                                            "GROUP BY {1}.SecondsElapsed, A.Name, P.Name, P.Id " +
                                            "ORDER BY {1}.SecondsElapsed, A.Name, P.Name, P.Id",
                                            RecordTable, RecordTableAlias);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT CONCAT(A.Name, ' (', P.Name, ')') AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
                                                "FROM {0} WHERE EncounterId = {2} AND TargetNpcId = {3} AND SourcePlayerId IS NOT NULL))) AS Percentage " +
                                                InteractionJoinSourcePlayer + BaseWhereIncoming + " AND {1}.SourcePlayerId IS NOT NULL " +
                                                "GROUP BY P.Name, P.Id, A.Name, A.DamageType, A.Icon, E.Duration ORDER BY Effective DESC",
                                                RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            public static class OnlySelf
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, A.Name AS AbilityName, " +
                                                 InteractionBaseQueryPerSecond +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                                 BaseWhereIncoming + "AND {1}.SourceNpcId = {2} GROUP BY {1}.SecondsElapsed, A.Name " +
                                                 "ORDER BY {1}.SecondsElapsed, AbilityName",
                                                 RecordTable, RecordTableAlias, SearchCharacterId);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT A.Name AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
                                                "FROM {0} WHERE EncounterId = {2} AND TargetNpcId = {3} AND SourceNpcId = {3}))) AS Percentage " +
                                                InteractionJoinNoPlayer + BaseWhereIncoming + " AND {1}.SourceNpcId = {3} " +
                                                "GROUP BY {1}.SourceNpcName, {1}.SourceNpcId, A.Name, A.DamageType, A.Icon, E.Duration ORDER BY Effective DESC",
                                                RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            public static class AllSources
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, CONCAT(A.Name, ' (', {1}.SourceNpcName, ')') AS AbilityName, " +
                                                InteractionBaseQueryPerSecond +
                                                "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                                BaseWhereIncoming + "AND {1}.SourceNpcId IS NOT NULL " +
                                                "GROUP BY {1}.SecondsElapsed, A.Name, {1}.SourceNpcName, {1}.SourceNpcId " +
                                                "UNION " +
                                                "SELECT {1}.SecondsElapsed, CONCAT(A.Name, ' (', P.Name, ')') AS AbilityName, " +
                                                InteractionBaseQueryPerSecond +
                                                "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.SourcePlayerId = P.Id " +
                                                BaseWhereIncoming + "AND {1}.SourcePlayerId IS NOT NULL " +
                                                "GROUP BY {1}.SecondsElapsed, A.Name, P.Name, P.Id " +
                                                "ORDER BY SecondsElapsed, AbilityName",
                                                RecordTable, RecordTableAlias);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT CONCAT(A.Name, ' (', {1}.SourceNpcName, ')') AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND TargetNpcId = {3} AND SourceNpcId IS NOT NULL))) AS Percentage " +
                                               InteractionJoinNoPlayer + BaseWhereIncoming + " AND {1}.SourceNpcId IS NOT NULL " +
                                               "GROUP BY {1}.SourceNpcName, {1}.SourceNpcId, A.Name, A.DamageType, A.Icon, E.Duration " +
                                               "UNION " +
                                               "SELECT CONCAT(A.Name, ' (', P.Name, ')') AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND TargetNpcId = {3} AND SourcePlayerId IS NOT NULL))) AS Percentage " +
                                               InteractionJoinSourcePlayer + BaseWhereIncoming + " AND {1}.SourcePlayerId IS NOT NULL " +
                                               "GROUP BY P.Name, P.Id, A.Name, A.DamageType, A.Icon, E.Duration ORDER BY Effective DESC",
                                                RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }
                        }

                        public static class BySource
                        {
                            public static class OtherNpcs
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, {1}.SourceNpcName AS SourceName, {1}.SourceNpcId AS SourceId, " +
                                            InteractionBaseQueryPerSecond +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                                 BaseWhereIncoming + "AND {1}.SourceNpcId <> {2} GROUP BY {1}.SecondsElapsed, {1}.SourceNpcName, {1}.SourceNpcId " +
                                                 "ORDER BY {1}.SecondsElapsed, {1}.SourceNpcName", RecordTable, RecordTableAlias, SearchCharacterId);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SourceNpcName AS SourceName," + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND TargetNpcId = {3} AND SourceNpcId <> {3}))) AS Percentage " +
                                               InteractionJoinNoPlayer + BaseWhereIncoming + " AND {1}.SourceNpcId <> {3} " +
                                               "GROUP BY {1}.SourceNpcName, {1}.SourceNpcId, E.Duration ORDER BY Effective DESC",
                                                RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            public static class OnlyPlayers
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, P.Name AS SourceName, P.Id AS SourceId, " +
                                            InteractionBaseQueryPerSecond +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.SourcePlayerId = P.Id " +
                                                 BaseWhereIncoming + "AND {1}.SourcePlayerId IS NOT NULL GROUP BY {1}.SecondsElapsed, P.Name, P.Id " +
                                                 "ORDER BY {1}.SecondsElapsed, P.Name", RecordTable, RecordTableAlias);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT P.Name AS SourceName," + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND TargetNpcId = {3} AND SourcePlayerId IS NOT NULL))) AS Percentage " +
                                               InteractionJoinSourcePlayer + BaseWhereIncoming + " AND {1}.SourcePlayerId IS NOT NULL " +
                                               "GROUP BY P.Name, P.Id, E.Duration ORDER BY Effective DESC",
                                                RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            public static class AllSources
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format(
                                            "SELECT {1}.SecondsElapsed, {1}.SourceNpcName AS SourceName, {1}.SourceNpcId AS SourceId, " + InteractionBaseQueryPerSecond +
                                            "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                            BaseWhereIncoming + "AND {1}.SourceNpcId IS NOT NULL GROUP BY {1}.SecondsElapsed, {1}.SourceNpcName, {1}.SourceNpcId " +
                                            "UNION " +
                                            "SELECT {1}.SecondsElapsed, P.Name AS SourceName, CAST(P.Id AS CHAR(50)) AS SourceId, " + InteractionBaseQueryPerSecond +
                                            "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.SourcePlayerId = P.Id " +
                                            BaseWhereIncoming + "AND {1}.SourcePlayerId IS NOT NULL GROUP BY {1}.SecondsElapsed, P.Name, P.Id " +
                                            "ORDER BY SecondsElapsed, SourceName",
                                            RecordTable, RecordTableAlias, SearchCharacterId);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SourceNpcName AS SourceName," + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND TargetNpcId = {3}))) AS Percentage " +
                                               InteractionJoinNoPlayer + BaseWhereIncoming + " AND {1}.SourceNpcId IS NOT NULL " +
                                               "GROUP BY {1}.SourceNpcName, {1}.SourceNpcId, E.Duration " +
                                               "UNION " +
                                               "SELECT P.Name AS SourceName," + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND TargetNpcId = {3}))) AS Percentage " +
                                               InteractionJoinSourcePlayer + BaseWhereIncoming + " AND {1}.SourcePlayerId IS NOT NULL " +
                                               "GROUP BY P.Name, P.Id, E.Duration ORDER BY Effective DESC",
                                                RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }
                        }
                    }
                }

                public static class Shielding
                {
                    public static string RecordTable { get { return "ShieldingDone"; } }
                    public static string RecordTableAlias { get { return "SD"; } }
                    public static string SearchCharacter { get { return "Npc"; } }
                    public static string SearchEncounterId { get { return "@Id"; } }
                    public static string SearchCharacterId { get { return "@npcId"; } }
                    public static string SearchField { get { return "ShieldValue"; } }
                    #region 'Base' queries
                    public static string InteractionJoinSourcePlayer
                    {
                        get
                        {
                            return string.Format("FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                   "JOIN Encounter E ON {1}.EncounterId = E.Id JOIN Player P ON {1}.SourcePlayerId = P.Id ", RecordTable, RecordTableAlias);
                        }
                    }
                    public static string InteractionJoinTargetPlayer
                    {
                        get
                        {
                            return string.Format("FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                   "JOIN Encounter E ON {1}.EncounterId = E.Id JOIN Player P ON {1}.TargetPlayerId = P.Id ", RecordTable, RecordTableAlias);
                        }
                    }
                    public static string InteractionJoinNoPlayer
                    {
                        get
                        {
                            return string.Format("FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                   "JOIN Encounter E ON {1}.EncounterId = E.Id ", RecordTable, RecordTableAlias);
                        }
                    }
                    public static string InteractionBaseQueryTotals
                    {
                        get
                        {
                            return string.Format("COUNT(CASE WHEN {0}.CriticalHit = 1 THEN 1 END) AS Crits, " +
                                    "count(CASE WHEN {0}.CriticalHit = 0 THEN 1 END) AS Hits, " +
                                    "MAX({0}.ShieldValue) AS BiggestHit, AVG({0}.ShieldValue) AS AverageHit, " +
                                    "SUM({0}.ShieldValue) AS Total, " +
                                    "(SUM(ShieldValue) / {1}) AS Average, " +
                                    "((100.0 * (SUM({0}.ShieldValue)) / (SELECT (SUM(ShieldValue)) ",
                                    RecordTableAlias, SearchEncounterDuration);
                        }
                    }
                    public static string InteractionBaseQueryPerSecond
                    {
                        get
                        {
                            return string.Format("SUM({0}.ShieldValue) AS Total ", RecordTableAlias);
                        }
                    }
                    public static string BaseWhereIncoming
                    {
                        get
                        {
                            return string.Format("WHERE {0}.EncounterId = {1} AND {0}.Target{2}Id = {3} AND {4} > 0 ",
                                RecordTableAlias, SearchEncounterId, SearchCharacter, SearchCharacterId, SearchField);
                        }
                    }
                    public static string BaseWhereOutgoing
                    {
                        get
                        {
                            return string.Format("WHERE {0}.EncounterId = {1} AND {0}.Source{2}Id = {3} AND {4} > 0 ",
                                   RecordTableAlias, SearchEncounterId, SearchCharacter, SearchCharacterId, SearchField);
                        }
                    }
                    #endregion

                    /// <summary>
                    /// Outgoing shielding
                    /// </summary>
                    public static class Done
                    {
                        public static class ByAbility
                        {
                            public static class OtherNpcs
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {0}.SecondsElapsed, A.Name AS AbilityName, " +
                                                             InteractionBaseQueryPerSecond +
                                                             "FROM {1} {0} JOIN Ability A ON {0}.AbilityId = A.Id " +
                                                             BaseWhereOutgoing + "AND {0}.TargetNpcId <> {2} " +
                                                             "GROUP BY {0}.SecondsElapsed, A.Name " +
                                                             "ORDER BY {0}.SecondsElapsed, A.Name",
                                                             RecordTableAlias, RecordTable, SearchCharacterId);
                                    }
                                }
                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT A.Name AS AbilityName, A.Icon, " + InteractionBaseQueryTotals +
                                                             "FROM {0} WHERE EncounterId = {2} AND SourceNpcId = {3} AND TargetNpcId <> {3}))) AS Percentage " +
                                                             InteractionJoinNoPlayer + BaseWhereOutgoing + " AND {1}.TargetNpcId <> {3} " +
                                                             "GROUP BY A.Name, A.Icon, E.Duration ORDER BY ShieldValue DESC",
                                                             RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            public static class OnlyPlayers
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, A.Name AS AbilityName, " +
                                            InteractionBaseQueryPerSecond +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                                 BaseWhereOutgoing + "AND {1}.TargetPlayerId IS NOT NULL GROUP BY {1}.SecondsElapsed, A.Name " +
                                                 "ORDER BY {1}.SecondsElapsed, A.Name", RecordTable, RecordTableAlias, SearchCharacterId);
                                    }
                                }
                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT A.Name AS AbilityName, A.Icon, " + InteractionBaseQueryTotals +
                                                "FROM {0} WHERE EncounterId = {1} AND SourceNpcId = {2} AND TargetPlayerId IS NOT NULL))) AS Percentage " +
                                                InteractionJoinNoPlayer + BaseWhereOutgoing + " AND {3}.TargetPlayerId IS NOT NULL " +
                                               "GROUP BY A.Name, A.Icon, E.Duration ORDER BY Total DESC",
                                               RecordTable, SearchEncounterId, SearchCharacterId, RecordTableAlias);
                                    }
                                }
                            }

                            public static class OnlySelf
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, A.Name AS AbilityName, " +
                                            InteractionBaseQueryPerSecond +
                                            "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                            BaseWhereOutgoing + "AND {1}.TargetNpcId = {2} GROUP BY {1}.SecondsElapsed, A.Name " +
                                            "ORDER BY {1}.SecondsElapsed, AbilityName",
                                            RecordTable, RecordTableAlias, SearchCharacterId);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT A.Name AS AbilityName, A.Icon, " + InteractionBaseQueryTotals +
                                                "FROM {0} WHERE EncounterId = {1} AND TargetNpcId = {2} AND SourceNpcId = {2}))) AS Percentage " +
                                                InteractionJoinNoPlayer + BaseWhereOutgoing + " AND {3}.TargetNpcId = {2} " +
                                               "GROUP BY A.Name, A.Icon, E.Duration ORDER BY Total DESC",
                                               RecordTable, SearchEncounterId, SearchCharacterId, RecordTableAlias);
                                    }
                                }
                            }

                            public static class AllTargets
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, A.Name AS AbilityName, " +
                                               InteractionBaseQueryPerSecond +
                                               "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                               BaseWhereOutgoing + "AND {1}.TargetNpcId IS NOT NULL " +
                                               "GROUP BY {1}.SecondsElapsed, A.Name " +
                                               "UNION " +
                                               "SELECT {1}.SecondsElapsed, A.Name AS AbilityName, " +
                                               InteractionBaseQueryPerSecond +
                                               "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                               BaseWhereOutgoing + "AND {1}.TargetPlayerId IS NOT NULL " +
                                               "GROUP BY {1}.SecondsElapsed, A.Name " +
                                               "ORDER BY SecondsElapsed, AbilityName",
                                               RecordTable, RecordTableAlias);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT A.Name AS AbilityName, A.Icon, " + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND SourceNpcId = {3} AND TargetNpcId IS NOT NULL))) AS Percentage " +
                                               InteractionJoinNoPlayer + BaseWhereOutgoing + " AND {1}.TargetNpcId IS NOT NULL " +
                                               "GROUP BY A.Name, A.Icon, E.Duration " +
                                               "UNION " +
                                               "SELECT A.Name AS AbilityName, A.Icon, " + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND SourceNpcId = {3} AND TargetPlayerId IS NOT NULL))) AS Percentage " +
                                               InteractionJoinSourcePlayer + BaseWhereOutgoing + " AND {1}.TargetPlayerId IS NOT NULL " +
                                               "GROUP BY A.Name, A.Icon, E.Duration ORDER BY Total DESC",
                                               RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }
                        }

                        public static class ByTarget
                        {
                            public static class OtherNpcs
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, {1}.TargetNpcName AS TargetName, {1}.TargetNpcId AS TargetId, " +
                                            InteractionBaseQueryPerSecond +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                                 BaseWhereOutgoing + "AND {1}.TargetNpcId <> {2} GROUP BY {1}.SecondsElapsed, {1}.TargetNpcName, {1}.TargetNpcId " +
                                                 "ORDER BY {1}.SecondsElapsed, {1}.TargetNpcName", RecordTable, RecordTableAlias, SearchCharacterId);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.TargetNpcName AS TargetName," + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND SourceNpcId = {3} AND TargetNpcId <> {3}))) AS Percentage " +
                                               InteractionJoinNoPlayer + BaseWhereOutgoing + " AND {1}.TargetNpcId <> {3} " +
                                               "GROUP BY {1}.TargetNpcName, {1}.TargetNpcId, E.Duration ORDER BY Total DESC",
                                               RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            public static class OnlyPlayers
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, P.Name AS TargetName, P.Id AS TargetId, " +
                                            InteractionBaseQueryPerSecond +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.TargetPlayerId = P.Id " +
                                                 BaseWhereOutgoing + "AND {1}.TargetPlayerId IS NOT NULL GROUP BY {1}.SecondsElapsed, P.Name, P.Id " +
                                                 "ORDER BY {1}.SecondsElapsed, P.Name", RecordTable, RecordTableAlias);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT P.Name AS TargetName," + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND SourceNpcId = {3} AND TargetPlayerId IS NOT NULL))) AS Percentage " +
                                               InteractionJoinTargetPlayer + BaseWhereOutgoing + " AND {1}.TargetPlayerId IS NOT NULL " +
                                               "GROUP BY P.Name, P.Id, E.Duration ORDER BY Total DESC",
                                               RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            public static class AllTargets
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, {1}.TargetNpcName AS TargetName, {1}.TargetNpcId AS TargetId, " +
                                                 "SUM({1}.ShieldValue) AS Total " +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                                 "WHERE {1}.EncounterId = {2} AND {1}.SourceNpcId = {3} AND {1}.{4} > 0 AND {1}.TargetNpcId IS NOT NULL " +
                                                 "GROUP BY {1}.SecondsElapsed, {1}.TargetNpcName, {1}.TargetNpcId " +
                                                 "UNION " +
                                                 "SELECT {1}.SecondsElapsed, P.Name AS TargetName, CAST(P.Id AS CHAR(50)) AS TargetId, " +
                                                 "SUM({1}.ShieldValue) AS Total " +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.TargetPlayerId = P.Id " +
                                                 "WHERE {1}.EncounterId = {2} AND {1}.SourceNpcId = {3} AND {1}.{4} > 0 AND {1}.TargetPlayerId IS NOT NULL " +
                                                 "GROUP BY {1}.SecondsElapsed, P.Name, P.Id " +
                                                 "ORDER BY SecondsElapsed, TargetName",
                                                 RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId, SearchField);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.TargetNpcName AS TargetName," + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND SourceNpcId = {3}))) AS Percentage " +
                                               InteractionJoinNoPlayer + BaseWhereOutgoing + " AND {1}.TargetNpcId IS NOT NULL " +
                                               "GROUP BY {1}.TargetNpcName, {1}.TargetNpcId, E.Duration " +
                                               "UNION " +
                                               "SELECT P.Name AS TargetName," + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND SourceNpcId = {3}))) AS Percentage " +
                                               InteractionJoinTargetPlayer + BaseWhereOutgoing + " AND {1}.TargetPlayerId IS NOT NULL " +
                                               "GROUP BY P.Name, P.Id, E.Duration ORDER BY Total DESC",
                                               RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }
                        }
                    }

                    public static class Taken
                    {
                        public static class ByAbility
                        {
                            public static class OtherNpcs
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {0}.SecondsElapsed, CONCAT(A.Name, ' (', {0}.SourceNpcName, ')') AS AbilityName, " +
                                                             InteractionBaseQueryPerSecond +
                                                             "FROM {1} {0} JOIN Ability A ON {0}.AbilityId = A.Id " +
                                                             BaseWhereIncoming + "AND {0}.SourceNpcId <> {2} " +
                                                             "GROUP BY {0}.SecondsElapsed, A.Name, {0}.SourceNpcName, {0}.SourceNpcId " +
                                                             "ORDER BY {0}.SecondsElapsed, A.Name, {0}.SourceNpcName, {0}.SourceNpcId",
                                                             RecordTableAlias, RecordTable, SearchCharacterId);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT CONCAT(A.Name, ' (', {1}.SourceNpcName, ')') AS AbilityName, A.Icon, " + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND TargetNpcId = {3} AND SourceNpcId <> {3}))) AS Percentage " +
                                               InteractionJoinNoPlayer + BaseWhereIncoming + " AND {1}.SourceNpcId <> {3} " +
                                               "GROUP BY {1}.SourceNpcName, {1}.SourceNpcId, A.Name, A.Icon, E.Duration ORDER BY Total DESC",
                                               RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            public static class OnlyPlayers
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, CONCAT(A.Name, ' (', P.Name, ')') AS AbilityName, " +
                                            InteractionBaseQueryPerSecond +
                                            "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.SourcePlayerId = P.Id " +
                                            BaseWhereIncoming + "AND {1}.SourcePlayerId IS NOT NULL " +
                                            "GROUP BY {1}.SecondsElapsed, A.Name, P.Name, P.Id " +
                                            "ORDER BY {1}.SecondsElapsed, A.Name, P.Name, P.Id",
                                            RecordTable, RecordTableAlias, SearchCharacterId);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT CONCAT(A.Name, ' (', P.Name, ')') AS AbilityName, A.Icon, " + InteractionBaseQueryTotals +
                                                "FROM {0} WHERE EncounterId = {2} AND TargetNpcId = {3} AND SourcePlayerId IS NOT NULL))) AS Percentage " +
                                                InteractionJoinSourcePlayer + BaseWhereIncoming + " AND {1}.SourcePlayerId IS NOT NULL " +
                                                "GROUP BY P.Name, P.Id, A.Name, A.Icon, E.Duration ORDER BY Total DESC",
                                                RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            public static class OnlySelf
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, A.Name AS AbilityName, " +
                                                 InteractionBaseQueryPerSecond +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                                 BaseWhereIncoming + "AND {1}.SourceNpcId = {2} GROUP BY {1}.SecondsElapsed, A.Name " +
                                                 "ORDER BY {1}.SecondsElapsed, AbilityName",
                                                 RecordTable, RecordTableAlias, SearchCharacterId);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT CONCAT(A.Name, ' (', {1}.SourceNpcName, ')') AS AbilityName, A.Icon, " + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND TargetNpcId = {3} AND SourceNpcId = {3}))) AS Percentage " +
                                               InteractionJoinNoPlayer + BaseWhereIncoming + " AND {1}.SourceNpcId = {3} " +
                                               "GROUP BY {1}.SourceNpcName, {1}.SourceNpcId, A.Name, A.Icon, E.Duration ORDER BY Total DESC",
                                               RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            public static class AllSources
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, CONCAT(A.Name, ' (', {1}.SourceNpcName, ')') AS AbilityName, " +
                                                InteractionBaseQueryPerSecond +
                                                "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                                BaseWhereIncoming + "AND {1}.SourceNpcId IS NOT NULL " +
                                                "GROUP BY {1}.SecondsElapsed, A.Name, {1}.SourceNpcName, {1}.SourceNpcId " +
                                                "UNION " +
                                                "SELECT {1}.SecondsElapsed, CONCAT(A.Name, ' (', P.Name, ')') AS AbilityName, " +
                                                InteractionBaseQueryPerSecond +
                                                "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.SourcePlayerId = P.Id " +
                                                BaseWhereIncoming + "AND {1}.SourcePlayerId IS NOT NULL " +
                                                "GROUP BY {1}.SecondsElapsed, A.Name, P.Name, P.Id " +
                                                "ORDER BY SecondsElapsed, AbilityName",
                                                RecordTable, RecordTableAlias);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT CONCAT(A.Name, ' (', {1}.SourceNpcName, ')') AS AbilityName, A.Icon, " + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND TargetNpcId = {3} AND SourceNpcId IS NOT NULL))) AS Percentage " +
                                               InteractionJoinNoPlayer + BaseWhereIncoming + " AND {1}.SourceNpcId IS NOT NULL " +
                                               "GROUP BY {1}.SourceNpcName, {1}.SourceNpcId, A.Name, A.Icon, E.Duration " +
                                               "UNION " +
                                               "SELECT CONCAT(A.Name, ' (', P.Name, ')') AS AbilityName, A.Icon, " + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND TargetNpcId = {3} AND SourcePlayerId IS NOT NULL))) AS Percentage " +
                                               InteractionJoinSourcePlayer + BaseWhereIncoming + " AND {1}.SourcePlayerId IS NOT NULL " +
                                               "GROUP BY P.Name, P.Id, A.Name, A.Icon, E.Duration ORDER BY Total DESC",
                                                RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }
                        }

                        public static class BySource
                        {
                            public static class OtherNpcs
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, {1}.SourceNpcName AS SourceName, {1}.SourceNpcId AS SourceId, " +
                                            InteractionBaseQueryPerSecond +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                                 BaseWhereIncoming + "AND {1}.SourceNpcId <> {2} GROUP BY {1}.SecondsElapsed, {1}.SourceNpcName, {1}.SourceNpcId " +
                                                 "ORDER BY {1}.SecondsElapsed, {1}.SourceNpcName", RecordTable, RecordTableAlias, SearchCharacterId);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SourceNpcName AS SourceName," + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND TargetNpcId = {3} AND SourceNpcId <> {3}))) AS Percentage " +
                                               InteractionJoinNoPlayer + BaseWhereIncoming + " AND {1}.SourceNpcId <> {3} " +
                                               "GROUP BY {1}.SourceNpcName, {1}.SourceNpcId, E.Duration ORDER BY Total DESC",
                                                RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            public static class OnlyPlayers
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SecondsElapsed, P.Name AS SourceName, P.Id AS SourceId, " +
                                            InteractionBaseQueryPerSecond +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.SourcePlayerId = P.Id " +
                                                 BaseWhereIncoming + "AND {1}.SourcePlayerId IS NOT NULL GROUP BY {1}.SecondsElapsed, P.Name, P.Id " +
                                                 "ORDER BY {1}.SecondsElapsed, P.Name", RecordTable, RecordTableAlias, SearchCharacterId);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT P.Name AS SourceName," + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND TargetNpcId = {3} AND SourcePlayerId IS NOT NULL)) AS Percentage " +
                                               InteractionJoinSourcePlayer + BaseWhereIncoming + " AND {1}.SourcePlayerId IS NOT NULL " +
                                               "GROUP BY P.Name, P.Id, E.Duration ORDER BY Total DESC",
                                                RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }

                            public static class AllSources
                            {
                                public static string PerSecond
                                {
                                    get
                                    {
                                        return string.Format(
                                            "SELECT {1}.SecondsElapsed, {1}.SourceNpcName AS SourceName, {1}.SourceNpcId AS SourcEId, " +
                                            InteractionBaseQueryPerSecond +
                                            "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                            BaseWhereIncoming + "AND {1}.SourceNpcId IS NOT NULL GROUP BY {1}.SecondsElapsed, {1}.SourceNpcName, {1}.SourceNpcId " +
                                            "UNION " +
                                            "SELECT {1}.SecondsElapsed, P.Name AS SourceName, CAST(P.Id AS CHAR(50)) AS SourceId, " + InteractionBaseQueryPerSecond +
                                            "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.SourcePlayerId = P.Id " +
                                            BaseWhereIncoming + "AND {1}.SourcePlayerId IS NOT NULL GROUP BY {1}.SecondsElapsed, P.Name, P.Id " +
                                            "ORDER BY SecondsElapsed, SourceName",
                                            RecordTable, RecordTableAlias, SearchCharacterId);
                                    }
                                }
                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT {1}.SourceNpcName AS SourceName," + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND TargetNpcId = {3}))) AS Percentage " +
                                               InteractionJoinNoPlayer + BaseWhereIncoming + " AND {1}.SourceNpcId IS NOT NULL " +
                                               "GROUP BY {1}.SourceNpcName, {1}.SourceNpcId, E.Duration " +
                                               "UNION " +
                                               "SELECT P.Name AS SourceName," + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND TargetNpcId = {3}))) AS Percentage " +
                                               InteractionJoinSourcePlayer + BaseWhereIncoming + " AND {1}.SourcePlayerId IS NOT NULL " +
                                               "GROUP BY P.Name, P.Id, E.Duration ORDER BY Total DESC",
                                                RecordTable, RecordTableAlias, SearchEncounterId, SearchCharacterId);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static class Detail
        {
            public static string DamageToNpcsByPlane
            {
                get { return "SELECT A.DamageType, COUNT(1) AS Count, SUM(DD.TotalDamage) AS TotalDamage " +
                             "FROM DamageDone DD " +
                             "JOIN Ability A ON DD.AbilityId = A.Id " +
                             "WHERE EncounterId = @encounterId AND DD.TotalDamage > 0 " +
                             "AND DD.SourcePlayerId IS NOT NULL AND TargetNpcName IS NOT NULL " +
                             "GROUP BY A.DamageType " +
                             "ORDER BY TotalDamage DESC"; }
            }

            public static string DamageToPlayersByPlane
            {
                get
                {
                    return "SELECT A.DamageType, COUNT(1) AS Count, SUM(DD.TotalDamage) AS TotalDamage " +
                           "FROM DamageDone DD " +
                           "JOIN Ability A ON DD.AbilityId = A.Id " +
                           "WHERE EncounterId = @encounterId AND DD.TotalDamage > 0 AND DD.SourceNpcName IS NOT NULL " +
                           "AND DD.TargetPlayerId IS NOT NULL " +
                           "GROUP BY A.DamageType " +
                           "ORDER BY TotalDamage DESC";
                }
            }

            public static string DamageToNpcsByClass
            {
                get
                {
                    return "SELECT EPR.Class, SUM(X.Count) AS Count, SUM(X.TotalDamage) AS TotalDamage FROM (" +
                           "SELECT P.Id AS PlayerId, DD.EncounterId, P.Name, COUNT(1) AS Count, SUM(DD.TotalDamage) AS TotalDamage " +
                           "FROM DamageDone DD " +
                           "JOIN Player P ON DD.SourcePlayerId = P.Id " +
                           "WHERE DD.EncounterId = @encounterId AND DD.EffectiveDamage > 0 AND DD.SourcePlayerId IS NOT NULL AND DD.TargetNpcName IS NOT NULL " +
                           "GROUP BY DD.SourcePlayerId " +
                           "ORDER BY TotalDamage DESC) AS X " +
                           "JOIN EncounterPlayerRole EPR ON X.PlayerId = EPR.PlayerId AND X.EncounterId = EPR.EncounterId " +
                           "GROUP BY EPR.Class ORDER BY TotalDamage DESC;";
                }
            }
        }
    }


}
