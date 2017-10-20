using System.Linq;
using Dapper;
using Database.Repositories.Interfaces;

namespace Database.Repositories
{
    public class BossFightDifficultyRepository : DapperRepositoryBase, IBossFightDifficultyRepository
    {
        public BossFightDifficultyRepository(IConnectionFactory connectionFactory) : base(connectionFactory)
        {
        }

        public bool Exists(int bossFightId)
        {
            string timeElapsed;
            return Query(q => q.Query<long>(MySQL.BossFightDifficulty.DifficultyRecordsExist, new { bossFightId }), out timeElapsed).SingleOrDefault() > 0;
        }
    }
}
