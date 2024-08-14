using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNet.SignalR;

using Microsoft.Owin;

using Owin;
using System;
using System.Threading.Tasks;

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
    
            // Configure SignalR
            app.MapSignalR();
            ConfigureAuth(app);
        }

       

    }
}
