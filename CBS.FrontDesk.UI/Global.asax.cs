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
           // UnityConfig.RegisterComponents();
            MvcHandler.DisableMvcResponseHeader = true;
            //GlobalFilters.Filters.Add(new UserAuditFilter()); // Register UserAuditFilter
            ValueProviderFactories.Factories.Add(new JsonValueProviderFactory());
            //ConnectionMonitoringService connectionService = new ConnectionMonitoringService();
            //var configService = new RateLimitConfigService(); // or resolve from container
            //RateLimitConfigHolder.LoadAsync(configService).GetAwaiter().GetResult(); // One-time safe call
            // Start IP Unblock Service
            //new UnblockIpService();
        }
        //protected void Application_BeginRequest()
        //{
        //    // ✅ Use the correct key and handle TestBed too
        //    var env = ConfigurationManager.AppSettings["URLConf_Environment"]?.Trim() ?? "Production";
        //    bool isProdOrTb = env.Equals("Production", StringComparison.OrdinalIgnoreCase)
        //                   || env.Equals("TestBed", StringComparison.OrdinalIgnoreCase);

        //    var ctx = HttpContext.Current;
        //    var req = ctx?.Request;
        //    var resp = ctx?.Response;

        //    if (req == null || resp == null) return;

        //    var host = req.Url?.Host?.ToLowerInvariant() ?? string.Empty;

        //    // ✅ Block localhost only in Production/TestBed, without Response.End()
        //    if (isProdOrTb && host.Contains("localhost"))
        //    {
        //        resp.StatusCode = 403;
        //        resp.StatusDescription = "Localhost access is blocked in Production/TestBed";
        //        resp.Write("Forbidden");
        //        resp.Flush();
        //        ctx.ApplicationInstance.CompleteRequest();
        //        return;
        //    }



        //    // ✅ Culture: Session may be null in BeginRequest; prefer cookie first, then session if available
        //    string lang = req.Cookies["TSC_Lang"]?.Value;

        //    if (string.IsNullOrWhiteSpace(lang) && ctx.Session != null)
        //        lang = ctx.Session["SelectedLanguage"] as string;

        //    if (string.IsNullOrWhiteSpace(lang))
        //        lang = "en";

        //    try
        //    {
        //        var ci = CultureInfo.GetCultureInfo(lang);
        //        Thread.CurrentThread.CurrentCulture = ci;
        //        Thread.CurrentThread.CurrentUICulture = ci;
        //    }
        //    catch
        //    {
        //        var ci = CultureInfo.GetCultureInfo("en");
        //        Thread.CurrentThread.CurrentCulture = ci;
        //        Thread.CurrentThread.CurrentUICulture = ci;
        //    }
        //}

        protected void Application_BeginRequest()
        {
            // ✅ Read environment and decide blocking
            var env = ConfigurationManager.AppSettings["URLConf_Environment"]?.Trim() ?? "Production";
            bool isProdOrTb = env.Equals("Production", StringComparison.OrdinalIgnoreCase)
                           || env.Equals("TestBed", StringComparison.OrdinalIgnoreCase);

            var ctx = HttpContext.Current;
            var req = ctx?.Request;
            var resp = ctx?.Response;

            if (req == null || resp == null) return;

            // Always set to avoid IIS custom 403 page taking over
            resp.TrySkipIisCustomErrors = true;

            var host = req.Url?.Host?.ToLowerInvariant() ?? string.Empty;

            // ✅ Block localhost only in Production/TestBed, render branded page
            //if (isProdOrTb && host.Contains("localhost"))
            //{
            //    // Correlation ID (re-use existing if present via header; else create one)
            //    string correlationId = req.Headers["X-Correlation-Id"] ?? Guid.NewGuid().ToString("N");
            //    // Client IP (best effort)
            //    string clientIp = req.ServerVariables["HTTP_X_FORWARDED_FOR"];
            //    if (string.IsNullOrWhiteSpace(clientIp))
            //        clientIp = req.UserHostAddress ?? "";

            //    // Prevent FormsAuth 302
            //    try { resp.SuppressFormsAuthenticationRedirect = true; } catch { }

            //    // Render branded page with reason
            //    RenderForbiddenPage(resp,
            //        correlationId,
            //        clientIp,
            //        reason: "Localhost access is blocked in Production/TestBed");

            //    // Finish request cleanly
            //    ctx.ApplicationInstance.CompleteRequest();
            //    return;
            //}

            // ✅ Culture selection (cookie -> session -> default)
            string lang = req.Cookies["TSC_Lang"]?.Value;
            if (string.IsNullOrWhiteSpace(lang) && ctx?.Session != null)
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

        // ===== 2) Branded 403 renderer (keeps your colors/branding) =====================

        private static void RenderForbiddenPage(HttpResponse response, string correlationId, string clientIp, string reason)
        {
            // ✅ Standard 403 setup + no cache + headers
            try
            {
                if (!string.IsNullOrWhiteSpace(correlationId) &&
                    response.Headers["X-Correlation-Id"] == null)
                {
                    response.AppendHeader("X-Correlation-Id", correlationId);
                }
            }
            catch { /* ignore if headers sent */ }

            response.Clear();
            response.StatusCode = 403;
            response.StatusDescription = "Forbidden";
            response.ContentType = "text/html";
            response.TrySkipIisCustomErrors = true;
            try { response.SuppressFormsAuthenticationRedirect = true; } catch { }

            response.Cache.SetCacheability(HttpCacheability.NoCache);
            response.Cache.SetNoStore();
            response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
            response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));

            // ✅ Safe encodes
            string corrId = System.Web.HttpUtility.HtmlEncode(correlationId ?? "");
            string ip = System.Web.HttpUtility.HtmlEncode(clientIp ?? "");
            string safeReason = System.Web.HttpUtility.HtmlEncode(reason ?? "");

            // 🎨 Brand colors preserved: #326183 (brand), #d9534f (danger)
            // Small UX: logo slot, “Copy Ref” button, keyboard focus on Home.
            response.Write($@"
<!DOCTYPE html>
<html lang='en'>
<head>
  <meta charset='UTF-8' />
  <meta name='viewport' content='width=device-width, initial-scale=1' />
  <title>TRUSTSOFTCREDIT — Access Denied</title>
  <link rel='icon' href='/favicon.ico' />
  <style>
    :root {{
      --brand:#326183;
      --danger:#d9534f;
      --bg:#f5f7fa;
      --text:#444;
      --muted:#666;
      --card:#fff;
      --border:#e6e8eb;
    }}
    * {{ box-sizing: border-box; }}
    body {{
      font-family: 'Segoe UI', Roboto, -apple-system, BlinkMacSystemFont, 'Helvetica Neue', Arial, sans-serif;
      background: var(--bg);
      margin: 0;
      color: var(--text);
    }}
    .top-bar {{
      background: var(--brand);
      padding: 16px;
      color: #fff;
      font-size: 20px;
      text-align: center;
      letter-spacing: 3px;
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 12px;
    }}
    .top-bar img.logo {{
      height: 28px;
      width: auto;
      vertical-align: middle;
    }}
    .container {{
      margin: 64px auto;
      max-width: 680px;
      padding: 32px;
      border: 2px solid var(--danger);
      border-radius: 10px;
      background: var(--card);
      text-align: center;
      box-shadow: 0 6px 24px rgba(0,0,0,0.04);
    }}
    h1 {{ color: var(--danger); margin: 0 0 8px 0; font-size: 28px; }}
    .subtitle {{ color: var(--muted); margin-bottom: 18px; }}
    .message {{ font-size: 16px; margin: 12px 0 18px; color: var(--text); }}
    .meta {{
      font-size: 12px; color: var(--muted); margin-top: 14px;
      display:flex; gap:10px; align-items:center; justify-content:center; flex-wrap:wrap;
    }}
    .meta code {{
      background:#fafafa; border:1px solid #eee; padding:4px 6px; border-radius:6px;
      font-family: ui-monospace, SFMono-Regular, Menlo, Consolas, 'Liberation Mono', monospace;
    }}
    .buttons {{ margin-top: 22px; display:flex; gap:12px; justify-content:center; flex-wrap:wrap; }}
    .btn {{
      background: var(--brand); color:#fff; padding: 10px 20px; font-size: 15px;
      border: none; border-radius: 8px; cursor: pointer; text-decoration:none; display:inline-block;
      transition: transform .04s ease-in-out;
    }}
    .btn:active {{ transform: translateY(1px); }}
    .btn.secondary {{ background:#6c757d; }}
    .reason {{
      text-align:left; white-space:pre-wrap; word-break:break-word;
      background:#fbfbfc; border:1px dashed var(--border); padding:10px; border-radius:8px; margin-top:16px; display:none;
    }}
    .toggle-reason {{ font-size: 12px; color: var(--brand); cursor:pointer; margin-top: 8px; text-decoration: underline; }}
    .footer {{
      background: var(--brand); color:#fff; font-size: 13px; text-align:center; padding: 12px;
      position: fixed; bottom: 0; width: 100%;
    }}
    .icon {{
      width:84px; height:84px; margin-bottom:16px; display:block; margin-left:auto; margin-right:auto;
      filter: drop-shadow(0 2px 6px rgba(0,0,0,0.08));
    }}
  </style>
</head>
<body>
  <div class='top-bar'>
    <!-- Optional brand mark -->
    <!-- <img class='logo' src='/content/branding/tsc-logo.svg' alt='TSC Logo' /> -->
    T R U S T S O F T C R E D I T — A C C E S S &nbsp; D E N I E D
  </div>

  <main class='container' role='main' aria-labelledby='title'>
    <img class='icon' src='https://cdn-icons-png.flaticon.com/512/1828/1828843.png' alt='Access Denied' />
    <h1 id='title'>ACCESS DENIED</h1>
    <div class='subtitle'>403 — Forbidden</div>
    <p class='message'>
      You are not authorized to access this page from this host.<br/>
      Please return to the home page or contact your administrator.
    </p>

    <div class='meta'>
      <span>Ref:</span> <code id='ref'>{corrId}</code>
      <button class='btn secondary' id='copyRef' aria-label='Copy reference'>Copy Ref</button>
      <span>IP:</span> <code>{ip}</code>
    </div>

    <div class='toggle-reason' id='toggleReason' aria-controls='reason' aria-expanded='false'>Show details for support</div>
    <pre id='reason' class='reason'>{safeReason}</pre>

    <div class='buttons'>
      <a href='/' class='btn' id='homeBtn'>Return to Home</a>
    </div>
  </main>

  <div class='footer'>
    Copyright © {DateTime.UtcNow:yyyy} by <b>F L U X SARL Cameroon</b>
  </div>

  <script>
    (function() {{
      var btn = document.getElementById('copyRef');
      var ref = document.getElementById('ref');
      if (btn && ref) {{
        btn.addEventListener('click', function() {{
          try {{
            var text = ref.textContent || ref.innerText || '';
            navigator.clipboard.writeText(text).then(function(){{
              btn.textContent = 'Copied';
              setTimeout(function(){{ btn.textContent = 'Copy Ref'; }}, 1200);
            }});
          }} catch(e) {{}}
        }});
      }}
      var t = document.getElementById('toggleReason');
      var pre = document.getElementById('reason');
      if (t && pre) {{
        t.addEventListener('click', function(){{
          var isOpen = pre.style.display === 'block';
          pre.style.display = isOpen ? 'none' : 'block';
          t.setAttribute('aria-expanded', String(!isOpen));
          t.textContent = isOpen ? 'Show details for support' : 'Hide details';
        }});
      }}
      var home = document.getElementById('homeBtn');
      if (home) home.focus();
    }})();
  </script>
</body>
</html>");

            response.Flush();
            // CompleteRequest is already called by the caller in our flow when needed.
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
