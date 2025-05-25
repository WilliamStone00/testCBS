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
    public class RateLimitingMiddleware : IHttpModule
    {
        private readonly RateLimitConfigService _configService = new RateLimitConfigService();
        private readonly RateLimiteTrackerLoggerServices _loggerService = new RateLimiteTrackerLoggerServices();
        private readonly RateLimitedUserService _rateLimitedUserService = new RateLimitedUserService();

        private static readonly MemoryCache _ipCache = MemoryCache.Default;

        public void Init(HttpApplication context)
        {
            context.PostAuthenticateRequest += OnPostAuthenticateRequest;
        }

        private string GetUserNameFromTSCCookie(HttpRequest request)
        {
            try
            {
                var authCookie = request.Cookies["TSC"];
                if (authCookie == null) return "Unknown";

                var ticket = FormsAuthentication.Decrypt(authCookie.Value);
                if (ticket == null || ticket.Expired) return "Unknown";

                var userData = JsonConvert.DeserializeObject<CustomSerializeModel>(ticket.UserData);
                return string.IsNullOrWhiteSpace(userData?.UserName) ? "Unknown" : userData.UserName;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error reading TSC cookie: {ex.Message}");
                return "Unknown";
            }
        }

        private void OnPostAuthenticateRequest(object sender, EventArgs e)
        {
            var app = (HttpApplication)sender;
            var request = app.Context.Request;
            var response = app.Context.Response;
            var httpContext = HttpContext.Current;

            var userName = GetUserNameFromTSCCookie(request);

            string requestPath = request.Path.ToLower();
            var (userIp, location) = GetIpAndLocationSync();
            string computerName = request.Headers["Computer-Name"] ?? "Unknown Computer";
            DateTime now = DateTime.UtcNow;

            // Check if user is globally blocked from DB
            var isBlockedResponse = _rateLimitedUserService.CheckIsBlocked(userIp);
            if (isBlockedResponse == true)
            {
                HandleRateLimitExceeded(response);
                return;
            }

            var config = RateLimitConfigHolder.Config;

            int requestLimit = config.RequestLimit;
            TimeSpan timeWindow = TimeSpan.FromSeconds(config.TimeWindowSeconds);
            TimeSpan blockDuration = TimeSpan.FromMinutes(config.BlockDurationMinutes);

            if (IsAjaxRequest(request) || IsExcludedPath(requestPath))
            {
                System.Diagnostics.Debug.WriteLine($"Skipped request: {requestPath}");
                return;
            }

            if (_rateLimitedUserService.ShouldBlock(userIp, requestLimit, timeWindow))
            {
                _rateLimitedUserService.BlockUser(userIp, userName, "User-Based", blockDuration, "Exceeded user-based request limit",location);
                HandleRateLimitExceeded(response);
                return;
            }

            Task.Run(() => _loggerService.LogRequest(userIp, userName, computerName, requestPath, 200, location));
        }

        private bool IsAjaxRequest(HttpRequest request)
        {
            string requestedWith = request.Headers["X-Requested-With"];
            return !string.IsNullOrEmpty(requestedWith) && requestedWith.Equals("XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
        }

        private async Task<string> FetchUserLocationAsync(string ipAddress)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    string url = $"https://ipinfo.io/{ipAddress}/json";
                    var response = await client.GetStringAsync(url);
                    dynamic data = JsonConvert.DeserializeObject(response);
                    return $"{data.city}, {data.region}, {data.country}";
                }
            }
            catch
            {
                return "Unknown Location";
            }
        }

        private void HandleRateLimitExceeded(HttpResponse response)
        {
            response.Clear();
            response.StatusCode = 429;
            response.Redirect("/Error/Show/429", false);
            response.End();
        }

        private (string IpAddress, string Location) GetIpAndLocationSync()
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
                    return ((string IpAddress, string Location))_ipCache.Get(cacheKey);

                using (var client = new WebClient())
                {
                    string json = client.DownloadString($"https://ipinfo.io/{ip}/json");
                    dynamic result = JsonConvert.DeserializeObject(json);

                    string city = result?.city ?? "";
                    string region = result?.region ?? "";
                    string country = result?.country ?? "";

                    string location = $"{city}, {region}, {country}".Trim().Trim(',');

                    _ipCache.Set(cacheKey, (ip, location), DateTimeOffset.Now.AddHours(4));

                    return (ip, location);
                }
            }
            catch
            {
                return (Guid.NewGuid().ToString("N"), "Unknown");
            }
        }

        public void Dispose() { }

        private static readonly List<string> ExcludedPaths = new List<string>
    {
        "/favicon.ico", "/error/show/404", "/error/show/500",
        "/Content/", "/Scripts/", "/fonts/", "/Images/",
        ".js", ".css", ".png", ".jpg", ".jpeg", ".gif", ".woff", ".woff2", ".ttf", ".ico",
        "/session/getidletimeout", "/signalr/hubs"
    };

        private static readonly List<string> ExcludedExtensions = new List<string>
    {
        ".css", ".js", ".png", ".jpg", ".jpeg", ".gif", ".woff", ".woff2", ".ttf", ".ico", ".json"
    };

        private async Task LogRequestAsync(string userIp, string mac, string computerName, string path)
        {
            if (IsExcludedPath(path))
                return;

            string location = await FetchUserLocationAsync(userIp);
            _loggerService.LogRequest(userIp, mac, computerName, path, 200, location);
        }

        private bool IsExcludedPath(string path)
        {
            return ExcludedPaths.Any(excludedPath => path.StartsWith(excludedPath, StringComparison.OrdinalIgnoreCase)) ||
                   ExcludedExtensions.Contains(System.IO.Path.GetExtension(path).ToLower());
        }

        private class RequestCounter
        {
            public bool IsBlocked { get; set; } = false;
            public DateTime BlockEndTime { get; set; }
            public bool HasWarned { get; set; } = false;
            public bool HasLoggedBlock { get; set; } = false;
            public List<DateTime> RequestTimestamps { get; } = new List<DateTime>();
        }
    }

}






//namespace CBS.FrontDesk.UI.Filter
//{
//    using global::CBS.BusinessService.RequestLoggerServicesP;
//    using System;
//    using System.Collections.Concurrent;
//    using System.Collections.Generic;
//    using System.Net.NetworkInformation;
//    using System.Web;


//    namespace CBS.FrontDesk.UI.Filter
//    {
//        public class RateLimitingMiddleware : IHttpModule
//        {
//            private readonly RateLimitConfigService _configService = new RateLimitConfigService();
//            private readonly RateLimiteTrackerLoggerServices _loggerService = new RateLimiteTrackerLoggerServices();

//            private static readonly ConcurrentDictionary<string, RequestCounter> RequestCounters = new ConcurrentDictionary<string, RequestCounter>();

//            public void Init(HttpApplication context)
//            {
//                context.BeginRequest += OnBeginRequest;
//            }

//            private void OnBeginRequest(object sender, EventArgs e)
//            {
//                var app = (HttpApplication)sender;
//                var request = app.Context.Request;
//                var response = app.Context.Response;

//                string requestPath = request.Path.ToLower();
//                string userIp = request.UserHostAddress ?? "Unknown IP";
//                string macAddress = GetMacAddress();
//                string computerName = Environment.MachineName;
//                DateTime now = DateTime.UtcNow;

//                var config = _configService.GetRateLimitConfig();
//                int requestLimit = config.RequestLimit;
//                TimeSpan timeWindow = TimeSpan.FromSeconds(config.TimeWindowSeconds);
//                TimeSpan blockDuration = TimeSpan.FromMinutes(config.BlockDurationMinutes);

//                // ✅ Exclude specific paths from rate limiting
//                if (IsWhitelistedRequest(request))
//                {
//                    _loggerService.LogRequest(userIp, macAddress, computerName, requestPath, 200);
//                    return;
//                }

//                // Apply Rate Limiting Logic
//                if (!RequestCounters.TryGetValue(userIp, out var counter))
//                {
//                    counter = new RequestCounter();
//                    RequestCounters.TryAdd(userIp, counter);
//                }

//                lock (counter)
//                {
//                    counter.RequestTimestamps.RemoveAll(ts => now - ts > timeWindow);

//                    if (counter.IsBlocked)
//                    {
//                        if (now < counter.BlockEndTime)
//                        {
//                            _loggerService.LogBlockedUser(userIp, macAddress, computerName, counter.BlockEndTime);
//                            HandleRateLimitExceeded(response);
//                            return;
//                        }

//                        counter.IsBlocked = false;
//                        counter.RequestTimestamps.Clear();
//                    }

//                    counter.RequestTimestamps.Add(now);

//                    if (counter.RequestTimestamps.Count > requestLimit)
//                    {
//                        counter.IsBlocked = true;
//                        counter.BlockEndTime = now.Add(blockDuration);
//                        _loggerService.LogBlockedUser(userIp, macAddress, computerName, counter.BlockEndTime);
//                        HandleRateLimitExceeded(response);
//                    }
//                }

//                // Log request
//                _loggerService.LogRequest(userIp, macAddress, computerName, requestPath, 200);
//            }

//            private void HandleRateLimitExceeded(HttpResponse response)
//            {
//                response.Clear();
//                response.StatusCode = 429;
//                response.Redirect("/Error/Show/429", false);
//                response.End();
//            }

//            private bool IsWhitelistedRequest(HttpRequest request)
//            {
//                var config = _configService.GetRateLimitConfig();
//                foreach (var header in config.WhitelistedHeaders)
//                {
//                    if (!string.IsNullOrEmpty(request.Headers[header]))
//                    {
//                        return true;
//                    }
//                }
//                return false;
//            }

//            private string GetMacAddress()
//            {
//                foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
//                {
//                    if (nic.OperationalStatus == OperationalStatus.Up)
//                    {
//                        return nic.GetPhysicalAddress().ToString();
//                    }
//                }
//                return "Unknown MAC";
//            }

//            public void Dispose() { }

//            private class RequestCounter
//            {
//                public bool IsBlocked { get; set; } = false;
//                public DateTime BlockEndTime { get; set; }
//                public List<DateTime> RequestTimestamps { get; } = new List<DateTime>();
//            }
//        }
//    }
//}
