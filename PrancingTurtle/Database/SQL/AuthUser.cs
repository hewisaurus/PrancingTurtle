namespace Database.SQL
{
    public static class AuthUser
    {
        public static string UserCanUpload 
        {
            get
            {
                return "IF EXISTS(SELECT * FROM AuthUserCharacter AUC " +
                       "JOIN AuthUser AU ON AUC.AuthUserId = AU.Id " +
                       "JOIN GuildRank GR ON AUC.GuildRankId = GR.Id " +
                       "WHERE AUC.GuildRankId IS NOT NULL " +
                       "AND AU.Email = @email " +
                       "AND GR.CanUploadLogs = 1) SELECT 1";
            }
        }

        public static string GetAll
        {
            get { return "SELECT * FROM AuthUser ORDER BY Email ASC"; }
        }

        public static string GetIdByEmail
        {
            get { return "SELECT TOP 1 Id FROM AuthUser WHERE Email = @email"; }
        }

        public static string GetAuthUserByEmail
        {
            get { return "SELECT TOP 1 * FROM AuthUser WHERE Email = @email"; }
        }

        public static string CheckAuthUserExistsByEmail
        {
            get { return "IF EXISTS(SELECT * FROM AuthUser WHERE Email = @email) SELECT 1;"; }
        }

        public static string CanConfirmEmail
        {
            get { return "IF EXISTS(SELECT * FROM AuthUser WHERE Email = @email AND EmailConfirmationToken = @token) SELECT 1"; }
        }

        public static string CanResetPassword
        {
            get { return "IF EXISTS(SELECT * FROM AuthUser WHERE Email = @email AND PasswordResetToken = @token) SELECT 1"; }
        }
    }
}
