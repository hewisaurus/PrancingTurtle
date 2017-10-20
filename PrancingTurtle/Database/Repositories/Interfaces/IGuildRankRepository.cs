using System.Collections.Generic;
using Database.Models;

namespace Database.Repositories.Interfaces
{
    public interface IGuildRankRepository
    {
        GuildRank GetDefaultRankForGuildCreators();
        GuildRank GetDefaultRankForGuildApplications();
        List<GuildRank> GetRanks();
    }
}
