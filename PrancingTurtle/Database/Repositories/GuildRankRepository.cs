using System.Collections.Generic;
using System.Linq;
using Dapper;
using Database.Models;
using Database.Repositories.Interfaces;

namespace Database.Repositories
{
    public class GuildRankRepository : DapperRepositoryBase, IGuildRankRepository
    {
        public GuildRankRepository(IConnectionFactory connectionFactory) : base(connectionFactory)
        {
        }

        /// <summary>
        /// Updated for MySQL
        /// </summary>
        /// <returns></returns>
        public GuildRank GetDefaultRankForGuildCreators()
        {
            string timeElapsed;
            return Query(s => s.Query<GuildRank>(MySQL.GuildRank.Default.GuildCreator), out timeElapsed).SingleOrDefault();
        }
        /// <summary>
        /// Updated for MySQL
        /// </summary>
        /// <returns></returns>
        public GuildRank GetDefaultRankForGuildApplications()
        {
            string timeElapsed;
            return Query(s => s.Query<GuildRank>(MySQL.GuildRank.Default.GuildAppApproved), out timeElapsed).SingleOrDefault();
        }
        /// <summary>
        /// Updated for MySQL
        /// </summary>
        /// <returns></returns>
        public List<GuildRank> GetRanks()
        {
            string timeElapsed;
            return Query(s => s.Query<GuildRank>(MySQL.GuildRank.GetAll), out timeElapsed).ToList();
        }
    }
}
