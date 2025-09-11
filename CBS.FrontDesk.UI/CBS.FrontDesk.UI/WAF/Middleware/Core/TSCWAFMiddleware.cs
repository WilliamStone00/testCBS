using CBS.BusinessService.RequestLoggerServicesP;
using CBS.FrontDesk.Data.Entity.RequestManagement;
using CBS.FrontDesk.UI.Filter;
using CBS.FrontDesk.UI.Utility;
using CBS.FrontDesk.UI.WAF.Middleware.Core.ResponsCapture;
using CBS.FrontDesk.UI.WAF.Services.Cookie;
using CBS.FrontDesk.UI.WAF.Services.DDoS;
using CBS.FrontDesk.UI.WAF.Services.GeoIP;
using CBS.FrontDesk.UI.WAF.Services.Validation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace CBS.FrontDesk.UI.WAF.Middleware.Core
{


    using System;
    using System.Linq;
    using System.Web;
    using CBS.FrontDesk.UI.WAF.Middleware.Helpers;
    using CBS.FrontDesk.UI.WAF.Utility;
    using Newtonsoft.Json;

    /// <summary>
    /// TRUSTSOFTCREDIT Web Application Firewall Middleware (TSCWAFMiddleware).
    /// This IHttpModule implementation intercepts incoming requests after authentication and:
    /// - Applies advanced threat detection via modular analyzers
    /// - Validates headers, paths, body content, IP reputation, and geolocation
    /// - Enforces correlation IDs for traceability
    /// - Renders custom access denial pages for blocked requests
    /// </summary>
    public class TSCWAFMiddleware : IHttpModule
    {
        private static bool _initialized = false;
        private static readonly object _lock = new object();

        private static WAFRequestAnalyzer _analyzer;
        private static GeoIpResolver _geoResolver;
        private static UserContextResolver _userResolver;
        private static readonly string EnvName =
    (System.Configuration.ConfigurationManager.AppSettings["URLConf_Environment"] ?? "Production").Trim();

        private static readonly bool IsDevEnvironment =
            string.Equals(EnvName, "Development", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Initializes the middleware and attaches WAF logic to request lifecycle events.
        /// </summary>
        public void Init(HttpApplication context)
        {
            System.Diagnostics.Trace.TraceInformation("TSCWAF Init attached.");
            // Hook into authentication-complete event for security processing
            context.PostAuthenticateRequest += OnPostAuthenticateRequest;

            // Hook into response execution to collect response body
            context.PostRequestHandlerExecute += (sender, args) =>
            {
                var app = (HttpApplication)sender;
                string responseBody = app.Context.Items["CapturedResponseBody"] as string;

                if (app.Context.Items["WAFContext"] is WAFContext wafContext)
                {
                    wafContext.ResponseBody = responseBody;
                }
            };
        }
        //private void OnBeginRequest(object sender, EventArgs e)
        //{
        //    var app = (HttpApplication)sender;
        //    var req = app.Context.Request;

        //    // normalize
        //    var path = req.Path.ToLowerInvariant();

        //    // skip static
        //    if (StaticPathHelper.IsStatic(path)) return;

        //    // quick auth endpoint skip (only for heavy checks; we still can rate-limit if you want)
        //    if (path == "/authentication/login" || path == "/authentication/logout") return;

        //    // build minimal context
        //    var config = RateLimitConfigHolder.Get();
        //    EnsureInitialized(config);

        //    // client IP
        //    var ip = GetClientIp(req);

        //    // quick rate-limit gate (no body, no geo)
        //    if (_analyzer.QuickRateLimit(ip, path, out var reason)) // implement a thin wrapper around your RateLimiterService
        //    {
        //        var wafCtx = new WAFContext { Ip = ip, Path = path, CorrelationId = Guid.NewGuid().ToString("N") };
        //        var resp = app.Context.Response;
        //        resp.AddHeader("X-Correlation-ID", wafCtx.CorrelationId);
        //        RenderBlockPage(resp, wafCtx, reason);
        //    }
        //}

        /// <summary>
        /// Entry point after authentication. Executes WAF analysis and blocks malicious requests.
        /// </summary>

        private void OnPostAuthenticateRequest(object sender, EventArgs e)
        {
            var app = (HttpApplication)sender;
            var request = app.Context.Request;
            var response = app.Context.Response;

            // ✅ Correlation ID (do NOT write to request headers)
            string correlationId = Guid.NewGuid().ToString("N");
            app.Context.Items["CorrelationId"] = correlationId;
            response.AddHeader("X-Correlation-ID", correlationId);

            // ✅ Compile-time and runtime DEV bypass (skip all WAF work)
            #if DEBUG
            System.Diagnostics.Debug.WriteLine("🟢 WAF bypassed: DEBUG build.");
            return;
            #endif
            if (IsDevEnvironment)
            {
                System.Diagnostics.Debug.WriteLine("🟢 WAF bypassed: Development environment (URLConf_Environment=Development).");
                return;
            }

            // ✅ Load dynamic WAF configuration
            var config = RateLimitConfigHolder.Get();
            if (config == null)
            {
                System.Diagnostics.Debug.WriteLine("❌ Failed to load WAF config.");
                return;
            }

            // ✅ Ensure internal services are initialized once
            EnsureInitialized(config);
            if (_analyzer == null)
                return;

            // ✅ Build WAF context from HTTP request
            var wafContext = WAFContext.BuildFromHttp(app.Context, _geoResolver.Resolve, _userResolver.Resolve, config);
            wafContext.CorrelationId = correlationId;
            wafContext.ActionMethod = request.HttpMethod;

            // ✅ Capture request body for further inspection
            string requestBody = RequestCaptureHelper.CaptureRequestBody(request);
            wafContext.RequestBody = requestBody;

            // ✅ If multipart, extract form fields and serialize
            if (request.ContentType?.StartsWith("multipart/form-data") == true)
            {
                var fields = request.Form.AllKeys.ToDictionary(k => k, k => request.Form[k]);
                wafContext.RequestBody = Newtonsoft.Json.JsonConvert.SerializeObject(fields, Newtonsoft.Json.Formatting.Indented);
            }

            // ✅ Optionally capture response if available
            if (app.Context.Items["__ResponseCaptureFilter"] is ResponseCaptureFilterStream filter)
            {
                wafContext.ResponseBody = filter.GetCapturedBody();
            }

            // ✅ Normalize path
            string requestPath = request.Path.ToLowerInvariant();

            // ✅ Skip static resources
            if (StaticPathHelper.IsStatic(requestPath))
            {
                System.Diagnostics.Debug.WriteLine($"🟢 Skipped static path: {requestPath}");
                return;
            }

            // ✅ Auth endpoint exclusions (exact matches)
            string[] authExclusions = {
        "/authentication/login",
        "/authentication/logout",
        "/authentication/mfa",
        "/authentication/tokenrefresh"
    };
            if (authExclusions.Any(ex => requestPath.Equals(ex, StringComparison.OrdinalIgnoreCase)))
            {
                System.Diagnostics.Debug.WriteLine($"🟢 Skipped auth endpoint: {requestPath}");
                return;
            }

            // ✅ Skip WAF for whitelisted developer/tester IPs
            if (WAFBypassHelper.ShouldBypass(wafContext.Ip))
            {
                System.Diagnostics.Debug.WriteLine($"🟢 WAF bypassed for Dev IP: {wafContext.Ip}");
                return;
            }

            // ✅ Perform WAF analysis; block if threats detected
            if (_analyzer.RequestAnalyzer(request, wafContext, out var reason))
            {
                if (!string.IsNullOrWhiteSpace(wafContext.RequestBody))
                    reason += $"\n📦 Request Body: {RequestCaptureHelper.Truncate(wafContext.RequestBody)}";

                RenderBlockPage(response, wafContext, reason);
            }
        }


        //private void OnPostAuthenticateRequest(object sender, EventArgs e)
        //{
        //    var app = (HttpApplication)sender;
        //    var request = app.Context.Request;
        //    var response = app.Context.Response;

        //    // ✅ Generate correlation ID for tracing
        //    string correlationId = Guid.NewGuid().ToString("N");
        //    request.Headers.Add("X-Correlation-ID", correlationId);
        //    app.Context.Items["CorrelationId"] = correlationId;

        //    // ✅ Load dynamic WAF configuration
        //    var config = RateLimitConfigHolder.Get();
        //    if (config == null)
        //    {
        //        System.Diagnostics.Debug.WriteLine("❌ Failed to load WAF config.");
        //        return;
        //    }

        //    // ✅ Ensure internal services are initialized once
        //    EnsureInitialized(config);
        //    if (_analyzer == null)
        //        return;

        //    // ✅ Build WAF context from HTTP request
        //    var wafContext = WAFContext.BuildFromHttp(app.Context, _geoResolver.Resolve, _userResolver.Resolve, config);
        //    wafContext.CorrelationId = correlationId;
        //    wafContext.ActionMethod = request.HttpMethod;

        //    // ✅ Capture request body for further inspection
        //    string requestBody = RequestCaptureHelper.CaptureRequestBody(request);
        //    wafContext.RequestBody = requestBody;

        //    // ✅ If multipart, extract form fields and serialize
        //    if (request.ContentType?.StartsWith("multipart/form-data") == true)
        //    {
        //        var fields = request.Form.AllKeys.ToDictionary(k => k, k => request.Form[k]);
        //        wafContext.RequestBody = JsonConvert.SerializeObject(fields, Formatting.Indented);
        //    }

        //    // ✅ Optionally capture response if available
        //    if (app.Context.Items["__ResponseCaptureFilter"] is ResponseCaptureFilterStream filter)
        //    {
        //        wafContext.ResponseBody = filter.GetCapturedBody();
        //    }

        //    // ✅ Skip WAF checks for login/auth endpoints
        //    // ✅ Normalize path
        //    string requestPath = request.Path.ToLowerInvariant();

        //    // ✅ Skip static resources
        //    if (StaticPathHelper.IsStatic(requestPath))
        //    {
        //        System.Diagnostics.Debug.WriteLine($"🟢 Skipped static path: {requestPath}");
        //        return;
        //    }

        //    // ✅ Auth endpoint exclusions
        //    string[] authExclusions = {
        //        "/authentication/login",
        //        "/authentication/logout",
        //        "/authentication/mfa",
        //        "/authentication/tokenrefresh"
        //    };

        //    if (authExclusions.Any(ex => requestPath.Equals(ex, StringComparison.OrdinalIgnoreCase)))
        //    {
        //        System.Diagnostics.Debug.WriteLine($"🟢 Skipped auth endpoint: {requestPath}");
        //        return;
        //    }


        //    // ✅ Skip WAF for whitelisted developer/tester IPs
        //    if (WAFBypassHelper.ShouldBypass(wafContext.Ip))
        //    {
        //        System.Diagnostics.Debug.WriteLine($"🟢 WAF bypassed for Dev IP: {wafContext.Ip}");
        //        return;
        //    }

        //    // ✅ Perform WAF analysis; block if threats detected
        //    if (_analyzer.RequestAnalyzer(request, wafContext, out var reason))
        //    {
        //        if (!string.IsNullOrWhiteSpace(wafContext.RequestBody))
        //            reason += $"\n📦 Request Body: {RequestCaptureHelper.Truncate(wafContext.RequestBody)}";

        //        RenderBlockPage(response, wafContext, reason);
        //    }
        //}

        /// <summary>
        /// Initializes internal services used by the WAF only once in a thread-safe way.
        /// </summary>
        private void EnsureInitialized(RateLimitConfig config)
        {
            if (_initialized) return;

            lock (_lock)
            {
                if (_initialized) return;

                var headerValidator = new HeaderValidator(config);
                var pathValidator = new PathValidator(new SuspiciousPathService(), config);
                var bodyScanner = new BodyScanner(new MaliciousContentScannerService());
                var rateLimiter = new RateLimiterService(config.RequestLimit, TimeSpan.FromSeconds(config.TimeWindowSeconds));
                var blockCache = new BlockCacheService();
                var suspiciousHandler = new SuspiciousEventHandler(
                    new RateLimiteTrackerLoggerServices(),
                    new RateLimitedUserService(),
                    blockCache
                );

                _geoResolver = new GeoIpResolver();
                _userResolver = new UserContextResolver();

                _analyzer = new WAFRequestAnalyzer(
                    headerValidator,
                    pathValidator,
                    bodyScanner,
                    rateLimiter,
                    blockCache,
                    _geoResolver,
                    suspiciousHandler,
                    config
                );

                _initialized = true;
            }
        }

        /// <summary>
        /// Renders a branded 403 Access Denied HTML page for blocked requests.
        /// Includes correlation ID, IP, action, and block reason for traceability.
        /// </summary>


        private void RenderBlockPage(HttpResponse response, WAFContext ctx, string reason)
        {
            // ✅ Add correlation header if missing (before any Write); swallow if already committed
            try
            {
                var corr = ctx?.CorrelationId;
                if (!string.IsNullOrWhiteSpace(corr) &&
                    response.Headers[CorrelationConstants.HeaderKey] == null)
                {
                    response.AppendHeader(CorrelationConstants.HeaderKey, corr);
                }
            }
            catch { /* headers may already be sent; ignore */ }

            // ✅ Reset & status
            response.Clear();
            response.StatusCode = 403;
            response.StatusDescription = "Forbidden";
            response.ContentType = "text/html";
            response.TrySkipIisCustomErrors = true;

            // ✅ Don’t let FormsAuth change 403 to 302 login redirect
            try { response.SuppressFormsAuthenticationRedirect = true; } catch { }

            // ✅ No cache
            response.Cache.SetCacheability(HttpCacheability.NoCache);
            response.Cache.SetNoStore();
            response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
            response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));

            // ✅ Safely show minimal diagnostics (encoded)
            string corrId = System.Web.HttpUtility.HtmlEncode(ctx?.CorrelationId ?? "");
            string clientIp = System.Web.HttpUtility.HtmlEncode(ctx?.Ip ?? "");
            string safeReason = System.Web.HttpUtility.HtmlEncode(reason ?? "");

            response.Write($@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8' />
    <title>TRUSTSOFTCREDIT - Access Denied</title>
    <style>
        body {{ font-family:'Segoe UI',sans-serif; background:#f5f7fa; margin:0; }}
        .top-bar {{ background:#326183; padding:15px; color:#fff; font-size:22px; text-align:center; letter-spacing:3px; }}
        .container {{ margin:60px auto; max-width:620px; padding:30px; border:2px solid #d9534f; border-radius:8px; background:#fff; text-align:center; }}
        h1 {{ color:#d9534f; }}
        .message {{ font-size:16px; margin:15px 0; color:#444; }}
        .meta {{ font-size:12px; color:#666; margin-top:14px; }}
        .button {{ background:#326183; color:#fff; padding:10px 25px; font-size:15px; border:none; border-radius:5px; cursor:pointer; text-decoration:none; margin-top:20px; display:inline-block; }}
        .footer {{ background:#326183; color:#fff; font-size:13px; text-align:center; padding:12px; position:fixed; bottom:0; width:100%; }}
        .access-icon {{ width:80px; height:80px; margin-bottom:20px; }}
        pre.reason {{ text-align:left; white-space:pre-wrap; word-break:break-word; background:#fafafa; border:1px solid #eee; padding:10px; border-radius:6px; display:none; }}
    </style>
</head>
<body>
    <div class='top-bar'>T R U S T S O F T C R E D I T - ACCESS DENIED</div>
    <div class='container'>
        <img class='access-icon' src='https://cdn-icons-png.flaticon.com/512/1828/1828843.png' alt='Access Denied Icon' />
        <h1>ACCESS DENIED</h1>
        <div class='message'>
            <strong>Status Code:</strong> 403 - Forbidden<br />
            You are not authorized to access this page.<br />
            Please return to the home page or contact your administrator.
        </div>
        <div class='meta'>
            Ref: <strong>{corrId}</strong> &nbsp;•&nbsp; IP: <strong>{clientIp}</strong>
        </div>
        <!-- Reason (hidden by default; useful during support) -->
        <pre class='reason'>{safeReason}</pre>
        <a href='/' class='button'>Return to Home</a>
    </div>
    <div class='footer'>
        Copyright © 2024 by <b>F L U X SARL Cameroon</b>
    </div>
</body>
</html>");

            // ✅ Finish without ThreadAbortException
            response.Flush();
            HttpContext.Current?.ApplicationInstance?.CompleteRequest();
        }


        //    private void RenderBlockPage(HttpResponse response, WAFContext ctx, string reason)
        //    {

        //        response.Clear();
        //        response.StatusCode = 403;
        //        response.ContentType = "text/html";
        //        response.TrySkipIisCustomErrors = true;

        //        response.Write($@"
        //<!DOCTYPE html>
        //<html lang='en'>
        //<head>
        //    <meta charset='UTF-8' />
        //    <title>TRUSTSOFTCREDIT - Access Denied</title>
        //    <style>
        //        body {{
        //            font-family: 'Segoe UI', sans-serif;
        //            background-color: #f5f7fa;
        //            margin: 0;
        //        }}
        //        .top-bar {{
        //            background-color: #326183;
        //            padding: 15px;
        //            color: white;
        //            font-size: 22px;
        //            text-align: center;
        //            letter-spacing: 3px;
        //        }}
        //        .container {{
        //            margin: 60px auto;
        //            max-width: 620px;
        //            padding: 30px;
        //            border: 2px solid #d9534f;
        //            border-radius: 8px;
        //            background-color: #fff;
        //            text-align: center;
        //        }}
        //        h1 {{
        //            color: #d9534f;
        //        }}
        //        .message {{
        //            font-size: 16px;
        //            margin: 15px 0;
        //            color: #444;
        //        }}
        //        .button {{
        //            background-color: #326183;
        //            color: white;
        //            padding: 10px 25px;
        //            font-size: 15px;
        //            border: none;
        //            border-radius: 5px;
        //            cursor: pointer;
        //            text-decoration: none;
        //            margin-top: 20px;
        //        }}
        //        .footer {{
        //            background-color: #326183;
        //            color: white;
        //            font-size: 13px;
        //            text-align: center;
        //            padding: 12px;
        //            position: fixed;
        //            bottom: 0;
        //            width: 100%;
        //        }}
        //        .footer b {{
        //            color: #e6e6e6;
        //        }}
        //        .access-icon {{
        //            width: 80px;
        //            height: 80px;
        //            margin-bottom: 20px;
        //        }}
        //    </style>
        //</head>
        //<body>
        //    <div class='top-bar'>T R U S T S O F T C R E D I T - ACCESS DENIED</div>
        //    <div class='container'>
        //        <img class='access-icon' src='https://cdn-icons-png.flaticon.com/512/1828/1828843.png' alt='Access Denied Icon' />
        //        <h1>ACCESS DENIED</h1>
        //        <div class='message'>
        //            <strong>Status Code:</strong> 403 - Forbidden<br />
        //            You are not authorized to access this page.<br />
        //            Please return to the home page or contact your administrator.
        //        </div>
        //        <a href='/' class='button'>Return to Home</a>
        //    </div>
        //    <div class='footer'>
        //        Copyright © 2024 by <b>F L U X SARL Cameroon</b>
        //    </div>
        //</body>
        //</html>");

        //        response.End();
        //    }

        /// <summary>
        /// Disposes resources if needed (currently unused).
        /// </summary>
        public void Dispose() { }
    }


}