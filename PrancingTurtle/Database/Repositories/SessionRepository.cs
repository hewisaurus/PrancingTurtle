using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Common;
using Dapper;
using Database.Models;
using Database.QueryModels;
using Database.QueryModels.Misc;
using Database.Repositories.Interfaces;
using Logging;

namespace Database.Repositories
{
    public class SessionRepository : DapperRepositoryBase, ISessionRepository
    {
        private readonly ILogger _logger;

        public SessionRepository(IConnectionFactory connectionFactory, ILogger logger)
            : base(connectionFactory)
        {
            _logger = logger;
        }

        public List<Session> GetAll(string username = "")
        {
            string timeElapsed;
            if (string.IsNullOrEmpty(username))
            {
                return Query(q => q.Query<Session, AuthUserCharacter, Shard, Guild, Session>
                    (MySQL.Session.GetAllSessionsUnauthenticated,
                        (s, auc, sh, g) =>
                        {
                            auc.Shard = sh;
                            if (g != null)
                            {
                                auc.Guild = g;
                            }
                            s.AuthUserCharacter = auc;
                            return s;
                        }), out timeElapsed).ToList();
            }

            return Query(q => q.Query<Session, AuthUserCharacter, Shard, Guild, Session>
                (MySQL.Session.GetAllSessionsAuthenticated,
                    (s, auc, sh, g) =>
                    {
                        auc.Shard = sh;
                        if (g != null)
                        {
                            auc.Guild = g;
                        }
                        s.AuthUserCharacter = auc;
                        return s;
                    }, new { @email = username }), out timeElapsed).ToList();

        }
        
        /// <summary>
        /// Updated for MySQL
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Session Get(int id)
        {
            string timeElapsed;
            return Query(q => q.Query<Session, AuthUserCharacter, Shard, Guild, Session>
                (MySQL.Session.GetSession, (s, auc, sh, g) =>
                {
                    auc.Shard = sh;
                    if (g != null)
                    {
                        auc.Guild = g;
                    }
                    s.AuthUserCharacter = auc;
                    return s;
                }, new { @id = id }), out timeElapsed).SingleOrDefault();
        }

        /// <summary>
        /// Updated for MySQL
        /// </summary>
        /// <param name="id"></param>
        /// <param name="includeEncountersMarkedForDeletion"></param>
        /// <returns></returns>
        public List<Encounter> GetEncounters(int id, bool includeEncountersMarkedForDeletion)
        {
            var query = includeEncountersMarkedForDeletion 
                ? MySQL.Session.GetEncountersWithKillTimeAndDpsRank 
                : MySQL.Session.GetEncountersWithKillTimeAndDpsRankHideDeleted;

            string timeElapsed;
            return Query(q => q.Query<Encounter, EncounterDifficulty, BossFight, Instance, EncounterOverview, Encounter>
                (query, (e, ed, bf, i, eo) =>
                {
                    bf.Instance = i;
                    e.BossFight = bf;
                    e.EncounterDifficulty = ed;
                    if (eo != null)
                    {
                        e.Overview = eo;
                    }
                    return e;
                }, new { id }), out timeElapsed).ToList();
        }

        /// <summary>
        /// Updated for MySQL
        /// </summary>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public int TotalSessions(int guildId)
        {
            string timeElapsed;
            return Convert.ToInt32(Query(q => q.Query<long>(MySQL.Session.CountGuildSessions, new { guildId }), out timeElapsed).SingleOrDefault());
        }

        /// <summary>
        /// Updated for MySQL
        /// </summary>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public List<Session> GetGuildSessions(int guildId)
        {
            string timeElapsed;
            return Query(q => q.Query<Session, AuthUserCharacter, Shard, Guild, Session>
                (MySQL.Session.GetGuildSessions,
                (s, auc, sh, g) =>
                {
                    auc.Shard = sh;
                    if (g != null)
                    {
                        auc.Guild = g;
                    }
                    s.AuthUserCharacter = auc;
                    return s;
                }, new { @id = guildId }), out timeElapsed).ToList();
        }

        public List<GuildSession> GetBossFightSessions(int bossFightId, string username, int difficultyId = -1)
        {
            string timeElapsed;

            if (string.IsNullOrEmpty(username))
            {
                return difficultyId != -1
                    ? Query(q => q.Query<GuildSession, Guild, GuildSession>
                        (MySQL.Session.GetSessionsForBossFightNoAuthWithDifficulty,
                            (gs, g) =>
                            {
                                gs.Guild = g;
                                return gs;
                            }, new { bossFightId, difficultyId }),
                        out timeElapsed).ToList()
                    : Query(q => q.Query<GuildSession, Guild, GuildSession>
                        (MySQL.Session.GetSessionsForBossFightNoAuth, (gs, g) =>
                        {
                            gs.Guild = g;
                            return gs;
                        }, new { bossFightId }),
                        out timeElapsed).ToList();

                //if (difficultyId != -1)
                //{
                //    return Query(q => q.Query<GuildSession, Guild, GuildSession>
                //    (MySQL.Session.GetSessionsForBossFightNoAuthWithDifficulty, 
                //    (gs, g) =>
                //    {
                //        gs.Guild = g;
                //        return gs;
                //    }, new { bossFightId, difficultyId }),
                //    out timeElapsed).ToList();
                //}
                //return Query(q => q.Query<GuildSession, Guild, GuildSession>
                //    (MySQL.Session.GetSessionsForBossFightNoAuth, (gs, g) =>
                //    {
                //        gs.Guild = g;
                //        return gs;
                //    }, new { bossFightId }),
                //    out timeElapsed).ToList();
            }

            return difficultyId != -1
                ? Query(q => q.Query<GuildSession, Guild, GuildSession>
                    (MySQL.Session.GetSessionsForBossFightWithAuthWithDifficulty, (gs, g) =>
                    {
                        gs.Guild = g;
                        return gs;
                    }, new { bossFightId, @email = username, difficultyId }),
                    out timeElapsed).ToList()
                : Query(q => q.Query<GuildSession, Guild, GuildSession>
                    (MySQL.Session.GetSessionsForBossFightWithAuth, (gs, g) =>
                    {
                        gs.Guild = g;
                        return gs;
                    }, new { bossFightId, @email = username }),
                    out timeElapsed).ToList();
        }

        public List<GuildSession> GetAllBossFightSessions(int bossFightId, int difficultyId = -1)
        {
            string timeElapsed;
            return
                difficultyId != -1
                ? Query(q => q.Query<GuildSession, Guild, GuildSession>
                    (MySQL.Session.GetAllSessionsForBossFightWithDifficulty, (gs, g) =>
                    {
                        gs.Guild = g;
                        return gs;
                    }, new { bossFightId, difficultyId }),
                    out timeElapsed).ToList()
                : Query(q => q.Query<GuildSession, Guild, GuildSession>
                    (MySQL.Session.GetAllSessionsForBossFight, (gs, g) =>
                    {
                        gs.Guild = g;
                        return gs;
                    }, new { bossFightId }),
                    out timeElapsed).ToList();
        }

        public List<GuildSession> GetInstanceSessions(int instanceId, string username)
        {
            string timeElapsed;

            if (string.IsNullOrEmpty(username))
            {
                return Query(q => q.Query<GuildSession, Guild, GuildSession>
                    (MySQL.Session.GetSessionsForInstanceNoAuth, (gs, g) =>
                    {
                        gs.Guild = g;
                        return gs;
                    }, new { instanceId }),
                    out timeElapsed).ToList();
            }

            return Query(q => q.Query<GuildSession, Guild, GuildSession>
                (MySQL.Session.GetSessionsForInstanceWithAuth, (gs, g) =>
                {
                    gs.Guild = g;
                    return gs;
                }, new { instanceId, @email = username }),
                    out timeElapsed).ToList();
        }

        public List<GuildSession> GetAllInstanceSessions(int instanceId)
        {
            string timeElapsed;
            return
                Query(q => q.Query<GuildSession, Guild, GuildSession>
                    (MySQL.Session.GetAllSessionsForInstance, (gs, g) =>
                    {
                        gs.Guild = g;
                        return gs;
                    }, new { instanceId }),
                    out timeElapsed).ToList();
        }
        
        public List<Session> GetPlayerSessions(int playerId, string username)
        {
            string timeElapsed;
            if (string.IsNullOrEmpty(username))
            {
                return Query(q => q.Query<Session>
                (MySQL.Session.GetPlayerSessionsNoAuth, new { playerId }), out timeElapsed).ToList();
            }

            return Query(q => q.Query<Session>(MySQL.Session.GetPlayerSessionsAuthenticated,
                new { playerId, @email = username }), out timeElapsed).ToList();

        }

        /// <summary>
        /// Gets the list of X most recent sessions. Updated for MySQL
        /// </summary>
        /// <param name="sessionCount"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        public List<Session> GetRecentSessions(int sessionCount, string email = "")
        {
            string timeElapsed;

            if (string.IsNullOrEmpty(email))
            {
                return Query(q => q.Query<Session, AuthUserCharacter, Shard, Guild, Session>
                (MySQL.Session.GetRecentSessionsNoAuth,
                (s, auc, sh, g) =>
                {
                    auc.Shard = sh;
                    if (g != null)
                    {
                        auc.Guild = g;
                    }
                    s.AuthUserCharacter = auc;
                    return s;
                }, new { sessionCount }), out timeElapsed).ToList();
            }
            else
            {
                return Query(q => q.Query<Session, AuthUserCharacter, Shard, Guild, Session>
                (MySQL.Session.GetRecentSessionsAuthenticated,
                (s, auc, sh, g) =>
                {
                    auc.Shard = sh;
                    if (g != null)
                    {
                        auc.Guild = g;
                    }
                    s.AuthUserCharacter = auc;
                    return s;
                }, new { sessionCount, email }), out timeElapsed).ToList();
            }
        }
        
        public List<Instance> GetInstancesSeen(int sessionId)
        {
            string timeElapsed;
            return
                Query(q => q.Query<Instance>(MySQL.Session.GetInstancesSeen, new { sessionId }), out timeElapsed).ToList();
        }

        public List<string> GetDifficultiesSeen(int sessionId, int instanceId)
        {
            string timeElapsed;
            return
                Query(q => q.Query<string>(MySQL.Session.GetDifficultiesSeen, new { sessionId, instanceId }), out timeElapsed).ToList();
        }

        public List<string> GetBossesKilled(int sessionId)
        {
            string timeElapsed;
            return
                Query(q => q.Query<string>(MySQL.Session.ListBossesKilled, new { sessionId }), out timeElapsed).ToList();
        }

        public List<string> GetBossesSeenButNotKilled(int sessionId)
        {
            string timeElapsed;
            return
                Query(q => q.Query<string>(MySQL.Session.ListBossesSeenButNotKilled, new { sessionId }), out timeElapsed).ToList();
        }
        
        public ReturnValue CreateSession(string email, Session session)
        {
            var returnValue = new ReturnValue();

            try
            {
                DapperDb dapperDb = DapperDb.Init(OpenConnection(), 3, false);
                _logger.Debug(string.Format("{0} is trying to create a session. Name: {1}, AuthUserCharacterId: {2}, Date: {3}, Public: {4}", email, session.Name, session.AuthUserCharacterId, session.Date, session.EncountersPublic));
                var newId = dapperDb.SessionTable.Insert(
                    new //Session()
                    {
                        Name = session.Name,
                        AuthUserCharacterId = session.AuthUserCharacterId,
                        Date = session.Date,
                        Duration = new TimeSpan(0, 0, 1).ToString(),
                        EncountersPublic = session.EncountersPublic
                    });

                if (newId != null)
                {
                    _logger.Debug(string.Format("{0} has successfully created a new session ({1}: {2})", email, newId, session.Name));
                    returnValue.Message = newId.ToString();
                    returnValue.Success = true;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("An error occurred while trying to create a session! {0}", ex.Message));
                returnValue.Message = ex.Message;
                returnValue.Success = false;
            }

            return returnValue;
        }

        public void RemoveSession(string email, int sessionId)
        {
            try
            {
                _logger.Debug(string.Format("{0} is beginning session removal process (session ID {1})", email, sessionId));

                string timeElapsed;

                var encounterIds = GetEncounters(sessionId, true).Select(e => e.Id).ToList();
                foreach (var encId in encounterIds)
                {
                    int id = encId;
                    _logger.Debug(string.Format("Deleting records for encounter ID {0}", id));
                    string deleteQuery = @"DELETE FROM EncounterOverview WHERE EncounterId = @id;
                                           DELETE FROM EncounterBuffEvent WHERE EncounterId = @id;
                                           DELETE FROM EncounterBuffUptime WHERE EncounterId = @id;
                                           DELETE FROM EncounterBuffAction WHERE EncounterId = @id;
                                           DELETE FROM EncounterDebuffAction WHERE EncounterId = @id;
                                           DELETE FROM EncounterNpcCast WHERE EncounterId = @id;
                                           DELETE FROM EncounterDeath WHERE EncounterId = @id;
                                           DELETE FROM DamageDone WHERE EncounterId = @id;
                                           DELETE FROM HealingDone WHERE EncounterId = @id;
                                           DELETE FROM ShieldingDone WHERE EncounterId = @id;
                                           DELETE FROM SessionEncounter WHERE EncounterId = @id;
                                           DELETE FROM EncounterPlayerRole WHERE EncounterId = @id;
                                           DELETE FROM EncounterNpc WHERE EncounterId = @id;
                                           DELETE FROM EncounterPlayerStatistics WHERE EncounterId = @id;
                                           DELETE FROM Encounter WHERE Id = @id;";
                    var result = Execute(s => s.Execute(deleteQuery, new { id }), out timeElapsed);
                    _logger.Debug(string.Format("Encounter ID {0} removed in {1}.", encId, timeElapsed));
                }

                const string deleteSessionQuery = @"DELETE FROM SessionEncounter WHERE SessionId = @sessionId;
                                                    DELETE FROM SessionLog WHERE SessionId = @sessionId;
                                                    DELETE FROM Session WHERE Id = @sessionId;";
                var deleteResult = Execute(s => s.Execute(deleteSessionQuery, new { sessionId }), out timeElapsed);
                _logger.Debug(string.Format("Session #{0} removed by {1}.", sessionId, email));
            }
            catch (Exception ex)
            {
                _logger.Debug(string.Format("The following error occurred while trying to remove session {0}: {1}", sessionId, ex.Message));
            }
        }

        public void ClearSession(string email, int sessionId)
        {
            try
            {
                _logger.Debug(string.Format("Session clearing process (session ID {0}) started by {1}", sessionId, email));

                string timeElapsed;

                var encounterIds = GetEncounters(sessionId, true).Select(e => e.Id).ToList();
                foreach (var encId in encounterIds)
                {
                    int id = encId;
                    _logger.Debug(string.Format("Deleting records for encounter ID {0}", id));
                    string deleteQuery = @"DELETE FROM EncounterOverview WHERE EncounterId = @id;
                                           DELETE FROM EncounterBuffEvent WHERE EncounterId = @id;
                                           DELETE FROM EncounterBuffUptime WHERE EncounterId = @id;
                                           DELETE FROM EncounterBuffAction WHERE EncounterId = @id;
                                           DELETE FROM EncounterDebuffAction WHERE EncounterId = @id;
                                           DELETE FROM EncounterNpcCast WHERE EncounterId = @id;
                                           DELETE FROM EncounterDeath WHERE EncounterId = @id;
                                           DELETE FROM DamageDone WHERE EncounterId = @id;
                                           DELETE FROM HealingDone WHERE EncounterId = @id;
                                           DELETE FROM ShieldingDone WHERE EncounterId = @id;
                                           DELETE FROM SessionEncounter WHERE EncounterId = @id;
                                           DELETE FROM EncounterPlayerRole WHERE EncounterId = @id;
                                           DELETE FROM EncounterNpc WHERE EncounterId = @id;
                                           DELETE FROM EncounterPlayerStatistics WHERE EncounterId = @id;
                                           DELETE FROM Encounter WHERE Id = @id;";
                    var result = Execute(s => s.Execute(deleteQuery, new { @id = id }), out timeElapsed);
                    _logger.Debug(string.Format("Encounter ID {0} removed in {1}.", encId, timeElapsed));
                }

                const string clearSessionQuery = @"DELETE FROM SessionEncounter WHERE SessionId = @sessionId";
                var deleteResult = Execute(s => s.Execute(clearSessionQuery, new { sessionId }), out timeElapsed);

                // Update tokens for each SessionLog for the Session
                var sessionLogs =
                    Query(q => q.Query<SessionLog>(SQL.Session.GetSessionLogs, new { sessionId }),
                        out timeElapsed).ToList();
                foreach (var sessionLog in sessionLogs)
                {
                    var token = sessionLog.Filename.Replace(".zip", "");
                    const string updateTokenQuery = "UPDATE SessionLog SET Token = @token WHERE Id = @id";
                    Execute(s => s.Execute(updateTokenQuery, new { @token = token, @id = sessionLog.Id }), out timeElapsed);
                }
                _logger.Debug(string.Format("Session #{0} cleared and enabled for reimport by {1}.", sessionId, email));
            }
            catch (Exception ex)
            {
                _logger.Debug(string.Format("The following error occurred while trying to clear session {0}: {1}", sessionId, ex.Message));
            }
        }

        public void ReimportSession(string email, int sessionId)
        {
            try
            {
                // Make sure that each log file gets copied to the folder for reimport
                string timeElapsed;

                var sessionLogs =
                    Query(q => q.Query<SessionLog>(SQL.Session.GetSessionLogs, new { @sessionId = sessionId }),
                        out timeElapsed).ToList();

                if (!sessionLogs.Any())
                {
                    _logger.Debug(string.Format("Couldn't reimport session {0} - session does not exist!", sessionId));
                    return;
                }

                foreach (var sessionLog in sessionLogs)
                {
                    string archivePath = string.Format(@"\\ptdc1.prancingturtle.com\ptarchive$\Guild{0}\Session{1}\{2}", sessionLog.GuildId, sessionLog.SessionId, sessionLog.Filename);
                    string destinationPath = string.Format(@"C:\PrancingTurtle\UploadedFiles\{0}", sessionLog.Filename);
                    //if (!File.Exists(archivePath))
                    //{
                    //    _logger.Debug(string.Format("Couldn't reimport session {0} - logfile {1} does not exist!", sessionId, archivePath));
                    //    return;
                    //}
                    FileInfo logFile = new FileInfo(archivePath);

                    logFile.CopyTo(destinationPath);
                }
                _logger.Debug(string.Format("Reimport process for session {0} started by {1}", sessionId, email));
            }
            catch (Exception ex)
            {
                _logger.Debug(string.Format("The following error occurred while trying to reimport session {0}: {1}", sessionId, ex.Message));
            }
        }

        public ReturnValue RenameSession(string email, int sessionId, string newName)
        {
            var returnValue = new ReturnValue();

            try
            {
                DapperDb dapperDb = DapperDb.Init(OpenConnection(), 3, false);

                // Get the existing session
                var existingSession = dapperDb.SessionTable.Get(sessionId);
                if (existingSession == null)
                {
                    // Something went drastically wrong, this shouldn't happen
                    string msg = string.Format("Couldn't update the session with ID {0} because it didn't exist when we went to update it!", sessionId);
                    _logger.Error(msg);
                    returnValue.Message = msg;
                    return returnValue;
                }

                // Snapshot the current record to track changes
                var snapshot = Snapshotter.Start(existingSession);

                existingSession.Name = newName;

                // Check if we have any changes to make
                DynamicParameters dynamicParameters = snapshot.Diff();
                if (!dynamicParameters.ParameterNames.Any())
                {
                    const string msg = "Session name has not changed - nothing to save!";
                    _logger.Info(msg);
                    returnValue.Message = msg;
                    return returnValue;
                }

                dapperDb.SessionTable.Update(sessionId, snapshot.Diff());

                _logger.Debug(string.Format("Updated session {0} with the new name {1}", sessionId, newName));

                returnValue.Success = true;
                return returnValue;
            }
            catch (Exception ex)
            {
                string msg = string.Format("An error occurred while trying to update the session name! {0}", ex.Message);
                _logger.Error(msg);
                returnValue.Message = msg;
                return returnValue;
            }
        }
    }
}
