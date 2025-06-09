using CBS.FrontDesk.Data.Entity.RequestManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace CBS.FrontDesk.UI.WAF.Services.DDoS
{
    /// <summary>
    /// Static configuration holder that caches and auto-refreshes the WAF's rate-limiting configuration
    /// from the backing service (<see cref="RateLimitConfigService"/>).
    /// 
    /// It supports in-memory caching with a time-based refresh mechanism to minimize redundant
    /// database or API calls.
    /// </summary>
    public static class RateLimitConfigHolder
    {
        // 🧠 Cached instance of the current config
        private static RateLimitConfig _config;

        // 🕒 Last time the config was refreshed
        private static DateTime _lastFetched = DateTime.MinValue;

        // ⏳ Refresh interval threshold (every 10 minutes)
        private static readonly TimeSpan RefreshInterval = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Retrieves the cached rate-limiting configuration, refreshing it from the source if expired.
        /// </summary>
        /// <returns>Most recent <see cref="RateLimitConfig"/> object</returns>
        public static RateLimitConfig Get()
        {
            // ⏱️ Only refresh if config is null or older than the threshold
            if (_config == null || DateTime.UtcNow - _lastFetched > RefreshInterval)
            {
                // ⚠️ Blocking Task.Run used to synchronously get the result
                _config = Task.Run(() => new RateLimitConfigService().GetAllAsync()).Result?.FirstOrDefault();

                // 🕓 Track the refresh time
                _lastFetched = DateTime.UtcNow;

                System.Diagnostics.Debug.WriteLine($"🔁 WAF config auto-refreshed at {_lastFetched:HH:mm:ss}");
            }

            return _config;
        }
    }


}