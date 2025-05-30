using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Web;

namespace CBS.FrontDesk.UI.Utility
{
    public static class CidrUtility
    {

        /// <summary>
        /// Checks whether an IP address is within a specific CIDR block.
        /// </summary>
        public static bool IsIpInRange(string ip, string cidr)
        {
            var parts = cidr.Split('/');
            if (parts.Length != 2)
                return false;

            if (!IPAddress.TryParse(ip, out var ipAddress) || !IPAddress.TryParse(parts[0], out var cidrAddress))
                return false;

            int prefixLength = int.Parse(parts[1]);
            var ipBytes = ipAddress.GetAddressBytes();
            var cidrBytes = cidrAddress.GetAddressBytes();

            if (ipBytes.Length != cidrBytes.Length)
                return false;

            int byteCount = prefixLength / 8;
            int bitRemainder = prefixLength % 8;

            for (int i = 0; i < byteCount; i++)
            {
                if (ipBytes[i] != cidrBytes[i])
                    return false;
            }

            if (bitRemainder > 0)
            {
                int mask = (byte)(~(255 >> bitRemainder));
                if ((ipBytes[byteCount] & mask) != (cidrBytes[byteCount] & mask))
                    return false;
            }

            return true;
        }
        public static bool IsValidIPv4(string ip)
        {
            return IPAddress.TryParse(ip, out var addr) && addr.AddressFamily == AddressFamily.InterNetwork;
        }
      

        /// <summary>
        /// Checks if the given IP is within the CIDRs for the specified country ISO code.
        /// </summary>
        public static bool IsIpInCidr(string ip, List<string> cidrs)
        {
            return cidrs.Any(cidr => IsIpInRange(ip, cidr));
        }

        /// <summary>
        /// Checks if the given country ISO code is in the list of allowed countries.
        /// </summary>
        public static bool IsWhitelistedCountry(string isoCountryCode, List<string> whitelistedCountries)
        {
            if (string.IsNullOrWhiteSpace(isoCountryCode) || whitelistedCountries == null || whitelistedCountries.Count == 0)
                return false;

            return whitelistedCountries.Contains(isoCountryCode.ToUpperInvariant());
        }

       
    }

}