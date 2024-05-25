
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

    public class SavingProductFeeController : BaseController
    {
        // GET: SavingProductFee
        private readonly SavingProductFeeServices _services;
        private readonly OperationFeeServices _feeServices;

        public SavingProductFeeController(SavingProductFeeServices services, OperationFeeServices feeServices = null)
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
        public async Task<ActionResult> Create(SavingConfiguration model)
        {
            if (model.SavingProductFee.Id== null)
            {
                var data = await _services.Create(model.SavingProductFee);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else
            {
                return await Update(model.SavingProductFee);
            }
        }
        [HttpPost]
        public async Task<ActionResult> Update(SavingProductFee model)
        {
            var data = await _services.Update(model);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
        {
            if (path == "list")
            {
                var data = await _services.GetSavingProductFees();
                return PartialView(partialView, new SavingConfiguration { SavingProductFees=data.ToList()});
            }
            else if (path == "savingFeeMapping")
            {
                var data = await _services.GetSavingProductFees(KEY);
                return PartialView(partialView, new SavingConfiguration { SavingProductFees = data.ToList() });
            }

            else if (path == "new")
            {
                var fees = await _feeServices.GetFees();
                ViewBag.Fees = fees;
                return PartialView(partialView, new SavingConfiguration { });
            }
            else
            {
                ViewBag.Key = KEY;
                var fees = await _feeServices.GetFees();
                ViewBag.Fees = fees;
                var Guaranty = await _services.GetSavingProductFee(KEY);
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