using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Database.QueryModels;
using DotNet.Highcharts;
using DotNet.Highcharts.Enums;
using DotNet.Highcharts.Helpers;
using DotNet.Highcharts.Options;
using PrancingTurtle.Models.Misc;
using PrancingTurtle.Models.ViewModels.Encounter;
using Database.Models;
using Database.Repositories.Interfaces;
using Logging;

namespace PrancingTurtle.Helpers.Charts
{
    public class EncounterCharts : IEncounterCharts
    {
        private readonly ILogger _logger;
        private readonly IEncounterRepository _encounterRepository;
        private readonly ISessionEncounterRepository _sessionEncounterRepository;

        public EncounterCharts(IEncounterRepository encounterRepository, ILogger logger,
            ISessionEncounterRepository sessionEncounterRepository)
        {
            _encounterRepository = encounterRepository;
            _logger = logger;
            _sessionEncounterRepository = sessionEncounterRepository;
        }
        #region Private methods
        private List<EncounterPlayerRole> EncounterRoleList(int id)
        {
            // Check the list of NPCs seen in this encounter, but don't return this to the UI
            var npcsExist = _encounterRepository.EncounterNpcRecordsExist(id);
            if (!npcsExist)
            {
                // Find and add the NPCs for this encounter
                var encounterNpcs = _encounterRepository.GetEncounterNpcsFromEncounterInfo(id);
                if (encounterNpcs.Any())
                {
                    encounterNpcs.ForEach(e => e.EncounterId = id);
                    _logger.Debug(string.Format("Didn't find EncounterNpc records for encounter {0}, creating them now.", id));
                    var addNpcResult = _encounterRepository.AddEncounterNpcs(encounterNpcs);
                    _logger.Debug(addNpcResult.Success
                        ? string.Format("Successfully added {0} EncounterNpc records", encounterNpcs.Count)
                        : string.Format("An error occurred while adding EncounterNpc records: {0}", addNpcResult.Message));
                }
            }

            // Check if the encounter role list exists in the database for this encounter
            var roles = _encounterRepository.GetEncounterRoleRecords(id);
            if (roles.Any()) return roles;

            _logger.Debug(string.Format("Didn't find EncounterPlayerRole records for encounter {0}, creating them now.", id));

            var rolesFromRecords = _encounterRepository.GetPlayerRoles(id);
            var playerRoleList = rolesFromRecords.Select(role => new EncounterPlayerRole()
            {
                Class = role.Class,
                EncounterId = id,
                PlayerId = role.Id,
                Role = role.Role,
                Name = role.Name
            }).ToList();

            var result = _encounterRepository.AddPlayerEncounterRoles(playerRoleList);
            _logger.Debug(result.Success
                ? string.Format("Successfully added {0} EncounterPlayerRole records", playerRoleList.Count)
                : string.Format("An error occurred while adding EncounterPlayerRole records: {0}", result.Message));

            return playerRoleList;
        }

        private List<OverviewPlayerSomethingDone> UpdateRoleIcons(List<OverviewPlayerSomethingDone> inputList,
            List<EncounterPlayerRole> roleList)
        {
            if (inputList == null) return new List<OverviewPlayerSomethingDone>();

            foreach (var pRole in roleList)
            {
                var player = inputList.FirstOrDefault(p => p.PlayerId == pRole.PlayerId);
                // If the player doesn't exist in the list, add them. If they do, update the icon and such
                if (player != null)
                {
                    player.RoleIcon = pRole.Icon;
                    player.RoleName = pRole.Role;
                    player.Class = pRole.Class;
                }
                else
                {
                    inputList.Add(new OverviewPlayerSomethingDone()
                    {
                        Class = pRole.Class,
                        Average = 0,
                        Percentage = 0,
                        PlayerId = pRole.PlayerId,
                        PlayerName = pRole.Name,
                        ProgressBarPercentage = "0%",
                        Total = 0,
                        TotalToNpcs = 0,
                        TotalToOtherPlayers = 0,
                        TotalToSelf = 0,
                        RoleIcon = pRole.Icon,
                        RoleName = pRole.Role
                    });
                }
            }

            return inputList;
        }

        private List<OverviewPlayerSomethingTaken> UpdateRoleIcons(List<OverviewPlayerSomethingTaken> inputList,
            List<EncounterPlayerRole> roleList)
        {
            if (inputList == null) return new List<OverviewPlayerSomethingTaken>();

            foreach (var pRole in roleList)
            {
                var player = inputList.FirstOrDefault(p => p.PlayerId == pRole.PlayerId);
                // If the player doesn't exist in the list, add them. If they do, update the icon and such
                if (player != null)
                {
                    player.RoleIcon = pRole.Icon;
                    player.RoleName = pRole.Role;
                    player.Class = pRole.Class;
                }
                else
                {
                    inputList.Add(new OverviewPlayerSomethingTaken()
                    {
                        Class = pRole.Class,
                        Average = 0,
                        Percentage = 0,
                        PlayerId = pRole.PlayerId,
                        PlayerName = pRole.Name,
                        ProgressBarPercentage = "0%",
                        Total = 0,
                        TotalFromNpcs = 0,
                        TotalFromOtherPlayers = 0,
                        TotalFromSelf = 0,
                        RoleIcon = pRole.Icon,
                        RoleName = pRole.Role
                    });
                }
            }

            return inputList;
        }
        #endregion

        public PlayerSomethingDone PlayerDamageDone(Encounter encounter)
        {
            Stopwatch swBuildTime = new Stopwatch();
            swBuildTime.Start();

            #region Get Data
            var playerRoles = EncounterRoleList(encounter.Id);
            var damageDone = _encounterRepository.GetOverviewPlayerDamageDone(encounter.Id);
            var playerDamageDone = UpdateRoleIcons(damageDone, playerRoles);
            var damageDoneGraph = _encounterRepository.GetOverviewPlayerDamageDoneGraph(encounter.Id);
            var raidBuffTimers = _encounterRepository.GetMainRaidBuffs(encounter.Id);
            var playerDeaths = _encounterRepository.GetAllPlayerDeathTimers(encounter.Id);
            var npcDeaths = _encounterRepository.GetAllNpcDeathTimers(encounter.Id);
            var encSession = _sessionEncounterRepository.GetSessionForEncounter(encounter.Id);
            #endregion

            if (damageDone.Any())
            {
                // Loop through the totals and generate our progress bar percentages
                long topToNpc = damageDone.First().TotalToNpcs;
                for (int i = 0; i < damageDone.Count; i++)
                {
                    var dmgRow = damageDone[i];
                    dmgRow.ProgressBarPercentage = i == 0
                        ? "100%"
                        : ((decimal)dmgRow.TotalToNpcs / topToNpc).ToString("#.##%");
                }
            }

            #region Export Text

            string exportText = string.Format("{0} {1}: {2}DPS", encounter.Duration.ToString(@"mm\:ss"), encounter.BossFight.Name,
                damageDone.Sum(d => d.TotalToNpcs) / (int)encounter.Duration.TotalSeconds);
            exportText = damageDone
                .OrderByDescending(d => d.TotalToNpcs)
                .Aggregate(exportText, (current, dmg) =>
                    current + string.Format(" | {0} {1}", dmg.PlayerName, dmg.TotalToNpcs / (int)encounter.Duration.TotalSeconds));

            #endregion
            var overallTotal = damageDone.Sum(d => d.Total);
            var overallTotalNpcs = damageDone.Sum(d => d.TotalToNpcs);
            var overallTotalPlayers = damageDone.Sum(d => d.TotalToOtherPlayers);
            var overallTotalSelf = damageDone.Sum(d => d.TotalToSelf);
            damageDone.Insert(0, new OverviewPlayerSomethingDone()
            {
                PlayerId = -1,
                PlayerName = "All players",
                Total = overallTotal,
                TotalToNpcs = overallTotalNpcs,
                TotalToOtherPlayers = overallTotalPlayers,
                TotalToSelf = overallTotalSelf
            });

            #region Create arrays for the chart
            //var totalList = new List<object>();
            var npcList = new List<object>();
            var playerList = new List<object>();
            var secondsList = new List<string>();

            var instantValues = new List<long>();
            var fiveSecondAverageValues = new List<object>();
            var averageValues = new List<object>();
            var playerDeathList = new List<object>();
            var npcDeathList = new List<object>();

            for (int i = 0; i <= encounter.Duration.TotalSeconds; i++)
            {
                npcList.Add(damageDoneGraph.Where(d => d.SecondsElapsed == i).Sum(d => d.TotalNpcs));
                playerList.Add(damageDoneGraph.Where(d => d.SecondsElapsed == i).Sum(d => d.TotalPlayers));
                instantValues.Add(damageDoneGraph.Where(d => d.SecondsElapsed == i).Sum(d => d.TotalNpcs));
                secondsList.Add(i.ToString(CultureInfo.InvariantCulture));
                playerDeathList.Add(playerDeaths.Count(d => d == i));
                npcDeathList.Add(npcDeaths.Count(d => d == i));
                // 5-second moving average
                if (i > 1 && i <= encounter.Duration.TotalSeconds - 2)
                {
                    // Able to calculate - use -2 to +2 seconds
                    fiveSecondAverageValues.Add((damageDoneGraph.Where(d => d.SecondsElapsed >= i - 2 && d.SecondsElapsed <= i + 2).Sum(d => d.TotalNpcs)) / 5);
                }
                else
                {
                    // Unable to calculate moving average, so add 0
                    fiveSecondAverageValues.Add(0);
                }
            }

            // Loop through instant values and calculate average
            long npcTotalDmg = 0;
            for (var i = 0; i < instantValues.Count; i++)
            {
                npcTotalDmg += instantValues[i];
                if (i == 0)
                {
                    averageValues.Add(0);
                }
                else
                {
                    averageValues.Add(npcTotalDmg / i);
                }
            }

            //var totalArray = totalList.ToArray();
            var npcArray = npcList.ToArray();
            var playerArray = playerList.ToArray();
            var secondsArray = secondsList.ToArray();
            #endregion

            var model = new PlayerSomethingDone()
            {
                Encounter = encounter,
                Data = playerDamageDone,
                AverageText = "Average DPS",
                GraphType = "DPS",
                PageTitle = "Damage Done by Players",
                IsOutgoing = true,
                TotalText = "Total Damage",
                Session = encSession,
                ExportText = exportText,
                SplineGraph = PlayerNpcSomethingDoneOrTaken(encounter.Id, secondsArray, "Seconds elapsed", npcArray,
                "Deaths", averageValues.ToArray(), fiveSecondAverageValues.ToArray(), true, true, "DPS", raidBuffTimers, playerDeathList, npcDeathList)
            };
            swBuildTime.Stop();
            model.BuildTime = swBuildTime.Elapsed;

            return model;
        }

        public PlayerSomethingTaken PlayerDamageTaken(Encounter encounter)
        {
            Stopwatch swBuildTime = new Stopwatch();
            swBuildTime.Start();

            #region Get Data
            var playerRoles = EncounterRoleList(encounter.Id);
            var damageTaken = _encounterRepository.GetOverviewPlayerDamageTaken(encounter.Id);
            var playerDamageTaken = UpdateRoleIcons(damageTaken, playerRoles);
            var damageTakenGraph = _encounterRepository.GetOverviewPlayerDamageTakenGraph(encounter.Id);
            var raidBuffTimers = _encounterRepository.GetMainRaidBuffs(encounter.Id);
            var playerDeaths = _encounterRepository.GetAllPlayerDeathTimers(encounter.Id);
            var npcDeaths = _encounterRepository.GetAllNpcDeathTimers(encounter.Id);
            var encSession = _sessionEncounterRepository.GetSessionForEncounter(encounter.Id);
            #endregion

            if (damageTaken.Any(d => d.TotalFromNpcs > 0))
            {
                // Loop through the totals and generate our progress bar percentages
                long topToNpc = damageTaken.First().TotalFromNpcs;
                for (int i = 0; i < damageTaken.Count; i++)
                {
                    var dmgRow = damageTaken[i];
                    dmgRow.ProgressBarPercentage = i == 0
                        ? "100%"
                        : ((decimal)dmgRow.TotalFromNpcs / topToNpc).ToString("#.##%");
                }
            }

            #region Export Text

            string exportText = string.Format("{0} {1}: {2}DTPS", encounter.Duration.ToString(@"mm\:ss"), encounter.BossFight.Name,
                damageTaken.Sum(d => d.Total) / (int)encounter.Duration.TotalSeconds);
            exportText = damageTaken
                .OrderByDescending(d => d.Total)
                .Aggregate(exportText, (current, dmg) =>
                    current + string.Format(" | {0} {1}", dmg.PlayerName, dmg.Total / (int)encounter.Duration.TotalSeconds));

            #endregion
            var overallTotal = damageTaken.Sum(d => d.Total);
            var overallTotalNpcs = damageTaken.Sum(d => d.TotalFromNpcs);
            var overallTotalPlayers = damageTaken.Sum(d => d.TotalFromOtherPlayers);
            var overallTotalSelf = damageTaken.Sum(d => d.TotalFromSelf);
            damageTaken.Insert(0, new OverviewPlayerSomethingTaken()
            {
                PlayerId = -1,
                PlayerName = "All players",
                Total = overallTotal,
                TotalFromNpcs = overallTotalNpcs,
                TotalFromOtherPlayers = overallTotalPlayers,
                TotalFromSelf = overallTotalSelf
            });

            #region Create arrays for the chart
            //var totalList = new List<object>();
            var npcList = new List<object>();
            var playerList = new List<object>();
            var secondsList = new List<string>();

            var instantValues = new List<long>();
            var fiveSecondAverageValues = new List<object>();
            var averageValues = new List<object>();
            var playerDeathList = new List<object>();
            var npcDeathList = new List<object>();

            for (int i = 0; i <= encounter.Duration.TotalSeconds; i++)
            {
                var totalNpcs = damageTakenGraph.Where(d => d.SecondsElapsed == i).Sum(d => d.TotalNpcs);
                var totalPlayers = damageTakenGraph.Where(d => d.SecondsElapsed == i).Sum(d => d.TotalPlayers);

                npcList.Add(totalNpcs);
                playerList.Add(totalPlayers);
                instantValues.Add(totalNpcs + totalPlayers);
                secondsList.Add(i.ToString(CultureInfo.InvariantCulture));
                playerDeathList.Add(playerDeaths.Count(d => d == i));
                //npcDeathList.Add(npcDeaths.Count(d => d == i));
                // 5-second moving average
                if (i > 1 && i <= encounter.Duration.TotalSeconds - 2)
                {
                    // Able to calculate - use -2 to +2 seconds
                    fiveSecondAverageValues.Add((damageTakenGraph.Where(d => d.SecondsElapsed >= i - 2 && d.SecondsElapsed <= i + 2).Sum(d => d.Total)) / 5);
                }
                else
                {
                    // Unable to calculate moving average, so add 0
                    fiveSecondAverageValues.Add(0);
                }
            }

            // Loop through instant values and calculate average
            long totalDamageTaken = 0;
            for (var i = 0; i < instantValues.Count; i++)
            {
                totalDamageTaken += instantValues[i];
                if (i == 0)
                {
                    averageValues.Add(0);
                }
                else
                {
                    averageValues.Add(totalDamageTaken / i);
                }
            }

            var npcArray = npcList.ToArray();
            var playerArray = playerList.ToArray();
            var secondsArray = secondsList.ToArray();
            #endregion

            var model = new PlayerSomethingTaken()
            {
                Encounter = encounter,
                Data = playerDamageTaken,
                AverageText = "Average DPS",
                GraphType = "DPS",
                PageTitle = "Damage Taken by Players",
                IsOutgoing = false,
                TotalText = "Total Damage",
                Session = encSession,
                ExportText = exportText,
                SplineGraph = PlayerNpcSomethingDoneOrTaken(encounter.Id, secondsArray, "Seconds elapsed", npcArray,
                "Deaths", averageValues.ToArray(), fiveSecondAverageValues.ToArray(), false, true, "DPS", null, playerDeathList, null)
            };
            swBuildTime.Stop();
            model.BuildTime = swBuildTime.Elapsed;

            return model;
        }

        public PlayerSomethingDone PlayerHealingDone(Encounter encounter)
        {
            Stopwatch swBuildTime = new Stopwatch();
            swBuildTime.Start();

            #region Get Data
            var playerRoles = EncounterRoleList(encounter.Id);
            var healingDone = _encounterRepository.GetOverviewPlayerHealingDone(encounter.Id);
            var playerHealingDone = UpdateRoleIcons(healingDone, playerRoles);
            var healingDoneGraph = _encounterRepository.GetOverviewPlayerHealingDoneGraph(encounter.Id);
            var raidBuffTimers = _encounterRepository.GetMainRaidBuffs(encounter.Id);
            var playerDeaths = _encounterRepository.GetAllPlayerDeathTimers(encounter.Id);
            var npcDeaths = _encounterRepository.GetAllNpcDeathTimers(encounter.Id);
            var encSession = _sessionEncounterRepository.GetSessionForEncounter(encounter.Id);
            #endregion

            if (healingDone.Any())
            {
                // Loop through the totals and generate our progress bar percentages
                long topTotal = healingDone.First().Total;
                for (int i = 0; i < healingDone.Count; i++)
                {
                    var healRow = healingDone[i];
                    healRow.ProgressBarPercentage = i == 0
                        ? "100%"
                        : ((decimal)healRow.Total / topTotal).ToString("#.##%");
                }
            }

            #region Export Text

            string exportText = string.Format("{0} {1}: {2}HPS", encounter.Duration.ToString(@"mm\:ss"), encounter.BossFight.Name,
                healingDone.Sum(h => h.Total) / (int)encounter.Duration.TotalSeconds);
            exportText = healingDone
                .OrderByDescending(h => h.Total)
                .Aggregate(exportText, (current, heal) =>
                    current + string.Format(" | {0} {1}", heal.PlayerName, heal.Total / (int)encounter.Duration.TotalSeconds));

            #endregion
            var overallTotal = healingDone.Sum(d => d.Total);
            var overallTotalNpcs = healingDone.Sum(d => d.TotalToNpcs);
            var overallTotalPlayers = healingDone.Sum(d => d.TotalToOtherPlayers);
            var overallTotalSelf = healingDone.Sum(d => d.TotalToSelf);
            healingDone.Insert(0, new OverviewPlayerSomethingDone()
            {
                PlayerId = -1,
                PlayerName = "All players",
                Total = overallTotal,
                TotalToNpcs = overallTotalNpcs,
                TotalToOtherPlayers = overallTotalPlayers,
                TotalToSelf = overallTotalSelf
            });

            #region Create arrays for the chart
            //var totalList = new List<object>();
            var npcList = new List<object>();
            var playerList = new List<object>();
            var secondsList = new List<string>();

            var instantValues = new List<long>();
            var fiveSecondAverageValues = new List<object>();
            var averageValues = new List<object>();
            var playerDeathList = new List<object>();
            var npcDeathList = new List<object>();

            for (int i = 0; i <= encounter.Duration.TotalSeconds; i++)
            {
                npcList.Add(healingDoneGraph.Where(d => d.SecondsElapsed == i).Sum(d => d.TotalNpcs));
                playerList.Add(healingDoneGraph.Where(d => d.SecondsElapsed == i).Sum(d => d.TotalPlayers));
                instantValues.Add(healingDoneGraph.Where(d => d.SecondsElapsed == i).Sum(d => d.TotalPlayers));
                secondsList.Add(i.ToString(CultureInfo.InvariantCulture));
                playerDeathList.Add(playerDeaths.Count(d => d == i));
                npcDeathList.Add(npcDeaths.Count(d => d == i));
                // 5-second moving average
                if (i > 1 && i <= encounter.Duration.TotalSeconds - 2)
                {
                    // Able to calculate - use -2 to +2 seconds
                    fiveSecondAverageValues.Add((healingDoneGraph.Where(d => d.SecondsElapsed >= i - 2 && d.SecondsElapsed <= i + 2).Sum(d => d.TotalPlayers)) / 5);
                }
                else
                {
                    // Unable to calculate moving average, so add 0
                    fiveSecondAverageValues.Add(0);
                }
            }

            // Loop through instant values and calculate average
            long total = 0;
            for (var i = 0; i < instantValues.Count; i++)
            {
                total += instantValues[i];
                if (i == 0)
                {
                    averageValues.Add(0);
                }
                else
                {
                    averageValues.Add(total / i);
                }
            }

            //var totalArray = totalList.ToArray();
            var npcArray = npcList.ToArray();
            var playerArray = playerList.ToArray();
            var secondsArray = secondsList.ToArray();
            #endregion

            var model = new PlayerSomethingDone()
            {
                Encounter = encounter,
                Data = playerHealingDone,
                AverageText = "Average HPS",
                GraphType = "HPS",
                PageTitle = "Healing done by Players",
                IsOutgoing = true,
                TotalText = "Total healing",
                Session = encSession,
                ExportText = exportText,
                SplineGraph = PlayerNpcSomethingDoneOrTaken(encounter.Id, secondsArray, "Seconds elapsed", playerArray,
                "Deaths", averageValues.ToArray(), fiveSecondAverageValues.ToArray(), true, true, "HPS", raidBuffTimers, playerDeathList, null)
            };
            swBuildTime.Stop();
            model.BuildTime = swBuildTime.Elapsed;

            return model;
        }

        public PlayerSomethingTaken PlayerHealingTaken(Encounter encounter)
        {
            Stopwatch swBuildTime = new Stopwatch();
            swBuildTime.Start();

            #region Get Data
            var playerRoles = EncounterRoleList(encounter.Id);
            var healingTaken = _encounterRepository.GetOverviewPlayerHealingTaken(encounter.Id);
            var playerHealingTaken = UpdateRoleIcons(healingTaken, playerRoles);
            var healingTakenGraph = _encounterRepository.GetOverviewPlayerHealingTakenGraph(encounter.Id);

            var playerDeaths = _encounterRepository.GetAllPlayerDeathTimers(encounter.Id);
            var encSession = _sessionEncounterRepository.GetSessionForEncounter(encounter.Id);
            #endregion

            if (healingTaken.Any())
            {
                // Loop through the totals and generate our progress bar percentages
                long topTotal = healingTaken.First().Total;
                for (int i = 0; i < healingTaken.Count; i++)
                {
                    var healRow = healingTaken[i];
                    healRow.ProgressBarPercentage = i == 0
                        ? "100%"
                        : ((decimal)healRow.Total / topTotal).ToString("#.##%");
                }
            }

            #region Export Text

            string exportText = string.Format("{0} {1}: {2}HTPS", encounter.Duration.ToString(@"mm\:ss"), encounter.BossFight.Name,
                healingTaken.Sum(h => h.Total) / (int)encounter.Duration.TotalSeconds);
            exportText = healingTaken
                .OrderByDescending(h => h.Total)
                .Aggregate(exportText, (current, heal) =>
                    current + string.Format(" | {0} {1}", heal.PlayerName, heal.Total / (int)encounter.Duration.TotalSeconds));

            #endregion
            var overallTotal = healingTaken.Sum(d => d.Total);
            var overallTotalNpcs = healingTaken.Sum(d => d.TotalFromNpcs);
            var overallTotalPlayers = healingTaken.Sum(d => d.TotalFromOtherPlayers);
            var overallTotalSelf = healingTaken.Sum(d => d.TotalFromSelf);
            healingTaken.Insert(0, new OverviewPlayerSomethingTaken()
            {
                PlayerId = -1,
                PlayerName = "All players",
                Total = overallTotal,
                TotalFromNpcs = overallTotalNpcs,
                TotalFromOtherPlayers = overallTotalPlayers,
                TotalFromSelf = overallTotalSelf
            });

            #region Create arrays for the chart
            //var totalList = new List<object>();
            var npcList = new List<object>();
            var playerList = new List<object>();
            var secondsList = new List<string>();

            var instantValues = new List<long>();
            var fiveSecondAverageValues = new List<object>();
            var averageValues = new List<object>();
            var playerDeathList = new List<object>();

            for (int i = 0; i <= encounter.Duration.TotalSeconds; i++)
            {
                npcList.Add(healingTakenGraph.Where(d => d.SecondsElapsed == i).Sum(d => d.TotalNpcs));
                playerList.Add(healingTakenGraph.Where(d => d.SecondsElapsed == i).Sum(d => d.TotalPlayers));
                instantValues.Add(healingTakenGraph.Where(d => d.SecondsElapsed == i).Sum(d => d.TotalPlayers));
                secondsList.Add(i.ToString(CultureInfo.InvariantCulture));
                playerDeathList.Add(playerDeaths.Count(d => d == i));
                //npcDeathList.Add(npcDeaths.Count(d => d == i));
                // 5-second moving average
                if (i > 1 && i <= encounter.Duration.TotalSeconds - 2)
                {
                    // Able to calculate - use -2 to +2 seconds
                    fiveSecondAverageValues.Add((healingTakenGraph.Where(d => d.SecondsElapsed >= i - 2 && d.SecondsElapsed <= i + 2).Sum(d => d.TotalPlayers)) / 5);
                }
                else
                {
                    // Unable to calculate moving average, so add 0
                    fiveSecondAverageValues.Add(0);
                }
            }

            // Loop through instant values and calculate average
            long total = 0;
            for (var i = 0; i < instantValues.Count; i++)
            {
                total += instantValues[i];
                if (i == 0)
                {
                    averageValues.Add(0);
                }
                else
                {
                    averageValues.Add(total / i);
                }
            }

            var npcArray = npcList.ToArray();
            var playerArray = playerList.ToArray();
            var secondsArray = secondsList.ToArray();
            #endregion

            var model = new PlayerSomethingTaken()
            {
                Encounter = encounter,
                Data = playerHealingTaken,
                AverageText = "Average HPS",
                GraphType = "HPS",
                PageTitle = "Healing received by players",
                IsOutgoing = false,
                TotalText = "Total healing",
                Session = encSession,
                ExportText = exportText,
                SplineGraph = PlayerNpcSomethingDoneOrTaken(encounter.Id, secondsArray, "Seconds elapsed", playerArray,
                "Deaths", averageValues.ToArray(), fiveSecondAverageValues.ToArray(), false, true, "HPS", null, playerDeathList, null)
            };
            swBuildTime.Stop();
            model.BuildTime = swBuildTime.Elapsed;

            return model;
        }

        public PlayerSomethingDone PlayerShieldingDone(Encounter encounter)
        {
            Stopwatch swBuildTime = new Stopwatch();
            swBuildTime.Start();

            #region Get Data
            var playerRoles = EncounterRoleList(encounter.Id);
            var shieldingDone = _encounterRepository.GetOverviewPlayerShieldingDone(encounter.Id);
            var playerShieldingDone = UpdateRoleIcons(shieldingDone, playerRoles);
            var shieldingDoneGraph = _encounterRepository.GetOverviewPlayerShieldingDoneGraph(encounter.Id);
            var raidBuffTimers = _encounterRepository.GetMainRaidBuffs(encounter.Id);
            var playerDeaths = _encounterRepository.GetAllPlayerDeathTimers(encounter.Id);
            var npcDeaths = _encounterRepository.GetAllNpcDeathTimers(encounter.Id);
            var encSession = _sessionEncounterRepository.GetSessionForEncounter(encounter.Id);
            #endregion

            if (shieldingDone.Any())
            {
                // Loop through the totals and generate our progress bar percentages
                long topTotal = shieldingDone.First().Total;
                for (int i = 0; i < shieldingDone.Count; i++)
                {
                    var shieldRow = shieldingDone[i];
                    shieldRow.ProgressBarPercentage = i == 0
                        ? "100%"
                        : ((decimal)shieldRow.Total / topTotal).ToString("#.##%");
                }
            }

            #region Export Text

            string exportText = string.Format("{0} {1}: {2}APS", encounter.Duration.ToString(@"mm\:ss"), encounter.BossFight.Name,
                shieldingDone.Sum(h => h.Total) / (int)encounter.Duration.TotalSeconds);
            exportText = shieldingDone
                .OrderByDescending(h => h.Total)
                .Aggregate(exportText, (current, shield) =>
                    current + string.Format(" | {0} {1}", shield.PlayerName, shield.Total / (int)encounter.Duration.TotalSeconds));

            #endregion
            var overallTotal = shieldingDone.Sum(d => d.Total);
            var overallTotalNpcs = shieldingDone.Sum(d => d.TotalToNpcs);
            var overallTotalPlayers = shieldingDone.Sum(d => d.TotalToOtherPlayers);
            var overallTotalSelf = shieldingDone.Sum(d => d.TotalToSelf);
            shieldingDone.Insert(0, new OverviewPlayerSomethingDone()
            {
                PlayerId = -1,
                PlayerName = "All players",
                Total = overallTotal,
                TotalToNpcs = overallTotalNpcs,
                TotalToOtherPlayers = overallTotalPlayers,
                TotalToSelf = overallTotalSelf
            });

            #region Create arrays for the chart
            //var totalList = new List<object>();
            var npcList = new List<object>();
            var playerList = new List<object>();
            var secondsList = new List<string>();

            var instantValues = new List<long>();
            var fiveSecondAverageValues = new List<object>();
            var averageValues = new List<object>();
            var playerDeathList = new List<object>();
            var npcDeathList = new List<object>();

            for (int i = 0; i <= encounter.Duration.TotalSeconds; i++)
            {
                npcList.Add(shieldingDoneGraph.Where(d => d.SecondsElapsed == i).Sum(d => d.TotalNpcs));
                playerList.Add(shieldingDoneGraph.Where(d => d.SecondsElapsed == i).Sum(d => d.TotalPlayers));
                instantValues.Add(shieldingDoneGraph.Where(d => d.SecondsElapsed == i).Sum(d => d.TotalPlayers));
                secondsList.Add(i.ToString(CultureInfo.InvariantCulture));
                playerDeathList.Add(playerDeaths.Count(d => d == i));
                npcDeathList.Add(npcDeaths.Count(d => d == i));
                // 5-second moving average
                if (i > 1 && i <= encounter.Duration.TotalSeconds - 2)
                {
                    // Able to calculate - use -2 to +2 seconds
                    fiveSecondAverageValues.Add((shieldingDoneGraph.Where(d => d.SecondsElapsed >= i - 2 && d.SecondsElapsed <= i + 2).Sum(d => d.TotalPlayers)) / 5);
                }
                else
                {
                    // Unable to calculate moving average, so add 0
                    fiveSecondAverageValues.Add(0);
                }
            }

            // Loop through instant values and calculate average
            long total = 0;
            for (var i = 0; i < instantValues.Count; i++)
            {
                total += instantValues[i];
                if (i == 0)
                {
                    averageValues.Add(0);
                }
                else
                {
                    averageValues.Add(total / i);
                }
            }

            //var totalArray = totalList.ToArray();
            var npcArray = npcList.ToArray();
            var playerArray = playerList.ToArray();
            var secondsArray = secondsList.ToArray();
            #endregion

            var model = new PlayerSomethingDone()
            {
                Encounter = encounter,
                Data = playerShieldingDone,
                AverageText = "Average APS",
                GraphType = "APS",
                PageTitle = "Absorption increased by Players",
                IsOutgoing = true,
                TotalText = "Total absorption",
                Session = encSession,
                ExportText = exportText,
                SplineGraph = PlayerNpcSomethingDoneOrTaken(encounter.Id, secondsArray, "Seconds elapsed", playerArray,
                "Deaths", averageValues.ToArray(), fiveSecondAverageValues.ToArray(), true, true, "APS", raidBuffTimers, playerDeathList, null)
            };
            swBuildTime.Stop();
            model.BuildTime = swBuildTime.Elapsed;

            return model;
        }

        public PlayerSomethingTaken PlayerShieldingTaken(Encounter encounter)
        {
            Stopwatch swBuildTime = new Stopwatch();
            swBuildTime.Start();

            #region Get Data
            var playerRoles = EncounterRoleList(encounter.Id);
            var shieldingTaken = _encounterRepository.GetOverviewPlayerShieldingTaken(encounter.Id);
            var playerShieldingTaken = UpdateRoleIcons(shieldingTaken, playerRoles);
            var shieldingTakenGraph = _encounterRepository.GetOverviewPlayerShieldingTakenGraph(encounter.Id);

            var playerDeaths = _encounterRepository.GetAllPlayerDeathTimers(encounter.Id);
            var encSession = _sessionEncounterRepository.GetSessionForEncounter(encounter.Id);
            #endregion

            if (shieldingTaken.Any())
            {
                // Loop through the totals and generate our progress bar percentages
                long topTotal = shieldingTaken.First().Total;
                for (int i = 0; i < shieldingTaken.Count; i++)
                {
                    var healRow = shieldingTaken[i];
                    healRow.ProgressBarPercentage = i == 0
                        ? "100%"
                        : ((decimal)healRow.Total / topTotal).ToString("#.##%");
                }
            }

            #region Export Text

            string exportText = string.Format("{0} {1}: {2}ATPS", encounter.Duration.ToString(@"mm\:ss"), encounter.BossFight.Name,
                shieldingTaken.Sum(h => h.Total) / (int)encounter.Duration.TotalSeconds);
            exportText = shieldingTaken
                .OrderByDescending(h => h.Total)
                .Aggregate(exportText, (current, shield) =>
                    current + string.Format(" | {0} {1}", shield.PlayerName, shield.Total / (int)encounter.Duration.TotalSeconds));

            #endregion
            var overallTotal = shieldingTaken.Sum(d => d.Total);
            var overallTotalNpcs = shieldingTaken.Sum(d => d.TotalFromNpcs);
            var overallTotalPlayers = shieldingTaken.Sum(d => d.TotalFromOtherPlayers);
            var overallTotalSelf = shieldingTaken.Sum(d => d.TotalFromSelf);
            shieldingTaken.Insert(0, new OverviewPlayerSomethingTaken()
            {
                PlayerId = -1,
                PlayerName = "All players",
                Total = overallTotal,
                TotalFromNpcs = overallTotalNpcs,
                TotalFromOtherPlayers = overallTotalPlayers,
                TotalFromSelf = overallTotalSelf
            });

            #region Create arrays for the chart
            //var totalList = new List<object>();
            var npcList = new List<object>();
            var playerList = new List<object>();
            var secondsList = new List<string>();

            var instantValues = new List<long>();
            var fiveSecondAverageValues = new List<object>();
            var averageValues = new List<object>();
            var playerDeathList = new List<object>();

            for (int i = 0; i <= encounter.Duration.TotalSeconds; i++)
            {
                npcList.Add(shieldingTakenGraph.Where(d => d.SecondsElapsed == i).Sum(d => d.TotalNpcs));
                playerList.Add(shieldingTakenGraph.Where(d => d.SecondsElapsed == i).Sum(d => d.TotalPlayers));
                instantValues.Add(shieldingTakenGraph.Where(d => d.SecondsElapsed == i).Sum(d => d.Total));
                secondsList.Add(i.ToString(CultureInfo.InvariantCulture));
                playerDeathList.Add(playerDeaths.Count(d => d == i));
                //npcDeathList.Add(npcDeaths.Count(d => d == i));
                // 5-second moving average
                if (i > 1 && i <= encounter.Duration.TotalSeconds - 2)
                {
                    // Able to calculate - use -2 to +2 seconds
                    fiveSecondAverageValues.Add((shieldingTakenGraph.Where(d => d.SecondsElapsed >= i - 2 && d.SecondsElapsed <= i + 2).Sum(d => d.TotalPlayers)) / 5);
                }
                else
                {
                    // Unable to calculate moving average, so add 0
                    fiveSecondAverageValues.Add(0);
                }
            }

            // Loop through instant values and calculate average
            long total = 0;
            for (var i = 0; i < instantValues.Count; i++)
            {
                total += instantValues[i];
                if (i == 0)
                {
                    averageValues.Add(0);
                }
                else
                {
                    averageValues.Add(total / i);
                }
            }

            var npcArray = npcList.ToArray();
            var playerArray = playerList.ToArray();
            var secondsArray = secondsList.ToArray();
            #endregion

            var model = new PlayerSomethingTaken()
            {
                Encounter = encounter,
                Data = playerShieldingTaken,
                AverageText = "Average APS",
                GraphType = "APS",
                PageTitle = "Absorption increased on Players",
                IsOutgoing = false,
                TotalText = "Total absorption",
                Session = encSession,
                ExportText = exportText,
                SplineGraph = PlayerNpcSomethingDoneOrTaken(encounter.Id, secondsArray, "Seconds elapsed", playerArray,
                "Deaths", averageValues.ToArray(), fiveSecondAverageValues.ToArray(), false, true, "APS", null, playerDeathList, null)
            };
            swBuildTime.Stop();
            model.BuildTime = swBuildTime.Elapsed;

            return model;
        }

        public NpcSomethingDone NpcDamageDone(Encounter encounter)
        {
            Stopwatch swBuildTime = new Stopwatch();
            swBuildTime.Start();

            #region Get Data
            var damageDone = _encounterRepository.GetOverviewNpcDamageDone(encounter.Id);
            var damageDoneGraph = _encounterRepository.GetOverviewNpcDamageDoneGraph(encounter.Id);
            var playerDeaths = _encounterRepository.GetAllPlayerDeathTimers(encounter.Id);
            var npcDeaths = _encounterRepository.GetAllNpcDeathTimers(encounter.Id);
            var encSession = _sessionEncounterRepository.GetSessionForEncounter(encounter.Id);
            #endregion

            if (damageDone.Any())
            {
                // Loop through the totals and generate our progress bar percentages
                long topToPlayers = damageDone.First().TotalToPlayers;
                for (int i = 0; i < damageDone.Count; i++)
                {
                    var dmgRow = damageDone[i];
                    dmgRow.ProgressBarPercentage = i == 0
                        ? "100%"
                        : ((decimal)dmgRow.TotalToPlayers / topToPlayers).ToString("#.##%");
                }
            }

            #region Export Text

            string exportText = string.Format("{0} {1}: {2}DPS", encounter.Duration.ToString(@"mm\:ss"), encounter.BossFight.Name,
                damageDone.Sum(d => d.Total) / (int)encounter.Duration.TotalSeconds);
            exportText = damageDone
                .OrderByDescending(d => d.Total)
                .Aggregate(exportText, (current, dmg) =>
                    current + string.Format(" | {0} {1}", dmg.NpcName, dmg.Total / (int)encounter.Duration.TotalSeconds));
            #endregion
            var overallTotal = damageDone.Sum(d => d.Total);
            var overallTotalNpcs = damageDone.Sum(d => d.TotalToOtherNpcs);
            var overallTotalPlayers = damageDone.Sum(d => d.TotalToPlayers);
            var overallTotalSelf = damageDone.Sum(d => d.TotalToSelf);
            damageDone.Insert(0, new OverviewNpcSomethingDone()
            {
                NpcId = "",
                NpcName = "All NPCs",
                Total = overallTotal,
                TotalToOtherNpcs = overallTotalNpcs,
                TotalToPlayers = overallTotalPlayers,
                TotalToSelf = overallTotalSelf
            });

            #region Create arrays for the chart
            //var totalList = new List<object>();
            var npcList = new List<object>();
            var playerList = new List<object>();
            var secondsList = new List<string>();

            var instantValues = new List<long>();
            var fiveSecondAverageValues = new List<object>();
            var averageValues = new List<object>();
            var playerDeathList = new List<object>();
            var npcDeathList = new List<object>();

            for (int i = 0; i <= encounter.Duration.TotalSeconds; i++)
            {
                npcList.Add(damageDoneGraph.Where(d => d.SecondsElapsed == i).Sum(d => d.TotalNpcs));
                playerList.Add(damageDoneGraph.Where(d => d.SecondsElapsed == i).Sum(d => d.TotalPlayers));
                instantValues.Add(damageDoneGraph.Where(d => d.SecondsElapsed == i).Sum(d => d.TotalPlayers));
                secondsList.Add(i.ToString(CultureInfo.InvariantCulture));
                playerDeathList.Add(playerDeaths.Count(d => d == i));
                npcDeathList.Add(npcDeaths.Count(d => d == i));
                // 5-second moving average
                if (i > 1 && i <= encounter.Duration.TotalSeconds - 2)
                {
                    // Able to calculate - use -2 to +2 seconds
                    fiveSecondAverageValues.Add((damageDoneGraph.Where(d => d.SecondsElapsed >= i - 2 && d.SecondsElapsed <= i + 2).Sum(d => d.TotalPlayers)) / 5);
                }
                else
                {
                    // Unable to calculate moving average, so add 0
                    fiveSecondAverageValues.Add(0);
                }
            }

            // Loop through instant values and calculate average
            long total = 0;
            for (var i = 0; i < instantValues.Count; i++)
            {
                total += instantValues[i];
                if (i == 0)
                {
                    averageValues.Add(0);
                }
                else
                {
                    averageValues.Add(total / i);
                }
            }

            //var totalArray = totalList.ToArray();
            var npcArray = npcList.ToArray();
            var playerArray = playerList.ToArray();
            var secondsArray = secondsList.ToArray();
            #endregion

            var model = new NpcSomethingDone()
            {
                Encounter = encounter,
                Data = damageDone,
                AverageText = "Average DPS",
                GraphType = "DPS",
                PageTitle = "Damage Done by NPCs",
                IsOutgoing = true,
                TotalText = "Total Damage",
                Session = encSession,
                ExportText = exportText,
                SplineGraph = PlayerNpcSomethingDoneOrTaken(encounter.Id, secondsArray, "Seconds elapsed", playerArray,
                "Deaths", averageValues.ToArray(), fiveSecondAverageValues.ToArray(), true, false, "DPS", null, playerDeathList, npcDeathList)
            };
            swBuildTime.Stop();
            model.BuildTime = swBuildTime.Elapsed;

            return model;
        }

        public NpcSomethingTaken NpcDamageTaken(Encounter encounter)
        {
            Stopwatch swBuildTime = new Stopwatch();
            swBuildTime.Start();

            #region Get Data
            var damageTaken = _encounterRepository.GetOverviewNpcDamageTaken(encounter.Id);
            var damageTakenGraph = _encounterRepository.GetOverviewNpcDamageTakenGraph(encounter.Id);
            var playerDeaths = _encounterRepository.GetAllPlayerDeathTimers(encounter.Id);
            var npcDeaths = _encounterRepository.GetAllNpcDeathTimers(encounter.Id);
            var encSession = _sessionEncounterRepository.GetSessionForEncounter(encounter.Id);
            #endregion

            if (damageTaken.Any())
            {
                // Loop through the totals and generate our progress bar percentages
                long topFromPlayers = damageTaken.First().TotalFromPlayers;
                for (int i = 0; i < damageTaken.Count; i++)
                {
                    var dmgRow = damageTaken[i];
                    dmgRow.ProgressBarPercentage = i == 0
                        ? "100%"
                        : ((decimal)dmgRow.TotalFromPlayers / topFromPlayers).ToString("#.##%");
                }
            }

            #region Export Text

            string exportText = string.Format("{0} {1}: {2}DTPS", encounter.Duration.ToString(@"mm\:ss"), encounter.BossFight.Name,
                damageTaken.Sum(d => d.Total) / (int)encounter.Duration.TotalSeconds);
            exportText = damageTaken
                .OrderByDescending(d => d.Total)
                .Aggregate(exportText, (current, dmg) =>
                    current + string.Format(" | {0} {1}", dmg.NpcName, dmg.Total / (int)encounter.Duration.TotalSeconds));
            #endregion
            var overallTotal = damageTaken.Sum(d => d.Total);
            var overallTotalNpcs = damageTaken.Sum(d => d.TotalFromOtherNpcs);
            var overallTotalPlayers = damageTaken.Sum(d => d.TotalFromPlayers);
            var overallTotalSelf = damageTaken.Sum(d => d.TotalFromSelf);
            damageTaken.Insert(0, new OverviewNpcSomethingTaken()
            {
                NpcId = "",
                NpcName = "All NPCs",
                Total = overallTotal,
                TotalFromOtherNpcs = overallTotalNpcs,
                TotalFromPlayers = overallTotalPlayers,
                TotalFromSelf = overallTotalSelf
            });

            #region Create arrays for the chart
            //var totalList = new List<object>();
            var npcList = new List<object>();
            var playerList = new List<object>();
            var secondsList = new List<string>();

            var instantValues = new List<long>();
            var fiveSecondAverageValues = new List<object>();
            var averageValues = new List<object>();
            var playerDeathList = new List<object>();
            var npcDeathList = new List<object>();

            for (int i = 0; i <= encounter.Duration.TotalSeconds; i++)
            {
                npcList.Add(damageTakenGraph.Where(d => d.SecondsElapsed == i).Sum(d => d.TotalNpcs));
                playerList.Add(damageTakenGraph.Where(d => d.SecondsElapsed == i).Sum(d => d.TotalPlayers));
                instantValues.Add(damageTakenGraph.Where(d => d.SecondsElapsed == i).Sum(d => d.TotalPlayers));
                secondsList.Add(i.ToString(CultureInfo.InvariantCulture));
                playerDeathList.Add(playerDeaths.Count(d => d == i));
                npcDeathList.Add(npcDeaths.Count(d => d == i));
                // 5-second moving average
                if (i > 1 && i <= encounter.Duration.TotalSeconds - 2)
                {
                    // Able to calculate - use -2 to +2 seconds
                    fiveSecondAverageValues.Add((damageTakenGraph.Where(d => d.SecondsElapsed >= i - 2 && d.SecondsElapsed <= i + 2).Sum(d => d.TotalPlayers)) / 5);
                }
                else
                {
                    // Unable to calculate moving average, so add 0
                    fiveSecondAverageValues.Add(0);
                }
            }

            // Loop through instant values and calculate average
            long total = 0;
            for (var i = 0; i < instantValues.Count; i++)
            {
                total += instantValues[i];
                if (i == 0)
                {
                    averageValues.Add(0);
                }
                else
                {
                    averageValues.Add(total / i);
                }
            }

            //var totalArray = totalList.ToArray();
            var npcArray = npcList.ToArray();
            var playerArray = playerList.ToArray();
            var secondsArray = secondsList.ToArray();
            #endregion

            var model = new NpcSomethingTaken()
            {
                Encounter = encounter,
                Data = damageTaken,
                AverageText = "Average DPS",
                GraphType = "DPS",
                PageTitle = "Damage taken by NPCs",
                IsOutgoing = false,
                TotalText = "Total Damage",
                Session = encSession,
                ExportText = exportText,
                SplineGraph = PlayerNpcSomethingDoneOrTaken(encounter.Id, secondsArray, "Seconds elapsed", playerArray,
                "Deaths", averageValues.ToArray(), fiveSecondAverageValues.ToArray(), false, false, "DPS", null, playerDeathList, npcDeathList)
            };
            swBuildTime.Stop();
            model.BuildTime = swBuildTime.Elapsed;

            return model;
        }

        public NpcSomethingDone NpcHealingDone(Encounter encounter)
        {
            Stopwatch swBuildTime = new Stopwatch();
            swBuildTime.Start();

            #region Get Data
            var healingDone = _encounterRepository.GetOverviewNpcHealingDone(encounter.Id);
            var healingDoneGraph = _encounterRepository.GetOverviewNpcHealingDoneGraph(encounter.Id);
            var npcDeaths = _encounterRepository.GetAllNpcDeathTimers(encounter.Id);
            var encSession = _sessionEncounterRepository.GetSessionForEncounter(encounter.Id);
            #endregion

            if (healingDone.Any())
            {
                long topTotal = healingDone.First().Total;
                for (int i = 0; i < healingDone.Count; i++)
                {
                    var healRow = healingDone[i];
                    healRow.ProgressBarPercentage = i == 0
                        ? "100%"
                        : ((decimal)healRow.Total / topTotal).ToString("#.##%");
                }
            }

            #region Export Text

            string exportText = string.Format("{0} {1}: {2}HPS", encounter.Duration.ToString(@"mm\:ss"), encounter.BossFight.Name,
                healingDone.Sum(d => d.Total) / (int)encounter.Duration.TotalSeconds);
            exportText = healingDone
                .OrderByDescending(d => d.Total)
                .Aggregate(exportText, (current, heal) =>
                    current + string.Format(" | {0} {1}", heal.NpcName, heal.Total / (int)encounter.Duration.TotalSeconds));

            #endregion
            var overallTotal = healingDone.Sum(d => d.Total);
            var overallTotalNpcs = healingDone.Sum(d => d.TotalToOtherNpcs);
            var overallTotalPlayers = healingDone.Sum(d => d.TotalToPlayers);
            var overallTotalSelf = healingDone.Sum(d => d.TotalToSelf);
            healingDone.Insert(0, new OverviewNpcSomethingDone()
            {
                NpcId = "",
                NpcName = "All NPCs",
                Total = overallTotal,
                TotalToOtherNpcs = overallTotalNpcs,
                TotalToPlayers = overallTotalPlayers,
                TotalToSelf = overallTotalSelf
            });

            #region Create arrays for the chart
            //var totalList = new List<object>();
            var npcList = new List<object>();
            var playerList = new List<object>();
            var secondsList = new List<string>();

            var instantValues = new List<long>();
            var fiveSecondAverageValues = new List<object>();
            var averageValues = new List<object>();
            var npcDeathList = new List<object>();

            for (int i = 0; i <= encounter.Duration.TotalSeconds; i++)
            {
                npcList.Add(healingDoneGraph.Where(d => d.SecondsElapsed == i).Sum(d => d.TotalNpcs));
                playerList.Add(healingDoneGraph.Where(d => d.SecondsElapsed == i).Sum(d => d.TotalPlayers));
                instantValues.Add(healingDoneGraph.Where(d => d.SecondsElapsed == i).Sum(d => d.Total));
                secondsList.Add(i.ToString(CultureInfo.InvariantCulture));
                npcDeathList.Add(npcDeaths.Count(d => d == i));
                // 5-second moving average
                if (i > 1 && i <= encounter.Duration.TotalSeconds - 2)
                {
                    // Able to calculate - use -2 to +2 seconds
                    fiveSecondAverageValues.Add((healingDoneGraph.Where(d => d.SecondsElapsed >= i - 2 && d.SecondsElapsed <= i + 2).Sum(d => d.Total)) / 5);
                }
                else
                {
                    // Unable to calculate moving average, so add 0
                    fiveSecondAverageValues.Add(0);
                }
            }

            // Loop through instant values and calculate average
            long total = 0;
            for (var i = 0; i < instantValues.Count; i++)
            {
                total += instantValues[i];
                if (i == 0)
                {
                    averageValues.Add(0);
                }
                else
                {
                    averageValues.Add(total / i);
                }
            }

            //var totalArray = totalList.ToArray();
            var npcArray = npcList.ToArray();
            var playerArray = playerList.ToArray();
            var secondsArray = secondsList.ToArray();
            #endregion

            var model = new NpcSomethingDone()
            {
                Encounter = encounter,
                Data = healingDone,
                AverageText = "Average HPS",
                GraphType = "HPS",
                PageTitle = "NPC Healing Done",
                IsOutgoing = true,
                TotalText = "Total Healing",
                Session = encSession,
                ExportText = exportText,
                SplineGraph = PlayerNpcSomethingDoneOrTaken(encounter.Id, secondsArray, "Seconds elapsed", npcArray,
                "Deaths", averageValues.ToArray(), fiveSecondAverageValues.ToArray(), true, false, "HPS", null, null, npcDeathList)
            };
            swBuildTime.Stop();
            model.BuildTime = swBuildTime.Elapsed;

            return model;
        }

        public NpcSomethingTaken NpcHealingTaken(Encounter encounter)
        {
            Stopwatch swBuildTime = new Stopwatch();
            swBuildTime.Start();

            #region Get Data
            var healingTaken = _encounterRepository.GetOverviewNpcHealingTaken(encounter.Id);
            var healingTakenGraph = _encounterRepository.GetOverviewNpcHealingTakenGraph(encounter.Id);
            var npcDeaths = _encounterRepository.GetAllNpcDeathTimers(encounter.Id);
            var encSession = _sessionEncounterRepository.GetSessionForEncounter(encounter.Id);
            #endregion

            if (healingTaken.Any())
            {
                long topTotal = healingTaken.First().Total;
                for (int i = 0; i < healingTaken.Count; i++)
                {
                    var healRow = healingTaken[i];
                    healRow.ProgressBarPercentage = i == 0
                        ? "100%"
                        : ((decimal)healRow.Total / topTotal).ToString("#.##%");
                }
            }

            #region Export Text

            string exportText = string.Format("{0} {1}: {2}HTPS", encounter.Duration.ToString(@"mm\:ss"), encounter.BossFight.Name,
                healingTaken.Sum(d => d.Total) / (int)encounter.Duration.TotalSeconds);
            exportText = healingTaken
                .OrderByDescending(d => d.Total)
                .Aggregate(exportText, (current, heal) =>
                    current + string.Format(" | {0} {1}", heal.NpcName, heal.Total / (int)encounter.Duration.TotalSeconds));
            #endregion
            var overallTotal = healingTaken.Sum(d => d.Total);
            var overallTotalNpcs = healingTaken.Sum(d => d.TotalFromOtherNpcs);
            var overallTotalPlayers = healingTaken.Sum(d => d.TotalFromPlayers);
            var overallTotalSelf = healingTaken.Sum(d => d.TotalFromSelf);
            healingTaken.Insert(0, new OverviewNpcSomethingTaken()
            {
                NpcId = "",
                NpcName = "All NPCs",
                Total = overallTotal,
                TotalFromOtherNpcs = overallTotalNpcs,
                TotalFromPlayers = overallTotalPlayers,
                TotalFromSelf = overallTotalSelf
            });

            #region Create arrays for the chart
            //var totalList = new List<object>();
            var npcList = new List<object>();
            var playerList = new List<object>();
            var secondsList = new List<string>();

            var instantValues = new List<long>();
            var fiveSecondAverageValues = new List<object>();
            var averageValues = new List<object>();
            var npcDeathList = new List<object>();

            for (int i = 0; i <= encounter.Duration.TotalSeconds; i++)
            {
                npcList.Add(healingTakenGraph.Where(d => d.SecondsElapsed == i).Sum(d => d.TotalNpcs));
                playerList.Add(healingTakenGraph.Where(d => d.SecondsElapsed == i).Sum(d => d.TotalPlayers));
                instantValues.Add(healingTakenGraph.Where(d => d.SecondsElapsed == i).Sum(d => d.Total));
                secondsList.Add(i.ToString(CultureInfo.InvariantCulture));
                npcDeathList.Add(npcDeaths.Count(d => d == i));
                // 5-second moving average
                if (i > 1 && i <= encounter.Duration.TotalSeconds - 2)
                {
                    // Able to calculate - use -2 to +2 seconds
                    fiveSecondAverageValues.Add((healingTakenGraph.Where(d => d.SecondsElapsed >= i - 2 && d.SecondsElapsed <= i + 2).Sum(d => d.Total)) / 5);
                }
                else
                {
                    // Unable to calculate moving average, so add 0
                    fiveSecondAverageValues.Add(0);
                }
            }

            // Loop through instant values and calculate average
            long total = 0;
            for (var i = 0; i < instantValues.Count; i++)
            {
                total += instantValues[i];
                if (i == 0)
                {
                    averageValues.Add(0);
                }
                else
                {
                    averageValues.Add(total / i);
                }
            }

            //var totalArray = totalList.ToArray();
            var npcArray = npcList.ToArray();
            var playerArray = playerList.ToArray();
            var secondsArray = secondsList.ToArray();
            #endregion

            var model = new NpcSomethingTaken()
            {
                Encounter = encounter,
                Data = healingTaken,
                AverageText = "Average HPS",
                GraphType = "HPS",
                PageTitle = "NPC Healing Taken",
                IsOutgoing = false,
                TotalText = "Total Healing",
                Session = encSession,
                ExportText = exportText,
                SplineGraph = PlayerNpcSomethingDoneOrTaken(encounter.Id, secondsArray, "Seconds elapsed", npcArray,
                "Deaths", averageValues.ToArray(), fiveSecondAverageValues.ToArray(), false, false, "HPS", null, null, npcDeathList)
            };
            swBuildTime.Stop();
            model.BuildTime = swBuildTime.Elapsed;

            return model;
        }

        public NpcSomethingDone NpcShieldingDone(Encounter encounter)
        {
            Stopwatch swBuildTime = new Stopwatch();
            swBuildTime.Start();

            #region Get Data
            var shieldingDone = _encounterRepository.GetOverviewNpcShieldingDone(encounter.Id);
            var shieldingDoneGraph = _encounterRepository.GetOverviewNpcShieldingDoneGraph(encounter.Id);
            var npcDeaths = _encounterRepository.GetAllNpcDeathTimers(encounter.Id);
            var encSession = _sessionEncounterRepository.GetSessionForEncounter(encounter.Id);
            #endregion

            if (shieldingDone.Any())
            {
                // Loop through the totals and generate our progress bar percentages
                long topTotal = shieldingDone.First().Total;
                for (int i = 0; i < shieldingDone.Count; i++)
                {
                    var shieldRow = shieldingDone[i];
                    shieldRow.ProgressBarPercentage = i == 0
                        ? "100%"
                        : ((decimal)shieldRow.Total / topTotal).ToString("#.##%");
                }
            }

            #region Export Text

            string exportText = string.Format("{0} {1}: {2}APS", encounter.Duration.ToString(@"mm\:ss"), encounter.BossFight.Name,
                shieldingDone.Sum(d => d.Total) / (int)encounter.Duration.TotalSeconds);
            exportText = shieldingDone
                .OrderByDescending(d => d.Total)
                .Aggregate(exportText, (current, shield) =>
                    current + string.Format(" | {0} {1}", shield.NpcName, shield.Total / (int)encounter.Duration.TotalSeconds));
            #endregion
            var overallTotal = shieldingDone.Sum(d => d.Total);
            var overallTotalNpcs = shieldingDone.Sum(d => d.TotalToOtherNpcs);
            var overallTotalPlayers = shieldingDone.Sum(d => d.TotalToPlayers);
            var overallTotalSelf = shieldingDone.Sum(d => d.TotalToSelf);
            shieldingDone.Insert(0, new OverviewNpcSomethingDone()
            {
                NpcId = "",
                NpcName = "All NPCs",
                Total = overallTotal,
                TotalToOtherNpcs = overallTotalNpcs,
                TotalToPlayers = overallTotalPlayers,
                TotalToSelf = overallTotalSelf
            });

            #region Create arrays for the chart
            //var totalList = new List<object>();
            var npcList = new List<object>();
            var playerList = new List<object>();
            var secondsList = new List<string>();

            var instantValues = new List<long>();
            var fiveSecondAverageValues = new List<object>();
            var averageValues = new List<object>();
            var npcDeathList = new List<object>();

            for (int i = 0; i <= encounter.Duration.TotalSeconds; i++)
            {
                npcList.Add(shieldingDoneGraph.Where(d => d.SecondsElapsed == i).Sum(d => d.TotalNpcs));
                playerList.Add(shieldingDoneGraph.Where(d => d.SecondsElapsed == i).Sum(d => d.TotalPlayers));
                instantValues.Add(shieldingDoneGraph.Where(d => d.SecondsElapsed == i).Sum(d => d.Total));
                secondsList.Add(i.ToString(CultureInfo.InvariantCulture));
                npcDeathList.Add(npcDeaths.Count(d => d == i));
                // 5-second moving average
                if (i > 1 && i <= encounter.Duration.TotalSeconds - 2)
                {
                    // Able to calculate - use -2 to +2 seconds
                    fiveSecondAverageValues.Add((shieldingDoneGraph.Where(d => d.SecondsElapsed >= i - 2 && d.SecondsElapsed <= i + 2).Sum(d => d.Total)) / 5);
                }
                else
                {
                    // Unable to calculate moving average, so add 0
                    fiveSecondAverageValues.Add(0);
                }
            }

            // Loop through instant values and calculate average
            long total = 0;
            for (var i = 0; i < instantValues.Count; i++)
            {
                total += instantValues[i];
                if (i == 0)
                {
                    averageValues.Add(0);
                }
                else
                {
                    averageValues.Add(total / i);
                }
            }

            //var totalArray = totalList.ToArray();
            var npcArray = npcList.ToArray();
            var playerArray = playerList.ToArray();
            var secondsArray = secondsList.ToArray();
            #endregion

            var model = new NpcSomethingDone()
            {
                Encounter = encounter,
                Data = shieldingDone,
                AverageText = "Average APS",
                GraphType = "APS",
                PageTitle = "Absorption increased by NPCs",
                IsOutgoing = true,
                TotalText = "Total Absorption",
                Session = encSession,
                ExportText = exportText,
                SplineGraph = PlayerNpcSomethingDoneOrTaken(encounter.Id, secondsArray, "Seconds elapsed", npcArray,
                "Deaths", averageValues.ToArray(), fiveSecondAverageValues.ToArray(), true, false, "APS", null, null, npcDeathList)
            };
            swBuildTime.Stop();
            model.BuildTime = swBuildTime.Elapsed;

            return model;
        }

        public NpcSomethingTaken NpcShieldingTaken(Encounter encounter)
        {
            Stopwatch swBuildTime = new Stopwatch();
            swBuildTime.Start();

            #region Get Data
            var shieldingTaken = _encounterRepository.GetOverviewNpcShieldingTaken(encounter.Id);
            var shieldingTakenGraph = _encounterRepository.GetOverviewNpcShieldingTakenGraph(encounter.Id);
            var npcDeaths = _encounterRepository.GetAllNpcDeathTimers(encounter.Id);
            var encSession = _sessionEncounterRepository.GetSessionForEncounter(encounter.Id);
            #endregion

            if (shieldingTaken.Any())
            {
                long topTotal = shieldingTaken.First().Total;
                for (int i = 0; i < shieldingTaken.Count; i++)
                {
                    var shieldRow = shieldingTaken[i];
                    shieldRow.ProgressBarPercentage = i == 0
                        ? "100%"
                        : ((decimal)shieldRow.Total / topTotal).ToString("#.##%");
                }
            }

            #region Export Text

            string exportText = string.Format("{0} {1}: {2}ATPS", encounter.Duration.ToString(@"mm\:ss"), encounter.BossFight.Name,
                shieldingTaken.Sum(d => d.Total) / (int)encounter.Duration.TotalSeconds);
            exportText = shieldingTaken
                .OrderByDescending(d => d.Total)
                .Aggregate(exportText, (current, shield) =>
                    current + string.Format(" | {0} {1}", shield.NpcName, shield.Total / (int)encounter.Duration.TotalSeconds));
            #endregion
            var overallTotal = shieldingTaken.Sum(d => d.Total);
            var overallTotalNpcs = shieldingTaken.Sum(d => d.TotalFromOtherNpcs);
            var overallTotalPlayers = shieldingTaken.Sum(d => d.TotalFromPlayers);
            var overallTotalSelf = shieldingTaken.Sum(d => d.TotalFromSelf);
            shieldingTaken.Insert(0, new OverviewNpcSomethingTaken()
            {
                NpcId = "",
                NpcName = "All NPCs",
                Total = overallTotal,
                TotalFromOtherNpcs = overallTotalNpcs,
                TotalFromPlayers = overallTotalPlayers,
                TotalFromSelf = overallTotalSelf
            });

            #region Create arrays for the chart
            //var totalList = new List<object>();
            var npcList = new List<object>();
            var playerList = new List<object>();
            var secondsList = new List<string>();

            var instantValues = new List<long>();
            var fiveSecondAverageValues = new List<object>();
            var averageValues = new List<object>();
            var npcDeathList = new List<object>();

            for (int i = 0; i <= encounter.Duration.TotalSeconds; i++)
            {
                npcList.Add(shieldingTakenGraph.Where(d => d.SecondsElapsed == i).Sum(d => d.TotalNpcs));
                playerList.Add(shieldingTakenGraph.Where(d => d.SecondsElapsed == i).Sum(d => d.TotalPlayers));
                instantValues.Add(shieldingTakenGraph.Where(d => d.SecondsElapsed == i).Sum(d => d.Total));
                secondsList.Add(i.ToString(CultureInfo.InvariantCulture));
                npcDeathList.Add(npcDeaths.Count(d => d == i));
                // 5-second moving average
                if (i > 1 && i <= encounter.Duration.TotalSeconds - 2)
                {
                    // Able to calculate - use -2 to +2 seconds
                    fiveSecondAverageValues.Add((shieldingTakenGraph.Where(d => d.SecondsElapsed >= i - 2 && d.SecondsElapsed <= i + 2).Sum(d => d.Total)) / 5);
                }
                else
                {
                    // Unable to calculate moving average, so add 0
                    fiveSecondAverageValues.Add(0);
                }
            }

            // Loop through instant values and calculate average
            long total = 0;
            for (var i = 0; i < instantValues.Count; i++)
            {
                total += instantValues[i];
                if (i == 0)
                {
                    averageValues.Add(0);
                }
                else
                {
                    averageValues.Add(total / i);
                }
            }

            //var totalArray = totalList.ToArray();
            var npcArray = npcList.ToArray();
            var playerArray = playerList.ToArray();
            var secondsArray = secondsList.ToArray();
            #endregion

            var model = new NpcSomethingTaken()
            {
                Encounter = encounter,
                Data = shieldingTaken,
                AverageText = "Average APS",
                GraphType = "APS",
                PageTitle = "Absorption increased by NPCs",
                IsOutgoing = false,
                TotalText = "Total Absorption",
                Session = encSession,
                ExportText = exportText,
                SplineGraph = PlayerNpcSomethingDoneOrTaken(encounter.Id, secondsArray, "Seconds elapsed", npcArray,
                "Deaths", averageValues.ToArray(), fiveSecondAverageValues.ToArray(), false, false, "APS", null, null, npcDeathList)
            };
            swBuildTime.Stop();
            model.BuildTime = swBuildTime.Elapsed;

            return model;
        }

        public Highcharts PlayerNpcSomethingDoneOrTaken(int encounterId, string[] xAxisArray, string xAxisTitle, object[] yAxis1Array,
            string yAxis2Title, object[] averageArray, object[] movingAverageArray = null, bool outgoing = true,
            bool isPlayer = true, string actionType = "DPS", List<CharacterBuffAction> buffTimers = null,
            List<object> playerDeathList = null, List<object> npcDeathList = null)
        {
            //string graphTitle = "Player damage done (per second)";
            string graphTitle = isPlayer ? "Player" : "NPC";
            string yAxis1Title = "";
            switch (actionType)
            {
                case "APS":
                    graphTitle += " absorption increased";
                    yAxis1Title = "Absorption";
                    break;
                case "HPS":
                    graphTitle += outgoing ? " healing done" : " healing taken";
                    yAxis1Title = "Healing";
                    break;
                default:
                    graphTitle += outgoing ? " damage done" : " damage taken";
                    yAxis1Title = "Damage";
                    break;
            }

            graphTitle += " (per second)";
            yAxis1Title += outgoing ? " done" : " taken";

            var seriesList = new List<Series>();

            var series1 = new Series
            {
                Data = new Data(yAxis1Array),
                Name = yAxis1Title,
                PlotOptionsSeries = new PlotOptionsSeries() { Color = Color.FromArgb(255, 124, 181, 236), Marker = new PlotOptionsSeriesMarker() { Enabled = false } }
                //PlotOptionsSeries = new PlotOptionsSeries() { Color = ColorTranslator.FromHtml("#c0c0c0") }
            };

            seriesList.Add(series1);

            // Average series
            if (averageArray != null)
            {
                var avgSeries = new Series
                {
                    Name = string.Format("Overall average {0}", yAxis1Title.ToLower()),
                    Data = new Data(averageArray),
                    Type = ChartTypes.Line,
                    YAxis = "0",
                    PlotOptionsLine = new PlotOptionsLine()
                    {
                        Marker = new PlotOptionsLineMarker()
                        {
                            LineWidth = 1,
                            Enabled = false
                        },
                        //Color = Color.FromArgb(255, 188, 128, 189)
                    }
                };
                seriesList.Add(avgSeries);
            }

            // Moving average series
            if (movingAverageArray != null)
            {
                var movingAvgSeries = new Series
                {
                    Name = string.Format("Average {0} (5s)", yAxis1Title.ToLower()),
                    Data = new Data(movingAverageArray),
                    Type = ChartTypes.Line,
                    YAxis = "0",
                    PlotOptionsLine = new PlotOptionsLine()
                    {
                        Marker = new PlotOptionsLineMarker()
                        {
                            LineWidth = 1,
                            Enabled = false
                        },
                        //Color = Color.FromArgb(255, 255, 160, 160)
                    }
                };
                seriesList.Add(movingAvgSeries);
            }

            var plotBands = new List<XAxisPlotBands>();

            if (buffTimers != null)
            {
                plotBands.AddRange(buffTimers.Select(timer => new XAxisPlotBands()
                {
                    From = timer.SecondBuffWentUp,
                    To = timer.SecondBuffWentDown,
                    Label = new XAxisPlotBandsLabel()
                    {
                        Text = timer.BuffName,
                        Style = "color: '#000000'"
                    },
                    Color = Color.LightGray
                }));
            }

            var yAxes = new List<YAxis>();
            // Standard Y
            yAxes.Add(
                new YAxis
                {
                    Title = new YAxisTitle { Text = yAxis1Title, Style = ChartColors.WhiteTextStyle },
                    Min = 0,
                    TickColor = Color.White,
                    LineColor = Color.White,
                    Labels = new YAxisLabels { Style = ChartColors.WhiteTextStyle },
                });
            // Deaths
            if (!string.IsNullOrEmpty(yAxis2Title))
            {
                yAxes.Add(
                new YAxis
                {
                    Title = new YAxisTitle { Text = yAxis2Title, Style = ChartColors.WhiteTextStyle },
                    Opposite = true,
                    MinTickInterval = 1,
                    Min = 0,
                    TickColor = Color.White,
                    LineColor = Color.White,
                    Labels = new YAxisLabels { Style = ChartColors.WhiteTextStyle }
                });

                if (playerDeathList != null)
                {
                    var playerDeathSeries =
                        new Series()
                        {
                            Name = "Player Deaths",
                            Data = new Data(playerDeathList.ToArray()),
                            Type = ChartTypes.Column,
                            PlotOptionsColumn = new PlotOptionsColumn() { BorderWidth = 0, Visible = isPlayer },
                            Color = Color.DarkOrange,
                            YAxis = "1"
                        };
                    seriesList.Add(playerDeathSeries);
                }
                if (npcDeathList != null)
                {
                    var npcDeathSeries =
                        new Series()
                        {
                            Name = "NPC Deaths",
                            Data = new Data(npcDeathList.ToArray()),
                            Type = ChartTypes.Column,
                            PlotOptionsColumn = new PlotOptionsColumn() { BorderWidth = 0, Visible = !isPlayer },
                            Color = Color.DarkRed,
                            YAxis = "1"
                        };
                    seriesList.Add(npcDeathSeries);
                }
            }


            var chart = new Highcharts(string.Format("e{0}pnsdt", encounterId))
            .InitChart(new Chart
            {
                DefaultSeriesType = ChartTypes.Spline,
                ZoomType = ZoomTypes.Xy,
                Height = 400,
                BackgroundColor = new BackColorOrGradient(new Gradient
                {
                    LinearGradient = new[] { 0, 0, 0, 400 },
                    Stops = new object[,]
                        {{ 0, Color.FromArgb(13, 255, 255, 255) },
                        { 1, Color.FromArgb(13, 255, 255, 255) }}
                }),
                Style = ChartColors.WhiteTextStyle
            })
                .SetCredits(ChartDefaults.Credits)
            .SetOptions(new GlobalOptions
            {
                Colors = ChartColors.ColorArrayBlackBg(),
                Global = new Global { UseUTC = false }
            })
            .SetTitle(new Title
            {
                Text = graphTitle,
                Style = ChartColors.WhiteTextStyle
            })
            .SetXAxis(new XAxis
            {
                LineColor = Color.White,
                TickColor = Color.White,
                Labels = new XAxisLabels { Style = ChartColors.WhiteTextStyle },
                PlotBands = plotBands.ToArray()
            })
            .SetYAxis(yAxes.ToArray())
            .SetTooltip(new Tooltip() { ValueSuffix = " per second" })
            .SetSeries(seriesList.ToArray())
                .SetExporting(new Exporting { Enabled = false })
                .SetLegend(new Legend() { ItemStyle = ChartColors.WhiteTextStyle, ItemHoverStyle = "color: '#bbb'" })
                ;
            return chart;
        }

        public Highcharts OverviewChart(string graphTitle, string graphSubtitle, List<CharacterInteractionPerSecond> records, int totalSeconds, bool isHealingGraph,
            bool groupByAbility, bool outgoing)
        {
            var secondsElapsed = new List<string>();
            var instantValues = new List<long>();
            var averageValues = new List<object>();
            var fiveSecondAverageValues = new List<object>();

            var instantValueSeries = new List<Series>();

            #region Loop and calculate averages
            if (groupByAbility)
            {
                var instantValueDictionary = new Dictionary<string, List<object>>();
                var abilityNames = records.Select(r => r.AbilityName).OrderBy(r => r).Distinct().ToList();
                instantValueDictionary = abilityNames.ToDictionary(s => s, s => new List<object>());

                #region Ability Loop
                Stopwatch sw = new Stopwatch();
                sw.Start();
                for (int i = 0; i < totalSeconds; i++)
                {
                    var thisSecond = i;
                    secondsElapsed.Add(i.ToString(CultureInfo.InvariantCulture));
                    // Loop through abilities and check them all for events that occurred "this second"

                    if (isHealingGraph)
                    {
                        foreach (var ability in abilityNames)
                        {
                            var abilityValuesThisSecond =
                                records.FirstOrDefault(
                                    r => r.SecondsElapsed == thisSecond && r.AbilityName == ability);

                            instantValueDictionary[ability].Add(abilityValuesThisSecond == null
                                ? 0
                                : abilityValuesThisSecond.Effective);
                        }
                    }
                    else
                    {
                        foreach (var ability in abilityNames)
                        {
                            var abilityValuesThisSecond =
                                records.FirstOrDefault(
                                    r => r.SecondsElapsed == thisSecond && r.AbilityName == ability);

                            instantValueDictionary[ability].Add(abilityValuesThisSecond == null
                                ? 0
                                : abilityValuesThisSecond.Total);

                        }
                    }

                    // Get the total of abilities that hit "this second"
                    instantValues.Add(isHealingGraph
                        ? records.Where(r => r.SecondsElapsed == thisSecond).Sum(r => r.Effective)
                        : records.Where(r => r.SecondsElapsed == thisSecond).Sum(r => r.Total));

                    // 5-second moving average
                    if (i >= 2 && i <= totalSeconds - 2)
                    {
                        // Able to calculate - use -2 to +2 seconds
                        if (isHealingGraph)
                        {
                            fiveSecondAverageValues.Add((records.Where(d => d.SecondsElapsed >= i - 2 && d.SecondsElapsed <= i + 2).Sum(d => d.Effective)) / 5);
                        }
                        else
                        {
                            fiveSecondAverageValues.Add((records.Where(d => d.SecondsElapsed >= i - 2 && d.SecondsElapsed <= i + 2).Sum(d => d.Total)) / 5);
                        }

                    }
                    else
                    {
                        // Unable to calculate moving average, so add 0
                        fiveSecondAverageValues.Add(0);
                    }
                }
                sw.Stop();
                //_logger.Debug(string.Format("Second loop finished in {0}", sw.Elapsed));
                #endregion

                //instantValueSeries = instantValueDictionary.Select(kvp => new Series()
                //{
                //    Name = kvp.Key.Replace("'", "\\\'"),
                //    Data = new Data(kvp.Value.ToArray())
                //}).ToList();

                foreach (var instantValue in instantValueDictionary.OrderBy(v => v.Key))
                {
                    instantValueSeries.Add(new Series()
                    {
                        Name = instantValue.Key.Replace("'", "\\\'"),
                        Data = new Data(instantValue.Value.ToArray()),
                        Type = ChartTypes.Spline,
                        PlotOptionsSpline = new PlotOptionsSpline()
                        {
                            Visible = false,
                            Marker = new PlotOptionsSplineMarker() { Enabled = false }
                        }
                    });
                }
            }
            else // group by source or target
            {
                var characters = new List<InteractionByCharacter>();
                if (outgoing)
                {
                    characters.AddRange(records.Where(r =>
                        !string.IsNullOrEmpty(r.TargetName) &&
                        !string.IsNullOrEmpty(r.TargetId) &&
                        !characters.Any(c => c.CharacterId == r.TargetId))
                        .Select(record => new InteractionByCharacter() { CharacterId = record.TargetId, CharacterName = record.TargetName }));
                }
                else
                {
                    characters.AddRange(records.Where(r =>
                        !string.IsNullOrEmpty(r.SourceName) &&
                        !string.IsNullOrEmpty(r.SourceId) &&
                        !characters.Any(c => c.CharacterId == r.SourceId))
                        .Select(record => new InteractionByCharacter() { CharacterId = record.SourceId, CharacterName = record.SourceName }));
                }

                var instantValueDictionary = new Dictionary<InteractionByCharacter, List<object>>();

                #region Source / Target Loop
                for (int i = 0; i < totalSeconds; i++)
                {
                    int thisSecond = i;
                    secondsElapsed.Add(i.ToString(CultureInfo.InvariantCulture));
                    // Loop through abilities and check them all for events that occurred "this second"
                    #region Per-character
                    foreach (var character in characters)
                    {
                        var characterValuesThisSecond = outgoing
                            ? records.FirstOrDefault
                                (r => r.SecondsElapsed == thisSecond && r.TargetName == character.CharacterName && r.TargetId == character.CharacterId)
                            : records.FirstOrDefault
                                (r => r.SecondsElapsed == thisSecond && r.SourceName == character.CharacterName && r.SourceId == character.CharacterId);
                        long value = characterValuesThisSecond == null
                                ? 0 : isHealingGraph ? characterValuesThisSecond.Effective : characterValuesThisSecond.Total;

                        if (!instantValueDictionary.Any(
                                    c =>
                                        c.Key.CharacterId == character.CharacterId &&
                                        c.Key.CharacterName == character.CharacterName))
                        {
                            instantValueDictionary.Add(new InteractionByCharacter()
                            {
                                CharacterId = character.CharacterId,
                                CharacterName = character.CharacterName
                            }, new List<object>() { value });
                        }
                        else
                        {
                            instantValueDictionary.First(c => c.Key.CharacterId == character.CharacterId).Value.Add(value);
                        }
                    }
                    #endregion

                    // Get the total of abilities that hit "this second"
                    instantValues.Add(isHealingGraph
                        ? records.Where(r => r.SecondsElapsed == thisSecond).Sum(r => r.Effective)
                        : records.Where(r => r.SecondsElapsed == thisSecond).Sum(r => r.Total));

                    // 5-second moving average
                    if (i >= 2 && i <= totalSeconds - 2)
                    {
                        // Able to calculate - use -2 to +2 seconds
                        if (isHealingGraph)
                        {
                            fiveSecondAverageValues.Add((records.Where(d => d.SecondsElapsed >= i - 2 && d.SecondsElapsed <= i + 2).Sum(d => d.Effective)) / 5);
                        }
                        else
                        {
                            fiveSecondAverageValues.Add((records.Where(d => d.SecondsElapsed >= i - 2 && d.SecondsElapsed <= i + 2).Sum(d => d.Total)) / 5);
                        }

                    }
                    else
                    {
                        // Unable to calculate moving average, so add 0
                        fiveSecondAverageValues.Add(0);
                    }

                    //instantValueSeries = instantValueDictionary.Select(kvp => new Series()
                    //{
                    //    Name = kvp.Key.CharacterName.Replace("'", "\\\'"),
                    //    Data = new Data(kvp.Value.ToArray())
                    //}).ToList();
                }

                foreach (var instantValue in instantValueDictionary.OrderBy(v => v.Key.CharacterName))
                {
                    instantValueSeries.Add(new Series()
                    {
                        Name = instantValue.Key.CharacterName.Replace("'", "\\\'"),
                        Data = new Data(instantValue.Value.ToArray()),
                        Type = ChartTypes.Spline,
                        PlotOptionsSpline = new PlotOptionsSpline()
                        {
                            Visible = false,
                            Marker = new PlotOptionsSplineMarker() { Enabled = false }
                        }
                    });
                }
                #endregion
            }

            // Loop through instant values and calculate average
            long overallTotal = 0;
            for (int i = 0; i < instantValues.Count; i++)
            {
                overallTotal += instantValues[i];
                averageValues.Add(overallTotal / (i + 1));
            }

            //var secondsArray = secondsElapsed.ToArray();
            //instantValueSeries.Add(new Series()
            //{
            //    Name = "Average",
            //    Data = new Data(averageValues.ToArray()),
            //    Type = ChartTypes.Line,
            //    PlotOptionsLine = new PlotOptionsLine()
            //    {
            //        Marker = new PlotOptionsLineMarker()
            //        {
            //            LineWidth = 1,
            //            Enabled = false
            //        }
            //    }
            //});
            //var instantValueSeriesArray = instantValueSeries.ToArray();
            #endregion

            var instantValueObj = instantValues.Cast<object>().ToList();
            var averageArray = averageValues.Any() ? averageValues.ToArray() : null;
            var fiveSecondAverageArray = fiveSecondAverageValues.Any() ? fiveSecondAverageValues.ToArray() : null;

            var seriesList = new List<Series>();

            var series1 = new Series
            {
                Data = new Data(instantValueObj.ToArray()),
                Name = "Total",
                PlotOptionsSeries = new PlotOptionsSeries()
                {
                    Color = Color.FromArgb(255, 124, 181, 236),
                    Marker = new PlotOptionsSeriesMarker() { Enabled = false }
                }
                //PlotOptionsSeries = new PlotOptionsSeries() { Color = ColorTranslator.FromHtml("#c0c0c0") }
            };

            seriesList.Add(series1);

            if (averageArray != null)
            {
                var avgSeries = new Series
                {
                    Name = "Average",
                    Data = new Data(averageArray),
                    Type = ChartTypes.Line,
                    YAxis = "0",
                    PlotOptionsLine = new PlotOptionsLine()
                    {
                        Marker = new PlotOptionsLineMarker()
                        {
                            LineWidth = 1,
                            Enabled = false
                        },
                        //Color = Color.FromArgb(255, 188, 128, 189)
                    }
                };
                seriesList.Add(avgSeries);
            }

            if (fiveSecondAverageArray != null)
            {
                var fiveSecondAvgSeries = new Series
                {
                    Name = "Average (5s)",
                    Data = new Data(fiveSecondAverageArray),
                    Type = ChartTypes.Line,
                    YAxis = "0",
                    PlotOptionsLine = new PlotOptionsLine()
                    {
                        Marker = new PlotOptionsLineMarker()
                        {
                            LineWidth = 1,
                            Enabled = false
                        },
                        //Color = Color.FromArgb(255, 188, 128, 189)
                    }
                };
                seriesList.Add(fiveSecondAvgSeries);
            }


            seriesList.AddRange(instantValueSeries);

            var chart = new Highcharts("specificPlayerInteraction")
            .InitChart(new Chart
            {
                DefaultSeriesType = ChartTypes.Spline,
                ZoomType = ZoomTypes.Xy,
                Height = 400,
                BackgroundColor = new BackColorOrGradient(new Gradient
                {
                    LinearGradient = new[] { 0, 0, 0, 400 },
                    Stops = new object[,]
                        {{ 0, Color.FromArgb(13, 255, 255, 255) },
                        { 1, Color.FromArgb(13, 255, 255, 255) }}
                }),
                Style = ChartColors.WhiteTextStyle
            })
                .SetCredits(ChartDefaults.Credits)
            .SetOptions(new GlobalOptions
            {
                Colors = ChartColors.ColorArrayBlackBg(),
                Global = new Global { UseUTC = false }
            })
            .SetTitle(new Title
            {
                Text = graphTitle,
                Style = ChartColors.WhiteTextStyle
            })
            .SetXAxis(new XAxis
            {
                LineColor = Color.White,
                TickColor = Color.White,
                Labels = new XAxisLabels { Style = ChartColors.WhiteTextStyle },
            })
            .SetYAxis(new YAxis
            {
                Title = new YAxisTitle { Style = ChartColors.WhiteTextStyle, Text = "" },
                Min = 0,
                TickColor = Color.White,
                LineColor = Color.White,
                Labels = new YAxisLabels { Style = ChartColors.WhiteTextStyle },
            })
            .SetTooltip(new Tooltip() { ValueSuffix = " per second" })
            .SetSeries(seriesList.ToArray())
                .SetExporting(new Exporting { Enabled = false })
                .SetLegend(new Legend() { ItemStyle = ChartColors.WhiteTextStyle, ItemHoverStyle = "color: '#bbb'" })
                ;
            return chart;
        }

        public Highcharts PieChart(string chartName, string graphTitle, string graphSubtitle, Series chartSeries)
        {
            var chart = new Highcharts(chartName)
            .InitChart(new Chart
            {
                DefaultSeriesType = ChartTypes.Pie,
                //ZoomType = ZoomTypes.Xy,
                Height = 400,
                BackgroundColor = new BackColorOrGradient(new Gradient
                {
                    LinearGradient = new[] { 0, 0, 0, 400 },
                    Stops = new object[,]
                        {{ 0, Color.FromArgb(13, 255, 255, 255) },
                        { 1, Color.FromArgb(13, 255, 255, 255) }}
                }),
                Style = ChartColors.WhiteTextStyle,
                
            })
            .SetPlotOptions(new PlotOptions()
            {
                Pie = new PlotOptionsPie()
                {
                    DataLabels = new PlotOptionsPieDataLabels()
                    {
                        Style = ChartColors.WhiteTextStyle,
                        Format = "{point.name} ({point.percentage:.1f}%)"
                    },
                    BorderColor = null,
                    //BorderWidth = 0
                },
               
            })
                .SetCredits(ChartDefaults.Credits)
            .SetOptions(new GlobalOptions
            {
                Colors = ChartColors.ColorArrayBlackBg(),
                Global = new Global { UseUTC = false }
            })
            .SetTitle(new Title
            {
                Text = graphTitle,
                Style = ChartColors.WhiteTextStyle
            })
                //.SetXAxis(new XAxis
                //{
                //    //LineColor = Color.White,
                //    //TickColor = Color.White,
                //    Labels = new XAxisLabels { Style = ChartColors.WhiteTextStyle },
                //})
                //.SetYAxis(new YAxis
                //{
                //    Title = new YAxisTitle { Style = ChartColors.WhiteTextStyle, Text = "" },
                //    //Min = 0,
                //    //TickColor = Color.White,
                //    //LineColor = Color.White,
                //    Labels = new YAxisLabels { Style = ChartColors.WhiteTextStyle },
                //})
                .SetTooltip(new Tooltip()
                {
                    //ValueSuffix = " per second",
                    PointFormat = "<b>{point.y}</b> ({point.percentage:.1f}%)"
                })
                .SetSeries(chartSeries)
                .SetExporting(new Exporting { Enabled = false })
                //.SetLegend(new Legend() { ItemStyle = ChartColors.WhiteTextStyle, ItemHoverStyle = "color: '#bbb'" })
                ;
            return chart;
        }
        
    }
}