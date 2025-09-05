
using CBS.BusinessService.Session;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Script.Serialization;
using System.Web.Security;

namespace CBS.FrontDesk.UI {


    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Script.Serialization;
    using System.Net.Http; // only if you switch to HttpClient later
    using System.Threading; // if you later add async
    using System.Globalization;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class CheckSessionTimeOutAttribute : AuthorizeAttribute
    {
        LocalSession local = new LocalSession();

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            // Unauthenticated User Handling
            if (filterContext.HttpContext.User == null || !filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                HandleUnauthorizedRequest(filterContext);
                return;
            }

            base.OnAuthorization(filterContext);

            string controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            string actionName = filterContext.ActionDescriptor.ActionName;

            // Internet Connectivity Check
            if (!IsInternetAvailable())
            {
                filterContext.Result = new RedirectResult("~/Home/NoInternet");
                return;
            }

            // Bypass Access Check for Specific Pages
            if (
                actionName.Equals("ChangePassword", StringComparison.OrdinalIgnoreCase) ||
                actionName.Equals("FLoginChangePassword", StringComparison.OrdinalIgnoreCase) ||
                actionName.Equals("GetLiveSessionDashboard", StringComparison.OrdinalIgnoreCase) ||
                actionName.Equals("MFACodeVerification", StringComparison.OrdinalIgnoreCase) ||
                actionName.Equals("MyProfile", StringComparison.OrdinalIgnoreCase) ||
                actionName.ToLower().Contains("download") ||
                (controllerName.Equals("Home", StringComparison.OrdinalIgnoreCase) && actionName.Equals("Index", StringComparison.OrdinalIgnoreCase))
            )
            {
                return;
            }

            // If AJAX request, authorize immediately
            if (IsAjax(filterContext))
            {
                return;
            }

            // Extract User Identity
            var identity = filterContext.HttpContext.User as CustomPrincipal;
            if (identity == null || string.IsNullOrWhiteSpace(identity.SessionCode) || string.IsNullOrWhiteSpace(identity.UserName))
            {
                HandleUnauthorizedRequest(filterContext);
                return;
            }

            // Get User Session
            var user = local.GetCurrentUserSession(identity.SessionCode, identity.UserName);
            if (user == null)
            {
                HandleUnauthorizedRequest(filterContext);
                return;
            }

            // Permission Check
            if (!HasPermission(user, controllerName, actionName))
            {
                HandleUnauthorizedRequest(filterContext);
                return;
            }
        }
        private bool HasPermission(dynamic user, string controller, string action)
        {
            if (user?.UserAuthDto?.Permissions == null)
                return false;

            foreach (var permission in user.UserAuthDto.Permissions)
            {
                if (permission.ControllerName?.Equals(controller, StringComparison.OrdinalIgnoreCase) == true &&
                    permission.ActionName?.Equals(action, StringComparison.OrdinalIgnoreCase) == true &&
                    permission.Read)
                {
                    return true;
                }
            }

            return false;
        }


        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            var isAjax = filterContext.HttpContext.Request.IsAjaxRequest();
            var isAuthenticated = filterContext.HttpContext.User?.Identity?.IsAuthenticated == true;

            if (isAjax)
            {
                var jsonResponse = new
                {
                    success = false,
                    message = isAuthenticated ? "You do not have permission to access this resource." : "Your session has expired. Please log in again.",
                    redirectUrl = isAuthenticated ? null : "/Authentication/Login"
                };

                filterContext.HttpContext.Response.ContentType = "application/json";
                filterContext.HttpContext.Response.StatusCode = isAuthenticated ? (int)HttpStatusCode.Forbidden : (int)HttpStatusCode.Unauthorized;
                filterContext.HttpContext.Response.Write(new JavaScriptSerializer().Serialize(jsonResponse));
                filterContext.HttpContext.Response.Flush();
                filterContext.HttpContext.ApplicationInstance.CompleteRequest();
            }
            else
            {
                filterContext.Result = isAuthenticated ? new RedirectResult("~/Error/Unauthorized") : new RedirectResult("~/Authentication/Login");
            }
        }

        private bool IsInternetAvailable()
        {
            return true;

            //try
            //{
            //    using (var client = new WebClient())
            //    {
            //        using (client.OpenRead("https://www.youtube.com/"))
            //        {
            //            return true;
            //        }
            //    }
            //}
            //catch
            //{
            //    return false;
            //}
        }

        private bool IsAjax(AuthorizationContext filterContext)
        {
            return filterContext.HttpContext.Request.IsAjaxRequest();
        }
    }







}
