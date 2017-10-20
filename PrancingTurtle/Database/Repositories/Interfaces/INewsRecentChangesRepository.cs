using System.Collections.Generic;
using Database.Models;

namespace Database.Repositories.Interfaces
{
    public interface INewsRecentChangesRepository
    {
        List<NewsRecentChanges> GetRecentChanges();
    }
}
