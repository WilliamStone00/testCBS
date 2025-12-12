using CBS.BusinessService.AccountingV2.InterestProductConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.AccountingV2.InterestProductConfig
{
    public class InterestProductConfigController : Controller
    {


        private readonly InterestProductConfigService interestProductConfigService;
        // GET: InterestProductConfig
        public ActionResult Index()
        {
            return View();
        }
    }
}