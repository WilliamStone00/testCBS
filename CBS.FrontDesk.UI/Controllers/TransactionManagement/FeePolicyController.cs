
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.TransactionManagement
{
    [CheckSessionTimeOutAttribute]

    public class FeePolicyController : BaseController
    {
        // GET: FeePolicy
        private readonly FeePolicyServices _services;
        private readonly OperationFeeServices _feeServices;
        public FeePolicyController(FeePolicyServices services, OperationFeeServices feeServices = null)
        {
            _services = services;
            _feeServices = feeServices;
        }

        public async Task<ActionResult> Index()
        {
            var fees = await _feeServices.GetFees();
            ViewBag.Fees = fees;
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Create(FeePolicy model)
        {
            if (model.Id== null)
            {
                var data = await _services.Create(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else
            {
                return await Update(model);
            }
        }
        [HttpPost]
        public async Task<ActionResult> Update(FeePolicy model)
        {
            var data = await _services.Update(model);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
        {
            if (path == "list")
            {
                var data = await _services.GetFeePolicys();
                return PartialView(partialView, data);
            }

            else if (path == "new")
            {
                var fees = await _feeServices.GetFees();
                ViewBag.Fees = fees;
                return PartialView(partialView, new FeePolicy());
            }
            else
            {
                ViewBag.Key = KEY;
                var fees = await _feeServices.GetFees();
                ViewBag.Fees = fees;
                var Guaranty = await _services.GetFeePolicy(KEY);
                return PartialView(partialView, Guaranty);

            }
        }
        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _services.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetFee(string Key)
        {
            var data = await _feeServices.GetFee(Key);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }

}