using CBS.BusinessService.RequestLoggerServicesP;
using CBS.FrontDesk.Data.Entity.RequestManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using System.Web;

namespace CBS.FrontDesk.UI.WAF.Services.DDoS
{
    using System;
    using System.Runtime.Caching;
    using System.Threading.Tasks;

    /// <summary>
    /// <b>BlockCacheService</b> is a hybrid in-memory + database-based caching mechanism
    /// used by the WAF to determine if an IP or user is currently rate-limited or blocked.
    /// 
    /// It prioritizes fast lookups via <see cref="MemoryCache"/>, and falls back to the database
    /// using <see cref="RateLimitedUserService"/> if the IP is not already cached.
    /// </summary>
    public class BlockCacheService
    {
        private readonly MemoryCache _cache = MemoryCache.Default;
        private readonly int _maxCacheSize;
        private readonly RateLimitedUserService _rateLimitedUserService;
        private readonly int _fallbackCacheDurationMinutes = 15;

        /// <summary>
        /// Initializes the block cache service with an optional max cache size.
        /// </summary>
        /// <param name="maxCacheSize">Maximum number of IPs that can be cached before logging a warning</param>
        public BlockCacheService(int maxCacheSize = 10000)
        {
            _maxCacheSize = maxCacheSize;
            _rateLimitedUserService = new RateLimitedUserService();
        }

        /// <summary>
        /// Adds the given IP to the block cache with a specified duration.
        /// </summary>
        /// <param name="ip">The IP address to block</param>
        /// <param name="duration">The duration to block the IP</param>
        public void BlockIp(string ip, TimeSpan duration)
        {
            // 🧱 Log if cache size is at capacity
            if (_cache.GetCount() >= _maxCacheSize)
            {
                System.Diagnostics.Debug.WriteLine("⚠️ Block cache size limit reached.");
            }

            var cacheKey = GetCacheKey(ip);

            // 🕒 Setup caching policy with absolute expiration
            var policy = new CacheItemPolicy
            {
                AbsoluteExpiration = DateTimeOffset.UtcNow.Add(duration),
                SlidingExpiration = ObjectCache.NoSlidingExpiration
            };

            _cache.Set(cacheKey, true, policy); // ✅ Save in-memory
        }

        /// <summary>
        /// Checks whether an IP is blocked, prioritizing cache and falling back to DB.
        /// </summary>
        /// <param name="ip">IP address to check</param>
        /// <param name="username">Optional username if known</param>
        /// <returns>True if blocked; false otherwise</returns>
        public async Task<bool> IsBlockedAsync(string ip, string username = null)
        {
            var cacheKey = GetCacheKey(ip); // or use both ip+username if needed

            // ✅ If cached and not expired
            if (_cache.Contains(cacheKey))
            {
                var cached = _cache.Get(cacheKey);
                if (cached is DateTime expiresAt && expiresAt > DateTime.UtcNow)
                    return true;
            }

            // ✅ Check DB only if not in cache
            var isBlockedFromDb = await _rateLimitedUserService.CheckIsBlocked(new CheckRateLimitBlockQuery
            {
                IpOrUser = ip,
                Username = username
            });

            if (isBlockedFromDb)
            {
                _cache.Set(cacheKey, DateTime.UtcNow.AddMinutes(1), new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(1)
                });
            }

            return isBlockedFromDb;
        }


        /// <summary>
        /// Removes a blocked IP from the in-memory cache.
        /// </summary>
        /// <param name="ip">The IP to unblock</param>
        public void UnblockIp(string ip)
        {
            _cache.Remove(GetCacheKey(ip));
        }

        /// <summary>
        /// Returns the current count of blocked entries in the cache.
        /// </summary>
        public int GetBlockedCount()
        {
            return (int)_cache.GetCount();
        }
        public void ReleaseFromBlockCache(string ip)
        {
            var cacheKey = GetCacheKey(ip);
            _cache.Remove(cacheKey); // 🔁 Evicts from memory
        }

        /// <summary>
        /// Builds the unique cache key for a given IP.
        /// </summary>
        private string GetCacheKey(string ip)
        {
            return $"Blocked:{ip}";
        }
    }


}