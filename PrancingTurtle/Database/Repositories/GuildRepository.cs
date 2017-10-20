using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Dapper;
using Database.Models;
using Database.Models.Misc;
using Database.Repositories.Interfaces;
using Logging;

namespace Database.Repositories
{
    public class GuildRepository : DapperRepositoryBase, IGuildRepository
    {
        private readonly ILogger _logger;

        public GuildRepository(IConnectionFactory connectionFactory, ILogger logger)
            : base(connectionFactory)
        {
            _logger = logger;
        }

        /// <summary>
        /// Updated for MySQL
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        private int GetIdByEmail(string email)
        {
            string timeElapsed;

            return Query(s => s.Query<int>(MySQL.AuthUser.GetAuthIdUserByEmail, new { email }), out timeElapsed).SingleOrDefault();
        }

        /// <summary>
        /// Updated for MySQL
        /// </summary>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public Guild Get(int guildId)
        {
            string timeElapsed;
            return Query(q => q.Query<Guild, Shard, GuildStatus, Guild>
                (MySQL.Guild.GetGuildById,
                (g, s, gs) =>
                {
                    g.Status = gs;
                    g.Shard = s;
                    return g;
                }, new { @id = guildId }), out timeElapsed).SingleOrDefault();
        }

        public Guild Get(string name)
        {
            string timeElapsed;
            return Query(q => q.Query<Guild, Shard, GuildStatus, Guild>
                (MySQL.Guild.GetGuildByName,
                (g, s, gs) =>
                {
                    g.Status = gs;
                    g.Shard = s;
                    return g;
                }, new { name }), out timeElapsed).SingleOrDefault();
        }

        public List<Guild> GetVisibleGuilds(string username)
        {
            string timeElapsed;
            if (string.IsNullOrEmpty(username))
            {
                return Query(q => q.Query<Guild>(MySQL.Guild.ListVisibleGuildsNoAuth), out timeElapsed).ToList();
            }

            return Query(q => q.Query<Guild>(MySQL.Guild.ListVisibleGuildsWithAuth, new { @email = username }), out timeElapsed).ToList();
        }

        [Obsolete]
        public List<int> GetBossFightsCleared(int guildId)
        {
            string timeElapsed;
            return Query(q => q.Query<int>(MySQL.Guild.GetBossFightsCleared, new { guildId }), out timeElapsed).ToList();
        }

        /// <summary>
        /// Combine the three counts that exist on this page into a single executed query
        /// </summary>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public GuildMemberSessionEncounterCount GetGuildIndexCounts(int guildId)
        {
            string timeElapsed;

            return
                Query(
                    q =>
                        q.Query<GuildMemberSessionEncounterCount>(MySQL.Guild.CountMembersSessionsEncountersForGuild,
                            new {guildId}), out timeElapsed).SingleOrDefault();
        }

        /// <summary>
        /// New method for returning guild progression, including difficulty
        /// </summary>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public List<BossFightProgression> GetGuildProgression(int guildId)
        {
            string timeElapsed;
            return
                Query(q => q.Query<BossFightProgression>(MySQL.Guild.GetGuildBossFightProgression, new { guildId }), out timeElapsed)
                    .ToList();
        }

        /// <summary>
        /// Updated for MySQL
        /// </summary>
        /// <param name="email"></param>
        /// <param name="name"></param>
        /// <param name="shardId"></param>
        /// <returns></returns>
        public ReturnValue Create(string email, string name, int shardId)
        {
            var returnValue = new ReturnValue();

            string timeElapsed;
            var alreadyExists = Query(
                q =>
                    q.Query<long>(MySQL.Guild.ExistsOnShard,
                        new { @guildName = name, shardId }), out timeElapsed).SingleOrDefault() == 1;

            if (alreadyExists)
            {
                returnValue.Message = string.Format("The guild '{0}' already exists!", name);
                return returnValue;
            }

            // Get the AuthUserId for this user

            var userId = GetIdByEmail(email);
            if (userId == 0)
            {
                returnValue.Message = string.Format("No user was found with the email address {0}", email);
                return returnValue;
            }

            // Get the default guild status (pending approval)
            var defaultStatus = Query(q => q.Query<GuildStatus>(MySQL.GuildStatus.GetDefaultCreationStatus), out timeElapsed).SingleOrDefault();

            if (defaultStatus == null)
            {
                returnValue.Message =
                    string.Format("Couldn't add the guild {0}, as no guild status has been marked as default.",
                        name);
                return returnValue;
            }

            // Add the guild
            try
            {
                DapperDb dapperDb = DapperDb.Init(OpenConnection(), 3, false);

                var newId = dapperDb.GuildTable.Insert(
                    new //Guild()
                    {
                        Name = name,
                        ShardId = shardId,
                        GuildStatusId = defaultStatus.Id
                    });

                if (newId > 0)
                {
                    _logger.Debug(string.Format("{0} has successfully created the guild {1}", email, name));
                    returnValue.Message = newId.ToString();
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
        /// <param name="email"></param>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public ReturnValue Remove(string email, int guildId)
        {
            string timeElapsed;
            ReturnValue returnValue = new ReturnValue();

            try
            {
                var result = Execute(e => e.Execute(MySQL.Guild.RemoveGuild, new { guildId }), out timeElapsed);
                if (result == 0)
                {
                    _logger.Debug(string.Format("Couldn't remove guild {0} - guild doesn't exist!", guildId));
                    returnValue.Message = "Guild doesn't exist!";
                    return returnValue;
                }
                returnValue.Success = true;
                _logger.Debug(string.Format("{0} has successfully removed guild {1}", email, guildId));
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    _logger.Debug(string.Format("An error occurred while trying to remove guild {0}: {1} ({2})", guildId, ex.Message, ex.InnerException.Message));
                    returnValue.Message = string.Format("An error occurred while trying to remove guild {0}: {1} ({2})", guildId, ex.Message, ex.InnerException.Message);
                }
                else
                {
                    if (ex.Message.Contains("Cannot delete or update a parent row"))
                    {
                        const string msg = "There are records in the database that are linked to this guild and so it cannot be deleted.";
                        _logger.Debug(string.Format("An error occurred while trying to remove guild {0}: {1}", guildId, msg));
                        returnValue.Message = string.Format("An error occurred while trying to remove guild {0}: {1}", guildId, msg);
                    }
                    else
                    {
                        _logger.Debug(string.Format("An error occurred while trying to remove guild {0}: {1}", guildId, ex.Message));
                        returnValue.Message = string.Format("An error occurred while trying to remove guild {0}: {1}", guildId, ex.Message);
                    }
                }
            }

            return returnValue;
        }

        public ReturnValue Approve(string email, int guildId, int statusId)
        {
            string timeElapsed;
            ReturnValue returnValue = new ReturnValue();

            try
            {
                var result = Execute(e => e.Execute(MySQL.Guild.Approve, new { @guildStatusId = statusId, guildId }), out timeElapsed);
                if (result == 0)
                {
                    _logger.Debug(string.Format("Couldn't approve guild {0} - guild doesn't exist!", guildId));
                    returnValue.Message = "Guild doesn't exist!";
                    return returnValue;
                }
                returnValue.Success = true;
                _logger.Debug(string.Format("{0} has successfully approved guild {1}", email, guildId));
            }
            catch (Exception ex)
            {
                _logger.Debug(string.Format("An error occurred while trying to approve guild {0}: {1}", guildId, ex.Message));
                returnValue.Message = string.Format("An error occurred while trying to approve guild {0}: {1}", guildId, ex.Message);
            }

            return returnValue;
        }

        public ReturnValue SetGuildRosterPrivacy(string email, int guildId, bool setPublic)
        {
            string timeElapsed;
            ReturnValue returnValue = new ReturnValue();

            try
            {
                var result = Execute(e => e.Execute(setPublic ? MySQL.Guild.SetRosterPublic : MySQL.Guild.SetRosterPrivate,
                    new { guildId }), out timeElapsed);
                if (result == 0)
                {
                    _logger.Debug(string.Format("Couldn't update roster privacy for guild {0} - guild doesn't exist!", guildId));
                    returnValue.Message = "Guild doesn't exist!";
                    return returnValue;
                }
                returnValue.Success = true;
                _logger.Debug(string.Format("{0} has successfully set the guild roster for {1} to {2}", email, guildId, setPublic ? "public" : "private"));
            }
            catch (Exception ex)
            {
                _logger.Debug(string.Format("An error occurred while trying to setting guild roster privacy {0}: {1}", guildId, ex.Message));
                returnValue.Message = string.Format("An error occurred while trying to setting guild roster privacy {0}: {1}", guildId, ex.Message);
            }

            return returnValue;
        }

        public ReturnValue SetGuildListPrivacy(string email, int guildId, bool setPublic)
        {
            string timeElapsed;
            ReturnValue returnValue = new ReturnValue();

            try
            {
                var result = Execute(e => e.Execute(setPublic ? MySQL.Guild.SetListsPublic : MySQL.Guild.SetListsPrivate,
                    new { guildId }), out timeElapsed);
                if (result == 0)
                {
                    _logger.Debug(string.Format("Couldn't update list privacy for guild {0} - guild doesn't exist!", guildId));
                    returnValue.Message = "Guild doesn't exist!";
                    return returnValue;
                }
                returnValue.Success = true;
                _logger.Debug(string.Format("{0} has successfully set the guild list privacy for {1} to {2}", email, guildId, setPublic ? "public" : "private"));
            }
            catch (Exception ex)
            {
                _logger.Debug(string.Format("An error occurred while trying to setting guild list privacy {0}: {1}", guildId, ex.Message));
                returnValue.Message = string.Format("An error occurred while trying to setting guild list privacy {0}: {1}", guildId, ex.Message);
            }

            return returnValue;
        }

        public ReturnValue SetGuildRankingPrivacy(string email, int guildId, bool setPublic)
        {
            string timeElapsed;
            ReturnValue returnValue = new ReturnValue();

            try
            {
                var result = Execute(e => e.Execute(setPublic ? MySQL.Guild.SetRankingsPublic : MySQL.Guild.SetRankingsPrivate,
                    new { guildId }), out timeElapsed);
                if (result == 0)
                {
                    _logger.Debug(string.Format("Couldn't update ranking privacy for guild {0} - guild doesn't exist!", guildId));
                    returnValue.Message = "Guild doesn't exist!";
                    return returnValue;
                }
                returnValue.Success = true;
                _logger.Debug(string.Format("{0} has successfully set the guild ranking privacy for {1} to {2}", email, guildId, setPublic ? "public" : "private"));
            }
            catch (Exception ex)
            {
                _logger.Debug(string.Format("An error occurred while trying to setting guild ranking privacy {0}: {1}", guildId, ex.Message));
                returnValue.Message = string.Format("An error occurred while trying to setting guild ranking privacy {0}: {1}", guildId, ex.Message);
            }

            return returnValue;
        }

        public ReturnValue SetGuildSearchPrivacy(string email, int guildId, bool setPublic)
        {
            string timeElapsed;
            ReturnValue returnValue = new ReturnValue();

            try
            {
                var result = Execute(e => e.Execute(setPublic ? MySQL.Guild.SetSearchPublic : MySQL.Guild.SetSearchPrivate,
                    new { guildId }), out timeElapsed);
                if (result == 0)
                {
                    _logger.Debug(string.Format("Couldn't update search privacy for guild {0} - guild doesn't exist!", guildId));
                    returnValue.Message = "Guild doesn't exist!";
                    return returnValue;
                }
                returnValue.Success = true;
                _logger.Debug(string.Format("{0} has successfully set the guild search privacy for {1} to {2}", email, guildId, setPublic ? "public" : "private"));
            }
            catch (Exception ex)
            {
                _logger.Debug(string.Format("An error occurred while trying to setting guild search privacy {0}: {1}", guildId, ex.Message));
                returnValue.Message = string.Format("An error occurred while trying to setting guild search privacy {0}: {1}", guildId, ex.Message);
            }

            return returnValue;
        }

        public ReturnValue SetGuildSessionPrivacy(string email, int guildId, bool setPublic)
        {
            string timeElapsed;
            ReturnValue returnValue = new ReturnValue();

            try
            {
                var result = Execute(e => e.Execute(setPublic ? MySQL.Guild.SetSessionsPublic : MySQL.Guild.SetSessionsPrivate,
                    new { guildId }), out timeElapsed);
                if (result == 0)
                {
                    _logger.Debug(string.Format("Couldn't update session privacy for guild {0} - guild doesn't exist!", guildId));
                    returnValue.Message = "Guild doesn't exist!";
                    return returnValue;
                }
                returnValue.Success = true;
                _logger.Debug(string.Format("{0} has successfully set the guild session privacy for {1} to {2}", email, guildId, setPublic ? "public" : "private"));
            }
            catch (Exception ex)
            {
                _logger.Debug(string.Format("An error occurred while trying to setting guild session privacy {0}: {1}", guildId, ex.Message));
                returnValue.Message = string.Format("An error occurred while trying to setting guild session privacy {0}: {1}", guildId, ex.Message);
            }

            return returnValue;
        }

        public ReturnValue SetGuildProgressionPrivacy(string email, int guildId, bool setPublic)
        {
            string timeElapsed;
            ReturnValue returnValue = new ReturnValue();

            try
            {
                var result = Execute(e => e.Execute(setPublic ? MySQL.Guild.SetProgressionPublic : MySQL.Guild.SetProgressionPrivate,
                    new { guildId }), out timeElapsed);
                if (result == 0)
                {
                    _logger.Debug(string.Format("Couldn't update progression privacy for guild {0} - guild doesn't exist!", guildId));
                    returnValue.Message = "Guild doesn't exist!";
                    return returnValue;
                }
                returnValue.Success = true;
                _logger.Debug(string.Format("{0} has successfully set the guild progression privacy for {1} to {2}", email, guildId, setPublic ? "public" : "private"));
            }
            catch (Exception ex)
            {
                _logger.Debug(string.Format("An error occurred while trying to setting guild progression privacy {0}: {1}", guildId, ex.Message));
                returnValue.Message = string.Format("An error occurred while trying to setting guild progression privacy {0}: {1}", guildId, ex.Message);
            }

            return returnValue;
        }

        public List<Guild> GetAll()
        {
            string timeElapsed;
            return Query(q => q.Query<Guild>(SQL.Guild.GetAll), out timeElapsed).ToList();
        }

        public List<Guild> GetApprovedGuilds()
        {
            string timeElapsed;
            return Query(q => q.Query<Guild, Shard, Guild>(
                MySQL.Guild.ListAllApprovedGuilds,
                (g, s) =>
                {
                    g.Shard = s;
                    return g;
                }), out timeElapsed).ToList();
        }

        /// <summary>
        /// Updated for MySQL
        /// </summary>
        /// <param name="shardId"></param>
        /// <param name="canApplyToOnly"></param>
        /// <returns></returns>
        public List<Guild> GetGuilds(int shardId, bool canApplyToOnly = false)
        {
            string timeElapsed;
            return Query(q => q.Query<Guild>(canApplyToOnly
                ? MySQL.Guild.GuildsAcceptingApplications
                : MySQL.Guild.AllGuildsOnShard
                , new { shardId }), out timeElapsed).ToList();
        }
    }
}
