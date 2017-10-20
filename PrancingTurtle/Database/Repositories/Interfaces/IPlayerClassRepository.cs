using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Models;

namespace Database.Repositories.Interfaces
{
    public interface IPlayerClassRepository
    {
        Task<List<PlayerClass>> GetAllAsync();
    }
}
