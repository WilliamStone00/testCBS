using CBS.BusinessService.RequestLoggerServicesP;
using CBS.FrontDesk.Data.Entity.RequestManagement;
using CBS.FrontDesk.UI.SC.Security.WAF.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using static RateLimitConfigService;

namespace CBS.FrontDesk.UI.SC.Security.WAF.Services
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    namespace TSC.Security.WAF.Services
    {
        public class RateLimitEvaluator : IRateLimitEvaluator
        {
            private static readonly ConcurrentDictionary<string, List<DateTime>> RequestLog = new ConcurrentDictionary<string, List<DateTime>>();

            private readonly RateLimitedUserService _rateLimitedUserService;
            private readonly RateLimitConfigService _configService;

            public RateLimitEvaluator(RateLimitedUserService rateLimitedUserService, RateLimitConfigService configService)
            {
                _rateLimitedUserService = rateLimitedUserService;
                _configService = configService;
            }

            /// <summary>
            /// Determines whether the request count from the specified IP or user exceeds the allowed threshold.
            /// If exceeded, block logic is applied and persisted.
            /// </summary>
            public async Task<bool> ShouldBlockAsync(
                string ipOrUser,
                string username,
                string branchId,
                string branchCode,
                string branchName,
                string tel,
                string fullName,
                string location = "",
                string lat = "",
                string lon = "",
                string city = "",
                string region = "",
                string country = "")
            {
                var config = RateLimitConfigHolder.Config;
                if (config == null)
                {
                    await RateLimitConfigHolder.LoadAsync(_configService);
                    config = RateLimitConfigHolder.Config;
                }

                int maxRequests = config.RequestLimit;
                TimeSpan timeWindow = TimeSpan.FromSeconds(config.TimeWindowSeconds);
                TimeSpan blockDuration = TimeSpan.FromMinutes(config.BlockDurationMinutes);

                var now = DateTime.UtcNow;
                var log = RequestLog.GetOrAdd(ipOrUser, _ => new List<DateTime>());

                lock (log)
                {
                    log.RemoveAll(t => t < now - timeWindow);
                    log.Add(now);
                }

                if (log.Count > maxRequests)
                {
                    bool alreadyBlocked = await _rateLimitedUserService.CheckIsBlocked(new CheckRateLimitBlockQuery
                    {
                        IpOrUser = ipOrUser,
                        Username = username
                    });

                    if (!alreadyBlocked)
                    {
                        await _rateLimitedUserService.BlockUser(
                            ipOrUser,
                            username,
                            "IP-Based",
                            blockDuration,
                            "Exceeded rate limit",
                            location,
                            lat, lon,
                            city, region, country,
                            branchId, branchCode, branchName,
                            tel, fullName
                        );
                    }

                    return true;
                }

                return false;
            }

            /// <summary>
            /// Returns the count of recent requests within the configured time window.
            /// </summary>
            public int GetRecentRequestCount(string ipOrUser)
            {
                var config = RateLimitConfigHolder.Config ?? new RateLimitConfig
                {
                    RequestLimit = 50,
                    TimeWindowSeconds = 20,
                    BlockDurationMinutes = 30
                };

                var now = DateTime.UtcNow;
                var timeWindow = TimeSpan.FromSeconds(config.TimeWindowSeconds);

                if (RequestLog.TryGetValue(ipOrUser, out var log))
                {
                    lock (log)
                    {
                        log.RemoveAll(t => t < now - timeWindow);
                        return log.Count;
                    }
                }

                return 0;
            }
        }
    }


}