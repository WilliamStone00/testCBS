using CBS.FrontDesk.UI.Filter;
using CBS.FrontDesk.UI.WAF.Middleware.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace CBS.FrontDesk.UI.WAF.Services.Validation
{
    /// <summary>
    /// Performs request body inspection (text and multipart form data) to detect
    /// malicious payloads such as XSS, SQL injection, command injection, etc.
    ///
    /// Uses a pluggable scanner service to check both:
    /// - POST body content (e.g., JSON, form-urlencoded)
    /// - Multipart form fields and uploaded files (e.g., .csv, .xml)
    /// </summary>
    public class BodyScanner
    {
        private readonly MaliciousContentScannerService _scanner;
        private readonly int _maxFileSizeInBytes;
        private readonly HashSet<string> _allowedMimeTypes;

        /// <summary>
        /// Constructor for BodyScanner.
        /// </summary>
        /// <param name="scanner">Injected service that detects malicious patterns.</param>
        /// <param name="maxFileScanSizeInBytes">Maximum file size to scan (default: 1MB).</param>
        /// <param name="allowedFileMimeTypes">Whitelisted file types to scan.</param>
        public BodyScanner(
            MaliciousContentScannerService scanner,
            int maxFileScanSizeInBytes = 1024 * 1024,
            IEnumerable<string> allowedFileMimeTypes = null)
        {
            _scanner = scanner;
            _maxFileSizeInBytes = maxFileScanSizeInBytes;
            _allowedMimeTypes = new HashSet<string>(
                (allowedFileMimeTypes ?? new[] { "text/plain", "text/csv", "text/xml", "application/xml" })
                .Select(m => m.ToLowerInvariant())
            );
        }

        /// <summary>
        /// Scans the request body (JSON or form-urlencoded) for malicious content.
        /// </summary>
        public bool IsMaliciousBody(HttpRequest request, WAFContext context, out string reason)
        {
            reason = null;

            if (request.HttpMethod != "POST" || request.ContentType == null)
                return false;

            string contentType = request.ContentType.ToLowerInvariant();
            if (!(contentType.Contains("application/json") || contentType.Contains("application/x-www-form-urlencoded")))
                return false;

            try
            {
                string body = ReadRequestBody(request);

                if (!string.IsNullOrWhiteSpace(body) && _scanner.IsMalicious(body, out var matched))
                {
                    reason = $"🚨 Malicious pattern detected in request body. Pattern: {matched}\n📝 Payload: {Truncate(body, 500)}";
                    return true;
                }
            }
            catch (Exception ex)
            {
                reason = $"⚠️ Body scan failed: {ex.Message}";
                return true; // Block on scanner failure
            }

            return false;
        }

        /// <summary>
        /// Scans multipart/form-data (form fields and files) for suspicious keys or malicious content.
        /// </summary>
        public bool IsMaliciousMultipart(HttpRequest request, WAFContext context, out string reason)
        {
            reason = null;

            if (request.HttpMethod != "POST" || !request.ContentType?.StartsWith("multipart/form-data", StringComparison.OrdinalIgnoreCase) == true)
                return false;

            try
            {
                // 🔍 Scan form fields
                foreach (string key in request.Form.AllKeys)
                {
                    var value = request.Form[key];

                    if (IsSuspiciousKey(key, out var matchedKeyPattern))
                    {
                        reason = $"🚨 Suspicious form key name: '{key}'. Pattern: {matchedKeyPattern}";
                        return true;
                    }

                    if (!string.IsNullOrWhiteSpace(value) && _scanner.IsMalicious(value, out var matchedValue))
                    {
                        reason = $"🚨 Malicious field value in '{key}'. Pattern: {matchedValue}";
                        return true;
                    }
                }

                // 🗃️ Scan uploaded files
                foreach (string fileKey in request.Files.AllKeys)
                {
                    var file = request.Files[fileKey];
                    if (file != null && file.ContentLength > 0 && file.ContentLength <= _maxFileSizeInBytes)
                    {
                        var mime = file.ContentType.ToLowerInvariant();

                        if (_allowedMimeTypes.Contains(mime))
                        {
                            using (var reader = new StreamReader(file.InputStream))
                            {
                                string fileContent = reader.ReadToEnd();

                                if (_scanner.IsMalicious(fileContent, out var matchedPattern))
                                {
                                    reason = $"🚨 Malicious pattern in file '{file.FileName}'. Pattern: {matchedPattern}";
                                    return true;
                                }
                            }

                            file.InputStream.Position = 0; // Reset for downstream access
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                reason = $"⚠️ Multipart scan failed: {ex.Message}";
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines if a form key contains suspicious naming (e.g., encoded or obfuscated).
        /// </summary>
        private bool IsSuspiciousKey(string key, out string reason)
        {
            reason = null;
            if (string.IsNullOrWhiteSpace(key)) return false;

            string lower = key.ToLowerInvariant();

            if (lower.StartsWith("0x"))
            {
                reason = "Hex-like key prefix (0x)";
                return true;
            }

            if (Regex.IsMatch(lower, @"[{}<>]"))
            {
                reason = "Braces or angle brackets in key";
                return true;
            }

            if (Regex.IsMatch(lower, @"[^a-z0-9_\-.\[\]]"))
            {
                reason = "Unusual characters in key";
                return true;
            }

            return false;
        }

        /// <summary>
        /// Reads and resets the request input stream.
        /// </summary>
        private string ReadRequestBody(HttpRequest request)
        {
            if (request.InputStream == null || !request.InputStream.CanRead)
                return string.Empty;

            try
            {
                request.InputStream.Position = 0;
                using (var reader = new StreamReader(request.InputStream, request.ContentEncoding, true, 1024, true))
                {
                    string body = reader.ReadToEnd();
                    request.InputStream.Position = 0;
                    return body;
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Truncates long strings to prevent logging huge payloads.
        /// </summary>
        private string Truncate(string input, int maxLength)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return input.Length <= maxLength ? input : input.Substring(0, maxLength) + "...[truncated]";
        }
    }



}