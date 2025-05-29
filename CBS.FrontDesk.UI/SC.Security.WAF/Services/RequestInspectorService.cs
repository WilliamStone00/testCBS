using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CBS.FrontDesk.UI.SC.Security.WAF.Services
{
    using System;
    using System.Web;
    using CBS.BusinessService.RequestLoggerServicesP;
    using CBS.FrontDesk.UI.SC.Security.WAF.Interfaces;
    using CBS.FrontDesk.UI.SC.Security.WAF.Models;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;

    namespace TSC.Security.WAF.Services
    {
        public class RequestInspectorService
        {
            private readonly CIDRFilterService _cidrService;
            private readonly HeaderValidator _headerValidator;
            private readonly IContentScanner _scanner;
            private readonly WAFSuspiciousPathService _suspiciousPathService;
            private readonly IRateLimitEvaluator _rateLimitEvaluator;
            private readonly IRequestLogger _logger;

            public RequestInspectorService(
                CIDRFilterService cidrService,
                HeaderValidator headerValidator,
                IContentScanner scanner,
                WAFSuspiciousPathService suspiciousPathService,
                IRateLimitEvaluator rateLimitEvaluator,
                IRequestLogger logger)
            {
                _cidrService = cidrService;
                _headerValidator = headerValidator;
                _scanner = scanner;
                _suspiciousPathService = suspiciousPathService;
                _rateLimitEvaluator = rateLimitEvaluator;
                _logger = logger;
            }

            public async Task<WafDecisionResult> InspectAsync(HttpRequest request, string username)
            {
                string ip = request.UserHostAddress;
                string path = request.Path.ToLowerInvariant();
                string fullUrl = request.Url?.AbsoluteUri;

                var result = new WafDecisionResult();

                // 1️⃣ IP Format check
                if (!IPAddress.TryParse(ip, out var _))
                {
                    result.IsBlocked = true;
                    result.Reason = "Invalid IP format";
                    return result;
                }

                // 2️⃣ GeoIP + CIDR filter
                if (!_cidrService.IsAllowedCameroonIP(ip))
                {
                    result.IsBlocked = true;
                    result.Reason = "IP outside allowed Cameroon CIDRs";
                    return result;
                }

                // 3️⃣ Header inspection
                var headerValidation = _headerValidator.Validate(request);
                if (headerValidation.IsSuspicious)
                {
                    result.IsBlocked = true;
                    result.Reason = headerValidation.Reason;
                    return result;
                }

                // 4️⃣ Suspicious path check (ASYNC)
                if (await _suspiciousPathService.CheckIfSuspiciousPathAsync(path))
                {
                    result.IsBlocked = true;
                    result.Reason = "Known suspicious path";
                    return result;
                }

                // 5️⃣ Malicious body content (JSON or URL-Encoded)
                if (request.HttpMethod == "POST" && request.ContentLength > 0 &&
                    (request.ContentType?.Contains("application/json") == true ||
                     request.ContentType?.Contains("application/x-www-form-urlencoded") == true))
                {
                    request.InputStream.Position = 0;
                    using (var reader = new StreamReader(request.InputStream, request.ContentEncoding, true, 1024, true))
                    {
                        string body = await reader.ReadToEndAsync();
                        request.InputStream.Position = 0; // reset for downstream use
                    }

                    var bodyScanResult = _scanner.Scan(request);
                    if (bodyScanResult.IsMalicious)
                    {
                        result.IsBlocked = true;
                        result.Reason = bodyScanResult.Reason;
                        return result;
                    }
                }

                // 6️⃣ Malicious multipart content (e.g. file uploads, large forms)
                if (request.HttpMethod == "POST" &&
                    request.ContentType?.ToLowerInvariant().Contains("multipart/form-data") == true)
                {
                    var multipartScanResult = await _scanner.ScanMultipartAsync(request);
                    if (multipartScanResult.IsMalicious)
                    {
                        result.IsBlocked = multipartScanResult.IsMalicious;
                        result.Reason = multipartScanResult.Reason;
                        return result;
                    }
                }

                // 7️⃣ Rate limiting
                //if (_rateLimitEvaluator.ShouldBlockAsync(ip, 50, TimeSpan.FromMinutes(10)))
                //{
                //    result.IsBlocked = true;
                //    result.Reason = "Rate limit exceeded";
                //    return result;
                //}

                // ✅ Log clean access
                _logger.Log(new WafLogContext
                {
                    Ip = ip,
                    Username = username,
                    Path = path,
                    FullUrl = fullUrl,
                    IsBlocked = false,
                    Reason = "Allowed"
                });

                return result;
            }
        }

    }


}