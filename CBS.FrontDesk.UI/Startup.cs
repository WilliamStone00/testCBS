using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(CBS.FrontDesk.UI.Startup))]
namespace CBS.FrontDesk.UI
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
