using System.Collections.Generic;
using Common;
using Database.Models;

namespace Database.Repositories.Interfaces
{
    public interface IAuthUserCharacterGuildApplicationRepository
    {
        // Queries
        string PendingApplication(int authUserCharacterId);
        bool PendingApplication(int authUserCharacterId, int guildId);
        List<AuthUserCharacterGuildApplication> PendingApplications(int guildId);
        AuthUserCharacterGuildApplication GetPendingApplication(int applicationId);
        AuthUserCharacterGuildApplication GetPendingApplicationForCharacter(int authUserCharacterId);
        // Commands
        ReturnValue Create(AuthUserCharacterGuildApplication application);
        ReturnValue Remove(int applicationId, string email);
    }
}
