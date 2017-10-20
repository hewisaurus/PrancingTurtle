using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Database.Models;
using Database.Repositories.Interfaces;

namespace Database.Repositories
{
    public class RoleIconRepository : DapperRepositoryBase, IRoleIconRepository
    {
        public RoleIconRepository(IConnectionFactory connectionFactory) : base(connectionFactory)
        {
        }

        public async Task<List<RoleIcon>> GetAllAsync()
        {
            return (await QueryAsync(q => q.QueryAsync<RoleIcon>(MySQL.RoleIcon.GetAll))).ToList();
        }
    }
}
