using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CBS.FrontDesk.UI.Filter
{
    using System.Text.RegularExpressions;

    public class MaliciousContentScannerService
    {
        private static readonly Regex[] MaliciousPatterns = new[]
        {
        new Regex(@"(['""]\s*(or|and)\s*['""]?\d+=\d+)", RegexOptions.IgnoreCase), // SQLi
        new Regex(@"(<script\b[^>]*>(.*?)</script>)", RegexOptions.IgnoreCase),   // XSS
        new Regex(@"(on\w+\s*=\s*['""]?javascript:)", RegexOptions.IgnoreCase),   // inline JS
        new Regex(@"(\b(select|insert|delete|drop|update)\b\s+\w+)", RegexOptions.IgnoreCase),
        new Regex(@"<iframe\b[^>]*>", RegexOptions.IgnoreCase),
        new Regex(@"(\balert\s*\()", RegexOptions.IgnoreCase)
    };

        public bool IsMalicious(string input, out string matchedPattern)
        {
            matchedPattern = null;

            if (string.IsNullOrWhiteSpace(input))
                return false;

            foreach (var pattern in MaliciousPatterns)
            {
                if (pattern.IsMatch(input))
                {
                    matchedPattern = pattern.ToString();
                    return true;
                }
            }

            return false;
        }
    }

}