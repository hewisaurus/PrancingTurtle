using System.Collections.Generic;
using Common;
using Database.Models;

namespace Database.Repositories.Interfaces
{
    public interface IEncounterOverviewRepository
    {
        ReturnValue Add(EncounterOverview overview);
       
        List<Encounter> GetEncountersWithIncompleteOverviews(int limit = 100);
    }
}
