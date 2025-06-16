using CBS.FrontDesk.Data.Entity.RequestManagement;
using CBS.FrontDesk.UI.WAF.Middleware.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.PeerToPeer;
using System.Web;

namespace CBS.FrontDesk.UI.WAF.Middleware.Core
{
    using System;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Web;
    using Newtonsoft.Json;

    /// <summary>
    /// <b>WAFContext</b> is a unified data structure that encapsulates everything 
    /// needed to inspect, log, and analyze a web request in the TRUSTSOFTCREDIT Web Application Firewall (WAF).
    /// 
    /// It includes client IP, headers, request metadata, user identity, geolocation, and correlation details
    /// extracted at runtime and passed to the WAF analyzers.
    /// </summary>
    public class WAFContext
    {
        // ========== 🔐 Request + Identity ==========
        public string Ip { get; set; }                    // Final resolved IP (with Geo lookup support)
        public string Path { get; set; }                  // URL path of the request (lowercased)
        public string Method { get; set; }                // HTTP method (GET, POST, etc.)
        public string ContentType { get; set; }           // Request Content-Type
        public string UserAgent { get; set; }             // User-Agent string
        public string Referer { get; set; }               // HTTP Referer
        public string Origin { get; set; }                // HTTP Origin header
        public string FullUrl { get; set; }               // Full reconstructed request URL
        public bool IsAjax { get; set; }                  // If request was made via AJAX (X-Requested-With)
        public string Host { get; set; }                  // Host target of the request

        // ========== 👤 User (if authenticated) ==========
        public bool IsAuthenticated { get; set; }         // True if user is logged in
        public string Username { get; set; }              // Logged-in username
        public string FullName { get; set; }              // Full name from identity source
        public string PhoneNumber { get; set; }           // User phone number
        public string BranchName { get; set; }            // Logical branch name
        public string BranchId { get; set; }              // Internal branch ID
        public string BranchCode { get; set; }            // Branch code (external reference)

        // ========== 🌍 Geolocation ==========
        public string Country { get; set; }               // Country derived from IP
        public string Region { get; set; }                // Region/State/Province
        public string City { get; set; }                  // City/town of origin
        public string Latitude { get; set; }              // Geo latitude
        public string Longitude { get; set; }             // Geo longitude
        public string Location { get; set; }              // Formatted location string

        // ========== 📦 Headers & Payload ==========
        public NameValueCollection Headers { get; set; }  // All request headers
        public string RawHeaders { get; set; }            // Headers serialized as JSON
        public string RequestBody { get; set; }           // Raw or parsed request body (set later in middleware)
        public string ResponseBody { get; set; }          // Response body (optional, for post-analysis)
        public string ActionMethod { get; set; }          // Alias for HTTP method (used in analyzers)

        // ========== 🧩 Tracking ==========
        public string CorrelationId { get; set; }         // Unique ID per request (used for traceability)

        // ========== ⚙️ Config ==========
        public RateLimitConfig limitConfig { get; set; } = new RateLimitConfig(); // WAF rules used for the current request

        /// <summary>
        /// Constructs a WAFContext from the current HTTP context and injectable services.
        /// </summary>
        /// <param name="context">The raw ASP.NET HttpContext of the incoming request.</param>
        /// <param name="geoResolver">A function to resolve IP to (location, lat, lon, city, region, country).</param>
        /// <param name="userResolver">A function to extract user identity and branch info from the request.</param>
        /// <param name="limitConfig">The active WAF configuration used for this request.</param>
        /// <returns>A populated WAFContext instance ready for inspection.</returns>
        public static WAFContext BuildFromHttp(
            HttpContext context,
            Func<string, (string ip, string location, string lat, string lon, string city, string region, string country)> geoResolver,
            Func<HttpRequest, (string username, string branchId, string branchCode, string branchName, string phone, string fullName, bool isAuthenticated)> userResolver,
            RateLimitConfig limitConfig)
        {
            var request = context.Request;

            // 🔍 Resolve IP address from X-Forwarded-For or fallback to UserHostAddress
            var ip = request?.Headers["X-Forwarded-For"]?.Split(',')?.FirstOrDefault()?.Trim()
                  ?? request.UserHostAddress;
            //var ip = "102.244.43.113";

            // 👤 Resolve user identity from token or session
            var (username, branchId, branchCode, branchName, phone, fullName, isAuthenticated) = userResolver(request);

            // 🗺️ Perform geolocation resolution
            var (resolvedIp, location, lat, lon, city, region, country) = geoResolver(ip);

            // 🧵 Retrieve or assign correlation ID
            string correlationId = context.Items["CorrelationId"]?.ToString()
                                ?? request.Headers[CorrelationConstants.HeaderKey]
                                ?? Guid.NewGuid().ToString();

            // 📥 Optional response body capture (used for blocked response rendering)
            string responseBody = context.Items["CapturedResponseBody"]?.ToString();

            // 📦 Construct and return full context object
            return new WAFContext
            {
                Ip =resolvedIp,
                Path = request.Path.ToLower(),
                Method = request.HttpMethod,
                ContentType = request.ContentType,
                UserAgent = request.UserAgent,
                Referer = request.Headers["Referer"],
                Origin = request.Headers["Origin"],
                FullUrl = request.Url?.GetLeftPart(UriPartial.Authority) + request.RawUrl,
                IsAjax = request.Headers["X-Requested-With"] == "XMLHttpRequest",
                IsAuthenticated = isAuthenticated,
                Username = username,
                FullName = fullName,
                PhoneNumber = phone,
                BranchName = branchName,
                BranchId = branchId,
                BranchCode = branchCode,
                Country = country,
                Region = region,
                City = city,
                Latitude = lat,
                Longitude = lon,
                Location = location,
                Host = request.Url?.Host?.ToLowerInvariant(),
                CorrelationId = correlationId,
                Headers = request.Headers,
                RawHeaders = JsonConvert.SerializeObject(request.Headers.AllKeys.Where(k => !string.Equals(k, "Cookie", StringComparison.OrdinalIgnoreCase))
                    .ToDictionary(k => k, k => request.Headers[k]),
                    Formatting.Indented),
                RequestBody = null, // will be populated later in middleware
                ActionMethod = request.HttpMethod,
                ResponseBody = responseBody,
                limitConfig = limitConfig
            };
        }
    }



}