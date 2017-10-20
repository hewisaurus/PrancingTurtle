using System.Collections.Generic;
using System.Linq;
using Dapper;
using Database.Repositories.Interfaces;

namespace Database.Repositories
{
    public class BossFightSingleTargetDetail : DapperRepositoryBase, IBossFightSingleTargetDetail
    {
        public BossFightSingleTargetDetail(IConnectionFactory connectionFactory) : base(connectionFactory)
        {
        }

        public List<Models.BossFightSingleTargetDetail> GetAll()
        {
            string timeElapsed;
            return
                Query(q => q.Query<Models.BossFightSingleTargetDetail>(MySQL.BossFightSingleTargetDetail.GetAll),
                    out timeElapsed).ToList();
        }
    }
}
