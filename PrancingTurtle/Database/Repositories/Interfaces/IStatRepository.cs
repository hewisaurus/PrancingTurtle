using Database.QueryModels;

namespace Database.Repositories.Interfaces
{
    public interface IStatRepository
    {
        SiteStats GetSiteStats();
    }
}
