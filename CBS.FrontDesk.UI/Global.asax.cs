using CBS.API.Helper;
using CBS.BusinessService;
using CBS.BusinessService.Accounting;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.BusinessService.UserManagement;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Service;
using CBS.FrontDesk.UI.Controllers.ErrorHandler;
using CBS.FrontDesk.UI.Filter;
using CBS.FrontDesk.UI.Filters;
using CBS.FrontDesk.UI.Utility.Middlware_logger;
using CBS.FrontDesk.UI.WAF.Middleware.Core;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
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
using static RateLimitConfigService;

namespace CBS.FrontDesk.UI
{

    public class MvcApplication : System.Web.HttpApplication
    {
        public static readonly string EnvironmentName =
        ConfigurationManager.AppSettings["URLConf_Environment"]?.Trim() ?? "Production";

        public static readonly List<string> AllowedOriginDomains =
            (ConfigurationManager.AppSettings["AllowedOrigins"] ?? "")
            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select((origin, index) => new { origin = origin.Trim(), index })
            .Where(entry =>
            {
                if (EnvironmentName == "Development" && entry.index == 0)
                    return true;
                if (EnvironmentName == "TestBed" && entry.index == 1)
                    return true;
                if (EnvironmentName == "Production" && entry.index == 2)
                    return true;
                return false;
            })
            .Select(entry =>
            {
                try
                {
                    return new Uri(entry.origin).GetLeftPart(UriPartial.Authority).ToLowerInvariant();
                }
                catch
                {
                    return null;
                }
            })
            .Where(origin => !string.IsNullOrWhiteSpace(origin))
            .Distinct()
            .ToList();
 
   
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
            //GlobalFilters.Filters.Add(new UserAuditFilter()); // Register UserAuditFilter
            ValueProviderFactories.Factories.Add(new JsonValueProviderFactory());
            //ConnectionMonitoringService connectionService = new ConnectionMonitoringService();
            //var configService = new RateLimitConfigService(); // or resolve from container
            //RateLimitConfigHolder.LoadAsync(configService).GetAwaiter().GetResult(); // One-time safe call
            // Start IP Unblock Service
            //new UnblockIpService();
        }
        protected void Application_BeginRequest()
        {
            // ✅ Use the correct key and handle TestBed too
            var env = ConfigurationManager.AppSettings["URLConf_Environment"]?.Trim() ?? "Production";
            bool isProdOrTb = env.Equals("Production", StringComparison.OrdinalIgnoreCase)
                           || env.Equals("TestBed", StringComparison.OrdinalIgnoreCase);

            var ctx = HttpContext.Current;
            var req = ctx?.Request;
            var resp = ctx?.Response;

            if (req == null || resp == null) return;

            var host = req.Url?.Host?.ToLowerInvariant() ?? string.Empty;

            // ✅ Block localhost only in Production/TestBed, without Response.End()
            if (isProdOrTb && host.Contains("localhost"))
            {
                resp.StatusCode = 403;
                resp.StatusDescription = "Localhost access is blocked in Production/TestBed";
                resp.Write("Forbidden");
                resp.Flush();
                ctx.ApplicationInstance.CompleteRequest();
                return;
            }

            //// ✅ Avoid redirect loops behind proxies; prefer Web.config rewrite.
            //// If you KEEP this as a fallback, honor X-Forwarded-Proto and don't use Response.End().
            //var xfProto = req.Headers["X-Forwarded-Proto"];
            //bool isHttps = req.IsSecureConnection
            //            || string.Equals(xfProto, "https", StringComparison.OrdinalIgnoreCase);

            //if (!isHttps)
            //{
            //    // If you already have a Web.config HTTPS rewrite, you can DELETE this whole block.
            //    var ub = new UriBuilder(req.Url) { Scheme = Uri.UriSchemeHttps, Port = -1 }; // -1 = default port
            //    resp.RedirectPermanent(ub.Uri.ToString(), endResponse: false);
            //    ctx.ApplicationInstance.CompleteRequest();
            //    return;
            //}

            // ✅ Culture: Session may be null in BeginRequest; prefer cookie first, then session if available
            string lang = req.Cookies["TSC_Lang"]?.Value;

            if (string.IsNullOrWhiteSpace(lang) && ctx.Session != null)
                lang = ctx.Session["SelectedLanguage"] as string;

            if (string.IsNullOrWhiteSpace(lang))
                lang = "en";

            try
            {
                var ci = CultureInfo.GetCultureInfo(lang);
                Thread.CurrentThread.CurrentCulture = ci;
                Thread.CurrentThread.CurrentUICulture = ci;
            }
            catch
            {
                var ci = CultureInfo.GetCultureInfo("en");
                Thread.CurrentThread.CurrentCulture = ci;
                Thread.CurrentThread.CurrentUICulture = ci;
            }
        }




        protected void Application_EndRequest()
        {
            if (HttpContext.Current?.Response?.Cookies != null)
            {
                foreach (string cookieKey in HttpContext.Current.Response.Cookies.AllKeys)
                {
                    var cookie = HttpContext.Current.Response.Cookies[cookieKey];
                    cookie.Secure = true;
                    cookie.HttpOnly = true;
                    cookie.SameSite = SameSiteMode.Strict;
                }
            }
        }
        protected void Application_End()
        {
            AdvancedMiddlewareLogger.Shutdown();
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
        protected void Application_Error(object sender, EventArgs e)
        {
            var ex = Server.GetLastError();
            if (ex is HttpException httpEx && httpEx.GetHttpCode() == 429)
            {
                Response.Clear();
                Response.StatusCode = 429;
                Response.Write("Too Many Requests. Please try again later.");
                Response.Flush();
                HttpContext.Current.ApplicationInstance.CompleteRequest();
            }
        }



        public class CorrelationIdActionFilter : ActionFilterAttribute
        {
            public override void OnActionExecuting(ActionExecutingContext filterContext)
            {
                var httpContext = filterContext.HttpContext;
                var correlationId =
                    httpContext.Items["CorrelationId"] as string ??
                    httpContext.Request.Headers[CorrelationConstants.HeaderKey] ??
                    Guid.NewGuid().ToString("N");

                // keep it in Items for downstream code
                httpContext.Items["CorrelationId"] = correlationId;

                // ❌ DO NOT write headers here – can throw if response already started
                // httpContext.Response.Headers[CorrelationConstants.HeaderKey] = correlationId;
                // httpContext.Response.AddHeader(...);  // <-- remove
            }
        }
        protected void Application_PreSendRequestHeaders()
        {
            var ctx = HttpContext.Current;
            if (ctx == null) return;

            var id = ctx.Items["CorrelationId"] as string;
            if (string.IsNullOrWhiteSpace(id)) return;

            // If already set (e.g., by WAF), do nothing
            if (!ctx.Response.Headers.AllKeys.Contains(CorrelationConstants.HeaderKey))
            {
                // This runs before headers are sent, so it’s safe
                ctx.Response.AppendHeader(CorrelationConstants.HeaderKey, id);
            }
        }


        protected void Application_PostAuthenticateRequest(object sender, EventArgs e)
        {
            try
            {
                HttpCookie authCookie = HttpContext.Current?.Request?.Cookies["TSC"];
                if (authCookie == null || string.IsNullOrWhiteSpace(authCookie.Value))
                    return;

                FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                if (authTicket == null || authTicket.Expired)
                    return;

                var user = JsonConvert.DeserializeObject<CustomSerializeModel>(authTicket.UserData);
                if (user == null || string.IsNullOrWhiteSpace(user.UserName))
                    return;

                var principal = new CustomPrincipal(authTicket.Name)
                {
                    UserId = user.Id,
                    FullName = user.FullName,
                    UserName = user.UserName,
                    Roles = user.RoleName,
                    SessionIP = user.SessionIP,
                    SessionUserAgent = user.SessionUserAgent,
                    SessionID = user.SessionID,
                    Email = user.Email,
                    SessionCode = user.SessionCode,
                    Phonenumber = user.Phonenumber,
                    IsAuthenticated = true
                };

                HttpContext.Current.User = principal;
                System.Threading.Thread.CurrentPrincipal = principal;

                // ✅ Ensure session is hydrated if not already
                if (HttpContext.Current.Session != null && HttpContext.Current.Session["UserID"] == null)
                {
                    RehydrateSession(user);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ PostAuthenticateRequest failed: {ex.Message}");
                // Optionally: log securely using your logger (e.g., NLog/Serilog)
            }
        }




        private void RehydrateSession(CustomSerializeModel user)
        {
            HttpContext.Current.Session["UserID"] = user.Id;
            HttpContext.Current.Session["FullName"] = user.FullName;
            HttpContext.Current.Session["UserName"] = user.UserName;
            HttpContext.Current.Session["SessionIP"] = user.SessionIP;
            HttpContext.Current.Session["SessionUserAgent"] = user.SessionUserAgent;
            HttpContext.Current.Session["SessionID"] = user.SessionID;
            HttpContext.Current.Session["Email"] = user.Email;
            HttpContext.Current.Session["SessionCode"] = user.SessionCode;
            HttpContext.Current.Session["Phonenumber"] = user.Phonenumber;
            HttpContext.Current.Session["Roles"] = user.RoleName;

            // Additional data (if any)
            // HttpContext.Current.Session["SomeKey"] = user.SomeData;

            System.Diagnostics.Debug.WriteLine($"Session rehydrated for UserID: {user.Id}");
        }


    }
}
