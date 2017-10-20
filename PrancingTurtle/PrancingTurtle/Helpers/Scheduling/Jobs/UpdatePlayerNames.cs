using System;
using System.Collections.Generic;
using System.Linq;
using Database.Models;
using Database.Repositories.Interfaces;
using Quartz;
using Logging;

namespace PrancingTurtle.Helpers.Scheduling.Jobs
{
    public class UpdatePlayerNames : IJob
    {
        private readonly ILogger _logger;
        private readonly IPlayerRepository _playerRepository;
        private readonly IScheduledTaskRepository _taskRepository;

        public UpdatePlayerNames(ILogger logger, IPlayerRepository playerRepository, IScheduledTaskRepository taskRepository)
        {
            _logger = logger;
            _playerRepository = playerRepository;
            _taskRepository = taskRepository;
        }

        public void Execute(IJobExecutionContext context)
        {
            var task = _taskRepository.Get("UpdatePlayerNames");
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

            var playersToUpdate = _playerRepository.PlayersWithShardNamesInPlayerName();
            if (!playersToUpdate.Any())
            {
                _logger.Info("Player name update skipped - no names to change!");

                return;
            }
            List<Player> updatedPlayers = new List<Player>();
            foreach (var player in playersToUpdate)
            {
                var nameSplit = player.Name.Split('@');
                if (nameSplit.Length == 2)
                {
                    player.Name = nameSplit[0];
                    player.Shard = nameSplit[1];
                    updatedPlayers.Add(player);
                }
            }

            var result = _playerRepository.UpdatePlayerNameAndShard(updatedPlayers);
            _logger.Debug(result.Success
                ? "Player name update succeeded."
                : string.Format("Player name update failed. {0}", result.Message));
        }
    }
}