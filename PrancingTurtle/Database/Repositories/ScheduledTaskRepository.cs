using System;
using System.Linq;
using Common;
using Dapper;
using Database.Models;
using Database.Repositories.Interfaces;
using Logging;

namespace Database.Repositories
{
    public class ScheduledTaskRepository : DapperRepositoryBase, IScheduledTaskRepository
    {
        private readonly ILogger _logger;

        public ScheduledTaskRepository(IConnectionFactory connectionFactory, ILogger logger)
            : base(connectionFactory)
        {
            _logger = logger;
        }

        public ScheduledTask Get(string name)
        {
            string timeElapsed;
            return
                Query(q => q.Query<ScheduledTask>(MySQL.ScheduledTask.GetByName, new { name }), out timeElapsed)
                    .SingleOrDefault();
        }

        public ReturnValue UpdateTask(int id, DateTime runTime)
        {
            var returnValue = new ReturnValue();

            try
            {

                using (var connection = OpenConnection())
                {
                    connection.Execute(MySQL.ScheduledTask.UpdateRunTime, new {id, @lastRun = runTime});
                }

                returnValue.Success = true;
            }
            catch (Exception ex)
            {
                returnValue.Message = ex.Message;
                _logger.Debug(string.Format("Error while updating task lastrun: {0}", ex.Message));
            }

            return returnValue;
        }
    }
}
