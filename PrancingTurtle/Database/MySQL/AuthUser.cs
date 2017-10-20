namespace Database.MySQL
{
    public static class AuthUser
    {
        public static string GetById
        {
            get { return "SELECT * FROM AuthUser WHERE Id = @id"; }
        }
        public static string GetAuthUserByEmail
        {
            get { return "SELECT * FROM AuthUser WHERE Email = @email LIMIT 0,1"; }
        }
        public static string GetAuthIdUserByEmail
        {
            get { return "SELECT Id FROM AuthUser WHERE Email = @email LIMIT 0,1"; }
        }
        public static string CheckAuthUserExistsByEmail
        {
            get { return "SELECT IF (EXISTS(SELECT * FROM AuthUser WHERE Email = @email), 1, 0) AS UserExists"; }
        }

        public static string UserCanUpload
        {
            get
            {
                return "SELECT IF (EXISTS( " +
                       "SELECT * FROM AuthUserCharacter AUC " +
                       "JOIN AuthUser AU ON AUC.AuthUserId = AU.Id " +
                       "JOIN GuildRank GR ON AUC.GuildRankId = GR.Id " +
                       "JOIN Guild G ON AUC.GuildId = G.Id " +
                       "JOIN GuildStatus GS ON G.GuildStatusId = GS.Id " +
                       "WHERE AUC.GuildRankId IS NOT NULL " +
                       "AND AU.Email = @email " +
                       "AND GR.CanUploadLogs = 1 " +
                       "AND GS.Active = 1), 1, 0) AS CanUpload";
            }
        }

        public static string GetAll
        {
            get { return "SELECT * FROM AuthUser ORDER BY Email ASC"; }
        }

        public static string CanResetPassword
        {
            get { return "SELECT IF (EXISTS(SELECT * FROM AuthUser WHERE Email = @email AND PasswordResetToken = @token), 1, 0) AS CanResetPassword"; }
        }

        public static string CanConfirmEmail
        {
            get { return "SELECT IF (EXISTS(SELECT * FROM AuthUser WHERE Email = @email AND EmailConfirmationToken = @token), 1, 0) AS CanConfirmEmail"; }
        }

        public const string GetGroupMembershipForUser =
            "SELECT UG.Name FROM AuthUser AU " +
            "JOIN UserGroupMembership UGM ON AU.Id = UGM.AuthUserId " +
            "JOIN UserGroup UG ON UGM.UserGroupId = UG.Id " +
            "WHERE AU.Email = @email " +
            "GROUP BY UG.Name";
    }
}
