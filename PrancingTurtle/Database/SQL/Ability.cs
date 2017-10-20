namespace Database.SQL
{
    public static class Ability
    { 
        public static string AbilitiesMissingIcons
        {
            get { return "SELECT Id, Name FROM Ability WHERE Icon IS NULL"; }
        }

        public static string DeleteOrphanedAbilities
        {
            get { return "DELETE FROM Ability WHERE Id IN @Ids"; }
        }

        public static string UpdateAbilityIcon
        {
            get { return "UPDATE Ability SET Icon = @icon WHERE Id = @id"; }
        }
    }
}
