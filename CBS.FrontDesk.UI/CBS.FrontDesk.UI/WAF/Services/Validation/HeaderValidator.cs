using CBS.FrontDesk.Data.Entity.RequestManagement;
using CBS.FrontDesk.UI.WAF.Middleware.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace CBS.FrontDesk.UI.WAF.Services.Validation
{

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// The <c>HeaderValidator</c> class is part of the Web Application Firewall (WAF) layer.
    /// It analyzes HTTP request headers and detects suspicious or malicious patterns
    /// such as spoofed origins, missing user agents, telemetry/bot tracking, or risky content.
    /// 
    /// It uses the configuration defined in <see cref="RateLimitConfig"/> to apply custom rules,
    /// helping to mitigate attacks like:
    /// - Bot scanning and fingerprinting
    /// - Header spoofing
    /// - API abuse
    /// - Cross-site forgery
    /// - Injection attempts in header values
    /// </summary>
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// The <c>HeaderValidator</c> class is responsible for inspecting HTTP request headers to detect
    /// suspicious, malformed, or potentially malicious content. It uses configuration settings (from <see cref="RateLimitConfig"/>)
    /// to enforce both a whitelist (allowed header value patterns) and a blacklist (suspicious header value patterns).
    /// In addition, it validates other header-related aspects such as Origin, Referer, and telemetry headers.
    /// </summary>
    public class HeaderValidator
    {
        private readonly RateLimitConfig _config;

        /// <summary>
        /// Initializes the validator with WAF configuration rules.
        /// </summary>
        /// <param name="config">WAF rules defined via the <see cref="RateLimitConfig"/> object.</param>
        public HeaderValidator(RateLimitConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Main entry point for header inspection. Evaluates incoming request headers and flags suspicious patterns.
        /// </summary>
        /// <param name="context">The HTTP request context wrapped in a <see cref="WAFContext"/> object.</param>
        /// <param name="reason">If a suspicious header is detected, the reason describing the issue is returned here.</param>
        /// <returns>True if the headers are suspicious or malicious; otherwise, false.</returns>
        public bool IsSuspiciousHeaders(WAFContext context, out string reason)
        {
            reason = null;

            // Extract and normalize key header values.
            string userAgent = context.UserAgent?.ToLowerInvariant() ?? "";
            string origin = context.Origin ?? "";
            string referer = context.Referer ?? "";
            string method = context.Method?.ToUpperInvariant() ?? "";
            string path = context.Path?.ToLowerInvariant() ?? "";
            string host = context.Host?.ToLowerInvariant() ?? "";

            // Build a dictionary of all headers for case-insensitive access.
            var headers = context.Headers.AllKeys
                .Where(k => !string.IsNullOrWhiteSpace(k))
                .ToDictionary(k => k.ToLowerInvariant(), k => context.Headers[k]);

            // === [1] User-Agent Validation ===
            if (string.IsNullOrWhiteSpace(userAgent))
            {
                reason = "🚨 Missing User-Agent header";
                return true;
            }

            // Check if User-Agent contains any blacklisted substring.
            var badUserAgent = _config.BadUserAgents
                .FirstOrDefault(b => userAgent.Contains(b.ToLowerInvariant()));
            if (!string.IsNullOrEmpty(badUserAgent))
            {
                reason = $"🚨 Suspicious User-Agent detected: {userAgent} (matched '{badUserAgent}')";
                return true;
            }

            // === [2] Origin Validation for State-changing Methods ===
            if ((method == "POST" || method == "PUT") && !string.IsNullOrWhiteSpace(origin))
            {
                bool isOriginAllowed = _config.AllowedOrigins.Any(o =>
                    origin.TrimEnd('/').StartsWith(o.TrimEnd('/'), StringComparison.OrdinalIgnoreCase));
                if (!isOriginAllowed)
                {
                    reason = $"🚨 Untrusted or spoofed Origin: {origin}";
                    return true;
                }
            }

            // === [3] Referer Requirement on Protected Paths ===
            if (_config.ProtectedPathRefererRequired.Any(p => path.Contains(p.ToLowerInvariant())))
            {
                bool validReferer = !string.IsNullOrWhiteSpace(referer) &&
                    (referer.ToLowerInvariant().Contains(host) ||
                     (!string.IsNullOrEmpty(origin) && referer.ToLowerInvariant().Contains(origin.ToLowerInvariant())));

                if (!validReferer)
                {
                    reason = "🚨 Missing or mismatched Referer for protected path";
                    return true;
                }
            }

            // === [4] Spoofed X-Forwarded-For Detection ===
            if (headers.TryGetValue("x-forwarded-for", out var xff) &&
                _config.SpoofedForwardedForIndicators.Any(ind =>
                    xff?.IndexOf(ind, StringComparison.OrdinalIgnoreCase) >= 0))
            {
                reason = $"🚨 Spoofed X-Forwarded-For header: {xff}";
                return true;
            }

            // === [5] Origin/Referer Requirement on Sensitive API Endpoints ===
            if (_config.SensitiveHeaderMissingCheckPaths.Any(p => path.Contains(p.ToLowerInvariant())))
            {
                if (!headers.ContainsKey("origin") && !headers.ContainsKey("referer"))
                {
                    reason = "🚨 API or token route missing both Origin and Referer headers";
                    return true;
                }
            }

            // === [6] Telemetry/Bot Tracking Headers Check ===
            foreach (var headerKey in headers.Keys)
            {
                // Skip internal headers like correlation IDs.
                if (headerKey.Equals("x-correlation-id", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (_config.TelemetryHeaderPrefixes.Any(prefix =>
                    headerKey.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
                {
                    reason = $"🚨 Telemetry or bot header detected: {headerKey}";
                    return true;
                }
            }

            // === [7] High-risk Header Keys and Value Safety Check ===
            foreach (var headerKey in headers.Keys)
            {
                var lowerKey = headerKey.ToLowerInvariant();
                var value = headers[headerKey];

                // Check if header key is known to be dangerous.
                if (_config.HighRiskHeaderKeys.Any(risk => lowerKey.Contains(risk.ToLowerInvariant())))
                {
                    reason = $"🚨 Exploit-style header key found: {headerKey}";
                    return true;
                }

                // Use the helper to verify that the header value does not contain any suspicious patterns.
                // Here we assume that _config.SuspiciousHeaderValuePatterns is a List<string> containing disallowed regexes.
                bool isStandardHeader = headerKey == "accept" || headerKey == "content-type";
                if (!isStandardHeader && PatternValidator.ContainsSuspiciousPattern(value, _config.SuspiciousFormKeyPatterns, out var matchedSuspicious))
                {
                    reason = $"🚨Suspicious value in custom header: {headerKey} = {value} (Matched pattern: {matchedSuspicious})";
                    return true;
                }


                // Additionally, require that the header value matches at least one allowed pattern.
                // If no allowed pattern is matched, consider it unsafe.
                bool matchesAllowed = _config.AllowedHeaderValuePattern.Any(pattern => Regex.IsMatch(value, pattern));
                if (!matchesAllowed)
                {
                    reason = $"🚨 Header value does not match allowed format: {headerKey} = {value}";
                    return true;
                }
            }

            // ✅ All checks passed; header set is considered safe.
            return false;
        }
    }


}