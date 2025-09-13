using CBS.FrontDesk.UI.Filter;
using System.Web.Mvc;
using static CBS.FrontDesk.UI.MvcApplication;

namespace CBS.FrontDesk.UI
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new RequireHttpsAttribute());
            filters.Add(new RequestThrottlingFilter());
            filters.Add(new CorrelationIdActionFilter()); 

        }
    }
}

