using CBS.BusinessService.Accounting;
using CBS.FrontDesk.UI.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers
{
    [CheckSessionTimeOutAttribute]
    //[SessionLockCheck]
    public class HomeController : BaseController
    {
        AccountingServices _accountingServices = new AccountingServices();
        public HomeController()
        {
                _accountingServices = new  AccountingServices();
        }
        public ActionResult Index()
        {
            
            return View();
        }
        

        public ActionResult About()
        {
            ViewBag.Message = Resources.GlobalHome.use_this_area_to_provide_additional_information;
            
            return View();
        }

        public ActionResult NoInternet()
        {

            return View();
        }
        
        public ActionResult Contact()
        {
            ViewBag.Message = Resources.GlobalHome.use_this_area_to_provide_additional_information;

            return View();
        }
    }
}