using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;
using System;

[assembly: OwinStartupAttribute(typeof(CBS.FrontDesk.UI.Startup))]
namespace CBS.FrontDesk.UI
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
       {
            //GlobalConfiguration.Configuration
            //.UseSqlServerStorage("CBSTransactionDB");

            // app.UseHangfireDashboard();
            // app.UseHangfireServer();
            // Enable CORS for SignalR hubs
            //app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
            // Configure SignalR
            var hubConfiguration = new HubConfiguration
            {
                EnableDetailedErrors = true
            };

            // Configure SignalR options
            GlobalHost.Configuration.MaxIncomingWebSocketMessageSize = 32 * 1024; // 32 KB

            // Set DisconnectTimeout first
            GlobalHost.Configuration.DisconnectTimeout = TimeSpan.FromSeconds(180); // 3 minutes

            // Set ConnectionTimeout (same as DisconnectTimeout or less)
            GlobalHost.Configuration.ConnectionTimeout = TimeSpan.FromSeconds(180);

            // Set KeepAlive to be at most 1/3 of DisconnectTimeout
            GlobalHost.Configuration.KeepAlive = TimeSpan.FromSeconds(60); // 1 minute

            // Map SignalR hubs
            app.MapSignalR("/signalr", hubConfiguration);
            app.MapSignalR();

            ConfigureAuth(app);
        }

       

    }
}
