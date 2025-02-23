using CBS.API.Helper;
using CBS.BusinessService;
using CBS.BusinessService.Accounting;
using CBS.BusinessService.Config;
using CBS.BusinessService.UserManagement;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Service;
using CBS.FrontDesk.UI.Filters;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using System;
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
            ConnectionMonitoringService connectionService = new ConnectionMonitoringService();
        }
       
      
        protected void Application_EndRequest()
        {
            if (HttpContext.Current.Response.StatusCode == 401)
            {
                HttpContext.Current.Response.Clear();
                HttpContext.Current.Response.Redirect("~/Authentication/Logout");
            }
        }

        protected void Application_PreSendRequestHeaders()
        {
            // Remove server version details and set custom server Name
            Response.Headers.Remove("Server");
            Response.Headers.Remove("X-AspNet-Version");
            Response.Headers.Remove("X-Powered-By");
            Response.Headers.Add("Server", "Flux Server TBS");
            Response.Headers.Add("X-Powered-By", "Flux");
            Response.Headers.Add("X-Content-Type-Options", "nosniff");
            Response.Headers.Add("X-Frame-Options", "DENY");
            Response.Headers.Add("X-XSS-Protection", "1; mode=block");
            Response.Headers.Add("Referrer-Policy", "no-referrer");
            Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
            Response.Headers.Add("Permissions-Policy", "geolocation=(), microphone=(), camera=(), payment=()");

            // Set secure cookie attributes
            foreach (var cookieKey in Response.Cookies.AllKeys)
            {
                Response.Cookies[cookieKey].Secure = true; // Requires HTTPS
                Response.Cookies[cookieKey].HttpOnly = true; // Helps mitigate XSS attacks
                Response.Cookies[cookieKey].SameSite = SameSiteMode.Strict; // Prevents CSRF attacks
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

        protected void Application_Error(object sender, EventArgs e)
        {
            var exception = Server.GetLastError();
            var httpException = exception as HttpException;

            Response.Clear();
            Server.ClearError();

            if (httpException != null)
            {
                int errorCode = httpException.GetHttpCode();

                // Redirect based on error code
                switch (errorCode)
                {
                    case 400:
                        Response.Redirect("~/Error/BadRequest?message=" + httpException.Message);
                        break;
                    case 401:
                        Response.Redirect("~/Error/Unauthorized?message=" + httpException.Message);
                        break;
                    case 403:
                        Response.Redirect("~/Error/Forbidden?message=" + httpException.Message);
                        break;
                    case 404:
                        Response.Redirect("~/Error/NotFound?message=" + httpException.Message);
                        break;
                    case 500:
                        Response.Redirect("~/Error/InternalServer?message=" + httpException.Message);
                        break;
                    case 503:
                        Response.Redirect("~/Error/ServiceUnavailable?message=" + httpException.Message);
                        break;
                    // Add more cases for other error codes if needed
                    default:
                        Response.Redirect("~/Error?message=" + httpException.Message);
                        break;
                }
            }
            else
            {
                // Redirect to a generic error page for other types of exceptions
                Response.Redirect("~/Error?message=" + exception.Message);
            }
        }

        private void SetupUserPrincipal(JwtSecurityToken jwtToken)
        {
            var claims = jwtToken.Claims.ToList();
            var identity = new ClaimsIdentity(claims, "Jwt");
            var principal = new ClaimsPrincipal(identity);

            HttpContext.Current.User = principal;
            Thread.CurrentPrincipal = principal;
        }

        protected void Application_AcquireRequestState(Object sender, EventArgs e)
        {
            var context = HttpContext.Current;

            if (context != null && context.Session != null)
            {
                var encryptedToken = context.Session["EncryptedJWToken"] as string;

                // Check if the encrypted token is null or empty
                if (string.IsNullOrEmpty(encryptedToken))
                {
                    // Redirect to login if not MFA or Change Password
                    if (context.Request.Url.AbsolutePath != FormsAuthentication.LoginUrl)
                    {
                        FormsAuthentication.SignOut();
                        context.Session.Clear();
                        context.Session.Abandon();
                        context.Response.Clear(); // Clear any existing content
                        context.Response.Redirect(FormsAuthentication.LoginUrl, false); // Set endResponse to false
                        context.ApplicationInstance.CompleteRequest(); // Complete the request without aborting the thread
                        return;
                    }
                }
                else
                {
                    try
                    {
                        // Decrypt and validate the token
                        var token = TokenEncryptionHelper.DecryptToken(encryptedToken);
                        var handler = new JwtSecurityTokenHandler();
                        var jwtToken = handler.ReadJwtToken(token);

                        if (jwtToken.ValidTo > DateTime.UtcNow)
                        {
                            // Set up the user principal with the JWT claims
                            SetupUserPrincipal(jwtToken);

                            context.Session.Timeout = (int)(jwtToken.ValidTo - DateTime.UtcNow).TotalMinutes;
                        }
                        else
                        {
                            // Token expired, remove session and redirect to login
                            context.Session.Remove("EncryptedJWToken");
                            FormsAuthentication.SignOut();
                            context.Session.Clear();
                            context.Session.Abandon();
                            if (context.Request.Url.AbsolutePath != FormsAuthentication.LoginUrl)
                            {
                                context.Response.Clear(); // Clear any existing content
                                context.Response.Redirect(FormsAuthentication.LoginUrl, false); // Set endResponse to false
                                context.ApplicationInstance.CompleteRequest(); // Complete the request without aborting the thread
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Handle exception, clean up session, and redirect to login
                        context.Session.Remove("EncryptedJWToken");
                        FormsAuthentication.SignOut();
                        context.Session.Clear();
                        context.Session.Abandon();
                        if (context.Request.Url.AbsolutePath != FormsAuthentication.LoginUrl)
                        {
                            context.Response.Clear(); // Clear any existing content
                            context.Response.Redirect(FormsAuthentication.LoginUrl, false); // Set endResponse to false
                            context.ApplicationInstance.CompleteRequest(); // Complete the request without aborting the thread
                        }
                    }
                }
            }
        }






    }
}
