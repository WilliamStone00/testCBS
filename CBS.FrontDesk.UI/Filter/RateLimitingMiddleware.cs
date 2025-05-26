using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Web;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using CBS.BusinessService.RequestLoggerServicesP;
using CBS.API.Helper;
using System.Net.Http.Headers;
using System.Runtime.Caching;
using MvcSiteMapProvider.Caching;
using System.Net;
using DocumentFormat.OpenXml.Spreadsheet;
using CBS.FrontDesk.Data.Entity.RequestManagement;
using System.Linq;
using static RateLimitConfigService;
using CBS.FrontDesk.Data.Entity;
using System.Web.Security;

namespace CBS.FrontDesk.UI.Filter
{


    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Runtime.Caching;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Security;
    using CBS.FrontDesk.UI.Utility.Middlware_logger;
    using Newtonsoft.Json;

    /// <summary>
    /// 📛 RateLimitingMiddleware is an HTTP module for ASP.NET MVC applications designed to:
    /// - Block abusive users based on rate limits.
    /// - Detect and respond to suspicious path access (security probing).
    /// - Log suspicious activity with metadata like IP, user, and device info.
    /// - Integrate with internal services for blocking, logging, and rate tracking.
    ///
    /// It protects the application from brute force, DDoS-like behavior, or probing attempts.
    /// </summary>


    public class RateLimitingMiddleware : IHttpModule
    {
        private readonly RateLimitConfigService _configService = new RateLimitConfigService();
        private readonly RateLimiteTrackerLoggerServices _loggerService = new RateLimiteTrackerLoggerServices();
        private readonly RateLimitedUserService _rateLimitedUserService = new RateLimitedUserService();
        private readonly SuspiciousPathService _suspiciousPathService = new SuspiciousPathService();
        private static readonly MemoryCache SuspiciousPathCache = MemoryCache.Default;
        private static readonly MemoryCache _ipCache = MemoryCache.Default;
        string globalUserFullname = "anonymous";
        string globalbranchname = "global";

        public void Init(HttpApplication context)
        {
            context.PostAuthenticateRequest += OnPostAuthenticateRequest;
        }

        private (string username, string branchid, string branchcode, string branchname, string tel, string fullname) GetUserNameFromTSCCookie(HttpRequest request)
        {
            try
            {
                var authCookie = request.Cookies["TSC"];
                if (authCookie == null)
                    return ("Unknown", "", "", "", "", "");

                var ticket = FormsAuthentication.Decrypt(authCookie.Value);
                if (ticket == null || ticket.Expired)
                    return ("Unknown", "", "", "", "", "");

                var userData = JsonConvert.DeserializeObject<CustomSerializeModel>(ticket.UserData);
                if (userData == null || string.IsNullOrWhiteSpace(userData.UserName))
                    return ("Unknown", "", "", "", "", "");

                string fullName = $"{userData.FullName}".Trim();
                globalUserFullname=fullName;
                globalbranchname=userData.BranchName;
                return (userData.UserName, userData.BranchId ?? "", userData.BranchCode ?? "", userData.BranchName ?? "", userData.Phonenumber ?? "", fullName);
            }
            catch (Exception ex)
            {
                string logMsg = $"❌ Error reading TSC cookie: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(logMsg);
                AdvancedMiddlewareLogger.Log(logMsg, LogLevel.ERROR, globalUserFullname, globalbranchname);
                return ("Unknown", "", "", "", "", "");
            }
        }

        private void HandleSuspiciousPath(HttpRequest request, HttpResponse response, string path)
        {
            var (username, branchid, branchcode, branchname, tel, fullname) = GetUserNameFromTSCCookie(request);
            var (ip, location, lat, lon, city, region, country) = GetIpAndLocationSync();
            string computerName = request.Headers["Computer-Name"] ?? "Unknown Computer";

            string reason = $"🚨 Suspicious path access detected: {path}";
            System.Diagnostics.Debug.WriteLine(reason);
            AdvancedMiddlewareLogger.Log(reason, LogLevel.WARN, globalUserFullname);

            Task.Run(() => _loggerService.LogRequest(ip, username, computerName, path, 403, location, lat, lon, city, region, country, branchid, branchcode, branchname, tel, fullname, true, reason));
            Task.Run(() => _rateLimitedUserService.BlockUser(ip, username, "Suspicious-Attack", TimeSpan.FromDays(360),
                reason, location, lat, lon, city, region, country, branchid, branchcode, branchname, tel, fullname));
        }

        private void OnPostAuthenticateRequest(object sender, EventArgs e)
        {
            var app = (HttpApplication)sender;
            var request = app.Context.Request;
            var response = app.Context.Response;
            string requestPath = request.Path.ToLower();

            if (requestPath.Contains("/error/blocked") || requestPath.Contains("/authentication/login"))
                return;

            if (IsSafeStaticAsset(requestPath) || IsAjaxRequest(request))
            {
                string logMsg = $"⏭️ Skipped static/ajax request: {requestPath}";
                System.Diagnostics.Debug.WriteLine(logMsg);
                AdvancedMiddlewareLogger.Log(logMsg, LogLevel.INFO, globalUserFullname, globalbranchname);
                return;
            }

            var (username, branchid, branchcode, branchname, tel, fullname) = GetUserNameFromTSCCookie(request);
            var (ip, location, lat, lon, city, region, country) = GetIpAndLocationSync();
            string computerName = request.Headers["Computer-Name"] ?? "Unknown Computer";
            DateTime now = DateTime.UtcNow;

            var checkRateLimitBlockQuery = new CheckRateLimitBlockQuery { IpOrUser = ip, Username = username };
            bool isBlockedResponse = Task.Run(() => _rateLimitedUserService.CheckIsBlocked(checkRateLimitBlockQuery)).GetAwaiter().GetResult();

            if (isBlockedResponse)
            {
                string logMsg = $"❌ Access denied for blocked IP/user: {ip} | {username}";
                System.Diagnostics.Debug.WriteLine(logMsg);
                AdvancedMiddlewareLogger.Log(logMsg, LogLevel.WARN, globalUserFullname, globalbranchname);
                HandleRateLimitExceeded(response, ip, "Contact your administrator", 0);
                return;
            }

            bool isSuspiciousPath = IsSuspiciousPathCached(requestPath);
            if (isSuspiciousPath)
            {
                string logMsg = $"🚨 Suspicious path matched and blocked: {requestPath}";
                System.Diagnostics.Debug.WriteLine(logMsg);
                AdvancedMiddlewareLogger.Log(logMsg, LogLevel.WARN, globalUserFullname, globalbranchname);
                HandleSuspiciousPath(request, response, requestPath);
                HandleRateLimitExceeded(response, ip, "Suspicious-Attack", 0);
                return;
            }

            var config = RateLimitConfigHolder.Config;
            int requestLimit = config.RequestLimit;
            TimeSpan timeWindow = TimeSpan.FromSeconds(config.TimeWindowSeconds);
            TimeSpan blockDuration = TimeSpan.FromMinutes(config.BlockDurationMinutes);

            bool isBlocked = false;
            string reasonOfBlocked = string.Empty;

            if (_rateLimitedUserService.ShouldBlock(ip, requestLimit, timeWindow))
            {
                isBlocked = true;
                reasonOfBlocked = "Exceeded user-based request limit";

                string logMsg = $"🚫 IP {ip} exceeded request limit. Blocked for {blockDuration.TotalMinutes} min";
                System.Diagnostics.Debug.WriteLine(logMsg);
                AdvancedMiddlewareLogger.Log(logMsg, LogLevel.WARN, globalUserFullname, globalbranchname);

                Task.Run(() => _rateLimitedUserService.BlockUser(
                    ip, username, "IP-Based", blockDuration, reasonOfBlocked,
                    location, lat, lon, city, region, country,
                    branchid, branchcode, branchname, tel, fullname));

                HandleRateLimitExceeded(response, ip, reasonOfBlocked, config.BlockDurationMinutes);
                return;
            }

            Task.Run(() => _loggerService.LogRequest(
                ip, username, computerName, requestPath, 200,
                location, lat, lon, city, region, country,
                branchid, branchcode, branchname, tel, fullname,
                isBlocked, reasonOfBlocked));
        }

        private bool IsSafeStaticAsset(string path)
        {
            var lower = path.ToLowerInvariant();

            string[] suspiciousIndicators = new[]
            {
            ".env", ".git", ".svn", ".hg", ".bak", ".old", ".backup",
            "web.config", "application.yml", "application.yaml", "settings.py",
            "config.php", "database.yml", "composer.json", "package.json",
            "requirements.txt", ".htaccess", ".htpasswd",
            "id_rsa", "id_dsa", "private.key", "access_token", "jwt", "secret",
            "phpmyadmin", "pma", "adminer", "dbadmin", "wp-admin", "wp-login",
            "cpanel", "login.jsp", "login.php", "dashboard.jsp", "dashboard.php",
            "passwd", "shadow", "boot.ini", "hosts", "system.ini",
            "windows/win.ini", "etc/passwd", "etc/shadow",
            "shell.php", "backdoor.php", "test.php", "eval.php", "mailer.php",
            "cmd.php", "rce.php", "upload.php", "exploit.php", "drupal", "magento",
            "owa", "ecp", "autodiscover", "activesync", "exchange", "server-status",
            ".php", ".jsp", ".asp", ".aspx", ".cgi", ".exe", ".sh", ".pl", ".py", ".rb", ".lua",
            ".log", ".sql", ".zip", ".tar.gz", ".7z", ".gz", ".tgz", ".rar",
            ".idea", ".vscode", ".dockerignore", "docker-compose", ".DS_Store",
            "crossdomain.xml", "clientaccesspolicy.xml", ".well-known", "favicon.ico.php"
        };

            bool isExcluded = ExcludedExtensions.Any(ext => lower.EndsWith(ext)) ||
                              ExcludedPaths.Any(p => lower.StartsWith(p));

            string logMsg = $"🔍 Checked static path: {path} | Result: {(isExcluded ? "Excluded" : "Included")}";
            System.Diagnostics.Debug.WriteLine(logMsg);
            AdvancedMiddlewareLogger.Log(logMsg, LogLevel.INFO, globalUserFullname, globalbranchname);

            return isExcluded;
        }
        private static readonly List<string> ExcludedPaths = new List<string>
        {
            "/favicon.ico", "/error/show/404", "/error/show/500", "/error/blocked",
            "/Content/", "/Scripts/", "/fonts/", "/Images/",
            "/session/getidletimeout", "/signalr/hubs","/bundles/jqueryval","/aspnet_client/system_web/4_0_30319/crystalreportviewers13/js/dhtmllib/empty.html"
        };

        private static readonly List<string> ExcludedExtensions = new List<string>
        {
            ".js", ".css", ".png", ".jpg", ".jpeg", ".gif", ".woff", ".woff2", ".ttf", ".ico", ".aspx", ".json", ".axd", ".html"
        };

        private static readonly List<string> ExcludedSubstrings = new List<string>
        {
            "dashboard", "login", "logout"
        };
        private (string IpAddress, string Location, string Latitude, string Longitude, string City, string Region, string Country) GetIpAndLocationSync()
        {
            try
            {
                var request = HttpContext.Current?.Request;
                string ip = request?.Headers["X-Forwarded-For"]?.Split(',')?.FirstOrDefault()?.Trim();
                if (string.IsNullOrWhiteSpace(ip))
                    ip = request?.UserHostAddress;

                if (string.IsNullOrWhiteSpace(ip) || ip.Equals("unknown", StringComparison.OrdinalIgnoreCase))
                    ip = Guid.NewGuid().ToString("N");

                string cacheKey = $"ClientResolvedIPAndLocation:{ip}";
                if (_ipCache.Contains(cacheKey))
                    return ((string, string, string, string, string, string, string))_ipCache.Get(cacheKey);

                using (var client = new WebClient())
                {
                    string json = client.DownloadString($"https://ipinfo.io/{ip}/json");
                    dynamic result = JsonConvert.DeserializeObject(json);

                    string city = result?.city ?? "";
                    string region = result?.region ?? "";
                    string country = result?.country ?? "";
                    string loc = result?.loc ?? "";

                    string latitude = "", longitude = "";
                    if (!string.IsNullOrWhiteSpace(loc) && loc.Contains(","))
                    {
                        var parts = loc.Split(',');
                        latitude = parts[0];
                        longitude = parts[1];
                    }

                    string location = $"{city}, {region}, {country}".Trim().Trim(',');

                    var resolvedData = (ip, location, latitude, longitude, city, region, country);
                    _ipCache.Set(cacheKey, resolvedData, DateTimeOffset.Now.AddHours(4));
                    return resolvedData;
                }
            }
            catch
            {
                string logMsg = "⚠️ Failed to resolve IP location for request. Fallback IP assigned.";
                System.Diagnostics.Debug.WriteLine(logMsg);
                AdvancedMiddlewareLogger.Log(logMsg, LogLevel.WARN, globalUserFullname, globalbranchname);
                string fallback = Guid.NewGuid().ToString("N");
                return (fallback, "Unknown", "", "", "", "", "");
            }
        }

        private bool IsSuspiciousPathCached(string path)
        {
            string key = $"SuspiciousPath:{path}";
            if (SuspiciousPathCache.Contains(key))
                return (bool)SuspiciousPathCache[key];

            bool result = Task.Run(() => _suspiciousPathService.CheckIfSuspiciousPath(path)).GetAwaiter().GetResult();
            SuspiciousPathCache.Add(key, result, DateTimeOffset.Now.AddMinutes(10));
            return result;
        }

        private bool IsAjaxRequest(HttpRequest request)
        {
            string requestedWith = request.Headers["X-Requested-With"];
            return !string.IsNullOrEmpty(requestedWith) && requestedWith.Equals("XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
        }

        private void HandleRateLimitExceeded(HttpResponse response, string ip, string reason, int unblockAfterMinutes)
        {
            var redirectUrl = $"/Error/Blocked?" +
                              $"ip={HttpUtility.UrlEncode(ip)}" +
                              $"reason={HttpUtility.UrlEncode(reason)}" +
                              $"blockedAt={HttpUtility.UrlEncode(DateTime.UtcNow.ToString("o"))}" +
                              $"&unblockAfter={unblockAfterMinutes}";

            response.Clear();
            response.StatusCode = 403;
            response.Redirect(redirectUrl, false);
            response.End();
        }

        public void Dispose() { }
    }


    //public class RateLimitingMiddleware : IHttpModule
    //{
    //    // 🔧 Services used to fetch config, log entries, and block users
    //    private readonly RateLimitConfigService _configService = new RateLimitConfigService();
    //    private readonly RateLimiteTrackerLoggerServices _loggerService = new RateLimiteTrackerLoggerServices();
    //    private readonly RateLimitedUserService _rateLimitedUserService = new RateLimitedUserService();
    //    private readonly SuspiciousPathService _suspiciousPathService = new SuspiciousPathService();
    //    private static readonly MemoryCache SuspiciousPathCache = MemoryCache.Default;
    //    // 🧠 Caches resolved IP info to reduce repeated geolocation lookups
    //    private static readonly MemoryCache _ipCache = MemoryCache.Default;

    //    /// <summary>
    //    /// Initializes the middleware and hooks into the request pipeline.
    //    /// </summary>
    //    public void Init(HttpApplication context)
    //    {
    //        context.PostAuthenticateRequest += OnPostAuthenticateRequest;
    //    }

    //    /// <summary>
    //    /// Extracts user information from the "TSC" auth cookie.
    //    /// Returns safe default values if invalid or missing.
    //    /// </summary>
    //    private (string username, string branchid, string branchcode, string branchname, string tel, string fullname) GetUserNameFromTSCCookie(HttpRequest request)
    //    {
    //        try
    //        {
    //            var authCookie = request.Cookies["TSC"];
    //            if (authCookie == null)
    //                return ("Unknown", "", "", "", "", "");

    //            var ticket = FormsAuthentication.Decrypt(authCookie.Value);
    //            if (ticket == null || ticket.Expired)
    //                return ("Unknown", "", "", "", "", "");

    //            var userData = JsonConvert.DeserializeObject<CustomSerializeModel>(ticket.UserData);
    //            if (userData == null || string.IsNullOrWhiteSpace(userData.UserName))
    //                return ("Unknown", "", "", "", "", "");

    //            string fullName = $"{userData.FullName}".Trim();
    //            return (userData.UserName, userData.BranchId ?? "", userData.BranchCode ?? "", userData.BranchName ?? "", userData.Phonenumber ?? "", fullName);
    //        }
    //        catch (Exception ex)
    //        {
    //            string logMsg = $"❌ Error reading TSC cookie: {ex.Message}";
    //            System.Diagnostics.Debug.WriteLine(logMsg);
    //            AdvancedMiddlewareLogger.Log(logMsg, LogLevel.ERROR);
    //            return ("Unknown", "", "", "", "", "");
    //        }

    //    }

    //    /// <summary>
    //    /// Logs access to suspicious paths and blocks the user/IP immediately.
    //    /// </summary>
    //    private void HandleSuspiciousPath(HttpRequest request, HttpResponse response, string path)
    //    {
    //        var (username, branchid, branchcode, branchname, tel, fullname) = GetUserNameFromTSCCookie(request);
    //        var (ip, location, lat, lon, city, region, country) = GetIpAndLocationSync();
    //        string computerName = request.Headers["Computer-Name"] ?? "Unknown Computer";

    //        string reason = $"🚨 Suspicious path access detected: {path}";

    //        // Log attempt
    //        Task.Run(() => _loggerService.LogRequest(ip, username, computerName, path, 403, location, lat, lon, city, region, country, branchid, branchcode, branchname, tel, fullname, true, reason));

    //        // Block user for 1 year (adjust as needed)
    //        Task.Run(() => _rateLimitedUserService.BlockUser(ip, username, "Suspicious-Attack", TimeSpan.FromDays(360),
    //            reason, location, lat, lon, city, region, country, branchid, branchcode, branchname, tel, fullname));
    //    }

    //    /// <summary>
    //    /// Main event handler called after authentication.
    //    /// Applies rate-limiting, logging, and blocking logic.
    //    /// </summary>
    //    private void OnPostAuthenticateRequest(object sender, EventArgs e)
    //    {
    //        var app = (HttpApplication)sender;
    //        var request = app.Context.Request;
    //        var response = app.Context.Response;
    //        string requestPath = request.Path.ToLower();

    //        // 🔄 1. Prevent blocking of core and auth pages
    //        if (requestPath.Contains("/error/blocked") || requestPath.Contains("/authentication/login"))
    //            return;

    //        // ✅ 2. Skip known safe paths or static content early
    //        if (IsSafeStaticAsset(requestPath) || IsAjaxRequest(request))
    //        {
    //            string logMsg = $"⏭️ Skipped static/ajax request: {requestPath}";
    //            System.Diagnostics.Debug.WriteLine(logMsg);
    //            AdvancedMiddlewareLogger.Log(logMsg, LogLevel.INFO);
    //            return;
    //        }

    //        // 📦 3. Collect user metadata
    //        var (username, branchid, branchcode, branchname, tel, fullname) = GetUserNameFromTSCCookie(request);
    //        var (ip, location, lat, lon, city, region, country) = GetIpAndLocationSync();
    //        string computerName = request.Headers["Computer-Name"] ?? "Unknown Computer";
    //        DateTime now = DateTime.UtcNow;

    //        // 🛑 4. Check if already blocked by DB or CIDR logic
    //        var checkRateLimitBlockQuery = new CheckRateLimitBlockQuery { IpOrUser = ip, Username = username };
    //        bool isBlockedResponse = Task.Run(() => _rateLimitedUserService.CheckIsBlocked(checkRateLimitBlockQuery))
    //                                     .GetAwaiter().GetResult();

    //        if (isBlockedResponse)
    //        {
    //            HandleRateLimitExceeded(response, ip, "Contact your administrator", 0);
    //            string logMsg = $"❌ Access denied for blocked IP/user: {ip} | {username}";
    //            System.Diagnostics.Debug.WriteLine(logMsg);
    //            AdvancedMiddlewareLogger.Log(logMsg, LogLevel.WARN);

    //            return;
    //        }

    //        // 🚨 5. Check if request path is flagged as suspicious (via cache + DB)
    //        bool isSuspiciousPath = IsSuspiciousPathCached(requestPath);
    //        if (isSuspiciousPath)
    //        {
    //            HandleSuspiciousPath(request, response, requestPath);
    //            HandleRateLimitExceeded(response, ip, "Suspicious-Attack", 0);
    //            string logMsg = $"🚨 Suspicious path matched and blocked: {requestPath}";
    //            System.Diagnostics.Debug.WriteLine(logMsg);
    //            AdvancedMiddlewareLogger.Log(logMsg, LogLevel.WARN);
    //            return;
    //        }

    //        // ⚙️ 6. Load rate limiting config
    //        var config = RateLimitConfigHolder.Config;
    //        int requestLimit = config.RequestLimit;
    //        TimeSpan timeWindow = TimeSpan.FromSeconds(config.TimeWindowSeconds);
    //        TimeSpan blockDuration = TimeSpan.FromMinutes(config.BlockDurationMinutes);

    //        // 🚫 7. Rate limit enforcement
    //        bool isBlocked = false;
    //        string reasonOfBlocked = string.Empty;

    //        if (_rateLimitedUserService.ShouldBlock(ip, requestLimit, timeWindow))
    //        {
    //            isBlocked = true;
    //            reasonOfBlocked = "Exceeded user-based request limit";

    //            Task.Run(() => _rateLimitedUserService.BlockUser(
    //                ip, username, "IP-Based", blockDuration, reasonOfBlocked,
    //                location, lat, lon, city, region, country,
    //                branchid, branchcode, branchname, tel, fullname));

    //            HandleRateLimitExceeded(response, ip, reasonOfBlocked, config.BlockDurationMinutes);
    //            string logMsg = $"🚫 IP {ip} exceeded request limit. Blocked for {blockDuration.TotalMinutes} min";
    //            System.Diagnostics.Debug.WriteLine(logMsg);
    //            AdvancedMiddlewareLogger.Log(logMsg, LogLevel.WARN);

    //            return;
    //        }

    //        // 📝 8. Log valid request
    //        Task.Run(() => _loggerService.LogRequest(
    //            ip, username, computerName, requestPath, 200,
    //            location, lat, lon, city, region, country,
    //            branchid, branchcode, branchname, tel, fullname,
    //            isBlocked, reasonOfBlocked));
    //    }
    //    private bool IsSuspiciousPathCached(string path)
    //    {
    //        string key = $"SuspiciousPath:{path}";
    //        if (SuspiciousPathCache.Contains(key))
    //            return (bool)SuspiciousPathCache[key];
    //        // ⚠️ Blocking call - consider async if service is async
    //        bool result = Task.Run(() => _suspiciousPathService.CheckIfSuspiciousPath(path)).GetAwaiter().GetResult();
    //        SuspiciousPathCache.Add(key, result, DateTimeOffset.Now.AddMinutes(10));
    //        return result;
    //    }
    //    private bool IsSafeStaticAsset(string path)
    //    {
    //        var lower = path.ToLowerInvariant();

    //        // 🛑 Extended blocklist for exploit bait and malicious probes
    //        string[] suspiciousIndicators = new[]
    //        {
    //            // Configuration / secrets
    //            ".env", ".git", ".svn", ".hg", ".bak", ".old", ".backup",
    //            "web.config", "application.yml", "application.yaml", "settings.py",
    //            "config.php", "database.yml", "composer.json", "package.json",
    //            "requirements.txt", ".htaccess", ".htpasswd",

    //            // SSH / keys / tokens
    //            "id_rsa", "id_dsa", "private.key", "access_token", "jwt", "secret",

    //            // Admin/probing tools
    //            "phpmyadmin", "pma", "adminer", "dbadmin", "wp-admin", "wp-login",
    //            "cpanel", "login.jsp", "login.php", "dashboard.jsp", "dashboard.php",

    //            // File access targets
    //            "passwd", "shadow", "boot.ini", "hosts", "system.ini",
    //            "windows/win.ini", "etc/passwd", "etc/shadow",

    //            // Known CVEs & probes
    //            "shell.php", "backdoor.php", "test.php", "eval.php", "mailer.php",
    //            "cmd.php", "rce.php", "upload.php", "exploit.php", "drupal", "magento",

    //            // Web service paths
    //            "owa", "ecp", "autodiscover", "activesync", "exchange", "server-status",

    //            // Code & script extensions
    //            ".php", ".jsp", ".asp", ".aspx", ".cgi", ".exe", ".sh", ".pl", ".py", ".rb", ".lua",

    //            // Backup & logs
    //            ".log", ".sql", ".zip", ".tar.gz", ".7z", ".gz", ".tgz", ".rar",

    //            // Misc
    //            ".idea", ".vscode", ".dockerignore", "docker-compose", ".DS_Store",
    //            "crossdomain.xml", "clientaccesspolicy.xml", ".well-known", "favicon.ico.php"
    //        };

    //        // ✅ Allow only known safe static assets
    //        return ExcludedExtensions.Any(ext => lower.EndsWith(ext)) ||
    //               ExcludedPaths.Any(p => lower.StartsWith(p));
    //    }



    //    /// <summary>
    //    /// Determines whether a request is an AJAX call.
    //    /// </summary>
    //    private bool IsAjaxRequest(HttpRequest request)
    //    {
    //        string requestedWith = request.Headers["X-Requested-With"];
    //        return !string.IsNullOrEmpty(requestedWith) && requestedWith.Equals("XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
    //    }

    //    /// <summary>
    //    /// Redirects to a blocked error page with parameters.
    //    /// </summary>
    //    private void HandleRateLimitExceeded(HttpResponse response, string ip, string reason, int unblockAfterMinutes)
    //    {
    //        var redirectUrl = $"/Error/Blocked?" +
    //                          $"ip={HttpUtility.UrlEncode(ip)}" +
    //                          $"reason={HttpUtility.UrlEncode(reason)}" +
    //                          $"blockedAt={HttpUtility.UrlEncode(DateTime.UtcNow.ToString("o"))}" +
    //                          $"&unblockAfter={unblockAfterMinutes}";

    //        response.Clear();
    //        response.StatusCode = 403;
    //        response.Redirect(redirectUrl, false);
    //        response.End(); // ⛔ Immediately ends pipeline
    //    }

    //    /// <summary>
    //    /// Resolves client IP and fetches geolocation using ipinfo.io (cached).
    //    /// </summary>
    //    private (string IpAddress, string Location, string Latitude, string Longitude, string City, string Region, string Country) GetIpAndLocationSync()
    //    {
    //        try
    //        {
    //            var request = HttpContext.Current?.Request;

    //            // Extract IP
    //            string ip = request?.Headers["X-Forwarded-For"]?.Split(',')?.FirstOrDefault()?.Trim();
    //            if (string.IsNullOrWhiteSpace(ip))
    //                ip = request?.UserHostAddress;

    //            if (string.IsNullOrWhiteSpace(ip) || ip.Equals("unknown", StringComparison.OrdinalIgnoreCase))
    //                ip = Guid.NewGuid().ToString("N"); // fallback IP

    //            string cacheKey = $"ClientResolvedIPAndLocation:{ip}";
    //            if (_ipCache.Contains(cacheKey))
    //                return ((string, string, string, string, string, string, string))_ipCache.Get(cacheKey);

    //            using (var client = new WebClient())
    //            {
    //                string json = client.DownloadString($"https://ipinfo.io/{ip}/json");
    //                dynamic result = JsonConvert.DeserializeObject(json);

    //                string city = result?.city ?? "";
    //                string region = result?.region ?? "";
    //                string country = result?.country ?? "";
    //                string loc = result?.loc ?? "";

    //                string latitude = "", longitude = "";
    //                if (!string.IsNullOrWhiteSpace(loc) && loc.Contains(","))
    //                {
    //                    var parts = loc.Split(',');
    //                    latitude = parts[0];
    //                    longitude = parts[1];
    //                }

    //                string location = $"{city}, {region}, {country}".Trim().Trim(',');

    //                var resolvedData = (ip, location, latitude, longitude, city, region, country);
    //                _ipCache.Set(cacheKey, resolvedData, DateTimeOffset.Now.AddHours(4)); // cache result
    //                return resolvedData;
    //            }
    //        }
    //        catch
    //        {
    //            string fallback = Guid.NewGuid().ToString("N");
    //            return (fallback, "Unknown", "", "", "", "", "");
    //        }
    //    }

    //    public void Dispose() { }

    //    // === ⛔ Path Exclusions (static resources, APIs, etc.) ===
    //    private static readonly List<string> ExcludedPaths = new List<string>
    //    {
    //        "/favicon.ico", "/error/show/404", "/error/show/500", "/error/blocked",
    //        "/Content/", "/Scripts/", "/fonts/", "/Images/",
    //        "/session/getidletimeout", "/signalr/hubs","/bundles/jqueryval","/aspnet_client/system_web/4_0_30319/crystalreportviewers13/js/dhtmllib/empty.html"
    //    };

    //    private static readonly List<string> ExcludedExtensions = new List<string>
    //    {
    //        ".js", ".css", ".png", ".jpg", ".jpeg", ".gif", ".woff", ".woff2", ".ttf", ".ico", ".aspx",".json",".axd",".html"
    //    };

    //    private static readonly List<string> ExcludedSubstrings = new List<string>
    //    {
    //        "dashboard", "login", "logout"
    //    };

    //    /// <summary>
    //    /// Returns true if the path is considered safe or static and should bypass middleware.
    //    /// </summary>
    //    private bool IsExcludedPath(string path)
    //    {
    //        path = path.ToLowerInvariant();
    //        return ExcludedPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)) ||
    //               ExcludedExtensions.Any(ext => path.EndsWith(ext, StringComparison.OrdinalIgnoreCase)) ||
    //               ExcludedSubstrings.Any(substr => path.Contains(substr));
    //    }

    //}
}






