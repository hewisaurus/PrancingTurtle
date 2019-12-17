using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Database.QueryModels.Misc;
using Database.Repositories.Interfaces;
using Microsoft.AspNet.Identity;
using PrancingTurtle.Models.Misc;
using Logging;
using Player = Database.Models.Player;
using EncounterPlayerStatistics = Database.Models.EncounterPlayerStatistics;
using Database.Models;
using DiscordLogger.Provider;
using PrancingTurtle.Helpers.Authorization;
using PrancingTurtle.Helpers.BurstCalculation;
using PrancingTurtle.Helpers.Math;
using PrancingTurtle.Helpers.Scheduling;

namespace PrancingTurtle.Controllers
{
    [CustomAuthorization]
    public class SilentUpdateController : Controller
    {
        private readonly IEncounterRepository _encounterRepository;
        private readonly IBossFightRepository _bossFightRepository;
        private readonly IEncounterPlayerRoleRepository _encounterPlayerRoleRepository;
        private readonly ILogger _logger;
        private readonly IDiscordService _discord;
        
        public SilentUpdateController(ILogger logger, IEncounterRepository encounterRepository,
            IBossFightRepository bossFightRepository, IEncounterPlayerRoleRepository encounterPlayerRoleRepository, IDiscordService discord)
        {
            _logger = logger;
            _encounterRepository = encounterRepository;
            _bossFightRepository = bossFightRepository;
            _encounterPlayerRoleRepository = encounterPlayerRoleRepository;
            _discord = discord;
        }

        public async Task<ActionResult> CheckForOrphanedEncounters()
        {
            await CheckForOrphanedEncountersAsync();
            TempData.Add("flash", new FlashSuccessViewModel("CheckForOrphanedEncounters() complete!"));
            return RedirectToAction("Index", "Home");
        }

        private async Task CheckForOrphanedEncountersAsync()
        {
            await _encounterRepository.CheckForOrphanedEncountersAsync();
        }

        public async Task<ActionResult> AddMissingEncounterPlayerRolesv2()
        {
            await Add_Missing_EncounterPlayerRolesv2();
            return RedirectToAction("Index", "Home");
        }

        public ActionResult AddMissingEncounterPlayerRoles()
        {
            Add_Missing_EncounterPlayerRoles();
            return RedirectToAction("Index", "Home");
        }

        private async Task Add_Missing_EncounterPlayerRolesv2()
        {
            var encounterIds = await _encounterRepository.GetAllEncounterIdsDescending();
            foreach (var encId in encounterIds)
            {
                Debug.WriteLine($"Checking encounter #{encId}");
                var encPlayersAndRoles = await _encounterRepository.CountEncounterPlayersAndRoles(encId);
                if (encPlayersAndRoles.Players != encPlayersAndRoles.PlayersWithRoles)
                {
                    Debug.WriteLine($"Encounter #{encId} has {encPlayersAndRoles.Players} players but only {encPlayersAndRoles.PlayersWithRoles} roles");

                    // Wipe out all of the EncounterPlayerRole records and update it, then check again
                    if (encPlayersAndRoles.PlayersWithRoles > 0)
                    {
                        var removeResult = await _encounterRepository.RemoveRoleRecordsForEncounter(encId);
                        if (removeResult == false)
                        {
                            Debug.WriteLine($"Something went wrong while removing roles for encounter {encId}");
                        }
                    }

                    var rolesFromRecords = _encounterRepository.GetPlayerRoles(encId);
                    if (rolesFromRecords.Any())
                    {
                        var playerRoleList = rolesFromRecords.Select(role => new EncounterPlayerRole()
                        {
                            Class = role.Class,
                            EncounterId = encId,
                            PlayerId = role.Id,
                            Role = role.Role,
                            Name = role.Name
                        }).ToList();
                        var result = _encounterRepository.AddPlayerEncounterRoles(playerRoleList);
                    }

                    //Debug.WriteLine($"Added {rolesFromRecords.Count} roles. Now, checking again...");

                    //encPlayersAndRoles = await _encounterRepository.CountEncounterPlayersAndRoles(encId);

                    //Debug.WriteLine($"Encounter #{encId} now has {encPlayersAndRoles.Players} players and {encPlayersAndRoles.PlayersWithRoles} roles");
                }
            }
        }

        private void Add_Missing_EncounterPlayerRoles()
        {
            var encounters = _encounterRepository.GetEncountersMissingPlayerRecords(10000);
            if (!encounters.Any()) return;

            _logger.Debug($"EncounterPlayerRole update: Found {encounters.Count} encounters that need updating!");

            Stopwatch sw = new Stopwatch();
            sw.Start();
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
                        _logger.Debug($"Marking {id} for deletion as it has no basic records.");
                        _encounterRepository.MarkEncountersForDeletion(new List<int>() { id }, User.Identity.GetUserId());
                    }
                    else
                    {
                        // Records exist, but we couldn't determine roles, so make sure that the damage records
                        // cover the correct duration of the encounter. If the encounter was a wipe, remove it.
                        var thisEncounter = _encounterRepository.Get(id);
                        if (thisEncounter != null && !thisEncounter.SuccessfulKill)
                        {
                            _logger.Debug($"Marking {id} for deletion as it was a wipe with no available role detection.");
                            _encounterRepository.MarkEncountersForDeletion(new List<int>() { id }, User.Identity.GetUserId());
                        }
                    }
                }
            }

            if (!encounterPlayerRolesToAdd.Any())
            {
                _logger.Debug("Didn't find any records to add from these encounters, stopping now!");
                sw.Stop();
                return;
            }

            var result = _encounterRepository.AddPlayerEncounterRoles(encounterPlayerRolesToAdd);
            _logger.Debug(result.Success
                ? $"Successfully added {encounterPlayerRolesToAdd.Count} EncounterPlayerRole records"
                : $"An error occurred while adding EncounterPlayerRole records: {result.Message}");
            sw.Stop();

            TempData.Add("flash", new FlashSuccessViewModel(
                $"AddMissingEncounterPlayerRoles() complete! Added {encounterPlayerRolesToAdd.Count} in {sw.Elapsed}"));
        }
        
        public async Task<ActionResult> RemoveEncountersMarkedForDeletion()
        {
            await Remove_Encounters_Marked_For_Deletion();
            TempData.Add("flash", new FlashSuccessViewModel("RemoveEncountersMarkedForDeletion() complete!"));
            return RedirectToAction("Index", "Home");
        }

        private async Task Remove_Encounters_Marked_For_Deletion()
        {
            await _encounterRepository.RemoveEncountersMarkedForDeletionAsync(User.Identity.GetUserId());
        }
        
        public ActionResult MarkOldWipesForDeletion(int id = -1)
        {
            // Created 30/11/2015
            Mark_Old_Wipes_For_Deletion(id);
            TempData.Add("flash", new FlashSuccessViewModel("MarkOldWipesForDeletion() complete!"));
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Finds wipes older than 6 months and marks them for deletion
        /// </summary>
        /// <param name="id"></param>
        private void Mark_Old_Wipes_For_Deletion(int id)
        {
            // Get encounters more than 3 months old
            var dateLimit = DateTime.Today.Subtract(new TimeSpan(90, 0, 0, 0));
            var encounters = id == -1
            ? _encounterRepository.GetUnsuccessfulEncountersBefore(dateLimit)
            : _encounterRepository.GetUnsuccessfulEncountersBefore(dateLimit, id); // Specific bossfight
            _logger.Debug($"Found {encounters.Count} encounters that are old enough to be removed");

            int maxEncounters = 500;

            while (encounters.Any())
            {
                var encounterList = encounters.Count > maxEncounters ? encounters.Take(maxEncounters).ToList() : encounters;

                var result = _encounterRepository.MarkEncountersForDeletion(encounterList.Select(e => e.Id).ToList(), User.Identity.GetUserId());
                if (result.Success)
                {
                    _logger.Debug($"Marked {encounterList.Count} encounters for deletion due to age and failure");
                }
                else
                {
                    _logger.Debug($"Error while marking encounters for deletion: {result.Message}");
                    break;
                }

                encounters.RemoveRange(0, encounterList.Count);
            }
        }
        
        public ActionResult RemoveDuplicateEncounterPlayerRoleRecords()
        {
            Remove_Duplicate_EncounterPlayerRole_Records();

            return RedirectToAction("Index", "Home");
        }

        private void Remove_Duplicate_EncounterPlayerRole_Records()
        {
            int i = 0;
            var encounterPlayerRoleIdsToRemove = new List<long>();
            var playerRoles = _encounterPlayerRoleRepository.GetAll().GroupBy(r => r.EncounterId);
            foreach (var pGroup in playerRoles)
            {
                var playerId = -1;
                foreach (var playerRole in pGroup)
                {
                    if (playerRole.PlayerId == playerId)
                    {
                        //_logger.Debug(string.Format("Found a duplicate for encounter {0}: Player ID {1} exists more than once", pGroup.Key, playerRole.PlayerId));
                        i++;
                        encounterPlayerRoleIdsToRemove.Add(playerRole.Id);
                    }
                    //else
                    //{
                    //    _logger.Debug(string.Format("Encounter {0}: first time seeing player {1}", pGroup.Key, playerRole.PlayerId));
                    //}
                    playerId = playerRole.PlayerId;
                }
            }
            _logger.Debug($"Found {i} duplicates!");
            if (encounterPlayerRoleIdsToRemove.Any())
            {
                _encounterPlayerRoleRepository.RemoveDuplicates(encounterPlayerRoleIdsToRemove,
                    User.Identity.GetUserId());
            }
            else
            {
                _logger.Debug("No duplicate EncounterPlayerRole records to remove!");
            }
        }
        
        public ActionResult DoubleCheckEncounterDifficulty()
        {
            // Check that the encounters since a certain date have the correct difficult set against them
            Double_Check_Encounter_Difficulty();

            return RedirectToAction("Index", "Home");
        }

        private void Double_Check_Encounter_Difficulty()
        {
            var encountersToCheck = _encounterRepository.GetSuccessfulEncountersSince(new DateTime(2016, 10, 01));
            foreach (var encounter in encountersToCheck)
            {
                if (_bossFightRepository.DifficultyRecordsExist(encounter.BossFightId))
                {
                    var difficulties = _bossFightRepository.GetDifficultySettings(encounter.BossFightId).OrderByDescending(d => d.OverrideHitpoints);

                    _logger.Debug($"BossFight {encounter.BossFightId} has {difficulties.Count()} difficulty settings");

                    var damageTaken = _encounterRepository.GetTopDamageTakenForNpc(encounter.Id, difficulties.First().OverrideHitpointTarget);

                    bool passedOneDifficulty = false;
                    foreach (var difficulty in difficulties)
                    {
                        if (damageTaken >= difficulty.OverrideHitpoints)
                        {
                            if (encounter.EncounterDifficultyId == difficulty.EncounterDifficultyId)
                            {
                                if (encounter.ValidForRanking)
                                {
                                    //_logger.Debug(string.Format("Encounter {0} ({1}/{2}) is valid for {3} mode but the difficulty is already correct and the encounter is already valid.", encounter.Id, eN, encounters.Count, difficulty.EncounterDifficulty.Name));
                                    passedOneDifficulty = true;
                                    break;
                                }

                                //_logger.Debug(string.Format("Encounter {0} ({1}/{2}) is valid for {3} mode but the difficulty is already correct. Setting validity now.", encounter.Id, eN, encounters.Count, difficulty.EncounterDifficulty.Name));
                                _encounterRepository.MakeValidForRankings(encounter.Id);
                                passedOneDifficulty = true;
                                break;
                            }

                            if (encounter.ValidForRanking)
                            {
                                //_logger.Debug(string.Format("Encounter {0} ({1}/{2}) is valid for {3} mode and is already valid, but the difficulty is incorrect. Changing from {4} to {5}.", encounter.Id, eN, encounters.Count, difficulty.EncounterDifficulty.Name, encounter.EncounterDifficultyId, difficulty.EncounterDifficultyId));
                                _encounterRepository.MakeValidForRankings(encounter.Id, difficulty.EncounterDifficultyId);
                                passedOneDifficulty = true;
                                break;
                            }

                            //_logger.Debug(string.Format("Encounter {0} ({1}/{2}) is valid for {3} mode. Updating both difficulty and validity now.", encounter.Id, eN, encounters.Count, difficulty.EncounterDifficulty.Name));
                            _encounterRepository.MakeValidForRankings(encounter.Id, difficulty.EncounterDifficultyId);
                            passedOneDifficulty = true;
                            break;
                        }

                        //_logger.Debug(string.Format("Encounter {0} ({1}/{2}) is not valid for {3} mode.", encounter.Id, eN, encounters.Count, difficulty.EncounterDifficulty.Name));
                    }
                }
            }
        }
        
        public async Task<ActionResult> UpdateEncounterStatsBurstOnly()
        {
            // 20160622 - Added burst statistics to EncounterPlayerStatistics.
            // Loop through encounters missing these and update them.
            await Update_Encounter_Stats_BurstOnly();

            return RedirectToAction("Index", "Home");
        }

        private async Task Update_Encounter_Stats_BurstOnly()
        {
            var encList = await _encounterRepository.GetEncountersMissingBurstStatistics(15000);
            _logger.Debug($"Found {encList.Count} encounters missing burst statistics");
            // Optionally filter our list for a specific bossfight here for testing
            //encList = encList.Where(e => e.BossFightId == 129).ToList();
            //_logger.Debug($"Updating stats for a filtered set of {encList.Count} encounters");

            Stopwatch sw = new Stopwatch();

            for (var i = 1; i <= encList.Count; i++)
            {
                sw.Restart();
                var enc = encList[i - 1];
                var maxSeconds = Convert.ToInt32(Math.Floor(enc.Duration.TotalSeconds));

                var dmgRecords = await _encounterRepository.GetAllDamageDoneForEncounterAsync(enc.Id);
                var healRecords = await _encounterRepository.GetAllHealingDoneForEncounterAsync(enc.Id);
                var shieldRecords = await _encounterRepository.GetAllShieldingDoneForEncounterAsync(enc.Id);
                var recordTimer = sw.Elapsed.ToString();
                sw.Restart();
                var burstRecords = await BurstCalculator.CalculateBurst(dmgRecords, healRecords, shieldRecords, enc.Id, maxSeconds);
                var calcTimer = sw.Elapsed.ToString();
                sw.Restart();
                // Perform the update
                var updateResult = await _encounterRepository.UpdateEncounterBurstStatistics(burstRecords);
                sw.Stop();
                if (updateResult.Success)
                {
                    _logger.Debug(
                        $"Encounter {enc.Id} burst updated. Records in {recordTimer}, calc in {calcTimer}, updated in {sw.Elapsed}");
                }
                else
                {
                    _logger.Debug($"Encounter {enc.Id} burst update failed: {updateResult.Message}");
                }

            }

            TempData.Add("flash", new FlashSuccessViewModel("Update_Encounter_Stats_BurstOnly() complete!"));
        }
        
        public ActionResult ValidateSuccessfulEncounters(int id = -1)
        {
            Validate_Successful_Encounters(id);

            return RedirectToAction("Index", "Home");
        }

        private void Validate_Successful_Encounters(int id)
        {
            bool limitToCurrentlyInvalid = id == -1;
            // For now, limit it to one BossFight to check that it works
            //var bossFights = _bossFightRepository.GetAll(true).Where(bf => bf.Id == 86).ToList();
            var bossFights = _bossFightRepository.GetAll(true);
            if (id != -1)
            {
                bossFights = bossFights.Where(bf => bf.Id == id).ToList();
                limitToCurrentlyInvalid = false;
            }
            var i = 0;
            foreach (var bossFight in bossFights)
            {
                i++;

                _logger.Debug($"Checking BossFight: {i}/{bossFights.Count} {bossFight.Name}");

                // Check if this bossfight has difficulty settings
                #region Bosses with difficulty settings
                if (_bossFightRepository.DifficultyRecordsExist(bossFight.Id))
                {
                    var difficulties = _bossFightRepository.GetDifficultySettings(bossFight.Id).OrderByDescending(d => d.OverrideHitpoints);

                    _logger.Debug($"BossFight {bossFight.Name} has {difficulties.Count()} difficulty settings");

                    var encounters = _encounterRepository.GetSuccessfulEncounters(bossFight.Id, limitToCurrentlyInvalid);
                    _logger.Debug($"Encounters to check: {encounters.Count}");
                    var eN = 0;
                    foreach (var encounter in encounters)
                    {
                        eN++;
                        var damageTaken = _encounterRepository.GetTopDamageTakenForNpc(encounter.Id, difficulties.First().OverrideHitpointTarget);

                        bool passedOneDifficulty = false;
                        foreach (var difficulty in difficulties)
                        {
                            if (damageTaken >= difficulty.OverrideHitpoints)
                            {
                                if (encounter.EncounterDifficultyId == difficulty.EncounterDifficultyId)
                                {
                                    if (encounter.ValidForRanking)
                                    {
                                        //_logger.Debug(string.Format("Encounter {0} ({1}/{2}) is valid for {3} mode but the difficulty is already correct and the encounter is already valid.", encounter.Id, eN, encounters.Count, difficulty.EncounterDifficulty.Name));
                                        passedOneDifficulty = true;
                                        break;
                                    }

                                    //_logger.Debug(string.Format("Encounter {0} ({1}/{2}) is valid for {3} mode but the difficulty is already correct. Setting validity now.", encounter.Id, eN, encounters.Count, difficulty.EncounterDifficulty.Name));
                                    _encounterRepository.MakeValidForRankings(encounter.Id);
                                    passedOneDifficulty = true;
                                    break;
                                }

                                if (encounter.ValidForRanking)
                                {
                                    //_logger.Debug(string.Format("Encounter {0} ({1}/{2}) is valid for {3} mode and is already valid, but the difficulty is incorrect. Changing from {4} to {5}.", encounter.Id, eN, encounters.Count, difficulty.EncounterDifficulty.Name, encounter.EncounterDifficultyId, difficulty.EncounterDifficultyId));
                                    _encounterRepository.MakeValidForRankings(encounter.Id, difficulty.EncounterDifficultyId);
                                    passedOneDifficulty = true;
                                    break;
                                }

                                //_logger.Debug(string.Format("Encounter {0} ({1}/{2}) is valid for {3} mode. Updating both difficulty and validity now.", encounter.Id, eN, encounters.Count, difficulty.EncounterDifficulty.Name));
                                _encounterRepository.MakeValidForRankings(encounter.Id, difficulty.EncounterDifficultyId);
                                passedOneDifficulty = true;
                                break;
                            }

                            //_logger.Debug(string.Format("Encounter {0} ({1}/{2}) is not valid for {3} mode.", encounter.Id, eN, encounters.Count, difficulty.EncounterDifficulty.Name));
                        }

                        if (!passedOneDifficulty && encounter.ValidForRanking)
                        {
                            // This encounter should not be valid for ranking, so update it here
                            _logger.Debug(
                                $"Encounter {encounter.Id} ({eN}/{encounters.Count}) was marked as valid, but didn't pass any difficulty requirements");
                            _encounterRepository.MakeInvalidForRankings(encounter.Id);
                        }
                    }
                }
                #endregion
                else
                #region Bosses without difficulty settings
                {
                    // If we get here, then this bossfight doesn't have difficulty settings, so proceed normally
                    if (bossFight.Hitpoints == 0)
                    {
                        continue;
                    }

                    var encounters = _encounterRepository.GetSuccessfulEncounters(bossFight.Id, limitToCurrentlyInvalid);
                    _logger.Debug($"Encounters to check: {encounters.Count}");
                    var eN = 0;

                    foreach (var encounter in encounters)
                    {
                        eN++;
                        var damageTaken = _encounterRepository.GetTopDamageTakenForNpc(encounter.Id,
                            string.IsNullOrEmpty(bossFight.HitpointTarget) ? bossFight.Name : bossFight.HitpointTarget);

                        if (damageTaken >= bossFight.Hitpoints && !encounter.ValidForRanking)
                        {
                            // This encounter should be valid for ranking, so update it here
                            //_logger.Debug(string.Format("Encounter {0} ({1}/{2}) should be valid but has not been set yet - damage taken: {3}", encounter.Id, eN, encounters.Count, damageTaken));
                            _encounterRepository.MakeValidForRankings(encounter.Id);
                        }

                        if (damageTaken < bossFight.Hitpoints && encounter.ValidForRanking)
                        {
                            // This encounter should not be valid for ranking, so update it here
                            //_logger.Debug(string.Format("Encounter {0} ({1}/{2}) invalid - damage taken: {3}", encounter.Id, eN, encounters.Count, damageTaken));
                            _encounterRepository.MakeInvalidForRankings(encounter.Id);
                        }
                    }
                }
                #endregion
                _logger.Debug($"Finished checking encounters for {bossFight.Name}");
            }
        }
        
        private Dictionary<string, string> ParseSoulXmlFile(string path)
        {
            StreamReader sr = new StreamReader(path);

            Dictionary<string, string> abilities = new Dictionary<string, string>();

            string line = "";
            string name = "";
            string icon = "";
            while ((line = sr.ReadLine()) != null)
            {
                line = line.Trim();

                if (line.Contains("<Name>"))
                {
                    name = line.Replace("<Name>", "").Replace("</Name>", "").Replace("&apos;", "''");
                }
                else if (line.Contains("<Icon>"))
                {
                    icon = line.Replace("<Icon>", "").Replace("</Icon>", "");
                    if (!abilities.ContainsKey(name))
                    {
                        abilities.Add(name, icon);
                    }
                }
            }

            sr.Close();
            return abilities;
        }

        // Create the task schedule
        public ActionResult KickOffScheduledTasks()
        {
            Start_Task_Scheduler();

            return RedirectToAction("Index", "Home");
        }

        private void Start_Task_Scheduler()
        {
            ScheduledTasks.Start();
        }
    }
}