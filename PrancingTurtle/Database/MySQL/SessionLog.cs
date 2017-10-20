namespace Database.MySQL
{
    public static class SessionLog
    {
        public static string TokenExists
        {
            get { return "SELECT IF(EXISTS(SELECT * FROM SessionLog WHERE Token = @token), 1, 0) AS TokenExists"; }
        }

        public static string GetFirstSessionLogForSession
        {
            get { return "SELECT * FROM SessionLog WHERE SessionId = @sessionId ORDER BY CreationDate ASC LIMIT 0,1"; }
        }

        public static string GetUploadersForSession
        {
            get { return "SELECT DISTINCT AUC.*, SH.*, G.* FROM SessionLog SL " +
                         "JOIN AuthUserCharacter AUC ON SL.AuthUserCharacterId = AUC.Id " +
                         "JOIN Shard SH ON AUC.ShardId = SH.Id " +
                         "JOIN Guild G ON AUC.GuildId = G.Id " +
                         "WHERE SessionId = @sessionId " +
                         "ORDER BY AUC.CharacterName ASC"; }
        }
    }
}
