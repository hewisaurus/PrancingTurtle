using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Database.Models;
using Database.Repositories.Interfaces;
using Microsoft.AspNet.Identity;
using PrancingTurtle.Helpers;
using PrancingTurtle.Models.Misc;
using PrancingTurtle.Models.ViewModels.Session;
using Common;
using Logging;

namespace PrancingTurtle.Controllers
{
    public class SessionController : Controller
    {
        private readonly ILogger _logger;

        private readonly ISessionRepository _sessionRepository;
        private readonly IAuthenticationRepository _authRepository;
        //private readonly IDapperRepository _mainRepository;
        private readonly IInstanceRepository _instanceRepository;
        private readonly IAuthUserCharacterRepository _authUserCharacterRepository;
        private readonly IGuildRepository _guildRepository;
        private readonly IPlayerRepository _playerRepository;
        private readonly IEncounterRepository _encounterRepository;
        private readonly IEncounterOverviewRepository _encounterOverviewRepository;
        private readonly ISessionLogRepository _sessionLogRepository;
        private readonly IBossFightRepository _bossFightRepository;
        private readonly IEncounterDifficultyRepository _difficultyRepository;

        private readonly ILeaderboardRepository _leaderboardRepository;

        public SessionController(ISessionRepository sessionRepository,
            IAuthenticationRepository authRepository, //IDapperRepository mainRepository,
            IAuthUserCharacterRepository authUserCharacterRepository,
            IGuildRepository guildRepository, ILogger logger,
            IEncounterRepository encounterRepository, IEncounterOverviewRepository encounterOverviewRepository,
            ISessionLogRepository sessionLogRepository, IPlayerRepository playerRepository,
            IInstanceRepository instanceRepository, IBossFightRepository bossFightRepository,
            IEncounterDifficultyRepository difficultyRepository, ILeaderboardRepository leaderboardRepository)
        {
            _sessionRepository = sessionRepository;
            _authRepository = authRepository;
            //_mainRepository = mainRepository;
            _authUserCharacterRepository = authUserCharacterRepository;
            _guildRepository = guildRepository;
            _logger = logger;
            _encounterRepository = encounterRepository;
            _encounterOverviewRepository = encounterOverviewRepository;
            _sessionLogRepository = sessionLogRepository;
            _playerRepository = playerRepository;
            _instanceRepository = instanceRepository;
            _bossFightRepository = bossFightRepository;
            _difficultyRepository = difficultyRepository;
            _leaderboardRepository = leaderboardRepository;
        }

        /// <summary>
        /// TESTING - CALENDAR
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public JsonResult TestGetEvents(double start, double end)
        {
            var fromDate = UnixTimeConversion.ConvertFromUnixTimestamp(start);
            var toDate = UnixTimeConversion.ConvertFromUnixTimestamp(end);
            var sessions = _sessionRepository.GetAll().Where(s => s.Date >= fromDate && s.Date <= toDate).ToList();
            //var jSessions = sessions.Select(x => new {id = x.Id, title=x.Name.Replace("\\","-"), start=x.Date, end = x.Date.Add(x.Duration), allDay = false});
            //var rows = jSessions.ToArray();
            //return Json(rows, JsonRequestBehavior.AllowGet);
            var eventList = from e in sessions
                            select new
                            {
                                id = e.Id,
                                title = e.Name,
                                start = e.Date.ToString(CultureInfo.InvariantCulture),
                                //end = e.Date.Add(e.Duration),
                                allDay = false,
                                url = Url.AbsoluteAction("Detail", "Session", new { @id = e.Id })
                            };
            var rows = eventList.ToArray();
            return Json(rows, JsonRequestBehavior.AllowGet);
        }

        // GET: Session
        public ActionResult Index()
        {
            // Don't want anyone to get a list of sessions - it could be enormous.
            // Alternatively, remove the redirect and just show the x most recent
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Detail(int? id, bool debug = false)
        {
            #region Validation
            if (id == null)
            {
                return RedirectToAction("InvalidSession");
            }
            var sessionId = (int)id;
            var session = _sessionRepository.Get(sessionId);
            if (session == null)
            {
                return RedirectToAction("InvalidSession");
            }
            #endregion
            var encounters = _sessionRepository.GetEncounters(sessionId, false);
            var log = _sessionLogRepository.GetFirstSessionLogForSession(sessionId);
            var uploaders = _sessionLogRepository.GetUploadersForSession(sessionId);

            var model = new SessionDetailVM()
            {
                FirstLog = log,
                Session = session,
                Encounters = encounters,
                CanBeRemoved = false,
                DebugMode = debug,
                Uploaders = uploaders,
                OriginalUploaderGuildId = 0
            };

            Guild uploaderGuild = _guildRepository.Get(session.AuthUserCharacter.GuildId);
            model.OriginalUploaderGuildId = uploaderGuild.Id;
            if (!Request.IsAuthenticated && uploaderGuild.HideSessions)
            {
                return View("Private", model: "session");
            }

            bool sameGuild = false;
            bool canModifyPrivacy = false;

            if (Request.IsAuthenticated)
            {
                // Check we have a valid user
                var user = _authRepository.GetUserAccount(User.Identity.GetUserId());
                if (user == null)
                {
                    HttpContext.GetOwinContext().Authentication.SignOut();
                    return RedirectToAction("Index", "Home");
                }
                model.TimeZoneId = user.TimeZone;

                // Get the characters for this user
                var userCharacters = _authUserCharacterRepository.GetCharacters(user.Email);
                if (userCharacters.Any(c => c.GuildId == uploaderGuild.Id))
                {
                    sameGuild = true;
                    // Update the ranks from the DB
                    foreach (var character in userCharacters)
                    {
                        if (character.GuildId != null)
                        {
                            character.GuildRank = _authUserCharacterRepository.GetGuildRankForCharacter(character.Id);
                        }
                    }
                    // This user has one or more characters in this guild, so check if they are the original uploader or have a rank that allows modifications
                    if (userCharacters.Where(c => c.GuildId == uploaderGuild.Id).Any(userChar => userChar.GuildRank.CanModifyAnySession || userChar.Id == session.AuthUserCharacterId))
                    {
                        model.CanBeRenamed = true;
                        model.CanBeRemoved = true;
                        canModifyPrivacy = true;
                    }
                }
                else
                {
                    // This user doesn't have a character in the guild
                    if (uploaderGuild.HideSessions)
                    {
                        // This user doesn't have a character in this guild, sessions are hidden
                        if (!User.IsInRole(UserGroups.Admin))
                        {
                            return View("Private", model: "session");
                        }
                    }
                }

                if (User.IsInRole(UserGroups.Admin))
                {
                    model.CanBeReimported = true;
                    model.CanBeRenamed = true;
                    model.CanBeRemoved = true;
                    sameGuild = true;
                }
            }

            // Loop through the encounters and set UI-specific values (that aren't in the DB)
            foreach (var encounter in encounters)
            {
                //encounter.AverageDps = _mainRepository.GetAverageDpsFromEncounterOverview(encounter.Id);
                if (encounter.Overview == null)
                {
                    encounter.AverageDps = _encounterRepository.GetEncounterDps(encounter.Id,
                        (int)encounter.Duration.TotalSeconds);
                    encounter.AverageHps = _encounterRepository.GetEncounterHps(encounter.Id,
                        (int)encounter.Duration.TotalSeconds);
                    encounter.AverageAps = _encounterRepository.GetEncounterAps(encounter.Id,
                        (int)encounter.Duration.TotalSeconds);
                    encounter.PlayerDeaths = _encounterRepository.GetTotalPlayerDeaths(encounter.Id);

                    // Update these overviews
                    _encounterOverviewRepository.Add(new EncounterOverview
                    {
                        AverageDps = encounter.AverageDps,
                        AverageHps = encounter.AverageHps,
                        AverageAps = encounter.AverageAps,
                        PlayerDeaths = encounter.PlayerDeaths,
                        EncounterId = encounter.Id
                    });
                }
                else
                {
                    encounter.AverageDps = encounter.Overview.AverageDps;
                    encounter.AverageHps = encounter.Overview.AverageHps;
                    encounter.AverageAps = encounter.Overview.AverageAps;
                    encounter.PlayerDeaths = encounter.Overview.PlayerDeaths;
                }
                encounter.IsViewable = sameGuild || encounter.IsPublic;
                encounter.CanModifyPrivacy = canModifyPrivacy;
            }

            return View(model);
        }

        /// <summary>
        /// Fired when an administrator clicks the 'remove selected encounters' button on the session detail page
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult RemoveSelectedEncounters(FormCollection collection)
        {
            int sessionId = 0;
            var removeList = new List<int>();
            foreach (var key in collection)
            {
                var keyName = key.ToString();
                if (keyName == "Session.Id")
                {
                    sessionId = int.Parse(collection[keyName]);
                }
                else if (keyName.StartsWith("chkRemoveEncounter"))
                {
                    string chkVal = collection[keyName];
                    if (chkVal.Contains("true"))
                    {
                        int encId = int.Parse(keyName.Replace("chkRemoveEncounter", ""));
                        if (!removeList.Contains(encId))
                        {
                            removeList.Add(encId);
                        }
                    }
                }
            }

            if (sessionId == 0)
            {
                return View("InvalidResource", model: "session");
            }

            if (removeList.Any())
            {
                var removeEncounterVm = new RemoveEncountersVM
                {
                    SessionId = sessionId,
                    EncounterIds = new List<int>(removeList)
                };
                return View("RemoveEncounters", removeEncounterVm);
                //return RedirectToAction("RemoveEncounters", removeEncounterVm);
            }

            return RedirectToAction("Detail", new { @id = sessionId });
        }

        //[Authorize]
        //public ActionResult RemoveEncounters(RemoveEncountersVM model)
        //{
        //    // Check that this user is allowed to remove encounters for this session
        //    return View(model);
        //}

        [HttpPost]
        [Authorize]
        public ActionResult RemoveEncountersConfirmed(RemoveEncountersVM model)
        {
            if (!model.EncounterIds.Any())
            {
                return RedirectToAction("Detail", new { @id = model.SessionId });
            }
            // Double-check that this user is allowed to remove encounters for this session
            // 3 possible cases: GlobalAdmin, Guild Administrator or Original Uploader
            // Check we have a valid user
            var user = _authRepository.GetUserAccount(User.Identity.GetUserId());
            if (user == null)
            {
                HttpContext.GetOwinContext().Authentication.SignOut();
                return RedirectToAction("Index", "Home");
            }
            var session = _sessionRepository.Get(model.SessionId);
            if (session == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var uploaderGuild = _guildRepository.Get(session.AuthUserCharacter.GuildId);
            bool canRemove = false;

            // Get the characters for this user
            var userCharacters = _authUserCharacterRepository.GetCharacters(user.Email);
            if (userCharacters.Any(c => c.GuildId == uploaderGuild.Id))
            {
                // Update the ranks from the DB
                foreach (var character in userCharacters)
                {
                    if (character.GuildId != null)
                    {
                        character.GuildRank = _authUserCharacterRepository.GetGuildRankForCharacter(character.Id);
                    }
                }
                // This user has one or more characters in this guild, so check if they are the original uploader or have a rank that allows modifications
                if (userCharacters.Where(c => c.GuildId == uploaderGuild.Id).Any(userChar => userChar.GuildRank.CanModifyAnySession || userChar.Id == session.AuthUserCharacterId))
                {
                    canRemove = true;
                    _logger.Debug(string.Format("Allowing {0} to remove 1 or more encounters from session {1} as they are the original uploader or a guild admin", 
                        User.Identity.GetUserId(), model.SessionId));
                }
            }

            if (User.IsInRole(UserGroups.Admin))
            {
                canRemove = true;
                _logger.Debug(string.Format("Allowing {0} to remove {2} {3} from session {1} as they are a site administrator",
                    User.Identity.GetUserId(), model.SessionId, model.EncounterIds.Count, model.EncounterIds.Count == 1 ? "encounter" : "encounters"));
            }

            if (!canRemove)
            {
                _logger.Error(string.Format("Denying {0} from removing {2} {3} from session {1} as they do not have permission",
                    User.Identity.GetUserId(), model.SessionId, model.EncounterIds.Count, model.EncounterIds.Count == 1 ? "encounter" : "encounters"));
                TempData.Add("flash", new FlashDangerViewModel("You don't have the correct permissions to do that!"));
                return RedirectToAction("Detail", new { @id = model.SessionId });
            }

            var result = _encounterRepository.MarkEncountersForDeletion(model.EncounterIds, User.Identity.GetUserId());
            if (result.Success)
            {
                TempData.Add("flash",
                    model.EncounterIds.Count == 1
                        ? new FlashSuccessViewModel("1 encounter was successfully removed.")
                        : new FlashSuccessViewModel(string.Format("{0} encounters were successfully removed.",
                            model.EncounterIds.Count)));
            }
            else
            {
                TempData.Add("flash", new FlashDangerViewModel(string.Format("Error removing encounter(s): {0}", result.Message)));
            }
            return RedirectToAction("Detail", new { @id = model.SessionId });
        }

        // TODO: THIS NEEDS A REVAMP
        // The longer the player exists, the bigger this page gets. Needs some form of paging or restriction
        public ActionResult Player(int id = -1)
        {
            if (id == -1)
            {
                return View("InvalidResource", model: "player");
            }

            var player = _playerRepository.GetSingleFromPlayerId(id);
            if (player == null)
            {
                return View("InvalidResource", model: "player");
            }
            bool returnAll = false;
            if (Request.IsAuthenticated)
            {
                if (User.IsInRole(UserGroups.Admin))
                {
                    returnAll = true;
                }
            }

            var model = new SessionPlayerVM()
            {
                Player = player,
                Sessions = _sessionRepository.GetPlayerSessions(id, User.Identity.GetUserId())
            };

            if (Request.IsAuthenticated)
            {
                var user = _authRepository.GetUserAccount(User.Identity.GetUserId());
                if (user == null)
                {
                    HttpContext.GetOwinContext().Authentication.SignOut();
                    return RedirectToAction("Index", "Home");
                }
                model.TimeZoneId = user.TimeZone;
            }

            foreach (var session in model.Sessions)
            {
                session.InstancesSeen = _sessionRepository.GetInstancesSeen(session.Id);
                session.BossesSeen = _sessionRepository.GetBossesKilled(session.Id);
                session.BossesSeenNotKilled = _sessionRepository.GetBossesSeenButNotKilled(session.Id);
            }
            return View(model);
        }

        // TODO: THIS NEEDS A REVAMP
        // The longer the instance exists, the bigger this page gets. Needs some form of paging or restriction
        public ActionResult Instance(int id = -1)
        {
            if (id == -1)
            {
                return View("InvalidResource", model: "instance");
            }

            var instance = _instanceRepository.Get(id);
            if (instance == null)
            {
                return View("InvalidResource", model: "instance");
            }
            bool returnAll = false;
            if (Request.IsAuthenticated)
            {
                if (User.IsInRole(UserGroups.Admin))
                {
                    returnAll = true;
                }
            }
            var model = new SessionInstanceVM
            {
                Instance = instance,
                Sessions = returnAll ? _sessionRepository.GetAllInstanceSessions(id) : _sessionRepository.GetInstanceSessions(id, User.Identity.GetUserId())
            };

            foreach (var guildSession in model.Sessions)
            {
                guildSession.BossesSeen = _sessionRepository.GetBossesKilled(guildSession.Id);
                guildSession.BossesSeenNotKilled = _sessionRepository.GetBossesSeenButNotKilled(guildSession.Id);
            }
            return View(model);
        }

        public ActionResult BossFight(int id = -1, int o = 0, int d = -1)
        {
            if (id == -1)
            {
                return View("InvalidResource", model: "bossfight");
            }

            var difficulty = _difficultyRepository.GetDefaultDifficulty();
            if (difficulty == null)
            {
                return View("InvalidResource", model: "encounter difficulty");
            }

            if (d != -1)
            {
                var overrideDifficulty = _difficultyRepository.Get(d);
                if (overrideDifficulty != null)
                {
                    difficulty = overrideDifficulty;
                }
            }

            var bossFight = _bossFightRepository.Get(id);
            if (bossFight == null)
            {
                return View("InvalidResource", model: "bossfight");
            }
            bool returnAll = false;
            if (Request.IsAuthenticated)
            {
                if (User.IsInRole(UserGroups.Admin))
                {
                    returnAll = true;
                }
            }

            var dbSessions = returnAll
                ? _sessionRepository.GetAllBossFightSessions(id, difficulty.Id)
                : _sessionRepository.GetBossFightSessions(id, User.Identity.GetUserId(), difficulty.Id);

            if (o == 0)
            {
                dbSessions = dbSessions.Where(s => s.Date >= DateTime.Today.Date.Subtract(new TimeSpan(14, 0, 0, 0))).Take(10).ToList();
            }
            
            var leaderboards = _leaderboardRepository.GetLeaderboards_v2(id, 10, difficulty.Id);
            var model = new SessionBossFightVM
            {
                BossFight = bossFight,
                Sessions = dbSessions,
                Difficulty = difficulty,
                TopSingleTargetDps = leaderboards.TopSingleTargetDps,
                Top10WarriorAps = leaderboards.WarriorTopAps,
                Top10WarriorDps = leaderboards.WarriorTopDps,
                Top10WarriorHps = leaderboards.WarriorTopHps,
                Top10MageAps = leaderboards.MageTopAps,
                Top10MageDps = leaderboards.MageTopDps,
                Top10MageHps = leaderboards.MageTopHps,
                Top10ClericAps = leaderboards.ClericTopAps,
                Top10ClericDps = leaderboards.ClericTopDps,
                Top10ClericHps = leaderboards.ClericTopHps,
                Top10RogueAps = leaderboards.RogueTopAps,
                Top10RogueDps = leaderboards.RogueTopDps,
                Top10RogueHps = leaderboards.RogueTopHps,
                Top10PrimalistAps = leaderboards.PrimalistTopAps,
                Top10PrimalistDps = leaderboards.PrimalistTopDps,
                Top10PrimalistHps = leaderboards.PrimalistTopHps,
                FastestKills = leaderboards.FastestKills,
                Top10SingleTargetWarriorDps = leaderboards.WarriorTopSingleTarget,
                Top10SingleTargetMageDps = leaderboards.MageTopSingleTarget,
                Top10SingleTargetClericDps = leaderboards.ClericTopSingleTarget,
                Top10SingleTargetRogueDps = leaderboards.RogueTopSingleTarget,
                Top10SingleTargetPrimalistDps = leaderboards.PrimalistTopSingleTarget,
                TopAps = leaderboards.TopAps,
                TopDps = leaderboards.TopDps,
                TopHps = leaderboards.TopHps,
                TopSupportDps = leaderboards.TopSupportDps,
                WarriorTopSupportDps = leaderboards.WarriorTopSupportDps,
                MageTopSupportDps = leaderboards.MageTopSupportDps,
                ClericTopSupportDps = leaderboards.ClericTopSupportDps,
                RogueTopSupportDps = leaderboards.RogueTopSupportDps,
                PrimalistTopSupportDps = leaderboards.PrimalistTopSupportDps,
                TopBurstDamage1S = leaderboards.TopBurstDamage1S
            };

            model.SingleFastestKill = model.FastestKills.FirstOrDefault();
            model.SingleTopDps = model.Top10Dps.FirstOrDefault();
            model.SingleTopHps = model.Top10Hps.FirstOrDefault();
            model.SingleTopAps = model.Top10Aps.FirstOrDefault();
            
            foreach (var guildSession in model.Sessions)
            {
                guildSession.BossesSeen = _sessionRepository.GetBossesKilled(guildSession.Id);
                guildSession.BossesSeenNotKilled = _sessionRepository.GetBossesSeenButNotKilled(guildSession.Id);
            }

            if (System.IO.File.Exists(Server.MapPath(string.Format("~/Content/images/portrait/{0}.png", bossFight.PortraitFilename))))
            {
                model.LoadImage = true;
            }
            return View(model);
        }

        public ActionResult Guild(int? id)
        {
            if (id == null)
            {
                return View("InvalidResource", model: "guild");
            }
            var guild = _guildRepository.Get((int)id);
            if (guild == null)
            {
                return View("InvalidResource", model: "guild");
            }
            var model = new SessionGuildVM()
            {
                Guild = guild,
                Sessions = _sessionRepository.GetGuildSessions(guild.Id)
            };

            if (Request.IsAuthenticated)
            {
                var user = _authRepository.GetUserAccount(User.Identity.GetUserId());
                if (user == null)
                {
                    HttpContext.GetOwinContext().Authentication.SignOut();
                    return RedirectToAction("Index", "Home");
                }
                model.TimeZoneId = user.TimeZone;

                var guildMembers = _authUserCharacterRepository.GetGuildMembers(guild.Id);
                if (!guildMembers.Any(gm => gm.AuthUserId == user.Id))
                {
                    // This user isn't a member of this guild
                    if (guild.HideSessions)
                    {
                        if (!User.IsInRole(UserGroups.Admin))
                        {
                            // This user is logged in, but isn't a member of this guild which has its sessions set to private
                            return View("Private", model: "session list");
                        }
                    }
                }
            }
            else
            {
                if (guild.HideSessions)
                {
                    // This user isn't logged in, and this guild has its sessions set to private
                    return View("Private", model: "session list");
                }
            }

            foreach (var session in model.Sessions)
            {
                session.InstancesSeen = _sessionRepository.GetInstancesSeen(session.Id);
                session.BossesSeen = _sessionRepository.GetBossesKilled(session.Id);
                session.BossesSeenNotKilled = _sessionRepository.GetBossesSeenButNotKilled(session.Id);
            }
            return View(model);
        }

        public ActionResult CantUpload()
        {
            return View("~/Views/Shared/CantUpload.cshtml");
        }

        public ActionResult InvalidSession()
        {
            return View("~/Views/Shared/InvalidResource.cshtml", null, "Session");
        }

        [Authorize]
        public ActionResult Upload()
        {
            // Check we have a valid user
            var user = _authRepository.GetUserAccount(User.Identity.GetUserId());
            if (user == null)
            {
                HttpContext.GetOwinContext().Authentication.SignOut();
                return RedirectToAction("Index", "Home");
            }

            // Check if the user is allowed to upload
            if (!_authUserCharacterRepository.UserCanUpload(user.Email))
            {
                return RedirectToAction("CantUpload");
            }

            return View();
        }

        [Authorize]
        public ActionResult UploadPart1()
        {
            // Check we have a valid user
            var user = _authRepository.GetUserAccount(User.Identity.GetUserId());
            if (user == null)
            {
                HttpContext.GetOwinContext().Authentication.SignOut();
                return RedirectToAction("Index", "Home");
            }

            // Check if the user is allowed to upload
            if (!_authUserCharacterRepository.UserCanUpload(user.Email))
            {
                return RedirectToAction("CantUpload");
            }

            var userCharacters = _authUserCharacterRepository.GetUploaders(user.Email);

            // Allow site admins to upload parses as any user
            var model = new UploadSessionVM()
            {
                Characters = userCharacters,
                Public = true,
                SessionDate = DateTime.Now
            };

            return View(model);
        }

        [Authorize]
        public ActionResult VerifyExistingSession()
        {
            // Check we have a valid user
            var user = _authRepository.GetUserAccount(User.Identity.GetUserId());
            if (user == null)
            {
                HttpContext.GetOwinContext().Authentication.SignOut();
                return RedirectToAction("Index", "Home");
            }

            // Check if the user is allowed to upload
            if (!_authUserCharacterRepository.UserCanUpload(user.Email))
            {
                return RedirectToAction("CantUpload");
            }

            return View();
        }

        [HttpPost]
        [Authorize]
        public ActionResult VerifyExistingSession(ValidateExistingSession model)
        {
            if (ModelState.IsValid)
            {

            }
            return View();
        }

        [Authorize]
        public ActionResult UploadPart3()
        {
            // Check we have a valid user
            var user = _authRepository.GetUserAccount(User.Identity.GetUserId());
            if (user == null)
            {
                HttpContext.GetOwinContext().Authentication.SignOut();
                return RedirectToAction("Index", "Home");
            }

            // Check if the user is allowed to upload
            if (!_authUserCharacterRepository.UserCanUpload(user.Email))
            {
                return RedirectToAction("CantUpload");
            }

            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult UploadPart1(UploadSessionVM model)
        {
            // Check we have a valid user
            var user = _authRepository.GetUserAccount(User.Identity.GetUserId());
            if (user == null)
            {
                HttpContext.GetOwinContext().Authentication.SignOut();
                return RedirectToAction("Index", "Home");
            }

            if (ModelState.IsValid)
            {
                // Generate an upload token and make sure it's not in use
                while (true)
                {
                    model.UploadToken = AuthEncryption.RandomFilename();

                    _logger.Debug(string.Format("Checking to see if session token {0} has been used before", model.UploadToken));
                    if (!_sessionLogRepository.SessionLogTokenExists(model.UploadToken))
                    {
                        _logger.Debug("Token wasn't found, so inserting a new session with it now.");
                        break;
                    }

                    _logger.Debug(string.Format("Found an existing log with the token {0} - generating a new one.", model.UploadToken));
                }

                TimeSpan utcOffset = TimeZoneInfo.FindSystemTimeZoneById(user.TimeZone).GetUtcOffset(DateTime.UtcNow);

                var result = _sessionRepository.CreateSession(User.Identity.GetUserId(), new Session()
                {
                    AuthUserCharacterId = model.UploadCharacterId,
                    Date = model.SessionDate.Subtract(utcOffset),
                    Name = model.Name,
                    EncountersPublic = model.Public
                });
                if (result.Success)
                {
                    int sessionId = int.Parse(result.Message);
                    model.UploadedSessionId = sessionId;
                    // Add this session to the SessionLog table in case other guild members or this user choose to upload additional logs to the session
                    var guildId = _authUserCharacterRepository.GetGuildIdForCharacter(model.UploadCharacterId);
                    if (guildId > 0)
                    {
                        var newSessionLog = new SessionLog()
                        {
                            AuthUserCharacterId = model.UploadCharacterId,
                            Filename = model.UploadToken + ".zip",
                            Token = model.UploadToken,
                            GuildId = guildId,
                            LogSize = 0,
                            SessionId = sessionId,
                            TotalPlayedTime = 0
                        };
                        var returnValue = _sessionLogRepository.Create(newSessionLog);
                    }
                    return UploadPart2(model);
                }

                ModelState.AddModelError("", result.Message);
            }

            //var user = _authRepository.GetUserAccount(User.Identity.GetUserId());
            var userCharacters = _authUserCharacterRepository.GetUploaders(user.Email);
            model.Characters = userCharacters;

            return View(model);
        }

        [Authorize]
        public ActionResult UploadPart2(UploadSessionVM model)
        {
            if (model == null) return RedirectToAction("Index", "Home");
            if (model.Name == null) return RedirectToAction("Index", "Home");

            return View("UploadPart2", model);
        }

        public ActionResult FTP()
        {
            return View();
        }

        public ActionResult UploadSession()
        {
            var file = Request.Files["Filedata"];
            string filename = file.FileName;
            _logger.Debug(string.Format("Incoming session: {0}", filename));
            try
            {
                string savePath = Server.MapPath(@"/UploadedFiles/" + filename);
                _logger.Debug(string.Format("Attempting to save to {0}", savePath));
                file.SaveAs(savePath);
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Encountered an error while uploading {0}: {1}", filename, ex.Message));
            }

            return Content(Url.Content(@"~/UploadedFiles/" + filename));
        }

        [Authorize]
        public ActionResult RemoveSession(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Home");
            }
            // Check we have a valid user
            var user = _authRepository.GetUserAccount(User.Identity.GetUserId());
            if (user == null)
            {
                HttpContext.GetOwinContext().Authentication.SignOut();
                return RedirectToAction("Index", "Home");
            }
            var sessionId = (int)id;
            var session = _sessionRepository.Get(sessionId);
            if (session == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // Must be GlobalAdmin or the original Uploader in order to remove
            if (!User.IsInRole(UserGroups.Admin) && session.AuthUserCharacter.AuthUserId != user.Id)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(session);
        }

        [HttpPost]
        [Authorize]
        [ActionName("RemoveSession")]
        public ActionResult RemoveSessionConfirmed(Session session)
        {
            _sessionRepository.RemoveSession(User.Identity.GetUserId(), session.Id);

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public ActionResult ReimportSession(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Home");
            }

            AuthUser user;
            if (!UserIsValid(out user))
            {
                HttpContext.GetOwinContext().Authentication.SignOut();
                return RedirectToAction("Index", "Home");
            }

            var sessionId = (int)id;
            var session = _sessionRepository.Get(sessionId);
            if (session == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (!User.IsInRole(UserGroups.Admin) && session.AuthUserCharacter.AuthUserId != user.Id)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(session);
        }

        [HttpPost]
        [Authorize]
        [ActionName("ReimportSession")]
        public ActionResult ReimportSessionConfirmed(Session session)
        {
            _sessionRepository.ClearSession(User.Identity.GetUserId(), session.Id);

            _sessionRepository.ReimportSession(User.Identity.GetUserId(), session.Id);

            return RedirectToAction("Index");
        }

        [Authorize]
        public ActionResult Rename(int id = -1)
        {
            if (id == -1)
            {
                return View("~/Views/Shared/InvalidResource.cshtml", model: "Session");
            }

            AuthUser user;
            if (!UserIsValid(out user))
            {
                HttpContext.GetOwinContext().Authentication.SignOut();
                return RedirectToAction("Index", "Home");
            }

            var session = _sessionRepository.Get(id);
            if (session == null)
            {
                return View("~/Views/Shared/InvalidResource.cshtml", model: "Session");
            }

            // We have confirmed both the user and session exist and are valid, now check that this user is allowed to rename it in order to continue
            // This next statement should always be true, unless we change the system to allow logs without guilds
            // Currently, we get the guild id from the uploader, but this has to change in case the uploader leaves the guild or deletes their account.
            //TODO: FIX THE SESSION RECORDS SO IT IS NOT TIED TO A USER
            if (session.AuthUserCharacter.GuildId == null)
            {
                return View("~/Views/Shared/InvalidResource.cshtml", model: "Session");
            }

            if (User.IsInRole(UserGroups.Admin))
            {
                return View(session);
            }

            var userRank = _authUserCharacterRepository.GetCharacterWithHighestGuildRank(user.Id, (int)session.AuthUserCharacter.GuildId);
            if (userRank == null)
            {
                return View("~/Views/Shared/AccessDenied.cshtml");
            }
            // Finally, return the rename view if everything is OK
            if (user.Id == session.AuthUserCharacter.AuthUserId || userRank.GuildRank.CanModifyAnySession)
            {
                return View(session);
            }
            return View("~/Views/Shared/AccessDenied.cshtml");
        }

        [HttpPost]
        [Authorize]
        [ActionName("Rename")]
        public ActionResult RenameConfirmed(Session model)
        {
            if (!ModelState.IsValid || model.Name.Trim().Length == 0)
            {
                //model = _sessionRepository.Get(model.Id);
                return View(model);
            }

            model.Name = model.Name.Trim();

            var result = _sessionRepository.RenameSession(User.Identity.GetUserId(), model.Id, model.Name);
            if (result.Success)
            {
                return RedirectToAction("Detail", new { id = model.Id });
            }

            ModelState.AddModelError("", result.Message);
            //model = _sessionRepository.Get(model.Id);
            return View(model);
        }



        private bool UserIsValid(out AuthUser authUser)
        {
            authUser = _authRepository.GetUserAccount(User.Identity.GetUserId());
            return authUser != null;
        }

        public ActionResult SaveUploadedFile()
        {
            bool isSavedSuccessfully = true;
            string fName = "";
            try
            {
                foreach (string fileName in Request.Files)
                {
                    HttpPostedFileBase file = Request.Files[fileName];
                    //Save file content goes here
                    fName = file.FileName;
                    if (file != null && file.ContentLength > 0)
                    {
                        var originalDirectory = new DirectoryInfo(Server.MapPath(@"\"));

                        string pathString = System.IO.Path.Combine(originalDirectory.ToString(), "UploadedFiles");

                        var fileName1 = Path.GetFileName(file.FileName);

                        bool isExists = System.IO.Directory.Exists(pathString);

                        if (!isExists)
                            System.IO.Directory.CreateDirectory(pathString);

                        var path = string.Format("{0}\\{1}", pathString, file.FileName);
                        file.SaveAs(path);
                    }
                }

                return Json(new { Message = fName });
            }
            catch (Exception ex)
            {
                return Json(new { Message = string.Format("Error in saving file: {0}", ex.Message) });
            }
        }
    }
}