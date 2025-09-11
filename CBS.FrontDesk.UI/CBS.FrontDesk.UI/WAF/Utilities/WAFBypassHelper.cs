using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace CBS.FrontDesk.UI.WAF.Utility
{
    /// <summary>
    /// <b>WAFBypassHelper</b> provides a utility to determine whether the Web Application Firewall (WAF)
    /// should skip analysis for a given IP address based on the current environment and a configured developer whitelist.
    /// 
    /// Typically used to bypass rate-limiting, header scanning, and other filters for trusted developer IPs during development.
    /// </summary>
    public static class WAFBypassHelper
    {
        /// <summary>
        /// The current environment (e.g., Development, TestBed, Production), loaded from config.
        /// Defaults to "Production" if not defined.
        /// </summary>
        private static readonly string Environment =
            ConfigurationManager.AppSettings["URLConf_Environment"]?.Trim() ?? "Production";

        /// <summary>
        /// Set of developer or trusted IPs loaded from AppSettings["DevWhitelistIPs"].
        /// IPv6 loopback (::1) is automatically added.
        /// </summary>
        private static readonly HashSet<string> DevIps = (ConfigurationManager.AppSettings["DevWhitelistIPs"] ?? "")
            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(ip => ip.Trim())
            .Append("::1")  // ✅ Always allow local IPv6 loopback for dev
            .ToHashSet(StringComparer.OrdinalIgnoreCase); // Ensure IP comparison is case-insensitive

        /// <summary>
        /// Determines whether WAF should be bypassed for a given IP.
        /// Returns true only if the environment is "Development" and the IP is in the whitelist.
        /// </summary>
        /// <param name="ip">Client IP address</param>
        /// <returns>True if WAF checks should be skipped; otherwise, false.</returns>
        public static bool ShouldBypass(string ip)
        {
            return Environment.Equals("Development", StringComparison.OrdinalIgnoreCase)
                && DevIps.Contains(ip);
        }
    }


}