using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace CBS.FrontDesk.UI.WAF.Middleware.Helpers
{
    /// <summary>
    /// <b>RequestCaptureHelper</b> provides utility methods to extract, summarize, and truncate
    /// the request body from incoming HTTP requests. This is especially useful for logging,
    /// WAF inspection, debugging, or auditing POST requests and form submissions.
    /// </summary>
    public static class RequestCaptureHelper
    {
        /// <summary>
        /// Safely captures the raw request body for POST requests.
        /// Returns null if the request is not POST or unreadable.
        /// </summary>
        /// <param name="request">The HTTP request</param>
        /// <returns>The full raw request body string or error message</returns>
        public static string CaptureRequestBody(HttpRequest request)
        {
            // ✅ Ensure it's a POST request with a readable body
            if (request.HttpMethod != "POST" || request.InputStream == null || !request.InputStream.CanRead)
                return null;

            try
            {
                // 🧭 Reset the stream position to the beginning
                request.InputStream.Position = 0;

                // 📥 Read body using correct encoding, leave stream open for reuse
                using (var reader = new StreamReader(request.InputStream, request.ContentEncoding, true, 1024, true))
                {
                    string body = reader.ReadToEnd();

                    // 🔁 Reset again so controllers or model binders can read it later
                    request.InputStream.Position = 0;

                    return body;
                }
            }
            catch (Exception ex)
            {
                // ❌ Return readable error message if reading fails
                return $"[Error reading body: {ex.Message}]";
            }
        }

        /// <summary>
        /// Truncates any string to a maximum length, appending an indicator if cut.
        /// </summary>
        /// <param name="input">The string to truncate</param>
        /// <param name="maxLength">The maximum allowed length (default: 500)</param>
        /// <returns>Truncated string with suffix if exceeded</returns>
        public static string Truncate(string input, int maxLength = 500)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            return input.Length <= maxLength ? input : input.Substring(0, maxLength) + "... [truncated]";
        }

        /// <summary>
        /// Builds a human-readable summary of the request, including method, path, content type,
        /// whether it's AJAX, and a truncated version of the body (if applicable).
        /// </summary>
        /// <param name="request">The HTTP request</param>
        /// <returns>A formatted summary string</returns>
        public static string GetRequestSummary(HttpRequest request)
        {
            string method = request.HttpMethod;
            string path = request.Path;
            bool isAjax = request.Headers["X-Requested-With"] == "XMLHttpRequest";
            string contentType = request.ContentType ?? "unknown";

            string body = method == "POST" ? CaptureRequestBody(request) : null;
            string shortBody = Truncate(body);

            return $"Method: {method}, Path: {path}, Ajax: {isAjax}, ContentType: {contentType}, Body: {shortBody}";
        }
    }


}