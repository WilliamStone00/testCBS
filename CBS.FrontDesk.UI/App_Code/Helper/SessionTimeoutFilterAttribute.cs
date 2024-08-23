namespace CBS.FrontDesk.UI.App_Code.Helper
{
    using System;
    using System.Web;
    using System.Web.Mvc;

    public class SessionTimeoutFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            HttpContextBase httpContext = filterContext.HttpContext;

            // Check if the session has expired
            if (httpContext.Session != null && httpContext.Session.IsNewSession)
            {
                string sessionCookie = httpContext.Request.Headers["Cookie"];
                if (sessionCookie != null && sessionCookie.IndexOf("ASP.NET_SessionId", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    HandleExpiredSession(filterContext);
                    return;
                }
            }

            base.OnActionExecuting(filterContext);
        }

        private void HandleExpiredSession(ActionExecutingContext filterContext)
        {
            if (!filterContext.HttpContext.Request.IsAuthenticated)
            {
                string returnUrl = filterContext.HttpContext.Request.RawUrl;
                filterContext.HttpContext.Session["ReturnUrl"] = returnUrl;
            }

            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                filterContext.Result = new JsonResult
                {
                    Data = new { redirectTo = "~/Authentication/Login" },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            else
            {
                filterContext.Result = new RedirectResult("~/Authentication/Login");
            }
        }

        private void InvalidateCookie(string cookieName)
        {
            HttpCookie cookie = new HttpCookie(cookieName);
            cookie.Expires = DateTime.Now.AddDays(-1);
            HttpContext.Current.Response.Cookies.Add(cookie);
        }
    }

    //public class SessionTimeoutFilterAttribute : ActionFilterAttribute
    //    {
    //        public override void OnActionExecuting(ActionExecutingContext filterContext)
    //        {
    //            HttpContextBase httpContext = filterContext.HttpContext;

    //            // Check if the session has expired
    //            if (httpContext.Session != null && httpContext.Session.IsNewSession)
    //            {
    //                string sessionCookie = httpContext.Request.Headers["Cookie"];
    //                if (sessionCookie != null && sessionCookie.IndexOf("ASP.NET_SessionId", StringComparison.OrdinalIgnoreCase) >= 0)
    //                {
    //                    HandleExpiredSession(filterContext);
    //                    return;
    //                }
    //            }

    //            // Check if the authentication token is expired
    //            HttpCookie authCookie = httpContext.Request.Cookies["Token"];
    //            if (authCookie != null)
    //            {
    //                FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);

    //                // Check if the ticket exists and if it's expired
    //                if (authTicket != null && authTicket.Expiration < DateTime.Now)
    //                {
    //                    InvalidateCookie("Token");
    //                    HandleExpiredSession(filterContext);
    //                    return;
    //                }
    //            }
    //            else
    //            {
    //                HandleExpiredSession(filterContext);
    //                return;
    //            }

    //            base.OnActionExecuting(filterContext);
    //        }

    //        private void HandleExpiredSession(ActionExecutingContext filterContext)
    //        {
    //            if (filterContext.HttpContext.Request.IsAjaxRequest())
    //            {
    //                filterContext.Result = new JsonResult
    //                {
    //                    Data = new { redirectTo = "~/Authentication/Login" },
    //                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
    //                };
    //            }
    //            else
    //            {
    //                filterContext.Result = new RedirectResult("~/Authentication/Login");
    //            }
    //        }

    //        private void InvalidateCookie(string cookieName)
    //        {
    //            HttpCookie cookie = new HttpCookie(cookieName);
    //            cookie.Expires = DateTime.Now.AddDays(-1);
    //            HttpContext.Current.Response.Cookies.Add(cookie);
    //        }
    //    }
}