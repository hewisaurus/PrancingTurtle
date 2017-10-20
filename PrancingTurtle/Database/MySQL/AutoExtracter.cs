namespace Database.MySQL
{
    public static class AutoExtracter
    {
        public static string GetSessionLogByToken
        {
            get { return "SELECT * FROM SessionLog SL JOIN Guild G ON SL.GuildId = G.Id WHERE SL.Token = @token LIMIT 0,1"; }
        }
        public static string GetSessionLogByFilename
        {
            get { return "SELECT * FROM SessionLog WHERE Filename = @filename LIMIT 0,1"; }
        }
    }
}
