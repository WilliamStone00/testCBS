using CBS.BusinessService.Config.Localization;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.BusinessService.Config;

namespace CBS.FrontDesk.UI.Controllers.Configuration
{
    [CheckSessionTimeOutAttribute]

    public class LoanProductCategoryController : BaseController
    {
        // GET: LoanProductCategory
        private readonly LoanProductCategoryServices _loanProductCategoryServices;
        public LoanProductCategoryController(LoanProductCategoryServices PeriodServices)
        {
            _loanProductCategoryServices = PeriodServices;
        }
      
        public async Task<ActionResult> Index()
        {
           
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Create(LoanProductCategory model)
        {
            if (ModelState.IsValid)
            {
                var data = await _loanProductCategoryServices.Create(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }
        [HttpPost]
        public async Task<ActionResult> Update(LoanProductCategory model)
        {
            if (ModelState.IsValid)
            {
                var data = await _loanProductCategoryServices.Update(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }
        
        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path=null)
        {
            if (path=="list")
            {
                var data = await _loanProductCategoryServices.GetLoanProductCategorys();
                return PartialView(partialView, data);
            }

            else if (path == "new")
            {
                return PartialView(partialView, new LoanProductCategory());
            }
            else
            {
                var Period = await _loanProductCategoryServices.GetLoanProductCategory(KEY);
                return PartialView(partialView, Period);
                
            }
        }

        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _loanProductCategoryServices.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
    }
}