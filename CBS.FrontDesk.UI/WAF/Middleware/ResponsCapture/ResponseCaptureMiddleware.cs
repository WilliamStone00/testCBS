using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace CBS.FrontDesk.UI.WAF.Middleware.Core.ResponsCapture
{
    /// <summary>
    /// <b>ResponseCaptureMiddleware</b> is an ASP.NET <see cref="IHttpModule"/> that intercepts
    /// the HTTP response stream at the beginning of each request.
    /// It replaces the response filter with a custom stream <see cref="ResponseCaptureFilterStream"/>
    /// that captures a full copy of the response body for inspection or logging (e.g. by WAF).
    /// </summary>
    public class ResponseCaptureMiddleware : IHttpModule
    {
        /// <summary>
        /// Initializes the middleware and hooks into the <c>BeginRequest</c> event.
        /// It replaces the <c>HttpResponse.Filter</c> stream with a custom capturing stream.
        /// </summary>
        /// <param name="context">The current ASP.NET application</param>
        public void Init(HttpApplication context)
        {
            context.BeginRequest += (sender, e) =>
            
            {
                var app = (HttpApplication)sender;

                // ✅ Logging initialization for diagnostics
                System.Diagnostics.Debug.WriteLine("✅ ResponseCaptureMiddleware initialized.");

                // 🎯 Save original response stream
                var originalFilter = app.Response.Filter;

                // 📦 Wrap response with a capture stream
                var captureStream = new ResponseCaptureFilterStream(originalFilter);

                // 🔁 Replace the response stream with our capturing stream
                app.Response.Filter = captureStream;

                // 🧠 Store the capturing stream in context for downstream access
                app.Context.Items["__ResponseCaptureFilter"] = captureStream;
            };
        }

        /// <summary>
        /// Disposes any internal resources — unused in this case.
        /// </summary>
        public void Dispose() { }
    }


}