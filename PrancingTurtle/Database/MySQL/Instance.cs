namespace Database.MySQL
{
    public static class Instance
    {
        public static string Get
        {
            get { return "SELECT * FROM Instance WHERE Id = @id"; }
        }

        public static string GetAll
        {
            get { return "SELECT * FROM Instance"; }
        }

        public const string Create = "INSERT INTO Instance (Name, MaxRaidSize, Visible, IncludeInProgression, IncludeInLists, ShortName) " +
                                     "VALUES (@name, @maxRaidSize, @visible, @includeInProgression, @includeInLists, @shortName)";

        public const string Delete = "DELETE FROM Instance WHERE Id = @id";
                                     

        public const string Update = "UPDATE Instance " +
                                     "SET Name = @name, MaxRaidSize = @maxRaidSize, Visible = @visible, IncludeInProgression = @includeInProgression, IncludeInLists = @includeInLists, ShortName = @shortName " +
                                     "WHERE Id = @id";
    }
}
