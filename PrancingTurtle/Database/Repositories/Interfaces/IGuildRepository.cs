using System.Collections.Generic;
using Common;
using Database.Models;
using Database.Models.Misc;

namespace Database.Repositories.Interfaces
{
    public interface IGuildRepository
    {
        // Queries
        List<Guild> GetAll();
        List<Guild> GetApprovedGuilds();
        List<Guild> GetGuilds(int shardId, bool canApplyToOnly = false);
        Guild Get(int guildId);
        Guild Get(string name);
        List<Guild> GetVisibleGuilds(string username);
        List<int> GetBossFightsCleared(int guildId); // Going to be obsolete
        GuildMemberSessionEncounterCount GetGuildIndexCounts(int guildId);
        List<BossFightProgression> GetGuildProgression(int guildId);
        // Commands
        ReturnValue Create(string email, string name, int shardId);
        ReturnValue Remove(string email, int guildId);
        ReturnValue Approve(string email, int guildId, int statusId);

        ReturnValue SetGuildRosterPrivacy(string email, int guildId, bool setPublic);
        ReturnValue SetGuildListPrivacy(string email, int guildId, bool setPublic);
        ReturnValue SetGuildRankingPrivacy(string email, int guildId, bool setPublic);
        ReturnValue SetGuildSearchPrivacy(string email, int guildId, bool setPublic);
        ReturnValue SetGuildSessionPrivacy(string email, int guildId, bool setPublic);
        ReturnValue SetGuildProgressionPrivacy(string email, int guildId, bool setPublic);
    }
}
