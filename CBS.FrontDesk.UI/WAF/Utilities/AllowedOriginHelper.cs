using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace CBS.FrontDesk.UI.Utility
{
    /// <summary>
    /// <b>AllowedOriginHelper</b> provides a thread-safe utility to retrieve a list of 
    /// allowed CORS origins based on the configured environment (Development, TestBed, Production).
    /// 
    /// It reads from AppSettings and caches the result for performance. Only the origin that 
    /// corresponds to the current environment index is included.
    /// </summary>
    public static class AllowedOriginHelper
    {
        private static readonly object _lock = new object();       // Ensures thread-safety for cache access
        private static List<string> _cachedOrigins;                // Stores processed origins for the current app lifecycle

        /// <summary>
        /// Retrieves and caches allowed origins for the current environment.
        /// The method reads 'AllowedOrigins' from config and returns the relevant origin(s)
        /// based on the index that matches the environment name in 'URLConf_Environment'.
        /// </summary>
        /// <returns>A distinct, lowercased list of allowed origin hostnames.</returns>
        public static List<string> GetAllowedOrigins()
        {
            // ✅ Return cached if already loaded
            if (_cachedOrigins != null)
                return _cachedOrigins;

            // 🔒 Thread-safe cache initialization
            lock (_lock)
            {
                if (_cachedOrigins != null)
                    return _cachedOrigins;

                // 🌍 Determine environment: Development | TestBed | Production
                string environment = ConfigurationManager.AppSettings["URLConf_Environment"]?.Trim() ?? "Production";

                // 🔧 Read comma-separated origins from config
                string originsRaw = ConfigurationManager.AppSettings["AllowedOrigins"] ?? "";

                // 🧠 Process and filter based on environment index
                _cachedOrigins = originsRaw
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select((origin, index) => new { origin = origin.Trim(), index })
                    .Where(entry =>
                    {
                        // For Development, pick index 0; TestBed → index 1; Production → index 2
                        return (environment == "Development" && entry.index == 0) ||
                               (environment == "TestBed" && entry.index == 1) ||
                               (environment == "Production" && entry.index == 2);
                    })
                    .Select(entry =>
                    {
                        try
                        {
                            // Return just the host part of the origin (e.g., example.com)
                            return new Uri(entry.origin).Host.ToLowerInvariant();
                        }
                        catch
                        {
                            // Skip invalid URIs
                            return null;
                        }
                    })
                    .Where(host => !string.IsNullOrWhiteSpace(host)) // Filter out failed parsing
                    .Distinct()                                      // Remove duplicates
                    .ToList();

                return _cachedOrigins;
            }
        }
    }

}