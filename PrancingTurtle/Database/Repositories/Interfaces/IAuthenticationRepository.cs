using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Database.Models;

namespace Database.Repositories.Interfaces
{
    public interface IAuthenticationRepository
    {
        // Query
        AuthUser Get(int id);
        bool Validate(string username, string password);
        bool AlreadyExists(string email);
        bool CanConfirmEmail(string email, string token);
        bool CanResetPassword(string email, string token);
        AuthUser GetUserAccount(string email);

        // Async
        Task<AuthUser> GetAsync(int id);
        Task<AuthUser> GetUserAccountAsync(string email);
        Task<List<string>> GetUserGroupMembership(string email);

        Task<bool> ValidateAsync(string username, string password);

        //Command
        ReturnValue Register(string email, string password);
        ReturnValue SetEmailConfirmationToken(string email, string token);
        ReturnValue SetPasswordResetToken(string email, string token);
        ReturnValue ConfirmEmailAddress(string email);
        ReturnValue ResetPassword(string email, string password);
        ReturnValue FailedPasswordAttempt(string email, int totalFailures);
        ReturnValue ResetFailedAttemptCounter(string email);
        ReturnValue LockAccount(string email);
        ReturnValue UpdateLastLoginInfo(string email, DateTime loginTime, string loginAddress);
        ReturnValue SetTimeZone(string email, string timezone);
        ReturnValue UpdateMenuFormat(string email, bool isShortMenuFormat);
        ReturnValue UpdateGuildMenuVisibility(string email, bool showGuildMenu);
    }
}
