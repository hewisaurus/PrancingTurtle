namespace Database.MySQL
{
    /// <summary>
    /// Provides a simple way of reusing the same aliases for SQL queries
    /// </summary>
    public sealed class AliasTs
    {
        private readonly string _tableName;
        private readonly string _tableAlias;

        public string Name { get { return _tableName; } }
        public string Alias { get { return _tableAlias; } }

        public static readonly AliasTs Ability = new AliasTs("Ability", "AB");
        public static readonly AliasTs AbilityRole = new AliasTs("AbilityRole", "AR");
        public static readonly AliasTs BossFight = new AliasTs("BossFight", "BF");
        public static readonly AliasTs Instance = new AliasTs("Instance", "INS");
        public static readonly AliasTs RoleIcon = new AliasTs("RoleIcon", "RI");
        public static readonly AliasTs PlayerClass = new AliasTs("PlayerClass", "PC");
        public static readonly AliasTs Soul = new AliasTs("Soul", "SL");

        private AliasTs(string tableName, string tableAlias)
        {
            _tableName = tableName;
            _tableAlias = tableAlias;
        }

        public override string ToString()
        {
            return _tableAlias;
        }
    }
}
