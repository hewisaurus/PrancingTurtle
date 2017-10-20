using Database.Models;

namespace Database.Repositories.Interfaces
{
    public interface IEncounterDifficultyRepository
    {
        EncounterDifficulty Get(int id);
        EncounterDifficulty GetDefaultDifficulty();
        int GetDefaultDifficultyId();
    }
}
