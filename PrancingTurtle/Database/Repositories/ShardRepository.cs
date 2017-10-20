using System.Collections.Generic;
using System.Linq;
using Dapper;
using Database.Models;
using Database.Repositories.Interfaces;

namespace Database.Repositories
{
    public class ShardRepository : DapperRepositoryBase, IShardRepository
    {
        public ShardRepository(IConnectionFactory connectionFactory) : base(connectionFactory)
        {
            
        }

        /// <summary>
        /// Updated for MySQL
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Shard> GetAll()
        {
            string timeElapsed;
            return Query(s => s.Query<Shard>(MySQL.Shard.GetAll), out timeElapsed);
        }
        /// <summary>
        /// Updated for MySQL
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Shard Get(int id)
        {
            string timeElapsed;
            return Query(s => s.Query<Shard>(MySQL.Shard.GetSingle, new { id }), out timeElapsed).SingleOrDefault();
        }
    }
}
