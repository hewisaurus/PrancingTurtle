using System.Collections.Generic;
using Common;
using Database.Models;

namespace Database.Repositories.Interfaces
{
    public interface IAbilityRoleRepository
    {
        //Task<List<AbilityRole>> GetAllAsync();
        PagedData<AbilityRole> GetPagedData(Dictionary<string, object> filters, string orderBy, int page, int pageSize);
    }
}
