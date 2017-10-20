namespace Database.Repositories.Interfaces
{
    public interface IRecurringTaskRepo
    {
        void UpdateDailyStats();
        void DeleteRemovedEncounter();
    }
}
