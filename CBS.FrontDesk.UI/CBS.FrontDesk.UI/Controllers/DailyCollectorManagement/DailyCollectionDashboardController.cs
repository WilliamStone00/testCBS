using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.DailyCollectorManagement
{
    public class DailyCollectionDashboardController : BaseController
    {
        // GET: DailyCollectionDashboard
        public ActionResult Index()
        {
            return View();
        }
    }
}