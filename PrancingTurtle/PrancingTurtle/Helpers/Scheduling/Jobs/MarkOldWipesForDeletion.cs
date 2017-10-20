using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Database.Repositories.Interfaces;
using Logging;
using Quartz;

namespace PrancingTurtle.Helpers.Scheduling.Jobs
{
    public class MarkOldWipesForDeletion : IJob
    {
        private readonly ILogger _logger;
        private readonly IScheduledTaskRepository _taskRepository;
        private readonly IEncounterRepository _encounterRepository;

        public MarkOldWipesForDeletion(ILogger logger, IScheduledTaskRepository taskRepository, IEncounterRepository encounterRepository)
        {
            _logger = logger;
            _taskRepository = taskRepository;
            _encounterRepository = encounterRepository;
        }

        public void Execute(IJobExecutionContext context)
        {

            var task = _taskRepository.Get("MarkOldWipesForDeletion");
            if (task == null)
            {
                _logger.Debug("Can't mark old wipes for deletion - no matching task definition exists in the database.");
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

            var dateLimit = DateTime.Today.Subtract(new TimeSpan(90, 0, 0, 0));
            var encounters = _encounterRepository.GetUnsuccessfulEncountersBefore(dateLimit);

            int maxEncounters = 500;

            while (encounters.Any())
            {
                var encounterList = encounters.Count > maxEncounters ? encounters.Take(maxEncounters).ToList() : encounters;

                var result = _encounterRepository.MarkEncountersForDeletion(encounterList.Select(e => e.Id).ToList(), "ScheduledTask");
                if (result.Success)
                {
                    _logger.Debug(string.Format("Marked {0} encounters for deletion due to age and failure", encounterList.Count));
                }
                else
                {
                    _logger.Debug(string.Format("Error while marking encounters for deletion: {0}", result.Message));
                    break;
                }

                encounters.RemoveRange(0, encounterList.Count);
            }
        }
    }
}