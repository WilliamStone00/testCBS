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
//using MvcSiteMapProvider.Caching;
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
    /// 📛 TSCWAFMiddleware is an HTTP module for ASP.NET MVC applications designed to:
    /// - Block abusive users based on rate limits.
    /// - Detect and respond to suspicious path access (security probing).
    /// - Log suspicious activity with metadata like IP, user, and device info.
    /// - Integrate with internal services for blocking, logging, and rate tracking.
    ///
    /// It protects the application from brute force, DDoS-like behavior, or probing attempts.
    /// </summary>


    public class TSCWAFMiddleware : IHttpModule
    {
        //private readonly RateLimitConfigService _configService = new RateLimitConfigService();
        private readonly RateLimiteTrackerLoggerServices _loggerService = new RateLimiteTrackerLoggerServices();
        private readonly RateLimitedUserService _rateLimitedUserService = new RateLimitedUserService();
        private readonly SuspiciousPathService _suspiciousPathService = new SuspiciousPathService();
        private static readonly MemoryCache SuspiciousPathCache = MemoryCache.Default;
        private static readonly MemoryCache _ipCache = MemoryCache.Default;
        private static readonly MemoryCache _blockCache = MemoryCache.Default;
        private readonly MaliciousContentScannerService _scanner = new MaliciousContentScannerService();
        RateLimitConfig rateLimitConfig = RateLimitConfigHolder.Config;

        string globalUserFullname = "anonymous";
        string globalbranchname = "global";




        private static readonly string EnvironmentName = ConfigurationManager.AppSettings["EnvironmentX"]?.Trim() ?? "Production";

        private static readonly List<string> AllowedOriginDomains = (ConfigurationManager.AppSettings["AllowedOrigins"] ?? "")
        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
        .Select((origin, index) => new { origin = origin.Trim(), index })
        .Where(entry =>
        {
            // 🧠 Environment-specific origin inclusion
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
                return new Uri(entry.origin).Host.ToLowerInvariant();
            }
            catch
            {
                return null;
            }
        })
        .Where(host => !string.IsNullOrEmpty(host))
        .Distinct()
        .ToList();





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
                if (!CidrUtility.IsValidIPv4(ip))
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

            // 🌍 7. GeoIP Enforcement — Enforce access only from Cameroon or approved countries/IPs
            if (!isLocalDevIp)
            {
                // 7.1 🚫 Reject if country is not whitelisted
                if (!CidrUtility.IsWhitelistedCountry(country, rateLimitConfig.WhitelistedCountriesCode))
                {
                    string logMsg = $"🚨 GeoIP Restriction — Blocked: Country '{country}' is not whitelisted | IP: {ip} | Path: {requestPath} | URL: {fullUrl}";
                    System.Diagnostics.Debug.WriteLine(logMsg);
                    AdvancedMiddlewareLogger.Log(logMsg, LogLevel.WARN, fullname, branchname);
                    HandleSuspiciousPath(request, response, requestPath, logMsg, "GeoIP Country Restriction", fullUrl);
                    HandleRateLimitExceeded(response, ip, "GeoIP Country Restriction", 0);
                    return;
                }

                // 7.2 🚫 Reject if IP is not in any allowed CIDR block
                if (!CidrUtility.IsIpInCidr(ip, rateLimitConfig.WhitelistedCidrs))
                {
                    string logMsg = $"🚨 CIDR Restriction — Blocked: IP '{ip}' not in any whitelisted CIDR | Country: {country} | Path: {requestPath}";
                    System.Diagnostics.Debug.WriteLine(logMsg);
                    AdvancedMiddlewareLogger.Log(logMsg, LogLevel.WARN, globalUserFullname, globalbranchname);
                    HandleSuspiciousPath(request, response, requestPath, logMsg, "GeoIP-CIDR Restriction", fullUrl);
                    HandleRateLimitExceeded(response, ip, "GeoIP CIDR Restriction", 0);
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
            int requestLimit = rateLimitConfig.RequestLimit;
            TimeSpan timeWindow = TimeSpan.FromSeconds(rateLimitConfig.TimeWindowSeconds);
            TimeSpan blockDuration = TimeSpan.FromMinutes(rateLimitConfig.BlockDurationMinutes);

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
                HandleRateLimitExceeded(response, ip, reasonOfBlocked, rateLimitConfig.BlockDurationMinutes);
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

            var suspiciousIndicators = rateLimitConfig.SuspiciousIndicators;

            bool isExcluded = rateLimitConfig.ExcludedExtensions.Any(ext => lower.EndsWith(ext)) ||
                              rateLimitConfig.ExcludedPaths.Any(p => lower.StartsWith(p));

            string logMsg = $"🔍 Checked static path: {path} | Result: {(isExcluded ? "Excluded" : "Included")}";
            System.Diagnostics.Debug.WriteLine(logMsg);
            AdvancedMiddlewareLogger.Log(logMsg, LogLevel.INFO, globalUserFullname, globalbranchname);

            return isExcluded;
        }




       
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

                // 1️⃣ Block known bad or spoofed User-Agent
                string matchedBadAgent = rateLimitConfig.BadUserAgents.Select(b => b.ToLowerInvariant()).FirstOrDefault(bad => userAgent.Contains(bad));

                if (!string.IsNullOrEmpty(matchedBadAgent))
                {
                    string logMsg = $"🚨 Blocked request with suspicious User-Agent: '{request.UserAgent}' | Matched: '{matchedBadAgent}' | IP: {ip}";
                    System.Diagnostics.Debug.WriteLine(logMsg);
                    AdvancedMiddlewareLogger.Log(logMsg, LogLevel.WARN, globalUserFullname, globalbranchname);

                    HandleSuspiciousPath(request, response, requestPath, logMsg, "Header-Filter", fullUrl);
                    HandleRateLimitExceeded(response, ip, $"Bad User-Agent ({matchedBadAgent})", 0);
                    return true;
                }

                // 2️⃣ Block POST requests to secure areas without Referer
                //if ((requestPath.StartsWith("/dashboard", StringComparison.OrdinalIgnoreCase) ||
                //     requestPath.StartsWith("/secure", StringComparison.OrdinalIgnoreCase)) &&
                //    string.IsNullOrWhiteSpace(referer))
                //{
                //    string logMsg = $"🚨 Missing Referer Header on protected path '{requestPath}' | IP: {ip}";
                //    System.Diagnostics.Debug.WriteLine(logMsg);
                //    AdvancedMiddlewareLogger.Log(logMsg, LogLevel.WARN, globalUserFullname, globalbranchname);

                //    HandleSuspiciousPath(request, response, requestPath, logMsg, "Header-Filter", fullUrl);
                //    HandleRateLimitExceeded(response, ip, "Missing Referer", 0);
                //    return true;
                //}
                // 3️⃣ Block POST requests with suspicious or unauthorized Origin headers
                if (request.HttpMethod == "POST" && !string.IsNullOrWhiteSpace(origin))
                {
                    string originHost = string.Empty;

                    try
                    {
                        // Parse and extract the domain host from the Origin header
                        originHost = new Uri(origin).Host.ToLowerInvariant();
                    }
                    catch
                    {
                        originHost = ""; // ⚠️ Origin header is malformed or invalid
                    }

                    // 🚫 If the origin is not on the allowlist, block the request
                    if (!string.IsNullOrWhiteSpace(originHost) && !AllowedOriginDomains.Contains(originHost))
                    {
                        string logMsg = $"❌ Request denied due to unauthorized Origin: '{origin}' (host: '{originHost}'). " +
                                        $"Path: '{requestPath}' | IP: {ip} | Reason: Origin not in allowlist.";

                        System.Diagnostics.Debug.WriteLine(logMsg);
                        AdvancedMiddlewareLogger.Log(logMsg, LogLevel.WARN, globalUserFullname, globalbranchname);

                        HandleSuspiciousPath(
                            request,
                            response,
                            requestPath,
                            "Unauthorized Origin detected in request header",
                            "Header-Origin-Filter",
                            fullUrl
                        );

                        HandleRateLimitExceeded(response, ip, "Blocked due to unauthorized Origin", 0);
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

        private bool IsSuspiciousHeadersxx(HttpRequest request, HttpResponse response, string requestPath, string fullUrl, string ip)
        {
            try
            {
                string userAgent = request.UserAgent?.ToLowerInvariant() ?? "";
                string referer = request.Headers["Referer"];
                string origin = request.Headers["Origin"];

                // 1️⃣ Block bad or empty User-Agent
                // Normalize the user agent

                // Try to match which specific bad keyword triggered
                string matchedBadAgent = rateLimitConfig.BadUserAgents.Select(b => b.ToLowerInvariant()).FirstOrDefault(bad => userAgent.Contains(bad));

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
                if (requestPath.StartsWith("/dashboard") || requestPath.StartsWith("/secure"))
                {
                    if (string.IsNullOrWhiteSpace(referer))
                    {
                        string logMsg = $"🚨 Missing Referer Header on protected path '{requestPath}' | IP: {ip}";
                        System.Diagnostics.Debug.WriteLine(logMsg);
                        AdvancedMiddlewareLogger.Log(logMsg, LogLevel.WARN, globalUserFullname, globalbranchname);

                        HandleSuspiciousPath(request, response, requestPath, logMsg, "Header-Filter", fullUrl);
                        HandleRateLimitExceeded(response, ip, "Missing Referer", 0);
                        return true;
                    }
                }

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






