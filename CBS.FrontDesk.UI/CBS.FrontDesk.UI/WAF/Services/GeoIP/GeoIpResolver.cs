using CBS.FrontDesk.UI.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Caching;
using System.Web;

namespace CBS.FrontDesk.UI.WAF.Services.GeoIP
{
    /// <summary>
    /// Resolves geolocation data (country, region, city, coordinates) for a given IP address
    /// using the external ipinfo.io API. Results are cached to improve performance and
    /// reduce external requests.
    ///
    /// Also provides helper methods to validate IP addresses and match against
    /// whitelisted countries or CIDR ranges.
    /// </summary>
    public class GeoIpResolver
    {
        private readonly MemoryCache _cache = MemoryCache.Default;

        // Cache duration for resolved IPs
        private readonly TimeSpan _cacheDuration = TimeSpan.FromHours(4);

        /// <summary>
        /// Resolves geolocation metadata for a given IP address using ipinfo.io.
        /// Returns details including location, coordinates, and region/country.
        /// </summary>
        /// <param name="ip">The public IP address to look up.</param>
        /// <returns>
        /// A tuple containing:
        /// - ip: original IP,
        /// - location: "City, Region, Country",
        /// - latitude,
        /// - longitude,
        /// - city,
        /// - region,
        /// - country
        /// </returns>
        public (string ip, string location, string lat, string lon, string city, string region, string country) Resolve(string ip)
        {
            string cacheKey = $"GeoIP:{ip}";

            // ✅ Return cached result if available
            if (_cache.Contains(cacheKey))
                return ((string, string, string, string, string, string, string))_cache.Get(cacheKey);

            try
            {
                using (var client = new WebClient())
                {
                    // 🌐 Query ipinfo.io for IP geolocation
                    string json = client.DownloadString($"https://ipinfo.io/{ip}/json");
                    dynamic result = JsonConvert.DeserializeObject(json);

                    string city = result?.city ?? "";
                    string region = result?.region ?? "";
                    string country = result?.country ?? "";
                    string loc = result?.loc ?? ""; // Format: "lat,lon"

                    string latitude = "", longitude = "";
                    if (!string.IsNullOrWhiteSpace(loc) && loc.Contains(","))
                    {
                        var parts = loc.Split(',');
                        latitude = parts[0];
                        longitude = parts[1];
                    }

                    // 🧭 Format full location string
                    string location = $"{city}, {region}, {country}".Trim().Trim(',');

                    var geoData = (ip, location, latitude, longitude, city, region, country);

                    // 🧠 Cache the result
                    _cache.Set(cacheKey, geoData, DateTimeOffset.Now.Add(_cacheDuration));

                    return geoData;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ GeoIP lookup failed for {ip}: {ex.Message}");
                return (ip, "Unknown", "", "", "", "", "");
            }
        }

        /// <summary>
        /// Checks whether a given string is a valid IPv4 address.
        /// </summary>
        public bool IsValidIPv4(string ip) => CidrUtility.IsValidIPv4(ip);

        /// <summary>
        /// Checks whether a resolved country is in the whitelist.
        /// </summary>
        public bool IsWhitelistedCountry(string country, List<string> whitelist)
            => CidrUtility.IsWhitelistedCountry(country, whitelist);

        /// <summary>
        /// Checks whether an IP is within a whitelisted CIDR block.
        /// </summary>
        public bool IsIpInCidr(string ip, List<string> cidrs)
            => CidrUtility.IsIpInCidr(ip, cidrs);
    }


}