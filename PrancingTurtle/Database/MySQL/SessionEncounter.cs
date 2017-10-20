namespace Database.MySQL
{
    public static class SessionEncounter
    {
        public static string GetSessionForEncounter
        {
            get
            {
                // This doesn't fix the issue of users uploading the same session under two names, but it does avoid the crash
                return "SELECT S.* FROM SessionEncounter SE JOIN Session S ON SE.SessionId = S.Id WHERE SE.EncounterId = @encounterId ORDER BY S.Id DESC LIMIT 1";
                return "SELECT S.* FROM SessionEncounter SE JOIN Session S ON SE.SessionId = S.Id WHERE SE.EncounterId = @encounterId";
                //SELECT S.* FROM SessionEncounter SE JOIN Session S ON SE.SessionId = S.Id WHERE SE.EncounterId = 55811 ORDER BY S.Id DESC LIMIT 1;
            }
        }
    }
}
