using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;

namespace CBS.FrontDesk.Helper.Helper
{
    public static class GeolocalizationHelper
    {
        private static readonly MemoryCache _ipCache = MemoryCache.Default;
        public static (string IpAddress, string Location, string Latitude, string Longitude, string City, string Region, string Country) GetIpAndLocationSync()
        {
            try
            {
                var request = HttpContext.Current?.Request;

                // Extract IP
                string ip = request?.Headers["X-Forwarded-For"]?.Split(',')?.FirstOrDefault()?.Trim();
                if (string.IsNullOrWhiteSpace(ip))
                    ip = request?.UserHostAddress;

                if (string.IsNullOrWhiteSpace(ip) || ip.Equals("unknown", StringComparison.OrdinalIgnoreCase))
                    ip = Guid.NewGuid().ToString("N"); // fallback IP

                string cacheKey = $"ClientResolvedIPAndLocation:{ip}";
                if (_ipCache.Contains(cacheKey))
                    return ((string, string, string, string, string, string, string))_ipCache.Get(cacheKey);

                using (var client = new WebClient())
                {
                    string json = client.DownloadString($"https://ipinfo.io/{ip}/json");
                    dynamic result = JsonConvert.DeserializeObject(json);

                    string city = result?.city ?? "";
                    string region = result?.region ?? "";
                    string country = result?.country ?? "";
                    string loc = result?.loc ?? "";

                    string latitude = "", longitude = "";
                    if (!string.IsNullOrWhiteSpace(loc) && loc.Contains(","))
                    {
                        var parts = loc.Split(',');
                        latitude = parts[0];
                        longitude = parts[1];
                    }

                    string location = $"{city}, {region}, {country}".Trim().Trim(',');

                    var resolvedData = (ip, location, latitude, longitude, city, region, country);
                    _ipCache.Set(cacheKey, resolvedData, DateTimeOffset.Now.AddHours(4)); // cache result
                    return resolvedData;
                }
            }
            catch
            {
                string fallback = Guid.NewGuid().ToString("N");
                return (fallback, "Unknown", "", "", "", "", "");
            }
        }
        public static bool IsPublicIp(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
                return false;

            if (IPAddress.TryParse(ip, out var address))
            {
                byte[] bytes = address.GetAddressBytes();

                // IPv4 Checks
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    return !(bytes[0] == 10 ||
                             (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) ||
                             (bytes[0] == 192 && bytes[1] == 168) ||
                             bytes[0] == 127);
                }

                // IPv6 Checks
                if (address.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    return !(address.IsIPv6LinkLocal || address.IsIPv6SiteLocal || IPAddress.IsLoopback(address));
                }
            }

            return false;
        }

    }
}
