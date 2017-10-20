using Database.Models.Misc;

namespace Database.Repositories.Interfaces
{
    public interface ILeaderboardRepository
    {
        Leaderboards GetLeaderboards_v2(int bossFightId, int topX, int difficultyId);
    }
}
