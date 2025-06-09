using CBS.BusinessService.RequestLoggerServicesP;
using CBS.FrontDesk.Data.Entity.RequestManagement;
using CBS.FrontDesk.UI.Utility.Middlware_logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using System.Web;

namespace CBS.FrontDesk.UI.WAF.Services.Validation
{
    /// <summary>
    /// Handles validation and inspection of request paths for potential threats,
    /// including static assets and dynamic endpoints. Caches results for efficiency,
    /// leverages centralized suspicious keyword logic, and allows configurable path exclusions.
    /// </summary>
    public class PathValidator
    {
        private readonly SuspiciousPathService _suspiciousPathService;
        private readonly MemoryCache _cache = MemoryCache.Default;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(10);
        private readonly RateLimitConfig _config;

        /// <summary>
        /// Initializes the validator with external suspicious path logic and WAF configuration.
        /// </summary>
        public PathValidator(SuspiciousPathService suspiciousPathService, RateLimitConfig config)
        {
            _suspiciousPathService = suspiciousPathService;
            _config = config;
        }

        /// <summary>
        /// Checks if a path is flagged as suspicious (e.g. /wp-admin, /phpmyadmin).
        /// Results are cached to reduce backend lookups.
        /// </summary>
        public bool IsSuspiciousPath(string path)
        {
            string key = $"SuspiciousPath:{path.ToLowerInvariant()}";

            // ✅ Return cached result if available
            if (_cache.Contains(key))
                return (bool)_cache.Get(key);

            // ⏳ Call backend service (synchronously here, but could be async-capable if architecture allows)
            bool result = Task.Run(() => _suspiciousPathService.CheckIfSuspiciousPath(path))
                              .GetAwaiter().GetResult();

            // 🧠 Cache result for reuse
            _cache.Set(key, result, DateTimeOffset.Now.Add(_cacheDuration));
            return result;
        }

        /// <summary>
        /// Determines if a static asset (JS, CSS, etc.) should bypass WAF checks.
        /// Applies both whitelist (safe paths/extensions) and blacklist (suspicious keywords).
        /// </summary>
        public bool IsSafeStaticAsset(string path)
        {
            bool isExcluded = IsExcludedStaticAssetPath(path);
            bool isSuspicious = HasSuspiciousKeyword(path);

            if (isSuspicious)
            {
                AdvancedMiddlewareLogger.Log($"🚨 Suspicious keyword in static asset path: {path}", LogLevel.WARN);
            }
            else if (isExcluded)
            {
                AdvancedMiddlewareLogger.Log($"✅ Safe static asset path excluded from WAF: {path}", LogLevel.INFO);
            }

            return isExcluded && !isSuspicious;
        }

        /// <summary>
        /// Checks if a static asset path should be excluded based on configured extensions or folder prefixes.
        /// E.g., skips `.css`, `.js`, `/fonts/`, etc.
        /// </summary>
        public bool IsExcludedStaticAssetPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            var lower = path.ToLowerInvariant();

            // ✅ Match against configured safe extensions or folders
            bool isExcluded = (_config.ExcludedExtensions?.Any(ext => lower.EndsWith(ext)) == true) ||
                              (_config.ExcludedPaths?.Any(p => lower.StartsWith(p)) == true);

            return isExcluded;
        }

        /// <summary>
        /// Checks whether a path contains any known suspicious keywords (e.g. "phpmyadmin", ".env").
        /// </summary>
        public bool HasSuspiciousKeyword(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            var lower = path.ToLowerInvariant();

            var suspiciousIndicators = _config.SuspiciousIndicators;
            return suspiciousIndicators.Any(ind => lower.Contains(ind));
        }
    }


}