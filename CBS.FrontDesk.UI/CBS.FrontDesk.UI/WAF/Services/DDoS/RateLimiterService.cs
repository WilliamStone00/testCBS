using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CBS.FrontDesk.UI.WAF.Services.DDoS
{
    /// <summary>
    /// Provides in-memory IP-based rate limiting to protect against abuse such as
    /// brute-force login attempts or denial-of-service attacks.
    /// 
    /// Tracks recent request timestamps per IP and blocks access if the request
    /// threshold is exceeded within a configured time window.
    /// </summary>
    public class RateLimiterService
    {
        // ✅ Maximum number of requests allowed in the time window
        private readonly int _requestLimit;

        // ✅ The time window within which requests are counted
        private readonly TimeSpan _timeWindow;

        // ✅ Stores request timestamps per IP address
        private static readonly ConcurrentDictionary<string, List<DateTime>> RequestTimestamps
            = new ConcurrentDictionary<string, List<DateTime>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="RateLimiterService"/> class.
        /// </summary>
        /// <param name="requestLimit">Maximum allowed requests per IP in the window.</param>
        /// <param name="timeWindow">Time window during which requests are tracked.</param>
        public RateLimiterService(int requestLimit, TimeSpan timeWindow)
        {
            _requestLimit = requestLimit;
            _timeWindow = timeWindow;
        }

        /// <summary>
        /// Checks if the given IP address has exceeded the configured rate limit.
        /// </summary>
        /// <param name="ip">Client IP address.</param>
        /// <returns>True if the rate limit is exceeded; otherwise, false.</returns>
        public bool IsRateLimitExceeded(string ip)
        {
            var now = DateTime.UtcNow;

            // ✅ Get or create the timestamp list for this IP
            var timestamps = RequestTimestamps.GetOrAdd(ip, _ => new List<DateTime>());

            lock (timestamps)
            {
                // ⏱ Add the current request timestamp
                timestamps.Add(now);

                // 🧹 Remove old requests outside the window
                timestamps.RemoveAll(t => t < now - _timeWindow);

                // 🚫 Block if the number of requests exceeds the limit
                return timestamps.Count > _requestLimit;
            }
        }

        /// <summary>
        /// Gets the current number of active requests from the given IP within the time window.
        /// </summary>
        /// <param name="ip">Client IP address.</param>
        /// <returns>Count of recent requests.</returns>
        public int GetCurrentRequestCount(string ip)
        {
            if (RequestTimestamps.TryGetValue(ip, out var timestamps))
            {
                var now = DateTime.UtcNow;

                lock (timestamps)
                {
                    // 🧹 Prune outdated timestamps
                    timestamps.RemoveAll(t => t < now - _timeWindow);

                    return timestamps.Count;
                }
            }

            return 0;
        }
    }


}