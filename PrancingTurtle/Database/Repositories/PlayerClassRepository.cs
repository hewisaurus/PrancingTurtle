using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Database.Models;
using Database.Repositories.Interfaces;

namespace Database.Repositories
{
    public class PlayerClassRepository : DapperRepositoryBase, IPlayerClassRepository
    {
        public PlayerClassRepository(IConnectionFactory connectionFactory) : base(connectionFactory)
        {
        }

        public async Task<List<PlayerClass>> GetAllAsync()
        {
            return (await QueryAsync(q => q.QueryAsync<PlayerClass>(SQL.PlayerClass.GetAll))).ToList();
        }
    }
}
