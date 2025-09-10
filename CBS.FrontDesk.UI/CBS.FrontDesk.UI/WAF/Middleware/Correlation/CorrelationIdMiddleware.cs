using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace CBS.FrontDesk.UI.WAF.Middleware.Core
{
    /// <summary>
    /// Middleware that ensures every incoming HTTP request has a unique Correlation ID.
    /// If the client does not provide one (via `X-Correlation-ID`), a new GUID is generated.
    /// The Correlation ID is stored in the OWIN context and added to the response header for traceability.
    /// </summary>
    public class CorrelationIdMiddleware : OwinMiddleware
    {
        /// <summary>
        /// Initializes the middleware and passes control to the next middleware component.
        /// </summary>
        public CorrelationIdMiddleware(OwinMiddleware next) : base(next) { }

        /// <summary>
        /// Middleware invocation logic that ensures correlation tracking is injected into the pipeline.
        /// </summary>
        /// <param name="context">The OWIN context of the request</param>
        public override async Task Invoke(IOwinContext context)
        {
            var headers = context.Request.Headers;

            // 🧾 Try to extract correlation ID from incoming request headers
            string correlationId = headers[CorrelationConstants.HeaderKey];

            // 🆕 If not provided, generate a new one
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
            }

            // 🧠 Set it into the OWIN request context (for downstream components)
            context.Set("CorrelationId", correlationId);

            // 📤 Also attach to the response headers
            context.Response.Headers.Append(CorrelationConstants.HeaderKey, correlationId);

            // 🪵 Optional: Log correlation ID to debug output
            System.Diagnostics.Debug.WriteLine($"🔗 CorrelationId set: {correlationId}");

            // ⏩ Proceed to next middleware in the pipeline
            await Next.Invoke(context);
        }
    }

    /// <summary>
    /// Constants used for correlation tracking.
    /// </summary>
    public static class CorrelationConstants
    {
        /// <summary>
        /// The header key used to transmit the correlation ID between client and server.
        /// </summary>
        public const string HeaderKey = "X-Correlation-ID";
    }
}