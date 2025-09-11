using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace CBS.FrontDesk.UI.Filter
{
    public class SessionLockCheckAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var session = filterContext.HttpContext.Session;
            var routeData = filterContext.RouteData;

            string controller = routeData.Values["controller"]?.ToString()?.ToLower();
            string action = routeData.Values["action"]?.ToString()?.ToLower();

            // Allow access to login and session recovery-related pages
            bool isPublicRoute =
                (controller == "authentication") ||
                (controller == "account") ||
                (controller == "session" && (action == "locked" || action == "validaterecoverycode"));

            if (isPublicRoute)
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            if (session != null && (session["SessionUnlocked"] == null || !(bool)session["SessionUnlocked"]))
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary
            {
                { "controller", "Session" },
                { "action", "Locked" }
            });
                return;
            }

            base.OnActionExecuting(filterContext);
        }
    }

}