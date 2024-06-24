
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

    public class FeeRangeController : BaseController
    {
        // GET: Guaranty
        private readonly FeeRangeServices _services;
        private readonly FeeServices _feeServices;
        public FeeRangeController(FeeRangeServices services, FeeServices feeServices = null)
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
        public async Task<ActionResult> Create(FeeRange model)
        {
            if (model.Id == null)
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
        public async Task<ActionResult> Update(FeeRange model)
        {
            var data = await _services.Update(model);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
        {

            if (path == "list")
            {
                var data = await _services.GetFeeRanges();
                return PartialView(partialView, data);
            }

            else if (path == "new")
            {
                var fees = await _feeServices.GetFees();
                ViewBag.Fees = fees;
                return PartialView(partialView, new FeeRange());
            }
            else
            {
                var fees = await _feeServices.GetFees();
                ViewBag.Fees = fees;
                ViewBag.Key = KEY;
                var Guaranty = await _services.GetFeeRange(KEY);
                ViewBag.Base = Guaranty.Fee.FeeBase;
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