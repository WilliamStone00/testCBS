using CBS.BusinessService.RequestLoggerServicesP;
using CBS.FrontDesk.UI.Utility;
using CBS.FrontDesk.UI.Utility.Middlware_logger;
using CBS.FrontDesk.UI.WAF.Middleware.Core;
using CBS.FrontDesk.UI.WAF.Services.DDoS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;

namespace CBS.FrontDesk.UI.WAF.Services.Validation
{
    /// <summary>
    /// Handles logging and enforcement for suspicious requests.
    /// This includes: logging malicious activity, blocking IPs or users,
    /// and caching temporary blocks to optimize request rejection.
    /// </summary>
    public class SuspiciousEventHandler
    {
        private readonly RateLimiteTrackerLoggerServices _loggerService;
        private readonly RateLimitedUserService _rateLimitedUserService;
        private readonly BlockCacheService _blockCache;

        /// <summary>
        /// Constructs the event handler with logging and blocking services.
        /// </summary>
        public SuspiciousEventHandler(
            RateLimiteTrackerLoggerServices loggerService,
            RateLimitedUserService rateLimitedUserService,
            BlockCacheService blockCache)
        {
            _loggerService = loggerService;
            _rateLimitedUserService = rateLimitedUserService;
            _blockCache = blockCache;
        }

        /// <summary>
        /// Logs a non-blocked request that passed WAF inspection.
        /// Helps in traffic analysis and request traceability.
        /// </summary>
        public async Task LogAllowedRequest(WAFContext ctx)
        {
            try
            {
                await _loggerService.LogRequest(
                    ip: ctx.Ip,
                    userName: ctx.Username,
                    computerName: ctx.Headers?["Computer-Name"] ?? "n/a",
                    path: ctx.Path,
                    statusCode: 200,
                    location: ctx.Location,
                    lat: ctx.Latitude,
                    lon: ctx.Longitude,
                    city: ctx.City,
                    region: ctx.Region,
                    country: ctx.Country,
                    branchid: ctx.BranchId,
                    branchcode: ctx.BranchCode,
                    branchname: ctx.BranchName,
                    tel: ctx.PhoneNumber,
                    fullname: ctx.FullName,
                    isblocked: false,
                    reason: ctx.IsAuthenticated
                        ? "✅ Request allowed (Authenticated User)"
                        : "✅ Request allowed (Redirect to login)",
                    isAuthenticated: ctx.IsAuthenticated,
                    fullUrl: ctx.FullUrl,
                    RequestBody: ctx.RequestBody,
                    ActionMethod: ctx.ActionMethod,
                    isAjax: ctx.IsAjax,
                    correlationId: ctx.CorrelationId,
                    responseBody: ctx.ResponseBody,
                    blockType: "n/a",
                    RawHeaders: ctx.RawHeaders
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Failed to log allowed request: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles a detected suspicious event by:
        /// 1. Logging the reason
        /// 2. Blocking the user/IP temporarily
        /// 3. Recording the activity in logs
        /// </summary>
        /// <param name="tag">Label describing the threat (e.g. RateLimit, GeoIP)</param>
        /// <param name="reason">Detailed explanation</param>
        /// <param name="ctx">WAF execution context</param>
        /// <param name="fullUrl">Requested full URL</param>
        /// <param name="blacklistingType">Category (e.g. "Suspicious-Attack")</param>
        public async Task Handle(string tag, string reason, WAFContext ctx, string fullUrl, string blacklistingType = "Suspicious-Attack")
        {
            string fullReason = $"🚨 {tag}: {reason} | Path: {ctx.Path}";
            string computerName = ctx.Headers?["Computer-Name"] ?? "Unknown Computer";

            System.Diagnostics.Debug.WriteLine(fullReason);
            AdvancedMiddlewareLogger.Log(fullReason, LogLevel.WARN, ctx.FullName, ctx.BranchName);

            try
            {
                // 1. Block at DB level
                await _rateLimitedUserService.BlockUser(
                    ctx.Ip, ctx.Username, blacklistingType,
                    TimeSpan.FromMinutes(ctx.limitConfig.BlockDurationMinutes),
                    fullReason,
                    ctx.Location, ctx.Latitude, ctx.Longitude,
                    ctx.City, ctx.Region, ctx.Country,
                    ctx.BranchId, ctx.BranchCode, ctx.BranchName,
                    ctx.PhoneNumber, ctx.FullName
                );

                // 2. Log the blocked request
                await _loggerService.LogRequest(
                    ctx.Ip, ctx.Username, computerName, ctx.Path, 403,
                    ctx.Location, ctx.Latitude, ctx.Longitude, ctx.City, ctx.Region, ctx.Country,
                    ctx.BranchId, ctx.BranchCode, ctx.BranchName, ctx.PhoneNumber, ctx.FullName,
                    true, fullReason, ctx.IsAuthenticated, fullUrl,
                    RequestBody: ctx.RequestBody,
                    ActionMethod: ctx.ActionMethod,
                    isAjax: ctx.IsAjax,
                    correlationId: ctx.CorrelationId,
                    responseBody: ctx.ResponseBody,
                    blacklistingType,
                    RawHeaders: ctx.RawHeaders
                );
            }
            catch (Exception ex)
            {
                AdvancedMiddlewareLogger.Log($"🔥 WAF async handler failed: {ex.Message}", LogLevel.ERROR, ctx.FullName, ctx.BranchName);
            }

            // 3. Add to in-memory block cache if applicable
            if (ShouldCacheBlock(ctx))
            {
                _blockCache.BlockIp(ctx.Ip, TimeSpan.FromMinutes(10));
            }
        }

        /// <summary>
        /// Determines whether to add IP to the in-memory block list.
        /// </summary>
        private bool ShouldCacheBlock(WAFContext ctx)
        {
            // 🛡️ Skip cache block for authenticated users
            if (ctx.IsAuthenticated)
                return false;

            // 🛡️ Allow whitelisted countries
            if (ctx.limitConfig.WhitelistedCountriesCode.Contains(ctx.Country, StringComparer.OrdinalIgnoreCase))
                return false;

            // 🛡️ Allow IPs within Cameroon CIDRs
            if (CidrUtility.IsIpInCidr(ctx.Ip, ctx.limitConfig.WhitelistedCidrs))
                return false;

            // ❌ Otherwise, add to in-memory block list
            return true;
        }
    }

}