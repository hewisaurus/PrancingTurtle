using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using Microsoft.AspNet.Identity;
using PrancingTurtle.Helpers.Authorization;

namespace PrancingTurtle.Helpers
{
    public class UserAuthorisationInfo
    {
        public string Username { get; set; }
        public int AuthUserId { get; set; }

        public static UserAuthorisationInfo Get(IPrincipal principal, HttpContextBase context)
        {
            var sid = context.GetOwinContext().Authentication.User.FindFirst(ClaimTypes.Sid);
            if (sid == null || sid.Value != ApplicationSid.Sid)
            {
                return null;
            }

            UserAuthorisationInfo authInfo = new UserAuthorisationInfo();

            try
            {
                authInfo.Username = principal.Identity.GetUserId();
                authInfo.AuthUserId = Convert.ToInt32(context.GetOwinContext().Authentication.User.FindFirst(ClaimTypes.PrimarySid).Value);
                return authInfo;
            }
            catch (Exception ex)
            {
                return authInfo;
            }
        }
    }
}