using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Database.Models;
using Database.QueryModels.Misc;
using Database.Repositories.Interfaces;
using DotNet.Highcharts;
using DotNet.Highcharts.Enums;
using DotNet.Highcharts.Helpers;
using DotNet.Highcharts.Options;
using Hangfire;
using Microsoft.AspNet.Identity;
using Common;
using PrancingTurtle.Helpers.Controllers;
using PrancingTurtle.Models;
using PrancingTurtle.Models.DatabaseType;
using PrancingTurtle.Models.Misc;
using PrancingTurtle.Models.ViewModels;
using PrancingTurtle.Models.ViewModels.GuildNav;
using PrancingTurtle.Models.ViewModels.Home;
using PrancingTurtle.Models.ViewModels.Navigation;
using Logging;

namespace PrancingTurtle.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger _logger;
        private readonly IAuthenticationRepository _authRepository;
        private readonly IAuthUserCharacterRepository _authUserCharacterRepository;
        private readonly ISessionRepository _sessionRepository;
        private readonly INavigationRepository _navigationRepository;
        private readonly IStatRepository _statRepository;
        private readonly IGuildRepository _guildRepository;
        private readonly ISearchRepository _searchRepository;
        private readonly IBossFightRepository _bossFightRepository;
        private readonly INewsRecentChangesRepository _recentChanges;
        private readonly ISiteNotificationRepository _siteNotification;

        private readonly IRecurringTaskRepo _recurringTaskRepo;

        public HomeController(IAuthenticationRepository authRepository,
            ISessionRepository sessionRepository, INavigationRepository navigationRepository,
            IStatRepository statRepository, IGuildRepository guildRepository, ILogger logger,
            ISearchRepository searchRepository, IAuthUserCharacterRepository authUserCharacterRepository,
            IBossFightRepository bossFightRepository, INewsRecentChangesRepository recentChanges,
            ISiteNotificationRepository siteNotification, IRecurringTaskRepo recurringTaskRepo)
        {
            _authRepository = authRepository;
            _sessionRepository = sessionRepository;
            _navigationRepository = navigationRepository;
            _statRepository = statRepository;
            _guildRepository = guildRepository;
            _logger = logger;
            _searchRepository = searchRepository;
            _authUserCharacterRepository = authUserCharacterRepository;
            _bossFightRepository = bossFightRepository;
            _recentChanges = recentChanges;
            _siteNotification = siteNotification;
            _recurringTaskRepo = recurringTaskRepo;
        }
        
        public ActionResult ScheduleDailyStats()
        {
            // Should be at the top of the hour, every 6 hours
            RecurringJob.AddOrUpdate("DailyStats", () => _recurringTaskRepo.UpdateDailyStats(), "0 */6 * * *");
            return RedirectToAction("Index");
        }

        public ActionResult TestTask()
        {
            RecurringJob.AddOrUpdate("DeleteRemovedEncounters", () => _recurringTaskRepo.DeleteRemovedEncounter(), "*/1 * * * *");
            return RedirectToAction("Index");
        }
        
        public ActionResult Donate()
        {
            return View();
        }

        public ActionResult Donated()
        {
            return View();
        }

        public ActionResult Stats()
        {
            var stats = _statRepository.GetSiteStats();
            stats.TotalTimeSpan = new TimeSpan(stats.TotalPlayedTimeTicks);

            return View(stats);
        }

        public ActionResult Index(string s)
        {
            if (!string.IsNullOrEmpty(s))
            {
                return RedirectToAction("Search", new { s });
            }

            var model = new HomeIndexVM();

            if (Request.IsAuthenticated)
            {
                // This should only happen once for each user if they're still logged in when the change is published
                if (UAuthInfo == null)
                {
                    HttpContext.GetOwinContext().Authentication.SignOut();
                    return RedirectToAction("Index", "Home");
                }

                var usrname = UAuthInfo.Username;
                var uId = UAuthInfo.AuthUserId;

                // Check we have a valid user
                var user = _authRepository.GetUserAccount(User.Identity.GetUserId());
                if (user == null)
                {
                    HttpContext.GetOwinContext().Authentication.SignOut();
                    return RedirectToAction("Index", "Home");
                }
                model.TimeZoneId = user.TimeZone;
                model.Sessions = _sessionRepository.GetRecentSessions(10, user.Email);
            }
            else
            {
                model.Sessions = _sessionRepository.GetRecentSessions(10);
            }

            foreach (var session in model.Sessions)
            {
                session.InstancesSeen = _sessionRepository.GetInstancesSeen(session.Id);
                foreach (var instance in session.InstancesSeen)
                {
                    // List the difficulties seen if there have been any configured
                    instance.DifficultiesSeen = _sessionRepository.GetDifficultiesSeen(session.Id, instance.Id);
                }
            }

            model.RecentChanges = _recentChanges.GetRecentChanges().Take(6).ToList();
            var notification = _siteNotification.GetNotification();
            if (notification != null)
            {
                model.SiteNotification = notification;
                model.DisplayNotification = true;
            }
            return View(model);
        }

        public ActionResult Search(string s)
        {
            if (string.IsNullOrEmpty(s.Trim())) return RedirectToAction("Index");
            s = s.Trim();
            var searchResult = new SearchResult();
            if (Request.IsAuthenticated)
            {
                searchResult = User.IsInRole(UserGroups.Admin)
                    ? _searchRepository.Search(s, "", true)
                    : _searchRepository.Search(s, User.Identity.GetUserId());
            }
            else
            {
                searchResult = _searchRepository.Search(s, "");
            }
            return View(searchResult);
        }

        [ChildActionOnly]
        public ActionResult MainNavigation()
        {
            bool showAll = false;
            var model = new MainNavigation()
            {
                LoggedIn = false,
                ShortMenuFormat = false,
                ShowGuildMenu = true,
                BossFightDifficultyRecords = _bossFightRepository.GetBossFightsAndDifficultySettings()
            };
            if (Request.IsAuthenticated)
            {
                if (User.IsInRole(UserGroups.Admin))
                {
                    showAll = true;
                }
                var user = _authRepository.GetUserAccount(User.Identity.GetUserId());
                if (user != null)
                {
                    model.LoggedIn = true;
                    model.ShortMenuFormat = user.ShortMenuFormat;
                    model.ShowGuildMenu = user.ShowGuildMenu;
                }
            }

            var guilds = showAll
                ? _guildRepository.GetAll()
                : _guildRepository.GetVisibleGuilds(User.Identity.GetUserId());

            foreach (var guild in guilds)
            {
                guild.Name = guild.Name.Substring(0, 1).ToUpper() + guild.Name.Substring(1);
            }
            model.Guilds = guilds.OrderBy(g => g.Name).ToList();

            return PartialView("_MainNavigation", model);
        }

        [ChildActionOnly]
        public ActionResult SessionNavigation()
        {
            bool showAll = false;
            if (Request.IsAuthenticated)
            {
                if (User.IsInRole(UserGroups.Admin))
                {
                    showAll = true;
                }
            }
            var model = new SessionNavigation
            {
                //BossFights = _bossFightRepository.GetAll(false),
                //BossFights = _bossFightRepository.GetAllIncludingDifficulties(), 
                //temp remove the above line while we test. If the test doesn't work, uncomment this line and return Sessionnav not V2
                BossFightDifficultyRecords = _bossFightRepository.GetBossFightsAndDifficultySettings()

            };
            var guilds = showAll
                ? _guildRepository.GetAll()
                : _guildRepository.GetVisibleGuilds(User.Identity.GetUserId());

            foreach (var guild in guilds)
            {
                guild.Name = guild.Name.Substring(0, 1).ToUpper() + guild.Name.Substring(1);
            }
            model.Guilds = guilds.OrderBy(g => g.Name).ToList();

            //return PartialView("_SessionNav", model);
            return PartialView("_SessionNavV2", model);
        }

        [ChildActionOnly]
        public ActionResult GetHost()
        {
            return PartialView("_Host", Environment.MachineName.ToLower());
        }

        [ChildActionOnly]
        public ActionResult GetGuildNav()
        {
            var guildIds = new List<int>();
            if (Request.IsAuthenticated)
            {
                guildIds.AddRange(_authUserCharacterRepository.GetGuildIdsForEmail(User.Identity.GetUserId()));
            }
            var guildNav = _navigationRepository.GetGuildNavigation();
            var navModel = new List<GuildNavShard>();
            foreach (var guild in guildNav)
            {
                var guildNavShard = navModel.FirstOrDefault(n => n.Name == guild.Shard.Name);
                if (guildNavShard == null)
                {
                    navModel.Add(new GuildNavShard()
                    {
                        Name = guild.Shard.Name,
                        Guilds = new List<GuildNavGuild>()
                    {
                        new GuildNavGuild()
                        {
                            Id = guild.Id,
                            Name = guild.Name,
                            Visible = !guild.HideFromLists || guildIds.Contains(guild.Id)
                        }
                    }
                    });
                }
                else
                {
                    guildNavShard.Guilds.Add(new GuildNavGuild() { Id = guild.Id, Name = guild.Name, Visible = !guild.HideFromLists || guildIds.Contains(guild.Id) });
                }
            }

            return PartialView("_GuildNav", navModel);
        }

        [ChildActionOnly]
        public ActionResult GetGuildAdminNav()
        {
            var guilds = _guildRepository.GetAll();
            return PartialView("_GuildAdminNav", guilds);
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }

        public ActionResult Changelog()
        {
            var recentChanges = _recentChanges.GetRecentChanges();

            return View(recentChanges);
        }

        public ActionResult SubmitLogs()
        {
            return View();
        }


        private void UpdatePlayerList(IEnumerable<DamageDone> sourceList, List<Player> playerList, List<string> namesList)
        {
            foreach (var entry in sourceList)
            {
                if (entry.SourcePlayer != null)
                {
                    if (!namesList.Contains(entry.SourcePlayer.Name))
                    {
                        namesList.Add(entry.SourcePlayer.Name);
                    }
                    if (!playerList.Any(p => p.PlayerId == entry.SourcePlayer.PlayerId))
                    {
                        playerList.Add(entry.SourcePlayer);
                    }
                }
                if (entry.TargetPlayer != null)
                {
                    if (!namesList.Contains(entry.TargetPlayer.Name))
                    {
                        namesList.Add(entry.TargetPlayer.Name);
                    }
                    if (!playerList.Any(p => p.PlayerId == entry.TargetPlayer.PlayerId))
                    {
                        playerList.Add(entry.TargetPlayer);
                    }
                }
            }
        }
        private void UpdatePlayerList(IEnumerable<HealingDone> sourceList, List<Player> playerList, List<string> namesList)
        {
            foreach (var entry in sourceList)
            {
                if (entry.SourcePlayer != null)
                {
                    if (!namesList.Contains(entry.SourcePlayer.Name))
                    {
                        namesList.Add(entry.SourcePlayer.Name);
                    }
                    if (!playerList.Any(p => p.PlayerId == entry.SourcePlayer.PlayerId))
                    {
                        playerList.Add(entry.SourcePlayer);
                    }
                }
                if (entry.TargetPlayer != null)
                {
                    if (!namesList.Contains(entry.TargetPlayer.Name))
                    {
                        namesList.Add(entry.TargetPlayer.Name);
                    }
                    if (!playerList.Any(p => p.PlayerId == entry.TargetPlayer.PlayerId))
                    {
                        playerList.Add(entry.TargetPlayer);
                    }
                }
            }
        }
        private void UpdatePlayerList(IEnumerable<ShieldingDone> sourceList, List<Player> playerList, List<string> namesList)
        {
            foreach (var entry in sourceList)
            {
                if (entry.SourcePlayer != null)
                {
                    if (!namesList.Contains(entry.SourcePlayer.Name))
                    {
                        namesList.Add(entry.SourcePlayer.Name);
                    }
                    if (!playerList.Any(p => p.PlayerId == entry.SourcePlayer.PlayerId))
                    {
                        playerList.Add(entry.SourcePlayer);
                    }
                }
                if (entry.TargetPlayer != null)
                {
                    if (!namesList.Contains(entry.TargetPlayer.Name))
                    {
                        namesList.Add(entry.TargetPlayer.Name);
                    }
                    if (!playerList.Any(p => p.PlayerId == entry.TargetPlayer.PlayerId))
                    {
                        playerList.Add(entry.TargetPlayer);
                    }
                }
            }
        }
    }
}