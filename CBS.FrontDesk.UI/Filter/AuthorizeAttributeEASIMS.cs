
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


   [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    //public class CheckSessionTimeOutAttribute : AuthorizeAttribute
    //{
    //    LocalSession local = new LocalSession();

    //    public override void OnAuthorization(AuthorizationContext filterContext)
    //    {
        

    //        // ✅ Handle unauthenticated users immediately
    //        if (filterContext.HttpContext.User == null || !filterContext.HttpContext.User.Identity.IsAuthenticated)
    //        {
    //            HandleUnauthorizedRequest(filterContext);
    //            return;
    //        }
      
    //        base.OnAuthorization(filterContext);

    //        string controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
    //        string actionName = filterContext.ActionDescriptor.ActionName;
    //        string url = $"/{controllerName}/{actionName}";
    //        // 🌐 Check Internet Connectivity
    //        if (!IsInternetAvailable())
    //        {
    //            // Handle offline scenario
    //            filterContext.Result = new RedirectResult("~/Home/NoInternet"); // Redirect to a "No Internet" page or handle it appropriately
    //            return;
    //        }
    //        // ✅ Skip access check for specific pages
    //        if (
    //               actionName.Equals("ChangePassword", StringComparison.OrdinalIgnoreCase) ||
    //               actionName.Equals("FLoginChangePassword", StringComparison.OrdinalIgnoreCase) ||
    //               actionName.Equals("GetLiveSessionDashboard", StringComparison.OrdinalIgnoreCase) ||
    //               actionName.Equals("MFACodeVerification", StringComparison.OrdinalIgnoreCase) ||
    //               actionName.Equals("MyProfile", StringComparison.OrdinalIgnoreCase) ||
    //               actionName.ToLower().Contains("download") ||
    //               (controllerName.Equals("Home", StringComparison.OrdinalIgnoreCase) &&
    //                actionName.Equals("Index", StringComparison.OrdinalIgnoreCase))
    //           )
    //        {
    //            return;
    //        }



    //        // ✅ Extract user identity (CustomPrincipal)
    //        var identity = filterContext.HttpContext.User as CustomPrincipal;
    //        if (identity == null || string.IsNullOrWhiteSpace(identity.SessionCode) || string.IsNullOrWhiteSpace(identity.UserName))
    //        {
    //            HandleUnauthorizedRequest(filterContext);
    //            return;
    //        }
    //        if (IsAjax(filterContext))
    //        {
    //            //filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
    //            //filterContext.HttpContext.Response.SuppressFormsAuthenticationRedirect = true;
    //            //filterContext.HttpContext.Response.End();
    //        }
    //        // ✅ Get session info
    //        var user = local.GetCurrentUserSession(identity.SessionCode, identity.UserName);
    //        if (user == null)
    //        {
    //            HandleUnauthorizedRequest(filterContext);
    //            return;
    //        }

    //        // ✅ Permission check
    //        if (!HasPermission(user, controllerName, actionName))
    //        {
    //            if (IsAjax(filterContext))
    //            {
    //                //filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
    //                //filterContext.HttpContext.Response.SuppressFormsAuthenticationRedirect = true;
    //                //filterContext.HttpContext.Response.End();
    //            }
    //            else
    //            {
    //                filterContext.Result = new RedirectResult("~/Error/Unauthorized");
    //            }
    //            return;
    //        }
    //    }

    //    private bool HasPermission(dynamic user, string controller, string action)
    //    {
    //        if (user?.UserAuthDto?.Permissions == null)
    //            return false;

    //        foreach (var permission in user.UserAuthDto.Permissions)
    //        {
    //            if (permission.ControllerName?.Equals(controller, StringComparison.OrdinalIgnoreCase) == true &&
    //                permission.ActionName?.Equals(action, StringComparison.OrdinalIgnoreCase) == true &&
    //                permission.Read)
    //            {
    //                return true;
    //            }
    //        }

    //        return false;
    //    }

    //    protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
    //    {
    //        var httpContext = filterContext.HttpContext;
    //        var request = httpContext.Request;
    //        var response = httpContext.Response;
    //        var user = httpContext.User;

    //        bool isAjax = request.IsAjaxRequest();
    //        bool isAuthenticated = user?.Identity?.IsAuthenticated == true;

    //        if (isAjax)
    //        {
    //            response.Clear();
    //            response.ContentType = "application/json";
    //            response.StatusCode = isAuthenticated ? (int)HttpStatusCode.Forbidden : (int)HttpStatusCode.Unauthorized;
    //            response.SuppressFormsAuthenticationRedirect = true;

    //            var json = new
    //            {
    //                success = false,
    //                message = isAuthenticated
    //                    ? "You do not have permission to access this resource."
    //                    : "Your session has expired. Please log in again.",
    //                redirectUrl = isAuthenticated ? null : "/Authentication/Login"
    //            };

    //            response.Write(new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(json));
    //            // ✅ Instead of End() use:
    //            response.Flush();
    //            httpContext.ApplicationInstance.CompleteRequest(); // gracefully complete the request
    //        }
    //        else
    //        {
    //            filterContext.Result = isAuthenticated
    //                ? new RedirectResult("~/Error/Unauthorized")
    //                : new RedirectResult("~/Authentication/Login");
    //        }
    //    }

    //    private bool IsInternetAvailable()
    //    {
    //        try
    //        {
    //            using (var client = new WebClient())
    //            {
    //                using (client.OpenRead("http://www.google.com"))
    //                {
    //                    return true;
    //                }
    //            }
    //        }
    //        catch
    //        {
    //            return false;
    //        }
    //    }

    //    private bool IsAjax(AuthorizationContext filterContext)
    //    {
    //        return filterContext.HttpContext.Request.IsAjaxRequest();
    //    }
    //}

 


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
            try
            {
                using (var client = new WebClient())
                {
                    using (client.OpenRead("https://www.youtube.com/"))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        private bool IsAjax(AuthorizationContext filterContext)
        {
            return filterContext.HttpContext.Request.IsAjaxRequest();
        }
    }


    //[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]




    //public class CheckSessionTimeOutAttribute : AuthorizeAttribute
    //{
    //    public override void OnAuthorization(AuthorizationContext filterContext)
    //    {
    //        base.OnAuthorization(filterContext); // Call the base method to perform the default authorization checks.

    //        // Check if the user is not authenticated, handle unauthorized request, and return.
    //        if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
    //        {
    //            HandleUnauthorizedRequest(filterContext);
    //            return;
    //        }

    //        // Retrieve controller and action names from the action descriptor.
    //        string controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
    //        string actionName = filterContext.ActionDescriptor.ActionName;
    //        string url = $"{controllerName}/{actionName}";

    //        // If action Name is "Index", set the URL to only contain the controller Name.
    //        if (actionName.Equals("Index", StringComparison.OrdinalIgnoreCase))
    //        {
    //            url = $"{controllerName}";
    //        }

    //        // If action Name is "FLoginChangePassword", return without further checks.
    //        if (actionName.Equals("FLoginChangePassword", StringComparison.OrdinalIgnoreCase))
    //        {
    //            return;
    //        }

    //        // Retrieve menus from session.
    //        var menus = GetMenus();

    //        // If menus are not available, or the URL is not in the menu URLs, proceed with further checks.
    //        if (menus == null || !IsUrlInMenu(url, menus))
    //        {
    //            // Check if the request is made with jQuery or AJAX.
    //            if (IsAjaxOrJQuery(filterContext))
    //            {
    //                // If it's jQuery or AJAX, authorize the request.
    //                return;
    //            }
    //            else
    //            {
    //                // If it's not AJAX or jQuery, handle unauthorized request and return the URL.
    //                //HandleUnauthorizedRequest(filterContext, url);
    //                //return;
    //            }
    //        }

    //        // If the URL is in the user's menu, no further checks are required.
    //    }

    //    // Override to handle unauthorized requests.
    //    // Override to handle unauthorized requests.
    //    protected virtual void HandleUnauthorizedRequest(AuthorizationContext filterContext, string url)
    //    {
    //        var httpContext = filterContext.HttpContext;
    //        var response = httpContext.Response;

    //        // If the request is AJAX or made with jQuery, return unauthorized status code along with the URL.
    //        if (IsAjaxOrJQuery(filterContext))
    //        {
    //            response.StatusCode = (int)HttpStatusCode.Unauthorized;
    //            response.SuppressFormsAuthenticationRedirect = true;
    //            response.Write(url); // Return the URL in the response
    //            response.End();
    //        }
    //        // For non-AJAX or non-jQuery requests, redirect to unauthorized page.
    //        else
    //        {
    //            response.Redirect("~/Error/Unauthorized");
    //        }
    //    }

    //    // Check if the request is AJAX or made with jQuery.
    //    private bool IsAjaxOrJQuery(AuthorizationContext filterContext)
    //    {
    //        var httpRequest = filterContext.HttpContext.Request;
    //        return httpRequest.IsAjaxRequest() || httpRequest.Headers["X-Requested-With"] == "XMLHttpRequest";
    //    }

    //    // Check if the URL is in the user's menu.
    //    private bool IsUrlInMenu(string url, List<DatabaseMenus> menus)
    //    {
    //        foreach (var item in menus)
    //        {

    //            string urlx = $"{item.ControllerName}/{item.ActionName}";
    //            if (urlx.Contains(url))
    //            {
    //                return true;
    //            }
    //        }
    //        return false;
    //    }

    //    // GetAllowAnonymous menus from session.
    //    public List<DatabaseMenus> GetMenus()
    //    {
    //        var menus = HttpContext.Current.Session?["menu"] as List<DatabaseMenus>;
    //        return menus ?? new List<DatabaseMenus>();
    //    }
    //}


}
