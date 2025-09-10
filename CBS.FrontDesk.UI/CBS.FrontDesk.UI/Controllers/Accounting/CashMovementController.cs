using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Accounting
{

        [CheckSessionTimeOutAttribute]
    public class CashMovementController : Controller
    {
        // GET: CashMovement
        public ActionResult Index()
        {
            return View();
        }
    }
}