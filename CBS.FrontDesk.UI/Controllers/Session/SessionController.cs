using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Session
{
    public class SessionController : Controller
    {
        // GET: Session
        [HttpGet]
        public ActionResult ExtendSessionTimeout()
        {
            // Extend session timeout on each request (e.g., add 5 minutes)
            Session.Timeout += 5; // Adjust the duration as needed

            // Return a success response
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }
    }
}