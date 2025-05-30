using CBS.FrontDesk.UI.SC.Security.WAF.Models;
using CBS.FrontDesk.UI.Utility.Middlware_logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CBS.FrontDesk.UI.SC.Security.WAF.Services
{
    public class HeaderValidator : IHeaderValidator
    {
        private static readonly List<string> BadUserAgents = new List<string>
        {
            "curl", "wget", "python", "httpclient", "sqlmap", "nikto", "fuzzer",
            "libwww", "java", "powershell", "nmap", "masscan", "go-http-client",
            "scan", "scrapy", "nessus"
        };

        private static readonly List<string> BlockedReferers = new List<string>
        {
            "evil.com", "scan", "burp", "localhost", "ngrok.io"
        };

        private static readonly List<string> AllowedDomains = new List<string>
        {
            "bapcculcbs.com", "camccul-bapccul.fluxtbcredit.com", "localhost"
        };

        /// <summary>
        /// Validates the request headers for suspicious patterns.
        /// </summary>
        /// <param name="request">The HTTP request to inspect.</param>
        /// <returns>A result indicating if the request is suspicious and why.</returns>
        public HeaderValidationResult Validate(HttpRequest request)
        {
            var result = new HeaderValidationResult();

            // 1️⃣ User-Agent Checks
            var userAgent = request.UserAgent?.ToLowerInvariant() ?? "";
            if (string.IsNullOrWhiteSpace(userAgent))
            {
                result.IsSuspicious = true;
                result.Reason = "Missing User-Agent header";
                return result;
            }

            if (BadUserAgents.Any(ua => userAgent.Contains(ua)))
            {
                result.IsSuspicious = true;
                result.Reason = $"Blocked User-Agent: {userAgent}";
                return result;
            }

            // 2️⃣ Referer Checks
            var referer = request.UrlReferrer?.Host?.ToLowerInvariant();
            if (!string.IsNullOrEmpty(referer))
            {
                if (BlockedReferers.Any(bad => referer.Contains(bad)))
                {
                    result.IsSuspicious = true;
                    result.Reason = $"Blocked Referer: {referer}";
                    return result;
                }

                if (!AllowedDomains.Any(allow => referer.Contains(allow)))
                {
                    result.IsSuspicious = true;
                    result.Reason = $"Foreign Referer not allowed: {referer}";
                    return result;
                }
            }

            // 3️⃣ Origin Checks
            var origin = request.Headers["Origin"];
            if (!string.IsNullOrWhiteSpace(origin) &&
                !AllowedDomains.Any(allow => origin.Contains(allow)))
            {
                result.IsSuspicious = true;
                result.Reason = $"Foreign Origin not allowed: {origin}";
                return result;
            }

            // 4️⃣ X-Forwarded-For Spoofing Check
            var forwarded = request.Headers["X-Forwarded-For"];
            if (!string.IsNullOrEmpty(forwarded) && forwarded.Contains("127.0.0.1"))
            {
                result.IsSuspicious = true;
                result.Reason = "Spoofed X-Forwarded-For header";
                return result;
            }

            // ✅ Passed all checks
            result.IsSuspicious = false;
            result.Reason = string.Empty;
            return result;
        }
    }


}