using System.Linq;
using Dapper;
using Database.Models;
using Database.Repositories.Interfaces;

namespace Database.Repositories
{
    public class SiteNotificationRepository : DapperRepositoryBase, ISiteNotificationRepository
    {
        public SiteNotificationRepository(IConnectionFactory connectionFactory)
            : base(connectionFactory)
        {
        }

        public SiteNotification GetNotification()
        {
            string timeElapsed;
            return
                Query(q => q.Query<SiteNotification>(MySQL.SiteNotification.GetNotification), out timeElapsed)
                    .SingleOrDefault();
        }

    }
}
