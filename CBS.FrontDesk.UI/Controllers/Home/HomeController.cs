using CBS.FrontDesk.UI.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers
{
    //[CheckSessionTimeOutAttribute]
    //[SessionLockCheck]
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            
            return View();
        }
        

        public ActionResult About()
        {
            ViewBag.Message = Resources.GlobalAbout.AboutInformation;
            
            return View();
        }

        public ActionResult NoInternet()
        {

            return View();
        }
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Ping()
        {
            return Content("Pong");
        }
        public ActionResult Contact()
        {
            ViewBag.Message = Resources.GlobalAbout.AboutInformation;

            return View();
        }
    }
}