using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Models;
using Database.Repositories.Interfaces;

namespace Database.Repositories
{
    public class SoulRepository : DapperRepositoryBase, ISoulRepository
    {
        public SoulRepository(IConnectionFactory connectionFactory) : base(connectionFactory)
        {
        }

        public Task<List<Soul>> GetAllAsync()
        {
            throw new NotImplementedException();
        }
    }
}
