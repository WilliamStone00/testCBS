using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace CBS.FrontDesk.UI.Utility
{
    public static class CidrUtility
    {
        public static bool IsIpInRange(string ip, string cidr)
        {
            var parts = cidr.Split('/');
            if (parts.Length != 2)
                return false;

            var ipAddress = IPAddress.Parse(ip);
            var cidrAddress = IPAddress.Parse(parts[0]);
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

        public static bool IsCameroonIp(string ip, List<string> cameroonCidrs)
        {
            return cameroonCidrs.Any(cidr => IsIpInRange(ip, cidr));
        }
    }

}