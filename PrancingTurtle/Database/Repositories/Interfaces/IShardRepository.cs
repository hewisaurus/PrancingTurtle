using System.Collections.Generic;
using Database.Models;

namespace Database.Repositories.Interfaces
{
    public interface IShardRepository
    {
        IEnumerable<Shard> GetAll();
        Shard Get(int id);
    }
}
