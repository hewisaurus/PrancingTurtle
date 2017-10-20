using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Database.Models;

namespace Database.Repositories.Interfaces
{
    public interface IInstanceRepository
    {
        Instance Get(int id);
        // async
        Task<Instance> GetAsync(int id);
        Task<List<Instance>> GetAllAsync();
        Task<ReturnValue> Create(Instance newInstance);
        Task<ReturnValue> Update(Instance updateInstance);
        Task<ReturnValue> Delete(int id);
    }
}
