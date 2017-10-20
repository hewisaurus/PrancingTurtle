using System;
using System.Linq;
using Database.Repositories.Interfaces;
using Quartz;
using Logging;

namespace PrancingTurtle.Helpers.Scheduling.Jobs
{
    public class RemoveOrphanedPlayerRecords : IJob
    {
        private readonly ILogger _logger;
        private readonly IPlayerRepository _playerRepository;
        private readonly IScheduledTaskRepository _taskRepository;

        public RemoveOrphanedPlayerRecords(ILogger logger, IPlayerRepository playerRepository, IScheduledTaskRepository taskRepository)
        {
            _logger = logger;
            _playerRepository = playerRepository;
            _taskRepository = taskRepository;
        }

        public void Execute(IJobExecutionContext context)
        {
            var task = _taskRepository.Get("OrphanedPlayerRecords");
            if (task == null)
            {
                _logger.Debug("Can't update player names - no matching task definition exists in the database.");
                return;
            }

            // Check if enough time has passed for us to run this task again
            if (task.LastRun.AddMinutes(task.ScheduleMinutes) > DateTime.Now)
            {
                _logger.Debug("Not enough time has passed for this scheduled task, so it won't be executed now");
                return;
            }

            // Update the task lastrun time first, so if it takes a minute to run, we don't run it on another server at the same time
            _taskRepository.UpdateTask(task.Id, DateTime.Now);

            var playerList = _playerRepository.GetAll();
            var uniquePlayerIds = _playerRepository.GetAllUniquePlayerIds();
            var removePlayerIds = playerList.Select(p => p.Id).Where(playerId => !uniquePlayerIds.Contains(playerId)).ToList();
            _logger.Debug(string.Format("Unique players: {0}, total players in DB: {1}. Players to remove: {2}", uniquePlayerIds.Count, playerList.Count, removePlayerIds.Count));
            if (removePlayerIds.Any())
            {
                _playerRepository.RemoveOrphanPlayers(removePlayerIds);
            }
            else
            {
                _logger.Debug("No orphaned player records to remove!");
            }
        }
    }
}