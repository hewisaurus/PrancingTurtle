using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Dapper;
using Database.Models;
using Database.Repositories.Interfaces;
using Logging;

namespace Database.Repositories
{
    public class SessionLogRepository : DapperRepositoryBase, ISessionLogRepository
    {
        private readonly ILogger _logger;

        public SessionLogRepository(IConnectionFactory connectionFactory, ILogger logger) : base(connectionFactory)
        {
            _logger = logger;
        }

        public ReturnValue Create(SessionLog sessionLog)
        {
            var returnValue = new ReturnValue();

            try
            {
                DapperDb dapperDb = DapperDb.Init(OpenConnection(), 3, false);
                var newId = dapperDb.SessionLogTable.Insert(
                    new //SessionLog()
                    {
                        AuthUserCharacterId = sessionLog.AuthUserCharacterId,
                        Filename = sessionLog.Filename,
                        GuildId = sessionLog.GuildId,
                        LogSize = sessionLog.LogSize,
                        SessionId = sessionLog.SessionId,
                        Token = sessionLog.Token,
                        TotalPlayedTime = sessionLog.TotalPlayedTime
                    });

                if (newId != null)
                {
                    //_logger.Debug(string.Format("{0} has successfully created a new session ({1}: {2})", email, newId, session.Name));
                    returnValue.Message = newId.ToString();
                    returnValue.Success = true;
                    _logger.Debug(string.Format("Session log successfully created for Session {0}", sessionLog.SessionId));
                }
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("An error occurred while trying to add a session log! {0}", ex.Message));
                returnValue.Message = ex.Message;
                returnValue.Success = false;
            }

            return returnValue;
        }

        public List<SessionLog> GetSessionLogsNoTotalPlayTime()
        {
            string timeElapsed;

            return Query(q => q.Query<SessionLog>(SQL.SessionLog.SessionLogsWithNoTotalPlayTime), out timeElapsed).ToList();
        }

        public SessionLog GetFirstSessionLogForSession(int sessionId)
        {
            string timeElapsed;
            return
                Query(q => q.Query<SessionLog>(MySQL.SessionLog.GetFirstSessionLogForSession, new {sessionId}),
                    out timeElapsed).SingleOrDefault();
        }

        public List<string> GetUploadersForSession(int sessionId)
        {
            string timeElapsed;
            var returnValue = new List<string>();

            var uploaders = Query(q => q.Query<AuthUserCharacter, Shard, Guild, AuthUserCharacter>
                (MySQL.SessionLog.GetUploadersForSession,
                    (auc, sh, g) =>
                    {
                        auc.Shard = sh;
                        auc.Guild = g;
                        return auc;
                    }, new {sessionId}), out timeElapsed).ToList();

            if (uploaders.Any())
            {
                returnValue.AddRange(uploaders.Select(uploader => uploader.FullDisplayName));
            }

            return returnValue;
        }

        public void UpdateSessionLogTotalPlayedTime(Dictionary<int, long> totalPlayedTimes)
        {
            try
            {
                DapperDb dapperDb = DapperDb.Init(OpenConnection(), 3);

                foreach (var kvp in totalPlayedTimes)
                {
                    // Get the existing session
                    var sessionLog = dapperDb.SessionLogTable.Get(kvp.Key);
                    if (sessionLog == null)
                    {
                        // Something went drastically wrong, this shouldn't happen
                        _logger.Error(string.Format("Couldn't update the sessionLog with ID {0} because it didn't exist when we went to update it!", kvp.Key));
                        continue;
                    }



                    // Snapshot the current record to track changes
                    var snapshot = Snapshotter.Start(sessionLog);

                    sessionLog.TotalPlayedTime = kvp.Value;

                    // Check if we have any changes to make
                    DynamicParameters dynamicParameters = snapshot.Diff();
                    if (!dynamicParameters.ParameterNames.Any())
                    {
                        continue;
                    }

                    dapperDb.SessionLogTable.Update(kvp.Key, snapshot.Diff());

                    TimeSpan timeSpan = new TimeSpan(kvp.Value);

                    _logger.Debug(string.Format("Updated sessionLog {0} with TotalPlayedTime {1} ({2})", kvp.Key, kvp.Value, timeSpan));
                }
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("An error occurred while trying to update the sessionLog TotalPlayedTimes! {0}", ex.Message));
            }
        }

        /// <summary>
        /// Checks whether the random token generator came up with a token that already exists. Shouldn't, but check anyway.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public bool SessionLogTokenExists(string token)
        {
            string timeElapsed;
            return Query(q => q.Query<long>(MySQL.SessionLog.TokenExists,
                new { token }), out timeElapsed).SingleOrDefault() == 1;
        }
    }
}