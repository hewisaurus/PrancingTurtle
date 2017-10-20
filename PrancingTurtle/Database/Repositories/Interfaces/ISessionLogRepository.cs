using System.Collections.Generic;
using Common;
using Database.Models;

namespace Database.Repositories.Interfaces
{
    public interface ISessionLogRepository
    {
        ReturnValue Create(SessionLog sessionLog);
        /// <summary>
        /// Gets the list of session logs that don't have a total playtime associated with them.
        /// This is used to calculate (for the stats page) the total amount of time that players
        /// have spent playing the game
        /// </summary>
        /// <returns></returns>
        List<SessionLog> GetSessionLogsNoTotalPlayTime();
        /// <summary>
        /// Updates the TotalPlayedTime for multiple session logs
        /// </summary>
        /// <param name="totalPlayedTimes"></param>
        void UpdateSessionLogTotalPlayedTime(Dictionary<int, long> totalPlayedTimes);

        bool SessionLogTokenExists(string token);
        SessionLog GetFirstSessionLogForSession(int sessionId);
        List<string> GetUploadersForSession(int sessionId);
    }
}
