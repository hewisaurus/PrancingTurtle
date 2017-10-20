namespace Database.MySQL
{
    public static class Api
    {
        public static string Validate
        {
            get { return "SELECT IF (EXISTS(SELECT * FROM ApiUsers WHERE AuthKey = @authKey), 1, 0) AS IsValid"; }
        }
    }
}
