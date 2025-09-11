using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.RequestManagement
{


    public class RateLimitConfig
    {
        public string Id { get; set; }

        // Rate limiting
        public int RequestLimit { get; set; }
        public int TimeWindowSeconds { get; set; }
        public int BlockDurationMinutes { get; set; }

        public int IPRequestLimit { get; set; }
        public int IPTimeWindowSeconds { get; set; }

        // Header & IP whitelisting
        public List<string> WhitelistedHeaders { get; set; } = new List<string>();
        public List<string> WhitelistedCidrs { get; set; } = new List<string>();
        public List<string> WhitelistedCountriesCode { get; set; } = new List<string>();

        // Path & extension exclusions
        public List<string> ExcludedPaths { get; set; } = new List<string>();
        public List<string> ExcludedSubstrings { get; set; } = new List<string>();
        public List<string> ExcludedExtensions { get; set; } = new List<string>();

        // Threat signatures
        public List<string> BadUserAgents { get; set; } = new List<string>(); // changed to List
        public List<string> SuspiciousIndicators { get; set; } = new List<string>();

        // Header validation
        public List<string> AllowedOrigins { get; set; } = new List<string>();
        public List<string> TelemetryHeaderPrefixes { get; set; } = new List<string>();
        public List<string> HighRiskHeaderKeys { get; set; } = new List<string>();
        public List<string> ProtectedPathRefererRequired { get; set; } = new List<string>();
        public List<string> SensitiveHeaderMissingCheckPaths { get; set; } = new List<string>();
        public List<string> SpoofedForwardedForIndicators { get; set; } = new List<string>();
        public List<string> AllowedHeaderValuePattern { get; set; } = new List<string>();

        // Payload scanning
        public List<string> SuspiciousFormKeyPatterns { get; set; } = new List<string>();
        public List<string> BlockedFileExtensions { get; set; } = new List<string>();
        public int MaxUploadFileSizeKb { get; set; }
        public DateTime Timestamp { get; set; }
        // 🧩 5. View & State Management (Optional: decorate with [JsonIgnore] if needed)
        public string Action { get; set; }
        public string Option { get; set; }
    }

    public class NoScriptInjectionAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value is IEnumerable<string> list)
            {
                foreach (var item in list)
                {
                    if (Regex.IsMatch(item, @"<script.*?>|</script>", RegexOptions.IgnoreCase))
                        return false;
                }
            }
            else if (value is string str)
            {
                if (Regex.IsMatch(str, @"<script.*?>|</script>", RegexOptions.IgnoreCase))
                    return false;
            }

            return true;
        }
    }

    public class CIDRValidationAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value is List<string> list)
            {
                foreach (var cidr in list)
                {
                    if (!Regex.IsMatch(cidr, @"^(\d{1,3}\.){3}\d{1,3}/\d{1,2}$"))
                        return false;
                }
            }

            return true;
        }
    }

    public class CountryCodeValidationAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value is List<string> codes)
            {
                foreach (var code in codes)
                {
                    if (string.IsNullOrWhiteSpace(code) || code.Length != 2 || !Regex.IsMatch(code, @"^[A-Z]{2}$"))
                        return false;
                }
            }

            return true;
        }
    }

}
