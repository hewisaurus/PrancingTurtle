using System;
using System.Collections.Generic;
using Common;
using Database.Models;
using Database.QueryModels;
using Database.QueryModels.Misc;

namespace Database.Repositories.Interfaces
{
    public interface ISessionRepository
    {
        /// <summary>
        /// Returns a list of sessions
        /// </summary>
        /// <returns></returns>
        List<Session> GetAll(string username = "");
        
        /// <summary>
        /// Get a single session
        /// </summary>
        /// <param name="id">The ID of the session to return</param>
        /// <returns></returns>
        Session Get(int id);

        /// <summary>
        /// Get the list of encounters for a given session
        /// </summary>
        /// <param name="id">The ID of the session to return encounters for</param>
        /// <param name="includeEncountersMarkedForDeletion"></param>
        /// <returns></returns>
        List<Encounter> GetEncounters(int id, bool includeEncountersMarkedForDeletion);
        /// <summary>
        /// Gets the total number of sessions that have been uploaded
        /// </summary>
        /// <param name="guildId">The ID of the guild to count sessions for</param>
        /// <returns>The total number of sessions that have been uploaded for this guild</returns>
        int TotalSessions(int guildId);
        /// <summary>
        /// Gets a list of sessions uploaded by any members of a particular guild
        /// </summary>
        /// <param name="guildId">The ID of the guild to return sessions for</param>
        /// <returns>A list of sessions uploaded by guild members</returns>
        List<Session> GetGuildSessions(int guildId);

        List<GuildSession> GetInstanceSessions(int instanceId, string username);
        List<GuildSession> GetAllInstanceSessions(int instanceId);
        List<GuildSession> GetBossFightSessions(int bossFightId, string username, int difficultyId = -1);
        List<GuildSession> GetAllBossFightSessions(int bossFightId, int difficultyId = -1);
        
        List<Session> GetPlayerSessions(int playerId, string username);
        /// <summary>
        /// Gets the most recent sessions
        /// </summary>
        /// <param name="sessionCount">The maximum number of sessions to return</param>
        /// <param name="email">The email address of the user. Blank if no user has authenticated.</param>
        /// <returns></returns>
        List<Session> GetRecentSessions(int sessionCount, string email = "");
        
        List<Instance> GetInstancesSeen(int sessionId);
        List<string> GetDifficultiesSeen(int sessionId, int instanceId);
        List<string> GetBossesKilled(int sessionId);
        List<string> GetBossesSeenButNotKilled(int sessionId);
        
        ReturnValue CreateSession(string email, Session session);

        /// <summary>
        /// Removes the entire session from the database
        /// </summary>
        /// <param name="email">The email address of the user performing the delete / remove</param>
        /// <param name="sessionId">The ID of the session to remove</param>
        void RemoveSession(string email, int sessionId);

        /// <summary>
        /// Removes any encounters and other records associated with a session, without remove the session record
        /// </summary>
        /// <param name="email">The email address of the user performing the clear</param>
        /// <param name="sessionId">The ID of the session to clear</param>
        void ClearSession(string email, int sessionId);

        /// <summary>
        /// Begins the reimport process on a given session. This simply moves the file from its archive location back to where the Auto Importer watches for logs.
        /// </summary>
        /// <param name="email">The email address of the user performing the reimport</param>
        /// <param name="sessionId"></param>
        void ReimportSession(string email, int sessionId);

        /// <summary>
        /// Renames a session
        /// </summary>
        /// <param name="email"></param>
        /// <param name="sessionId"></param>
        /// <param name="newName"></param>
        ReturnValue RenameSession(string email, int sessionId, string newName);
    }
}
