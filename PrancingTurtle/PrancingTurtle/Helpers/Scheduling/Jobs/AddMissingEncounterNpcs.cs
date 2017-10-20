using System;
using System.Linq;
using Database.Repositories.Interfaces;
using Quartz;
using Logging;

namespace PrancingTurtle.Helpers.Scheduling.Jobs
{
    public class AddMissingEncounterNpcs : IJob
    {
        private readonly ILogger _logger;
        private readonly IEncounterRepository _encounterRepository;
        private readonly IScheduledTaskRepository _taskRepository;

        public AddMissingEncounterNpcs(ILogger logger, IEncounterRepository encounterRepository, IScheduledTaskRepository taskRepository)
        {
            _logger = logger;
            _encounterRepository = encounterRepository;
            _taskRepository = taskRepository;
        }

        public void Execute(IJobExecutionContext context)
        {
            var task = _taskRepository.Get("EncounterNpcRecords");
            if (task == null)
            {
                _logger.Debug("Can't update EncounterNpcs - no matching task definition exists in the database.");
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

            var encounters = _encounterRepository.GetEncountersMissingNpcRecords(10);
            if (!encounters.Any()) return;

            _logger.Debug(string.Format("EncounterNpc update: Found {0} encounters that need updating!", encounters.Count));

            foreach (var encounter in encounters)
            {
                int encounterId = encounter.Id;
                // Find and add the NPCs for this encounter
                var encounterNpcs = _encounterRepository.GetEncounterNpcsFromEncounterInfo(encounterId);
                if (encounterNpcs.Any())
                {
                    foreach (var encounterNpc in encounterNpcs)
                    {
                        encounterNpc.EncounterId = encounterId;
                        if (string.IsNullOrEmpty(encounterNpc.NpcName))
                        {
                            encounterNpc.NpcName = "UNKNOWN NPC";
                        }
                    }
                    encounterNpcs.ForEach(e => e.EncounterId = encounterId);
                    var addNpcResult = _encounterRepository.AddEncounterNpcs(encounterNpcs);
                    _logger.Debug(addNpcResult.Success
                        ? string.Format("Successfully added {0} EncounterNpc records for {1}", encounterNpcs.Count, encounterId)
                        : string.Format("An error occurred while adding EncounterNpc records for {1}: {0}", addNpcResult.Message, encounterId));
                }
            }
            _logger.Debug("Finished looping through encounters to add EncounterNpc records");

            
        }
    }
}