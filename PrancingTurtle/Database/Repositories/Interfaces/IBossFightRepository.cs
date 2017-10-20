using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Database.Models;

namespace Database.Repositories.Interfaces
{
    public interface IBossFightRepository
    {
        BossFight Get(int id);
        List<BossFight> GetAll(bool filterProgressionInstances);
       List<BossFightDifficulty> GetBossFightsAndDifficultySettings();

        // BossFight Difficulty detection
        bool DifficultyRecordsExist(int bossFightId);

        List<BossFightDifficulty> GetDifficultySettings(int bossFightId);

        // async
        Task<BossFight> GetAsync(int id);
        Task<List<BossFight>> GetAllAsync();
        Task<ReturnValue> Create(BossFight model);
        Task<ReturnValue> Update(BossFight model);
        Task<ReturnValue> Delete(int id);

        // PagedData
        Task<PagedData<BossFight>> GetPagedDataAsync(Dictionary<string, object> filters, string orderBy, int offset, int pageSize, bool useOr = false);
    }
}
