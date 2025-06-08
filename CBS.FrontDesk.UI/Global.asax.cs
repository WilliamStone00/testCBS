using CBS.API.Helper;
using CBS.BusinessService;
using CBS.BusinessService.Accounting;
using CBS.BusinessService.Config;
using CBS.BusinessService.UserManagement;
using CBS.FrontDesk.Data.Entity;
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
            var isProduction = ConfigurationManager.AppSettings["Environment"] == "Production";
            var host = HttpContext.Current.Request.Url.Host.ToLowerInvariant();

            if (isProduction && host.Contains("localhost"))
            {
                HttpContext.Current.Response.StatusCode = 403;
                HttpContext.Current.Response.StatusDescription = "Localhost access is blocked in production/TB";
                HttpContext.Current.Response.End();
            }
            if (!Context.Request.IsSecureConnection)
            {
                Response.Redirect(Context.Request.Url.ToString().Replace("http:", "https:"));
            }
            string lang = null;

            if (HttpContext.Current.Session != null)
            {
                lang = HttpContext.Current.Session["SelectedLanguage"]?.ToString();
            }

            if (string.IsNullOrEmpty(lang))
            {
                lang = HttpContext.Current.Request.Cookies["TSC_Lang"]?.Value ?? "en";
            }

            var ci = new CultureInfo(lang);
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
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

        //protected void Application_EndRequest()
        //{
        //    var response = HttpContext.Current.Response;
        //    var request = HttpContext.Current.Request;

        //    // Handle 401 Unauthorized
        //    if (response.StatusCode == 401)
        //    {
        //        response.Clear();
        //        if (!request.Url.AbsolutePath.EndsWith("/Authentication/Logout", StringComparison.OrdinalIgnoreCase))
        //        {
        //            response.Redirect("~/Authentication/Logout");
        //        }
        //    }
        //    // Log 500 Internal Server Errors
        //    else if (response.StatusCode == 500)
        //    {
        //        var exception = Server.GetLastError(); // Get the last thrown exception
        //        if (exception != null)
        //        {
        //            string errorDetails = $"500 Error at {request.Url}\n" +
        //                                $"Exception: {exception.Message}\n" +
        //                                $"Stack Trace: {exception.StackTrace}\n" +
        //                                $"Inner Exception: {exception.InnerException?.Message}";


        //            // Optionally: Log to a file (ensure permissions)
        //            // File.AppendAllText(Server.MapPath("~/App_Data/ErrorLog.txt"), $"{DateTime.Now}: {errorDetails}\n\n");
        //        }
        //    }
        //    // Log other 4xx/5xx errors (optional)
        //    else if (response.StatusCode >= 400)
        //    {
        //        System.Diagnostics.Trace.TraceWarning($"HTTP {response.StatusCode} at {request.Url}");
        //    }
        //}

        protected void Application_PreSendRequestHeaders()
        {
            // 🔐 1️⃣ Remove default server-identifying headers
            Response.Headers.Remove("Server");
            Response.Headers.Remove("X-AspNet-Version");
            Response.Headers.Remove("X-Powered-By");

            // 🛡️ 2️⃣ Apply custom branding (optional but obfuscates real tech stack)
            Response.Headers.Add("Server", "SERVER FLUX TSC");
            Response.Headers.Add("X-Powered-By", "FLUXSAL CAMEROON");

            // 🔒 3️⃣ Enforce strict HTTPS with HSTS for 1 year and preload
            Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains; preload";

            // 🌐 4️⃣ Dynamically set allowed connect-src domains for CSP
            var connectSrcValues = string.Join(" ", AllowedOriginDomains.Prepend("'self'"));

            // 📜 5️⃣ Add Content-Security-Policy (CSP) for XSS protection
            string contentSecurityPolicy = string.Join(" ",
                "default-src 'self';",
                "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdnjs.cloudflare.com https://unpkg.com;",
                "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdnjs.cloudflare.com;",
                "font-src 'self' https://fonts.gstatic.com https://cdnjs.cloudflare.com;",
                $"connect-src {connectSrcValues};",
                "img-src 'self' data:;",
                "frame-src 'none';",
                "object-src 'none';",
                "base-uri 'self';",
                "form-action 'self';"
            );

            if (!Response.Headers.AllKeys.Contains("Content-Security-Policy"))
                Response.Headers.Add("Content-Security-Policy", contentSecurityPolicy);

            // 🔎 6️⃣ Privacy headers
            if (!Response.Headers.AllKeys.Contains("Referrer-Policy"))
                Response.Headers.Add("Referrer-Policy", "no-referrer");

            // 🎛️ 7️⃣ Permissions control
            if (!Response.Headers.AllKeys.Contains("Permissions-Policy"))
                Response.Headers.Add("Permissions-Policy", "geolocation=(), microphone=(), camera=(), payment=()");

            // 🛡️ 8️⃣ Anti-clickjacking
            if (!Response.Headers.AllKeys.Contains("X-Frame-Options"))
                Response.Headers.Add("X-Frame-Options", "DENY");

            // 🧪 9️⃣ Prevent MIME-type sniffing
            if (!Response.Headers.AllKeys.Contains("X-Content-Type-Options"))
                Response.Headers.Add("X-Content-Type-Options", "nosniff");

            // 🧰 🔒 10️⃣ Legacy XSS filter (for older browsers)
            if (!Response.Headers.AllKeys.Contains("X-XSS-Protection"))
                Response.Headers.Add("X-XSS-Protection", "1; mode=block");

            // 🍪 11️⃣ Secure all response cookies
            foreach (var cookieKey in Response.Cookies.AllKeys)
            {
                var cookie = Response.Cookies[cookieKey];
                if (cookie == null) continue;

                cookie.Secure = true; // Only send via HTTPS
                cookie.HttpOnly = true; // JS cannot access
                cookie.SameSite = SameSiteMode.Strict; // Prevent cross-origin usage
            }
        }
        //protected void Application_PreSendRequestHeaders()
        //{
        //    // 🔄 Remove existing headers not removed by <remove> in web.config
        //    Response.Headers.Remove("Server");
        //    Response.Headers.Remove("X-AspNet-Version");
        //    Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
        //    // 🛡️ Add branding (not security-sensitive)
        //    Response.Headers.Add("Server", "SERVER FLUX TSC");
        //    Response.Headers.Add("X-Powered-By", "FLUXSAL CAMEROON"); // Only if you're OK showing brand

        //    // ✅ These are already defined in web.config, so DO NOT add them again:
        //    // • Strict-Transport-Security
        //    // • X-Frame-Options
        //    // • X-Content-Type-Options
        //    // • X-XSS-Protection

        //    // 🔒 Additional secure headers (not present in web.config)
        //    if (!Response.Headers.AllKeys.Contains("Referrer-Policy"))
        //        Response.Headers.Add("Referrer-Policy", "no-referrer");

        //    if (!Response.Headers.AllKeys.Contains("Permissions-Policy"))
        //        Response.Headers.Add("Permissions-Policy", "geolocation=(), microphone=(), camera=(), payment=()");

        //    // 🍪 Secure cookies with best practices
        //    foreach (var cookieKey in Response.Cookies.AllKeys)
        //    {
        //        var cookie = Response.Cookies[cookieKey];
        //        if (cookie == null) continue;

        //        cookie.Secure = true; // HTTPS only
        //        cookie.HttpOnly = true; // No access from JS
        //        cookie.SameSite = SameSiteMode.Strict; // CSRF protection
        //    }
        //}

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
            Exception ex = Server.GetLastError();
            if (ex is HttpException httpEx && httpEx.GetHttpCode() == 429)
            {
                Response.Clear();
                Response.StatusCode = 429;
                Response.Write("Too Many Requests. Please try again later.");
                Response.End();
            }
        }
        //protected void Application_Error(object sender, EventArgs e)
        //{
        //    var exception = Server.GetLastError();
        //    Response.Clear();
        //    Server.ClearError();

        //    var httpException = exception as HttpException;
        //    int statusCode = httpException != null ? httpException.GetHttpCode() : 500;

        //    // Handle Missing Resources Separately
        //    if (statusCode == 404 && HttpContext.Current.Request.Url.ToString().Contains("/Scripts/"))
        //    {
        //        Response.StatusCode = 404;
        //        Response.End();
        //        return;
        //    }

        //    var routeData = new System.Web.Routing.RouteData();
        //    routeData.Values["controller"] = "Error";
        //    routeData.Values["action"] = "Index";
        //    routeData.Values["statusCode"] = statusCode;
        //    routeData.Values["message"] = exception.Message;
        //    routeData.Values["stackTrace"] = exception.StackTrace;

        //    // Set the response status code
        //    Response.StatusCode = statusCode;

        //    // Pass error details to the Error View
        //    IController errorController = new ErrorController();
        //    var rc = new RequestContext(new HttpContextWrapper(Context), routeData);
        //    errorController.Execute(rc);
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


        protected void Application_AcquireRequestState(object sender, EventArgs e)
        {
            //var identity = HttpContext.Current?.User as CustomPrincipal;
            //if (HttpContext.Current?.Session != null && identity != null && identity.Identity != null && identity.Identity.IsAuthenticated)
            //{
            //    // 1️⃣ Get Session values
            //    string sessionIP = HttpContext.Current.Session["SessionIP"] as string;
            //    string sessionUserAgent = HttpContext.Current.Session["SessionUserAgent"] as string;

            //    // 2️⃣ Get Current Connection values (from CustomPrincipal)
            //    string currentIP = identity.SessionIP;
            //    string currentUserAgent = identity.SessionUserAgent;

            //    // 3️⃣ Compare IP and User Agent
            //    if (!string.Equals(sessionIP, currentIP) || !string.Equals(sessionUserAgent, currentUserAgent))
            //    {
            //        // ⚡ Session Hijack Detected: Kill Session
            //        HttpContext.Current.Session.Clear();
            //        HttpContext.Current.Session.Abandon();
            //        FormsAuthentication.SignOut();
            //        //HttpContext.Current.Response.Redirect("~/Authentication/Login?reason=sessionhijack");
            //    }

            //    // 4️⃣ Absolute Timeout check
            //    DateTime? sessionStartTime = HttpContext.Current.Session["SessionStartTime"] as DateTime?;
            //    int? maxLifetimeMinutes = HttpContext.Current.Session["SessionMaxLifetimeMinutes"] as int?;

            //    if (sessionStartTime.HasValue && maxLifetimeMinutes.HasValue)
            //    {
            //        var now = DateTime.UtcNow;
            //        var elapsedMinutes = (now - sessionStartTime.Value).TotalMinutes;

            //        if (elapsedMinutes > maxLifetimeMinutes.Value)
            //        {
            //            // ⛔ Absolute Session Expired
            //            HttpContext.Current.Session.Clear();
            //            HttpContext.Current.Session.Abandon();
            //            FormsAuthentication.SignOut();
            //            //HttpContext.Current.Response.Redirect("~/Authentication/Logout?reason=sessionexpired");
            //            //return;
            //        }
            //    }
            //}
        }
        public class CorrelationIdActionFilter : ActionFilterAttribute
        {
            public override void OnActionExecuting(ActionExecutingContext filterContext)
            {
                var httpContext = filterContext.HttpContext;
                var correlationId = httpContext.Items["CorrelationId"]?.ToString() ??
                                    httpContext.Request.Headers[CorrelationConstants.HeaderKey] ??
                                    Guid.NewGuid().ToString();

                httpContext.Items["CorrelationId"] = correlationId;
                httpContext.Response.Headers[CorrelationConstants.HeaderKey] = correlationId;
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


        //protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
        //{
        //    HttpCookie authCookie = Request.Cookies["TSC"];

        //    if (authCookie != null)
        //    {
        //        FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);

        //        if (!authTicket.Expired)
        //        {
        //            var user = JsonConvert.DeserializeObject<CustomSerializeModel>(authTicket.UserData);

        //            CustomPrincipal principal = new CustomPrincipal(authTicket.Name)
        //            {
        //                UserId = user.Id,
        //                FullName = user.FullName,
        //                UserName = user.UserName,
        //                Roles = user.RoleName,
        //                SessionIP = user.SessionIP,
        //                SessionUserAgent = user.SessionUserAgent,
        //                SessionID = user.SessionID,
        //                Email = user.Email,
        //                SessionCode = user.SessionCode,
        //                Phonenumber = user.Phonenumber,
        //                IsAuthenticated = true
        //            };

        //            HttpContext.Current.User = principal;
        //            System.Threading.Thread.CurrentPrincipal = principal;

        //            // ✅ Rehydrate session if missing
        //            if (HttpContext.Current == null || HttpContext.Current.Session == null)
        //            {
        //                System.Diagnostics.Debug.WriteLine("HttpContext or Session is null. Skipping session rehydration.");
        //                return;
        //            }

        //            if (HttpContext.Current.Session["UserID"] == null)
        //            {
        //                RehydrateSession(user);
        //            }

        //        }
        //    }
        //}


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
