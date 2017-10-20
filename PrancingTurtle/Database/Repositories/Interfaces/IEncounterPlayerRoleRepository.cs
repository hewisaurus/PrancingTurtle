using System.Collections.Generic;
using Database.Models;

namespace Database.Repositories.Interfaces
{
    public interface IEncounterPlayerRoleRepository
    {
        List<EncounterPlayerRole> GetAll();
        void RemoveDuplicates(List<long> removeIds, string email);

        List<EncounterPlayerRole> GetAllForEncounter(int encounterId);
    }
}
