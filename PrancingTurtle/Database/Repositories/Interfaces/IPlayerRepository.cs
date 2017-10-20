using System.Collections.Generic;
using Common;
using Database.Models;
using Database.QueryModels.Misc;

namespace Database.Repositories.Interfaces
{
    public interface IPlayerRepository
    {
        Player Get(int id);
        List<Player> GetByIds(List<int> playerIds);
        PlayerSearchResult GetFromSearch(int id);
        PlayerSearchResult GetSingleFromPlayerId(int playerId);
        //int GetPlayerIdFromPlayerLogId(string playerLogId);
        string GetTargetNameFromLogId(string logId);

        List<Player> PlayersWithShardNamesInPlayerName();

        ReturnValue UpdatePlayerNameAndShard(List<Player> updatePlayers);

        List<Player> GetAll();
        List<int> GetAllUniquePlayerIds();
        ReturnValue RemoveOrphanPlayers(List<int> playerIds);
    }
}
