using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PrancingTurtle.Helpers.Controllers
{
    public class BaseController : Controller
    {
        public static UserAuthorisationInfo UAuthInfo;
        public static string CurrentController;
        public static string CurrentAction;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            CurrentController = ControllerContext.RouteData.Values["controller"].ToString().ToUpper();
            CurrentAction = ControllerContext.RouteData.Values["action"].ToString().ToUpper();

            if (Request.IsAuthenticated)
            {
                // Check that we have a valid user
                // If the Sid is not set or is null, then the user is logged in but not with this application and should be signed out
                UAuthInfo = UserAuthorisationInfo.Get(User, HttpContext);
                if (UAuthInfo == null)
                {
                    HttpContext.GetOwinContext().Authentication.SignOut();
                    RedirectToAction("Index", "Home");
                    return;
                }
            }
            else
            {
                UAuthInfo = null;
            }
            base.OnActionExecuting(filterContext);
        }
    }
}