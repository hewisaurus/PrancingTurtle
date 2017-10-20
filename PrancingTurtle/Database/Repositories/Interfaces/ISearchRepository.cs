using Database.QueryModels.Misc;

namespace Database.Repositories.Interfaces
{
    public interface ISearchRepository
    {
        SearchResult Search(string searchTerm, string username, bool showAll = false);
    }
}
