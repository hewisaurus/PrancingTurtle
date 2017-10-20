using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Models;

namespace Database.Repositories.Interfaces
{
    public interface ISessionEncounterRepository
    {
        List<int> GetEncounterIds();
        Session GetSessionForEncounter(int encounterId);
        // Async
        Task<Session> GetSessionForEncounterAsync(int encounterId);
    }
}
