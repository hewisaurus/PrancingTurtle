using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Dapper;
using Database.Models;
using Database.QueryModels;
using Database.Repositories.Interfaces;
using Logging;

namespace Database.Repositories
{
    public class AuthUserCharacterRepository : DapperRepositoryBase, IAuthUserCharacterRepository
    {
        private readonly ILogger _logger;

        public AuthUserCharacterRepository(IConnectionFactory connectionFactory, ILogger logger)
            : base(connectionFactory)
        {
            _logger = logger;
        }

        // Query methods

        /// <summary>
        /// Updated for MySQL
        /// </summary>
        /// <param name="email"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public AuthUserCharacter Get(string email, int? id)
        {
            string timeElapsed;
            return Query(s => s.Query<AuthUserCharacter, Shard, AuthUserCharacter>
                (MySQL.AuthUserCharacter.GetSingle,
                (uc, sh) =>
                {
                    uc.Shard = sh;
                    return uc;
                }, new { email, @characterId = id }), out timeElapsed).SingleOrDefault();
        }

        /// <summary>
        /// Updated for MySQL
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public List<CharacterGuild> GetCharacters(string email)
        {
            string timeElapsed;
            var results = Query(s => s.Query<CharacterGuild>(MySQL.AuthUserCharacter.GetAllCharactersForEmail, new { email }), out timeElapsed);
            return results.ToList();
        }

        /// <summary>
        /// Updated for MySQL
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public List<int> GetGuildIdsForEmail(string email)
        {
            string timeElapsed;
            var results = Query(s => s.Query<int>
                (MySQL.AuthUserCharacter.GetGuildIdsForEmail, new { email }), out timeElapsed);
            return results.ToList();
        }

        /// <summary>
        /// Updated for MySQL
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public List<AuthUserCharacter> GetUploaders(string email)
        {
            string timeElapsed;

            var results = Query(s => s.Query<AuthUserCharacter, Shard, Guild, GuildRank, AuthUserCharacter>
                (MySQL.AuthUserCharacter.GetAllUploadersForEmail,
                (auc, sh, g, gr) =>
                {
                    auc.Shard = sh;
                    if (g != null)
                    {
                        auc.Guild = g;
                    }
                    if (gr != null)
                    {
                        auc.GuildRank = gr;
                    }
                    return auc;
                }, new { email }), out timeElapsed);
            return results.ToList();
        }

        /// <summary>
        /// Returns whether the user trying to perform the action can join a guild.
        /// This requires that the character belong to the user performing the action, and
        /// the character can't already be in a guild. Updated for MySQL
        /// </summary>
        /// <param name="email">The user performing the action</param>
        /// <param name="userCharacterId">The ID of the character to join a guild with</param>
        /// <returns>True if the character belongs to this user, otherwise false</returns>
        public bool CanJoinAGuild(string email, int userCharacterId)
        {
            string timeElapsed;
            var hasPendingApp = Query(
                q => q.Query<long>(MySQL.AuthUserCharacterGuildApplication.CharacterHasAPendingApplication,
                    new { @authUserCharacterId = userCharacterId }), out timeElapsed).SingleOrDefault() == 1;

            if (hasPendingApp) return false;

            return Query(q => q.Query<long>(MySQL.AuthUserCharacter.CharacterCanJoinAGuild,
                new { email, userCharacterId }), out timeElapsed).SingleOrDefault() == 1;
        }
        
        /// <summary>
        /// Updated for MySQL
        /// </summary>
        /// <param name="authUserId"></param>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public AuthUserCharacter GetCharacterWithHighestGuildRank(int authUserId, int guildId)
        {
            string timeElapsed;
            return Query(
                q =>
                    q.Query<AuthUserCharacter, Guild, GuildRank, AuthUserCharacter>
                    (MySQL.AuthUserCharacter.GetCharacterWithHighestGuildRank,
                        (auc, g, gr) =>
                        {
                            auc.Guild = g;
                            auc.GuildRank = gr;
                            return auc;
                        },
                        new { guildId, authUserId }), out timeElapsed).SingleOrDefault();
        }
        /// <summary>
        /// Updated for MySQL
        /// </summary>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public List<AuthUserCharacter> GetGuildMembers(int guildId)
        {
            string timeElapsed;
            return Query(
                q =>
                    q.Query<AuthUserCharacter, Guild, GuildRank, AuthUserCharacter>
                    (MySQL.AuthUserCharacter.GetAllGuildMembers,
                         (auc, g, gr) =>
                         {
                             auc.Guild = g;
                             auc.GuildRank = gr;
                             return auc;
                         },
                        new { guildId }), out timeElapsed).ToList();
        }

        /// <summary>
        /// Updated for MySQL
        /// </summary>
        /// <param name="authUserCharacterId"></param>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public bool IsCharacterInGuild(int authUserCharacterId, int guildId)
        {
            string timeElapsed;
            return
                Query(
                    q =>
                        q.Query<long>(MySQL.AuthUserCharacter.CharacterIsInGuild,
                            new { authUserCharacterId, guildId }), out timeElapsed)
                    .SingleOrDefault() == 1;
        }
        /// <summary>
        /// Checks whether a user has any characters that are allowed to upload. Updated for MySQL
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public bool UserCanUpload(string email)
        {
            string timeElapsed;
            var results =
                Query(q => q.Query<long>(MySQL.AuthUser.UserCanUpload, new { email }), out timeElapsed)
                    .SingleOrDefault() == 1;
            _logger.Debug(string.Format("Checked that {0} can upload ({1}) in {2}", email, results, timeElapsed));
            return results;
        }
        /// <summary>
        /// Updated for MySQL
        /// </summary>
        /// <param name="authUserCharacterId"></param>
        /// <returns></returns>
        public int GetGuildIdForCharacter(int authUserCharacterId)
        {
            string timeElapsed;
            return
                Query(q => q.Query<int>(MySQL.AuthUserCharacter.GetGuildIdForCharacter, new { @id = authUserCharacterId }),
                    out timeElapsed).SingleOrDefault();
        }

        public GuildRank GetGuildRankForCharacter(int authUserCharacterId)
        {
            string timeElapsed;
            return
                Query(q => q.Query<GuildRank>(MySQL.AuthUserCharacter.GetGuildRankForCharacter, new { @id = authUserCharacterId }),
                    out timeElapsed).SingleOrDefault();
        }

        /// <summary>
        /// Updated for MySQL
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        private int GetAuthUserIdByEmail(string email)
        {
            string timeElapsed;
            return Query(s => s.Query<int>(MySQL.AuthUser.GetAuthIdUserByEmail, new { email }), out timeElapsed).SingleOrDefault();
        }

        // Command methods

        /// <summary>
        /// Create a new character and link it to the user performing the create.
        /// Validation of existing characters is performed here, prior to the insert. Updated for MySQL
        /// </summary>
        /// <param name="email">The email address of the user performing the create</param>
        /// <param name="character">The character to add</param>
        /// <returns>A ReturnValue indicating success or failure, along with optional messages</returns>
        public ReturnValue Create(string email, AuthUserCharacter character)
        {
            var returnValue = new ReturnValue();

            // Check to see if this character name has been taken on the given shard already. Ignore users that are 'removed'
            string timeElapsed;
            var alreadyExists = Query(
                q =>
                    q.Query<long>(MySQL.AuthUserCharacter.CharacterExistsOnShard,
                        new { @characterName = character.CharacterName, @shardId = character.ShardId }), out timeElapsed)
                .SingleOrDefault() == 1;

            if (alreadyExists)
            {
                returnValue.Message = string.Format("The character '{0}' already exists!", character.CharacterName);
                return returnValue;
            }

            // Check to see if this user has 6 characters on this shard already
            var totalCharacters = Query(
                q =>
                    q.Query<long>(MySQL.AuthUserCharacter.CheckMaxCharacterCountForAccount,
                        new { email, @shardId = character.ShardId }), out timeElapsed).SingleOrDefault();

            if (totalCharacters > 5)
            {
                returnValue.Message = "You have already created the maximum number of characters on this shard.";
                return returnValue;
            }

            // Get the AuthUserId for this user

            var userId = GetAuthUserIdByEmail(email);
            if (userId == 0)
            {
                returnValue.Message = string.Format("No user was found with the email address {0}", email);
                return returnValue;
            }

            // Add the character
            try
            {
                DapperDb dapperDb = DapperDb.Init(OpenConnection(), 3, false);

                var newId = dapperDb.AuthUserCharacterTable.Insert(
                    new //AuthUserCharacter()
                    {
                        AuthUserId = userId,
                        CharacterName = character.CharacterName,
                        ShardId = character.ShardId
                    });

                if (newId > 0)
                {
                    returnValue.Success = true;
                }
            }
            catch (Exception ex)
            {
                returnValue.Message = ex.Message;
            }

            return returnValue;
        }

        /// <summary>
        /// Removes a character if it belongs to the user attempting to perform the delete.
        /// Validation of ownership is performed here, prior to the delete. Updated for MySQL
        /// </summary>
        /// <param name="email">The email address of the user performing the delete</param>
        /// <param name="characterId">The ID of the character to remove</param>
        /// <returns></returns>
        public ReturnValue Delete(string email, int characterId)
        {
            var returnValue = new ReturnValue();
            string timeElapsed;

            // Get the list of characters belonging to this user
            var myCharacters = GetCharacters(email);
            // Check that the ID we were passed exists in the list, otherwise this user has tried
            // to delete a character that does not belong to them
            var thisCharacter = myCharacters.FirstOrDefault(c => c.Id == characterId);
            if (thisCharacter != null)
            {
                var hasCreatedSessions = Query(q => q.Query<long>
                    (MySQL.AuthUserCharacter.HasCreatedSessions, new { @id = thisCharacter.Id }), out timeElapsed)
                    .SingleOrDefault() == 1;
                var hasUploadedLogs = Query(q => q.Query<long>
                    (MySQL.AuthUserCharacter.HasUploadedLogs, new { @id = thisCharacter.Id }), out timeElapsed)
                    .SingleOrDefault() == 1;

                if (hasCreatedSessions || hasUploadedLogs)
                {
                    // Set to removed rather than delete the character
                    try
                    {
                        _logger.Debug("Can't remove this character as it has uploaded logs or created sessions, so marking as REMOVED");
                        var result =
                            Execute(
                                e =>
                                    e.Execute(MySQL.AuthUserCharacter.MarkCharacterRemoved, new { @id = thisCharacter.Id }),
                                out timeElapsed);
                        if (result > 0)
                        {
                            _logger.Debug(string.Format("Character (ID {0}) successfully marked as removed.", thisCharacter.Id));
                            returnValue.Success = true;
                            return returnValue;
                        }

                        _logger.Debug(string.Format("Character (ID {0}) update (mark as removed) failed!", thisCharacter.Id));
                        returnValue.Success = false;
                        returnValue.Message = "Failed to mark character as removed!";
                        return returnValue;

                    }
                    catch (Exception ex)
                    {
                        string msg = string.Format("Failed to mark character {0} as removed: {1}", thisCharacter.Id,
                            ex.Message);
                        _logger.Debug(msg);
                        returnValue.Message = msg;
                        return returnValue;
                    }
                }
                else
                {
                    try
                    {
                        var characterName = thisCharacter.CharacterName;

                        // Check if this user has created any sessions, or uploaded any logs.
                        // If they have, we need to set them to 'removed' rather than delete them.

                        var sw = new Stopwatch();
                        sw.Start();
                        DapperDb dapperDb = DapperDb.Init(OpenConnection(), 3, false);

                        bool success = dapperDb.AuthUserCharacterTable.Delete(characterId);

                        sw.Stop();

                        _logger.Debug(string.Format("{0} successfully removed the character '{1}' from their account.",
                            email, characterName));

                        if (thisCharacter.GuildId != null)
                        {
                            // Check if there are any remaining members in this guild. If not, delete that too.
                            var guildHasMembers =
                                Query(
                                    q =>
                                        q.Query<long>(MySQL.Guild.GuildHasMembers,
                                            new { @guildId = thisCharacter.GuildId }),
                                    out timeElapsed).SingleOrDefault() == 1;

                            if (!guildHasMembers)
                            {
                                // There isn't anyone left in this guild, so remove it
                                try
                                {
                                    var result =
                                        Execute(
                                            e =>
                                                e.Execute(MySQL.Guild.RemoveGuild,
                                                    new { @guildId = thisCharacter.GuildId }), out timeElapsed);
                                    if (result > 0)
                                    {
                                        _logger.Debug(
                                            string.Format(
                                                "{0} has also removed the now-empty guild after removing the last character.",
                                                email));
                                        returnValue.Success = true;
                                        return returnValue;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    string msg =
                                        string.Format("An error occurred while removing the now-empty guild: {0}",
                                            ex.Message);
                                    returnValue.Message = msg;
                                    _logger.Debug(msg);
                                    return returnValue;
                                }
                            }
                        }

                        returnValue.Success = success;
                        returnValue.TimeTaken = sw.Elapsed;


                    }
                    catch (Exception ex)
                    {
                        // Check for foreign key constraint failure first
                        if (ex.Message.Contains("DELETE statement conflicted with the REFERENCE constraint") || // MSSQL
                            ex.Message.Contains("Cannot delete or update a parent row")) // MySQL
                        {
                            returnValue.Message =
                                "Delete operation failed - this record is depended upon by other tables within the database and cannot be deleted at this time.";
                        }
                        else
                        {
                            returnValue.Message = ex.Message;
                        }
                    }
                }
            }
            else
            {
                returnValue.Message = "Invalid character specified";
            }

            return returnValue;
        }

        /// <summary>
        /// Updated for MySQL
        /// </summary>
        /// <param name="authUserCharacterId"></param>
        /// <param name="guildId"></param>
        /// <param name="guildRankId"></param>
        /// <returns></returns>
        public ReturnValue AddCharacterToGuild(int authUserCharacterId, int guildId, int guildRankId)
        {
            var returnValue = new ReturnValue();
            try
            {
                string timeElapsed;
                if (Execute(q => q.Execute(MySQL.AuthUserCharacter.AddCharacterToGuild,
                    new { guildId, guildRankId, authUserCharacterId }), out timeElapsed) > 0)
                {
                    returnValue.Success = true;
                }
            }
            catch (Exception ex)
            {
                returnValue.Message = ex.Message;
            }

            return returnValue;
        }

        /// <summary>
        /// Updated for MySQL
        /// </summary>
        /// <param name="authUserCharacterId"></param>
        /// <param name="guildId"></param>
        /// <param name="guildRankId"></param>
        /// <returns></returns>
        public ReturnValue ModifyCharacterRank(int authUserCharacterId, int guildId, int guildRankId)
        {
            var returnValue = new ReturnValue();

            try
            {
                string timeElapsed;

                var guildRank =
                    Query(q => q.Query<GuildRank>(MySQL.GuildRank.GetSingle, new { @id = guildRankId }), out timeElapsed)
                        .SingleOrDefault();
                if (guildRank == null)
                {
                    returnValue.Message = "Guild rank doesn't exist!";
                    _logger.Debug(string.Format("Couldn't change rank to {0} for AUC {1} - rank doesn't exist!", guildRankId, authUserCharacterId));
                    return returnValue;
                }

                var result = Execute(e => e.Execute(MySQL.AuthUserCharacter.UpdateGuildRankForCharacter,
                    new { guildRankId, authUserCharacterId, guildId }),
                    out timeElapsed);

                returnValue.Success = true;
                returnValue.Message = result.ToString();
                _logger.Debug(string.Format("Updated guild rank: {0} now has '{1}'", authUserCharacterId, guildRank.Name));
            }
            catch (Exception ex)
            {
                returnValue.Message = ex.Message;
                _logger.Error(string.Format("An error occurred while trying to change a user's guild rank! {0}", ex.Message));
            }

            return returnValue;
        }

        public ReturnValue KickCharacterFromGuild(int authUserCharacterId, int guildId, string email)
        {
            string timeElapsed;

            // To kick a member from a guild, we just set the 'Removed' value to true, and then add another AuthUserCharacter record with no guild.
            var returnValue = new ReturnValue();

            try
            {
                var outgoingMember =
                    Query(
                        q => q.Query<AuthUserCharacter>(MySQL.AuthUserCharacter.Get, new {@id = authUserCharacterId}),
                        out timeElapsed).SingleOrDefault();
                if (outgoingMember == null)
                {
                    returnValue.Message = "Couldn't find the character to remove!";
                    return returnValue;
                }

                // Set the removed flag first
                Execute(
                    q =>
                        q.Execute(MySQL.AuthUserCharacter.MarkCharacterRemovedIncludingGuildId,
                            new {@id = authUserCharacterId, guildId}), out timeElapsed);
                _logger.Debug(string.Format("{0} has kicked {1} from guild #{2}", email, outgoingMember.CharacterName, guildId));

                // Now, add the new unguilded character
                DapperDb dapperDb = DapperDb.Init(OpenConnection(), 3, false);

                var newId = dapperDb.AuthUserCharacterTable.Insert(
                    new //AuthUserCharacter()
                    {
                        AuthUserId = outgoingMember.AuthUserId,
                        CharacterName = outgoingMember.CharacterName,
                        ShardId = outgoingMember.ShardId
                    });

                if (newId > 0)
                {
                    returnValue.Success = true;
                    _logger.Debug(string.Format("The character {0} has been successfully recreated.", outgoingMember.CharacterName));
                }
            }
            catch (Exception ex)
            {
                _logger.Debug(string.Format("An error occurred while removing character {0} from guild {1}: {2}", authUserCharacterId, guildId, ex.Message));
                returnValue.Message = ex.Message;
            }

            return returnValue;
        }


        public async Task<List<CharacterGuild>> GetCharactersAsync(string email)
        {
            var results = await QueryAsync(s => s.QueryAsync<CharacterGuild>(MySQL.AuthUserCharacter.GetAllCharactersForEmail, new { email }));
            return results.ToList();
        }
    }
}
