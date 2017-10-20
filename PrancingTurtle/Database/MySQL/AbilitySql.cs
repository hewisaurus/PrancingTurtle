namespace Database.MySQL
{
    public static class AbilitySql
    {
        public static class PagedQuery
        {
            public static string Base(string selectFrom, string selectAlias, string selectObject)
            {
                return string.Format("SELECT " + selectObject + " FROM {0} {1} ", selectFrom, selectAlias);
            }
            public static string SelectAllFrom(string selectFrom, string selectAlias)
            {
                var selectObject = $"{selectAlias}.*";
                return $"{Base(selectFrom, selectAlias, selectObject)} /**where**/ GROUP BY {selectAlias}.Id /**orderby**/ LIMIT @offset,@total";
            }
            public static string CountAllFrom(string selectFrom, string selectAlias)
            {
                return
                    $"SELECT COUNT(1) FROM ({Base(selectFrom, selectAlias, "1")} /**where**/ GROUP BY {selectAlias}.Id) Q";
            }
        }
    }
}
