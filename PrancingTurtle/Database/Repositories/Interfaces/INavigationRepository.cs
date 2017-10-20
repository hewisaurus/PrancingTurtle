using System.Collections.Generic;
using Database.Models;

namespace Database.Repositories.Interfaces
{
    public interface INavigationRepository
    {
        List<Guild> GetGuildNavigation();
    }
}
