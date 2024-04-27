using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Home
{
    public class DashboardController : BaseController
    {
        // GET: Dashboard
        public ActionResult HeadOffice()
        {
            return View();
        }
        public ActionResult BranchOffice()
        {
            return View();
        }
    }
}