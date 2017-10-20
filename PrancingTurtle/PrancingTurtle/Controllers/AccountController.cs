using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using BotDetect.Web.UI.Mvc;
using Database.Repositories.Interfaces;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using PrancingTurtle.Helpers;
using PrancingTurtle.Models;
using PrancingTurtle.Models.ViewModels;
using Common;
using PrancingTurtle.Helpers.Authorization;
using Logging;
using PrancingTurtle.Helpers.Mail;

namespace PrancingTurtle.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly ILogger _logger;
        private readonly IAuthenticationRepository _authRepository;

        public AccountController(IAuthenticationRepository authRepository, ILogger logger)
        {
            _authRepository = authRepository;
            _logger = logger;
        }

        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl, string pl)
        {
            var model = new LoginViewModel()
            {
                ReturnUrl = returnUrl,
                PostLoginUser = pl
            };
            //ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Reverify()
        {
            //MvcCaptcha.ResetCaptcha("ReverifyCaptcha");
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [CaptchaValidation("CaptchaReverify", "ReverifyCaptcha", "Incorrect verification code!")]
        public ActionResult Reverify(ReverifyViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (_authRepository.AlreadyExists(model.Email))
                {
                    var token = AuthEncryption.RandomSalt(6);
                    var result = _authRepository.SetEmailConfirmationToken(model.Email, token);
                    var secureUrl = Url.Action("ConfirmEmail", "Account", new { e = model.Email, c = token }, "https");
                    //var linkUrl = Url.AbsoluteAction("ConfirmEmail", "Account", new { e = model.Email, c = token });
                    string mailBody = MailCommonStrings.RegisteredBody(secureUrl);
                    Mailer.SendEmail_NoWait(model.Email, MailCommonStrings.RegisteredSubject, mailBody);
                    _logger.Debug(string.Format("{0} has requested another verification email from {1}", model.Email, ClientIpAddress.GetIPAddress(Request)));
                }
                MvcCaptcha.ResetCaptcha("ReverifyCaptcha");
                return RedirectToAction("ReverificationSent");
            }

            return View();
        }

        [AllowAnonymous]
        public ActionResult ReverificationSent()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [CaptchaValidation("PwResetCaptchaCode", "ResetPasswordCaptcha", "Incorrect verification code!")]
        public ActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (_authRepository.AlreadyExists(model.Email))
                {
                    var token = AuthEncryption.RandomSalt(6);
                    var result = _authRepository.SetPasswordResetToken(model.Email, token);
                    var userIp = ClientIpAddress.GetIPAddress(Request);
                    var secureUrl = Url.Action("TryResetPassword", "Account", new { e = model.Email, c = token }, "https");
                    //var linkUrl = Url.AbsoluteAction("TryResetPassword", "Account", new { e = model.Email, c = token });
                    string mailBody = MailCommonStrings.PasswordResetBody(userIp, secureUrl);
                    Mailer.SendEmail_NoWait(model.Email, MailCommonStrings.ResetPasswordSubject, mailBody);
                    _logger.Debug(string.Format("{0} has requested to reset their password from {1}", model.Email, ClientIpAddress.GetIPAddress(Request)));
                }
                MvcCaptcha.ResetCaptcha("ResetPasswordCaptcha");
                return RedirectToAction("PasswordResetSent");
            }

            return View();
        }

        [AllowAnonymous]
        public ActionResult PasswordResetSent()
        {
            return View();
        }

        //private ApplicationSignInManager _signInManager;

        //public ApplicationSignInManager SignInManager
        //{
        //    get
        //    {
        //        return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
        //    }
        //    private set { _signInManager = value; }
        //}

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // SYNCHRONOUS
            //var user = _authRepository.GetUserAccount(model.Email);
            // ASYNCHRONOUS
            var user = await _authRepository.GetUserAccountAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                return View(model);
            }

            if (user.LockoutEndDate != null && user.LockoutEndDate > DateTime.UtcNow)
            {
                ModelState.AddModelError("", "Your account is currently locked due to too many incorrect password attempts. Try again in 5 minutes.");
                return View(model);
            }

            if (!user.EmailConfirmed)
            {
                ModelState.AddModelError("", "You must verify your email address before you can login");
                return View(model);
            }

            // SYNCHRONOUS
            //if (_authRepository.Validate(model.Email, model.Password))
            // ASYNCHRONOUS
            if (await _authRepository.ValidateAsync(model.Email, model.Password))
            {
                var claims = new List<Claim>();
                
                // For now, ignore the model.PostLoginUser property as well. If we need it, implement this code again.

                // Add the basic claims that all users need
                claims.Add(new Claim(ClaimTypes.NameIdentifier, model.Email));
                claims.Add(new Claim(ClaimTypes.Name, model.Email));
                claims.Add(new Claim(ClaimTypes.Email, model.Email));
                // Application Sid
                claims.Add(new Claim(ClaimTypes.Sid, ApplicationSid.Sid));
                // User Id so we don't have to query it again
                claims.Add(new Claim(ClaimTypes.PrimarySid, user.Id.ToString()));

                // Add any group-based claims
                var groupMembership = await _authRepository.GetUserGroupMembership(model.Email);
                if (groupMembership.Any())
                {
                    foreach (var group in groupMembership)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, group));
                    }
                }

                var identity = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);

                HttpContext.GetOwinContext().Authentication.SignIn(new AuthenticationProperties() { IsPersistent = model.RememberMe }, identity);

                var userIp = ClientIpAddress.GetIPAddress(Request);
                var loginSuccess = _authRepository.UpdateLastLoginInfo(model.Email, DateTime.UtcNow, userIp);

                _logger.Debug(string.Format("{0} has successfully logged in from {1}", model.Email, userIp));

                return RedirectToLocal(model.ReturnUrl);
            }
            else
            {
                // Authentication failed but the email address exists. If we haven't explicitly disabled the lockout mechanism, increment or lock here
                if (user.LockoutEnabled)
                {
                    _logger.Debug(string.Format("{0} has failed to login, and their fail count is now {1}.", user.Email, user.AccessFailedCount + 1));

                    if (user.AccessFailedCount == 4)
                    {
                        // Lock the account for 5 minutes
                        _logger.Debug(string.Format("Locking {0} out for 5 minutes (too many incorrect password attempts)", user.Email));
                        _authRepository.LockAccount(user.Email);
                        ModelState.AddModelError("", "Your account has been locked out for 5 minutes due to too many incorrect password attempts.");
                        return View(model);
                    }
                    else
                    {
                        // Increment the failed attempt count
                        _authRepository.FailedPasswordAttempt(user.Email, user.AccessFailedCount + 1);
                    }
                }
            }

            ModelState.AddModelError("", "Invalid login attempt.");
            return View(model);
        }

        #region PRevious Login POST
        // POST: /Account/Login
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(model);
        //    }

        //    // This doesn't count login failures towards account lockout
        //    // To enable password failures to trigger account lockout, change to shouldLockout: true

        //    // temporarily hardcode auth here cause I'm lazy and just want to do a basic secure of the app
        //    if (model.Email == "administrator@system.local")
        //    {
        //        if (model.Password == "Woof!s8dfj35jWKFLKEDJFGDSg83faceCheckThatBushOverTherePlox")
        //        {
        //            // Build the authorization claims object
        //            var claims = new List<Claim>
        //                        {
        //                            new Claim(ClaimTypes.NameIdentifier, "Administrator")
        //                            ,new Claim(ClaimTypes.Name, "System Administrator")
        //                            ,new Claim(ClaimTypes.Email, "administrator@system.local")
        //                        };

        //            claims.Add(new Claim(ClaimTypes.Role, "GlobalAdmin"));
        //            //claims.AddRange(_roleManager.GetPermissionsForUsername(person.Username, authSource.Id).Select(permission => new Claim(ClaimTypes.Role, permission)));

        //            var identity = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);

        //            HttpContext.GetOwinContext().Authentication.SignIn(new AuthenticationProperties() { IsPersistent = model.RememberMe }, identity);
        //            return RedirectToLocal(returnUrl);
        //        }
        //    }


        //    //var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
        //    //switch (result)
        //    //{
        //    //    case SignInStatus.Success:
        //    //        return RedirectToLocal(returnUrl);
        //    //    case SignInStatus.LockedOut:
        //    //        return View("Lockout");
        //    //    case SignInStatus.RequiresVerification:
        //    //        return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
        //    //    case SignInStatus.Failure:
        //    //    default:
        //    //        ModelState.AddModelError("", "Invalid login attempt.");
        //    //        return View(model);
        //    //}
        //    return View(model);
        //}
        #endregion

        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [CaptchaValidation("CaptchaRegoCode", "regoCaptcha", "Incorrect CAPTCHA code!")]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (!_authRepository.AlreadyExists(model.Email))
                {
                    // Try and send the confirmation email first. If that works, and the email is valid, then add it here.
                    
                    var result = _authRepository.Register(model.Email, model.Password);
                    if (result.Success)
                    {
                        // Generate an email confirmation token for this account
                        var token = AuthEncryption.RandomSalt(6);
                        result = _authRepository.SetEmailConfirmationToken(model.Email, token);
                        var secureUrl = Url.Action("ConfirmEmail", "Account", new {e = model.Email, c = token}, "https");

                        string mailBody = MailCommonStrings.RegisteredBody(secureUrl);
                        Mailer.SendEmail_NoWait(model.Email, MailCommonStrings.RegisteredSubject, mailBody);
                        _logger.Debug($"{model.Email} has successfully registered an account from {ClientIpAddress.GetIPAddress(Request)}");

                        MvcCaptcha.ResetCaptcha("regoCaptcha");
                       return RedirectToAction("ConfirmationRequired");
                    }

                    ModelState.AddModelError("", result.Message);
                }
                else
                {
                    ModelState.AddModelError("", "An account with this email address already exists!");
                }

            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public ActionResult ConfirmEmail(string e, string c)
        {
            if (!string.IsNullOrEmpty(e) && !string.IsNullOrEmpty(c))
            {
                if (_authRepository.CanConfirmEmail(e, c))
                {
                    var result = _authRepository.ConfirmEmailAddress(e);
                    if (result.Success)
                    {
                        _logger.Debug($"The email address {e} has been confirmed.");
                        return View();
                    }
                }
            }

            return RedirectToAction("ConfirmationFailed");
        }

        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public ActionResult TryResetPassword(string e, string c)
        {
            if (!string.IsNullOrEmpty(e) && !string.IsNullOrEmpty(c))
            {
                if (_authRepository.CanResetPassword(e, c))
                {
                    return RedirectToAction("ResetPassword", new {e = e, c = c});
                }
            }

            return RedirectToAction("ResetFailed");
        }

        [AllowAnonymous]
        public ActionResult ResetPassword(string e, string c)
        {
            if (!string.IsNullOrEmpty(e) && !string.IsNullOrEmpty(c))
            {
                var model = new PasswordResetViewModel()
                {
                    Code = c,
                    Email = e
                };
                return View(model);
            }
            return RedirectToAction("ResetFailed");
        }

        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [CaptchaValidation("PwChangeCaptchaCode", "ChangePasswordCaptcha", "Incorrect CAPTCHA code!")]
        public async Task<ActionResult> ResetPassword(PasswordResetViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(model.Email) && !string.IsNullOrEmpty(model.Code))
                {
                    if (_authRepository.CanResetPassword(model.Email, model.Code))
                    {
                        var result = _authRepository.ResetPassword(model.Email, model.Password);
                        if (result.Success)
                        {
                            MvcCaptcha.ResetCaptcha("ChangePasswordCaptcha");
                            _logger.Debug(string.Format("The password for {0} has successfully been reset.", model.Email));
                            return RedirectToAction("ResetPasswordConfirmation");
                        }
                    }

                    MvcCaptcha.ResetCaptcha("ChangePasswordCaptcha");
                    return RedirectToAction("ResetFailed");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        /// <summary>
        /// Fired if someone navigates to /Account/ConfirmEmail with no or invalid parameters
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult ConfirmationFailed()
        {
            return View();
        }

        /// <summary>
        /// Fired if someone navigates to /Account/ResetPassword with no or invalid parameters
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult ResetFailed()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult ConfirmationRequired()
        {
            return View();
        }

        //
        // POST: /Account/LogOff
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut();
            return RedirectToAction("Index", "Home");
        }
        

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}