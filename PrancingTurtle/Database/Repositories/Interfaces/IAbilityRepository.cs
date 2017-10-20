using System.Collections.Generic;
using Common;
using Database.Models;
using Database.QueryModels.Misc;

namespace Database.Repositories.Interfaces
{
    public interface IAbilityRepository
    {
        List<AbilityIdName> GetAbilitiesWithNoIcon();
        ReturnValue RemoveOrphanedAbilities(List<int> abilityIds);
        ReturnValue UpdateAbilityIcons(List<AbilityNameIcon> abilities);

        PagedData<Ability> GetPagedData(Dictionary<string, object> filters, string orderBy, int page, int pageSize);
    }
}
