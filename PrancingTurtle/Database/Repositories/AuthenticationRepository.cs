using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Dapper;
using Database.Models;
using Database.Repositories.Interfaces;

namespace Database.Repositories
{
    public class AuthenticationRepository : DapperRepositoryBase, IAuthenticationRepository
    {
        public AuthenticationRepository(IConnectionFactory connectionFactory)
            : base(connectionFactory)
        {
            // Once we're logging, set the logger here
        }

        // Queries
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

        public AuthUser Get(int id)
        {
            string timeElapsed;

            return Query(s => s.Query<AuthUser>(MySQL.AuthUser.GetById,
                new { id }), out timeElapsed).SingleOrDefault();
        }

        /// <summary>
        /// Updated for MySQL
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool Validate(string username, string password)
        {
            var user = GetUserAccount(username);

            if (user == null)
            {
                return false;
            }

            return AuthEncryption.ValidatePassword(password, user.PasswordHash, user.ExtraInformation2);
        }
        /// <summary>
        /// Updated for MySQL
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public bool AlreadyExists(string email)
        {
            string timeElapsed;

            return Query(q => q.Query<long>(MySQL.AuthUser.CheckAuthUserExistsByEmail, new { email }), out timeElapsed)
                    .SingleOrDefault() == 1;
        }
        /// <summary>
        /// Updated for MySQL
        /// </summary>
        /// <param name="email"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public bool CanConfirmEmail(string email, string token)
        {
            string timeElapsed;

            return Query(q => q.Query<long>(MySQL.AuthUser.CanConfirmEmail, new { email, token }), out timeElapsed).SingleOrDefault() == 1;
        }
        /// <summary>
        /// Updated for MySQL
        /// </summary>
        /// <param name="email"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public bool CanResetPassword(string email, string token)
        {
            string timeElapsed;

            return Query(q => q.Query<long>(MySQL.AuthUser.CanResetPassword, new { email, token }), out timeElapsed).SingleOrDefault() == 1;
        }
        

        /// <summary>
        /// Updated for MySQL
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public AuthUser GetUserAccount(string email)
        {
            string timeElapsed;

            return Query(s => s.Query<AuthUser>(MySQL.AuthUser.GetAuthUserByEmail,
                new { email }), out timeElapsed).SingleOrDefault();
        }
        /// <summary>
        /// Updated for MySQL
        /// </summary>
        /// <returns></returns>

        #region Asynchronous queries

        public async Task<AuthUser> GetAsync(int id)
        {
            var result = await QueryAsync(q => q.QueryAsync<AuthUser>(MySQL.AuthUser.GetById, new { id }));
            return result.SingleOrDefault();
        }

        public async Task<AuthUser> GetUserAccountAsync(string email)
        {
            var result = await QueryAsync(q => q.QueryAsync<AuthUser>(MySQL.AuthUser.GetAuthUserByEmail, new { email }));
            return result.SingleOrDefault();
        }

        public async Task<List<string>> GetUserGroupMembership(string email)
        {
            return (await QueryAsync(q => q.QueryAsync<string>(MySQL.AuthUser.GetGroupMembershipForUser, new { email }))).ToList();
        }

        public async Task<bool> ValidateAsync(string username, string password)
        {
            var user = await GetUserAccountAsync(username);

            if (user == null)
            {
                return false;
            }

            return AuthEncryption.ValidatePassword(password, user.PasswordHash, user.ExtraInformation2);
        }

        #endregion

        // Commands
        /// <summary>
        /// Updated for MySQL
        /// </summary>
        /// <param name="newItem"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        public ReturnValue Add(AuthUser newItem, string username)
        {
            ReturnValue returnValue = new ReturnValue();

            try
            {
                var sw = new Stopwatch();
                sw.Start();
                DapperDb dapperDb = DapperDb.Init(OpenConnection(), 3, false);

                var newId = dapperDb.AuthUserTable.Insert(
                    new //AuthUser()
                    {
                        AccessFailedCount = 0,
                        ShortMenuFormat = true,
                        ShowGuildMenu = true,
                        Email = newItem.Email,
                        ExtraInformation1 = newItem.ExtraInformation1,
                        ExtraInformation2 = newItem.ExtraInformation2,
                        LockoutEnabled = true,
                        PasswordHash = newItem.PasswordHash,
                        EmailConfirmed = newItem.EmailConfirmed,
                        TimeZone = newItem.TimeZone
                    });

                sw.Stop();

                if (newId != null)
                {
                    returnValue.Success = true;
                    returnValue.TimeTaken = sw.Elapsed;
                }
            }
            catch (Exception ex)
            {
                // Check for unique constraint failure first
                if (ex.Message.Contains("UNIQUE KEY"))
                {
                    const string msg = "Operation failed - a matching AuthUser already exists.";

                    returnValue.Message = msg;
                    return returnValue;
                }

                returnValue.Message = ex.Message;
            }

            return returnValue;
        }

        public ReturnValue Register(string email, string password)
        {
            string hash;
            string salt;
            AuthEncryption.GenerateHashAndSalt(password, out salt, out hash);

            return Add(new AuthUser()
            {
                Email = email,
                ExtraInformation1 = null,
                ExtraInformation2 = salt,
                ShortMenuFormat = true,
                ShowGuildMenu = true,
                AccessFailedCount = 0,
                LockoutEnabled = true,
                LockoutEndDate = null,
                PasswordHash = hash,
                EmailConfirmed = false,
                TimeZone = "UTC"
            }, "system");
        }

        /// <summary>
        /// Updated for MySQL
        /// </summary>
        /// <param name="email"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public ReturnValue SetEmailConfirmationToken(string email, string token)
        {
            ReturnValue returnValue = new ReturnValue();

            try
            {
                var sw = new Stopwatch();
                sw.Start();
                DapperDb dapperDb = DapperDb.Init(OpenConnection(), 3, false);

                var userId = GetIdByEmail(email);
                if (userId == 0)
                {
                    returnValue.Message = string.Format("No user was found with the email address {0}", email);
                    return returnValue;
                }

                var userToSet = dapperDb.AuthUserTable.Get(userId);

                var snapshot = Snapshotter.Start(userToSet);

                userToSet.EmailConfirmationToken = token;

                DynamicParameters dynamicParameters = snapshot.Diff();
                if (!dynamicParameters.ParameterNames.Any())
                {
                    sw.Stop();
                    returnValue.Message = "The token was not updated!";
                    return returnValue;
                }

                dapperDb.AuthUserTable.Update(userId, snapshot.Diff());

                sw.Stop();

                returnValue.Success = true;
                returnValue.TimeTaken = sw.Elapsed;
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
        /// <param name="token"></param>
        /// <returns></returns>
        public ReturnValue SetPasswordResetToken(string email, string token)
        {
            ReturnValue returnValue = new ReturnValue();

            try
            {
                var sw = new Stopwatch();
                sw.Start();
                DapperDb dapperDb = DapperDb.Init(OpenConnection(), 3, false);

                var userId = GetIdByEmail(email);
                if (userId == 0)
                {
                    returnValue.Message = string.Format("No user was found with the email address {0}", email);
                    return returnValue;
                }

                var userToSet = dapperDb.AuthUserTable.Get(userId);

                var snapshot = Snapshotter.Start(userToSet);

                userToSet.PasswordResetToken = token;

                DynamicParameters dynamicParameters = snapshot.Diff();
                if (!dynamicParameters.ParameterNames.Any())
                {
                    sw.Stop();
                    returnValue.Message = "The token was not updated!";
                    return returnValue;
                }

                dapperDb.AuthUserTable.Update(userId, snapshot.Diff());

                sw.Stop();

                returnValue.Success = true;
                returnValue.TimeTaken = sw.Elapsed;
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
        /// <returns></returns>
        public ReturnValue ConfirmEmailAddress(string email)
        {
            ReturnValue returnValue = new ReturnValue();

            try
            {
                var sw = new Stopwatch();
                sw.Start();
                DapperDb dapperDb = DapperDb.Init(OpenConnection(), 3, false);

                var userId = GetIdByEmail(email);
                if (userId == 0)
                {
                    returnValue.Message = string.Format("No user was found with the email address {0}", email);
                    return returnValue;
                }

                var userToSet = dapperDb.AuthUserTable.Get(userId);

                var snapshot = Snapshotter.Start(userToSet);

                userToSet.EmailConfirmationToken = null;
                userToSet.EmailConfirmed = true;

                DynamicParameters dynamicParameters = snapshot.Diff();
                if (!dynamicParameters.ParameterNames.Any())
                {
                    sw.Stop();
                    returnValue.Message = "There was nothing to update!";
                    return returnValue;
                }

                dapperDb.AuthUserTable.Update(userId, snapshot.Diff());

                sw.Stop();

                returnValue.Success = true;
                returnValue.TimeTaken = sw.Elapsed;
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
        /// <param name="password"></param>
        /// <returns></returns>
        public ReturnValue ResetPassword(string email, string password)
        {
            string hash;
            string salt;
            AuthEncryption.GenerateHashAndSalt(password, out salt, out hash);

            ReturnValue returnValue = new ReturnValue();

            try
            {
                var sw = new Stopwatch();
                sw.Start();
                DapperDb dapperDb = DapperDb.Init(OpenConnection(), 3, false);

                var userId = GetIdByEmail(email);
                if (userId == 0)
                {
                    returnValue.Message = string.Format("No user was found with the email address {0}", email);
                    return returnValue;
                }

                var userToSet = dapperDb.AuthUserTable.Get(userId);

                var snapshot = Snapshotter.Start(userToSet);

                userToSet.ExtraInformation2 = salt;
                userToSet.PasswordHash = hash;
                userToSet.PasswordResetToken = null;
                userToSet.AccessFailedCount = 0;

                DynamicParameters dynamicParameters = snapshot.Diff();
                if (!dynamicParameters.ParameterNames.Any())
                {
                    sw.Stop();
                    returnValue.Message = "There was nothing to update!";
                    return returnValue;
                }

                dapperDb.AuthUserTable.Update(userId, snapshot.Diff());

                sw.Stop();

                returnValue.Success = true;
                returnValue.TimeTaken = sw.Elapsed;
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
        /// <param name="totalFailures"></param>
        /// <returns></returns>
        public ReturnValue FailedPasswordAttempt(string email, int totalFailures)
        {
            ReturnValue returnValue = new ReturnValue();

            try
            {
                var sw = new Stopwatch();
                sw.Start();
                DapperDb dapperDb = DapperDb.Init(OpenConnection(), 3, false);

                var userId = GetIdByEmail(email);
                if (userId == 0)
                {
                    returnValue.Message = string.Format("No user was found with the email address {0}", email);
                    return returnValue;
                }

                var userToSet = dapperDb.AuthUserTable.Get(userId);

                var snapshot = Snapshotter.Start(userToSet);

                userToSet.AccessFailedCount = totalFailures;

                DynamicParameters dynamicParameters = snapshot.Diff();
                if (!dynamicParameters.ParameterNames.Any())
                {
                    sw.Stop();
                    returnValue.Message = "There was nothing to update!";
                    return returnValue;
                }

                dapperDb.AuthUserTable.Update(userId, snapshot.Diff());

                sw.Stop();

                returnValue.Success = true;
                returnValue.TimeTaken = sw.Elapsed;
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
        /// <returns></returns>
        public ReturnValue LockAccount(string email)
        {
            ReturnValue returnValue = new ReturnValue();

            try
            {
                var sw = new Stopwatch();
                sw.Start();
                DapperDb dapperDb = DapperDb.Init(OpenConnection(), 3, false);

                var userId = GetIdByEmail(email);
                if (userId == 0)
                {
                    returnValue.Message = string.Format("No user was found with the email address {0}", email);
                    return returnValue;
                }

                var userToSet = dapperDb.AuthUserTable.Get(userId);

                var snapshot = Snapshotter.Start(userToSet);

                userToSet.AccessFailedCount = 0;
                userToSet.LockoutEndDate = DateTime.UtcNow.AddMinutes(5);

                DynamicParameters dynamicParameters = snapshot.Diff();
                if (!dynamicParameters.ParameterNames.Any())
                {
                    sw.Stop();
                    returnValue.Message = "There was nothing to update!";
                    return returnValue;
                }

                dapperDb.AuthUserTable.Update(userId, snapshot.Diff());

                sw.Stop();

                returnValue.Success = true;
                returnValue.TimeTaken = sw.Elapsed;
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
        /// <param name="loginTime"></param>
        /// <param name="loginAddress"></param>
        /// <returns></returns>
        public ReturnValue UpdateLastLoginInfo(string email, DateTime loginTime, string loginAddress)
        {
            ReturnValue returnValue = new ReturnValue();

            try
            {
                var sw = new Stopwatch();
                sw.Start();
                DapperDb dapperDb = DapperDb.Init(OpenConnection(), 3, false);

                var userId = GetIdByEmail(email);
                if (userId == 0)
                {
                    returnValue.Message = string.Format("No user was found with the email address {0}", email);
                    return returnValue;
                }

                var userToSet = dapperDb.AuthUserTable.Get(userId);

                var snapshot = Snapshotter.Start(userToSet);

                userToSet.PreviousLoginAddress = userToSet.LastLoginAddress;
                userToSet.PreviousLoginTime = userToSet.LastLoggedIn;
                userToSet.LastLoggedIn = loginTime;
                userToSet.LastLoginAddress = loginAddress;
                userToSet.AccessFailedCount = 0;

                DynamicParameters dynamicParameters = snapshot.Diff();
                if (!dynamicParameters.ParameterNames.Any())
                {
                    sw.Stop();
                    returnValue.Message = "The failed count was not reset!";
                    return returnValue;
                }

                dapperDb.AuthUserTable.Update(userId, snapshot.Diff());

                sw.Stop();

                returnValue.Success = true;
                returnValue.TimeTaken = sw.Elapsed;
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
        /// <param name="timezone"></param>
        /// <returns></returns>
        public ReturnValue SetTimeZone(string email, string timezone)
        {
            ReturnValue returnValue = new ReturnValue();

            try
            {
                var sw = new Stopwatch();
                sw.Start();
                DapperDb dapperDb = DapperDb.Init(OpenConnection(), 3, false);

                var userId = GetIdByEmail(email);
                if (userId == 0)
                {
                    returnValue.Message = string.Format("No user was found with the email address {0}", email);
                    return returnValue;
                }

                var userToSet = dapperDb.AuthUserTable.Get(userId);

                var snapshot = Snapshotter.Start(userToSet);

                userToSet.TimeZone = timezone;

                DynamicParameters dynamicParameters = snapshot.Diff();
                if (!dynamicParameters.ParameterNames.Any())
                {
                    sw.Stop();
                    returnValue.Message = "The timezone was not changed.";
                    return returnValue;
                }

                dapperDb.AuthUserTable.Update(userId, snapshot.Diff());

                sw.Stop();

                returnValue.Success = true;
                returnValue.TimeTaken = sw.Elapsed;
            }
            catch (Exception ex)
            {
                returnValue.Message = ex.Message;
            }

            return returnValue;
        }

        public ReturnValue UpdateMenuFormat(string email, bool isShortMenuFormat)
        {
            ReturnValue returnValue = new ReturnValue();

            try
            {
                var sw = new Stopwatch();
                sw.Start();
                DapperDb dapperDb = DapperDb.Init(OpenConnection(), 3, false);

                var userId = GetIdByEmail(email);
                if (userId == 0)
                {
                    returnValue.Message = string.Format("No user was found with the email address {0}", email);
                    return returnValue;
                }

                var userToSet = dapperDb.AuthUserTable.Get(userId);

                var snapshot = Snapshotter.Start(userToSet);

                userToSet.ShortMenuFormat = isShortMenuFormat;

                DynamicParameters dynamicParameters = snapshot.Diff();
                if (!dynamicParameters.ParameterNames.Any())
                {
                    sw.Stop();
                    returnValue.Message = "The menu format was not changed.";
                    return returnValue;
                }

                dapperDb.AuthUserTable.Update(userId, snapshot.Diff());


                sw.Stop();

                returnValue.Success = true;
                returnValue.TimeTaken = sw.Elapsed;
            }
            catch (Exception ex)
            {
                returnValue.Message = ex.Message;
            }

            return returnValue;
        }

        public ReturnValue UpdateGuildMenuVisibility(string email, bool showGuildMenu)
        {
            ReturnValue returnValue = new ReturnValue();

            try
            {
                var sw = new Stopwatch();
                sw.Start();
                DapperDb dapperDb = DapperDb.Init(OpenConnection(), 3, false);

                var userId = GetIdByEmail(email);
                if (userId == 0)
                {
                    returnValue.Message = string.Format("No user was found with the email address {0}", email);
                    return returnValue;
                }

                var userToSet = dapperDb.AuthUserTable.Get(userId);

                var snapshot = Snapshotter.Start(userToSet);

                userToSet.ShowGuildMenu = showGuildMenu;

                DynamicParameters dynamicParameters = snapshot.Diff();
                if (!dynamicParameters.ParameterNames.Any())
                {
                    sw.Stop();
                    returnValue.Message = "The guild menu visibility was not changed.";
                    return returnValue;
                }

                dapperDb.AuthUserTable.Update(userId, snapshot.Diff());

                sw.Stop();

                returnValue.Success = true;
                returnValue.TimeTaken = sw.Elapsed;
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
        /// <returns></returns>
        public ReturnValue ResetFailedAttemptCounter(string email)
        {
            ReturnValue returnValue = new ReturnValue();

            try
            {
                var sw = new Stopwatch();
                sw.Start();
                DapperDb dapperDb = DapperDb.Init(OpenConnection(), 3, false);

                var userId = GetIdByEmail(email);
                if (userId == 0)
                {
                    returnValue.Message = string.Format("No user was found with the email address {0}", email);
                    return returnValue;
                }

                var userToSet = dapperDb.AuthUserTable.Get(userId);

                var snapshot = Snapshotter.Start(userToSet);

                userToSet.AccessFailedCount = 0;

                DynamicParameters dynamicParameters = snapshot.Diff();
                if (!dynamicParameters.ParameterNames.Any())
                {
                    sw.Stop();
                    returnValue.Message = "The failed count was not reset!";
                    return returnValue;
                }

                dapperDb.AuthUserTable.Update(userId, snapshot.Diff());

                sw.Stop();

                returnValue.Success = true;
                returnValue.TimeTaken = sw.Elapsed;
            }
            catch (Exception ex)
            {
                returnValue.Message = ex.Message;
            }

            return returnValue;
        }

    }
}
