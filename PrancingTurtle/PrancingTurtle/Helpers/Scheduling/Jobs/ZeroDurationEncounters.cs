using System;
using Database.Repositories.Interfaces;
using Quartz;
using Logging;

namespace PrancingTurtle.Helpers.Scheduling.Jobs
{
    public class ZeroDurationEncounters : IJob
    {
        private readonly ILogger _logger;
        private readonly IEncounterRepository _encounterRepository;
        private readonly IScheduledTaskRepository _taskRepository;

        public ZeroDurationEncounters(ILogger logger, IScheduledTaskRepository taskRepository, IEncounterRepository encounterRepository)
        {
            _logger = logger;
            _taskRepository = taskRepository;
            _encounterRepository = encounterRepository;
        }

        public void Execute(IJobExecutionContext context)
        {
            var task = _taskRepository.Get("ZeroDurationEncounters");
            if (task == null)
            {
                _logger.Debug("Can't update zero-duration encounters - no matching task definition exists in the database.");
                return;
            }

            // Check if enough time has passed for us to run this task again
            if (task.LastRun.AddMinutes(task.ScheduleMinutes) > DateTime.Now)
            {
                _logger.Debug("Not enough time has passed for this scheduled task (ZeroDurationEncounters), so it won't be executed now");
                return;
            }

            // Update the task lastrun time first, so if it takes a minute to run, we don't run it on another server at the same time
            _taskRepository.UpdateTask(task.Id, DateTime.Now);

            var encounterIds = _encounterRepository.GetEncounterIdsWithNoDuration();
            foreach (var encId in encounterIds)
            {
                var encSeconds = _encounterRepository.GetTotalSecondsFromDamageDone(encId);
                if (encSeconds > 5)
                {
                    TimeSpan encTs = new TimeSpan(0, 0, 0, encSeconds);
                    _encounterRepository.UpdateDurationForEncounter(encId, encTs);
                }
                else
                {
                    // Remove this encounter from the database entirely
                    _logger.Debug(string.Format("Completely removing encounter {0} as it's not valid or has a 0-second duration that can't be updated.", encId));
                    _encounterRepository.RemoveEncounter("ScheduledTask", encId);
                }
            }
        }
    }
}