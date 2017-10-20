using System;
using System.Collections.Generic;
using System.Linq;
using Database.Models;
using Database.Repositories.Interfaces;
using Quartz;
using Logging;

namespace PrancingTurtle.Helpers.Scheduling.Jobs
{
    public class AddMissingEncounterPlayerRoles : IJob
    {
        private readonly ILogger _logger;
        private readonly IEncounterRepository _encounterRepository;
        private readonly IScheduledTaskRepository _taskRepository;

        public AddMissingEncounterPlayerRoles(ILogger logger, IEncounterRepository encounterRepository, 
            IScheduledTaskRepository taskRepository)
        {
            _logger = logger;
            _encounterRepository = encounterRepository;
            _taskRepository = taskRepository;
        }

        public void Execute(IJobExecutionContext context)
        {
            var task = _taskRepository.Get("EncounterPlayerRoleRecords");
            if (task == null)
            {
                _logger.Debug("Can't update EncounterPlayerRoleRecords - no matching task definition exists in the database.");
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

            var encounters = _encounterRepository.GetEncountersMissingPlayerRecords(100);
            if (!encounters.Any()) return;

            _logger.Debug(string.Format("EncounterPlayerRole update: Found {0} encounters that need updating!", encounters.Count));

            var encounterPlayerRolesToAdd = new List<EncounterPlayerRole>();

            foreach (var id in encounters)
            {
                var rolesFromRecords = _encounterRepository.GetPlayerRoles(id);
                if (rolesFromRecords.Any())
                {
                    var playerRoleList = rolesFromRecords.Select(role => new EncounterPlayerRole()
                    {
                        Class = role.Class,
                        EncounterId = id,
                        PlayerId = role.Id,
                        Role = role.Role,
                        Name = role.Name
                    }).ToList();
                    encounterPlayerRolesToAdd.AddRange(playerRoleList);
                }
                else
                {
                    // Didn't get any roles. Why? No records in each of the tables?
                    var encRecordCount = _encounterRepository.CountBasicRecordsForEncounter(id);
                    if (encRecordCount.DamageCount == 0 &&
                        encRecordCount.HealingCount == 0 &&
                        encRecordCount.ShieldCount == 0)
                    {
                        // Encounter is empty. Remove it
                        _logger.Debug(string.Format("Marking {0} for deletion as it has no basic records.", id));
                        _encounterRepository.MarkEncountersForDeletion(new List<int>() { id }, "scheduledTask");
                    }
                    else
                    {
                        // Records exist, but we couldn't determine roles, so make sure that the damage records
                        // cover the correct duration of the encounter. If the encounter was a wipe, remove it.
                        var thisEncounter = _encounterRepository.Get(id);
                        if (thisEncounter != null && !thisEncounter.SuccessfulKill)
                        {
                            _logger.Debug(string.Format("Marking {0} for deletion as it was a wipe with no available role detection.", id));
                            _encounterRepository.MarkEncountersForDeletion(new List<int>() { id }, "scheduledTask");
                        }
                    }
                }
            }

            if (!encounterPlayerRolesToAdd.Any())
            {
                _logger.Debug("Didn't find any records to add from these encounters, stopping now!");
                return;
            }

            var result = _encounterRepository.AddPlayerEncounterRoles(encounterPlayerRolesToAdd);
            _logger.Debug(result.Success
                ? string.Format("Successfully added {0} EncounterPlayerRole records",
                    encounterPlayerRolesToAdd.Count)
                : string.Format("An error occurred while adding EncounterPlayerRole records: {0}", result.Message));
        }
    }
}