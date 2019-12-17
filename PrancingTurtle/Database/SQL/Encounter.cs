using System;

namespace Database.SQL
{
    public static class Encounter
    {
        public static string GetAllIds
        {
            get { return "SELECT Id FROM Encounter ORDER BY Id DESC"; }
        }
        /// <summary>
        /// Gets the query required to make an encounter private
        /// </summary>
        public static string MakePrivate
        {
            get { return "UPDATE Encounter SET IsPublic = 0 WHERE Id = @id"; }
        }
        /// <summary>
        /// Gets the query required to make an encounter public
        /// </summary>
        public static string MakePublic
        {
            get { return "UPDATE Encounter SET IsPublic = 1 WHERE Id = @id"; }
        }
        /// <summary>
        /// Gets the query required to set an encounter to valid for rankings
        /// </summary>
        public static string MakeValidForRankings
        {
            get { return "UPDATE Encounter SET ValidForRanking = 1 WHERE Id = @id"; }
        }
        /// <summary>
        /// Gets the query required to set an encounter to valid for rankings, and update the difficulty
        /// </summary>
        public static string MakeValidForRankingsIncDifficulty
        {
            get { return "UPDATE Encounter SET ValidForRanking = 1, EncounterDifficultyId = @difficultyId WHERE Id = @id"; }
        }
        /// <summary>
        /// Gets the query required to set an encounter to invalid for rankings
        /// </summary>
        public static string MakeInvalidForRankings
        {
            get { return "UPDATE Encounter SET ValidForRanking = 0 WHERE Id = @id"; }
        }
        /// <summary>
        /// Gets the query required to count the number of encounters that exist for a given guild
        /// </summary>
        public static string CountTotalEncountersForGuild
        {
            get { return "SELECT COUNT(*) FROM Encounter WHERE GuildId = @guildId"; }
        }

        public static string GetAllSuccessfulEncountersForSpecificBossFight
        {
            get { return "SELECT * FROM Encounter WHERE SuccessfulKill = 1 AND BossFightId = @bossFightId"; }
        }

        public static string GetAllSuccessfulEncountersSinceDate
        {
            get { return "SELECT * FROM Encounter WHERE SuccessfulKill = 1 AND Date > @date"; }
        }

        public static string GetAllUnsuccessfulEncountersBeforeDate
        {
            get { return "SELECT * FROM Encounter WHERE SuccessfulKill = 0 AND Date < @date AND ToBeDeleted = 0"; }
        }
        public static string GetAllUnsuccessfulEncountersBeforeDateBossFight
        {
            get { return "SELECT * FROM Encounter WHERE SuccessfulKill = 0 AND Date < @date AND BossFightId = @bossFightId AND ToBeDeleted = 0"; }
        }

        public static string GetAllSuccessfulButInvalidEncountersForSpecificBossFight
        {
            get { return "SELECT * FROM Encounter WHERE SuccessfulKill = 1 AND BossFightId = @bossFightId AND ValidForRanking = 0"; }
        }

        public static string GetTopDamageTakenForNpc
        {
            get { return "SELECT (CASE WHEN SUM(TotalDamage) > 0 THEN CAST(SUM(TotalDamage) AS unsigned integer) " +
                         "ELSE 0 END) AS DamageTaken FROM DamageDone WHERE TargetNpcName = @name " +
                         "AND EncounterId = @encounterId GROUP BY TargetNpcId ORDER BY DamageTaken DESC LIMIT 1";
            }
        }

        /// <summary>
        /// Gets the query required to return a single encounter. Updated for MySQL
        /// </summary>
        public static string GetSingle
        {
            get
            {
                return "SELECT * FROM Encounter E " +
                       "JOIN EncounterDifficulty ED ON E.EncounterDifficultyId = ED.Id " +
                       "JOIN BossFight BF ON E.BossFightId = BF.Id " +
                       "JOIN Instance I ON BF.InstanceId = I.Id " +
                       "JOIN AuthUserCharacter AUC ON E.UploaderId = AUC.Id " +
                       "JOIN Shard S ON AUC.ShardId = S.Id " +
                       "LEFT JOIN Guild G ON AUC.GuildId = G.Id " +
                       "WHERE E.Id = @id LIMIT 0,1";
            }
        }

        public static class PlayerRoles
        {
            /// <summary>
            /// The query to return player roles from Damage, Healing and Shielding done tables.
            /// Requires the parameter @id for Encounter ID
            /// </summary>
            public static string All
            {
                get
                {
                    return "SELECT * FROM (SELECT P.Id, P.Name, RI.Name AS Role, PC.Name AS Class " +
                           "FROM ShieldingDone D JOIN Ability A ON D.AbilityId = A.Id " +
                           "JOIN Player P ON D.SourcePlayerId = P.Id " +
                           "JOIN AbilityRole AR ON A.AbilityId = AR.AbilityLogId " +
                           "JOIN PlayerClass PC ON AR.PlayerClassId = PC.Id " +
                           "JOIN RoleIcon RI ON AR.RoleIconId = RI.Id " +
                           "WHERE D.EncounterId = @id ORDER BY RI.Priority ASC) AS X " +
                           "GROUP BY X.Name " +
                           "UNION " +
                           "SELECT * FROM (SELECT P.Id, P.Name, RI.Name AS Role, PC.Name AS Class " +
                           "FROM HealingDone D JOIN Ability A ON D.AbilityId = A.Id " +
                           "JOIN Player P ON D.SourcePlayerId = P.Id " +
                           "JOIN AbilityRole AR ON A.AbilityId = AR.AbilityLogId " +
                           "JOIN PlayerClass PC ON AR.PlayerClassId = PC.Id " +
                           "JOIN RoleIcon RI ON AR.RoleIconId = RI.Id " +
                           "WHERE D.EncounterId = @id ORDER BY RI.Priority ASC) AS X " +
                           "GROUP BY X.Name " +
                           "UNION " +
                           "SELECT * FROM (SELECT P.Id, P.Name, RI.Name AS Role, PC.Name AS Class " +
                           "FROM DamageDone D JOIN Ability A ON D.AbilityId = A.Id " +
                           "JOIN Player P ON D.SourcePlayerId = P.Id " +
                           "JOIN AbilityRole AR ON A.AbilityId = AR.AbilityLogId " +
                           "JOIN PlayerClass PC ON AR.PlayerClassId = PC.Id " +
                           "JOIN RoleIcon RI ON AR.RoleIconId = RI.Id " +
                           "WHERE D.EncounterId = @id ORDER BY RI.Priority ASC) AS X " +
                           "GROUP BY X.Name";
                }
            }
            /// <summary>
            /// The query to return player roles from the ShieldingDone table. Requires the parameter @id for ShieldingDone.EncounterId
            /// </summary>
            public static string Shield
            {
                get
                {
                    return "SELECT * FROM (SELECT D.SourcePlayerId AS Id, P.Name, RI.Name AS Role, " +
                           "PC.Name AS Class FROM (" +
                           "SELECT DISTINCT SourcePlayerId, AbilityId FROM ShieldingDone WHERE EncounterId = @id " +
                           "AND SourcePlayerId IS NOT NULL) D " +
                           "JOIN Ability A ON D.AbilityId = A.Id " +
                           "JOIN AbilityRole AR ON A.AbilityId = AR.AbilityLogId " +
                           "JOIN Player P ON D.SourcePlayerId = P.Id " +
                           "JOIN PlayerClass PC ON AR.PlayerClassId = PC.Id " +
                           "JOIN RoleIcon RI ON AR.RoleIconId = RI.Id ORDER BY RI.Priority ASC) AS X GROUP BY X.Id";
                }
            }
            /// <summary>
            /// The query to return player roles from the HealingDone table. Requires the parameter @id for HealingDone.EncounterId
            /// </summary>
            public static string Heal
            {
                get
                {
                    return "SELECT * FROM (SELECT D.SourcePlayerId AS Id, P.Name, RI.Name AS Role, " +
                           "PC.Name AS Class FROM (" +
                           "SELECT DISTINCT SourcePlayerId, AbilityId FROM HealingDone WHERE EncounterId = @id " +
                           "AND SourcePlayerId IS NOT NULL) D " +
                           "JOIN Ability A ON D.AbilityId = A.Id " +
                           "JOIN AbilityRole AR ON A.AbilityId = AR.AbilityLogId " +
                           "JOIN Player P ON D.SourcePlayerId = P.Id " +
                           "JOIN PlayerClass PC ON AR.PlayerClassId = PC.Id " +
                           "JOIN RoleIcon RI ON AR.RoleIconId = RI.Id ORDER BY RI.Priority ASC) AS X GROUP BY X.Id";
                }
            }
            /// <summary>
            /// The query to return player roles from the DamageDone table. Requires the parameter @id for DamageDone.EncounterId
            /// </summary>
            public static string Damage
            {
                get
                {
                    return "SELECT D.SourcePlayerId AS Id, P.Name, RI.Name AS Role, PC.Name AS Class, RI.Priority FROM (" +
                           "SELECT DISTINCT SourcePlayerId, AbilityId FROM DamageDone WHERE EncounterId = @id " +
                           "AND SourcePlayerId IS NOT NULL) D " +
                           "JOIN Ability A ON D.AbilityId = A.Id " +
                           "JOIN AbilityRole AR ON A.AbilityId = AR.AbilityLogId " +
                           "JOIN Player P ON D.SourcePlayerId = P.Id " +
                           "JOIN PlayerClass PC ON AR.PlayerClassId = PC.Id " +
                           "JOIN RoleIcon RI ON AR.RoleIconId = RI.Id ORDER BY RI.Priority ASC";
                }
            }

            public static string DamageTakenFromNpcs =
                "SELECT TargetPlayerId AS PlayerId, SUM(TotalDamage) AS ValueSum " +
                "FROM DamageDone WHERE EncounterId = @id AND SourcePlayerId IS NULL " +
                "AND TargetPlayerId IS NOT NULL GROUP BY TargetPlayerId";
            public static string HealingDoneToPlayers =
                "SELECT SourcePlayerId AS PlayerId, SUM(EffectiveHealing) AS ValueSum " +
                "FROM HealingDone WHERE EncounterId = @id AND SourcePlayerId IS NOT NULL " +
                "AND TargetPlayerId IS NOT NULL GROUP BY SourcePlayerId";

            public static string CountPlayersAndRolesForEncounter =
                "SELECT P.EncounterId, COUNT(1) AS Players, " +
                "(SELECT COUNT(1) FROM EncounterPlayerRole WHERE EncounterId = @id) " +
                "AS PlayersWithRoles FROM (" +
                "SELECT EncounterId, SourcePlayerId FROM DamageDone WHERE EncounterId = @id " +
                "AND SourcePlayerId IS NOT NULL GROUP BY SourcePlayerId " +
                "UNION " +
                "SELECT EncounterId, SourcePlayerId FROM HealingDone WHERE EncounterId = @id " +
                "AND SourcePlayerId IS NOT NULL GROUP BY SourcePlayerId " +
                "UNION " +
                "SELECT EncounterId, SourcePlayerId FROM ShieldingDone WHERE EncounterId = @id " +
                "AND SourcePlayerId IS NOT NULL GROUP BY SourcePlayerId) P";

            public static string RemoveRecords =
                "DELETE FROM EncounterPlayerRole WHERE EncounterId = @id";
        }

        public static class Overview
        {
            public static string TotalPlayerDeathsForEncounter
            {
                get { return "SELECT COUNT(*) FROM EncounterDeath WHERE EncounterId = @id AND TargetPlayerId IS NOT NULL"; }
            }

            public static string GetPlayerDeathsForEncounter
            {
                get
                {
                    return
                        "SELECT SecondsElapsed FROM EncounterDeath WHERE EncounterId = @id AND TargetPlayerId IS NOT NULL ORDER BY SecondsElapsed";
                }
            }
            public static string UniquePlayers
            {
                get
                {
                    return "SELECT DISTINCT P.Id, P.Name " +
                        "FROM ShieldingDone D JOIN Player P ON D.SourcePlayerId = P.Id " +
                        "WHERE EncounterId = @id " +
                        "UNION " +
                        "SELECT DISTINCT P.Id, P.Name " +
                        "FROM HealingDone D JOIN Player P ON D.SourcePlayerId = P.Id " +
                        "WHERE EncounterId = @id " +
                        "UNION " +
                        "SELECT DISTINCT P.Id, P.Name " +
                        "FROM DamageDone D JOIN Player P ON D.SourcePlayerId = P.Id " +
                        "WHERE EncounterId = @id";
                }
            }
            /// <summary>
            /// Player-based queries for incoming and outgoing damage, healing and shielding for overview
            /// </summary>
            public static class Player
            {
                public static class Damage
                {
                    public static class Done
                    {
                        /// <summary>
                        /// The query to return player damage done for an encounter overview. Requires the parameter @encounterId for DamageDone.EncounterId
                        /// </summary>
                        public static string Totals
                        {
                            get
                            {
                                // Query including Total, TotalToNpcs, TotalToOtherPlayers, TotalToSelf, sorted by damage to NPCs

                                return "SELECT DD.SourcePlayerId AS PlayerId, P.Name AS PlayerName, P.PlayerId AS PlayerLogId, " +
                                    "(SUM(CASE WHEN SourcePlayerId IS NOT NULL THEN DD.EffectiveDamage ELSE 0 END) " +
                                    "+ SUM(CASE WHEN SourcePlayerId IS NOT NULL THEN DD.AbsorbedAmount ELSE 0 END)) AS Total, " +
                                    "(SUM(CASE WHEN SourcePlayerId IS NOT NULL AND TargetNpcId IS NOT NULL THEN DD.EffectiveDamage ELSE 0 END)  " +
                                    "+ SUM(CASE WHEN SourcePlayerId IS NOT NULL AND TargetNpcId IS NOT NULL THEN DD.AbsorbedAmount ELSE 0 END)) AS TotalToNpcs, " +
                                    "(SUM(CASE WHEN SourcePlayerId IS NOT NULL AND TargetPlayerId <> SourcePlayerId THEN DD.EffectiveDamage ELSE 0 END)  " +
                                    "+ SUM(CASE WHEN SourcePlayerId IS NOT NULL AND TargetPlayerId <> SourcePlayerId THEN DD.AbsorbedAmount ELSE 0 END)) AS TotalToOtherPlayers, " +
                                    "(SUM(CASE WHEN SourcePlayerId IS NOT NULL AND TargetPlayerId = SourcePlayerId THEN DD.EffectiveDamage ELSE 0 END)  " +
                                    "+ SUM(CASE WHEN SourcePlayerId IS NOT NULL AND TargetPlayerId = SourcePlayerId THEN DD.AbsorbedAmount ELSE 0 END)) AS TotalToSelf " +
                                    "FROM DamageDone DD JOIN Player P ON DD.SourcePlayerId = P.Id " +
                                    "WHERE EncounterId = @encounterId " +
                                    "GROUP BY DD.SourcePlayerId, P.Name, P.PlayerId " +
                                    "ORDER BY TotalToNPCs DESC";
                            }
                        }

                        /// <summary>
                        /// The query to return single target damage dealt by players for an encounter. Requires the parameter @encounterId and @targetName
                        /// </summary>
                        public static string TotalSingleTarget
                        {
                            get
                            {
                                return "SELECT SourcePlayerId AS PlayerId, SUM(EffectiveDamage) AS Damage FROM DamageDone " +
                                       "WHERE EncounterId = @encounterId AND TargetNpcName = @targetName GROUP BY SourcePlayerId;";
                            }
                        }

                        /// <summary>
                        /// The query to return player damage done for an encounter overview graph.
                        /// Requires the parameter @encounterId for Encounter ID
                        /// </summary>
                        public static string PerSecond
                        {
                            get
                            {
                                return "SELECT SecondsElapsed, (SUM(CASE WHEN SourcePlayerId IS NOT NULL THEN EffectiveDamage ELSE 0 END)" +
                                       " + SUM(CASE WHEN SourcePlayerId IS NOT NULL THEN AbsorbedAmount ELSE 0 END)) AS Total, " +
                                       "(SUM(CASE WHEN SourcePlayerId IS NOT NULL AND TargetNpcId IS NOT NULL THEN EffectiveDamage ELSE 0 END)" +
                                       " + SUM(CASE WHEN SourcePlayerId IS NOT NULL AND TargetNpcId IS NOT NULL THEN AbsorbedAmount ELSE 0 END)) AS TotalNpcs, " +
                                       "(SUM(CASE WHEN SourcePlayerId IS NOT NULL AND TargetPlayerId IS NOT NULL THEN EffectiveDamage ELSE 0 END)" +
                                       " + SUM(CASE WHEN SourcePlayerId IS NOT NULL AND TargetPlayerId IS NOT NULL THEN AbsorbedAmount ELSE 0 END)) AS TotalPlayers " +
                                       "FROM DamageDone WHERE EncounterId = @encounterId " +
                                       "GROUP BY SecondsElapsed " +
                                       "ORDER BY SecondsElapsed";
                            }
                        }
                    }

                    public static class Taken
                    {
                        /// <summary>
                        /// The query to return player damage taken for an encounter overview. Requires the parameter @encounterId for DamageDone.EncounterId
                        /// </summary>
                        public static string Totals
                        {
                            get
                            {

                                return "SELECT DD.TargetPlayerId AS PlayerId, P.Name AS PlayerName, P.PlayerId AS PlayerLogId, " +
                                       "(SUM(CASE WHEN TargetPlayerId IS NOT NULL THEN DD.TotalDamage ELSE 0 END) " +
                                       "- SUM(CASE WHEN TargetPlayerId IS NOT NULL THEN DD.OverkillAmount ELSE 0 END)) AS Total, " +
                                       "(SUM(CASE WHEN TargetPlayerId IS NOT NULL AND SourceNpcId IS NOT NULL THEN DD.TotalDamage ELSE 0 END) " +
                                       "- SUM(CASE WHEN TargetPlayerId IS NOT NULL AND SourceNpcId IS NOT NULL THEN DD.OverkillAmount ELSE 0 END)) AS TotalFromNpcs, " +
                                       "(SUM(CASE WHEN TargetPlayerId IS NOT NULL AND SourcePlayerId <> TargetPlayerId THEN DD.TotalDamage ELSE 0 END) " +
                                       "- SUM(CASE WHEN TargetPlayerId IS NOT NULL AND SourcePlayerId <> TargetPlayerId THEN DD.OverkillAmount ELSE 0 END)) AS TotalFromOtherPlayers, " +
                                       "(SUM(CASE WHEN TargetPlayerId IS NOT NULL AND SourcePlayerId = TargetPlayerId THEN DD.TotalDamage ELSE 0 END) " +
                                       "- SUM(CASE WHEN TargetPlayerId IS NOT NULL AND SourcePlayerId = TargetPlayerId THEN DD.OverkillAmount ELSE 0 END)) AS TotalFromSelf " +
                                       "FROM DamageDone DD JOIN Player P ON DD.TargetPlayerId = P.Id " +
                                       "WHERE EncounterId = @encounterId " +
                                       "GROUP BY DD.TargetPlayerId, P.Name, P.PlayerId " +
                                       "ORDER BY TotalFromNPCs DESC";
                            }
                        }

                        /// <summary>
                        /// The query to return player damage taken for an encounter overview graph.
                        /// Requires the parameter @encounterId for Encounter ID
                        /// </summary>
                        public static string PerSecond
                        {
                            get
                            {
                                return "SELECT SecondsElapsed, (SUM(CASE WHEN TargetPlayerId IS NOT NULL THEN TotalDamage ELSE 0 END) " +
                                       "- SUM(CASE WHEN TargetPlayerId IS NOT NULL THEN OverkillAmount ELSE 0 END)) AS Total, " +
                                       "(SUM(CASE WHEN TargetPlayerId IS NOT NULL AND SourceNpcId IS NOT NULL THEN TotalDamage ELSE 0 END)" +
                                       " - SUM(CASE WHEN TargetPlayerId IS NOT NULL AND SourceNpcId IS NOT NULL THEN OverkillAmount ELSE 0 END)) AS TotalNpcs, " +
                                       "(SUM(CASE WHEN TargetPlayerId IS NOT NULL AND SourcePlayerId IS NOT NULL THEN TotalDamage ELSE 0 END)" +
                                       " - SUM(CASE WHEN TargetPlayerId IS NOT NULL AND SourcePlayerId IS NOT NULL THEN OverkillAmount ELSE 0 END)) AS TotalPlayers " +
                                       "FROM DamageDone WHERE EncounterId = @encounterId " +
                                       "GROUP BY SecondsElapsed " +
                                       "ORDER BY SecondsElapsed";
                            }
                        }
                    }
                }

                public static class Healing
                {
                    public static class Done
                    {
                        public static string Totals
                        {
                            get
                            {
                                return "SELECT HD.SourcePlayerId AS PlayerId, P.Name AS PlayerName, P.PlayerId AS PlayerLogId, " +
                                       "SUM(CASE WHEN SourcePlayerId IS NOT NULL THEN HD.EffectiveHealing ELSE 0 END) AS Total, " +
                                       "SUM(CASE WHEN SourcePlayerId IS NOT NULL AND TargetNpcId IS NOT NULL THEN HD.EffectiveHealing ELSE 0 END) AS TotalToNpcs, " +
                                       "SUM(CASE WHEN SourcePlayerId IS NOT NULL AND (TargetPlayerId <> SourcePlayerId OR TargetPetName IS NOT NULL) THEN HD.EffectiveHealing ELSE 0 END) AS TotalToOtherPlayers, " +
                                       "SUM(CASE WHEN SourcePlayerId IS NOT NULL AND TargetPlayerId = SourcePlayerId THEN HD.EffectiveHealing ELSE 0 END) AS TotalToSelf " +
                                       "FROM HealingDone HD JOIN Player P ON HD.SourcePlayerId = P.Id " +
                                       "WHERE EncounterId = @encounterId " +
                                       "GROUP BY HD.SourcePlayerId, P.Name, P.PlayerId " +
                                       "ORDER BY Total DESC";
                            }
                        }

                        public static string PerSecond
                        {
                            get
                            {
                                return "SELECT SecondsElapsed, SUM(CASE WHEN SourcePlayerId IS NOT NULL THEN EffectiveHealing ELSE 0 END) AS Total, " +
                                       "SUM(CASE WHEN SourcePlayerId IS NOT NULL AND TargetNpcId IS NOT NULL THEN EffectiveHealing ELSE 0 END) AS TotalNpcs, " +
                                       "SUM(CASE WHEN SourcePlayerId IS NOT NULL AND (TargetPlayerId IS NOT NULL OR TargetPetName IS NOT NULL) THEN EffectiveHealing ELSE 0 END) AS TotalPlayers " +
                                       "FROM HealingDone WHERE EncounterId = @encounterId " +
                                       "GROUP BY SecondsElapsed " +
                                       "ORDER BY SecondsElapsed";
                            }
                        }
                    }

                    public static class Taken
                    {
                        public static string Totals
                        {
                            get
                            {
                                return "SELECT HD.TargetPlayerId AS PlayerId, P.Name AS PlayerName, " +
                                       "P.PlayerId AS PlayerLogId, " +
                                       "SUM(CASE WHEN TargetPlayerId IS NOT NULL THEN HD.EffectiveHealing ELSE 0 END) AS Total, " +
                                       "SUM(CASE WHEN TargetPlayerId IS NOT NULL AND SourceNpcId IS NOT NULL THEN HD.EffectiveHealing ELSE 0 END) AS TotalFromNpcs, " +
                                       "SUM(CASE WHEN TargetPlayerId IS NOT NULL AND SourcePlayerId <> TargetPlayerId THEN HD.EffectiveHealing ELSE 0 END) AS TotalFromOtherPlayers, " +
                                       "SUM(CASE WHEN TargetPlayerId IS NOT NULL AND SourcePlayerId = TargetPlayerId THEN HD.EffectiveHealing ELSE 0 END) AS TotalFromSelf " +
                                       "FROM HealingDone HD JOIN Player P ON HD.TargetPlayerId = P.Id " +
                                       "WHERE EncounterId = @encounterId " +
                                       "GROUP BY HD.TargetPlayerId, P.Name, P.PlayerId " +
                                       "ORDER BY Total DESC";
                            }
                        }

                        public static string PerSecond
                        {
                            get
                            {
                                return "SELECT SecondsElapsed, " +
                                       "SUM(CASE WHEN TargetPlayerId IS NOT NULL THEN EffectiveHealing ELSE 0 END) AS Total, " +
                                       "SUM(CASE WHEN TargetPlayerId IS NOT NULL AND SourceNpcId IS NOT NULL THEN EffectiveHealing ELSE 0 END) AS TotalNpcs, " +
                                       "SUM(CASE WHEN TargetPlayerId IS NOT NULL AND SourcePlayerId IS NOT NULL THEN EffectiveHealing ELSE 0 END) AS TotalPlayers " +
                                       "FROM HealingDone " +
                                       "WHERE EncounterId = @encounterId " +
                                       "GROUP BY SecondsElapsed " +
                                       "ORDER BY SecondsElapsed";
                            }
                        }
                    }
                }

                public static class Shielding
                {
                    public static class Done
                    {
                        public static string Totals
                        {
                            get
                            {
                                return "SELECT SD.SourcePlayerId AS PlayerId, P.Name AS PlayerName, P.PlayerId AS PlayerLogId, " +
                                       "SUM(SD.ShieldValue) AS Total, " +
                                       "SUM(CASE WHEN SourcePlayerId IS NOT NULL AND TargetNpcId IS NOT NULL THEN SD.ShieldValue ELSE 0 END) AS TotalToNpcs, " +
                                       "SUM(CASE WHEN SourcePlayerId IS NOT NULL AND (TargetPlayerId <> SourcePlayerId OR TargetPetName IS NOT NULL) THEN SD.ShieldValue ELSE 0 END) AS TotalToOtherPlayers, " +
                                       "SUM(CASE WHEN SourcePlayerId IS NOT NULL AND TargetPlayerId = SourcePlayerId THEN SD.ShieldValue ELSE 0 END) AS TotalToSelf " +
                                       "FROM ShieldingDone SD JOIN Player P ON SD.SourcePlayerId = P.Id " +
                                       "WHERE EncounterId = @encounterId AND SourcePlayerId IS NOT NULL " +
                                       "GROUP BY SD.SourcePlayerId, P.Name " +
                                       "ORDER BY Total DESC";
                            }
                        }

                        public static string PerSecond
                        {
                            get
                            {
                                return "SELECT SecondsElapsed, SUM(CASE WHEN SourcePlayerId IS NOT NULL THEN ShieldValue ELSE 0 END) AS Total, " +
                                       "SUM(CASE WHEN SourcePlayerId IS NOT NULL AND TargetNpcId IS NOT NULL THEN ShieldValue ELSE 0 END) AS TotalNpcs, " +
                                       "SUM(CASE WHEN SourcePlayerId IS NOT NULL AND (TargetPlayerId IS NOT NULL OR TargetPetName IS NOT NULL) THEN ShieldValue ELSE 0 END) AS TotalPlayers " +
                                       "FROM ShieldingDone " +
                                       "WHERE EncounterId = @encounterId " +
                                       "GROUP BY SecondsElapsed " +
                                       "ORDER BY SecondsElapsed";
                            }
                        }
                    }

                    public static class Taken
                    {
                        public static string Totals
                        {
                            get
                            {
                                return "SELECT SD.TargetPlayerId AS PlayerId, P.Name AS PlayerName, P.PlayerId AS PlayerLogId, " +
                                       "SUM(SD.ShieldValue) AS Total, " +
                                       "SUM(CASE WHEN TargetPlayerId IS NOT NULL AND SourceNpcId IS NOT NULL THEN SD.ShieldValue ELSE 0 END) AS TotalFromNpcs, " +
                                       "SUM(CASE WHEN TargetPlayerId IS NOT NULL AND SourcePlayerId <> TargetPlayerId THEN SD.ShieldValue ELSE 0 END) AS TotalFromOtherPlayers, " +
                                       "SUM(CASE WHEN TargetPlayerId IS NOT NULL AND SourcePlayerId = TargetPlayerId THEN SD.ShieldValue ELSE 0 END) AS TotalFromSelf " +
                                       "FROM ShieldingDone SD JOIN Player P ON SD.TargetPlayerId = P.Id " +
                                       "WHERE EncounterId = @encounterId AND TargetPlayerId IS NOT NULL " +
                                       "GROUP BY SD.TargetPlayerId, P.Name " +
                                       "ORDER BY Total DESC";
                            }
                        }

                        public static string PerSecond
                        {
                            get
                            {
                                return "SELECT SecondsElapsed, " +
                                       "SUM(CASE WHEN TargetPlayerId IS NOT NULL THEN ShieldValue ELSE 0 END) AS Total, " +
                                       "SUM(CASE WHEN TargetPlayerId IS NOT NULL AND SourceNpcId IS NOT NULL THEN ShieldValue ELSE 0 END) AS TotalNpcs, " +
                                       "SUM(CASE WHEN TargetPlayerId IS NOT NULL AND SourcePlayerId IS NOT NULL THEN ShieldValue ELSE 0 END) AS TotalPlayers " +
                                       "FROM ShieldingDone " +
                                       "WHERE EncounterId = @encounterId " +
                                       "GROUP BY SecondsElapsed " +
                                       "ORDER BY SecondsElapsed";
                            }
                        }
                    }
                }
            }
            /// <summary>
            /// NPC-based queries for incoming and outgoing damage, healing and shielding for overview
            /// </summary>
            public static class Npc
            {
                public static class Damage
                {
                    public static class Done
                    {
                        public static string Totals
                        {
                            get
                            {
                                return "SELECT DD.SourceNpcId AS NpcId, " +
                                       "(CASE WHEN SourceNpcName IS NULL OR SourceNpcName = '' THEN 'Unknown NPC' ELSE SourceNpcName END) AS NpcName, " +
                                       "SUM(CASE WHEN SourceNpcId IS NOT NULL THEN (DD.TotalDamage - DD.OverkillAmount) ELSE 0 END) AS Total, " +
                                       "SUM(CASE WHEN SourceNpcId IS NOT NULL AND TargetPlayerId IS NOT NULL THEN (DD.TotalDamage - DD.OverkillAmount) ELSE 0 END) AS TotalToPlayers, " +
                                       "SUM(CASE WHEN SourceNpcId IS NOT NULL AND TargetNpcId <> SourceNpcId THEN (DD.TotalDamage - DD.OverkillAmount) ELSE 0 END) AS TotalToOtherNpcs, " +
                                       "SUM(CASE WHEN SourceNpcId IS NOT NULL AND TargetNpcId = SourceNpcId THEN (DD.TotalDamage - DD.OverkillAmount) ELSE 0 END) AS TotalToSelf " +
                                       "FROM DamageDone DD " +
                                       "WHERE EncounterId = @encounterId AND SourceNpcId IS NOT NULL " +
                                       "GROUP BY DD.SourceNpcId, DD.SourceNpcName " +
                                       "ORDER BY TotalToPlayers DESC";
                            }
                        }

                        public static string PerSecond
                        {
                            get
                            {
                                return "SELECT SecondsElapsed, " +
                                       "SUM(CASE WHEN SourceNpcId IS NOT NULL THEN (TotalDamage - OverkillAmount) ELSE 0 END) AS Total, " +
                                       "SUM(CASE WHEN SourceNpcId IS NOT NULL AND TargetNpcId IS NOT NULL THEN (TotalDamage - OverkillAmount) ELSE 0 END) AS TotalNpcs, " +
                                       "SUM(CASE WHEN SourceNpcId IS NOT NULL AND TargetPlayerId IS NOT NULL THEN (TotalDamage - OverkillAmount) ELSE 0 END) AS TotalPlayers " +
                                       "FROM DamageDone " +
                                       "WHERE EncounterId = @encounterId " +
                                       "GROUP BY SecondsElapsed " +
                                       "ORDER BY SecondsElapsed";
                            }
                        }
                    }

                    public static class Taken
                    {
                        public static string Totals
                        {
                            get
                            {
                                return "SELECT DD.TargetNpcId AS NpcId, " +
                                       "(CASE WHEN TargetNpcName IS NULL OR TargetNpcName = '' THEN 'Unknown NPC' ELSE TargetNpcName END) AS NpcName, " +
                                       "SUM(CASE WHEN TargetNpcId IS NOT NULL THEN (DD.TotalDamage - DD.OverkillAmount - DD.InterceptedAmount) ELSE 0 END) AS Total, " +
                                       "SUM(CASE WHEN TargetNpcId IS NOT NULL AND SourcePlayerId IS NOT NULL THEN (DD.TotalDamage - DD.OverkillAmount - DD.InterceptedAmount) ELSE 0 END) AS TotalFromPlayers, " +
                                       "SUM(CASE WHEN TargetNpcId IS NOT NULL AND SourceNpcId <> TargetNpcId THEN (DD.TotalDamage - DD.OverkillAmount - DD.InterceptedAmount) ELSE 0 END) AS TotalFromOtherNpcs, " +
                                       "SUM(CASE WHEN TargetNpcId IS NOT NULL AND SourceNpcId = TargetNpcId THEN (DD.TotalDamage - DD.OverkillAmount - DD.InterceptedAmount) ELSE 0 END) AS TotalFromSelf " +
                                       "FROM DamageDone DD " +
                                       "WHERE EncounterId = @encounterId AND TargetNpcId IS NOT NULL " +
                                       "GROUP BY DD.TargetNpcId, DD.TargetNpcName " +
                                       "ORDER BY TotalFromPlayers DESC";
                            }
                        }

                        public static string PerSecond
                        {
                            get
                            {
                                return "SELECT SecondsElapsed, " +
                                       "SUM(CASE WHEN TargetNpcId IS NOT NULL THEN (TotalDamage - OverkillAmount - InterceptedAmount) ELSE 0 END) AS Total, " +
                                       "SUM(CASE WHEN TargetNpcId IS NOT NULL AND SourceNpcId IS NOT NULL THEN (TotalDamage - OverkillAmount - InterceptedAmount) ELSE 0 END) AS TotalNpcs, " +
                                       "SUM(CASE WHEN TargetNpcId IS NOT NULL AND SourcePlayerId IS NOT NULL THEN (TotalDamage - OverkillAmount - InterceptedAmount) ELSE 0 END) AS TotalPlayers " +
                                       "FROM DamageDone " +
                                       "WHERE EncounterId = @encounterId " +
                                       "GROUP BY SecondsElapsed " +
                                       "ORDER BY SecondsElapsed";
                            }
                        }

                        public static string Top25AbilitiesPlayers
                        {
                            get
                            {
                                return "SELECT TOP 25 A.Name + ' (' + P.Name + ')' AS AbilityName " +
                                       "FROM DamageDone DD JOIN Ability A ON DD.AbilityId = A.Id JOIN Player P ON DD.SourcePlayerId = P.Id " +
                                       "WHERE DD.EncounterId = @Id AND DD.TargetNpcId = @npcId " +
                                       "AND DD.SourcePlayerId IS NOT NULL " +
                                       "GROUP BY A.Name, P.Name " +
                                       "ORDER BY SUM(DD.TotalDamage) DESC";
                            }
                        }

                        public static string Top25AbilitiesNpcs
                        {
                            get
                            {
                                return "SELECT TOP 25 A.Name + ' (' + DD.SourceNpcName + ')' AS AbilityName " +
                                       "FROM DamageDone DD JOIN Ability A ON DD.AbilityId = A.Id " +
                                       "WHERE DD.EncounterId = @Id AND DD.TargetNpcId = @npcId " +
                                       "AND DD.SourceNpcId IS NOT NULL " +
                                       "GROUP BY A.Name, DD.SourceNpcName " +
                                       "ORDER BY SUM(DD.TotalDamage) DESC";
                            }
                        }

                        public static string Top25AbilitiesAllSources
                        {
                            get 
                            { 
                                return "SELECT TOP 25 X.AbilityName FROM (SELECT A.Name + ' (' + P.Name + ')' AS AbilityName, " +
                                       "SUM(DD.TotalDamage) AS Total FROM DamageDone DD JOIN Ability A ON DD.AbilityId = A.Id " +
                                       "JOIN Player P ON DD.SourcePlayerId = P.Id " +
                                       "WHERE DD.EncounterId = @Id AND DD.TargetNpcId = @npcId " +
                                       "AND DD.SourcePlayerId IS NOT NULL " +
                                       "GROUP BY A.Name, P.Name " +
                                       "UNION " +
                                       "SELECT A.Name + ' (' + DD.SourceNpcName + ')' AS AbilityName, " +
                                       "SUM(DD.TotalDamage) AS Total " +
                                       "FROM DamageDone DD JOIN Ability A ON DD.AbilityId = A.Id " +
                                       "WHERE DD.EncounterId = @Id AND DD.TargetNpcId = @npcId " +
                                       "AND DD.SourceNpcId IS NOT NULL GROUP BY A.Name, DD.SourceNpcName) X " +
                                       "ORDER BY X.Total DESC"; }
                        }
                    }
                }

                public static class Healing
                {
                    public static class Done
                    {
                        public static string Totals
                        {
                            get
                            {
                                return "SELECT HD.SourceNpcId AS NpcId, " +
                                       "(CASE WHEN SourceNpcName IS NULL OR SourceNpcName = '' THEN 'Unknown NPC' ELSE SourceNpcName END) AS NpcName, " +
                                       "SUM(CASE WHEN SourceNpcId IS NOT NULL THEN HD.EffectiveHealing ELSE 0 END) AS Total, " +
                                       "SUM(CASE WHEN SourceNpcId IS NOT NULL AND TargetPlayerId IS NOT NULL THEN HD.EffectiveHealing ELSE 0 END) AS TotalToPlayers, " +
                                       "SUM(CASE WHEN SourceNpcId IS NOT NULL AND TargetNpcId <> SourceNpcId THEN HD.EffectiveHealing ELSE 0 END) AS TotalToOtherNpcs, " +
                                       "SUM(CASE WHEN SourceNpcId IS NOT NULL AND TargetNpcId = SourceNpcId THEN HD.EffectiveHealing ELSE 0 END) AS TotalToSelf " +
                                       "FROM HealingDone HD " +
                                       "WHERE EncounterId = @encounterId AND SourceNpcId IS NOT NULL " +
                                       "GROUP BY HD.SourceNpcId, HD.SourceNpcName " +
                                       "ORDER BY Total DESC";
                            }
                        }

                        public static string PerSecond
                        {
                            get
                            {
                                return "SELECT SecondsElapsed, " +
                                    "SUM(CASE WHEN SourceNpcId IS NOT NULL THEN EffectiveHealing ELSE 0 END) AS Total, " +
                                    "SUM(CASE WHEN SourceNpcId IS NOT NULL AND TargetNpcId IS NOT NULL THEN EffectiveHealing ELSE 0 END) AS TotalNpcs, " +
                                    "SUM(CASE WHEN SourceNpcId IS NOT NULL AND TargetPlayerId IS NOT NULL THEN EffectiveHealing ELSE 0 END) AS TotalPlayers " +
                                    "FROM HealingDone " +
                                    "WHERE EncounterId = @encounterId " +
                                    "GROUP BY SecondsElapsed " +
                                    "ORDER BY SecondsElapsed";
                            }
                        }
                    }

                    public static class Taken
                    {
                        public static string Totals
                        {
                            get
                            {
                                return "SELECT HD.TargetNpcId AS NpcId, " +
                                    "(CASE WHEN TargetNpcName IS NULL OR TargetNpcName = '' THEN 'Unknown NPC' ELSE TargetNpcName END) AS NpcName, " +
                                    "SUM(CASE WHEN TargetNpcId IS NOT NULL THEN HD.EffectiveHealing ELSE 0 END) AS Total, " +
                                    "SUM(CASE WHEN TargetNpcId IS NOT NULL AND SourcePlayerId IS NOT NULL THEN HD.EffectiveHealing ELSE 0 END) AS TotalFromPlayers, " +
                                    "SUM(CASE WHEN TargetNpcId IS NOT NULL AND SourceNpcId <> TargetNpcId THEN HD.EffectiveHealing ELSE 0 END) AS TotalFromOtherNpcs, " +
                                    "SUM(CASE WHEN TargetNpcId IS NOT NULL AND SourceNpcId = TargetNpcId THEN HD.EffectiveHealing ELSE 0 END) AS TotalFromSelf " +
                                    "FROM HealingDone HD " +
                                    "WHERE EncounterId = @encounterId AND TargetNpcId IS NOT NULL " +
                                    "GROUP BY HD.TargetNpcId, HD.TargetNpcName " +
                                    "ORDER BY Total DESC";
                            }
                        }

                        public static string PerSecond
                        {
                            get
                            {
                                return "SELECT SecondsElapsed, " +
                                    "SUM(CASE WHEN TargetNpcId IS NOT NULL THEN EffectiveHealing ELSE 0 END) AS Total, " +
                                    "SUM(CASE WHEN TargetNpcId IS NOT NULL AND SourceNpcId IS NOT NULL THEN EffectiveHealing ELSE 0 END) AS TotalNpcs, " +
                                    "SUM(CASE WHEN TargetNpcId IS NOT NULL AND SourcePlayerId IS NOT NULL THEN EffectiveHealing ELSE 0 END) AS TotalPlayers " +
                                    "FROM HealingDone " +
                                    "WHERE EncounterId = @encounterId " +
                                    "GROUP BY SecondsElapsed " +
                                    "ORDER BY SecondsElapsed";
                            }
                        }
                    }
                }

                public static class Shielding
                {
                    public static class Done
                    {
                        public static string Totals
                        {
                            get
                            {
                                return "SELECT SD.SourceNpcId AS NpcId, " +
                                       "(CASE WHEN SourceNpcName IS NULL OR SourceNpcName = '' THEN 'Unknown NPC' ELSE SourceNpcName END) AS NpcName, " +
                                       "SUM(SD.ShieldValue) AS Total, " +
                                       "SUM(CASE WHEN SourceNpcId IS NOT NULL AND TargetPlayerId IS NOT NULL THEN SD.ShieldValue ELSE 0 END) AS TotalToPlayers, " +
                                       "SUM(CASE WHEN SourceNpcId IS NOT NULL AND TargetNpcId <> SourceNpcId THEN SD.ShieldValue ELSE 0 END) AS TotalToOtherNpcs, " +
                                       "SUM(CASE WHEN SourceNpcId IS NOT NULL AND TargetNpcId = SourceNpcId THEN SD.ShieldValue ELSE 0 END) AS TotalToSelf " +
                                       "FROM ShieldingDone SD " +
                                       "WHERE EncounterId = @encounterId AND SourceNpcId IS NOT NULL " +
                                       "GROUP BY SD.SourceNpcId, SD.SourceNpcName " +
                                       "ORDER BY Total DESC";
                            }
                        }

                        public static string PerSecond
                        {
                            get
                            {
                                return "SELECT SecondsElapsed, " +
                                    "SUM(CASE WHEN SourceNpcId IS NOT NULL THEN ShieldValue ELSE 0 END) AS Total, " +
                                    "SUM(CASE WHEN SourceNpcId IS NOT NULL AND TargetNpcId IS NOT NULL THEN ShieldValue ELSE 0 END) AS TotalNpcs, " +
                                    "SUM(CASE WHEN SourceNpcId IS NOT NULL AND TargetPlayerId IS NOT NULL THEN ShieldValue ELSE 0 END) AS TotalPlayers " +
                                    "FROM ShieldingDone " +
                                    "WHERE EncounterId = @encounterId " +
                                    "GROUP BY SecondsElapsed " +
                                    "ORDER BY SecondsElapsed";
                            }
                        }
                    }

                    public static class Taken
                    {
                        public static string Totals
                        {
                            get
                            {
                                return "SELECT SD.TargetNpcId AS NpcId, " +
                                    "(CASE WHEN TargetNpcName IS NULL OR TargetNpcName = '' THEN 'Unknown NPC' ELSE TargetNpcName END) AS NpcName, " +
                                    "SUM(SD.ShieldValue) AS Total, " +
                                    "SUM(CASE WHEN TargetNpcId IS NOT NULL AND SourcePlayerId IS NOT NULL THEN SD.ShieldValue ELSE 0 END) AS TotalFromPlayers, " +
                                    "SUM(CASE WHEN TargetNpcId IS NOT NULL AND SourceNpcId <> TargetNpcId THEN SD.ShieldValue ELSE 0 END) AS TotalFromOtherNpcs, " +
                                    "SUM(CASE WHEN TargetNpcId IS NOT NULL AND SourceNpcId = TargetNpcId THEN SD.ShieldValue ELSE 0 END) AS TotalFromSelf " +
                                    "FROM ShieldingDone SD " +
                                    "WHERE EncounterId = @encounterId AND TargetNpcId IS NOT NULL " +
                                    "GROUP BY SD.TargetNpcId, SD.TargetNpcName " +
                                    "ORDER BY Total DESC";
                            }
                        }

                        public static string PerSecond
                        {
                            get
                            {
                                return "SELECT SecondsElapsed, " +
                                    "SUM(CASE WHEN TargetNpcId IS NOT NULL THEN ShieldValue ELSE 0 END) AS Total, " +
                                    "SUM(CASE WHEN TargetNpcId IS NOT NULL AND SourceNpcId IS NOT NULL THEN ShieldValue ELSE 0 END) AS TotalNpcs, " +
                                    "SUM(CASE WHEN TargetNpcId IS NOT NULL AND SourcePlayerId IS NOT NULL THEN ShieldValue ELSE 0 END) AS TotalPlayers " +
                                    "FROM ShieldingDone " +
                                    "WHERE EncounterId = @encounterId " +
                                    "GROUP BY SecondsElapsed " +
                                    "ORDER BY SecondsElapsed";
                            }
                        }
                    }
                }
            }


            /// <summary>
            /// Gets the query required to return player DPS for a given encounter
            /// </summary>
            public static string PlayerDpsAverage
            {
                get
                {
                    return "SELECT (SUM(EffectiveDamage) + SUM(AbsorbedAmount)) AS Total, " +
                        "((SUM(EffectiveDamage) + SUM(AbsorbedAmount)) / @totalSeconds) AS Average " +
                        "FROM DamageDone " +
                        "WHERE EncounterId = @id AND SourcePlayerId IS NOT NULL AND TargetNpcId IS NOT NULL";
                }
            }
            /// <summary>
            /// Gets the query required to return player HPS for a given encounter
            /// </summary>
            public static string PlayerHpsAverage
            {
                get
                {
                    return "SELECT SUM(EffectiveHealing) AS Total, " +
                             "(SUM(EffectiveHealing) / @totalSeconds) AS Average " +
                             "FROM HealingDone " +
                             "WHERE EncounterId = @id AND SourcePlayerId IS NOT NULL AND TargetNpcId IS NULL";
                }
            }
            /// <summary>
            /// Gets the query required to return player APS for a given encounter
            /// </summary>
            public static string PlayerApsAverage
            {
                get
                {
                    return @"SELECT SUM(ShieldValue) AS Total,
                             (SUM(ShieldValue) / @totalSeconds) AS Average
                             FROM ShieldingDone
                             WHERE EncounterId = @id AND SourcePlayerId IS NOT NULL AND TargetNpcId IS NULL";
                }
            }
            /// <summary>
            /// Gets the query required to return a list of debuffs seen for a given encounter
            /// </summary>
            public static string DebuffOverview
            {
                get
                {
                    return "SELECT * FROM (SELECT * FROM EncounterDebuffAction " +
                           "WHERE EncounterId = @id " +
                           "ORDER BY DebuffName) AS X GROUP BY DebuffName";
                }
            }
            /// <summary>
            /// Gets the query required to return a list of buffs seen for a given encounter
            /// </summary>
            public static string BuffOverview
            {
                get
                {
                    return "SELECT * FROM (SELECT * FROM EncounterBuffAction " +
                           "WHERE EncounterId = @id " +
                           "ORDER BY BuffName) AS X GROUP BY BuffName";
                }
            }

            public static string MainRaidBuffTimers
            {
                get
                {
                    return "SELECT BuffName, SecondBuffWentUp, MAX(SecondBuffWentDown) AS SecondBuffWentDown " +
                        "FROM EncounterBuffAction " +
                        "WHERE EncounterId = @id " +
                        "AND BuffName = @buffName " +
                        "GROUP BY BuffName, SecondBuffWentUp";
                }
            }

            /// <summary>
            /// Gets the query required to return a list of all of the NPC casts seen in a given encounter
            /// </summary>
            public static string NpcCast
            {
                get { return "SELECT * FROM EncounterNpcCast WHERE EncounterId = @id ORDER BY AbilityName ASC"; }
            }
            /// <summary>
            /// Gets the query required to return a list of all of the player deaths for a given encounter
            /// </summary>
            public static string Deaths
            {
                get
                {
                    return @"SELECT * FROM EncounterDeath ED
                             LEFT JOIN Player P1 ON ED.SourcePlayerId = P1.Id
                             LEFT JOIN Player P2 ON ED.TargetPlayerId = P2.Id
                             JOIN Encounter E ON ED.EncounterId = E.Id
                             JOIN Ability A ON ED.AbilityId = A.Id
                             WHERE ED.EncounterId = @id
                             ORDER BY ED.SecondsElapsed, ED.OrderWithinSecond";
                }
            }
        }

        /// <summary>
        /// Character-based (Player or NPC) Encounter info
        /// </summary>
        public static class Character
        {
            public static class Player
            {
                public static class Damage
                {
                    public static string RecordTable { get { return "DamageDone"; } }
                    public static string RecordTableAlias { get { return "DD"; } }
                    public static string SearchCharacter { get { return "Player"; } }
                    public static string SearchEncounterId { get { return "@Id"; } }
                    public static string SearchCharacterId { get { return "@playerId"; } }
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
                            return string.Format("COUNT(CASE WHEN {0}.CriticalHit = 'true' THEN 1 END) AS Crits, " +
                                    "count(CASE WHEN {0}.CriticalHit = 'false' THEN 1 END) AS Hits, " +
                                    "MAX({0}.TotalDamage) AS BiggestHit, AVG({0}.TotalDamage) AS AverageHit, " +
                                    "(SUM({0}.EffectiveDamage) + SUM({0}.AbsorbedAmount) + SUM({0}.BlockedAmount) " +
                                    "+ SUM({0}.InterceptedAmount) + SUM({0}.IgnoredAmount)) AS Total, " +
                                    "SUM({0}.BlockedAmount) AS Blocked, " +
                                    "SUM({0}.IgnoredAmount) AS Ignored, " +
                                    "SUM({0}.InterceptedAmount) AS Intercepted, " +
                                    "SUM({0}.AbsorbedAmount) AS Absorbed, " +
                                    "SUM({0}.EffectiveDamage) AS Effective, " +
                                    "((SUM(EffectiveDamage) + SUM(AbsorbedAmount) + SUM(BlockedAmount) " +
                                    "+ SUM(InterceptedAmount) + SUM(IgnoredAmount)) / ((DATEPART(HOUR, E.Duration) * 3600) + " +
                                    "(DATEPART(MINUTE, E.Duration) * 60) + DATEPART(SECOND, E.Duration))) AS Average, " +
                                    "(SUM(EffectiveDamage) / ((DATEPART(HOUR, E.Duration) * 3600) + " +
                                    "(DATEPART(MINUTE, E.Duration) * 60) + DATEPART(SECOND, E.Duration))) AS AverageEffective, " +
                                    "((100.0 * (SUM({0}.EffectiveDamage) + SUM({0}.AbsorbedAmount)) /  " +
                                    "(SELECT (SUM(EffectiveDamage) + SUM(AbsorbedAmount)) ", RecordTableAlias);
                        }
                    }

                    public static string InteractionBaseQueryTotalsNoPercentage
                    {
                        get
                        {
                            return string.Format("COUNT(CASE WHEN {0}.CriticalHit = 'true' THEN 1 END) AS Crits, " +
                                    "count(CASE WHEN {0}.CriticalHit = 'false' THEN 1 END) AS Hits, " +
                                    "MAX({0}.TotalDamage) AS BiggestHit, AVG({0}.TotalDamage) AS AverageHit, " +
                                    "(SUM({0}.EffectiveDamage) + SUM({0}.AbsorbedAmount) + SUM({0}.BlockedAmount) " +
                                    "+ SUM({0}.InterceptedAmount) + SUM({0}.IgnoredAmount)) AS Total, " +
                                    "SUM({0}.BlockedAmount) AS Blocked, " +
                                    "SUM({0}.IgnoredAmount) AS Ignored, " +
                                    "SUM({0}.InterceptedAmount) AS Intercepted, " +
                                    "SUM({0}.AbsorbedAmount) AS Absorbed, " +
                                    "SUM({0}.EffectiveDamage) AS Effective, " +
                                    "((SUM(EffectiveDamage) + SUM(AbsorbedAmount) + SUM(BlockedAmount) " +
                                    "+ SUM(InterceptedAmount) + SUM(IgnoredAmount)) / ((DATEPART(HOUR, E.Duration) * 3600) + " +
                                    "(DATEPART(MINUTE, E.Duration) * 60) + DATEPART(SECOND, E.Duration))) AS Average, " +
                                    "(SUM(EffectiveDamage) / ((DATEPART(HOUR, E.Duration) * 3600) + " +
                                    "(DATEPART(MINUTE, E.Duration) * 60) + DATEPART(SECOND, E.Duration))) AS AverageEffective ", RecordTableAlias);
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
                                            "(SUM(X.Total) / ((DATEPART(HOUR, X.Duration) * 3600) + (DATEPART(MINUTE, X.Duration) * 60) + DATEPART(SECOND, X.Duration))) AS Average, " +
                                            "(SUM(X.Effective) / ((DATEPART(HOUR, X.Duration) * 3600) + (DATEPART(MINUTE, X.Duration) * 60) + DATEPART(SECOND, X.Duration))) AS AverageEffective, " +
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
                                                 "SELECT {1}.SecondsElapsed, P.Name AS TargetName, CAST(P.Id AS nvarchar) AS TargetId, " +
                                                 "(SUM({1}.EffectiveDamage) + SUM({1}.AbsorbedAmount) + SUM({1}.BlockedAmount) + SUM({1}.InterceptedAmount) + SUM({1}.IgnoredAmount)) AS Total, " +
                                                 "SUM({1}.EffectiveDamage) AS Effective " +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.TargetPlayerId = P.Id " +
                                                 "WHERE {1}.EncounterId = {2} AND {1}.SourcePlayerId = {3} AND {1}.{4} > 0 AND {1}.TargetPlayerId IS NOT NULL " +
                                                 "GROUP BY {1}.SecondsElapsed, P.Name, P.Id " +
                                                 "ORDER BY {1}.SecondsElapsed, TargetName",
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
                                        return string.Format("SELECT {0}.SecondsElapsed, A.Name + ' (' + {0}.SourceNpcName + ')' AS AbilityName, " +
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
                                        return string.Format("SELECT A.Name + ' (' + {1}.SourceNpcName + ')' AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
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
                                        return string.Format("SELECT {1}.SecondsElapsed, A.Name + ' (' + P.Name + ')' AS AbilityName, " +
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
                                        return string.Format("SELECT A.Name + ' (' + P.Name + ')' AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
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
                                        return string.Format("SELECT {1}.SecondsElapsed, A.Name + ' (' + P.Name + ')' AS AbilityName, " +
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

                                        return string.Format("SELECT A.Name + ' (' + P.Name + ')' AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
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
                                        return string.Format("SELECT {1}.SecondsElapsed, A.Name + ' (' + {1}.SourceNpcName + ')' AS AbilityName, " +
                                                InteractionBaseQueryPerSecond +
                                                "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                                BaseWhereIncoming + "AND {1}.SourceNpcId IS NOT NULL " +
                                                "GROUP BY {1}.SecondsElapsed, A.Name, {1}.SourceNpcName, {1}.SourceNpcId " +
                                                "UNION " +
                                                "SELECT {1}.SecondsElapsed, A.Name + ' (' + P.Name + ')' AS AbilityName, " +
                                                InteractionBaseQueryPerSecond +
                                                "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.SourcePlayerId = P.Id " +
                                                BaseWhereIncoming + "AND {1}.SourcePlayerId IS NOT NULL " +
                                                "GROUP BY {1}.SecondsElapsed, A.Name, P.Name, P.Id " +
                                                "ORDER BY {1}.SecondsElapsed, AbilityName",
                                                RecordTable, RecordTableAlias);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT A.Name + ' (' + {1}.SourceNpcName + ')' AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND TargetPlayerId = {3} AND SourceNpcId IS NOT NULL))) AS Percentage " +
                                               InteractionJoinNoPlayer + BaseWhereIncoming + " AND {1}.SourceNpcId IS NOT NULL " +
                                               "GROUP BY {1}.SourceNpcName, {1}.SourceNpcId, A.Name, A.DamageType, A.Icon, E.Duration " +
                                               "UNION " +
                                               "SELECT A.Name + ' (' + P.Name + ')' AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
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
                                                 "SELECT {1}.SecondsElapsed, P.Name AS SourceName, CAST(P.Id AS nvarchar) AS SourceId, " +
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
                            return string.Format("COUNT(CASE WHEN {0}.CriticalHit = 'true' THEN 1 END) AS Crits, " +
                                    "count(CASE WHEN {0}.CriticalHit = 'false' THEN 1 END) AS Hits, " +
                                    "MAX({0}.TotalHealing) AS BiggestHit, AVG({0}.TotalHealing) AS AverageHit, " +
                                    "SUM({0}.TotalHealing) AS Total, " +
                                    "SUM({0}.EffectiveHealing) AS Effective, " +
                                    "SUM({0}.OverhealAmount) AS Overhealing, " +
                                    "(SUM(TotalHealing) / ((DATEPART(HOUR, E.Duration) * 3600) + " +
                                    "(DATEPART(MINUTE, E.Duration) * 60) + DATEPART(SECOND, E.Duration))) AS Average, " +
                                    "(SUM(EffectiveHealing) / ((DATEPART(HOUR, E.Duration) * 3600) + " +
                                    "(DATEPART(MINUTE, E.Duration) * 60) + DATEPART(SECOND, E.Duration))) AS AverageEffective, " +
                                    "((100.0 * (SUM({0}.EffectiveHealing)) / (SELECT (SUM(EffectiveHealing)) ", RecordTableAlias);
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
                                               "ORDER BY {1}.SecondsElapsed, AbilityName",
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
                                        return string.Format("SELECT {1}.SecondsElapsed, {1}.TargetNpcName AS TargetName, {2}.TargetNpcId AS TargetId, " +
                                                 "SUM({1}.TotalHealing) AS Total, " +
                                                 "SUM({1}.EffectiveHealing) AS Effective, " +
                                                 "SUM({1}.OverhealAmount) AS Overhealing " +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                                 "WHERE {1}.EncounterId = {2} AND {1}.SourcePlayerId = {3} AND {1}.{4} > 0 AND {1}.TargetNpcId IS NOT NULL " +
                                                 "GROUP BY {1}.SecondsElapsed, {1}.TargetNpcName, {1}.TargetNpcId " +
                                                 "UNION " +
                                                 "SELECT {1}.SecondsElapsed, P.Name AS TargetName, CAST(P.Id AS nvarchar) AS TargetId, " +
                                                 "SUM({1}.TotalHealing) AS Total, " +
                                                 "SUM({1}.EffectiveHealing) AS Effective, " +
                                                 "SUM({1}.OverhealAmount) AS Overhealing " +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.TargetPlayerId = P.Id " +
                                                 "WHERE {1}.EncounterId = {2} AND {1}.SourcePlayerId = {3} AND {1}.{4} > 0 AND {1}.TargetPlayerId IS NOT NULL " +
                                                 "GROUP BY {1}.SecondsElapsed, P.Name, P.Id " +
                                                 "ORDER BY {1}.SecondsElapsed, TargetName",
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
                                        return string.Format("SELECT {0}.SecondsElapsed, A.Name + ' (' + {0}.SourceNpcName + ')' AS AbilityName, " +
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
                                        return string.Format("SELECT A.Name + ' (' + {1}.SourceNpcName + ')' AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
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
                                        return string.Format("SELECT {1}.SecondsElapsed, A.Name + ' (' + P.Name + ')' AS AbilityName, " +
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
                                        return string.Format("SELECT A.Name + ' (' + P.Name + ')' AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
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
                                        return string.Format("SELECT {1}.SecondsElapsed, A.Name + ' (' + P.Name + ')' AS AbilityName, " +
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

                                        return string.Format("SELECT A.Name + ' (' + P.Name + ')' AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
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
                                        return string.Format("SELECT {1}.SecondsElapsed, A.Name + ' (' + {1}.SourceNpcName + ')' AS AbilityName, " +
                                                InteractionBaseQueryPerSecond +
                                                "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                                BaseWhereIncoming + "AND {1}.SourceNpcId IS NOT NULL " +
                                                "GROUP BY {1}.SecondsElapsed, A.Name, {1}.SourceNpcName, {1}.SourceNpcId " +
                                                "UNION " +
                                                "SELECT {1}.SecondsElapsed, A.Name + ' (' + P.Name + ')' AS AbilityName, " +
                                                InteractionBaseQueryPerSecond +
                                                "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.SourcePlayerId = P.Id " +
                                                BaseWhereIncoming + "AND {1}.SourcePlayerId IS NOT NULL " +
                                                "GROUP BY {1}.SecondsElapsed, A.Name, P.Name, P.Id " +
                                                "ORDER BY {1}.SecondsElapsed, AbilityName",
                                                RecordTable, RecordTableAlias);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT A.Name + ' (' + {1}.SourceNpcName + ')' AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND TargetPlayerId = {3} AND SourceNpcId IS NOT NULL))) AS Percentage " +
                                               InteractionJoinNoPlayer + BaseWhereIncoming + " AND {1}.SourceNpcId IS NOT NULL " +
                                               "GROUP BY {1}.SourceNpcName, {1}.SourceNpcId, A.Name, A.DamageType, A.Icon, E.Duration " +
                                               "UNION " +
                                               "SELECT A.Name + ' (' + P.Name + ')' AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
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
                                            "SELECT {1}.SecondsElapsed, P.Name AS SourceName, CAST(P.Id AS nvarchar) AS SourceId, " + InteractionBaseQueryPerSecond +
                                            "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.SourcePlayerId = P.Id " +
                                            BaseWhereIncoming + "AND {1}.SourcePlayerId IS NOT NULL GROUP BY {1}.SecondsElapsed, P.Name, P.Id " +
                                            "ORDER BY {1}.SecondsElapsed, SourceName",
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
                            return string.Format("COUNT(CASE WHEN {0}.CriticalHit = 'true' THEN 1 END) AS Crits, " +
                                    "count(CASE WHEN {0}.CriticalHit = 'false' THEN 1 END) AS Hits, " +
                                    "MAX({0}.ShieldValue) AS BiggestHit, AVG({0}.ShieldValue) AS AverageHit, " +
                                    "SUM({0}.ShieldValue) AS Total, " +
                                    "(SUM(ShieldValue) / ((DATEPART(HOUR, E.Duration) * 3600) + " +
                                    "(DATEPART(MINUTE, E.Duration) * 60) + DATEPART(SECOND, E.Duration))) AS Average, " +
                                    "((100.0 * (SUM({0}.ShieldValue)) / (SELECT (SUM(ShieldValue)) ", RecordTableAlias);
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
                                               "ORDER BY {1}.SecondsElapsed, AbilityName",
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
                                                 "SELECT {1}.SecondsElapsed, P.Name AS TargetName, CAST(P.Id AS nvarchar) AS TargetId, " +
                                                 "SUM({1}.ShieldValue) AS Total " +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.TargetPlayerId = P.Id " +
                                                 "WHERE {1}.EncounterId = {2} AND {1}.SourcePlayerId = {3} AND {1}.{4} > 0 AND {1}.TargetPlayerId IS NOT NULL " +
                                                 "GROUP BY {1}.SecondsElapsed, P.Name, P.Id " +
                                                 "ORDER BY {1}.SecondsElapsed, TargetName",
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
                                        return string.Format("SELECT {0}.SecondsElapsed, A.Name + ' (' + {0}.SourceNpcName + ')' AS AbilityName, " +
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
                                        return string.Format("SELECT A.Name + ' (' + {1}.SourceNpcName + ')' AS AbilityName, A.Icon, " + InteractionBaseQueryTotals +
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
                                        return string.Format("SELECT {1}.SecondsElapsed, A.Name + ' (' + P.Name + ')' AS AbilityName, " +
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
                                        return string.Format("SELECT A.Name + ' (' + P.Name + ')' AS AbilityName, A.Icon, " + InteractionBaseQueryTotals +
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
                                        return string.Format("SELECT {1}.SecondsElapsed, A.Name + ' (' + P.Name + ')' AS AbilityName, " +
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

                                        return string.Format("SELECT A.Name + ' (' + P.Name + ')' AS AbilityName, A.Icon, " + InteractionBaseQueryTotals +
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
                                        return string.Format("SELECT {1}.SecondsElapsed, A.Name + ' (' + {1}.SourceNpcName + ')' AS AbilityName, " +
                                                InteractionBaseQueryPerSecond +
                                                "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                                BaseWhereIncoming + "AND {1}.SourceNpcId IS NOT NULL " +
                                                "GROUP BY {1}.SecondsElapsed, A.Name, {1}.SourceNpcName, {1}.SourceNpcId " +
                                                "UNION " +
                                                "SELECT {1}.SecondsElapsed, A.Name + ' (' + P.Name + ')' AS AbilityName, " +
                                                InteractionBaseQueryPerSecond +
                                                "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.SourcePlayerId = P.Id " +
                                                BaseWhereIncoming + "AND {1}.SourcePlayerId IS NOT NULL " +
                                                "GROUP BY {1}.SecondsElapsed, A.Name, P.Name, P.Id " +
                                                "ORDER BY {1}.SecondsElapsed, AbilityName",
                                                RecordTable, RecordTableAlias);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT A.Name + ' (' + {1}.SourceNpcName + ')' AS AbilityName, A.Icon, " + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND TargetPlayerId = {3} AND SourceNpcId IS NOT NULL))) AS Percentage " +
                                               InteractionJoinNoPlayer + BaseWhereIncoming + " AND {1}.SourceNpcId IS NOT NULL " +
                                               "GROUP BY {1}.SourceNpcName, {1}.SourceNpcId, A.Name, A.Icon, E.Duration " +
                                               "UNION " +
                                               "SELECT A.Name + ' (' + P.Name + ')' AS AbilityName, A.Icon, " + InteractionBaseQueryTotals +
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
                                            "SELECT {1}.SecondsElapsed, P.Name AS SourceName, CAST(P.Id AS nvarchar) AS SourceId, " + InteractionBaseQueryPerSecond +
                                            "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.SourcePlayerId = P.Id " +
                                            BaseWhereIncoming + "AND {1}.SourcePlayerId IS NOT NULL GROUP BY {1}.SecondsElapsed, P.Name, P.Id " +
                                            "ORDER BY {1}.SecondsElapsed, SourceName",
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
                            return string.Format("COUNT(CASE WHEN {0}.CriticalHit = 'true' THEN 1 END) AS Crits, " +
                                    "count(CASE WHEN {0}.CriticalHit = 'false' THEN 1 END) AS Hits, " +
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
                                    "+ SUM(InterceptedAmount) + SUM(IgnoredAmount)) / ((DATEPART(HOUR, E.Duration) * 3600) + " +
                                    "(DATEPART(MINUTE, E.Duration) * 60) + DATEPART(SECOND, E.Duration))) AS Average, " +
                                    "(SUM(EffectiveDamage) / ((DATEPART(HOUR, E.Duration) * 3600) + " +
                                    "(DATEPART(MINUTE, E.Duration) * 60) + DATEPART(SECOND, E.Duration))) AS AverageEffective, " +
                                    "((100.0 * (SUM({0}.EffectiveDamage) + SUM({0}.AbsorbedAmount)) /  " +
                                    "(SELECT (SUM(EffectiveDamage) + SUM(AbsorbedAmount)) ", RecordTableAlias);
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
                                               "ORDER BY {1}.SecondsElapsed, AbilityName",
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
                                                 "SELECT {1}.SecondsElapsed, P.Name AS TargetName, CAST(P.Id AS nvarchar) AS TargetId, " +
                                                 "(SUM({1}.EffectiveDamage) + SUM({1}.AbsorbedAmount) + SUM({1}.BlockedAmount) + SUM({1}.InterceptedAmount) + SUM({1}.IgnoredAmount)) AS Total, " +
                                                 "SUM({1}.EffectiveDamage) AS Effective " +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.TargetPlayerId = P.Id " +
                                                 "WHERE {1}.EncounterId = {2} AND {1}.SourceNpcId = {3} AND {1}.{4} > 0 AND {1}.TargetPlayerId IS NOT NULL " +
                                                 "GROUP BY {1}.SecondsElapsed, P.Name, P.Id " +
                                                 "ORDER BY {1}.SecondsElapsed, TargetName",
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
                                        return string.Format("SELECT {0}.SecondsElapsed, A.Name + ' (' + P.Name + ')' AS AbilityName, " +
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
                                        return string.Format("SELECT A.Name + ' (' + P.Name + ')' AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
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
                                        return string.Format("SELECT {1}.SecondsElapsed, A.Name + ' (' + {1}.SourceNpcName + ')' AS AbilityName, " +
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
                                        return string.Format("SELECT A.Name + ' (' + {1}.SourceNpcName + ')' AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
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
                                        return string.Format("SELECT {1}.SecondsElapsed, A.Name + ' (' + {1}.SourceNpcName + ')' AS AbilityName, " +
                                                InteractionBaseQueryPerSecond +
                                                "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                                BaseWhereIncoming + "AND {1}.SourceNpcId IS NOT NULL " +
                                                "GROUP BY {1}.SecondsElapsed, A.Name, {1}.SourceNpcName, {1}.SourceNpcId " +
                                                "UNION " +
                                                "SELECT {1}.SecondsElapsed, A.Name + ' (' + P.Name + ')' AS AbilityName, " +
                                                InteractionBaseQueryPerSecond +
                                                "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.SourcePlayerId = P.Id " +
                                                BaseWhereIncoming + "AND {1}.SourcePlayerId IS NOT NULL " +
                                                "GROUP BY {1}.SecondsElapsed, A.Name, P.Name, P.Id " +
                                                "ORDER BY {1}.SecondsElapsed, AbilityName",
                                                RecordTable, RecordTableAlias);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT A.Name + ' (' + {1}.SourceNpcName + ')' AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND TargetNpcId = {3} AND SourceNpcId IS NOT NULL))) AS Percentage " +
                                               InteractionJoinNoPlayer + BaseWhereIncoming + " AND {1}.SourceNpcId IS NOT NULL " +
                                               "GROUP BY {1}.SourceNpcName, {1}.SourceNpcId, A.Name, A.DamageType, A.Icon, E.Duration " +
                                               "UNION " +
                                               "SELECT A.Name + ' (' + P.Name + ')' AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
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
                                                 "SELECT {1}.SecondsElapsed, P.Name AS SourceName, CAST(P.Id AS nvarchar) AS SourceId, " +
                                                 "(SUM({1}.EffectiveDamage) + SUM({1}.AbsorbedAmount) + SUM({1}.BlockedAmount) + " +
                                                 "SUM({1}.InterceptedAmount) + SUM({1}.IgnoredAmount)) AS Total, " +
                                                 "SUM({1}.EffectiveDamage) AS Effective " +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.SourcePlayerId = P.Id " +
                                                 "WHERE {1}.EncounterId = {2} AND {1}.TargetNpcId = {3} AND {1}.{4} > 0 AND {1}.SourcePlayerId IS NOT NULL " +
                                                 "GROUP BY {1}.SecondsElapsed, P.Name, P.Id " +
                                                 "ORDER BY {1}.SecondsElapsed, SourceName",
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
                                               "SELECT P.Name AS SourceName, CAST(P.Id AS nvarchar) AS SourceId," + InteractionBaseQueryTotals +
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
                            return string.Format("COUNT(CASE WHEN {0}.CriticalHit = 'true' THEN 1 END) AS Crits, " +
                                    "count(CASE WHEN {0}.CriticalHit = 'false' THEN 1 END) AS Hits, " +
                                    "MAX({0}.TotalHealing) AS BiggestHit, AVG({0}.TotalHealing) AS AverageHit, " +
                                    "SUM({0}.TotalHealing) AS Total, " +
                                    "SUM({0}.EffectiveHealing) AS Effective, " +
                                    "SUM({0}.OverhealAmount) AS Overhealing, " +
                                    "(SUM(TotalHealing) / ((DATEPART(HOUR, E.Duration) * 3600) + " +
                                    "(DATEPART(MINUTE, E.Duration) * 60) + DATEPART(SECOND, E.Duration))) AS Average, " +
                                    "(SUM(EffectiveHealing) / ((DATEPART(HOUR, E.Duration) * 3600) + " +
                                    "(DATEPART(MINUTE, E.Duration) * 60) + DATEPART(SECOND, E.Duration))) AS AverageEffective, " +
                                    "((100.0 * (SUM({0}.EffectiveHealing)) / (SELECT (SUM(EffectiveHealing)) ", RecordTableAlias);
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
                                               "ORDER BY {1}.SecondsElapsed, AbilityName",
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
                                                 "SELECT {1}.SecondsElapsed, P.Name AS TargetName, CAST(P.Id AS nvarchar) AS TargetId, " +
                                                 "SUM({1}.TotalHealing) AS Total, " +
                                                 "SUM({1}.EffectiveHealing) AS Effective, " +
                                                 "SUM({1}.OverhealAmount) AS Overhealing " +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.TargetPlayerId = P.Id " +
                                                 "WHERE {1}.EncounterId = {2} AND {1}.SourceNpcId = {3} AND {1}.{4} > 0 AND {1}.TargetPlayerId IS NOT NULL " +
                                                 "GROUP BY {1}.SecondsElapsed, P.Name, P.Id " +
                                                 "ORDER BY {1}.SecondsElapsed, TargetName",
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
                                        return string.Format("SELECT {0}.SecondsElapsed, A.Name + ' (' + {0}.SourceNpcName + ')' AS AbilityName, " +
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
                                        return string.Format("SELECT A.Name + ' (' + {1}.SourceNpcName + ')' AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
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
                                        return string.Format("SELECT {1}.SecondsElapsed, A.Name + ' (' + P.Name + ')' AS AbilityName, " +
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
                                        return string.Format("SELECT A.Name + ' (' + P.Name + ')' AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
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
                                        return string.Format("SELECT {1}.SecondsElapsed, A.Name + ' (' + {1}.SourceNpcName + ')' AS AbilityName, " +
                                                InteractionBaseQueryPerSecond +
                                                "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                                BaseWhereIncoming + "AND {1}.SourceNpcId IS NOT NULL " +
                                                "GROUP BY {1}.SecondsElapsed, A.Name, {1}.SourceNpcName, {1}.SourceNpcId " +
                                                "UNION " +
                                                "SELECT {1}.SecondsElapsed, A.Name + ' (' + P.Name + ')' AS AbilityName, " +
                                                InteractionBaseQueryPerSecond +
                                                "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.SourcePlayerId = P.Id " +
                                                BaseWhereIncoming + "AND {1}.SourcePlayerId IS NOT NULL " +
                                                "GROUP BY {1}.SecondsElapsed, A.Name, P.Name, P.Id " +
                                                "ORDER BY {1}.SecondsElapsed, AbilityName",
                                                RecordTable, RecordTableAlias);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT A.Name + ' (' + {1}.SourceNpcName + ')' AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND TargetNpcId = {3} AND SourceNpcId IS NOT NULL))) AS Percentage " +
                                               InteractionJoinNoPlayer + BaseWhereIncoming + " AND {1}.SourceNpcId IS NOT NULL " +
                                               "GROUP BY {1}.SourceNpcName, {1}.SourceNpcId, A.Name, A.DamageType, A.Icon, E.Duration " +
                                               "UNION " +
                                               "SELECT A.Name + ' (' + P.Name + ')' AS AbilityName, A.DamageType, A.Icon, " + InteractionBaseQueryTotals +
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
                                            "SELECT {1}.SecondsElapsed, P.Name AS SourceName, CAST(P.Id AS nvarchar) AS SourceId, " + InteractionBaseQueryPerSecond +
                                            "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.SourcePlayerId = P.Id " +
                                            BaseWhereIncoming + "AND {1}.SourcePlayerId IS NOT NULL GROUP BY {1}.SecondsElapsed, P.Name, P.Id " +
                                            "ORDER BY {1}.SecondsElapsed, SourceName",
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
                            return string.Format("COUNT(CASE WHEN {0}.CriticalHit = 'true' THEN 1 END) AS Crits, " +
                                    "count(CASE WHEN {0}.CriticalHit = 'false' THEN 1 END) AS Hits, " +
                                    "MAX({0}.ShieldValue) AS BiggestHit, AVG({0}.ShieldValue) AS AverageHit, " +
                                    "SUM({0}.ShieldValue) AS Total, " +
                                    "(SUM(ShieldValue) / ((DATEPART(HOUR, E.Duration) * 3600) + " +
                                    "(DATEPART(MINUTE, E.Duration) * 60) + DATEPART(SECOND, E.Duration))) AS Average, " +
                                    "((100.0 * (SUM({0}.ShieldValue)) / (SELECT (SUM(ShieldValue)) ", RecordTableAlias);
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
                                               "ORDER BY {1}.SecondsElapsed, AbilityName",
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
                                                 "SELECT {1}.SecondsElapsed, P.Name AS TargetName, CAST(P.Id AS nvarchar) AS TargetId, " +
                                                 "SUM({1}.ShieldValue) AS Total " +
                                                 "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.TargetPlayerId = P.Id " +
                                                 "WHERE {1}.EncounterId = {2} AND {1}.SourceNpcId = {3} AND {1}.{4} > 0 AND {1}.TargetPlayerId IS NOT NULL " +
                                                 "GROUP BY {1}.SecondsElapsed, P.Name, P.Id " +
                                                 "ORDER BY {1}.SecondsElapsed, TargetName",
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
                                        return string.Format("SELECT {0}.SecondsElapsed, A.Name + ' (' + {0}.SourceNpcName + ')' AS AbilityName, " +
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
                                        return string.Format("SELECT A.Name + ' (' + {1}.SourceNpcName + ')' AS AbilityName, A.Icon, " + InteractionBaseQueryTotals +
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
                                        return string.Format("SELECT {1}.SecondsElapsed, A.Name + ' (' + P.Name + ')' AS AbilityName, " +
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
                                        return string.Format("SELECT A.Name + ' (' + P.Name + ')' AS AbilityName, A.Icon, " + InteractionBaseQueryTotals +
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
                                        return string.Format("SELECT A.Name + ' (' + {1}.SourceNpcName + ')' AS AbilityName, A.Icon, " + InteractionBaseQueryTotals +
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
                                        return string.Format("SELECT {1}.SecondsElapsed, A.Name + ' (' + {1}.SourceNpcName + ')' AS AbilityName, " +
                                                InteractionBaseQueryPerSecond +
                                                "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id " +
                                                BaseWhereIncoming + "AND {1}.SourceNpcId IS NOT NULL " +
                                                "GROUP BY {1}.SecondsElapsed, A.Name, {1}.SourceNpcName, {1}.SourceNpcId " +
                                                "UNION " +
                                                "SELECT {1}.SecondsElapsed, A.Name + ' (' + P.Name + ')' AS AbilityName, " +
                                                InteractionBaseQueryPerSecond +
                                                "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.SourcePlayerId = P.Id " +
                                                BaseWhereIncoming + "AND {1}.SourcePlayerId IS NOT NULL " +
                                                "GROUP BY {1}.SecondsElapsed, A.Name, P.Name, P.Id " +
                                                "ORDER BY {1}.SecondsElapsed, AbilityName",
                                                RecordTable, RecordTableAlias);
                                    }
                                }

                                public static string Totals
                                {
                                    get
                                    {
                                        return string.Format("SELECT A.Name + ' (' + {1}.SourceNpcName + ')' AS AbilityName, A.Icon, " + InteractionBaseQueryTotals +
                                               "FROM {0} WHERE EncounterId = {2} AND TargetNpcId = {3} AND SourceNpcId IS NOT NULL))) AS Percentage " +
                                               InteractionJoinNoPlayer + BaseWhereIncoming + " AND {1}.SourceNpcId IS NOT NULL " +
                                               "GROUP BY {1}.SourceNpcName, {1}.SourceNpcId, A.Name, A.Icon, E.Duration " +
                                               "UNION " +
                                               "SELECT A.Name + ' (' + P.Name + ')' AS AbilityName, A.Icon, " + InteractionBaseQueryTotals +
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
                                            "SELECT {1}.SecondsElapsed, P.Name AS SourceName, CAST(P.Id AS nvarchar) AS SourceId, " + InteractionBaseQueryPerSecond +
                                            "FROM {0} {1} JOIN Ability A ON {1}.AbilityId = A.Id JOIN Player P ON {1}.SourcePlayerId = P.Id " +
                                            BaseWhereIncoming + "AND {1}.SourcePlayerId IS NOT NULL GROUP BY {1}.SecondsElapsed, P.Name, P.Id " +
                                            "ORDER BY {1}.SecondsElapsed, SourceName",
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


            /// <summary>
            /// The query to return player damage done for an encounter per second, by ability.
            /// Requires the parameter @id for Encounter ID and @playerId for Player ID
            /// </summary>
            public static string PlayerDamageDoneAbilityPerSecondAllTargets
            {
                get
                {
                    return @"SELECT DD.SecondsElapsed, A.Name AS AbilityName, (SUM(DD.EffectiveDamage) + SUM(DD.AbsorbedAmount)) AS Total, SUM(DD.EffectiveDamage) AS Effective
                             FROM DamageDone DD JOIN Ability A ON DD.AbilityId = A.Id
                             WHERE DD.EncounterId = @id AND DD.SourcePlayerId = @playerId
                             GROUP BY DD.SecondsElapsed, A.Name
                             ORDER BY DD.SecondsElapsed, A.Name";
                }
            }

            public static string PlayerDamageDoneAbilityPerSecondAllTargetsBreakdown
            {
                get
                {
                    return @"SELECT A.Name AS AbilityName, A.DamageType, A.Icon, 
                             COUNT(CASE WHEN DD.CriticalHit = 'true' THEN 1 END) AS Crits, count(CASE WHEN DD.CriticalHit = 'false' THEN 1 END) AS Hits,
                             MAX(DD.TotalDamage) AS BiggestHit, AVG(DD.TotalDamage) AS AverageHit,
                             (SUM(DD.EffectiveDamage) + SUM(DD.AbsorbedAmount)) AS Total, SUM(DD.EffectiveDamage) AS Effective,
                             ((SUM(EffectiveDamage) + SUM(AbsorbedAmount)) / ((DATEPART(HOUR, E.Duration) * 3600) +
                             (DATEPART(MINUTE, E.Duration) * 60) + DATEPART(SECOND, E.Duration))) AS Average,
                             ((100.0 * (SUM(DD.EffectiveDamage) + SUM(DD.AbsorbedAmount)) / 
                             (SELECT (SUM(EffectiveDamage) + SUM(AbsorbedAmount)) FROM DamageDone WHERE EncounterId = @id AND SourcePlayerId = @playerId))) AS Percentage
                             FROM DamageDone DD JOIN Ability A ON DD.AbilityId = A.Id JOIN Encounter E ON DD.EncounterId = E.Id
                             WHERE DD.EncounterId = @id AND DD.SourcePlayerId = @playerId
                             GROUP BY A.Name, A.DamageType, A.Icon, E.Duration
                             ORDER BY Total DESC";
                }
            }

            //TODO: INCLUDE EFFECTIVE DAMAGE IN THIS BREAKDOWN!!! (ABSORBS VS REAL DAMAGE)
            [Obsolete("Use Player.Damage.Taken.AllSources.Totals instead", true)]
            public static string PlayerDamageTakenAbilityPerSecondAllSourcesBreakdown
            {
                get
                {
                    return @"SELECT A.Name AS AbilityName, A.DamageType, A.Icon, 
                             COUNT(CASE WHEN DD.CriticalHit = 'true' THEN 1 END) AS Crits, count(CASE WHEN DD.CriticalHit = 'false' THEN 1 END) AS Hits,
                             MAX(DD.TotalDamage) AS BiggestHit, AVG(DD.TotalDamage) AS AverageHit,
                             (SUM(DD.EffectiveDamage) + SUM(DD.AbsorbedAmount)) AS Total, SUM(DD.EffectiveDamage) AS Effective,
                             ((SUM(EffectiveDamage) + SUM(AbsorbedAmount)) / ((DATEPART(HOUR, E.Duration) * 3600) +
                             (DATEPART(MINUTE, E.Duration) * 60) + DATEPART(SECOND, E.Duration))) AS Average,
                             ((100.0 * (SUM(DD.EffectiveDamage) + SUM(DD.AbsorbedAmount)) / 
                             (SELECT (SUM(EffectiveDamage) + SUM(AbsorbedAmount)) FROM DamageDone WHERE EncounterId = @id AND TargetPlayerId = @playerId))) AS Percentage
                             FROM DamageDone DD JOIN Ability A ON DD.AbilityId = A.Id JOIN Encounter E ON DD.EncounterId = E.Id
                             WHERE DD.EncounterId = @id AND DD.TargetPlayerId = @playerId
                             GROUP BY A.Name, A.DamageType, A.Icon, E.Duration
                             ORDER BY Total DESC";
                }
            }

            public static string PlayerDamageDoneAbilityPerSecondOnlyNpcs
            {
                get
                {
                    return @"SELECT DD.SecondsElapsed, A.Name AS AbilityName, (SUM(DD.EffectiveDamage) + SUM(DD.AbsorbedAmount)) AS Total, SUM(DD.EffectiveDamage) AS Effective
                             FROM DamageDone DD JOIN Ability A ON DD.AbilityId = A.Id
                             WHERE DD.EncounterId = @id AND DD.SourcePlayerId = @playerId AND DD.TargetNpcId IS NOT NULL
                             GROUP BY DD.SecondsElapsed, A.Name
                             ORDER BY DD.SecondsElapsed, A.Name";
                }
            }

            public static string PlayerDamageDoneAbilityPerSecondOnlyPlayers
            {
                get
                {
                    return @"SELECT DD.SecondsElapsed, A.Name AS AbilityName, (SUM(DD.EffectiveDamage) + SUM(DD.AbsorbedAmount)) AS Total, SUM(DD.EffectiveDamage) AS Effective
                             FROM DamageDone DD JOIN Ability A ON DD.AbilityId = A.Id
                             WHERE DD.EncounterId = @id AND DD.SourcePlayerId = @playerId AND DD.TargetPlayerId IS NOT NULL
                             GROUP BY DD.SecondsElapsed, A.Name
                             ORDER BY DD.SecondsElapsed, A.Name";
                }
            }

            public static string PlayerDamageDoneAbilityPerSecondOnlySelf
            {
                get
                {
                    return @"SELECT DD.SecondsElapsed, A.Name AS AbilityName, (SUM(DD.EffectiveDamage) + SUM(DD.AbsorbedAmount)) AS Total, SUM(DD.EffectiveDamage) AS Effective
                             FROM DamageDone DD JOIN Ability A ON DD.AbilityId = A.Id
                             WHERE DD.EncounterId = @id AND DD.SourcePlayerId = @playerId AND DD.TargetPlayerId = @playerId
                             GROUP BY DD.SecondsElapsed, A.Name
                             ORDER BY DD.SecondsElapsed, A.Name";
                }
            }

            public static string PlayerDamageDoneTargetPerSecondAllTargets
            {
                get
                {
                    return @"SELECT DD.SecondsElapsed, DD.TargetNpcId AS TargetId, DD.TargetNpcName AS TargetName,
                             (SUM(DD.EffectiveDamage) + SUM(DD.AbsorbedAmount)) AS Total, SUM(DD.EffectiveDamage) AS Effective
                             FROM DamageDone DD JOIN Ability A ON DD.AbilityId = A.Id
                             WHERE DD.EncounterId = @id AND DD.SourcePlayerId = @playerId AND DD.TargetNpcId IS NOT NULL
                             GROUP BY DD.SecondsElapsed, TargetNpcId,  TargetNpcName
                             UNION ALL
                             SELECT DD.SecondsElapsed, P.PlayerId AS TargetId, P.Name AS TargetName,
                             (SUM(DD.EffectiveDamage) + SUM(DD.AbsorbedAmount)) AS Total, SUM(DD.EffectiveDamage) AS Effective
                             FROM DamageDone DD JOIN Ability A ON DD.AbilityId = A.Id JOIN Player P ON DD.TargetPlayerId = P.Id
                             WHERE DD.EncounterId = @id AND DD.SourcePlayerId = @playerId AND DD.TargetPlayerId IS NOT NULL
                             GROUP BY DD.SecondsElapsed, P.PlayerId,  P.Name
                             ORDER BY DD.SecondsElapsed";
                }
            }

            public static string PlayerDamageDoneTargetPerSecondAllTargetsBreakdown
            {
                get
                {
                    return @"SELECT TargetIdInt = 0, DD.TargetNpcId AS TargetIdString, DD.TargetNpcName AS TargetName, (SUM(DD.EffectiveDamage) + SUM(DD.AbsorbedAmount)) AS Total,
                             ((SUM(DD.EffectiveDamage) + SUM(DD.AbsorbedAmount)) / 
                             ((DATEPART(HOUR, E.Duration) * 3600) + (DATEPART(MINUTE, E.Duration) * 60) + DATEPART(SECOND, E.Duration))) AS Average,
                             ((100.0 * (SUM(DD.EffectiveDamage) + SUM(DD.AbsorbedAmount)) / 
                             (SELECT (SUM(EffectiveDamage) + SUM(AbsorbedAmount)) FROM DamageDone WHERE EncounterId = @id AND SourcePlayerId = @playerId))) AS Percentage,
                             COUNT(CASE WHEN DD.CriticalHit = 'true' THEN 1 END) AS Crits, count(CASE WHEN DD.CriticalHit = 'false' THEN 1 END) AS Hits,
                             MAX(DD.TotalDamage) AS BiggestHit, AVG(DD.TotalDamage) AS AverageHit
                             FROM DamageDone DD JOIN Ability A ON DD.AbilityId = A.Id JOIN Encounter E ON DD.EncounterId = E.Id
                             WHERE DD.EncounterId = @id AND DD.SourcePlayerId = @playerId AND DD.TargetNpcId IS NOT NULL
                             GROUP BY DD.TargetNpcName, DD.TargetNpcId, E.Duration
                             UNION ALL
                             SELECT P.Id AS TargetIdInt, TargetIdString = NULL, P.Name AS TargetName, (SUM(DD.EffectiveDamage) + SUM(DD.AbsorbedAmount)) AS Total,
                             ((SUM(DD.EffectiveDamage) + SUM(DD.AbsorbedAmount)) / 
                             ((DATEPART(HOUR, E.Duration) * 3600) + (DATEPART(MINUTE, E.Duration) * 60) + DATEPART(SECOND, E.Duration))) AS Average,
                             ((100.0 * (SUM(DD.EffectiveDamage) + SUM(DD.AbsorbedAmount)) / 
                             (SELECT (SUM(EffectiveDamage) + SUM(AbsorbedAmount)) FROM DamageDone WHERE EncounterId = @id AND SourcePlayerId = @playerId))) AS Percentage,
                             COUNT(CASE WHEN DD.CriticalHit = 'true' THEN 1 END) AS Crits, count(CASE WHEN DD.CriticalHit = 'false' THEN 1 END) AS Hits,
                             MAX(DD.TotalDamage) AS BiggestHit, AVG(DD.TotalDamage) AS AverageHit
                             FROM DamageDone DD JOIN Ability A ON DD.AbilityId = A.Id JOIN Encounter E ON DD.EncounterId = E.Id JOIN Player P ON DD.TargetPlayerId = P.Id
                             WHERE DD.EncounterId = @id AND DD.SourcePlayerId = @playerId
                             GROUP BY P.Name, P.Id, E.Duration
                             ORDER BY Total DESC";
                }
            }

            public static string PlayerDamageDoneTargetPerSecondOnlyNpcs
            {
                get
                {
                    return @"SELECT DD.SecondsElapsed, DD.TargetNpcId AS TargetId, DD.TargetNpcName AS TargetName,
                             (SUM(DD.EffectiveDamage) + SUM(DD.AbsorbedAmount)) AS Total, SUM(DD.EffectiveDamage) AS Effective
                             FROM DamageDone DD JOIN Ability A ON DD.AbilityId = A.Id
                             WHERE DD.EncounterId = @id AND DD.SourcePlayerId = @playerId AND DD.TargetNpcId IS NOT NULL
                             GROUP BY DD.SecondsElapsed, TargetNpcId,  TargetNpcName
                             ORDER BY DD.SecondsElapsed";
                }
            }

            public static string PlayerDamageDoneTargetPerSecondOnlyPlayers
            {
                get
                {
                    return @"SELECT DD.SecondsElapsed, P.PlayerId AS TargetId, P.Name AS TargetName,
                             (SUM(DD.EffectiveDamage) + SUM(DD.AbsorbedAmount)) AS Total, SUM(DD.EffectiveDamage) AS Effective
                             FROM DamageDone DD JOIN Ability A ON DD.AbilityId = A.Id JOIN Player P ON DD.TargetPlayerId = P.Id
                             WHERE DD.EncounterId = @id AND DD.SourcePlayerId = @playerId AND DD.TargetPlayerId IS NOT NULL
                             GROUP BY DD.SecondsElapsed, P.PlayerId,  P.Name
                             ORDER BY DD.SecondsElapsed";
                }
            }



            /// <summary>
            /// The query to return NPC damage done for an encounter. Requires the parameter @id for Encounter ID and @npcId for NPC ID
            /// </summary>
            public static string NpcDamageDoneOutgoingBasic
            {
                get
                {
                    return @"SELECT DD.SecondsElapsed, SUM(DD.TotalDamage) AS Total, SUM(DD.EffectiveDamage) AS Effective
                             FROM DamageDone DD
                             WHERE DD.EncounterId = @id AND DD.SourceNpcId = @npcId AND (DD.TargetNpcId IS NULL OR DD.TargetNpcId <> @npcId)
                             GROUP BY DD.SecondsElapsed
                             ORDER BY DD.SecondsElapsed";
                }
            }
            /// <summary>
            /// The query to return player healing done for an encounter. Requires the parameter @id for Encounter ID and @playerId for Player ID
            /// </summary>
            public static string PlayerHealingDoneBasic
            {
                get
                {
                    return @"SELECT HD.SecondsElapsed, A.Name AS AbilityName, SUM(HD.TotalHealing) AS Total, SUM(HD.EffectiveHealing) AS Effective
                             FROM HealingDone HD JOIN Ability A ON HD.AbilityId = A.Id
                             WHERE HD.EncounterId = @id AND HD.SourcePlayerId = @playerId AND (HD.TargetPlayerId IS NULL OR HD.TargetPlayerId <> @playerId)
                             GROUP BY HD.SecondsElapsed, A.Name
                             ORDER BY HD.SecondsElapsed, A.Name";
                }
            }
            public static string NpcHealingDoneBasic { get { return ""; } }
            public static string PlayerShieldingDoneBasic
            {
                get
                {
                    return @"SELECT SD.SecondsElapsed, A.Name AS AbilityName, SUM(SD.ShieldValue) AS Total, SUM(SD.ShieldValue) AS Effective
                             FROM ShieldingDone SD JOIN Ability A ON SD.AbilityId = A.Id
                             WHERE SD.EncounterId = @id AND SD.SourcePlayerId = @playerId AND SD.TargetPlayerId IS NOT NULL
                             GROUP BY SD.SecondsElapsed, A.Name
                             ORDER BY SD.SecondsElapsed, A.Name";
                }
            }
            public static string NpcShieldingDoneBasic { get { return ""; } }
            [Obsolete("Use Player.Damage.Taken.AllSources.PerSecond instead", true)]
            public static string PlayerDamageTakenBasic
            {
                get
                {
                    return @"SELECT DD.SecondsElapsed, A.Name AS AbilityName, (SUM(DD.EffectiveDamage) + SUM(DD.AbsorbedAmount)) AS Total, SUM(DD.EffectiveDamage) AS Effective
                             FROM DamageDone DD JOIN Ability A ON DD.AbilityId = A.Id
                             WHERE DD.EncounterId = @id AND DD.TargetPlayerId = @playerId AND DD.TotalDamage > 0
                             GROUP BY DD.SecondsElapsed, A.Name
                             ORDER BY DD.SecondsElapsed, A.Name";
                }
            }
            public static string NpcDamageTakenBasic { get { return ""; } }
            public static string PlayerHealingTakenBasic
            {
                get
                {
                    return @"SELECT HD.SecondsElapsed, A.Name AS AbilityName, SUM(HD.TotalHealing) AS Total, SUM(HD.EffectiveHealing) AS Effective
                             FROM HealingDone HD JOIN Ability A ON HD.AbilityId = A.Id
                             WHERE HD.EncounterId = @id AND HD.TargetPlayerId = @playerId
                             GROUP BY HD.SecondsElapsed, A.Name
                             ORDER BY HD.SecondsElapsed, A.Name";
                }
            }
            public static string NpcHealingTakenBasic { get { return ""; } }
            public static string PlayerShieldingTakenBasic
            {
                get
                {
                    return @"SELECT SD.SecondsElapsed, A.Name AS AbilityName, SUM(SD.ShieldValue) AS Total, SUM(SD.ShieldValue) AS Effective
                             FROM ShieldingDone SD JOIN Ability A ON SD.AbilityId = A.Id
                             WHERE SD.EncounterId = @id AND SD.TargetPlayerId = @playerId
                             GROUP BY SD.SecondsElapsed, A.Name
                             ORDER BY SD.SecondsElapsed, A.Name";
                }
            }
            public static string NpcShieldingTakenBasic { get { return ""; } }

            /// <summary>
            /// The query to return the target breakdown for outgoing player damage.
            /// Requires the parameter @id for Encounter ID and @playerId for Player ID
            /// </summary>
            [Obsolete("No longer used")]
            public static string PlayerDamageDoneTargetNpcBreakdown
            {
                get
                {
                    return @"SELECT DD.TargetNpcId AS TargetId, DD.TargetNpcName AS TargetName, (SUM(DD.EffectiveDamage) + SUM(DD.AbsorbedAmount)) AS Total,
                             ((SUM(DD.EffectiveDamage) + SUM(DD.AbsorbedAmount)) / 
                             ((DATEPART(HOUR, E.Duration) * 3600) + (DATEPART(MINUTE, E.Duration) * 60) + DATEPART(SECOND, E.Duration))) AS Average
                             FROM DamageDone DD JOIN Ability A ON DD.AbilityId = A.Id JOIN Encounter E ON DD.EncounterId = E.Id
                             WHERE DD.EncounterId = @id AND DD.SourcePlayerId = @playerId AND DD.TargetNpcId IS NOT NULL
                             GROUP BY DD.TargetNpcName, DD.TargetNpcId, E.Duration
                             ORDER BY Total DESC";
                }
            }
            [Obsolete("No longer used")]
            public static string PlayerDamageDoneTargetPlayerBreakdown
            {
                get
                {
                    return @"SELECT DD.TargetPlayerId AS TargetId, P.Name AS TargetName, (SUM(DD.EffectiveDamage) + SUM(DD.AbsorbedAmount)) AS Total,
                             ((SUM(DD.EffectiveDamage) + SUM(DD.AbsorbedAmount)) / 
                             ((DATEPART(HOUR, E.Duration) * 3600) + (DATEPART(MINUTE, E.Duration) * 60) + DATEPART(SECOND, E.Duration))) AS Average
                             FROM DamageDone DD JOIN Ability A ON DD.AbilityId = A.Id JOIN Encounter E ON DD.EncounterId = E.Id JOIN Player P ON TargetPlayerId = P.Id
                             WHERE DD.EncounterId = @id AND DD.SourcePlayerId = @playerId AND DD.TargetPlayerId <> @playerId
                             GROUP BY DD.TargetPlayerId, P.Name, E.Duration
                             ORDER BY Total DESC";
                }
            }
            /// <summary>
            /// The query to return the name of a player from an encounter.
            /// Requires the parameters @id for Encounter ID and @playerId for Player ID
            /// </summary>
            public static string PlayerName
            {
                get
                {
                    return @"SELECT TOP 1 P.Name AS Name FROM DamageDone D JOIN Player P ON D.TargetPlayerId = P.Id WHERE D.EncounterId = @id AND P.Id = @playerId";
                }
            }
            /// <summary>
            /// The query to return the name of an NPC from an encounter.
            /// Requires the parameters @id for Encounter ID and @npcId for NPC ID
            /// </summary>
            public static string NpcName
            {
                get
                {
                    return @"SELECT TOP 1 TargetNpcName AS Name FROM DamageDone D WHERE D.EncounterId = @id AND D.TargetNpcId = @npcId";
                }
            }
            /// <summary>
            /// The query to return all the buffs and their uptimes on a given target for a given encounter.
            /// Requires the parameters @id for EncounterID and @targetId for the buff's target (string ID, like 242000000494000000)
            /// </summary>
            public static string CharacterBuffs
            {
                get
                {
                    return @"SELECT DISTINCT EBA.BuffName, EBA.SourceName, EBA.SecondBuffWentUp, EBA.SecondBuffWentDown, A.Icon, S.Role 
                             FROM EncounterBuffAction EBA JOIN Ability A ON EBA.AbilityId = A.Id LEFT JOIN Soul S ON A.SoulId = S.Id
                             WHERE EBA.EncounterId = @id AND EBA.TargetId = @targetId ORDER BY EBA.BuffName, EBA.SourceName";
                }
            }
            /// <summary>
            /// The query to return all the debuffs and their uptimes on a given target for a given encounter.
            /// Requires the parameters @id for EncounterID and @targetId for the debuff's target (string ID, like 242000000494000000)
            /// </summary>
            public static string CharacterDebuffs
            {
                get
                {
                    return "SELECT EBA.DebuffName, EBA.SourceName, EBA.SecondDebuffWentUp, EBA.SecondDebuffWentDown, A.Icon " +
                           "FROM EncounterDebuffAction EBA JOIN Ability A ON EBA.AbilityId = A.Id " +
                           "WHERE EBA.EncounterId = @id AND EBA.TargetId = @targetId " +
                           "GROUP BY EBA.DebuffName, EBA.SourceName, EBA.SecondDebuffWentUp, EBA.SecondDebuffWentDown, A.Icon " +
                           "ORDER BY EBA.DebuffName, EBA.SourceName";
                }
            }
        }
    }
}
