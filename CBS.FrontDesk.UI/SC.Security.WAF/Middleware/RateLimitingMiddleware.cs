using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CBS.FrontDesk.UI.SC.Security.WAF.Middleware
{
    public class RateLimitingMiddleware : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.PostAuthenticateRequest += OnPostAuthenticateRequest;
        }

        private void OnPostAuthenticateRequest(object sender, EventArgs e)
        {
            // This will delegate logic to service layer (RequestInspectorService etc.)
        }

        public void Dispose() { }
    }
}