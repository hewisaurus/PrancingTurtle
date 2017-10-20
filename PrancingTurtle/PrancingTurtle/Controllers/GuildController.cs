using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Database.Models;
using Database.Repositories.Interfaces;
using Microsoft.AspNet.Identity;
using Common;
using PrancingTurtle.Helpers.Authorization;
using PrancingTurtle.Models.Misc;
using PrancingTurtle.Models.ViewModels.Guild;
using Logging;

namespace PrancingTurtle.Controllers
{
    public class GuildController : Controller
    {
        private readonly ILogger _logger;
        private readonly IGuildRepository _guildRepository;
        private readonly IAuthenticationRepository _authRepository;
        private readonly IAuthUserCharacterRepository _authUserCharacterRepository;
        private readonly IGuildRankRepository _guildRankRepository;
        private readonly IAuthUserCharacterGuildApplicationRepository _authUserCharacterGuildApplicationRepository;
        private readonly ISessionRepository _sessionRepository;
        private readonly IEncounterRepository _encounterRepository;
        private readonly IGuildStatusRepository _guildStatusRepository;
        private readonly IBossFightRepository _bossFightRepository;

        public GuildController(ILogger logger, IGuildRepository guildRepository, IAuthenticationRepository authRepository,
            IGuildRankRepository guildRankRepository, IAuthUserCharacterGuildApplicationRepository authUserCharacterGuildApplicationRepository,
            IAuthUserCharacterRepository authUserCharacterRepository, ISessionRepository sessionRepository,
            IEncounterRepository encounterRepository, IGuildStatusRepository guildStatusRepository, IBossFightRepository bossFightRepository)
        {
            _logger = logger;
            _guildRepository = guildRepository;
            _authRepository = authRepository;
            _guildRankRepository = guildRankRepository;
            _authUserCharacterGuildApplicationRepository = authUserCharacterGuildApplicationRepository;
            _authUserCharacterRepository = authUserCharacterRepository;
            _sessionRepository = sessionRepository;
            _encounterRepository = encounterRepository;
            _guildStatusRepository = guildStatusRepository;
            _bossFightRepository = bossFightRepository;
        }

        // GET: Guild/1
        public ActionResult Index(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var guild = _guildRepository.Get((int)id);

            if (guild == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var guildCounts = _guildRepository.GetGuildIndexCounts(guild.Id);

            var model = new GuildIndexVM
            {
                Guild = guild,
                Members = guildCounts.MemberCount,
                Sessions = guildCounts.SessionCount,
                Encounters = guildCounts.EncounterCount,
                GuildRank = new GuildRank(),
                //BossFightProgressionOld = GetBossFightProgression(guild.Id), // Soon to be removed
                BossFightProgression = _guildRepository.GetGuildProgression(guild.Id),
                // We are checking roster visibility on the view, so get the member list here
                MemberList = _authUserCharacterRepository.GetGuildMembers(guild.Id)
            };

            
            // Check whether we're able to display the link to the guild sessions, or whether they are private
            if (guild.HideSessions)
            {
                if (!Request.IsAuthenticated)
                {
                    model.CanLinkToSessions = false;
                }
                else
                {
                    var user = _authRepository.GetUserAccount(User.Identity.GetUserId());
                    if (user == null)
                    {
                        HttpContext.GetOwinContext().Authentication.SignOut();
                        return RedirectToAction("Index", "Home");
                    }

                    model.CanLinkToSessions = model.MemberList.Any(gm => gm.AuthUserId == user.Id);
                }
            }
            else
            {
                model.CanLinkToSessions = true;
            }
            
            

            // If the request has been authenticated, check whether the user is valid
            if (Request.IsAuthenticated)
            {
                string email = User.Identity.GetUserId();

                var user = _authRepository.GetUserAccount(email);
                if (user == null)
                {
                    HttpContext.GetOwinContext().Authentication.SignOut();
                    return RedirectToAction("Index", "Home");
                }

                // So we have a valid user - are they an approved part of this guild? Also, approve global admins
                if (User.IsInRole(UserGroups.Admin))
                {
                    if (!guild.Status.Approved)
                    {
                        model.CanBeApproved = true;
                    }
                    model.CanBeRemoved = true;
                }

                var member = _authUserCharacterRepository.GetCharacterWithHighestGuildRank(user.Id, guild.Id);
                if (member != null)
                {
                    model.IsMember = true;
                    model.GuildRank = member.GuildRank;

                    // We're checking roster visibility on the view, so this has been moved further up the method
                    //model.MemberList = _authUserCharacterRepository.GetGuildMembers(guild.Id);
                    model.AvailableRanks = _guildRankRepository.GetRanks();
                    model.CurrentUserId = user.Id;

                    if (member.GuildRank.CanApproveUsers)
                    {
                        model.Applications = _authUserCharacterGuildApplicationRepository.PendingApplications(guild.Id);
                    }
                }
                else if (User.IsInRole(UserGroups.Admin)) // Allow admins to administrate any guild
                {
                    model.IsMember = true; // Required to see members and applications
                    //model.MemberList = _authUserCharacterRepository.GetGuildMembers(guild.Id);
                    model.AvailableRanks = _guildRankRepository.GetRanks(); // Get the list of ranks
                    model.Applications = _authUserCharacterGuildApplicationRepository.PendingApplications(guild.Id);
                    model.GuildRank = model.AvailableRanks.OrderBy(r => r.RankPriority).First(); // Assign the admin the top rank
                }
            }

            // Go through the list of members and make sure that if the current user is a member, that they can't 
            // accidentally remove their own permissions
            foreach (var member in model.MemberList)
            {
                member.CanBeModified = member.AuthUserId != model.CurrentUserId;
            }

            return View(model);
        }

        [Obsolete]
        private List<BossFightProgressionOLD> GetBossFightProgression(int guildId)
        {
            var bossFights = _bossFightRepository.GetAll(true);
            var guildProgression = _guildRepository.GetBossFightsCleared(guildId);

            return bossFights.Select(bossFight => guildProgression.Contains(bossFight.Id) 
                ? new BossFightProgressionOLD(bossFight, true) 
                : new BossFightProgressionOLD(bossFight, false)).ToList();
        }

        private bool CanContinueApproval(int? id)
        {
            if (id == null)
            {
                return false;
            }

            string username = User.Identity.GetUserId();
            var authUser = _authRepository.GetUserAccount(username);
            if (authUser == null)
            {
                // This isn't a valid user, so sign it out and redirect
                HttpContext.GetOwinContext().Authentication.SignOut();
                return false;
            }

            var application = _authUserCharacterGuildApplicationRepository.GetPendingApplication((int)id);
            if (application == null)
            {
                return false;
            }

            // The application is a valid one, does this user have access to approve it?

            if (User.IsInRole(UserGroups.Admin))
            {
                return true;
            }

            var rank = _authUserCharacterRepository.GetCharacterWithHighestGuildRank(authUser.Id, application.GuildId);
            if (rank == null)
            {
                // This user doesn't have any permissions for this guild!
                // LOG THIS!
                return false;
            }

            // The rank is valid, but check whether it can approve applications
            if (!rank.GuildRank.CanApproveUsers)
            {
                return false;
            }

            return true;
        }

        [Authorize]
        public ActionResult ApproveApp(int? id)
        {
            if (!CanContinueApproval(id))
            {
                return RedirectToAction("Index", "Home");
            }

            // The user has the required permission, so go ahead and confirm
            var application = _authUserCharacterGuildApplicationRepository.GetPendingApplication((int)id);
            return View(application);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult ApproveApp(AuthUserCharacterGuildApplication application)
        {
            string username = User.Identity.GetUserId();
            // Add the user to the guild roster, then remove the application
            var app = _authUserCharacterGuildApplicationRepository.GetPendingApplication(application.Id);
            var defaultRank = _guildRankRepository.GetDefaultRankForGuildApplications();
            if (defaultRank == null)
            {
                ModelState.AddModelError("", "Couldn't approve this guild application because no rank has been defined as default for new members!");
                return View(app);
            }
            var result = _authUserCharacterRepository.AddCharacterToGuild(app.AuthUserCharacterId, app.GuildId, defaultRank.Id);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                return View(app);
            }
            _logger.Debug(string.Format("{0} has approved {1}'s application for {2}", username, app.Character.CharacterName, app.Guild.Name));
            result = _authUserCharacterGuildApplicationRepository.Remove(application.Id, username);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                return View(app);
            }
            return RedirectToAction("AppApproved", new { @id = app.GuildId });
        }

        [Authorize]
        public ActionResult AppApproved(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View((int)id);
        }

        [Authorize]
        public ActionResult DeclineApp(int? id)
        {
            if (!CanContinueApproval(id))
            {
                return RedirectToAction("Index", "Home");
            }

            // The user has the required permission, so go ahead and confirm
            var application = _authUserCharacterGuildApplicationRepository.GetPendingApplication((int)id);
            return View(application);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult DeclineApp(AuthUserCharacterGuildApplication application)
        {
            string username = User.Identity.GetUserId();
            var app = _authUserCharacterGuildApplicationRepository.GetPendingApplication(application.Id);
            var result = _authUserCharacterGuildApplicationRepository.Remove(application.Id, username);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                return View(app);
            }
            _logger.Debug(string.Format("{0} has declined {1}'s application for {2}", username, app.Character.CharacterName, app.Guild.Name));
            return RedirectToAction("AppDeclined", new { @id = app.GuildId });
        }

        [Authorize]
        public ActionResult AppDeclined(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View((int)id);
        }

        [Authorize]
        public ActionResult ChangeRank(int u = -1, int g = -1, int r = -1)
        {
            // Check that we have a user to modify, a guild in which to do it, and a rank to change to
            if (u == -1 || g == -1 || r == -1)
            {
                return RedirectToAction("Index", "Home");
            }

            // Check our logged-in user is valid
            string email = User.Identity.GetUserId();

            var user = _authRepository.GetUserAccount(email);
            if (user == null)
            {
                HttpContext.GetOwinContext().Authentication.SignOut();
                return RedirectToAction("Index", "Home");
            }

            // Check that the given user is in the given guild
            if (!_authUserCharacterRepository.IsCharacterInGuild(u, g))
            {
                return RedirectToAction("Index", "Home");
            }

            // Now check if the logged-in user can modify this rank
            bool canContinue = false;

            // So we have a valid user - are they an approved part of this guild? Also, approve global admins
            if (User.IsInRole(UserGroups.Admin))
            {
                canContinue = true;
            }
            else
            {
                var member = _authUserCharacterRepository.GetCharacterWithHighestGuildRank(user.Id, g);
                if (member != null)
                {
                    if (member.GuildRank.CanPromoteUsers)
                    {
                        canContinue = true;
                    }
                }
            }

            // Redirect if we can't continue due to permissions
            if (!canContinue)
            {
                return RedirectToAction("Index", "Home");
            }

            var result = _authUserCharacterRepository.ModifyCharacterRank(u, g, r);

            return RedirectToAction("Index", new { @id = g });
        }

        
        [CustomAuthorization]
        public ActionResult Remove(int id = -1)
        {
            if (id == -1)
            {
                return View("~/Views/Shared/InvalidResource.cshtml", model: "Guild");
            }

            // Check that the guild actually exists
            var guild = _guildRepository.Get(id);
            if (guild == null)
            {
                return View("~/Views/Shared/InvalidResource.cshtml", model: "Guild");
            }

            return View(guild);
        }

        [HttpPost]
        [CustomAuthorization]
        
        [ValidateAntiForgeryToken]
        public ActionResult Remove(Guild guild)
        {
            var result = _guildRepository.Remove(User.Identity.GetUserId(), guild.Id);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                var model = _guildRepository.Get(guild.Id);
                return View(model);
            }
            return RedirectToAction("Index", "Home");
        }

        
        [CustomAuthorization]
        public ActionResult Approve(int id = -1)
        {
            if (id == -1)
            {
                return View("~/Views/Shared/InvalidResource.cshtml", model: "Guild");
            }

            // Check that the guild actually exists
            var guild = _guildRepository.Get(id);
            if (guild == null)
            {
                return View("~/Views/Shared/InvalidResource.cshtml", model: "Guild");
            }

            return View(guild);
        }

        [HttpPost]
        [CustomAuthorization]
        
        [ValidateAntiForgeryToken]
        public ActionResult Approve(Guild guild)
        {
            var approvalStatus = _guildStatusRepository.GetDefaultApprovedStatus();
            if (approvalStatus == 0)
            {
                ModelState.AddModelError("", "Couldn't approve this guild as there is no default status set!");
                var model = _guildRepository.Get(guild.Id);
                return View(model);
            }

            var result = _guildRepository.Approve(User.Identity.GetUserId(), guild.Id, approvalStatus);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                var model = _guildRepository.Get(guild.Id);
                return View(model);
            }
            return RedirectToAction("Index", new { @id = guild.Id });
        }

        [Authorize]
        public ActionResult KickMember(int id = -1, int gid = -1)
        {
            var user = _authRepository.GetUserAccount(User.Identity.GetUserId());
            if (user == null)
            {
                HttpContext.GetOwinContext().Authentication.SignOut();
                return RedirectToAction("Index", "Home");
            }

            var guild = _guildRepository.Get(gid);
            if (guild == null)
            {
                return View("InvalidResource", model: "guild");
            }

            var member = _authUserCharacterRepository.GetCharacterWithHighestGuildRank(user.Id, guild.Id);
            if (!User.IsInRole(UserGroups.Admin) && !member.GuildRank.CanApproveUsers)
            {
                return View("AccessDenied");
            }

            var kickMember = _authUserCharacterRepository.GetGuildMembers(gid).FirstOrDefault(m => m.Id == id);
            if (kickMember == null)
            {
                return View("InvalidResource", model: "user");
            }
            return View(kickMember);
        }

        [HttpPost]
        [Authorize]
        public ActionResult KickMember(AuthUserCharacter model)
        {
            var result = _authUserCharacterRepository.KickCharacterFromGuild(model.Id, model.GuildId,
                User.Identity.GetUserId());
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                return View(model);
            }
            return RedirectToAction("Index", new { @id = model.GuildId });
        }

        private bool CanSetPrivacy(int id = -1)
        {
            if (id == -1)
            {
                return false;
            }

            var authUser = _authRepository.GetUserAccount(User.Identity.GetUserId());
            if (authUser == null)
            {
                // This isn't a valid user, so sign it out and redirect
                HttpContext.GetOwinContext().Authentication.SignOut();
                return false;
            }

            if (User.IsInRole(UserGroups.Admin))
            {
                return true;
            }

            var rank = _authUserCharacterRepository.GetCharacterWithHighestGuildRank(authUser.Id, id);
            if (rank == null)
            {
                return false;
            }

            if (rank.GuildRank.CanModifyPrivacy)
            {
                return true;
            }

            return false;
        }

        [Authorize]
        public ActionResult SetRosterPublic(int id = -1)
        {
            if (id == -1)
            {
                return RedirectToAction("Index", "Home");
            }

            var guild = _guildRepository.Get(id);
            if (guild == null)
            {
                return View("InvalidResource", model: "guild");
            }

            string username = User.Identity.GetUserId();
            if (!CanSetPrivacy(id))
            {
                return RedirectToAction("Index", "Home");
            }

            var result = _guildRepository.SetGuildRosterPrivacy(username, id, true);
            return RedirectToAction("Index", new { id });
        }

        [Authorize]
        public ActionResult SetRosterPrivate(int id = -1)
        {
            if (id == -1)
            {
                return RedirectToAction("Index", "Home");
            }

            var guild = _guildRepository.Get(id);
            if (guild == null)
            {
                return View("InvalidResource", model: "guild");
            }

            string username = User.Identity.GetUserId();
            if (!CanSetPrivacy(id))
            {
                return RedirectToAction("Index", "Home");
            }

            var result = _guildRepository.SetGuildRosterPrivacy(username, id, false);
            return RedirectToAction("Index", new { id });
        }

        [Authorize]
        public ActionResult SetListsPublic(int id = -1)
        {
            if (id == -1)
            {
                return RedirectToAction("Index", "Home");
            }

            var guild = _guildRepository.Get(id);
            if (guild == null)
            {
                return View("InvalidResource", model: "guild");
            }

            string username = User.Identity.GetUserId();
            if (!CanSetPrivacy(id))
            {
                return RedirectToAction("Index", "Home");
            }

            var result = _guildRepository.SetGuildListPrivacy(username, id, true);
            return RedirectToAction("Index", new { id });
        }

        [Authorize]
        public ActionResult SetListsPrivate(int id = -1)
        {
            if (id == -1)
            {
                return RedirectToAction("Index", "Home");
            }

            var guild = _guildRepository.Get(id);
            if (guild == null)
            {
                return View("InvalidResource", model: "guild");
            }

            string username = User.Identity.GetUserId();
            if (!CanSetPrivacy(id))
            {
                return RedirectToAction("Index", "Home");
            }

            var result = _guildRepository.SetGuildListPrivacy(username, id, false);
            return RedirectToAction("Index", new { id });
        }

        [Authorize]
        public ActionResult SetRankingsPublic(int id = -1)
        {
            if (id == -1)
            {
                return RedirectToAction("Index", "Home");
            }

            var guild = _guildRepository.Get(id);
            if (guild == null)
            {
                return View("InvalidResource", model: "guild");
            }

            string username = User.Identity.GetUserId();
            if (!CanSetPrivacy(id))
            {
                return RedirectToAction("Index", "Home");
            }

            var result = _guildRepository.SetGuildRankingPrivacy(username, id, true);
            return RedirectToAction("Index", new { id });
        }

        [Authorize]
        public ActionResult SetRankingsPrivate(int id = -1)
        {
            if (id == -1)
            {
                return RedirectToAction("Index", "Home");
            }

            var guild = _guildRepository.Get(id);
            if (guild == null)
            {
                return View("InvalidResource", model: "guild");
            }

            string username = User.Identity.GetUserId();
            if (!CanSetPrivacy(id))
            {
                return RedirectToAction("Index", "Home");
            }

            var result = _guildRepository.SetGuildRankingPrivacy(username, id, false);
            return RedirectToAction("Index", new { id });
        }

        [Authorize]
        public ActionResult SetSessionsPublic(int id = -1)
        {
            if (id == -1)
            {
                return RedirectToAction("Index", "Home");
            }

            var guild = _guildRepository.Get(id);
            if (guild == null)
            {
                return View("InvalidResource", model: "guild");
            }

            string username = User.Identity.GetUserId();
            if (!CanSetPrivacy(id))
            {
                return RedirectToAction("Index", "Home");
            }

            var result = _guildRepository.SetGuildSessionPrivacy(username, id, true);
            return RedirectToAction("Index", new { id });
        }

        [Authorize]
        public ActionResult SetSessionsPrivate(int id = -1)
        {
            if (id == -1)
            {
                return RedirectToAction("Index", "Home");
            }

            var guild = _guildRepository.Get(id);
            if (guild == null)
            {
                return View("InvalidResource", model: "guild");
            }

            string username = User.Identity.GetUserId();
            if (!CanSetPrivacy(id))
            {
                return RedirectToAction("Index", "Home");
            }

            var result = _guildRepository.SetGuildSessionPrivacy(username, id, false);
            return RedirectToAction("Index", new { id });
        }

        [Authorize]
        public ActionResult SetProgressionPublic(int id = -1)
        {
            if (id == -1)
            {
                return RedirectToAction("Index", "Home");
            }

            var guild = _guildRepository.Get(id);
            if (guild == null)
            {
                return View("InvalidResource", model: "guild");
            }

            string username = User.Identity.GetUserId();
            if (!CanSetPrivacy(id))
            {
                return RedirectToAction("Index", "Home");
            }

            var result = _guildRepository.SetGuildProgressionPrivacy(username, id, true);
            return RedirectToAction("Index", new { id });
        }

        [Authorize]
        public ActionResult SetProgressionPrivate(int id = -1)
        {
            if (id == -1)
            {
                return RedirectToAction("Index", "Home");
            }

            var guild = _guildRepository.Get(id);
            if (guild == null)
            {
                return View("InvalidResource", model: "guild");
            }

            string username = User.Identity.GetUserId();
            if (!CanSetPrivacy(id))
            {
                return RedirectToAction("Index", "Home");
            }

            var result = _guildRepository.SetGuildProgressionPrivacy(username, id, false);
            return RedirectToAction("Index", new { id });
        }

        [Authorize]
        public ActionResult SetSearchPublic(int id = -1)
        {
            if (id == -1)
            {
                return RedirectToAction("Index", "Home");
            }

            var guild = _guildRepository.Get(id);
            if (guild == null)
            {
                return View("InvalidResource", model: "guild");
            }

            string username = User.Identity.GetUserId();
            if (!CanSetPrivacy(id))
            {
                return RedirectToAction("Index", "Home");
            }

            var result = _guildRepository.SetGuildSearchPrivacy(username, id, true);
            return RedirectToAction("Index", new { id });
        }

        [Authorize]
        public ActionResult SetSearchPrivate(int id = -1)
        {
            if (id == -1)
            {
                return RedirectToAction("Index", "Home");
            }

            var guild = _guildRepository.Get(id);
            if (guild == null)
            {
                return View("InvalidResource", model: "guild");
            }

            string username = User.Identity.GetUserId();
            if (!CanSetPrivacy(id))
            {
                return RedirectToAction("Index", "Home");
            }

            var result = _guildRepository.SetGuildSearchPrivacy(username, id, false);
            return RedirectToAction("Index", new { id });
        }
    }
}