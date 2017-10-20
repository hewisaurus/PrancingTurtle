namespace Database.MySQL
{
    public static class AbilityRole
    {
        public static class PagedQuery
        {

            public static string Base(string selectFrom, string selectAlias, string selectObject)
            {
                return string.Format(
                    "SELECT " + selectObject + " FROM {0} {1} " +
                    "JOIN RoleIcon {2} ON {1}.RoleIconId = {2}.Id " +
                    "JOIN PlayerClass {3} ON {1}.PlayerClassId = {3}.Id", 
                    selectFrom, selectAlias, AliasTs.RoleIcon.Alias, AliasTs.PlayerClass.Alias);
            }
            public static string SelectAllFrom(string selectFrom, string selectAlias)
            {
                var selectObject = string.Format("{0}.*, {1}.*, {2}.*", selectAlias, AliasTs.RoleIcon.Alias, AliasTs.PlayerClass.Alias);
                return string.Format("{0} /**where**/ GROUP BY {1}.Id /**orderby**/ LIMIT @offset,@total",
                    Base(selectFrom, selectAlias, selectObject), selectAlias);
            }
            public static string CountAllFrom(string selectFrom, string selectAlias)
            {
                return string.Format("SELECT COUNT(1) FROM ({0} /**where**/ GROUP BY {1}.Id) Q", Base(selectFrom, selectAlias, "1"), selectAlias);
            }
        }
    }
}
