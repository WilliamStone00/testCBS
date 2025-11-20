
using CBS.BusinessService.Accounting;
using CBS.BusinessService.Accounting_V2.AffiliateAccounts;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Configuration
{
    [CheckSessionTimeOutAttribute]
    public class FeeController : BaseController
    {
        // GET: Fee
        private readonly FeeServices _FeeServices;
        private readonly AffiliateAccountService _accountingServices;
        public FeeController(FeeServices FeeServices, AffiliateAccountService accountingServices)
        {
            _FeeServices = FeeServices;
            _accountingServices = accountingServices;
        }

        public async Task<ActionResult> Index()
        {
            try
            {
                ViewBag.Key = null;
                var chartOfAccounts = await _accountingServices.GetAllAffiliateAccounts();
                ViewBag.EventCodes = chartOfAccounts.ToList();
                return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction("InternalServer", "Error");
            }
         
        }
        [HttpPost]
        public async Task<ActionResult> Create(Fee model)
        {
            if (model.Id == null)
            {
                var data = await _FeeServices.Create(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else
            {
                return await Update(model);
            }
        }
        [HttpPost]
        public async Task<ActionResult> Update(Fee model)
        {
            var data = await _FeeServices.Update(model);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
        {
   
            
            if (path == "list")
            {
                var data = await _FeeServices.GetFees();
                return PartialView(partialView, data);
            }

            else if (path == "new")
            {
                ViewBag.EventCodes = await _accountingServices.GetAllAffiliateAccounts();
                ViewBag.Key = null;
                return PartialView(partialView, new Fee());
            }
            else
            {
                ViewBag.Key = KEY;
                ViewBag.EventCodes = await _accountingServices.GetAllAffiliateAccounts();
                var Fee = await _FeeServices.GetFee(KEY);
                return PartialView(partialView, Fee);

            }
        }

        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _FeeServices.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
        public async Task<bool> GetList()
        {
            ViewBag.Languages = _FeeServices.GetLanguages();
            
            return true;
        }
    }
}