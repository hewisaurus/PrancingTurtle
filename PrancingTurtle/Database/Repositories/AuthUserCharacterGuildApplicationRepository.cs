using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Common;
using Dapper;
using Database.Models;
using Database.Repositories.Interfaces;
using Logging;

namespace Database.Repositories
{
    public class AuthUserCharacterGuildApplicationRepository : DapperRepositoryBase, IAuthUserCharacterGuildApplicationRepository
    {
        private readonly ILogger _logger;

        public AuthUserCharacterGuildApplicationRepository(IConnectionFactory connectionFactory, ILogger logger)
            : base(connectionFactory)
        {
            _logger = logger;
        }

        /// <summary>
        /// Updated for MySQL
        /// </summary>
        /// <param name="authUserCharacterId"></param>
        /// <returns></returns>
        public string PendingApplication(int authUserCharacterId)
        {
            string timeElapsed;
            return Query(
                q =>
                    q.Query<string>(MySQL.AuthUserCharacterGuildApplication.GetGuildNameForPendingApplication,
                        new { authUserCharacterId }), out timeElapsed).SingleOrDefault();
        }
        /// <summary>
        /// Updated for MySQL
        /// </summary>
        /// <param name="authUserCharacterId"></param>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public bool PendingApplication(int authUserCharacterId, int guildId)
        {
            string timeElapsed;
            return Query(
                q =>
                    q.Query<long>(MySQL.AuthUserCharacterGuildApplication.HasPendingApplicationForGuild,
                        new { authUserCharacterId, guildId }), out timeElapsed).SingleOrDefault() == 1;
        }
        /// <summary>
        /// Updated for MySQL
        /// </summary>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public List<AuthUserCharacterGuildApplication> PendingApplications(int guildId)
        {
            string timeElapsed;
            return Query(q => q.Query<AuthUserCharacterGuildApplication, AuthUserCharacter, Guild, AuthUserCharacterGuildApplication>
                (MySQL.AuthUserCharacterGuildApplication.GetAllGuildApps,
                        (aucga, auc, g) =>
                        {
                            aucga.Guild = g;
                            aucga.Character = auc;
                            return aucga;
                        },
                        new { guildId }), out timeElapsed).ToList();
        }
        /// <summary>
        /// Updated for MySQL
        /// </summary>
        /// <param name="applicationId"></param>
        /// <returns></returns>
        public AuthUserCharacterGuildApplication GetPendingApplication(int applicationId)
        {
            string timeElapsed;
            return Query(q => q.Query<AuthUserCharacterGuildApplication, AuthUserCharacter, Guild, AuthUserCharacterGuildApplication>
                (MySQL.AuthUserCharacterGuildApplication.GetPendingApplicationById,
                        (aucga, auc, g) =>
                        {
                            aucga.Guild = g;
                            aucga.Character = auc;
                            return aucga;
                        },
                        new { @Id = applicationId }), out timeElapsed).SingleOrDefault();
        }
        /// <summary>
        /// Updated for MySQL
        /// </summary>
        /// <param name="authUserCharacterId"></param>
        /// <returns></returns>
        public AuthUserCharacterGuildApplication GetPendingApplicationForCharacter(int authUserCharacterId)
        {
            string timeElapsed;
            return Query(q => q.Query<AuthUserCharacterGuildApplication, AuthUserCharacter, Guild, AuthUserCharacterGuildApplication>
                (MySQL.AuthUserCharacterGuildApplication.GetPendingApplicationByCharacterId,
                        (aucga, auc, g) =>
                        {
                            aucga.Guild = g;
                            aucga.Character = auc;
                            return aucga;
                        },
                        new { @Id = authUserCharacterId }), out timeElapsed).SingleOrDefault();
        }
        /// <summary>
        /// Updated for MySQL
        /// </summary>
        /// <param name="application"></param>
        /// <returns></returns>
        public ReturnValue Create(AuthUserCharacterGuildApplication application)
        {
            var returnValue = new ReturnValue();

            // Check to see if this character already exists in the guild or has an existing application

            if (PendingApplication(application.AuthUserCharacterId, application.GuildId))
            {
                returnValue.Message = "You have already submitted an application for this guild with this character!";
                return returnValue;
            }

            string timeElapsed;
            var alreadyInGuild = Query(q => q.Query<long>(MySQL.AuthUserCharacter.CharacterIsInGuild,
                new { @authUserCharacterId = application.AuthUserCharacterId, @guildId = application.GuildId }),
                out timeElapsed).SingleOrDefault() == 1;

            if (alreadyInGuild)
            {
                returnValue.Message = "This character is already in the selected guild!";
                return returnValue;
            }

            var guild =
                Query(q => q.Query<Guild, Shard, GuildStatus, Guild>
                    (MySQL.Guild.GetGuildById,
                        (g, s, gs) =>
                        {
                            g.Shard = s;
                            g.Status = gs;
                            return g;
                        }, new { @id = application.GuildId }), out timeElapsed)
                    .Single();
            var character =
                Query(
                    q =>
                        q.Query<AuthUserCharacter>(MySQL.AuthUserCharacter.Get,
                            new {@id = application.AuthUserCharacterId}), out timeElapsed).Single();

            // Add the application
            try
            {
                DapperDb dapperDb = DapperDb.Init(OpenConnection(), 3, false);

                var newId = dapperDb.AuthUserCharacterGuildApplicationTable.Insert(
                    new //AuthUserCharacterGuildApplication()
                    {
                        GuildId = application.GuildId,
                        Message = application.Message,
                        AuthUserCharacterId = application.AuthUserCharacterId
                    });

                if (newId > 0)
                {
                    returnValue.Message = newId.ToString();
                    returnValue.Success = true;
                }

                _logger.Debug(string.Format("{0} has applied to join {1} on {2}", character.CharacterName, guild.Name, guild.Shard.Name));
            }
            catch (Exception ex)
            {
                returnValue.Message = ex.Message;
            }

            return returnValue;
        }

        public ReturnValue Remove(int applicationId, string email)
        {
            ReturnValue returnValue = new ReturnValue();

            // Check that the update ID has actually been set
            if (applicationId == 0)
            {
                const string msg = "Operation failed - The ID of the record to delete has not been set";
                _logger.Error(msg);
                returnValue.Message = msg;
                return returnValue;
            }

            try
            {
                var sw = new Stopwatch();
                sw.Start();
                DapperDb dapperDb = DapperDb.Init(OpenConnection(), 3, false);

                bool success = dapperDb.AuthUserCharacterGuildApplicationTable.Delete(applicationId);

                sw.Stop();

                if (success)
                {
                    _logger.Info(string.Format("The application with the ID of {0} was deleted by {1}", applicationId, email));
                    _logger.Debug(string.Format("Guild application (ID {0}) delete by {1} completed in {2}", applicationId, email, sw.Elapsed));
                }
                else
                {
                    _logger.Warn(string.Format("The application with the ID of {0} could not be deleted.", applicationId));
                    _logger.Debug(string.Format("Guild application {0} delete by {1} failed.", applicationId, email));
                }

                returnValue.Success = success;
                returnValue.TimeTaken = sw.Elapsed;
            }
            catch (Exception ex)
            {
                // Check for foreign key constraint failure first
                if (ex.Message.Contains("DELETE statement conflicted with the REFERENCE constraint") ||
                    ex.Message.Contains("Cannot delete or update a parent row"))
                {
                    string msg =
                        "Delete operation failed - this record is depended upon by other tables within the database and cannot be deleted at this time.";
                    _logger.Error(msg);
                    returnValue.Message = msg;
                    return returnValue;
                }

                returnValue.Message = ex.Message;
            }

            return returnValue;
        }
    }
}