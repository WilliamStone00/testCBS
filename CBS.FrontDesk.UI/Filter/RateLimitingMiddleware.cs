using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Web;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using CBS.BusinessService.RequestLoggerServicesP;

namespace CBS.FrontDesk.UI.Filter
{
    public class RateLimitingMiddleware : IHttpModule
    {
        private readonly RateLimitConfigService _configService = new RateLimitConfigService();
        private readonly RequestLoggerService _loggerService = new RequestLoggerService();

        private static readonly ConcurrentDictionary<string, RequestCounter> RequestCounters = new ConcurrentDictionary<string, RequestCounter>();
        private static readonly ConcurrentDictionary<string, RequestCounter> IPRequestCounters = new ConcurrentDictionary<string, RequestCounter>();

        public void Init(HttpApplication context)
        {
            context.BeginRequest += OnBeginRequest;
        }
      
        private void OnBeginRequest(object sender, EventArgs e)
        {
            var app = (HttpApplication)sender;
            var request = app.Context.Request;
            var response = app.Context.Response;

            string requestPath = request.Path.ToLower();
            string userIp = GetRealIpAddress(request);
            string macAddress = request.Headers["MAC-Address"] ?? "Unknown MAC";
            string computerName = request.Headers["Computer-Name"] ?? "Unknown Computer";
            DateTime now = DateTime.UtcNow;

            var config = _configService.GetRateLimitConfig();
            int requestLimit = config.RequestLimit;
            TimeSpan timeWindow = TimeSpan.FromSeconds(config.TimeWindowSeconds);
            TimeSpan blockDuration = TimeSpan.FromMinutes(config.BlockDurationMinutes);

            int ipRequestLimit = 300;  // IP-based limit
            TimeSpan ipWindow = TimeSpan.FromSeconds(60);  // IP window
            TimeSpan ipBlockDuration = TimeSpan.FromMinutes(30);

            // ✅ Skip AJAX and Excluded Paths
            if (IsAjaxRequest(request) || IsExcludedPath(requestPath))
            {
                System.Diagnostics.Debug.WriteLine($"Skipped request: {requestPath}");
                return;
            }

            // ✅ Apply IP-Based Rate Limiting
            ApplyIpBasedRateLimiting(userIp, response, macAddress, computerName, now, ipRequestLimit, ipWindow, ipBlockDuration);

            // ✅ Apply User-Based Rate Limiting
            bool isBlocked = ApplyRateLimiting(userIp, response, requestPath, macAddress, computerName, now, requestLimit, timeWindow, blockDuration);

            if (!isBlocked)
            {
                // Log the request asynchronously
                Task.Run(() => LogRequestAsync(userIp, macAddress, computerName, requestPath));
            }
        }

        /// <summary>
        /// Checks if the request is an AJAX request.
        /// </summary>
        private bool IsAjaxRequest(HttpRequest request)
        {
            string requestedWith = request.Headers["X-Requested-With"];
            return !string.IsNullOrEmpty(requestedWith) && requestedWith.Equals("XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Applies IP-Based Rate Limiting.
        /// </summary>
        private void ApplyIpBasedRateLimiting(string userIp, HttpResponse response, string mac, string computerName, DateTime now, int limit, TimeSpan window, TimeSpan blockDuration)
        {
            if (!IPRequestCounters.TryGetValue(userIp, out var ipCounter))
            {
                ipCounter = new RequestCounter();
                IPRequestCounters.TryAdd(userIp, ipCounter);
            }

            lock (ipCounter)
            {
                ipCounter.RequestTimestamps.RemoveAll(ts => now - ts > window);

                if (ipCounter.IsBlocked && now < ipCounter.BlockEndTime)
                {
                    Task.Run(() => LogBlockedUserAsync(userIp, mac, computerName, "IP-Based", ipCounter.BlockEndTime));
                    HandleRateLimitExceeded(response);
                    return;
                }

                if (ipCounter.RequestTimestamps.Count >= limit)
                {
                    ipCounter.IsBlocked = true;
                    ipCounter.BlockEndTime = now.Add(blockDuration);

                    Task.Run(() => LogBlockedUserAsync(userIp, mac, computerName, "IP-Based", ipCounter.BlockEndTime));
                    HandleRateLimitExceeded(response);
                }

                ipCounter.RequestTimestamps.Add(now);
            }
        }

        /// <summary>
        /// Applies User-Based Rate Limiting with 80% Warning Threshold.
        /// </summary>
        private bool ApplyRateLimiting(string userIp, HttpResponse response, string path, string mac, string computerName, DateTime now, int limit, TimeSpan window, TimeSpan blockDuration)
        {
            if (!RequestCounters.TryGetValue(userIp, out var counter))
            {
                counter = new RequestCounter();
                RequestCounters.TryAdd(userIp, counter);
            }

            lock (counter)
            {
                // Remove expired timestamps
                counter.RequestTimestamps.RemoveAll(ts => now - ts > window);

                // Handle blocked state
                if (counter.IsBlocked)
                {
                    if (now >= counter.BlockEndTime)
                    {
                        // Unblock user
                        counter.IsBlocked = false;
                        counter.RequestTimestamps.Clear();
                        counter.HasWarned = false; // Reset warning flag
                        System.Diagnostics.Debug.WriteLine($"User {userIp} unblocked.");
                    }
                    else
                    {
                        // If still blocked, handle rate limit exceeded
                        if (!counter.HasLoggedBlock)
                        {
                            Task.Run(() => LogBlockedUserAsync(userIp, mac, computerName, "User-Based", counter.BlockEndTime));
                            counter.HasLoggedBlock = true;
                        }

                        HandleRateLimitExceeded(response);
                        return true;
                    }
                }

                // Add the current request timestamp
                counter.RequestTimestamps.Add(now);

                // 80% Warning Threshold
                int warningThreshold = (int)(limit * 0.8);

                if (!counter.HasWarned && counter.RequestTimestamps.Count >= warningThreshold)
                {
                    Task.Run(() => LogWarningAsync(userIp, mac, computerName, path));
                    counter.HasWarned = true; // Set warning flag
                }

                // Apply blocking if limit is exceeded
                if (counter.RequestTimestamps.Count > limit)
                {
                    counter.IsBlocked = true;
                    counter.BlockEndTime = now.Add(blockDuration);
                    counter.HasLoggedBlock = false; // Reset for next block cycle

                    Task.Run(() => LogBlockedUserAsync(userIp, mac, computerName, "User-Based", counter.BlockEndTime));

                    HandleRateLimitExceeded(response);
                    return true;
                }
            }

            return false;
        }

 

        /// <summary>
        /// Logs blocked user data asynchronously.
        /// </summary>
        private async Task LogBlockedUserAsync(string userIp, string mac, string computerName, string blockType, DateTime blockEndTime)
        {
            string location = await FetchUserLocationAsync(userIp);
            _loggerService.LogBlockedUser(userIp, mac, computerName, blockEndTime, blockType, location);
        }

        /// <summary>
        /// Logs a warning at 80% threshold.
        /// </summary>
        private void LogWarningAsync(string userIp, string mac, string computerName, string path)
        {
            string warningMessage = $"User {userIp} is nearing the rate limit threshold. MAC: {mac}, Path: {path}";
            System.Diagnostics.Debug.WriteLine(warningMessage);
            _loggerService.LogWarning(userIp, mac, computerName, path, warningMessage);
        }


        /// <summary>
        /// Fetches user location asynchronously.
        /// </summary>
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

        /// <summary>
        /// Handles the rate limit exceeded response.
        /// </summary>
        private void HandleRateLimitExceeded(HttpResponse response)
        {
            response.Clear();
            response.StatusCode = 429;
            response.Redirect("/Error/Show/429", false);
            response.End();
        }

        /// <summary>
        /// Extracts the real IP address.
        /// </summary>
        private string GetRealIpAddress(HttpRequest request)
        {
            string ip = request.Headers["X-Forwarded-For"];
            return !string.IsNullOrEmpty(ip) ? ip.Split(',')[0].Trim() : request.UserHostAddress;
        }

        public void Dispose() { }

        /// <summary>
        /// Request counter class.
        /// </summary>
        private class RequestCounter
        {
            public bool IsBlocked { get; set; } = false;
            public DateTime BlockEndTime { get; set; }
            public bool HasWarned { get; set; } = false;  // Tracks if warning has been issued
            public bool HasLoggedBlock { get; set; } = false;  // Tracks if block has been logged
            public List<DateTime> RequestTimestamps { get; } = new List<DateTime>();
        }

        /// <summary>
        /// Paths or extensions to exclude from logging
        /// </summary>
        private static readonly List<string> ExcludedPaths = new List<string>
        {
            "/favicon.ico",
            "/Content/",
            "/Scripts/",
            "/Images/",
            "/fonts/"
        };

        /// <summary>
        /// Extensions to exclude from logging
        /// </summary>
        private static readonly List<string> ExcludedExtensions = new List<string>
        {
            ".css",
            ".js",
            ".png",
            ".jpg",
            ".jpeg",
            ".gif",
            ".woff",
            ".woff2",
            ".ttf",
            ".ico",".json"
        };

        /// <summary>
        /// Logs a request asynchronously with path filtering.
        /// </summary>
        private async Task LogRequestAsync(string userIp, string mac, string computerName, string path)
        {
            // ✅ Apply path filtering
            if (IsExcludedPath(path))
            {
                System.Diagnostics.Debug.WriteLine($"Skipping logging for path: {path}");
                return;
            }

            string location = await FetchUserLocationAsync(userIp);
            _loggerService.LogRequest(userIp, mac, computerName, path, 200, location);
        }

        /// <summary>
        /// Checks if the request path is excluded from logging.
        /// </summary>
        private bool IsExcludedPath(string path)
        {
            // Check by path prefix
            foreach (var excludedPath in ExcludedPaths)
            {
                if (path.StartsWith(excludedPath, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            // Check by file extension
            string extension = System.IO.Path.GetExtension(path);
            if (ExcludedExtensions.Contains(extension.ToLower()))
            {
                return true;
            }

            return false;
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
//            private readonly RequestLoggerService _loggerService = new RequestLoggerService();

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
