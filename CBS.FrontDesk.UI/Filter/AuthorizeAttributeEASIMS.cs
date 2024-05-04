
using CBS.FrontDesk.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;

namespace CBS.FrontDesk.UI
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class CheckSessionTimeOutAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext); // Call the base method to perform the default authorization checks.

            // Check if the user is not authenticated, handle unauthorized request, and return.
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                HandleUnauthorizedRequest(filterContext);
                return;
            }

            // Retrieve controller and action names from the action descriptor.
            string controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            string actionName = filterContext.ActionDescriptor.ActionName;
            string url = $"{controllerName}/{actionName}";

            // If action name is "Index", set the URL to only contain the controller name.
            if (actionName.Equals("Index", StringComparison.OrdinalIgnoreCase))
            {
                url = $"{controllerName}";
            }

            // If action name is "FLoginChangePassword", return without further checks.
            if (actionName.Equals("FLoginChangePassword", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // Retrieve menus from session.
            var menus = GetMenus();

            // If menus are not available, return.
            if (menus == null)
            {
                return;
            }

            // Build a string of menu URLs.
            var menuUrls = new StringBuilder();
            foreach (var item in menus)
            {
                menuUrls.Append($"{item.ControllerName}/{item.ActionName}*");
            }

            // Check if the request is not AJAX and the URL is not in the menu URLs.
            if (!IsAjax(filterContext) && !menuUrls.ToString().Contains(url))
            {
                // If the URL is not "/MenuMaster", redirect to unauthorized page.
                if (url != "/MenuMaster")
                {
                    filterContext.Result = new RedirectResult("~/Error/Unauthorized");
                    return;
                }
            }
        }

        // Override to handle unauthorized requests.
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            var httpContext = filterContext.HttpContext;
            var response = httpContext.Response;

            // If the request is AJAX, return unauthorized status code.
            if (IsAjax(filterContext))
            {
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.SuppressFormsAuthenticationRedirect = true;
                response.End();
            }
            // Otherwise, redirect to login page.
            else
            {
                filterContext.Result = new RedirectResult("~/Authentication/Login");
            }
        }

        // Check if the request is AJAX.
        private bool IsAjax(AuthorizationContext filterContext)
        {
            return filterContext.HttpContext.Request.IsAjaxRequest();
        }

        // Get menus from session.
        public List<DatabaseMenus> GetMenus()
        {
            var menus = System.Web.HttpContext.Current.Session?["menu"] as List<DatabaseMenus>;
            return menus ?? new List<DatabaseMenus>();
        }
    }



}
