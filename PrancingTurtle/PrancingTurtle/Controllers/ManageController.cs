using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Database.Models;
using Database.Repositories.Interfaces;
using Microsoft.AspNet.Identity;
using PrancingTurtle.Models.ViewModels;
using Logging;

namespace PrancingTurtle.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private readonly ILogger _logger;
        //private readonly IDapperRepository _repository;
        private readonly IAuthenticationRepository _authRepository;
        private readonly IAuthUserCharacterRepository _authUserCharRepository;
        private readonly IShardRepository _shardRepository;
        private readonly IGuildRepository _guildRepository;
        private readonly IGuildRankRepository _guildRankRepository;
        private readonly IAuthUserCharacterGuildApplicationRepository _authUserCharacterGuildApplicationRepository;

        public ManageController(IAuthenticationRepository authRepository,
            IAuthUserCharacterRepository authUserCharRepository, IShardRepository shardRepository, IGuildRepository guildRepository,
            IGuildRankRepository guildRankRepository,
            IAuthUserCharacterGuildApplicationRepository authUserCharacterGuildApplicationRepository, ILogger logger)
        {
            //_repository = repository;
            _authRepository = authRepository;
            _authUserCharRepository = authUserCharRepository;
            _shardRepository = shardRepository;
            _guildRepository = guildRepository;
            _guildRankRepository = guildRankRepository;
            _authUserCharacterGuildApplicationRepository = authUserCharacterGuildApplicationRepository;
            _logger = logger;
        }


        [Authorize]
        public ActionResult Index()
        {
            // Check that the username in the cookie is valid, not locked out, and has a confirmed email
            var username = User.Identity.GetUserId();
            var user = _authRepository.GetUserAccount(username);
            if (user == null)
            {
                HttpContext.GetOwinContext().Authentication.SignOut();
                return RedirectToAction("Index", "Home");
            }

            if (user.LockoutEndDate != null && user.LockoutEndDate > DateTime.UtcNow)
            {
                HttpContext.GetOwinContext().Authentication.SignOut();
                return RedirectToAction("Index", "Home");
            }

            if (!user.EmailConfirmed)
            {
                HttpContext.GetOwinContext().Authentication.SignOut();
                return RedirectToAction("Index", "Home");
            }

            var characters = _authUserCharRepository.GetCharacters(username).ToList();
            foreach (var character in characters)
            {
                if (character.GuildId == null)
                {
                    character.PendingApplicationGuildName = _authUserCharacterGuildApplicationRepository.PendingApplication(character.Id);
                }
                else
                {
                    character.GuildRank = _authUserCharRepository.GetGuildRankForCharacter(character.Id);
                }
            }
            
            var model = new ManageIndexVM
            {
                Characters = characters,
                LastLoginAddress = user.PreviousLoginAddress,
                TimeZoneId = user.TimeZone,
                TimeZoneDisplay = TimeZoneInfo.FindSystemTimeZoneById(user.TimeZone).DisplayName,
                ShortMenuFormat = user.ShortMenuFormat,
                ShowGuildMenu = user.ShowGuildMenu
            };

            if (user.PreviousLoginTime == null)
            {
                model.LastLoggedIn = DateTime.UtcNow;
            }
            else
            {
                model.LastLoggedIn =
                    ((DateTime)user.PreviousLoginTime).Add(
                        TimeZoneInfo.FindSystemTimeZoneById(user.TimeZone).GetUtcOffset(DateTime.UtcNow));
            }

            return View(model);
        }

        [Authorize]
        public ActionResult CreateCharacter()
        {
            var character = new AuthUserCharacter()
            {
                Shards = _shardRepository.GetAll().ToList()
            };
            return View(character);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult CreateCharacter(AuthUserCharacter model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = _authUserCharRepository.Create(User.Identity.GetUserId(), model);
            if (result.Success)
            {
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", result.Message);

            model.Shards = _shardRepository.GetAll().ToList();
            return View(model);
        }

        [Authorize]
        public ActionResult RemoveCharacter(int? id)
        {
            // Go back to the index if we weren't passed an ID to delete

            if (id == null)
            {
                return RedirectToAction("Index");
            }

            // Attempt to get the character to delete
            var character = _authUserCharRepository.Get(User.Identity.GetUserId(), id);
            if (character == null)
            {
                // Either we were passed an invalid ID, or the character specified by the ID didn't belong to the currently logged in user
                return RedirectToAction("Index");
            }

            return View(character);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveCharacter(AuthUserCharacter model)
        {
            string username = User.Identity.GetUserId();

            // Check if the character has uploaded any logs or created any sessions. If so, we need to set them to 'removed', not remove them.

            // If there are pending guild applications, remove them
            var pendingGuildApplication = _authUserCharacterGuildApplicationRepository.GetPendingApplicationForCharacter(model.Id);
            if (pendingGuildApplication != null)
            {
                _authUserCharacterGuildApplicationRepository.Remove(pendingGuildApplication.Id, username);
            }

            // Now, remove the user
            var result = _authUserCharRepository.Delete(username, model.Id);
            if (result.Success)
            {
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", result.Message);

            return View(model);
        }


        // GET: /Manage/ChangePassword
        [Authorize]
        public ActionResult ChangePassword()
        {
            return View();
        }

        ////
        // POST: /Manage/ChangePassword
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var email = User.Identity.GetUserId();

            if (_authRepository.Validate(email, model.OldPassword))
            {
                var result = _authRepository.ResetPassword(email, model.NewPassword);
                if (result.Success)
                {
                    return RedirectToAction("PasswordChangeSuccess");
                }

                ModelState.AddModelError("", result.Message);
            }
            else
            {
                // 'Old Password' was incorrect
                var user = _authRepository.GetUserAccount(email);
                if (user.LockoutEnabled)
                {
                    if (user.AccessFailedCount == 4)
                    {
                        // Lock the account for 5 minutes
                        _authRepository.LockAccount(user.Email);
                        HttpContext.GetOwinContext().Authentication.SignOut();
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        // Increment the failed attempt count
                        _authRepository.FailedPasswordAttempt(user.Email, user.AccessFailedCount + 1);
                        ModelState.AddModelError("", "Password change failed!");
                    }
                }
            }


            return View(model);
        }

        [Authorize]
        public ActionResult PasswordChangeSuccess()
        {
            return View();
        }

        [Authorize]
        public ActionResult TimeZone()
        {
            // Make sure the authenticated user is valid
            var user = _authRepository.GetUserAccount(User.Identity.GetUserId());

            if (user == null)
            {
                HttpContext.GetOwinContext().Authentication.SignOut();
                return RedirectToAction("Index", "Home");
            }

            var model = new ChangeTimeZoneVM(user.TimeZone);

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TimeZone(ChangeTimeZoneVM model)
        {
            var result = _authRepository.SetTimeZone(User.Identity.GetUserId(), model.SelectedTimeZoneId);
            if (result.Success)
            {
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", result.Message);
            return View(model);
        }

        [Authorize]
        public ActionResult CreateGuild(int? id)
        {
            // Check if the ID is null
            if (id == null)
            {
                return View("~/Views/Shared/InvalidResource.cshtml", model: "character");
            }

            string username = User.Identity.GetUserId();
            // Make sure the authenticated user is valid
            var user = _authRepository.GetUserAccount(username);

            if (user == null)
            {
                HttpContext.GetOwinContext().Authentication.SignOut();
                return RedirectToAction("Index", "Home");
            }

            var character = _authUserCharRepository.Get(username, id);
            if (character == null)
            {
                return View("~/Views/Shared/InvalidResource.cshtml", model: "character");
            }

            var model = new CreateGuildVM()
            {
                ShardId = character.ShardId,
                ShardName = character.Shard.Name,
                AuthUserCharacterId = (int)id
            };
            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateGuild(CreateGuildVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string email = User.Identity.GetUserId();
            // Remove leading and trailing spaces from the guild so we don't end up with duplicates
            string guildName = model.Name.Trim();

            #region Logout and redirect if the logged in user is invalid
            if (_authRepository.GetUserAccount(email) == null)
            {
                HttpContext.GetOwinContext().Authentication.SignOut();
                return RedirectToAction("Index", "Home");
            }
            #endregion
            // Get the default guild rank before the actual create happens, otherwise we could end up with
            // a guild that we can't link to a user (if there's no default rank)
            var defaultRank = _guildRankRepository.GetDefaultRankForGuildCreators();
            if (defaultRank != null)
            {
                var result = _guildRepository.Create(email, guildName, model.ShardId);
                if (result.Success)
                {
                    // The PK (ID) of the new guild is in result.Message (string format)
                    int newGuildId = int.Parse(result.Message);

                    result = _authUserCharRepository.AddCharacterToGuild(model.AuthUserCharacterId, newGuildId, defaultRank.Id);
                    if (result.Success)
                    {
                        return RedirectToAction("GuildCreated", new { @guildName = guildName.ToUpper() });
                    }
                }

                ModelState.AddModelError("", result.Message);
            }
            else
            {
                ModelState.AddModelError("", "The guild could not be created as no default guild rank has been selected! Please let the site operators know ASAP.");
            }

            return View(model);
        }

        [Authorize]
        public ActionResult GuildCreated(string guildName)
        {
            return View(model: guildName);
        }

        [Authorize]
        public ActionResult JoinGuild(int? id, int? sid)
        {
            string email = User.Identity.GetUserId();

            #region Logout and redirect if the logged in user is invalid
            if (_authRepository.GetUserAccount(email) == null)
            {
                HttpContext.GetOwinContext().Authentication.SignOut();
                return RedirectToAction("Index", "Home");
            }
            #endregion

            // Check if either ID are null
            if (id == null || sid == null)
            {
                return RedirectToAction("Index");
            }

            // Check if the shard ID is valid
            if (_shardRepository.Get((int)sid) == null)
            {
                return RedirectToAction("Index");
            }

            // Check if the logged in user can join a guild as this character (do they own it?)
            if (!_authUserCharRepository.CanJoinAGuild(email, (int)id))
            {
                return RedirectToAction("Index");
            }

            var model = new JoinGuildVM((int)id, (int)sid)
            {
                Guilds = _guildRepository.GetGuilds((int)sid, true)
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult JoinGuild(JoinGuildVM model)
        {
            if (ModelState.IsValid)
            {
                var result = _authUserCharacterGuildApplicationRepository.Create(
                    new AuthUserCharacterGuildApplication()
                    {
                        GuildId = model.GuildId,
                        Message = model.Message,
                        AuthUserCharacterId = model.AuthUserCharacterId
                    });

                if (result.Success)
                {
                    
                    return RedirectToAction("Index");
                }

                ModelState.AddModelError("", result.Message);
            }

            model.Guilds = _guildRepository.GetGuilds(model.ShardId, true);
            return View(model);
        }

        [Authorize]
        public ActionResult SetTextMenu()
        {
            var username = User.Identity.GetUserId();
            var user = _authRepository.GetUserAccount(username);
            if (user == null)
            {
                HttpContext.GetOwinContext().Authentication.SignOut();
                return RedirectToAction("Index", "Home");
            }
            
            var result = _authRepository.UpdateMenuFormat(user.Email, false);
            if (result.Success)
            {
                _logger.Debug(string.Format("{0} successfully set the menu format to text", user.Email));
            }
            else
            {
                _logger.Debug(string.Format("{0} failed to set the menu format to text: {1}", user.Email, result.Message));
            }
            return RedirectToAction("Index");
        }

        [Authorize]
        public ActionResult SetIconMenu()
        {
            var username = User.Identity.GetUserId();
            var user = _authRepository.GetUserAccount(username);
            if (user == null)
            {
                HttpContext.GetOwinContext().Authentication.SignOut();
                return RedirectToAction("Index", "Home");
            }

            var result = _authRepository.UpdateMenuFormat(user.Email, true);
            if (result.Success)
            {
                _logger.Debug(string.Format("{0} successfully set the menu format to icon/short", user.Email));
            }
            else
            {
                _logger.Debug(string.Format("{0} failed to set the menu format to icon/short: {1}", user.Email, result.Message));
            }
            return RedirectToAction("Index");
        }

        [Authorize]
        public ActionResult ShowGuildMenu()
        {
            var username = User.Identity.GetUserId();
            var user = _authRepository.GetUserAccount(username);
            if (user == null)
            {
                HttpContext.GetOwinContext().Authentication.SignOut();
                return RedirectToAction("Index", "Home");
            }

            var result = _authRepository.UpdateGuildMenuVisibility(user.Email, true);
            _logger.Debug(result.Success
                ? string.Format("{0} successfully set the guild menu visibility to true", user.Email)
                : string.Format("{0} failed to set the guild menu visibility to true: {1}", user.Email, result.Message));
            return RedirectToAction("Index");
        }

        [Authorize]
        public ActionResult HideGuildMenu()
        {
            var username = User.Identity.GetUserId();
            var user = _authRepository.GetUserAccount(username);
            if (user == null)
            {
                HttpContext.GetOwinContext().Authentication.SignOut();
                return RedirectToAction("Index", "Home");
            }

            var result = _authRepository.UpdateGuildMenuVisibility(user.Email, false);
            _logger.Debug(result.Success
                ? string.Format("{0} successfully set the guild menu visibility to false", user.Email)
                : string.Format("{0} failed to set the guild menu visibility to false: {1}", user.Email, result.Message));
            return RedirectToAction("Index");
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";



        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }


        #endregion
    }
}