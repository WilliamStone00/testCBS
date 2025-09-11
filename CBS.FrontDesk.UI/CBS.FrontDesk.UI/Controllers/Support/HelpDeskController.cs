using CBS.BusinessService;
using CBS.BusinessService.Accounting;

using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace CBS.FrontDesk.UI.Controllers.Support
{
    [CheckSessionTimeOutAttribute]
    public class HelpDeskController : BaseController
    {
        public ActionResult Index()
        {
            // Redirect to the external support URL
            return Redirect("https://support.fluxsarl.com/");
        }
    }

}