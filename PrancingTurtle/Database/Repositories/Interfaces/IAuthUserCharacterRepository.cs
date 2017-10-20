using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Database.Models;
using Database.QueryModels;

namespace Database.Repositories.Interfaces
{
    public interface IAuthUserCharacterRepository
    {
        // Query
        AuthUserCharacter Get(string email, int? id);
        List<CharacterGuild> GetCharacters(string email);
        List<int> GetGuildIdsForEmail(string email);
        List<AuthUserCharacter> GetUploaders(string email);
        bool CanJoinAGuild(string email, int userCharacterId);
        AuthUserCharacter GetCharacterWithHighestGuildRank(int authUserId, int guildId);
        List<AuthUserCharacter> GetGuildMembers(int guildId);
        bool IsCharacterInGuild(int authUserCharacterId, int guildId);
        /// <summary>
        /// Returns whether the given user has any characters that are allowed to upload
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        bool UserCanUpload(string email);
        /// <summary>
        /// Returns the ID of the character's guild, if any
        /// </summary>
        /// <param name="authUserCharacterId"></param>
        /// <returns></returns>
        int GetGuildIdForCharacter(int authUserCharacterId);

        GuildRank GetGuildRankForCharacter(int authUserCharacterId);
        // Command
        ReturnValue Create(string email, AuthUserCharacter character);
        ReturnValue Delete(string email, int characterId);
        ReturnValue AddCharacterToGuild(int authUserCharacterId, int guildId, int guildRankId);
        ReturnValue ModifyCharacterRank(int authUserCharacterId, int guildId, int guildRankId);
        ReturnValue KickCharacterFromGuild(int authUserCharacterId, int guildId, string email);

        // Async queries
        Task<List<CharacterGuild>> GetCharactersAsync(string email);
    }
}
