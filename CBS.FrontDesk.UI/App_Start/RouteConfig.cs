using System.Web.Mvc;
using System.Web.Routing;

namespace CBS.FrontDesk.UI
{

    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("CBSTemp/ui/assets/json/{*pathInfo}");

            // ✅ Register Blocked page explicitly before default
            routes.MapRoute(
                name: "Blocked",
                url: "Error/Blocked",
                defaults: new { controller = "Error", action = "Blocked" }
            );

            // ✅ Default fallback route
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }

}