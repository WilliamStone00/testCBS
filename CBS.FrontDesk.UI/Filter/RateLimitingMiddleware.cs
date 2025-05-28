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
using CBS.FrontDesk.Data.Entity;
using System.Web.Security;

namespace CBS.FrontDesk.UI.Filter
{


    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.Caching;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Security;
    using CBS.FrontDesk.Service;
    using CBS.FrontDesk.UI.Utility;
    using CBS.FrontDesk.UI.Utility.Middlware_logger;
    using Newtonsoft.Json;
    using static RateLimitConfigService;

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
        private static readonly MemoryCache _blockCache = MemoryCache.Default;
        private readonly MaliciousContentScannerService _scanner = new MaliciousContentScannerService();
        private static readonly List<string> AllowedOriginDomains = ConfigurationManager.AppSettings["AllowedOrigins"]
        ?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
        .Select(o => o.Trim().ToLowerInvariant())
        .ToList() ?? new List<string>();

        string globalUserFullname = "anonymous";
        string globalbranchname = "global";

        public void Init(HttpApplication context)
        {
            context.PostAuthenticateRequest += OnPostAuthenticateRequest;
        }

        private (string username, string branchid, string branchcode, string branchname, string tel, string fullname, bool isAuthenticated) GetUserNameFromTSCCookie(HttpRequest request)
        {
            try
            {
                var authCookie = request.Cookies["TSC"];
                if (authCookie == null)
                    return ("Unknown", "", "", "", "", "", false);

                var ticket = FormsAuthentication.Decrypt(authCookie.Value);
                if (ticket == null || ticket.Expired)
                    return ("Unknown", "", "", "", "", "", false);

                var userData = JsonConvert.DeserializeObject<CustomSerializeModel>(ticket.UserData);
                if (userData == null || string.IsNullOrWhiteSpace(userData.UserName))
                    return ("Unknown", "", "", "", "", "", false);

                string fullName = $"{userData.FullName}".Trim();
                globalUserFullname=fullName;
                globalbranchname=userData.BranchName;
                return (userData.UserName, userData.BranchId ?? "", userData.BranchCode ?? "", userData.BranchName ?? "", userData.Phonenumber ?? "", fullName, true);
            }
            catch (Exception ex)
            {
                string logMsg = $"❌ Error reading TSC cookie: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(logMsg);
                AdvancedMiddlewareLogger.Log(logMsg, LogLevel.ERROR, globalUserFullname, globalbranchname);
                return ("Unknown", "", "", "", "", "", false);
            }
        }

        private void HandleSuspiciousPath(HttpRequest request, HttpResponse response, string path, string blockedreason, string blackListingType, string fullUrl)
        {
            var (username, branchid, branchcode, branchname, tel, fullname, isAuthenticated) = GetUserNameFromTSCCookie(request);
            var (ip, location, lat, lon, city, region, country) = GetIpAndLocationSync();
            string computerName = request.Headers["Computer-Name"] ?? "Unknown Computer";

            string reason = $"🚨 {blockedreason}: {path}";
            System.Diagnostics.Debug.WriteLine(reason);
            AdvancedMiddlewareLogger.Log(reason, LogLevel.WARN, globalUserFullname);

            Task.Run(() => _loggerService.LogRequest(ip, username, computerName, path, 403, location, lat, lon, city, region, country, branchid, branchcode, branchname, tel, fullname, true, reason, isAuthenticated, fullUrl));
            Task.Run(() => _rateLimitedUserService.BlockUser(ip, username, blackListingType, TimeSpan.FromDays(360),
                reason, location, lat, lon, city, region, country, branchid, branchcode, branchname, tel, fullname));
            CacheTemporaryBlock(ip);
        }

        private void OnPostAuthenticateRequest(object sender, EventArgs e)
        {
            var app = (HttpApplication)sender;
            var request = app.Context.Request;
            var response = app.Context.Response;
            string requestPath = request.Path.ToLower();

            // 🔁 1. Prevent redirect loops for known safe pages
            if (requestPath.Contains("/error/blocked") || requestPath.Contains("/authentication/login"))
                return;

            // 🚫 2. Skip static file or AJAX request (non-relevant traffic)
            if (IsSafeStaticAsset(requestPath) || IsAjaxRequest(request))
            {
                string logMsg = $"⏭️ Skipped static/ajax request: {requestPath}";
                System.Diagnostics.Debug.WriteLine(logMsg);
                AdvancedMiddlewareLogger.Log(logMsg, LogLevel.INFO, globalUserFullname, globalbranchname);
                return;
            }

            // 📦 3. Gather request + user context info
            var (username, branchid, branchcode, branchname, tel, fullname, isAuthenticated) = GetUserNameFromTSCCookie(request);
            var (ip, location, lat, lon, city, region, country) = GetIpAndLocationSync();
            string computerName = request.Headers["Computer-Name"] ?? "Unknown Computer";
            DateTime now = DateTime.UtcNow;
            string fullUrl = request.Url?.GetLeftPart(UriPartial.Authority) + request.RawUrl;
            bool isLocalDevIp =ip == "::1";
            // ⚠️ 4. Immediately block malformed or non-IPv4 requests
            if (!isLocalDevIp)
            {
                if (!IsValidIPv4(ip))
                {
                    string logMsg = $"🚫 Invalid or malformed IP address. Invalid IP format: {ip} | Path: {requestPath}";
                    System.Diagnostics.Debug.WriteLine(logMsg);
                    AdvancedMiddlewareLogger.Log(logMsg, LogLevel.WARN, globalUserFullname, globalbranchname);
                    HandleSuspiciousPath(request, response, requestPath, logMsg, "GeoIP-CIDR-Filter", fullUrl);
                    HandleRateLimitExceeded(response, ip, "Invalid IP Format", 0);
                    return;
                }
            }
            // ⏱️ 5. Check short-term in-memory block cache
            if (IsTemporarilyBlocked(ip))
            {
                HandleRateLimitExceeded(response, ip, "Blocked (cache)", 0);
                return;
            }

            // 🧱 6. Check database block (serious or persistent attackers)
            var checkRateLimitBlockQuery = new CheckRateLimitBlockQuery { IpOrUser = ip, Username = username };
            bool isBlockedResponse = Task.Run(() => _rateLimitedUserService.CheckIsBlocked(checkRateLimitBlockQuery))
                                         .GetAwaiter().GetResult();
            if (isBlockedResponse)
            {
                string logMsg = $"❌ Access denied for blocked IP/user: {ip} | {username} | URL: {fullUrl}";
                System.Diagnostics.Debug.WriteLine(logMsg);
                AdvancedMiddlewareLogger.Log(logMsg, LogLevel.WARN, fullname, branchname);
                HandleRateLimitExceeded(response, ip, "Contact your administrator", 0);
                return;
            }

            // 🌍 7. Validate that IP belongs to allowed Cameroon CIDR
            if (!isLocalDevIp)
            {
                if (!CidrUtility.IsCameroonIp(ip, CameroonCidrs))
                {
                    string logMsg = $"🚨 Access from non-Cameroon IP. IP not in Cameroon CIDR: {ip} | Country: {country} | Path: {requestPath}";
                    System.Diagnostics.Debug.WriteLine(logMsg);
                    AdvancedMiddlewareLogger.Log(logMsg, LogLevel.WARN, globalUserFullname, globalbranchname);
                    HandleSuspiciousPath(request, response, requestPath, logMsg, "GeoIP-CIDR-Filter", fullUrl);
                    HandleRateLimitExceeded(response, ip, "Non-Cameroon CIDR IP", 0);
                    return;
                }
            }
            

            // 🧪 8. Scan for malformed headers (user-agents, referers, etc.)
            if (IsSuspiciousHeaders(request, response, requestPath, fullUrl, ip))
                return; // blocked internally

            // 📎 9. Check if it's a multipart file upload with malicious content
            if (request.HttpMethod == "POST" && request.ContentType?.StartsWith("multipart/form-data", StringComparison.OrdinalIgnoreCase) == true)
            {
                if (ContainsMaliciousMultipartContent(request, response, requestPath, fullUrl, ip))
                    return;
            }

            // 👤 10. Log if unauthenticated but reaching protected paths
            if (!isAuthenticated)
            {
                string logMsg = $"🔐 Unauthenticated access redirected: {requestPath} | Full URL: {fullUrl}";
                System.Diagnostics.Debug.WriteLine(logMsg);
                AdvancedMiddlewareLogger.Log(logMsg, LogLevel.INFO, fullname, branchname);
                requestPath = $"redirect-to-authenticate [access path: {requestPath}]";
            }

            // 📥 11. Analyze JSON or form POST body for attack patterns
            if (ContainsMaliciousRequestBodyContent(request, response, requestPath, fullUrl, ip))
                return;

            // 🧬 12. GeoIP filter — allow only CM or local development IPs
        
            if (!country.Equals("CM", StringComparison.OrdinalIgnoreCase) && !isLocalDevIp)
            {
                string logMsg = $"🚨 GeoIP Restriction. Foreign IP blocked by GeoIP: {requestPath} | IP: {ip} | Country: {country} | URL: {fullUrl}";
                System.Diagnostics.Debug.WriteLine(logMsg);
                AdvancedMiddlewareLogger.Log(logMsg, LogLevel.WARN, fullname, branchname);
                HandleSuspiciousPath(request, response, requestPath, logMsg, "GeoIP Restriction", fullUrl);
                HandleRateLimitExceeded(response, ip, "GeoIP Restriction", 0);
                return;
            }

            // 🚷 13. Match suspicious paths (stored in DB or memory cache)
            if (IsSuspiciousPathCached(requestPath))
            {
                string logMsg = $"🚨 Suspicious path detected: {requestPath} | URL: {fullUrl}";
                System.Diagnostics.Debug.WriteLine(logMsg);
                AdvancedMiddlewareLogger.Log(logMsg, LogLevel.WARN, fullname, branchname);
                HandleSuspiciousPath(request, response, requestPath, logMsg, "Suspicious-Attack", fullUrl);
                HandleRateLimitExceeded(response, ip, "Suspicious-Attack", 0);
                return;
            }

            // 📈 14. Apply rate limiting to detect bursts of traffic
            var config = RateLimitConfigHolder.Config;
            int requestLimit = config.RequestLimit;
            TimeSpan timeWindow = TimeSpan.FromSeconds(config.TimeWindowSeconds);
            TimeSpan blockDuration = TimeSpan.FromMinutes(config.BlockDurationMinutes);

            bool isBlocked = false;
            string reasonOfBlocked = string.Empty;

            if (_rateLimitedUserService.ShouldBlock(ip, requestLimit, timeWindow))
            {
                isBlocked = true;
                 reasonOfBlocked = $"🚫 DDOS Exceeded request threshold. IP {ip} Blocked for {blockDuration.TotalMinutes} min | URL: {fullUrl}";
                System.Diagnostics.Debug.WriteLine(reasonOfBlocked);
                AdvancedMiddlewareLogger.Log(reasonOfBlocked, LogLevel.WARN, fullname, branchname);

                // Persist to DB and cache for temporary block
                Task.Run(() => _rateLimitedUserService.BlockUser(ip, username, "IP-Based", blockDuration, reasonOfBlocked,
                    location, lat, lon, city, region, country, branchid, branchcode, branchname, tel, fullname));
                CacheTemporaryBlock(ip);
                HandleRateLimitExceeded(response, ip, reasonOfBlocked, config.BlockDurationMinutes);
                return;
            }

            // 📝 15. Final stage: log all accepted and clean requests
            Task.Run(() => _loggerService.LogRequest(
                ip, username, computerName, requestPath, 200,
                location, lat, lon, city, region, country,
                branchid, branchcode, branchname, tel, fullname,
                isBlocked, reasonOfBlocked, isAuthenticated, fullUrl
            ));
        }
        public bool IsTemporarilyBlocked(string ipOrUser)
        {

            string cacheKey = $"Blocked:{ipOrUser}";
            return _blockCache.Contains(cacheKey);
        }

        private bool ContainsMaliciousRequestBodyContent(HttpRequest request, HttpResponse response, string requestPath, string fullUrl, string ip)
        {
            try
            {
                if (request.HttpMethod != "POST" || request.ContentType == null)
                    return false;

                // ✅ Only inspect JSON and Form URL-encoded
                if (!(request.ContentType.Contains("application/json") ||
                      request.ContentType.Contains("application/x-www-form-urlencoded")))
                    return false;

                string bodyContent = ReadRequestBody(request);

                if (!string.IsNullOrEmpty(bodyContent))
                {
                    if (_scanner.IsMalicious(bodyContent, out string matchedPattern))
                    {
                        string logMsg = $"🚨 Malicious Request Body Detected pattern matched in request body. IP: {ip} | Pattern: {matchedPattern}";
                        System.Diagnostics.Debug.WriteLine(logMsg);
                        AdvancedMiddlewareLogger.Log(logMsg, LogLevel.WARN, globalUserFullname, globalbranchname);

                        HandleSuspiciousPath(request, response, requestPath, logMsg, "Request-Body-Filter", fullUrl);
                        HandleRateLimitExceeded(response, ip, "Malicious Payload", 0);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                string logMsg = $"⚠️ Error scanning request body: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(logMsg);
                AdvancedMiddlewareLogger.Log(logMsg, LogLevel.ERROR, globalUserFullname, globalbranchname);
            }

            return false;
        }

        private bool ContainsMaliciousMultipartContent(HttpRequest request, HttpResponse response, string requestPath, string fullUrl, string ip)
        {
            try
            {
                // 🔍 Scan form fields
                foreach (string key in request.Form)
                {
                    var value = request.Form[key];
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        if (_scanner.IsMalicious(value, out string matchedPattern))
                        {
                            string logMsg = $"🚨 Malicious multipart field pattern found in form field '{key}' on path {requestPath}. Pattern: {matchedPattern}";
                            System.Diagnostics.Debug.WriteLine(logMsg);
                            AdvancedMiddlewareLogger.Log(logMsg, LogLevel.WARN, globalUserFullname, globalbranchname);

                            HandleSuspiciousPath(request, response, requestPath, logMsg, "Request-Body-Filter", fullUrl);
                            HandleRateLimitExceeded(response, ip, "Malicious Field Detected", 0);
                            return true;
                        }
                    }
                }

                // 🔍 Scan small text files (e.g. .txt)
                foreach (string fileKey in request.Files)
                {
                    var file = request.Files[fileKey];

                    if (file != null && file.ContentLength > 0 && file.ContentLength < 1024 * 1024)
                    {
                        string contentType = file.ContentType.ToLowerInvariant();
                        if (contentType.StartsWith("text/") || contentType.Contains("plain"))
                        {
                            using (var reader = new StreamReader(file.InputStream))
                            {
                                string content = reader.ReadToEnd();

                                if (_scanner.IsMalicious(content, out string matchedFilePattern))
                                {
                                    string logMsg = $"🚨 Malicious pattern found in uploaded file '{file.FileName}' on path {requestPath}. Pattern: {matchedFilePattern}";
                                    System.Diagnostics.Debug.WriteLine(logMsg);
                                    AdvancedMiddlewareLogger.Log(logMsg, LogLevel.WARN, globalUserFullname, globalbranchname);

                                    HandleSuspiciousPath(request, response, requestPath, logMsg, "Request-File-Filter", fullUrl);
                                    HandleRateLimitExceeded(response, ip, "Malicious File Upload", 0);
                                    return true;
                                }
                            }

                            file.InputStream.Position = 0; // reset stream for downstream usage
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string logMsg = $"⚠️ Error while scanning multipart content: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(logMsg);
                AdvancedMiddlewareLogger.Log(logMsg, LogLevel.ERROR, globalUserFullname, globalbranchname);
            }

            return false;
        }

        private string ReadRequestBody(HttpRequest request)
        {
            if (request.InputStream == null || !request.InputStream.CanRead)
                return string.Empty;

            try
            {
                request.InputStream.Position = 0;
                using (var reader = new StreamReader(request.InputStream, request.ContentEncoding, true, 1024, true))
                {
                    string body = reader.ReadToEnd();
                    request.InputStream.Position = 0; // reset for downstream usage
                    return body;
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        private bool IsValidIPv4(string ip)
        {
            return IPAddress.TryParse(ip, out var addr) && addr.AddressFamily == AddressFamily.InterNetwork;
        }

        public void CacheTemporaryBlock(string ipOrUser, int durationMinutes = 10)
        {
            if (_blockCache.GetCount() > 10000) // or 5000
            {
                System.Diagnostics.Debug.WriteLine("⚠️ Block cache count high: " + _blockCache.GetCount());
                // Optionally purge oldest / log alerts
            }
            string cacheKey = $"Blocked:{ipOrUser}";

            var policy = new CacheItemPolicy
            {
                AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(durationMinutes),
                SlidingExpiration = TimeSpan.FromMinutes(5) // optional refresh window
            };

            _blockCache.Set(cacheKey, true, policy);
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
            "/Content/", "/Scripts/", "/fonts/", "/Images/","/dashboard/getlivedashboardheadoffice",
            "/session/getidletimeout", "/signalr/hubs","/bundles/jqueryval","/aspnet_client/system_web/4_0_30319/crystalreportviewers13/js/dhtmllib/empty.html"
        };

        private static readonly List<string> CameroonCidrs = new List<string>
        {
            // CAMTEL Allocations
            "41.202.219.0/24",
            "41.202.220.0/23",
            "41.202.222.0/24",
            "41.202.223.0/24",
            "41.217.192.0/18",
            "41.221.240.0/21",
            "102.244.192.0/18",
            "129.151.128.0/17",
            "154.66.128.0/17",
            "154.70.0.0/17",
            "154.72.162.0/23",
            "154.73.64.0/18",
            "154.118.64.0/18",
            "154.120.96.0/19",
            "160.120.0.0/16",
            "160.153.0.0/16",
            "165.98.0.0/16",
            "169.255.64.0/18",
            "197.149.192.0/18",
            "197.159.160.0/19",
            "197.214.0.0/17",
            "197.231.0.0/17",

            // MTN Cameroon Allocations
            "129.0.0.0/16",
            "129.0.101.0/24",
            "129.0.102.0/24",
            "129.0.103.0/24",
            "129.0.109.0/24",
            "129.0.110.0/24",
            "129.0.111.0/24",
            "129.0.113.0/24",
            "129.0.125.0/24",
            "129.0.128.0/24",
            "129.0.129.0/24",
            "129.0.130.0/24",
            "129.0.131.0/24",
            "129.0.132.0/24",
            "129.0.133.0/24",
            "129.0.134.0/24",
            "129.0.135.0/24",
            "129.0.136.0/24",
            "129.0.137.0/24",
            "129.0.138.0/24",
            "129.0.139.0/24",
            "129.0.140.0/24",
            "129.0.141.0/24",
            "129.0.142.0/24",
            "129.0.143.0/24",
            "129.0.144.0/24",
            "129.0.145.0/24",
            "129.0.146.0/24",
            "129.0.147.0/24",
            "129.0.148.0/24",
            "129.0.149.0/24",
            "129.0.150.0/24",
            "129.0.151.0/24",
            "129.0.152.0/24",
            "129.0.153.0/24",
            "129.0.154.0/24",
            "129.0.156.0/24",
            "129.0.157.0/24",
            "129.0.158.0/24",
            "129.0.159.0/24",
            "129.0.160.0/24",
            "129.0.164.0/24",
            "129.0.165.0/24",
            "129.0.168.0/21",
            "129.0.168.0/24",
            "129.0.169.0/24",
            "129.0.171.0/24",
            "129.0.172.0/24",
            "129.0.173.0/24",
            "129.0.180.0/24",
            "129.0.181.0/24",
            "129.0.182.0/24",
            "129.0.183.0/24",
            "129.0.188.0/24",
            "129.0.190.0/24",
            "129.0.202.0/24",
            "129.0.203.0/24",
            "129.0.204.0/24",
            "129.0.205.0/24",
            "129.0.206.0/24",
            "129.0.207.0/24",
            "129.0.208.0/24",
            "129.0.209.0/24",
            "129.0.210.0/24",
            "129.0.211.0/24",
            "129.0.212.0/24",
            "129.0.213.0/24",
            "129.0.214.0/24",
            "129.0.215.0/24",
            "129.0.216.0/24",
            "129.0.217.0/24",
            "129.0.218.0/24",
            "129.0.219.0/24",
            "129.0.220.0/24",
            "129.0.226.0/24",
            "129.0.231.0/24",
            "129.0.232.0/24",
            "129.0.233.0/24",
            "129.0.234.0/24",
            "129.0.237.0/24",
            "129.0.238.0/24",
            "129.0.239.0/24",
            "129.0.255.0/24",
            "129.0.125.0/24",

            // Creolink Communications Allocations
            "41.223.28.0/22",
            "154.126.160.0/19",
            "154.126.163.0/24",
            "154.126.165.0/24",
            "154.126.166.0/23",
            "154.126.168.0/22",
            "154.126.172.0/24",
            "154.126.173.0/24",
            "154.126.176.0/24",
            "154.126.178.0/24",
            "154.126.183.0/24",
            "154.126.190.0/24",

            // Orange Cameroun SA Allocations
            "41.202.192.0/19",
            "41.202.216.0/23",
            "41.202.217.0/24",
            "41.202.219.0/24",
            "102.244.0.0/14",



            "143.105.152.0/24"
        };


        private static readonly string[] BadUserAgents =
        {
            "", // Empty User-Agent
            "curl", "httpclient", "python", "sqlmap", "fuzzer", "wget", "libwww",
            "go-http-client", "scan", "scrapy", "nmap", "nessus", "masscan", "nikto",
            "httprequest", "powershell", "java", "perl", "ruby", "httpget", "netcat",
            "curl/7", "http-post", "metasploit", "acunetix", "netsparker", "dirbuster",
            "zmeu", "sqlninja", "webinspect", "openvas", "qualys", "jaascois", "havij",
            "morfeus", "httplib", "bot", "spider", "crawler", "hacktool", "recon",
            "loader", "brutus", "hydra", "paros", "burpsuite", "headless", "phantomjs",
            "python-requests", "okhttp", "axios", "node-fetch", "lwp", "java/", "vbscript",
            "curl-winhttp", "fetch", "http_request2", "cyberduck"
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
        private bool IsSuspiciousHeaders(HttpRequest request, HttpResponse response, string requestPath, string fullUrl, string ip)
        {
            try
            {
                string userAgent = request.UserAgent?.ToLowerInvariant() ?? "";
                string referer = request.Headers["Referer"];
                string origin = request.Headers["Origin"];

                // 1️⃣ Block bad or empty User-Agent
                // Normalize the user agent

                // Try to match which specific bad keyword triggered
                string matchedBadAgent = BadUserAgents
                    .Select(b => b.ToLowerInvariant())
                    .FirstOrDefault(bad => userAgent.Contains(bad));

                if (!string.IsNullOrEmpty(matchedBadAgent))
                {
                    string logMsg = $"🚨 Blocked request with suspicious User-Agent: '{request.UserAgent}' " +
                                    $"| Matched: '{matchedBadAgent}' | IP: {ip}";
                    System.Diagnostics.Debug.WriteLine(logMsg);
                    AdvancedMiddlewareLogger.Log(logMsg, LogLevel.WARN, globalUserFullname, globalbranchname);

                    HandleSuspiciousPath(request, response, requestPath, logMsg, "Header-Filter", fullUrl);
                    HandleRateLimitExceeded(response, ip, $"Bad User-Agent ({matchedBadAgent})", 0);
                    return true;
                }


                // 2️⃣ Optional: Block requests to secure paths with missing Referer
                //if (requestPath.StartsWith("/dashboard") || requestPath.StartsWith("/secure"))
                //{
                //    if (string.IsNullOrWhiteSpace(referer))
                //    {
                //        string logMsg = $"🚨 Missing Referer Header on protected path '{requestPath}' | IP: {ip}";
                //        System.Diagnostics.Debug.WriteLine(logMsg);
                //        AdvancedMiddlewareLogger.Log(logMsg, LogLevel.WARN, globalUserFullname, globalbranchname);

                //        HandleSuspiciousPath(request, response, requestPath, logMsg, "Header-Filter", fullUrl);
                //        HandleRateLimitExceeded(response, ip, "Missing Referer", 0);
                //        return true;
                //    }
                //}

                // 3️⃣ Optional: Check invalid Origin headers (e.g. non-site POST)
                if (request.HttpMethod == "POST" && !string.IsNullOrWhiteSpace(origin))
                {
                    //bool originIsAllowed = AllowedOriginDomains.Any(allowed =>
                    //    origin.StartsWith(allowed, StringComparison.OrdinalIgnoreCase));
                    bool originIsAllowed = AllowedOriginDomains.Contains(origin.ToLowerInvariant());
                    if (!originIsAllowed)
                    {
                        string logMsg = $"🚨 Suspicious Origin header: '{origin}' | Path: {requestPath} | IP: {ip}";
                        System.Diagnostics.Debug.WriteLine(logMsg);
                        AdvancedMiddlewareLogger.Log(logMsg, LogLevel.WARN, globalUserFullname, globalbranchname);

                        HandleSuspiciousPath(request, response, requestPath, logMsg, "Header-Filter", fullUrl);
                        HandleRateLimitExceeded(response, ip, "Invalid Origin", 0);
                        return true;
                    }
                }


            }
            catch (Exception ex)
            {
                string logMsg = $"⚠️ Error during header sanity check: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(logMsg);
                AdvancedMiddlewareLogger.Log(logMsg, LogLevel.ERROR, globalUserFullname, globalbranchname);
            }

            return false;
        }

        private bool IsMaliciousRequest(HttpRequest request, HttpResponse response, string requestPath, string fullUrl, string ip)
        {
            try
            {
                // 📦 1. Scan request body (application/json or form-urlencoded)
                if (request.HttpMethod == "POST" && request.ContentType != null &&
                    (request.ContentType.Contains("application/json") || request.ContentType.Contains("application/x-www-form-urlencoded")))
                {
                    string bodyContent = ReadRequestBody(request);

                    if (!string.IsNullOrWhiteSpace(bodyContent) &&
                        _scanner.IsMalicious(bodyContent, out string matchedBodyPattern))
                    {
                        string logMsg = $"🚨 Malicious pattern in request body. IP: {ip} | Pattern: {matchedBodyPattern}";
                        System.Diagnostics.Debug.WriteLine(logMsg);
                        AdvancedMiddlewareLogger.Log(logMsg, LogLevel.WARN, globalUserFullname, globalbranchname);

                        HandleSuspiciousPath(request, response, requestPath, "Malicious Request Body Detected", "Request-Body-Filter", fullUrl);
                        HandleRateLimitExceeded(response, ip, "Malicious Payload", 0);
                        return true;
                    }
                }

                // 📎 2. Scan multipart form fields + files
                if (request.HttpMethod == "POST" && request.ContentType != null &&
                    request.ContentType.StartsWith("multipart/form-data", StringComparison.OrdinalIgnoreCase))
                {
                    // 📝 Scan form fields
                    foreach (string key in request.Form)
                    {
                        var value = request.Form[key];
                        if (!string.IsNullOrWhiteSpace(value) &&
                            _scanner.IsMalicious(value, out string matchedFieldPattern))
                        {
                            string logMsg = $"🚨 Malicious pattern in form field '{key}'. IP: {ip} | Pattern: {matchedFieldPattern}";
                            System.Diagnostics.Debug.WriteLine(logMsg);
                            AdvancedMiddlewareLogger.Log(logMsg, LogLevel.WARN, globalUserFullname, globalbranchname);

                            HandleSuspiciousPath(request, response, requestPath, "Malicious multipart field", "Request-Body-Filter", fullUrl);
                            HandleRateLimitExceeded(response, ip, "Malicious Field Detected", 0);
                            return true;
                        }
                    }

                    // 📁 Scan uploaded files (only text files under 1MB)
                    foreach (string fileKey in request.Files)
                    {
                        var file = request.Files[fileKey];
                        if (file != null && file.ContentLength > 0 && file.ContentLength < 1024 * 1024)
                        {
                            string contentType = file.ContentType.ToLowerInvariant();
                            if (contentType.StartsWith("text/") || contentType.Contains("plain"))
                            {
                                using (var reader = new StreamReader(file.InputStream))
                                {
                                    string content = reader.ReadToEnd();
                                    if (_scanner.IsMalicious(content, out string matchedFilePattern))
                                    {
                                        string logMsg = $"🚨 Malicious pattern in uploaded file '{file.FileName}'. IP: {ip} | Pattern: {matchedFilePattern}";
                                        System.Diagnostics.Debug.WriteLine(logMsg);
                                        AdvancedMiddlewareLogger.Log(logMsg, LogLevel.WARN, globalUserFullname, globalbranchname);

                                        HandleSuspiciousPath(request, response, requestPath, "Malicious upload content", "Request-File-Filter", fullUrl);
                                        HandleRateLimitExceeded(response, ip, "Malicious File Upload", 0);
                                        return true;
                                    }
                                }

                                file.InputStream.Position = 0; // Reset for downstream processing
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string logMsg = $"⚠️ Exception during malicious request check: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(logMsg);
                AdvancedMiddlewareLogger.Log(logMsg, LogLevel.ERROR, globalUserFullname, globalbranchname);
            }

            return false;
        }

        public void Dispose() { }
    }


}






