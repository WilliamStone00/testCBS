using Hangfire;
using Hangfire.SqlServer;
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
            ConfigureAuth(app);
        }
    }
}
