using CBS.API.Helper;
using CBS.BusinessService;
using CBS.BusinessService.Accounting;
using CBS.BusinessService.Config;
using CBS.BusinessService.UserManagement;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Service;
using CBS.FrontDesk.UI.Filter;
using CBS.FrontDesk.UI.Filters;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;

namespace CBS.FrontDesk.UI
{

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            GlobalHost.DependencyResolver.Register(typeof(ConnectionHub), () => new ConnectionHub());
            GlobalFilters.Filters.Add(new System.Web.Mvc.AuthorizeAttribute());
            UnityConfig.RegisterComponents();
            MvcHandler.DisableMvcResponseHeader = true;
            GlobalFilters.Filters.Add(new UserAuditFilter()); // Register UserAuditFilter
            ValueProviderFactories.Factories.Add(new JsonValueProviderFactory());
            ConnectionMonitoringService connectionService = new ConnectionMonitoringService();

        }
        //protected void Application_AcquireRequestState(object sender, EventArgs e)
        //{
        //    var context = HttpContext.Current;
        //    if (context == null || context.Session == null)
        //        return;

        //    // If user is authenticated, and the session unlock flag is not yet set
        //    if (context.User?.Identity?.IsAuthenticated == true && context.Session["SessionUnlocked"] == null)
        //    {
        //        context.Session["SessionUnlocked"] = true; // ✅ unlock session
        //    }

        //    // Your session lock check to prevent page access (except for allowed routes)
        //    var path = context.Request.Path.ToLower();

        //    bool isAllowed = path.StartsWith("/session/locked") ||
        //                     path.StartsWith("/session/validaterecoverycode") ||
        //                     path.StartsWith("/authentication") ||
        //                     path.StartsWith("/account") ||
        //                     path.StartsWith("/content") ||
        //                     path.StartsWith("/scripts") ||
        //                     path.StartsWith("/favicon") ||
        //                     path.Contains(".axd");

        //    if (!isAllowed)
        //    {
        //        bool unlocked = context.Session["SessionUnlocked"] as bool? ?? false;

        //        if (!unlocked)
        //        {
        //            context.Response.Redirect("~/Session/Locked", true);
        //        }
        //    }
        //}


        protected void Application_EndRequest()
        {
            var response = HttpContext.Current.Response;
            var request = HttpContext.Current.Request;

            // Handle 401 Unauthorized
            if (response.StatusCode == 401)
            {
                response.Clear();
                if (!request.Url.AbsolutePath.EndsWith("/Authentication/Logout", StringComparison.OrdinalIgnoreCase))
                {
                    response.Redirect("~/Authentication/Logout");
                }
            }
            // Log 500 Internal Server Errors
            else if (response.StatusCode == 500)
            {
                var exception = Server.GetLastError(); // Get the last thrown exception
                if (exception != null)
                {
                    string errorDetails = $"500 Error at {request.Url}\n" +
                                        $"Exception: {exception.Message}\n" +
                                        $"Stack Trace: {exception.StackTrace}\n" +
                                        $"Inner Exception: {exception.InnerException?.Message}";


                    // Optionally: Log to a file (ensure permissions)
                    // File.AppendAllText(Server.MapPath("~/App_Data/ErrorLog.txt"), $"{DateTime.Now}: {errorDetails}\n\n");
                }
            }
            // Log other 4xx/5xx errors (optional)
            else if (response.StatusCode >= 400)
            {
                System.Diagnostics.Trace.TraceWarning($"HTTP {response.StatusCode} at {request.Url}");
            }
        }

        //protected void Application_EndRequest()
        //{
        //    if (HttpContext.Current.Response.StatusCode == 401)
        //    {
        //        HttpContext.Current.Response.Clear();
        //        HttpContext.Current.Response.Redirect("~/Authentication/Logout");
        //    }
        //    else
        //    {

        //    }
        //}

        protected void Application_PreSendRequestHeaders()
        {
            // 🔄 Remove existing headers not removed by <remove> in web.config
            Response.Headers.Remove("Server");
            Response.Headers.Remove("X-AspNet-Version");

            // 🛡️ Add branding (not security-sensitive)
            Response.Headers.Add("Server", "SERVER FLUX TSC");
            Response.Headers.Add("X-Powered-By", "FLUXSAL CAMEROON"); // Only if you're OK showing brand

            // ✅ These are already defined in web.config, so DO NOT add them again:
            // • Strict-Transport-Security
            // • X-Frame-Options
            // • X-Content-Type-Options
            // • X-XSS-Protection

            // 🔒 Additional secure headers (not present in web.config)
            if (!Response.Headers.AllKeys.Contains("Referrer-Policy"))
                Response.Headers.Add("Referrer-Policy", "no-referrer");

            if (!Response.Headers.AllKeys.Contains("Permissions-Policy"))
                Response.Headers.Add("Permissions-Policy", "geolocation=(), microphone=(), camera=(), payment=()");

            // 🍪 Secure cookies with best practices
            foreach (var cookieKey in Response.Cookies.AllKeys)
            {
                var cookie = Response.Cookies[cookieKey];
                if (cookie == null) continue;

                cookie.Secure = true; // HTTPS only
                cookie.HttpOnly = true; // No access from JS
                cookie.SameSite = SameSiteMode.Strict; // CSRF protection
            }
        }

        protected void Application_AuthenticateRequest(Object sender, EventArgs e)
        {
            HttpCookie authCookie = Context.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null)
            {
                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value);
                if (ticket.Expiration < DateTime.Now)
                {
                    FormsAuthentication.SignOut();
                    Response.Redirect(FormsAuthentication.LoginUrl);
                }
            }
        }
        //protected void Application_Error(object sender, EventArgs e)
        //{
        //    var exception = Server.GetLastError();
        //    Response.Clear();

        //    // Log the exception (optional)
        //    // Log.Error(exception);

        //    // Display detailed error for local machine only (ensure it's not exposed in production)
        //    if (HttpContext.Current.IsDebuggingEnabled)
        //    {
        //        // Show the error page in debug mode (locally)
        //        Response.Write("<h2>Error Occurred</h2>");
        //        Response.Write("<pre>" + exception.ToString() + "</pre>");
        //        Server.ClearError();
        //    }
        //    else
        //    {
        //        // Redirect to custom error page in production
        //        Server.ClearError();
        //        Response.Redirect("~/Error");
        //    }
        //}

        //protected void Application_Error(object sender, EventArgs e)
        //{
        //    var context = HttpContext.Current;
        //    var exception = Server.GetLastError();
        //    var httpException = exception as HttpException;

        //    // ✅ Skip JSON/AJAX calls to avoid disrupting front-end flow
        //    if (context?.Request?.Headers["X-Requested-With"] == "XMLHttpRequest")
        //    {
        //        Response.Clear();
        //        Response.StatusCode = 500;
        //        Response.ContentType = "application/json";
        //        Response.Write("{ \"success\": false, \"message\": \"An unexpected error occurred.\" }");
        //        Response.End();
        //        return;
        //    }

        //    Response.Flush();
        //    Server.ClearError();

        //    if (httpException != null)
        //    {
        //        var code = httpException.GetHttpCode();
        //        string message = HttpUtility.UrlEncode(httpException.Message);

        //        switch (code)
        //        {
        //            case 400: Response.Redirect("~/Error/BadRequest?message=" + message); break;
        //            case 401: Response.Redirect("~/Error/Unauthorized?message=" + message); break;
        //            case 403: Response.Redirect("~/Error/Forbidden?message=" + message); break;
        //            case 404: Response.Redirect("~/Error/NotFound?message=" + message); break;
        //            case 500: Response.Redirect("~/Error/InternalServer?message=" + message); break;
        //            case 503: Response.Redirect("~/Error/ServiceUnavailable?message=" + message); break;
        //            default: Response.Redirect("~/Error?message=" + message); break;
        //        }
        //    }
        //    else
        //    {
        //        string message = HttpUtility.UrlEncode(exception?.Message ?? "Unexpected error");
        //        Response.Redirect("~/Error?message=" + message);
        //    }
        //}


        //protected void Application_AcquireRequestState(Object sender, EventArgs e)
        //{
        //    var context = HttpContext.Current;

        //    if (context != null && context.Session != null)
        //    {
        //        var encryptedToken = context.Session["EncryptedJWToken"] as string;

        //        // Check if the encrypted token is null or empty
        //        if (string.IsNullOrEmpty(encryptedToken))
        //        {
        //            // Redirect to login if not MFA or Change Password
        //            if (context.Request.Url.AbsolutePath != FormsAuthentication.LoginUrl)
        //            {
        //                FormsAuthentication.SignOut();
        //                context.Session.Clear();
        //                context.Session.Abandon();
        //                context.Response.Clear(); // Clear any existing content
        //                context.Response.Redirect(FormsAuthentication.LoginUrl, false); // Set endResponse to false
        //                context.ApplicationInstance.CompleteRequest(); // Complete the request without aborting the thread
        //                return;
        //            }
        //        }
        //        else
        //        {
        //            try
        //            {
        //                // Decrypt and validate the token
        //                var token = TokenEncryptionHelper.DecryptToken(encryptedToken);
        //                var handler = new JwtSecurityTokenHandler();
        //                var jwtToken = handler.ReadJwtToken(token);

        //                if (jwtToken.ValidTo > DateTime.UtcNow)
        //                {
        //                    // Set up the user principal with the JWT claims
        //                    SetupUserPrincipal(jwtToken);

        //                    context.Session.Timeout = (int)(jwtToken.ValidTo - DateTime.UtcNow).TotalMinutes;
        //                }
        //                else
        //                {
        //                    // Token expired, remove session and redirect to login
        //                    context.Session.Remove("EncryptedJWToken");
        //                    FormsAuthentication.SignOut();
        //                    context.Session.Clear();
        //                    context.Session.Abandon();
        //                    if (context.Request.Url.AbsolutePath != FormsAuthentication.LoginUrl)
        //                    {
        //                        context.Response.Clear(); // Clear any existing content
        //                        context.Response.Redirect(FormsAuthentication.LoginUrl, false); // Set endResponse to false
        //                        context.ApplicationInstance.CompleteRequest(); // Complete the request without aborting the thread
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                // Handle exception, clean up session, and redirect to login
        //                context.Session.Remove("EncryptedJWToken");
        //                FormsAuthentication.SignOut();
        //                context.Session.Clear();
        //                context.Session.Abandon();
        //                if (context.Request.Url.AbsolutePath != FormsAuthentication.LoginUrl)
        //                {
        //                    context.Response.Clear(); // Clear any existing content
        //                    context.Response.Redirect(FormsAuthentication.LoginUrl, false); // Set endResponse to false
        //                    context.ApplicationInstance.CompleteRequest(); // Complete the request without aborting the thread
        //                }
        //            }
        //        }
        //    }
        //}


        protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
        {
            HttpCookie authCookie = Request.Cookies["TSC"];
            if (authCookie != null)
            {
                FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                if (!authTicket.Expired)
                {
                    var user = JsonConvert.DeserializeObject<CustomSerializeModel>(authTicket.UserData);

                    CustomPrincipal principal = new CustomPrincipal(authTicket.Name)
                    {
                        UserId = user.Id,
                        FullName = user.FullName,
                        UserName = user.UserName,
                        Roles = user.RoleName,
                        SessionID = user.SessionID,
                        Email = user.Email,
                        SessionCode = user.SessionCode,
                        Phonenumber = user.Phonenumber
                    };

                    HttpContext.Current.User = principal;

                
                }
            }
        }




    }
}
