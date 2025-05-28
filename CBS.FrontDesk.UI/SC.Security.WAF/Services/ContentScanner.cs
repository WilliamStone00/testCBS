using CBS.FrontDesk.UI.SC.Security.WAF.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Web;
using CBS.FrontDesk.UI.SC.Security.WAF.Models;
using System.Threading.Tasks;

namespace CBS.FrontDesk.UI.SC.Security.WAF.Services
{
    public class ContentScanner : IContentScanner
    {
        private static readonly List<Regex> DangerousPatterns = new List<Regex>
        {
            new Regex(@"<script.*?>.*?</script>", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new Regex(@"(\%27)|(\')|(\-\-)|(\%23)|(#)", RegexOptions.Compiled),
            new Regex(@"((\%3D)|(=))[^\n]*((\%27)|(\')|(\-\-)|(\%3B)|(;))", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new Regex(@"\b(union(\s+all)?\s+select)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new Regex(@"(\balert\s*\(|\bprompt\s*\(|\bconfirm\s*\()", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new Regex(@"(?i)(onerror|onload|onmouseover|onclick)=['""]?", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new Regex(@"(['""]\s*(or|and)\s*['""]?\d+=\d+)", RegexOptions.IgnoreCase), // SQLi
            new Regex(@"(<script\b[^>]*>(.*?)</script>)", RegexOptions.IgnoreCase),   // XSS
            new Regex(@"(on\w+\s*=\s*['""]?javascript:)", RegexOptions.IgnoreCase),   // inline JS
            new Regex(@"(\b(select|insert|delete|drop|update)\b\s+\w+)", RegexOptions.IgnoreCase),
            new Regex(@"<iframe\b[^>]*>", RegexOptions.IgnoreCase),
            new Regex(@"(\balert\s*\()", RegexOptions.IgnoreCase),
            new Regex(@"\b(base64_decode|eval|document\.cookie|window\.location)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled)
        };

        public ContentScanResultDto Scan(HttpRequest request)
        {
            var result = new ContentScanResultDto
            {
                IsMalicious = false,
                Reason = "",
                MatchedPattern = "",
                RawContent = ""
            };

            try
            {
                if (request.HttpMethod != "POST")
                    return result;

                string contentType = request.ContentType?.ToLowerInvariant();
                if (string.IsNullOrWhiteSpace(contentType) ||
                    (!contentType.Contains("application/json") &&
                     !contentType.Contains("application/x-www-form-urlencoded")))
                    return result;

                string bodyContent = ReadRequestBody(request);
                result.RawContent = bodyContent;

                if (string.IsNullOrWhiteSpace(bodyContent))
                    return result;

                foreach (var pattern in DangerousPatterns)
                {
                    if (pattern.IsMatch(bodyContent))
                    {
                        result.IsMalicious = true;
                        result.MatchedPattern = pattern.ToString();
                        result.Reason = $"🚨 Malicious content detected via pattern: {pattern}";
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                result.IsMalicious = false;
                result.Reason = $"⚠️ Error during body scan: {ex.Message}";
            }

            return result;
        }

        public async Task<ContentScanResultDto> ScanMultipartAsync(HttpRequest request)
        {
            var result = new ContentScanResultDto
            {
                IsMalicious = false,
                Reason = "",
                MatchedPattern = "",
                RawContent = ""
            };

            try
            {
                if (request.HttpMethod != "POST" || !request.ContentType.ToLower().Contains("multipart/form-data"))
                    return result;

                // 1️⃣ Scan form fields
                foreach (string key in request.Form)
                {
                    string value = request.Form[key];
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        foreach (var pattern in DangerousPatterns)
                        {
                            if (pattern.IsMatch(value))
                            {
                                result.IsMalicious = true;
                                result.MatchedPattern = pattern.ToString();
                                result.Reason = $"🚨 Malicious form field '{key}' matched pattern: {pattern}";
                                result.RawContent = value;
                                return result;
                            }
                        }
                    }
                }

                // 2️⃣ Scan small uploaded text files
                foreach (string fileKey in request.Files)
                {
                    HttpPostedFile file = request.Files[fileKey];

                    if (file != null && file.ContentLength > 0 && file.ContentLength < 1024 * 1024)
                    {
                        string contentType = file.ContentType.ToLowerInvariant();
                        if (contentType.StartsWith("text/") || contentType.Contains("plain"))
                        {
                            using (var reader = new StreamReader(file.InputStream))
                            {
                                string content = await reader.ReadToEndAsync();
                                file.InputStream.Position = 0;

                                foreach (var pattern in DangerousPatterns)
                                {
                                    if (pattern.IsMatch(content))
                                    {
                                        result.IsMalicious = true;
                                        result.MatchedPattern = pattern.ToString();
                                        result.Reason = $"🚨 Malicious upload '{file.FileName}' matched pattern: {pattern}";
                                        result.RawContent = content;
                                        return result;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.IsMalicious = false;
                result.Reason = $"⚠️ Error during multipart scan: {ex.Message}";
            }

            return result;
        }


        private string ReadRequestBody(HttpRequest request)
        {
            try
            {
                if (!request.InputStream.CanSeek)
                    return string.Empty;

                request.InputStream.Position = 0;
                using (var reader = new StreamReader(request.InputStream, Encoding.UTF8, true, 1024, leaveOpen: true))
                {
                    string body = reader.ReadToEnd();
                    request.InputStream.Position = 0; // Reset stream for downstream use
                    return body;
                }
            }
            catch
            {
                return string.Empty;
            }
        }
    }

}