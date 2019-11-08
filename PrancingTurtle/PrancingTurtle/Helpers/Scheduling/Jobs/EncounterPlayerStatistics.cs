using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Database.Repositories.Interfaces;
using Quartz;
using Logging;

namespace PrancingTurtle.Helpers.Scheduling.Jobs
{
    public class EncounterPlayerStatistics : IJob
    {
        private readonly ILogger _logger;
        private readonly IEncounterRepository _encounterRepository;
        private readonly IScheduledTaskRepository _taskRepository;
        private readonly IBossFightSingleTargetDetail _bossFightSingleTargetDetailRepository;

        public EncounterPlayerStatistics(ILogger logger, IEncounterRepository encounterRepository, IScheduledTaskRepository taskRepository, IBossFightSingleTargetDetail bossFightSingleTargetDetailRepository)
        {
            _logger = logger;
            _encounterRepository = encounterRepository;
            _taskRepository = taskRepository;
            _bossFightSingleTargetDetailRepository = bossFightSingleTargetDetailRepository;
        }

        public void Execute(IJobExecutionContext context)
        {
            Debug.WriteLine("** Stepping into the execution of EncounterPlayerStatistics!");

            var task = _taskRepository.Get("EncounterPlayerStatistics");
            if (task == null)
            {
                Debug.WriteLine("Can't update EncounterPlayerStatistics - no matching task definition exists in the database.");
                _logger.Debug("Can't update EncounterPlayerStatistics - no matching task definition exists in the database.");
                return;
            }

            // Check if enough time has passed for us to run this task again
            if (task.LastRun.AddMinutes(task.ScheduleMinutes) > DateTime.Now)
            {
                _logger.Debug("Not enough time has passed for this scheduled task, so it won't be executed now");
                Debug.WriteLine("Not enough time has passed for this scheduled task, so it won't be executed now");
                return;
            }

            // Update the task lastrun time first, so if it takes a minute to run, we don't run it on another server at the same time
            _taskRepository.UpdateTask(task.Id, DateTime.Now);

            Stopwatch sw = new Stopwatch();

            // Don't bother saving stats for encounters that aren't valid for rankings.
            // There's no point saving DPS/HPS/APS for encounters that are not successful, either.
            var encList = _encounterRepository.GetEncountersMissingPlayerStatistics(100);
            if (!encList.Any())
            {
                Debug.WriteLine("Found no encounters that require statistics updates!");
                _logger.Debug("Found no encounters that require statistics updates!");

            }
            else
            {
                _logger.Debug(string.Format("Found {0} encounters to save statistics for", encList.Count));
                Debug.WriteLine(string.Format("Found {0} encounters to save statistics for", encList.Count));
                // Optionally filter our list for a specific bossfight here for testing
                //encList = encList.Where(e => e.BossFightId == 41).ToList();
                //_logger.Debug(string.Format("Updating stats for a filtered set of {0} encounters", encList.Count));

                // Get the list of NPC names to check against when determining single target dps
                var bossFightSingleTargetDetails = _bossFightSingleTargetDetailRepository.GetAll();


                var addPlayerStats = new List<Database.Models.EncounterPlayerStatistics>();

                #region Loop through encounters that we previously identified as needing stats

                for (var i = 1; i <= encList.Count; i++)
                {
                    sw.Reset();
                    sw.Start();
                    var enc = encList[i - 1];

                    if ((int) enc.Duration.TotalSeconds == 0)
                    {
                        _logger.Debug(
                            string.Format(
                                "Skipping stats for encounter {0}/{1} as it has a 0-second duration that needs to be fixed.",
                                i, encList.Count));
                        continue;
                    }

                    //_logger.Debug(string.Format("Calculating stats for encounter {0}/{1}", i, encList.Count));
                    addPlayerStats = new List<Database.Models.EncounterPlayerStatistics>();
                    // DPS/HPS/APS

                    #region Damage

                    foreach (var dmg in _encounterRepository.GetOverviewPlayerDamageDone(enc.Id))
                    {
                        var thisPlayer = addPlayerStats.FirstOrDefault(p => p.PlayerId == dmg.PlayerId);
                        if (thisPlayer == null)
                        {
                            addPlayerStats.Add(new Database.Models.EncounterPlayerStatistics()
                            {
                                EncounterId = enc.Id,
                                PlayerId = dmg.PlayerId,
                                Deaths = 0,
                                APS = 0,
                                DPS = dmg.TotalToNpcs/(long) enc.Duration.TotalSeconds,
                                HPS = 0,
                            });
                        }
                        else
                        {
                            thisPlayer.DPS = dmg.TotalToNpcs/(long) enc.Duration.TotalSeconds;
                        }
                    }

                    #endregion

                    #region Healing

                    foreach (var heal in _encounterRepository.GetOverviewPlayerHealingDone(enc.Id))
                    {
                        var thisPlayer = addPlayerStats.FirstOrDefault(p => p.PlayerId == heal.PlayerId);
                        if (thisPlayer == null)
                        {
                            addPlayerStats.Add(new Database.Models.EncounterPlayerStatistics()
                            {
                                EncounterId = enc.Id,
                                PlayerId = heal.PlayerId,
                                Deaths = 0,
                                APS = 0,
                                DPS = 0,
                                HPS = (heal.TotalToOtherPlayers + heal.TotalToSelf)/(long) enc.Duration.TotalSeconds
                            });
                        }
                        else
                        {
                            thisPlayer.HPS = (heal.TotalToOtherPlayers + heal.TotalToSelf)/
                                             (long) enc.Duration.TotalSeconds;
                        }
                    }

                    #endregion

                    #region Shielding

                    foreach (var shield in _encounterRepository.GetOverviewPlayerShieldingDone(enc.Id))
                    {
                        var thisPlayer = addPlayerStats.FirstOrDefault(p => p.PlayerId == shield.PlayerId);
                        if (thisPlayer == null)
                        {
                            addPlayerStats.Add(new Database.Models.EncounterPlayerStatistics()
                            {
                                EncounterId = enc.Id,
                                PlayerId = shield.PlayerId,
                                Deaths = 0,
                                APS = (shield.TotalToSelf + shield.TotalToOtherPlayers)/(long) enc.Duration.TotalSeconds,
                                DPS = 0,
                                HPS = 0
                            });
                        }
                        else
                        {
                            thisPlayer.APS = (shield.TotalToOtherPlayers + shield.TotalToSelf)/
                                             (long) enc.Duration.TotalSeconds;
                        }
                    }

                    #endregion

                    #region Deaths

                    foreach (var playerDeathCount in _encounterRepository.CountDeathsPerPlayer(enc.Id))
                    {
                        var thisPlayer = addPlayerStats.FirstOrDefault(p => p.PlayerId == playerDeathCount.PlayerId);
                        if (thisPlayer == null)
                        {
                            addPlayerStats.Add(new Database.Models.EncounterPlayerStatistics()
                            {
                                EncounterId = enc.Id,
                                PlayerId = playerDeathCount.PlayerId,
                                Deaths = playerDeathCount.Deaths,
                                APS = 0,
                                DPS = 0,
                                HPS = 0
                            });
                        }
                        else
                        {
                            thisPlayer.Deaths = playerDeathCount.Deaths;
                        }
                    }

                    #endregion

                    // Top Hits

                    #region Damage

                    var topDmg = _encounterRepository.GetTopDamageHits(enc.Id);
                    foreach (var dmg in topDmg)
                    {
                        var thisPlayer = addPlayerStats.FirstOrDefault(p => p.PlayerId == dmg.PlayerId);
                        if (thisPlayer == null)
                        {
                            addPlayerStats.Add(new Database.Models.EncounterPlayerStatistics()
                            {
                                EncounterId = enc.Id,
                                PlayerId = dmg.PlayerId,
                                TopDpsAbilityId = dmg.TopDpsAbilityId,
                                TopDpsAbilityValue = dmg.TopDpsAbilityValue
                            });
                        }
                        else
                        {
                            thisPlayer.TopDpsAbilityId = dmg.TopDpsAbilityId;
                            thisPlayer.TopDpsAbilityValue = dmg.TopDpsAbilityValue;
                        }
                    }

                    #endregion

                    #region Healing

                    var topHeal = _encounterRepository.GetTopHealingHits(enc.Id);
                    foreach (var heal in topHeal)
                    {
                        var thisPlayer = addPlayerStats.FirstOrDefault(p => p.PlayerId == heal.PlayerId);
                        if (thisPlayer == null)
                        {
                            addPlayerStats.Add(new Database.Models.EncounterPlayerStatistics()
                            {
                                EncounterId = enc.Id,
                                PlayerId = heal.PlayerId,
                                TopHpsAbilityId = heal.TopHpsAbilityId,
                                TopHpsAbilityValue = heal.TopHpsAbilityValue
                            });
                        }
                        else
                        {
                            thisPlayer.TopHpsAbilityId = heal.TopHpsAbilityId;
                            thisPlayer.TopHpsAbilityValue = heal.TopHpsAbilityValue;
                        }
                    }

                    #endregion

                    #region Shielding

                    var topShield = _encounterRepository.GetTopShieldHits(enc.Id);
                    foreach (var shield in topShield)
                    {
                        var thisPlayer = addPlayerStats.FirstOrDefault(p => p.PlayerId == shield.PlayerId);
                        if (thisPlayer == null)
                        {
                            addPlayerStats.Add(new Database.Models.EncounterPlayerStatistics()
                            {
                                EncounterId = enc.Id,
                                PlayerId = shield.PlayerId,
                                TopApsAbilityId = shield.TopApsAbilityId,
                                TopApsAbilityValue = shield.TopApsAbilityValue
                            });
                        }
                        else
                        {
                            thisPlayer.TopApsAbilityId = shield.TopApsAbilityId;
                            thisPlayer.TopApsAbilityValue = shield.TopApsAbilityValue;
                        }
                    }

                    #endregion

                    #region Single target DPS

                    var bossFight = bossFightSingleTargetDetails.FirstOrDefault(b => b.BossFightId == enc.BossFightId);
                    if (bossFight != null)
                    {
                        if (bossFight.TargetName.Contains(","))
                        {
                            var npcsToCheck = bossFight.TargetName.Split(',');
                            foreach (var npcName in npcsToCheck)
                            {
                                var dmgRecords = _encounterRepository.GetPlayerSingleTargetDamageDone(enc.Id, npcName);
                                if (dmgRecords.Any())
                                {
                                    foreach (var dmg in dmgRecords)
                                    {
                                        if (dmg.PlayerId == 0) continue;

                                        var thisPlayer = addPlayerStats.FirstOrDefault(p => p.PlayerId == dmg.PlayerId);
                                        if (thisPlayer == null)
                                        {
                                            addPlayerStats.Add(new Database.Models.EncounterPlayerStatistics()
                                            {
                                                EncounterId = enc.Id,
                                                PlayerId = dmg.PlayerId,
                                                SingleTargetDps = dmg.Damage/(long) enc.Duration.TotalSeconds
                                            });
                                        }
                                        else
                                        {
                                            thisPlayer.SingleTargetDps = dmg.Damage/(long) enc.Duration.TotalSeconds;
                                        }
                                    }
                                    // If we've found one name that matches records, then don't bother checking the other names (might save a few queries)
                                    break;
                                }
                            }
                        }
                        else
                        {
                            foreach (
                                var dmg in
                                    _encounterRepository.GetPlayerSingleTargetDamageDone(enc.Id, bossFight.TargetName))
                            {
                                if (dmg.PlayerId == 0) continue;

                                var thisPlayer = addPlayerStats.FirstOrDefault(p => p.PlayerId == dmg.PlayerId);
                                if (thisPlayer == null)
                                {
                                    addPlayerStats.Add(new Database.Models.EncounterPlayerStatistics()
                                    {
                                        EncounterId = enc.Id,
                                        PlayerId = dmg.PlayerId,
                                        SingleTargetDps = dmg.Damage/(long) enc.Duration.TotalSeconds
                                    });
                                }
                                else
                                {
                                    thisPlayer.SingleTargetDps = dmg.Damage/(long) enc.Duration.TotalSeconds;
                                }
                            }
                        }
                    }

                    #endregion

                    sw.Stop();
                    var generateTime = sw.Elapsed.ToString();
                    sw.Reset();
                    sw.Start();
                    // Finished generating statistics, save this encounter's stats now
                    var result = _encounterRepository.SaveEncounterPlayerStatistics(addPlayerStats);
                    sw.Stop();
                    _logger.Debug(
                        string.Format(
                            "Finished saving stats for encounter {0}. Stats generated in {1} and saved in {2}.", enc.Id,
                            generateTime, sw.Elapsed));
                    Debug.WriteLine(string.Format(
                        "Finished saving stats for encounter {0}. Stats generated in {1} and saved in {2}.", enc.Id,
                        generateTime, sw.Elapsed));
                    //addPlayerStats.AddRange(addPlayerStats);
                }

                #endregion
            }
            // Now that we have added stats that didn't exist before, check if there are any single target stats that haven't been calculated at all yet
            var stEncList = _encounterRepository.GetEncountersMissingSingleTargetDpsStatistics(100);
            if (!stEncList.Any())
            {
                _logger.Debug("Found no encounters that require ST DPS updates!");
            }
            else
            {
                _logger.Debug(string.Format("Found {0} encounters to save ST DPS statistics for", stEncList.Count));

                var updatePlayerStats = new List<Database.Models.EncounterPlayerStatistics>();
                var bossFightSingleTargetDetails = _bossFightSingleTargetDetailRepository.GetAll();

                for (var i = 1; i <= stEncList.Count; i++)
                {
                    sw.Reset();
                    sw.Start();
                    var enc = stEncList[i - 1];

                    if ((int)enc.Duration.TotalSeconds == 0)
                    {
                        _logger.Debug(string.Format("Skipping ST DPS stats for encounter {0}/{1} as it has a 0-second duration that needs to be fixed.", i, stEncList.Count));
                        continue;
                    }

                    var bossFight = bossFightSingleTargetDetails.FirstOrDefault(b => b.BossFightId == enc.BossFightId);
                    if (bossFight == null)
                    {
                        _logger.Debug(string.Format("Skipping ST DPS stats for encounter {0}/{1} as a matching bossfight couldn't be found.", i, stEncList.Count));
                        continue;
                    }

                    _logger.Debug(string.Format("Calculating ST DPS stats for encounter {0}/{1}", i, stEncList.Count));
                    //updatePlayerStats = new List<EncounterPlayerStatistics>();

                    #region Damage
                    // Handle comma-separated names here!

                    if (bossFight.TargetName.Contains(","))
                    {
                        var npcsToCheck = bossFight.TargetName.Split(',');
                        foreach (var npcName in npcsToCheck)
                        {
                            var dmgRecords = _encounterRepository.GetPlayerSingleTargetDamageDone(enc.Id, npcName);
                            if (dmgRecords.Any())
                            {
                                foreach (var dmg in dmgRecords)
                                {
                                    if (dmg.PlayerId == 0) continue;

                                    var thisPlayer = updatePlayerStats.FirstOrDefault(p => p.PlayerId == dmg.PlayerId && p.EncounterId == enc.Id);
                                    if (thisPlayer == null)
                                    {
                                        updatePlayerStats.Add(new Database.Models.EncounterPlayerStatistics()
                                        {
                                            EncounterId = enc.Id,
                                            PlayerId = dmg.PlayerId,
                                            SingleTargetDps = dmg.Damage / (long)enc.Duration.TotalSeconds
                                        });
                                    }
                                }
                                // If we've found one name that matches records, then don't bother checking the other names (might save a few queries)
                                break;
                            }
                        }
                    }
                    else
                    {
                        foreach (var dmg in _encounterRepository.GetPlayerSingleTargetDamageDone(enc.Id, bossFight.TargetName))
                        {
                            if (dmg.PlayerId == 0) continue;

                            var thisPlayer = updatePlayerStats.FirstOrDefault(p => p.PlayerId == dmg.PlayerId && p.EncounterId == enc.Id);
                            if (thisPlayer == null)
                            {
                                updatePlayerStats.Add(new Database.Models.EncounterPlayerStatistics()
                                {
                                    EncounterId = enc.Id,
                                    PlayerId = dmg.PlayerId,
                                    SingleTargetDps = dmg.Damage / (long)enc.Duration.TotalSeconds
                                });
                            }
                        }
                    }

                    #endregion
                    sw.Stop();
                    var generateTime = sw.Elapsed.ToString();
                    _logger.Debug(string.Format("Finished calculating {2} ST DPS stats for encounter {0}. Stats generated in {1}.", enc.Id, generateTime, updatePlayerStats.Count));
                    //sw.Reset();
                    //sw.Start();
                    //// Finished generating statistics, save this encounter's stats now
                    //var result = _encounterRepository.UpdateEncounterSingleTargetDpsStatistics(updatePlayerStats);
                    //sw.Stop();
                    //_logger.Debug(string.Format("Finished updating ST DPS stats for encounter {0}. Stats generated in {1} and updated in {2}.", enc.Id, generateTime, sw.Elapsed));
                    //addPlayerStats.AddRange(addPlayerStats);
                }
                var result = _encounterRepository.UpdateEncounterSingleTargetDpsStatistics(updatePlayerStats);
                _logger.Debug(string.Format("Finished updating {0} ST DPS stats for {1} encounters.", updatePlayerStats.Count, stEncList.Count));
            }
        }
    }
}