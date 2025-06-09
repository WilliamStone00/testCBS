using CBS.FrontDesk.Data.Entity.RequestManagement;
using CBS.FrontDesk.UI.Utility.Middlware_logger;
using CBS.FrontDesk.UI.WAF.Middleware.Helpers;
using CBS.FrontDesk.UI.WAF.Services.DDoS;
using CBS.FrontDesk.UI.WAF.Services.GeoIP;
using CBS.FrontDesk.UI.WAF.Services.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;

namespace CBS.FrontDesk.UI.WAF.Middleware.Core
{
    /// <summary>
    /// The <c>WAFRequestAnalyzer</c> class is the core decision engine of the Web Application Firewall (WAF).
    /// It evaluates each incoming HTTP request based on multiple layered rules:
    ///     1. IP or username blacklist
    ///     2. Static asset probing
    ///     3. Geolocation and CIDR restrictions
    ///     4. Header validation
    ///     5. Path-based attack patterns
    ///     6. Payload/body scan (form data, file uploads)
    ///     7. Rate-limiting (anti-DDoS)
    ///     8. Authorization enforcement for protected routes
    /// 
    /// It returns whether the request should be blocked, and provides the reason for that decision.
    /// </summary>
    public class WAFRequestAnalyzer
    {
        private readonly HeaderValidator _headerValidator;
        private readonly PathValidator _pathValidator;
        private readonly BodyScanner _bodyScanner;
        private readonly RateLimiterService _rateLimiter;
        private readonly BlockCacheService _blockCache;
        private readonly GeoIpResolver _geoResolver;
        private readonly SuspiciousEventHandler _suspiciousHandler;
        private readonly RateLimitConfig _config;

        /// <summary>
        /// Constructs the analyzer with injected WAF services.
        /// </summary>
        public WAFRequestAnalyzer(
            HeaderValidator headerValidator,
            PathValidator pathValidator,
            BodyScanner bodyScanner,
            RateLimiterService rateLimiter,
            BlockCacheService blockCache,
            GeoIpResolver geoResolver,
            SuspiciousEventHandler suspiciousHandler,
            RateLimitConfig config)
        {
            _headerValidator = headerValidator;
            _pathValidator = pathValidator;
            _bodyScanner = bodyScanner;
            _rateLimiter = rateLimiter;
            _blockCache = blockCache;
            _geoResolver = geoResolver;
            _suspiciousHandler = suspiciousHandler;
            _config = config;
        }

        /// <summary>
        /// Analyzes the incoming HTTP request using layered WAF logic.
        /// If the request is flagged as malicious, it returns true with a reason.
        /// </summary>
        /// <param name="request">Raw HttpRequest object</param>
        /// <param name="ctx">The constructed WAFContext containing metadata</param>
        /// <param name="reason">OUT parameter with the reason for denial</param>
        /// <returns>True if the request should be blocked; otherwise, false</returns>
        public bool RequestAnalyzer(HttpRequest request, WAFContext ctx, out string reason)
        {
            var config = RateLimitConfigHolder.Get();
            reason = null;

            // 1️⃣ Check if IP or user is temporarily blocked
            if (Task.Run(() => _blockCache.IsBlockedAsync(ctx.Ip, ctx.Username)).GetAwaiter().GetResult())
            {
                var r = "🚫 IP or user is temporarily blocked";
                reason = r;
                return true;
            }

            // 2️⃣ Check for suspicious static path keywords (e.g., "/.env", "/admin.js")
            if (_pathValidator.HasSuspiciousKeyword(ctx.Path))
            {
                var r = $"🚨 Suspicious keyword detected in static path: {ctx.Path}";
                reason = r;
                Task.Run(() => _suspiciousHandler.Handle("Static-Path-Suspicious", r, ctx, ctx.FullUrl, "Static-Asset-Filter")).GetAwaiter().GetResult();
                return true;
            }

            // ✅ Whitelisted static resources (e.g., .css, .jpg)
            if (_pathValidator.IsExcludedStaticAssetPath(ctx.Path))
            {
                reason = "✅ Skipped WAF for safe static asset";
                return false;
            }

            // 3️⃣ Validate IP format and perform GeoIP country filtering
            if (!_geoResolver.IsValidIPv4(ctx.Ip))
            {
                var r = $"Blocked: Invalid IP format; IP: {ctx.Ip}";
                reason = r;
                Task.Run(() => _suspiciousHandler.Handle("GeoIP-Invalid-IP", r, ctx, ctx.FullUrl, "GeoIP-CIDR-Filter")).GetAwaiter().GetResult();
                return true;
            }

            if (!_geoResolver.IsWhitelistedCountry(ctx.Country, config.WhitelistedCountriesCode))
            {
                var r = $"Blocked: Non-whitelisted country ({ctx.Country})";
                reason = r;
                Task.Run(() => _suspiciousHandler.Handle("GeoIP-Country", r, ctx, ctx.FullUrl, "GeoIP-Country-Restriction")).GetAwaiter().GetResult();
                return true;
            }

            if (!_geoResolver.IsIpInCidr(ctx.Ip, config.WhitelistedCidrs))
            {
                var r = $"Blocked: IP not in whitelisted CIDR range: {ctx.Ip}";
                reason = r;
                Task.Run(() => _suspiciousHandler.Handle("GeoIP-CIDR", r, ctx, ctx.FullUrl, "GeoIP-CIDR-Restriction")).GetAwaiter().GetResult();
                return true;
            }

            // 4️⃣ Validate request headers (User-Agent, Referer, Origin, etc.)
            if (_headerValidator.IsSuspiciousHeaders(ctx, out string headerReason))
            {
                var r = headerReason;
                reason = r;
                Task.Run(() => _suspiciousHandler.Handle("Header-Validation", r, ctx, ctx.FullUrl, "Header-Filter")).GetAwaiter().GetResult();
                return true;
            }

            // 5️⃣ Block based on known suspicious paths (e.g. "/phpmyadmin", "/admin/delete")
            if (_pathValidator.IsSuspiciousPath(ctx.Path))
            {
                var r = $"Blocked: Suspicious path match ({ctx.Path})";
                reason = r;
                Task.Run(() => _suspiciousHandler.Handle("Path-Pattern", r, ctx, ctx.FullUrl, "Suspicious-Attack")).GetAwaiter().GetResult();
                return true;
            }

            // 6️⃣ Scan request body and multipart form for malicious content or patterns
            string fileReason = null;
            if (_bodyScanner.IsMaliciousBody(request, ctx, out var bodyReason) || _bodyScanner.IsMaliciousMultipart(request, ctx, out fileReason))
            {
                var r = bodyReason ?? fileReason;
                reason = r;
                Task.Run(() => _suspiciousHandler.Handle("Payload-Scan", r, ctx, ctx.FullUrl, "Request-Body-Filter")).GetAwaiter().GetResult();
                return true;
            }

            // 7️⃣ Enforce rate limits for IPs (anti-DDoS)
            if (_rateLimiter.IsRateLimitExceeded(ctx.Ip))
            {
                var r = $"Exceeded {config.RequestLimit} requests in {config.TimeWindowSeconds}s";
                reason = $"Blocked: DDoS rate limit exceeded";
                Task.Run(() => _suspiciousHandler.Handle("RateLimit", r, ctx, ctx.FullUrl, "IP-Based")).GetAwaiter().GetResult();
                return true;
            }

            // 8️⃣ Enforce login on protected/admin endpoints
            if (!ctx.IsAuthenticated && ctx.Path.StartsWith("/admin", StringComparison.OrdinalIgnoreCase))
            {
                var r = "🔒 Authentication required. Redirect to login page.";
                reason = r;
                Task.Run(() => _suspiciousHandler.Handle("Auth-Required", r, ctx, ctx.FullUrl, "Protected-Path")).GetAwaiter().GetResult();
                return true;
            }

            // ✅ 9️⃣ If no threat found, log as allowed request
            _ = Task.Run(() => _suspiciousHandler.LogAllowedRequest(ctx).GetAwaiter().GetResult());

            // Optional message for unauthenticated but allowed request
            if (!ctx.IsAuthenticated)
                reason = "✅ Request allowed but unauthenticated. Redirect to login.";

            return false;
        }
    }



}