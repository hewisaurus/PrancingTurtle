using System;

namespace Database.Models
{
    public class AuthUser
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string ExtraInformation1 { get; set; }
        public string ExtraInformation2 { get; set; }
        public bool ShortMenuFormat { get; set; }
        public bool ShowGuildMenu { get; set; }
        public int AccessFailedCount { get; set; }
        public bool LockoutEnabled { get; set; }
        public DateTime? LockoutEndDate { get; set; }
        public bool EmailConfirmed { get; set; }
        public string EmailConfirmationToken { get; set; }
        public string PasswordResetToken { get; set; }
        public DateTime? PreviousLoginTime { get; set; }
        public DateTime? LastLoggedIn { get; set; }
        public string PreviousLoginAddress { get; set; }
        public string LastLoginAddress { get; set; }
        public string TimeZone { get; set; }
        public DateTime? Created { get; set; }
    }
}
