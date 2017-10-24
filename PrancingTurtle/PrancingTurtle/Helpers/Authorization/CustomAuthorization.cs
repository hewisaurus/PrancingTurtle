using System;
using System.Web;
using System.Web.Mvc;
using Common;
using PrancingTurtle.Controllers;

namespace PrancingTurtle.Helpers.Authorization
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class CustomAuthorization : AuthorizeAttribute
    {
        private Type _controller;
        private ActionDescriptor _actionDescriptor;

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Request.IsAuthenticated)
            {
                filterContext.Result = new HttpStatusCodeResult((int)System.Net.HttpStatusCode.Forbidden);
            }
            else
            {
                base.HandleUnauthorizedRequest(filterContext);
            }
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (!httpContext.Request.IsAuthenticated) return false;

            #region Controller checking
            
            if (_controller == typeof(AbilityController) ||
                _controller == typeof(AbilityRoleController) ||
                _controller == typeof(BossFightController) ||
                _controller == typeof(InstanceController) ||
                _controller == typeof(SilentUpdateController))
            {
                return httpContext.User.IsInRole(UserGroups.Admin);
            }

            if (_controller == typeof(GuildController))
            {
                switch (_actionDescriptor.ActionName)
                {
                    case "Remove":
                    case "Approve":
                        return httpContext.User.IsInRole(UserGroups.Admin);
                }
            }
            #endregion
            return base.AuthorizeCore(httpContext);
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            _controller = filterContext.Controller.GetType();
            _actionDescriptor = filterContext.ActionDescriptor;
            
            base.OnAuthorization(filterContext);
        }
    }
}