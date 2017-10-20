using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Database.Models;
using Database.QueryModels;
using Database.Repositories.Interfaces;
using DotNet.Highcharts;
using DotNet.Highcharts.Enums;
using DotNet.Highcharts.Helpers;
using DotNet.Highcharts.Options;
using Microsoft.AspNet.Identity;
using PrancingTurtle.Helpers;
using PrancingTurtle.Helpers.Charts;
using PrancingTurtle.Models;
using PrancingTurtle.Models.Misc;
using PrancingTurtle.Models.ViewModels;
using PrancingTurtle.Models.ViewModels.Encounter;
using Common;
using Logging;
using WebGrease.Css.Extensions;

namespace PrancingTurtle.Controllers
{
    public class EncounterController : AsyncController
    {
        private readonly ILogger _logger;
        private readonly IAuthenticationRepository _authRepository;
        private readonly IAuthUserCharacterRepository _authUserCharacterRepository;
        private readonly IPlayerRepository _playerRepository;
        private readonly IEncounterRepository _encounterRepository;
        private readonly INpcRepository _npcRepository;
        private readonly ISessionEncounterRepository _sessionEncounterRepository;
        private readonly IEncounterCharts _encounterCharts;

        public EncounterController(IAuthenticationRepository authRepository,
            IAuthUserCharacterRepository authUserCharacterRepository,
            IEncounterRepository encounterRepository, ILogger logger, IPlayerRepository playerRepository,
            INpcRepository npcRepository, ISessionEncounterRepository sessionEncounterRepository, IEncounterCharts encounterCharts)
        {
            _authRepository = authRepository;
            _authUserCharacterRepository = authUserCharacterRepository;
            _encounterRepository = encounterRepository;
            _logger = logger;
            _playerRepository = playerRepository;
            _npcRepository = npcRepository;
            _sessionEncounterRepository = sessionEncounterRepository;
            _encounterCharts = encounterCharts;
        }

        [Authorize]
        public ActionResult MakePrivate(int eid = -1, int sid = -1)
        {
            if (eid == -1 || sid == -1)
            {
                return View("InvalidResource", model: "encounter");
            }

            var encounter = _encounterRepository.Get(eid);

            if (encounter == null)
            {
                return View("InvalidResource", model: "encounter");
            }

            // Check if the user can modify the privacy
            // Check we have a valid user
            var user = _authRepository.GetUserAccount(User.Identity.GetUserId());
            if (user == null)
            {
                HttpContext.GetOwinContext().Authentication.SignOut();
                return RedirectToAction("Index", "Home");
            }

            _logger.Debug(string.Format("{0} is attempting to make encounter {1} private", user.Email, eid));

            var result = _encounterRepository.ChangePrivacy(eid, user.Id, false);
            TempData.Add("flash",
                result.Success
                    ? new FlashSuccessViewModel("Encounter set to private")
                    : new FlashSuccessViewModel(string.Format("Error setting encounter to private: {0}", result.Message)));

            return RedirectToAction("Detail", "Session", new { @id = sid });
        }

        [Authorize]
        public ActionResult MakePublic(int eid = -1, int sid = -1)
        {
            if (eid == -1 || sid == -1)
            {
                return View("InvalidResource", model: "encounter");
            }

            var encounter = _encounterRepository.Get((int)eid);
            //var encounter = _mainRepository.GetEncounter((int)eid);

            if (encounter == null)
            {
                return View("InvalidResource", model: "encounter");
            }

            // Check if the user can modify the privacy
            // Check we have a valid user
            var user = _authRepository.GetUserAccount(User.Identity.GetUserId());
            if (user == null)
            {
                HttpContext.GetOwinContext().Authentication.SignOut();
                return RedirectToAction("Index", "Home");
            }

            _logger.Debug(string.Format("{0} is attempting to make encounter {1} public", user.Email, eid));

            var result = _encounterRepository.ChangePrivacy(eid, user.Id, true);
            TempData.Add("flash",
                result.Success
                    ? new FlashSuccessViewModel("Encounter set to public")
                    : new FlashSuccessViewModel(string.Format("Error setting encounter to public: {0}", result.Message)));

            return RedirectToAction("Detail", "Session", new { @id = sid });
        }

        [Obsolete]
        public ActionResult CharacterTargetBreakdown(int id = -1, int p = 0, string n = "", bool outgoing = true, string type = "DPS")
        {
            //#region Checks / Validation
            //// Check that IDs have been set
            //if (id == -1 || (p == 0 && string.IsNullOrEmpty(n)))
            //{
            //    return RedirectToAction("Index", "Home");
            //}

            //string timezone = "UTC";
            //AuthUser user = null;
            //if (Request.IsAuthenticated)
            //{
            //    user = _authRepository.GetUserAccount(User.Identity.GetUserId());
            //    if (user == null)
            //    {
            //        // Logged-in user isn't valid
            //        HttpContext.GetOwinContext().Authentication.SignOut();
            //        return RedirectToAction("Index", "Home");
            //    }
            //    timezone = user.TimeZone;
            //}

            //var enc = _encounterRepository.Get((int)id);

            //// Check if this encounter has been marked as private
            //if (!enc.IsPublic)
            //{
            //    // This is a private encounter, so if the user isn't authenticated or does not have access,
            //    // don't show the encounter
            //    if (!Request.IsAuthenticated)
            //    {
            //        // User is not logged in
            //        return RedirectToAction("Index", "Home");
            //    }

            //    if (enc.UploaderId == null)
            //    {
            //        // No uploader set, so don't show the encounter
            //        return RedirectToAction("Index", "Home");
            //    }

            //    bool guildParse = enc.GuildId != null;

            //    if (!guildParse)
            //    {
            //        // Get the characters for the logged-in user and check if any of them were the original uploader
            //        var userCharacters = _authUserCharacterRepository.GetCharacters(user.Email);
            //        if (!userCharacters.Any(c => c.AuthUserId == enc.UploaderId))
            //        {
            //            // This user wasn't the original uploader
            //            if (!User.IsInRole(UserGroups.Admin)) return RedirectToAction("Index", "Home");
            //        }
            //    }
            //    else
            //    {
            //        // Check if this user is a member of the guild that uploaded the parse
            //        var member = _authUserCharacterRepository.GetCharacterWithHighestGuildRank(user.Id, (int)enc.GuildId);
            //        if (member == null)
            //        {
            //            if (!User.IsInRole(UserGroups.Admin)) return RedirectToAction("Index", "Home");
            //        }
            //    }
            //}
            //#endregion

            //Stopwatch swBuildTime = new Stopwatch();
            //swBuildTime.Start();

            //var data = new List<EncounterCharacterActionBreakdown>();
            //data = p > 0
            //    ? _encounterRepository.GetBreakdownForEncouter(enc.Id, p, null, CharacterType.Player, outgoing)
            //    : _encounterRepository.GetBreakdownForEncouter(enc.Id, -1, n, CharacterType.Npc, outgoing);

            ////data.Insert(0, new EncounterCharacterActionBreakdown()
            ////{
            ////    TargetId = "NA",
            ////    TargetName = "All Targets",
            ////    Total = data.Sum(d => d.Total),
            ////    Average = data.Sum(d => d.Average),
            ////    Type = CharacterType.Unknown
            ////});

            //var model = new CharacterTargetBreakdownVM()
            //{
            //    Encounter = enc,
            //    CharacterName = _encounterRepository.GetCharacterNameForEncounter(enc.Id, p, n),
            //    TimeZoneId = timezone,
            //    Data = data
            //};

            //switch (type)
            //{
            //    case "DPS":
            //        model.TotalText = "Total Damage";
            //        model.AverageText = "Average DPS";
            //        break;
            //}

            //swBuildTime.Stop();
            //model.BuildTime = swBuildTime.Elapsed;
            return View();
        }

        private string GraphSubtitle(bool outgoing, string type, string filter, string mode, bool shortVersion = false)
        {
            string typeOutput = "";
            switch (type)
            {
                case "hps":
                    typeOutput = "healing";
                    break;
                case "aps":
                    typeOutput = "absorption";
                    break;
                default: // dps
                    typeOutput = "damage";
                    break;
            }

            string filterOutput = "";
            switch (filter.ToLower())
            {
                case "npcs":
                    filterOutput = outgoing ? "to all NPCs" : "from all NPCs";
                    break;
                case "allplayers":
                    filterOutput = outgoing ? "to all players" : "from all players";
                    break;
                case "otherplayers":
                    filterOutput = outgoing ? "to all other players" : "from all other players";
                    break;
                case "self":
                    filterOutput = outgoing ? "to self" : "from self";
                    break;
                default: // all
                    filterOutput = outgoing ? "to all targets" : "from all sources";
                    break;
            }
            // Outgoing damage to all npcs, grouped by ability

            // Outgoing damage to all npcs, grouped by target

            // Incoming healing from all players, grouped by source

            // Outgoing healing to all targets, grouped by abiltiy

            if (shortVersion)
            {
                return string.Format("{0} {1} per second", outgoing ? "Outgoing" : "Incoming", typeOutput);
            }

            return string.Format("{0} {1} per second ({2}), grouped by {3}", outgoing ? "Outgoing" : "Incoming",
                typeOutput, filterOutput,
                mode == "ability" ? "ability" : outgoing ? "target" : "source");
        }

        private InteractionType GetInteractionType(string textVersion)
        {
            switch (textVersion)
            {
                case "aps":
                    return InteractionType.Absorption;
                case "hps":
                    return InteractionType.Healing;
                default:
                    return InteractionType.Damage;
            }
        }

        private InteractionMode GetInteractionMode(string textVersion)
        {
            if (textVersion == "ability") return InteractionMode.Ability;
            return InteractionMode.SourceOrTarget;
        }

        private InteractionFilter GetInteractionFilter(string textVersion)
        {
            switch (textVersion)
            {
                case "otherplayers":
                case "players":
                    return InteractionFilter.Players;
                case "othernpcs":
                case "npcs":
                    return InteractionFilter.Npcs;
                case "self":
                    return InteractionFilter.Self;
                default:
                    return InteractionFilter.All;
            }
        }

        public async Task<ActionResult> DeathEvents(int id = -1, int p = -1, int se = -1)
        {
            #region Checks / Validation
            // Check that an ID has been set
            if (id == -1 || p == -1 || se == -1)
            {
                return View("InvalidResource", model: "encounter");
            }

            // Check if there is an encounter that matches this ID
            var enc = _encounterRepository.Get(id);
            if (enc == null)
            {
                return View("InvalidResource", model: "encounter");
            }

            Player player = null;
            if (p != -1) { player = _playerRepository.Get(p); }
            if (player == null)
            {
                return View("InvalidResource", model: "player");
            }

            string timezone = "UTC";
            if (Request.IsAuthenticated)
            {
                var user = _authRepository.GetUserAccount(User.Identity.GetUserId());
                if (user == null)
                {
                    // Logged-in user isn't valid
                    HttpContext.GetOwinContext().Authentication.SignOut();
                    return RedirectToAction("Index", "Home");
                }
                timezone = user.TimeZone;
            }

            if (!await VisibleToUser(enc, User.Identity.GetUserId()))
            {
                return View("_Private");
            }
            #endregion

            Stopwatch swBuildTime = new Stopwatch();
            swBuildTime.Start();
            #region Get Data

            var deathEvents = _encounterRepository.GetEventsBeforeDeath(enc.Id, player.Id, se - 5, se);
            var encSession = _sessionEncounterRepository.GetSessionForEncounter(enc.Id);
            #endregion

            var model = new EncounterDeathDetailViewModel()
            {
                Encounter = enc,
                Player = player,
                DeathEvents = deathEvents,
                SecondsElapsed = se,
                Session = encSession
            };
            swBuildTime.Stop();
            model.BuildTime = swBuildTime.Elapsed.ToString();
            return View(model);
        }
        
        public async Task<ActionResult> Interaction(int id = -1, int p = -1, string n = "", bool outgoing = true,
            string type = "dps", string mode = "ability", string filter = "all")
        {
            type = type.ToLower();
            // If no encounter ID is set, or no player or NPC specified, redirect
            if (id == -1 || (p == -1 && string.IsNullOrEmpty(n)))
            {
                return View("InvalidResource", model: "encounter or character");
            }
            #region Start Timer
            Stopwatch swBuildTime = new Stopwatch();
            swBuildTime.Start();
            #endregion
            #region Validation
            // Check that the encounter exists
            var enc = _encounterRepository.Get(id);
            if (enc == null)
            {
                return View("InvalidResource", model: "encounter");
            }
            // If the variable is set, check that the player exists
            Player player = null;
            if (p != -1)
            {
                player = _playerRepository.Get(p);
                if (player == null)
                {
                    return View("InvalidResource", model: "player");
                }
            }

            // If the variable is set, check that the npc exists
            string npc = null;
            if (!string.IsNullOrEmpty(n))
            {
                npc = _npcRepository.GetName(n, id);
                if (string.IsNullOrEmpty(npc))
                {
                    return View("InvalidResource", model: "NPC");
                }
            }

            if (!await VisibleToUser(enc, User.Identity.GetUserId()))
            {
                return View("_Private");
            }

            #endregion

            var model = new InteractionVm() { Outgoing = outgoing };

            //var records = new List<CharacterInteractionPerSecond>();
            //var summary = new List<EncounterCharacterAbilityBreakdownDetail>();

            string graphTitle = "";
            string graphSubtitle = "";
            string yAxisTitle = "";

            //records = await Task.Run(() =>
            //    _encounterRepository.CharacterInteractionPerSecond(id, p, n, outgoing,
            //    p == -1 ? CharacterType.Npc : CharacterType.Player, GetInteractionType(type),
            //    GetInteractionFilter(filter), GetInteractionMode(mode)).ToList());

            //summary = await Task.Run(() => _encounterRepository.CharacterInteractionTotals(id, p, n, outgoing,
            //                    p == -1 ? CharacterType.Npc : CharacterType.Player, GetInteractionType(type),
            //                    GetInteractionFilter(filter), GetInteractionMode(mode), (int)enc.Duration.TotalSeconds).ToList());
            
            var records = await _encounterRepository.CharacterInteractionPerSecondAsync(id, p, n, outgoing,
                p == -1 ? CharacterType.Npc : CharacterType.Player, GetInteractionType(type),
                GetInteractionFilter(filter), GetInteractionMode(mode));
            
            var summary = await _encounterRepository.CharacterInteractionTotalsAsync(id, p, n, outgoing,
                p == -1 ? CharacterType.Npc : CharacterType.Player, GetInteractionType(type),
                GetInteractionFilter(filter), GetInteractionMode(mode), (int)enc.Duration.TotalSeconds);

            //sw2.Stop();

            // Both of those should be running

            //await Task.WhenAll(new List<Task>() { recordsTask, summaryTask });

            //var records = recordsTask.Result;
            
            //var summary = summaryTask.Result;

            if (GetInteractionMode(mode) == InteractionMode.Ability
                && !string.IsNullOrEmpty(n) && !outgoing
                && GetInteractionType(type) == InteractionType.Damage
                && GetInteractionFilter(filter) != InteractionFilter.Self)
            {
                // Set a 25-series limit for the ability graphs - particularly for npc damage taken in 20-man raids!!!
                // Currently this only applies to incoming damage to NPCs, but make it apply to anything grouped by ability.
                var top25 = _encounterRepository.GetOverviewNpcDamageTakenTop25Abilities(id, n, filter);
                Stopwatch sw = new Stopwatch();
                sw.Start();
                records = records.Where(r => top25.Contains(r.AbilityName)).ToList();
                sw.Stop();
                _logger.Debug(string.Format("Top 25 record filtering finished in {0}", sw.Elapsed));
            }

            var title = "";
            if (p == -1)
            {
                title = string.Format("Observing NPC: {0}", npc);
                model.PageTitle = string.Format("{0}: {1}", npc, GraphSubtitle(outgoing, type, filter, mode, true));
            }
            else
            {
                title = string.Format("Observing player: {0}", player.Name);
                model.PageTitle = string.Format("{0}: {1}", player.Name, GraphSubtitle(outgoing, type, filter, mode, true));
            }


            var subtitle = GraphSubtitle(outgoing, type, filter, mode);
            swBuildTime.Stop();

            // No data retrieval here, no need for async
            model.SplineChart = _encounterCharts.OverviewChart(title, subtitle, records,
                Convert.ToInt32(enc.Duration.TotalSeconds),
                GetInteractionType(type) == InteractionType.Healing,
                GetInteractionMode(mode) == InteractionMode.Ability, outgoing);
            model.BuildTime = swBuildTime.Elapsed.ToString();
            model.DebugBuildTime = new List<string>();
            model.Mode = mode;
            model.Encounter = enc;

            model.Type = type;
            model.SetText();


            if (summary.Any())
            {
                // Loop through the totals and generate our progress bar percentages
                if (type == "aps")
                {
                    long topTotal = summary.First().Total;
                    for (int i = 0; i < summary.Count; i++)
                    {
                        var summaryRow = summary[i];
                        summaryRow.ProgressBarPercentage = i == 0
                            ? "100%"
                            : ((decimal)summaryRow.Total / topTotal).ToString("#.##%");
                    }
                }
                else
                {
                    long topEffective = summary.First().Effective;
                    for (int i = 0; i < summary.Count; i++)
                    {
                        var summaryRow = summary[i];
                        summaryRow.ProgressBarPercentage = i == 0
                            ? "100%"
                            : ((decimal)summaryRow.Effective / topEffective).ToString("#.##%");
                    }
                }

                var totalCrits = summary.Sum(r => r.Crits);
                var totalHits = summary.Sum(r => r.Hits);
                var topBiggestHit = summary.Max(r => r.BiggestHit);
                var effective = summary.Sum(t => t.Effective);
                var total = summary.Sum(t => t.Total);

                model.TotalCrits = totalCrits;
                model.TotalHits = totalHits;
                model.TopBiggestHit = topBiggestHit;
                model.TopAverageHit = summary.Max(r => r.AverageHit);

                var topRecord = new EncounterCharacterAbilityBreakdownDetail
                {
                    DamageType = "NA",
                    Total = summary.Sum(t => t.Total),
                    Effective = effective,
                    Overhealing = summary.Sum(t => t.Overhealing),
                    Blocked = summary.Sum(t => t.Blocked),
                    Intercepted = summary.Sum(t => t.Intercepted),
                    Ignored = summary.Sum(t => t.Ignored),
                    Absorbed = summary.Sum(t => t.Absorbed),
                    Average = total / (long)enc.Duration.TotalSeconds,
                    AverageEffective = effective / (long)enc.Duration.TotalSeconds,
                    BiggestHit = topBiggestHit,
                    AverageHit = total / (totalCrits + totalHits),
                    TopRecord = true
                };

                if (mode == "ability")
                {
                    topRecord.AbilityName = string.Format("All Abilities ({0})", summary.Count);
                }
                else
                {
                    topRecord.SourceName = string.Format("All Sources ({0})", summary.Count);
                    topRecord.TargetName = string.Format("All Targets ({0})", summary.Count);
                }

                summary.Insert(0, topRecord);
            }

            model.Breakdown = summary;

            return View("~/Views/Encounter/Interaction.cshtml", model);
        }

        private async Task<bool> VisibleToUser(Encounter encounter, string username)
        {
            if (!encounter.IsPublic)
            {
                if (string.IsNullOrEmpty(username))
                {
                    // Not authenticated
                    return false;
                }

                bool guildParse = encounter.GuildId != null;

                if (!guildParse)
                {
                    // Get the characters for the logged-in user and check if any of them were the original uploader
                    var userCharacters = await _authUserCharacterRepository.GetCharactersAsync(username);
                    if (!userCharacters.Any(c => c.Id == encounter.UploaderId))
                    {
                        // This user wasn't the original uploader
                        return false;
                    }
                }
                else
                {
                    // Check if this user is a member of the guild that uploaded the parse
                    var user = _authRepository.GetUserAccount(username);
                    var member = _authUserCharacterRepository.GetCharacterWithHighestGuildRank(user.Id, (int)encounter.GuildId);
                    if (member == null)
                    {
                        if (!User.IsInRole(UserGroups.Admin))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public async Task<ActionResult> Overview(int id = -1)
        {
            #region Checks / Validation
            #region Basic checks
            // Check that an ID has been set
            if (id == -1)
            {
                return View("InvalidResource", model: "encounter");
            }

            // Check if there is an encounter that matches this ID
            var enc = await _encounterRepository.GetAsync(id);
            if (enc == null || enc.Removed)
            {
                return View("InvalidResource", model: "encounter");
            }

            string timezone = "UTC";
            AuthUser user = null;
            if (Request.IsAuthenticated)
            {
                user = await _authRepository.GetUserAccountAsync(User.Identity.GetUserId());
                if (user == null)
                {
                    // Logged-in user isn't valid
                    HttpContext.GetOwinContext().Authentication.SignOut();
                    return RedirectToAction("Index", "Home");
                }
                timezone = user.TimeZone;
            }
            #endregion

            if (!await VisibleToUser(enc, User.Identity.GetUserId()))
            {
                return View("_Private");
            }

            #endregion

            Stopwatch swBuildTime = new Stopwatch();
            swBuildTime.Start();
            #region Get Data
            // Update the player role icons

            var playerRoles = EncounterRoleList(enc.Id);
            var encounterDps = _encounterRepository.GetEncounterDpsAsync(enc.Id, (int)enc.Duration.TotalSeconds);
            var encounterHps = _encounterRepository.GetEncounterHpsAsync(enc.Id, (int)enc.Duration.TotalSeconds);
            var encounterAps = _encounterRepository.GetEncounterApsAsync(enc.Id, (int)enc.Duration.TotalSeconds);
            var debuffActions = _encounterRepository.GetDebuffActionsAsync(enc.Id);
            var buffActions = _encounterRepository.GetBuffActionsAsync(enc.Id);
            var npcCasts = _encounterRepository.GetNpcCastsAsync(enc.Id);
            var playerDeaths = _encounterRepository.GetTotalPlayerDeathsAsync(enc.Id);
            var encSession = _sessionEncounterRepository.GetSessionForEncounterAsync(enc.Id);
            await Task.WhenAll(playerRoles, encounterDps, encounterHps, encounterAps, debuffActions, buffActions, npcCasts, playerDeaths, encSession);
            #endregion

            #region Build the model
            var model = new OverviewVM()
            {
                Encounter = enc,
                TimeZoneId = timezone,
                AverageDps = encounterDps.Result,
                AverageHps = encounterHps.Result,
                AverageAps = encounterAps.Result,
                PlayerRoles = playerRoles.Result,
                DebuffActions = debuffActions.Result,
                BuffActions = buffActions.Result,
                NpcCasts = npcCasts.Result,
                PlayerDeaths = playerDeaths.Result,
                Session = encSession.Result,
            };
            #endregion
            swBuildTime.Stop();
            model.BuildTime = swBuildTime.Elapsed;
            if (System.IO.File.Exists(Server.MapPath(string.Format("~/Content/images/portrait/{0}.png", enc.BossFight.Name.ToLower().Replace(" ", "")))))
            {
                model.LoadImage = true;
            }

            return View(model);
        }

        public async Task<ActionResult> Detail(int id = -1)
        {
            #region Checks / Validation
            #region Basic checks
            // Check that an ID has been set
            if (id == -1)
            {
                return View("InvalidResource", model: "encounter");
            }

            // Check if there is an encounter that matches this ID
            var enc = _encounterRepository.Get(id);
            if (enc == null || enc.Removed)
            {
                return View("InvalidResource", model: "encounter");
            }

            string timezone = "UTC";
            AuthUser user = null;
            if (Request.IsAuthenticated)
            {
                user = _authRepository.GetUserAccount(User.Identity.GetUserId());
                if (user == null)
                {
                    // Logged-in user isn't valid
                    HttpContext.GetOwinContext().Authentication.SignOut();
                    return RedirectToAction("Index", "Home");
                }
                timezone = user.TimeZone;
            }
            #endregion

            if (!await VisibleToUser(enc, User.Identity.GetUserId()))
            {
                return View("_Private");
            }

            #endregion

            Stopwatch swBuildTime = new Stopwatch();
            swBuildTime.Start();
            #region Get Data
            // Update the player role icons
            //var playerRoles = EncounterRoleList(enc.Id);
            var encounterDps = _encounterRepository.GetEncounterDps(enc.Id, (int)enc.Duration.TotalSeconds);
            var encounterHps = _encounterRepository.GetEncounterHps(enc.Id, (int)enc.Duration.TotalSeconds);
            var encounterAps = _encounterRepository.GetEncounterAps(enc.Id, (int)enc.Duration.TotalSeconds);
            //var debuffActions = _encounterRepository.GetDebuffActions(enc.Id);
            //var buffActions = _encounterRepository.GetBuffActions(enc.Id);
            //var npcCasts = _encounterRepository.GetNpcCasts(enc.Id);
            //var playerDeaths = _encounterRepository.GetTotalPlayerDeaths(enc.Id);
            var encSession = _sessionEncounterRepository.GetSessionForEncounter(enc.Id);
            // 20160217
            var dmgToNpcsByPlane = _encounterRepository.GetDetailDamageToNpcsByPlane(enc.Id);
            var dmgToNpcsByPlaneChart = _encounterCharts.PieChart("PlayerDmgDoneByPlane", "Damage by plane", "", dmgToNpcsByPlane.ToPieChartSeries());
            var dmgToPlayersByPlane = _encounterRepository.GetDetailDamageToPlayersByPlane(enc.Id);
            var dmgToPlayersByPlaneChart = _encounterCharts.PieChart("NpcDmgDoneByPlane", "Damage by plane", "", dmgToPlayersByPlane.ToPieChartSeries());
            var dmgToNpcsByClass = _encounterRepository.GetDetailDamageToNpcsByClass(enc.Id);
            var dmgToNpcsByClassChart = _encounterCharts.PieChart("PlayerDmgDoneByClass", "Damage by class", "", dmgToNpcsByClass.ToPieChartSeries());

            #endregion

            #region Build the model
            var model = new DetailVM()
            {
                Encounter = enc,
                TimeZoneId = timezone,
                AverageDps = encounterDps,
                AverageHps = encounterHps,
                AverageAps = encounterAps,
                //PlayerRoles = playerRoles,
                //DebuffActions = debuffActions,
                //BuffActions = buffActions,
                //NpcCasts = npcCasts,
                //PlayerDeaths = playerDeaths,
                Session = encSession,
                ChartDamageToNpcsByPlane = dmgToNpcsByPlaneChart,
                ChartDamageToPlayersByPlane = dmgToPlayersByPlaneChart,
                ChartDamageToNpcsByClass = dmgToNpcsByClassChart
            };
            #endregion
            swBuildTime.Stop();
            model.BuildTime = swBuildTime.Elapsed;
            if (System.IO.File.Exists(Server.MapPath(string.Format("~/Content/images/portrait/{0}.png", enc.BossFight.Name.ToLower().Replace(" ", "")))))
            {
                model.LoadImage = true;
            }

            return View(model);
        }
        
        private async Task<List<EncounterPlayerRole>> EncounterRoleList(int id)
        {
            // Check the list of NPCs seen in this encounter, but don't return this to the UI
            var npcsExist = await _encounterRepository.EncounterNpcRecordsExistAsync(id);
            if (!npcsExist)
            {
                // Find and add the NPCs for this encounter
                var encounterNpcs = await _encounterRepository.GetEncounterNpcsFromEncounterInfoAsync(id);
                if (encounterNpcs.Any())
                {
                    foreach (var npc in encounterNpcs.Where(npc => string.IsNullOrEmpty(npc.NpcName)))
                    {
                        npc.NpcName = "UNKNOWN NPC";
                    }
                    encounterNpcs.ForEach(e => e.EncounterId = id);

                    _logger.Debug(string.Format("Didn't find EncounterNpc records for encounter {0}, creating them now.", id));
                    var addNpcResult = await _encounterRepository.AddEncounterNpcsAsync(encounterNpcs);
                    _logger.Debug(addNpcResult.Success
                        ? string.Format("Successfully added {0} EncounterNpc records", encounterNpcs.Count)
                        : string.Format("An error occurred while adding EncounterNpc records: {0}", addNpcResult.Message));
                }
            }

            // Check if the encounter role list exists in the database for this encounter
            var roles = await _encounterRepository.GetEncounterRoleRecordsAsync(id);
            if (roles.Any()) return roles;

            _logger.Debug(string.Format("Didn't find EncounterPlayerRole records for encounter {0}, creating them now.", id));

            var rolesFromRecords = await _encounterRepository.GetPlayerRolesAsync(id);
            // TODO: Roll this next block into the previous line and do it all in the repository. No need for it to be here
            var playerRoleList = rolesFromRecords.Select(role => new EncounterPlayerRole()
            {
                Class = role.Class,
                EncounterId = id,
                PlayerId = role.Id,
                Role = role.Role,
                Name = role.Name
            }).ToList();

            var result = await _encounterRepository.AddPlayerEncounterRolesAsync(playerRoleList);
            _logger.Debug(result.Success
                ? string.Format("Successfully added {0} EncounterPlayerRole records", playerRoleList.Count)
                : string.Format("An error occurred while adding EncounterPlayerRole records: {0}", result.Message));

            return playerRoleList;
        }

        public async Task<ActionResult> PlayerDamageDone(int? id, bool showdebugtimers = false)
        {
            #region Checks / Validation
            // Check that an ID has been set
            if (id == null)
            {
                return View("InvalidResource", model: "encounter");
            }

            // Check if there is an encounter that matches this ID
            var enc = _encounterRepository.Get((int)id);
            if (enc == null)
            {
                return View("InvalidResource", model: "encounter");
            }

            string timezone = "UTC";
            if (Request.IsAuthenticated)
            {
                var user = _authRepository.GetUserAccount(User.Identity.GetUserId());
                if (user == null)
                {
                    // Logged-in user isn't valid
                    HttpContext.GetOwinContext().Authentication.SignOut();
                    return RedirectToAction("Index", "Home");
                }
                timezone = user.TimeZone;
            }
            if (!await VisibleToUser(enc, User.Identity.GetUserId()))
            {
                return View("_Private");
            }
            #endregion

            var model = _encounterCharts.PlayerDamageDone(enc);
            model.TimeZoneId = timezone;

            return View("PlayerSomethingDone", model);
        }

        [HttpPost]
        public ActionResult PlayerDamageDone(FormCollection collection)
        {
            int encounterId = 0;
            var compareList = new List<int>();
            foreach (var key in collection)
            {
                var keyName = key.ToString();
                if (keyName == "Encounter.Id")
                {
                    encounterId = int.Parse(collection[keyName]);
                }
                else if (keyName.StartsWith("chkComparePlayer"))
                {
                    string chkVal = collection[keyName];
                    if (chkVal.Contains("true"))
                    {
                        int playerId = int.Parse(keyName.Replace("chkComparePlayer", ""));
                        if (!compareList.Contains(playerId))
                        {
                            compareList.Add(playerId);
                        }
                    }
                }
            }

            if (compareList.Count <= 1)
            {
                if (encounterId > 0)
                {
                    return RedirectToAction("PlayerDamageDone", new { id = encounterId });
                }

                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("ComparePlayerDamageDone", new { id = encounterId, p = string.Join("-", compareList) });
        }

        public async Task<ActionResult> PlayerDamageTaken(int? id, bool showdebugtimers = false)
        {
            #region Checks / Validation
            // Check that an ID has been set
            if (id == null)
            {
                return View("InvalidResource", model: "encounter");
            }

            // Check if there is an encounter that matches this ID
            var enc = _encounterRepository.Get((int)id);
            if (enc == null)
            {
                return View("InvalidResource", model: "encounter");
            }

            string timezone = "UTC";
            if (Request.IsAuthenticated)
            {
                var user = _authRepository.GetUserAccount(User.Identity.GetUserId());
                if (user == null)
                {
                    // Logged-in user isn't valid
                    HttpContext.GetOwinContext().Authentication.SignOut();
                    return RedirectToAction("Index", "Home");
                }
                timezone = user.TimeZone;
            }
            if (!await VisibleToUser(enc, User.Identity.GetUserId()))
            {
                return View("_Private");
            }
            #endregion

            var model = _encounterCharts.PlayerDamageTaken(enc);
            model.TimeZoneId = timezone;
            return View("PlayerSomethingTaken", model);
        }

        [HttpPost]
        public ActionResult PlayerHealingDone(FormCollection collection)
        {
            if (string.IsNullOrEmpty(collection["Encounter.Id"]))
            {
                return RedirectToAction("Index", "Home");
            }
            if (!Request.IsAuthenticated || !User.IsInRole(UserGroups.Admin))
            {
                var id = int.Parse(collection["Encounter.Id"]);
                return RedirectToAction("PlayerHealingDone", new { id });
            }

            int encounterId = 0;
            var compareList = new List<int>();
            foreach (var key in collection)
            {
                var keyName = key.ToString();
                if (keyName == "Encounter.Id")
                {
                    encounterId = int.Parse(collection[keyName]);
                }
                else if (keyName.StartsWith("chkComparePlayer"))
                {
                    string chkVal = collection[keyName];
                    if (chkVal.Contains("true"))
                    {
                        int playerId = int.Parse(keyName.Replace("chkComparePlayer", ""));
                        if (!compareList.Contains(playerId))
                        {
                            compareList.Add(playerId);
                        }
                    }
                }
            }

            if (compareList.Count <= 1)
            {
                if (encounterId > 0)
                {
                    return RedirectToAction("PlayerHealingDone", new { id = encounterId });
                }

                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("ComparePlayerHealingDone", new { id = encounterId, p = string.Join("-", compareList) });
        }

        public async Task<ActionResult> PlayerHealingDone(int? id, bool showdebugtimers = false)
        {
            #region Checks / Validation
            // Check that an ID has been set
            if (id == null)
            {
                return View("InvalidResource", model: "encounter");
            }

            // Check if there is an encounter that matches this ID
            var enc = _encounterRepository.Get((int)id);
            if (enc == null)
            {
                return View("InvalidResource", model: "encounter");
            }

            string timezone = "UTC";
            if (Request.IsAuthenticated)
            {
                var user = _authRepository.GetUserAccount(User.Identity.GetUserId());
                if (user == null)
                {
                    // Logged-in user isn't valid
                    HttpContext.GetOwinContext().Authentication.SignOut();
                    return RedirectToAction("Index", "Home");
                }
                timezone = user.TimeZone;
            }
            if (!await VisibleToUser(enc, User.Identity.GetUserId()))
            {
                return View("_Private");
            }
            #endregion

            var model = _encounterCharts.PlayerHealingDone(enc);
            model.TimeZoneId = timezone;
            return View("PlayerSomethingDone", model);
        }

        public async Task<ActionResult> PlayerHealingTaken(int? id, bool showdebugtimers = false)
        {
            #region Checks / Validation
            // Check that an ID has been set
            if (id == null)
            {
                return View("InvalidResource", model: "encounter");
            }

            // Check if there is an encounter that matches this ID
            var enc = _encounterRepository.Get((int)id);
            if (enc == null)
            {
                return View("InvalidResource", model: "encounter");
            }

            string timezone = "UTC";
            if (Request.IsAuthenticated)
            {
                var user = _authRepository.GetUserAccount(User.Identity.GetUserId());
                if (user == null)
                {
                    // Logged-in user isn't valid
                    HttpContext.GetOwinContext().Authentication.SignOut();
                    return RedirectToAction("Index", "Home");
                }
                timezone = user.TimeZone;
            }
            if (!await VisibleToUser(enc, User.Identity.GetUserId()))
            {
                return View("_Private");
            }
            #endregion

            #region THIS CODE HAS BEEN MOVED TO THE ENCOUNTERCHARTS INTERFACE/CLASS
            //List<string> debugTimers = new List<string>();

            //Stopwatch swIndividualTimer = new Stopwatch();
            //Stopwatch swBuildTime = new Stopwatch();
            //swBuildTime.Start();
            //swIndividualTimer.Start();
            //#region Get Data
            //var playerRoles = EncounterRoleList(enc.Id);
            //swIndividualTimer.Stop();
            //debugTimers.Add(string.Format("GetPlayerRoles completed in {0}", swIndividualTimer.Elapsed));
            //swIndividualTimer.Restart();
            //var healingTaken = _encounterRepository.GetOverviewPlayerHealingTaken(enc.Id);
            //swIndividualTimer.Stop();
            //debugTimers.Add(string.Format("GetOverviewPlayerHealingTaken completed in {0}", swIndividualTimer.Elapsed));
            //swIndividualTimer.Restart();
            //var playerDamageTaken = UpdateRoleIcons(healingTaken, playerRoles);
            //swIndividualTimer.Stop();
            //debugTimers.Add(string.Format("UpdateRoleIcons completed in {0}", swIndividualTimer.Elapsed));
            //swIndividualTimer.Restart();
            //var healingTakenGraph = _encounterRepository.GetOverviewPlayerHealingTakenGraph(enc.Id);
            //swIndividualTimer.Stop();
            //debugTimers.Add(string.Format("GetOverviewPlayerHealingTakenGraph completed in {0}", swIndividualTimer.Elapsed));
            //swIndividualTimer.Restart();
            //var raidBuffTimers = _encounterRepository.GetMainRaidBuffs(enc.Id);
            //swIndividualTimer.Stop();
            //debugTimers.Add(string.Format("GetMainRaidBuffs completed in {0}", swIndividualTimer.Elapsed));
            //swIndividualTimer.Restart();
            //var playerDeaths = _encounterRepository.GetAllPlayerDeathTimers(enc.Id);
            //swIndividualTimer.Stop();
            //debugTimers.Add(string.Format("GetPlayerDeathTimers completed in {0}", swIndividualTimer.Elapsed));
            //swIndividualTimer.Restart();
            //var encSession = _sessionEncounterRepository.GetSessionForEncounter(enc.Id);
            //swIndividualTimer.Stop();
            //debugTimers.Add(string.Format("GetSessionIdForEncounter completed in {0}", swIndividualTimer.Elapsed));
            //if (showdebugtimers)
            //{
            //    _logger.Debug(string.Format("GetSessionIdForEncounter completed in {0}", swIndividualTimer.Elapsed));
            //}
            //#endregion

            //if (healingTaken.Any())
            //{
            //    // Loop through the totals and generate our progress bar percentages
            //    long topTotal = healingTaken.First().Total;
            //    for (int i = 0; i < healingTaken.Count; i++)
            //    {
            //        var healRow = healingTaken[i];
            //        healRow.ProgressBarPercentage = i == 0
            //            ? "100%"
            //            : ((decimal)healRow.Total / topTotal).ToString("#.##%");
            //    }
            //}

            //#region Export Text

            //string exportText = string.Format("{0} {1}: {2}HTPS", enc.Duration.ToString(@"mm\:ss"), enc.BossFight.Name,
            //    healingTaken.Sum(h => h.Total) / (int)enc.Duration.TotalSeconds);
            //exportText = healingTaken
            //    .OrderByDescending(h => h.Total)
            //    .Aggregate(exportText, (current, heal) =>
            //        current + string.Format(" | {0} {1}", heal.PlayerName, heal.Total / (int)enc.Duration.TotalSeconds));

            //#endregion

            //var overallTotal = healingTaken.Sum(d => d.Total);
            //var overallTotalNpcs = healingTaken.Sum(d => d.TotalFromNpcs);
            //var overallTotalPlayers = healingTaken.Sum(d => d.TotalFromOtherPlayers);
            //var overallTotalSelf = healingTaken.Sum(d => d.TotalFromSelf);
            //healingTaken.Insert(0, new OverviewPlayerSomethingTaken()
            //{
            //    PlayerId = -1,
            //    PlayerName = "All players",
            //    Total = overallTotal,
            //    TotalFromNpcs = overallTotalNpcs,
            //    TotalFromOtherPlayers = overallTotalPlayers,
            //    TotalFromSelf = overallTotalSelf
            //});

            //#region Create arrays for the chart
            ////var totalList = new List<object>();
            //var npcList = new List<object>();
            //var playerList = new List<object>();
            //var secondsList = new List<string>();

            //var instantValues = new List<long>();
            //var averageValues = new List<object>();
            //var playerDeathList = new List<object>();

            //for (int i = 0; i <= enc.Duration.TotalSeconds; i++)
            //{
            //    npcList.Add(healingTakenGraph.Where(d => d.SecondsElapsed == i).Sum(d => d.TotalNpcs));
            //    playerList.Add(healingTakenGraph.Where(d => d.SecondsElapsed == i).Sum(d => d.TotalPlayers));
            //    instantValues.Add(healingTakenGraph.Where(d => d.SecondsElapsed == i).Sum(d => d.TotalPlayers));
            //    secondsList.Add(i.ToString(CultureInfo.InvariantCulture));
            //    playerDeathList.Add(playerDeaths.Count(d => d == i));
            //}

            //// Loop through instant values and calculate average
            //long playerTotal = 0;
            //for (var i = 0; i < instantValues.Count; i++)
            //{
            //    playerTotal += instantValues[i];
            //    averageValues.Add(playerTotal / (i + 1));
            //}
            ////var totalArray = totalList.ToArray();
            //var npcArray = npcList.ToArray();
            //var playerArray = playerList.ToArray();
            //var secondsArray = secondsList.ToArray();
            //#endregion
            //#region Create Chart

            //var seriesArray = new[]
            //{
            //    new Series
            //            {
            //                Name = "Healing received from NPCs",
            //                Data = new Data(npcArray),
            //                //Color = ColorTranslator.FromHtml("#feab1a"),
            //                YAxis = "0",
            //                PlotOptionsArea = new PlotOptionsArea(){ Visible = false }
            //            },
            //    new Series

            //            {
            //                Name = "Healing received from players",
            //                Data = new Data(playerArray),
            //                //Color = ColorTranslator.FromHtml("#3a87ad"),
            //                YAxis = "0",
            //                PlotOptionsArea = new PlotOptionsArea(){ Visible = true }
            //            },
            //    new Series()
            //    {
            //        Name = "Average healing taken",
            //        Data = new Data(averageValues.ToArray()),
            //        Type = ChartTypes.Line,
            //        YAxis = "0",
            //        PlotOptionsLine = new PlotOptionsLine()
            //        {
            //            Marker = new PlotOptionsLineMarker()
            //            {
            //                LineWidth = 1,
            //                Enabled = false
            //            }
            //        }
            //    },
            //    new Series()
            //    {
            //        Name = "Player Deaths",
            //        Data = new Data(playerDeathList.ToArray()),
            //        Type = ChartTypes.Column,
            //        PlotOptionsColumn = new PlotOptionsColumn(){ BorderWidth = 0, Visible = true},
            //        Color = Color.DarkOrange,
            //        YAxis = "1"
            //    }

            //};
            //var newChart = CreateChart(enc.Id, "Player healing received (per second)", secondsArray, "Healing received", "Deaths", seriesArray);


            //#endregion
            //var model = new PlayerSomethingTaken()
            //{
            //    Encounter = enc,
            //    Data = playerDamageTaken,
            //    AverageText = "Average HPS",
            //    GraphType = "HPS",
            //    PageTitle = "Healing received by players",
            //    IsOutgoing = false,
            //    TotalText = "Total Healing",
            //    Graph = newChart,
            //    DebugBuildTime = new List<string>(),
            //    TimeZoneId = timezone,
            //    Session = encSession,
            //    ExportText = exportText,
            //    SplineGraph = _encounterCharts.PlayerNpcSomethingDoneOrTaken(enc.Id, secondsArray, "Seconds elapsed", playerArray,
            //    "Deaths", averageValues.ToArray(), null, false, true, "HPS", null, playerDeathList, null)
            //};
            //swBuildTime.Stop();
            //model.BuildTime = swBuildTime.Elapsed;
            //if (showdebugtimers)
            //{
            //    model.DebugBuildTime = debugTimers;
            //}
            #endregion
            var model = _encounterCharts.PlayerHealingTaken(enc);
            model.TimeZoneId = timezone;
            return View("PlayerSomethingTaken", model);
        }

        public async Task<ActionResult> PlayerShieldingDone(int? id, bool showdebugtimers = false)
        {
            #region Checks / Validation
            // Check that an ID has been set
            if (id == null)
            {
                return View("InvalidResource", model: "encounter");
            }

            // Check if there is an encounter that matches this ID
            var enc = _encounterRepository.Get((int)id);
            if (enc == null)
            {
                return View("InvalidResource", model: "encounter");
            }

            string timezone = "UTC";
            if (Request.IsAuthenticated)
            {
                var user = _authRepository.GetUserAccount(User.Identity.GetUserId());
                if (user == null)
                {
                    // Logged-in user isn't valid
                    HttpContext.GetOwinContext().Authentication.SignOut();
                    return RedirectToAction("Index", "Home");
                }
                timezone = user.TimeZone;
            }
            if (!await VisibleToUser(enc, User.Identity.GetUserId()))
            {
                return View("_Private");
            }
            #endregion

            var model = _encounterCharts.PlayerShieldingDone(enc);
            model.TimeZoneId = timezone;
            return View("PlayerSomethingDone", model);
        }

        public async Task<ActionResult> PlayerShieldingTaken(int? id, bool showdebugtimers = false)
        {
            #region Checks / Validation
            // Check that an ID has been set
            if (id == null)
            {
                return View("InvalidResource", model: "encounter");
            }

            // Check if there is an encounter that matches this ID
            var enc = _encounterRepository.Get((int)id);
            if (enc == null)
            {
                return View("InvalidResource", model: "encounter");
            }

            string timezone = "UTC";
            if (Request.IsAuthenticated)
            {
                var user = _authRepository.GetUserAccount(User.Identity.GetUserId());
                if (user == null)
                {
                    // Logged-in user isn't valid
                    HttpContext.GetOwinContext().Authentication.SignOut();
                    return RedirectToAction("Index", "Home");
                }
                timezone = user.TimeZone;
            }
            if (!await VisibleToUser(enc, User.Identity.GetUserId()))
            {
                return View("_Private");
            }
            #endregion

            var model = _encounterCharts.PlayerShieldingTaken(enc);
            model.TimeZoneId = timezone;
            return View("PlayerSomethingTaken", model);
        }

        public async Task<ActionResult> NpcDamageDone(int? id, bool showdebugtimers = false)
        {
            #region Checks / Validation
            // Check that an ID has been set
            if (id == null)
            {
                return View("InvalidResource", model: "encounter");
            }

            // Check if there is an encounter that matches this ID
            var enc = _encounterRepository.Get((int)id);
            if (enc == null)
            {
                return View("InvalidResource", model: "encounter");
            }

            string timezone = "UTC";
            if (Request.IsAuthenticated)
            {
                var user = _authRepository.GetUserAccount(User.Identity.GetUserId());
                if (user == null)
                {
                    // Logged-in user isn't valid
                    HttpContext.GetOwinContext().Authentication.SignOut();
                    return RedirectToAction("Index", "Home");
                }
                timezone = user.TimeZone;
            }
            if (!await VisibleToUser(enc, User.Identity.GetUserId()))
            {
                return View("_Private");
            }
            #endregion

            var model = _encounterCharts.NpcDamageDone(enc);
            model.TimeZoneId = timezone;
            return View("NpcSomethingDone", model);
        }

        public async Task<ActionResult> NpcDamageTaken(int? id, bool showdebugtimers = false)
        {
            #region Checks / Validation
            // Check that an ID has been set
            if (id == null)
            {
                return View("InvalidResource", model: "encounter");
            }

            // Check if there is an encounter that matches this ID
            var enc = _encounterRepository.Get((int)id);
            if (enc == null)
            {
                return View("InvalidResource", model: "encounter");
            }

            string timezone = "UTC";
            if (Request.IsAuthenticated)
            {
                var user = _authRepository.GetUserAccount(User.Identity.GetUserId());
                if (user == null)
                {
                    // Logged-in user isn't valid
                    HttpContext.GetOwinContext().Authentication.SignOut();
                    return RedirectToAction("Index", "Home");
                }
                timezone = user.TimeZone;
            }
            if (!await VisibleToUser(enc, User.Identity.GetUserId()))
            {
                return View("_Private");
            }
            #endregion

            var model = _encounterCharts.NpcDamageTaken(enc);
            model.TimeZoneId = timezone;
            return View("NpcSomethingTaken", model);
        }

        public async Task<ActionResult> NpcHealingDone(int? id, bool showdebugtimers = false)
        {
            #region Checks / Validation
            // Check that an ID has been set
            if (id == null)
            {
                return View("InvalidResource", model: "encounter");
            }

            // Check if there is an encounter that matches this ID
            var enc = _encounterRepository.Get((int)id);
            if (enc == null)
            {
                return View("InvalidResource", model: "encounter");
            }

            string timezone = "UTC";
            if (Request.IsAuthenticated)
            {
                var user = _authRepository.GetUserAccount(User.Identity.GetUserId());
                if (user == null)
                {
                    // Logged-in user isn't valid
                    HttpContext.GetOwinContext().Authentication.SignOut();
                    return RedirectToAction("Index", "Home");
                }
                timezone = user.TimeZone;
            }
            if (!await VisibleToUser(enc, User.Identity.GetUserId()))
            {
                return View("_Private");
            }
            #endregion

            var model = _encounterCharts.NpcHealingDone(enc);
            model.TimeZoneId = timezone;
            return View("NpcSomethingDone", model);
        }

        public async Task<ActionResult> NpcHealingTaken(int? id, bool showdebugtimers = false)
        {
            #region Checks / Validation
            // Check that an ID has been set
            if (id == null)
            {
                return View("InvalidResource", model: "encounter");
            }

            // Check if there is an encounter that matches this ID
            var enc = _encounterRepository.Get((int)id);
            if (enc == null)
            {
                return View("InvalidResource", model: "encounter");
            }

            string timezone = "UTC";
            if (Request.IsAuthenticated)
            {
                var user = _authRepository.GetUserAccount(User.Identity.GetUserId());
                if (user == null)
                {
                    // Logged-in user isn't valid
                    HttpContext.GetOwinContext().Authentication.SignOut();
                    return RedirectToAction("Index", "Home");
                }
                timezone = user.TimeZone;
            }
            if (!await VisibleToUser(enc, User.Identity.GetUserId()))
            {
                return View("_Private");
            }
            #endregion

            var model = _encounterCharts.NpcHealingTaken(enc);
            model.TimeZoneId = timezone;
            return View("NpcSomethingTaken", model);
        }

        public async Task<ActionResult> NpcShieldingDone(int? id, bool showdebugtimers = false)
        {
            #region Checks / Validation
            // Check that an ID has been set
            if (id == null)
            {
                return View("InvalidResource", model: "encounter");
            }

            // Check if there is an encounter that matches this ID
            var enc = _encounterRepository.Get((int)id);
            if (enc == null)
            {
                return View("InvalidResource", model: "encounter");
            }

            string timezone = "UTC";
            if (Request.IsAuthenticated)
            {
                var user = _authRepository.GetUserAccount(User.Identity.GetUserId());
                if (user == null)
                {
                    // Logged-in user isn't valid
                    HttpContext.GetOwinContext().Authentication.SignOut();
                    return RedirectToAction("Index", "Home");
                }
                timezone = user.TimeZone;
            }
            if (!await VisibleToUser(enc, User.Identity.GetUserId()))
            {
                return View("_Private");
            }
            #endregion

            var model = _encounterCharts.NpcShieldingDone(enc);
            model.TimeZoneId = timezone;
            return View("NpcSomethingDone", model);
        }

        public async Task<ActionResult> NpcShieldingTaken(int? id, bool showdebugtimers = false)
        {
            #region Checks / Validation
            // Check that an ID has been set
            if (id == null)
            {
                return View("InvalidResource", model: "encounter");
            }

            // Check if there is an encounter that matches this ID
            var enc = _encounterRepository.Get((int)id);
            if (enc == null)
            {
                return View("InvalidResource", model: "encounter");
            }

            string timezone = "UTC";
            if (Request.IsAuthenticated)
            {
                var user = _authRepository.GetUserAccount(User.Identity.GetUserId());
                if (user == null)
                {
                    // Logged-in user isn't valid
                    HttpContext.GetOwinContext().Authentication.SignOut();
                    return RedirectToAction("Index", "Home");
                }
                timezone = user.TimeZone;
            }
            if (!await VisibleToUser(enc, User.Identity.GetUserId()))
            {
                return View("_Private");
            }
            #endregion

            var model = _encounterCharts.NpcShieldingTaken(enc);
            model.TimeZoneId = timezone;
            return View("NpcSomethingTaken", model);
        }

        public async Task<ActionResult> Deaths(int? id)
        {
            #region Checks / Validation
            // Check that an ID has been set
            if (id == null)
            {
                return View("InvalidResource", model: "encounter");
            }

            // Check if there is an encounter that matches this ID
            var enc = _encounterRepository.Get((int)id);
            if (enc == null)
            {
                return View("InvalidResource", model: "encounter");
            }

            string timezone = "UTC";
            if (Request.IsAuthenticated)
            {
                var user = _authRepository.GetUserAccount(User.Identity.GetUserId());
                if (user == null)
                {
                    // Logged-in user isn't valid
                    HttpContext.GetOwinContext().Authentication.SignOut();
                    return RedirectToAction("Index", "Home");
                }
                timezone = user.TimeZone;
            }
            if (!await VisibleToUser(enc, User.Identity.GetUserId()))
            {
                return View("_Private");
            }
            #endregion

            Stopwatch swBuildTime = new Stopwatch();
            swBuildTime.Start();
            var deaths = _encounterRepository.GetDeaths(enc.Id);
            var encSession = _sessionEncounterRepository.GetSessionForEncounter(enc.Id);

            var model = new DeathVM()
            {
                Encounter = enc,
                TimeZoneId = timezone,
                Deaths = deaths,
                Session = encSession
            };
            swBuildTime.Stop();
            model.BuildTime = swBuildTime.Elapsed;

            return View(model);
        }

        public async Task<ActionResult> Buffs(int? id, string target, bool showdebugtimers = false)
        {
            #region Checks / Validation
            // Check that an ID has been set
            if (id == null || string.IsNullOrEmpty(target))
            {
                return View("InvalidResource", model: "target");
            }

            // Check if there is an encounter that matches this ID
            var enc = _encounterRepository.Get((int)id);
            if (enc == null)
            {
                return View("InvalidResource", model: "encounter");
            }

            string timezone = "UTC";
            if (Request.IsAuthenticated)
            {
                var user = _authRepository.GetUserAccount(User.Identity.GetUserId());
                if (user == null)
                {
                    // Logged-in user isn't valid
                    HttpContext.GetOwinContext().Authentication.SignOut();
                    return RedirectToAction("Index", "Home");
                }
                timezone = user.TimeZone;
            }
            if (!await VisibleToUser(enc, User.Identity.GetUserId()))
            {
                return View("_Private");
            }
            #endregion

            List<string> debugTimers = new List<string>();

            Stopwatch swIndividualTimer = new Stopwatch();
            Stopwatch swBuildTime = new Stopwatch();
            swBuildTime.Start();
            swIndividualTimer.Start();

            // Check players first
            var targetName = _playerRepository.GetTargetNameFromLogId(target);
            if (string.IsNullOrEmpty(targetName))
            {
                // Check NPCs
                targetName = _encounterRepository.GetNpcName((int)id, target);
            }

            if (string.IsNullOrEmpty(targetName))
            {
                _logger.Debug(string.Format("Couldn't find the name of the target with the log ID of {0}!", target));
                var noBuffsModel = new CharacterBuffs()
                {
                    Encounter = enc,
                    Graph = null,
                    TargetName = string.Format("Target ID {0}", target),
                    DebugBuildTime = new List<string>(),
                    TimeZoneId = timezone
                };
                swBuildTime.Stop();
                noBuffsModel.BuildTime = swBuildTime.Elapsed;
                if (showdebugtimers)
                {
                    noBuffsModel.DebugBuildTime = debugTimers;
                }
                return View(noBuffsModel);
            }

            var characterBuffs = _encounterRepository.GetCharacterBuffs(enc.Id, target);
            swIndividualTimer.Stop();
            debugTimers.Add(string.Format("GetCharacterBuffs completed in {0}", swIndividualTimer.Elapsed));
            //var player = _playerRepository.Get(playerId);

            //var playerRoles = _encounterRepository.GetPlayerRoles(enc.Id);

            if (characterBuffs.Count == 0)
            {
                var noBuffsModel = new CharacterBuffs()
                {
                    Encounter = enc,
                    Graph = null,
                    TargetName = targetName,
                    DebugBuildTime = new List<string>(),
                    TimeZoneId = timezone
                };
                swBuildTime.Stop();
                noBuffsModel.BuildTime = swBuildTime.Elapsed;
                if (showdebugtimers)
                {
                    noBuffsModel.DebugBuildTime = debugTimers;
                }
                return View(noBuffsModel);
            }

            #region Buff timer graph
            var buffTimers = new Dictionary<string, List<BuffUpDown>>();
            var heightCalcArrays = 0;

            foreach (var buffGroup in characterBuffs.GroupBy(b => new { b.BuffName, b.SourceName }))
            {
                heightCalcArrays += 1;
                // If, for some reason, a buff goes down after the encounter has finished, fix up the timer
                foreach (var buffEventItem in buffGroup)
                {
                    if (buffEventItem.SecondBuffWentDown > enc.Duration.TotalSeconds)
                    {
                        buffEventItem.SecondBuffWentDown = (int)enc.Duration.TotalSeconds;
                    }
                }

                // Loop through events to get the total uptime
                int secondsUp = 0;
                buffGroup.ForEach(b => secondsUp += (b.SecondBuffWentDown - b.SecondBuffWentUp));

                string buffName = string.Format("{0} ({1}) - {2} uptime",
                    buffGroup.Key.BuffName.Replace("'", "\\\'"), buffGroup.Key.SourceName,
                    ((decimal)secondsUp / (decimal)enc.Duration.TotalSeconds).ToString("#.##%"));
                foreach (var buffEventItem in buffGroup)
                {
                    var thisBuffUpDown = new BuffUpDown()
                    {
                        SecondUp = buffEventItem.SecondBuffWentUp,
                        SecondDown = buffEventItem.SecondBuffWentDown
                    };

                    if (!buffTimers.ContainsKey(buffName))
                    {
                        buffTimers.Add(buffName, new List<BuffUpDown>() { thisBuffUpDown });
                    }
                    else
                    {
                        buffTimers[buffName].Add(thisBuffUpDown);
                    }
                }
            }

            var chartHeight = (30 * heightCalcArrays) + 100;

            int seriesCount = buffTimers.Max(b => b.Value.Count);
            var seriesDictionary = new Dictionary<string, Dictionary<string, object[]>>();
            //var seriesColourDict = new Dictionary<string, Color>();
            for (int i = 1; i <= seriesCount; i++)
            {
                string seriesName = string.Format("#{0}", i);
                seriesDictionary.Add(seriesName, new Dictionary<string, object[]>());

                foreach (var kvp in buffTimers)
                {
                    if (i > kvp.Value.Count)
                    {
                        seriesDictionary[seriesName].Add(kvp.Key, new object[] { 0, 0 });
                    }
                    else
                    {
                        seriesDictionary[seriesName].Add(kvp.Key,
                            new object[]
                            {
                                kvp.Value[i - 1].SecondUp,
                                kvp.Value[i - 1].SecondDown
                            });
                    }
                    //if (!seriesColourDict.ContainsKey(seriesName))
                    //{
                    //    var playerName = kvp.Key.Substring(kvp.Key.IndexOf('(') + 1, kvp.Key.IndexOf(')') - kvp.Key.IndexOf('(') - 1);
                    //    var playerRole = playerRoles.FirstOrDefault(r => r.Name == playerName);
                    //    if (playerRole == null)
                    //    {
                    //        seriesColourDict.Add(seriesName, Color.Black);
                    //    }
                    //    else
                    //    {
                    //        if (playerRole.Role == "Healing")
                    //        {
                    //            seriesColourDict.Add(seriesName, Color.Green);
                    //        }
                    //        else if (playerRole.Role == "Damage")
                    //        {
                    //            seriesColourDict.Add(seriesName, Color.Red);
                    //        }
                    //        else if (playerRole.Role == "Support")
                    //        {
                    //            seriesColourDict.Add(seriesName, Color.DarkOrchid);
                    //        }
                    //        else
                    //        {
                    //            seriesColourDict.Add(seriesName, Color.CornflowerBlue);
                    //        }
                    //    }
                    //}
                }
            }
            var mainSeriesList = new List<Series>();
            foreach (var kvp in seriesDictionary)
            {
                var objArray = kvp.Value.Values.ToArray();
                //var firstKey = kvp.Value.First().Key;
                //var playerName = firstKey.Substring(firstKey.IndexOf('(') + 1, firstKey.IndexOf(')') - firstKey.IndexOf('(') - 1);
                //var role = playerRoles.First(r => r.Name == )

                string num = kvp.Key.Replace("#", "");
                int numKey = int.Parse(num);

                mainSeriesList.Add(
                    new Series()
                    {
                        Name = kvp.Key,
                        Color = ColorTranslator.FromHtml("#7cb5ec"),
                        //Color = seriesColourDict[kvp.Key],
                        //Color = numKey % 2 == 0 ? Color.Red : Color.Black,
                        Data = new Data(objArray)
                    });
            }

            var plotLines = new List<YAxisPlotLines>
            {
                new YAxisPlotLines()
                {
                    Color = Color.Green,
                    Value = enc.Duration.TotalSeconds,
                    Label = new YAxisPlotLinesLabel() {Text = "Encounter end"},
                    Width = 1,
                    DashStyle = DashStyles.Dash
                }
            };

            var plotLinesArray = plotLines.ToArray();

            var buffEventTimers = new Highcharts(string.Format("encounter{0}buffeventtimers", enc.Id))
                .InitChart(new Chart()
                {
                    DefaultSeriesType = ChartTypes.Columnrange,
                    Inverted = true,
                    Height = chartHeight,
                    BackgroundColor = new BackColorOrGradient(new Gradient
                    {
                        LinearGradient = new[] { 0, 0, 0, 400 },
                        Stops = new object[,]
                        {{ 0, Color.FromArgb(13, 255, 255, 255) },
                        { 1, Color.FromArgb(13, 255, 255, 255) }}
                    }),
                    Style = ChartColors.WhiteTextStyle
                })
                .SetCredits(new Credits()
                {
                    Enabled = false,
                    Href = "#",
                    Text = "Hewi@Laethys"
                })
                .SetTitle(new Title { Text = "Showing all buffs", Style = ChartColors.WhiteTextStyle })
                .SetXAxis(new XAxis()
                {
                    Categories = buffTimers.Keys.ToArray(),
                    LineColor = Color.White,
                    TickColor = Color.White,
                    Labels = new XAxisLabels { Style = ChartColors.WhiteTextStyle },
                })
                .SetTooltip(new Tooltip { ValueSuffix = " seconds" })
                .SetYAxis(new YAxis()
                {
                    Title = new YAxisTitle() { Text = "Seconds Elapsed", Style = ChartColors.WhiteTextStyle },
                    Min = 0,
                    Max = enc.Duration.TotalSeconds,
                    TickColor = Color.White,
                    LineColor = Color.White,
                    PlotLines = plotLinesArray,
                    Labels = new YAxisLabels { Style = ChartColors.WhiteTextStyle }
                })
                .SetLegend(new Legend() { Enabled = false })
                .SetSeries(mainSeriesList.ToArray())
                .SetExporting(new Exporting { Enabled = false })
                .SetPlotOptions(new PlotOptions
                {
                    Columnrange = new PlotOptionsColumnrange
                    {
                        DataLabels = new PlotOptionsColumnrangeDataLabels
                        {
                            Enabled = false
                            //Formatter = "function () { return this.y + '°C'; }"
                        },
                        Grouping = false,
                        BorderWidth = 0
                    }
                });
            #endregion

            var model = new CharacterBuffs()
            {
                Encounter = enc,
                Graph = buffEventTimers,
                TargetName = targetName,
                DebugBuildTime = new List<string>(),
                TimeZoneId = timezone
            };
            swBuildTime.Stop();
            model.BuildTime = swBuildTime.Elapsed;
            if (showdebugtimers)
            {
                model.DebugBuildTime = debugTimers;
            }
            return View(model);
        }

        public async Task<ActionResult> Debuffs(int? id, string target, bool showdebugtimers = false)
        {
            #region Checks / Validation
            // Check that an ID has been set
            if (id == null || string.IsNullOrEmpty(target))
            {
                return View("InvalidResource", model: "target");
            }

            // Check if there is an encounter that matches this ID
            var enc = _encounterRepository.Get((int)id);
            if (enc == null)
            {
                return View("InvalidResource", model: "encounter");
            }

            string timezone = "UTC";
            if (Request.IsAuthenticated)
            {
                var user = _authRepository.GetUserAccount(User.Identity.GetUserId());
                if (user == null)
                {
                    // Logged-in user isn't valid
                    HttpContext.GetOwinContext().Authentication.SignOut();
                    return RedirectToAction("Index", "Home");
                }
                timezone = user.TimeZone;
            }
            if (!await VisibleToUser(enc, User.Identity.GetUserId()))
            {
                return View("_Private");
            }
            #endregion

            List<string> debugTimers = new List<string>();

            Stopwatch swIndividualTimer = new Stopwatch();
            Stopwatch swBuildTime = new Stopwatch();
            swBuildTime.Start();
            swIndividualTimer.Start();

            // Check players first
            var targetName = _playerRepository.GetTargetNameFromLogId(target);
            if (string.IsNullOrEmpty(targetName))
            {
                // Check NPCs
                targetName = _encounterRepository.GetNpcName((int)id, target);
            }

            if (string.IsNullOrEmpty(targetName))
            {
                _logger.Debug(string.Format("Couldn't find the name of the target with the log ID of {0}!", target));
                var noBuffsModel = new CharacterBuffs()
                {
                    Encounter = enc,
                    Graph = null,
                    TargetName = string.Format("Target ID {0}", target),
                    DebugBuildTime = new List<string>(),
                    TimeZoneId = timezone
                };
                swBuildTime.Stop();
                noBuffsModel.BuildTime = swBuildTime.Elapsed;
                if (showdebugtimers)
                {
                    noBuffsModel.DebugBuildTime = debugTimers;
                }
                return View(noBuffsModel);
            }

            var characterBuffs = _encounterRepository.GetCharacterDebuffs(enc.Id, target);
            swIndividualTimer.Stop();
            debugTimers.Add(string.Format("GetCharacterDebuffs completed in {0}", swIndividualTimer.Elapsed));
            //var player = _playerRepository.Get(playerId);

            //var playerRoles = _encounterRepository.GetPlayerRoles(enc.Id);

            #region Buff timer graph
            var buffTimers = new Dictionary<string, List<BuffUpDown>>();
            var heightCalcArrays = 0;
            foreach (var buffGroup in characterBuffs.GroupBy(b => new { b.DebuffName, b.SourceName }))
            {
                heightCalcArrays += 1;
                // If, for some reason, a buff goes down after the encounter has finished, fix up the timer
                foreach (var buffEventItem in buffGroup)
                {
                    if (buffEventItem.SecondDebuffWentDown > enc.Duration.TotalSeconds)
                    {
                        buffEventItem.SecondDebuffWentDown = (int)enc.Duration.TotalSeconds;
                    }
                }

                // Loop through events to get the total uptime
                int secondsUp = 0;
                foreach (var buffItem in buffGroup)
                {
                    int durationUp = buffItem.SecondDebuffWentDown - buffItem.SecondDebuffWentUp;
                    secondsUp += durationUp;
                }
                buffGroup.ForEach(b => secondsUp += (b.SecondDebuffWentDown - b.SecondDebuffWentDown));

                string buffName = string.Format("{0} ({1}) - {2} uptime",
                    buffGroup.Key.DebuffName.Replace("'", "\\\'"), buffGroup.Key.SourceName,
                    ((decimal)secondsUp / (decimal)enc.Duration.TotalSeconds).ToString("#.##%"));
                foreach (var buffEventItem in buffGroup)
                {
                    var thisBuffUpDown = new BuffUpDown()
                    {
                        SecondUp = buffEventItem.SecondDebuffWentUp,
                        SecondDown = buffEventItem.SecondDebuffWentDown
                    };

                    if (!buffTimers.ContainsKey(buffName))
                    {
                        buffTimers.Add(buffName, new List<BuffUpDown>() { thisBuffUpDown });
                    }
                    else
                    {
                        buffTimers[buffName].Add(thisBuffUpDown);
                    }
                }
            }

            var chartHeight = (30 * heightCalcArrays) + 100;

            int seriesCount = buffTimers.Max(b => b.Value.Count);
            var seriesDictionary = new Dictionary<string, Dictionary<string, object[]>>();
            //var seriesColourDict = new Dictionary<string, Color>();
            for (int i = 1; i <= seriesCount; i++)
            {
                string seriesName = string.Format("#{0}", i);
                seriesDictionary.Add(seriesName, new Dictionary<string, object[]>());

                foreach (var kvp in buffTimers)
                {
                    if (i > kvp.Value.Count)
                    {
                        seriesDictionary[seriesName].Add(kvp.Key, new object[] { 0, 0 });
                    }
                    else
                    {
                        seriesDictionary[seriesName].Add(kvp.Key,
                            new object[]
                            {
                                kvp.Value[i - 1].SecondUp,
                                kvp.Value[i - 1].SecondDown
                            });
                    }
                    //if (!seriesColourDict.ContainsKey(seriesName))
                    //{
                    //    var playerName = kvp.Key.Substring(kvp.Key.IndexOf('(') + 1, kvp.Key.IndexOf(')') - kvp.Key.IndexOf('(') - 1);
                    //    var playerRole = playerRoles.FirstOrDefault(r => r.Name == playerName);
                    //    if (playerRole == null)
                    //    {
                    //        seriesColourDict.Add(seriesName, Color.Black);
                    //    }
                    //    else
                    //    {
                    //        if (playerRole.Role == "Healing")
                    //        {
                    //            seriesColourDict.Add(seriesName, Color.Green);
                    //        }
                    //        else if (playerRole.Role == "Damage")
                    //        {
                    //            seriesColourDict.Add(seriesName, Color.Red);
                    //        }
                    //        else if (playerRole.Role == "Support")
                    //        {
                    //            seriesColourDict.Add(seriesName, Color.DarkOrchid);
                    //        }
                    //        else
                    //        {
                    //            seriesColourDict.Add(seriesName, Color.CornflowerBlue);
                    //        }
                    //    }
                    //}
                }
            }
            var mainSeriesList = new List<Series>();
            foreach (var kvp in seriesDictionary)
            {
                var objArray = kvp.Value.Values.ToArray();
                //var firstKey = kvp.Value.First().Key;
                //var playerName = firstKey.Substring(firstKey.IndexOf('(') + 1, firstKey.IndexOf(')') - firstKey.IndexOf('(') - 1);
                //var role = playerRoles.First(r => r.Name == )

                string num = kvp.Key.Replace("#", "");
                int numKey = int.Parse(num);

                mainSeriesList.Add(
                    new Series()
                    {
                        Name = kvp.Key,
                        Color = ColorTranslator.FromHtml("#7cb5ec"),
                        //Color = seriesColourDict[kvp.Key],
                        //Color = numKey % 2 == 0 ? Color.Red : Color.Black,
                        Data = new Data(objArray)
                    });
            }

            var plotLines = new List<YAxisPlotLines>
            {
                new YAxisPlotLines()
                {
                    Color = Color.Green,
                    Value = enc.Duration.TotalSeconds,
                    Label = new YAxisPlotLinesLabel() {Text = "Encounter end"},
                    Width = 1,
                    DashStyle = DashStyles.Dash
                }
            };

            //var deaths = _encounterRepository.GetDeaths(enc.Id);
            //if (deaths.Any())
            //{
            //    plotLines.AddRange(deaths.Where(d => d.TargetPlayerId == playerId).Select(playerDeath => new YAxisPlotLines()
            //    {
            //        Color = Color.Red,
            //        Value = playerDeath.SecondsElapsed,
            //        Label = new YAxisPlotLinesLabel() { Text = "Player death" },
            //        Width = 1,
            //        DashStyle = DashStyles.Solid
            //    }));
            //}

            var plotLinesArray = plotLines.ToArray();

            var buffEventTimers = new Highcharts(string.Format("encounter{0}debuffeventtimers", enc.Id))
                .InitChart(new Chart()
                {
                    DefaultSeriesType = ChartTypes.Columnrange,
                    Inverted = true,
                    Height = chartHeight,
                    BackgroundColor = new BackColorOrGradient(new Gradient
                    {
                        LinearGradient = new[] { 0, 0, 0, 400 },
                        Stops = new object[,]
                        {{ 0, Color.FromArgb(13, 255, 255, 255) },
                        { 1, Color.FromArgb(13, 255, 255, 255) }}
                    }),
                    Style = ChartColors.WhiteTextStyle
                })
                .SetCredits(new Credits()
                {
                    Enabled = false,
                    Href = "#",
                    Text = "Hewi@Laethys"
                })
                .SetTitle(new Title { Text = "Showing all debuffs", Style = ChartColors.WhiteTextStyle })
                .SetXAxis(new XAxis()
                {
                    Categories = buffTimers.Keys.ToArray(),
                    LineColor = Color.White,
                    TickColor = Color.White,
                    Labels = new XAxisLabels { Style = ChartColors.WhiteTextStyle }
                })
                .SetTooltip(new Tooltip { ValueSuffix = " seconds" })
                .SetYAxis(new YAxis()
                {
                    Title = new YAxisTitle() { Text = "Seconds Elapsed", Style = ChartColors.WhiteTextStyle },
                    Min = 0,
                    Max = enc.Duration.TotalSeconds,
                    TickColor = Color.White,
                    LineColor = Color.White,
                    PlotLines = plotLinesArray,
                    Labels = new YAxisLabels { Style = ChartColors.WhiteTextStyle }
                })
                .SetLegend(new Legend() { Enabled = false })
                .SetSeries(mainSeriesList.ToArray())
                .SetExporting(new Exporting { Enabled = false })
                .SetPlotOptions(new PlotOptions
                {
                    Columnrange = new PlotOptionsColumnrange
                    {
                        DataLabels = new PlotOptionsColumnrangeDataLabels
                        {
                            Enabled = false,
                            //Formatter = "function () { return this.y + '°C'; }"
                        },
                        Grouping = false,
                        BorderWidth = 0
                    }
                });
            #endregion

            var model = new CharacterBuffs()
            {
                Encounter = enc,
                Graph = buffEventTimers,
                TargetName = targetName,
                DebugBuildTime = new List<string>(),
                TimeZoneId = timezone
            };
            swBuildTime.Stop();
            model.BuildTime = swBuildTime.Elapsed;
            if (showdebugtimers)
            {
                model.DebugBuildTime = debugTimers;
            }
            return View(model);
        }

        // COMPARISON METHODS

        public async Task<ActionResult> ComparePlayerDamageDone(int? id, string p)
        {
            #region Checks / Validation
            // Check that an ID has been set
            if (id == null)
            {
                return View("InvalidResource", model: "encounter");
            }

            // Check if there is an encounter that matches this ID
            var enc = _encounterRepository.Get((int)id);
            if (enc == null)
            {
                return View("InvalidResource", model: "encounter");
            }
            var encSession = _sessionEncounterRepository.GetSessionForEncounter(enc.Id);
            string timezone = "UTC";
            if (Request.IsAuthenticated)
            {
                var user = _authRepository.GetUserAccount(User.Identity.GetUserId());
                if (user == null)
                {
                    // Logged-in user isn't valid
                    HttpContext.GetOwinContext().Authentication.SignOut();
                    return RedirectToAction("Index", "Home");
                }
                timezone = user.TimeZone;
            }
            if (!await VisibleToUser(enc, User.Identity.GetUserId()))
            {
                return View("_Private");
            }
            #endregion

            Stopwatch swBuildTime = new Stopwatch();
            swBuildTime.Start();

            var compareVm = new EncounterComparePlayersViewModel { Encounter = enc, Session = encSession };
            var playerIds = new List<int>();
            try
            {
                playerIds = p.Split('-').Select(int.Parse).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("An error occurred while splitting {0} into a list for comparison: {1}", p, ex.Message));
                return RedirectToAction("PlayerDamageDone", new { id });
            }

            var players = _playerRepository.GetByIds(playerIds);
            var records = _encounterRepository.GetDamageForEncounter((int)id, playerIds);

            foreach (var dmgGroup in records.GroupBy(r => r.SourcePlayerId))
            {
                var player = players.FirstOrDefault(pl => pl.Id == dmgGroup.Key);

                var dmgBreakdown = new AbilityBreakdown();
                #region Build overall breakdown by ability
                foreach (var dmgAbility in dmgGroup.GroupBy(d => d.Ability.Name))
                {
                    long critDamage = 0;
                    long hitDamage = 0;

                    int crits = dmgAbility.Count(d => d.CriticalHit);
                    int swings = dmgAbility.Count();
                    int hits = swings - crits;
                    decimal critrate = crits > 0 ? (((decimal)crits / (decimal)swings) * 100) : 0;

                    Models.Ability ability = new Models.Ability
                    {
                        Name = dmgAbility.Key,
                        IsPetAbility = !string.IsNullOrEmpty(dmgAbility.First().SourcePetName),
                        DamageType = dmgAbility.First().Ability.DamageType ?? "unknown",
                        AbilityId = dmgAbility.First().AbilityId,
                        Statistics = new AbilityStatistics
                        {
                            Swings = swings,
                            Hits = hits,
                            Crits = crits,
                            CritRate = critrate
                        },
                        IconPath = dmgAbility.First().Ability.Icon
                    };

                    foreach (var dmg in dmgAbility)
                    {
                        // IGNORE OVERKILL

                        if (dmg.CriticalHit)
                        {
                            #region Min Crit
                            if (ability.Statistics.MinCrit == 0 || dmg.TotalDamage < ability.Statistics.MinCrit)
                            {
                                ability.Statistics.MinCrit = dmg.TotalDamage - dmg.OverkillAmount;
                            }
                            #endregion
                            #region Max Crit
                            if (ability.Statistics.MaxCrit == 0 || dmg.TotalDamage > ability.Statistics.MaxCrit)
                            {
                                ability.Statistics.MaxCrit = dmg.TotalDamage - dmg.OverkillAmount;
                            }
                            #endregion

                            critDamage += dmg.TotalDamage - dmg.OverkillAmount;
                        }
                        else
                        {
                            #region Min Hit
                            if (ability.Statistics.MinHit == 0 || dmg.TotalDamage < ability.Statistics.MinHit)
                            {
                                ability.Statistics.MinHit = dmg.TotalDamage - dmg.OverkillAmount;
                            }
                            #endregion
                            #region Max Hit
                            if (ability.Statistics.MaxHit == 0 || dmg.TotalDamage > ability.Statistics.MaxHit)
                            {
                                ability.Statistics.MaxHit = dmg.TotalDamage - dmg.OverkillAmount;
                            }
                            #endregion

                            hitDamage += dmg.TotalDamage - dmg.OverkillAmount;
                        }
                    }

                    #region AverageHit
                    ability.Statistics.AverageHit = hits > 0 ? hitDamage / hits : 0;
                    #endregion
                    #region AverageCrit
                    ability.Statistics.AverageCrit = crits > 0 ? critDamage / crits : 0;
                    #endregion

                    ability.TotalDamage = critDamage + hitDamage;
                    ability.DamagePerSecond = enc.Duration.TotalSeconds > 0 ? ability.TotalDamage / (long)enc.Duration.TotalSeconds : 0;

                    dmgBreakdown.Abilities.Add(ability);
                }
                #endregion

                compareVm.PlayersToCompare.Add(new PlayerComparison()
                {
                    Player = player,
                    DamageBreakdown = dmgBreakdown
                });
            }

            compareVm.UpdateDetailedStats();

            var secondsList = new List<string>();
            for (int i = 0; i <= enc.Duration.TotalSeconds; i++)
            {
                secondsList.Add(i.ToString(CultureInfo.InvariantCulture));
            }
            var secondsArray = secondsList.ToArray();

            foreach (var ability in compareVm.AbilityDps)
            {
                var seriesList = new List<Series>();

                foreach (var playerAbility in records.Where(r => r.AbilityId == ability.AbilityId).GroupBy(r => r.SourcePlayerId))
                {
                    var player = players.FirstOrDefault(pl => pl.Id == playerAbility.Key);

                    var instantValues = new List<object>();

                    for (int i = 0; i <= enc.Duration.TotalSeconds; i++)
                    {
                        instantValues.Add(playerAbility.Where(d => d.SecondsElapsed == i).Sum(d => d.EffectiveDamage));
                    }

                    #region Create Chart
                    seriesList.Add(new Series
                    {
                        Name = player.Name,
                        Data = new Data(instantValues.ToArray()),
                        Type = ChartTypes.Line
                    });

                    #endregion
                }

                var seriesListArray = seriesList.ToArray();
                //var abilityChart = CreateChart(enc.Id, "Ability damage", secondsArray, "Damage Done", seriesListArray);

                ability.ComparisonChart = new Highcharts(string.Format("encounter{0}ability{1}", enc.Id, ability.AbilityId))
            .InitChart(new Chart
            {
                DefaultSeriesType = ChartTypes.Area,
                ZoomType = ZoomTypes.Xy,
                Height = 200,
                BackgroundColor = new BackColorOrGradient(new Gradient
                {
                    LinearGradient = new[] { 0, 0, 0, 400 },
                    Stops = new object[,]
                        {{ 0, Color.FromArgb(15, 255, 255, 255) },
                        { 1, Color.FromArgb(15, 255, 255, 255) }}
                }),
                Style = ChartColors.WhiteTextStyle
            })
            .SetCredits(ChartDefaults.Credits)
            // Name = kvp.Key.Replace("'", "\\\'"),
            .SetOptions(new GlobalOptions { Colors = ChartColors.ColorArray() })
            .SetTitle(new Title { Text = ability.Name.Replace("'", "\\\'"), Style = ChartColors.WhiteTextStyle })
            //.SetSubtitle(new Subtitle() { Text = graphSubtitle, Style = ChartColors.WhiteTextStyle })
            .SetXAxis(new XAxis
            {
                Categories = secondsArray,
                Title = ChartDefaults.XAxisTitle,
                LineColor = Color.White,
                TickColor = Color.White,
                Labels = new XAxisLabels { Style = ChartColors.WhiteTextStyle },
                TickmarkPlacement = Placement.On,
                TickPositioner = ChartDefaults.TickPositioner
            })
            .SetYAxis(new[] { new YAxis
                {
                    Title = new YAxisTitle {Text = "Damage Done", Style = ChartColors.WhiteTextStyle},
                    Min = 0, TickColor = Color.White, LineColor = Color.White, Labels = new YAxisLabels { Style = ChartColors.WhiteTextStyle }
                }})
            .SetPlotOptions(new PlotOptions { Area = ChartDefaults.AreaPlotOptions })
            .SetSeries(seriesListArray)
            .SetExporting(new Exporting { Enabled = false })
            .SetLegend(new Legend() { ItemStyle = ChartColors.WhiteTextStyle, ItemHoverStyle = "color: '#bbb'", Align = HorizontalAligns.Right, VerticalAlign = VerticalAligns.Middle, Layout = Layouts.Vertical });

                //ability.ComparisonChart = abilityChart;


                //var abilityRecords = records.Where(r => r.AbilityId == ability.AbilityId).OrderBy(r => r.SecondsElapsed).ToList();
                //var abilityRecords = records.Where(r => r.AbilityId == ability.AbilityId).GroupBy(r => r.SecondsElapsed);
                //foreach (var abilitySecond in abilityRecords)
                //{

                //}

            }

            swBuildTime.Stop();
            compareVm.BuildTime = swBuildTime.Elapsed;
            compareVm.TimeZoneId = timezone;
            compareVm.PageTitle = "Player Comparison";

            return View("ComparePlayers", compareVm);
        }
        public async Task<ActionResult> ComparePlayerHealingDone(int? id, string p)
        {
            #region Checks / Validation
            // Check that an ID has been set
            if (id == null)
            {
                return View("InvalidResource", model: "encounter");
            }

            // Check if there is an encounter that matches this ID
            var enc = _encounterRepository.Get((int)id);
            if (enc == null)
            {
                return View("InvalidResource", model: "encounter");
            }
            var encSession = _sessionEncounterRepository.GetSessionForEncounter(enc.Id);
            string timezone = "UTC";
            if (Request.IsAuthenticated)
            {
                var user = _authRepository.GetUserAccount(User.Identity.GetUserId());
                if (user == null)
                {
                    // Logged-in user isn't valid
                    HttpContext.GetOwinContext().Authentication.SignOut();
                    return RedirectToAction("Index", "Home");
                }
                timezone = user.TimeZone;
            }
            if (!await VisibleToUser(enc, User.Identity.GetUserId()))
            {
                return View("_Private");
            }
            #endregion

            Stopwatch swBuildTime = new Stopwatch();
            swBuildTime.Start();

            var compareVm = new EncounterComparePlayersViewModel { Encounter = enc, Session = encSession };
            var playerIds = new List<int>();
            try
            {
                playerIds = p.Split('-').Select(int.Parse).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("An error occurred while splitting {0} into a list for comparison: {1}", p, ex.Message));
                return RedirectToAction("PlayerHealingDone", new { id });
            }

            var players = _playerRepository.GetByIds(playerIds);
            var records = _encounterRepository.GetHealingForEncounter((int)id, playerIds);

            foreach (var healGroup in records.GroupBy(r => r.SourcePlayerId))
            {
                var player = players.FirstOrDefault(pl => pl.Id == healGroup.Key);

                var healBreakdown = new AbilityBreakdown();
                #region Build overall breakdown by ability
                foreach (var healAbility in healGroup.GroupBy(d => d.Ability.Name))
                {
                    long critHeal = 0;
                    long hitHeal = 0;

                    int crits = healAbility.Count(d => d.CriticalHit);
                    int swings = healAbility.Count();
                    int hits = swings - crits;
                    decimal critrate = crits > 0 ? (((decimal)crits / (decimal)swings) * 100) : 0;

                    Models.Ability ability = new Models.Ability
                    {
                        Name = healAbility.Key,
                        IsPetAbility = !string.IsNullOrEmpty(healAbility.First().SourcePetName),
                        AbilityId = healAbility.First().AbilityId,
                        Statistics = new AbilityStatistics
                        {
                            Swings = swings,
                            Hits = hits,
                            Crits = crits,
                            CritRate = critrate
                        },
                        IconPath = healAbility.First().Ability.Icon
                    };

                    foreach (var heal in healAbility)
                    {
                        if (heal.CriticalHit)
                        {
                            #region Min Crit
                            if (ability.Statistics.MinCrit == 0 || heal.EffectiveHealing < ability.Statistics.MinCrit)
                            {
                                ability.Statistics.MinCrit = heal.EffectiveHealing;
                            }
                            #endregion
                            #region Max Crit
                            if (ability.Statistics.MaxCrit == 0 || heal.EffectiveHealing > ability.Statistics.MaxCrit)
                            {
                                ability.Statistics.MaxCrit = heal.EffectiveHealing;
                            }
                            #endregion

                            critHeal += heal.EffectiveHealing;
                        }
                        else
                        {
                            #region Min Hit
                            if (ability.Statistics.MinHit == 0 || heal.EffectiveHealing < ability.Statistics.MinHit)
                            {
                                ability.Statistics.MinHit = heal.EffectiveHealing;
                            }
                            #endregion
                            #region Max Hit
                            if (ability.Statistics.MaxHit == 0 || heal.EffectiveHealing > ability.Statistics.MaxHit)
                            {
                                ability.Statistics.MaxHit = heal.EffectiveHealing;
                            }
                            #endregion

                            hitHeal += heal.EffectiveHealing;
                        }
                    }

                    #region AverageHit
                    ability.Statistics.AverageHit = hits > 0 ? hitHeal / hits : 0;
                    #endregion
                    #region AverageCrit
                    ability.Statistics.AverageCrit = crits > 0 ? critHeal / crits : 0;
                    #endregion

                    ability.TotalEffectiveHealing = critHeal + hitHeal;
                    ability.HealingPerSecond = enc.Duration.TotalSeconds > 0 ? ability.TotalEffectiveHealing / (long)enc.Duration.TotalSeconds : 0;

                    healBreakdown.Abilities.Add(ability);
                }
                #endregion

                compareVm.PlayersToCompare.Add(new PlayerComparison()
                {
                    Player = player,
                    HealingBreakdown = healBreakdown
                });
            }

            compareVm.UpdateDetailedStats();

            swBuildTime.Stop();
            compareVm.BuildTime = swBuildTime.Elapsed;
            compareVm.TimeZoneId = timezone;
            compareVm.PageTitle = "Player Comparison";

            return View("ComparePlayersHealing", compareVm);
        }
    }
}