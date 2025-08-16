
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
        private readonly LocalSession _local = new LocalSession();

        // Configure explicit controller/action pairs here (no empty controller unless treated as wildcard below)
        private static readonly HashSet<(string Controller, string Action)> BypassEndpoints =
            new HashSet<(string, string)>(new TupleComparer())
            {
            ("Home", "Index"),
            ("", "ChangePassword"),
            ("", "FLoginChangePassword"),
            ("", "GetLiveSessionDashboard"),
            ("", "MFACodeVerification"),
            ("", "MyProfile")
            };

        // Optional: treat any action name containing "download" as bypass
        private const string DownloadKeyword = "download";

        // Internet reachability cache (simple, thread-safe enough for this use)
        private static DateTime _netCheckExpires = DateTime.MinValue;
        private static bool _netIsUpCached = true;
        private static readonly object _netLock = new object();

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            var ctx = filterContext.HttpContext;

            // ✅ NEW: honor [AllowAnonymous]
            if (IsAnonymousAllowed(filterContext))
                return;

            // 1) Must be authenticated
            if (!(ctx.User?.Identity?.IsAuthenticated ?? false))
            {
                HandleUnauthorizedRequest(filterContext);
                return;
            }

            base.OnAuthorization(filterContext);

            var controller = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            var action = filterContext.ActionDescriptor.ActionName;

            // 2) Bypass endpoints
            if (IsBypassEndpoint(controller, action))
                return;

            // ❌ REMOVE this:
            // if (ctx.Request.IsAjaxRequest()) return;

            // 3) Optional internet check (cached)
            if (!IsInternetAvailableCached())
            {
                filterContext.Result = new RedirectResult("~/Home/NoInternet");
                return;
            }

            // 4) Validate identity/session
            var identity = ctx.User as CustomPrincipal;
            if (identity == null || string.IsNullOrWhiteSpace(identity.SessionCode) || string.IsNullOrWhiteSpace(identity.UserName))
            {
                HandleUnauthorizedRequest(filterContext);
                return;
            }

            var user = _local.GetCurrentUserSession(identity.SessionCode, identity.UserName);
            if (user == null)
            {
                HandleUnauthorizedRequest(filterContext);
                return;
            }

            // 5) Permission check
            if (!HasPermission(user, controller, action))
            {
                HandleUnauthorizedRequest(filterContext);
            }
        }

        private static bool IsAnonymousAllowed(AuthorizationContext context)
        {
            // Action level
            if (context.ActionDescriptor
                     .IsDefined(typeof(AllowAnonymousAttribute), inherit: true))
                return true;

            // Controller level
            if (context.ActionDescriptor.ControllerDescriptor
                     .IsDefined(typeof(AllowAnonymousAttribute), inherit: true))
                return true;

            return false;
        }

        private static bool IsBypassEndpoint(string controller, string action)
        {
            // Exact controller+action
            if (BypassEndpoints.Contains((controller, action)))
                return true;

            // Wildcard by action name keyword (e.g., downloads)
            if (action?.IndexOf(DownloadKeyword, StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            return false;
        }

        private static bool HasPermission(dynamic user, string controller, string action)
        {
            var raw = user?.UserAuthDto?.Permissions as System.Collections.IEnumerable;
            if (raw == null) return false;

            foreach (var o in raw)
            {
                dynamic p = o; // late-bound
                string c = p?.ControllerName as string;
                string a = p?.ActionName as string;

                bool read = false;
                try { read = (bool)(p?.Read ?? false); } catch { read = false; }

                if (read &&
                    string.Equals(c, controller, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(a, action, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }


        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            var ctx = filterContext.HttpContext;
            bool isAjax = ctx.Request.IsAjaxRequest();
            bool isAuthenticated = ctx.User?.Identity?.IsAuthenticated ?? false;

            if (isAjax)
            {
                var jsonResponse = new
                {
                    success = false,
                    message = isAuthenticated
                        ? "You do not have permission to access this resource."
                        : "Your session has expired. Please log in again.",
                    redirectUrl = isAuthenticated ? null : "/Authentication/Login"
                };

                ctx.Response.ContentType = "application/json";
                ctx.Response.StatusCode = isAuthenticated
                    ? (int)HttpStatusCode.Forbidden
                    : (int)HttpStatusCode.Unauthorized;

                ctx.Response.Write(new JavaScriptSerializer().Serialize(jsonResponse));
                ctx.Response.Flush();
                ctx.ApplicationInstance.CompleteRequest();
            }
            else
            {
                filterContext.Result = new RedirectResult(
                    isAuthenticated ? "~/Error/Unauthorized" : "~/Authentication/Login");
            }
        }

        // Cached internet check to avoid per-request external calls
        private static bool IsInternetAvailableCached()
        {
            var now = DateTime.UtcNow;
            if (now < _netCheckExpires) return _netIsUpCached;

            lock (_netLock)
            {
                if (now < _netCheckExpires) return _netIsUpCached;

                bool ok;
                try
                {
                    using (var client = new WebClient())
                    using (client.OpenRead("https://www.google.com/"))
                    {
                        ok = true;
                    }
                }
                catch
                {
                    ok = false;
                }

                _netIsUpCached = ok;
                _netCheckExpires = now.AddMinutes(2); // cache window
                return ok;
            }
        }

        // Case-insensitive tuple comparer for bypass set
        private sealed class TupleComparer : IEqualityComparer<(string Controller, string Action)>
        {
            public bool Equals((string Controller, string Action) x, (string Controller, string Action) y) =>
                string.Equals(x.Controller, y.Controller, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.Action, y.Action, StringComparison.OrdinalIgnoreCase);

            public int GetHashCode((string Controller, string Action) obj) =>
                (obj.Controller?.ToLowerInvariant() + "|" + obj.Action?.ToLowerInvariant()).GetHashCode();
        }
    }




}
