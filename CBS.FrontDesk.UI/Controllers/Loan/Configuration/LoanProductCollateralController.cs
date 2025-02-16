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

    public class LoanProductCollateralController : BaseController
    {
        // GET: LoanProductCollateral
        private readonly LoanProductCollateralServices _LoanProductCollateralServices;
        private readonly CollateralServices _collateralServices;
        private readonly LoanProductServices _loanProductServices;
        public LoanProductCollateralController(LoanProductCollateralServices LoanProductCollateralServices, CollateralServices collateralServices = null, LoanProductServices loanProductServices = null)
        {
            _LoanProductCollateralServices = LoanProductCollateralServices;
            _collateralServices = collateralServices;
            _loanProductServices = loanProductServices;
        }

        public async Task<ActionResult> Index()
        {
            ViewBag.Products = await _loanProductServices.GetStringValuesAsync();
            ViewBag.Collaterals = await _collateralServices.GetCollaterals();
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Create(LoanProductCollateral model)
        {
            if (model.Id==null)
            {
                if (ModelState.IsValid)
                {
                    var data = await _LoanProductCollateralServices.Create(model);
                    return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
                }
            }
            else
            {
                return await Update(model);
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }
        [HttpPost]
        public async Task<ActionResult> Update(LoanProductCollateral model)
        {
            var data = await _LoanProductCollateralServices.Update(model);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
        {
            ViewBag.Key = null;
            if (path == "list")
            {
                var data = await _LoanProductCollateralServices.GetLoanProductCollaterals();
                return PartialView(partialView, data);
            }

            else if (path == "new")
            {
                ViewBag.Products = await _loanProductServices.GetLoanProducts();
                ViewBag.Collaterals = await _collateralServices.GetCollaterals();
                return PartialView(partialView, new LoanProductCollateral());
            }
            else
            {
                ViewBag.Products = await _loanProductServices.GetLoanProducts();
                ViewBag.Collaterals = await _collateralServices.GetCollaterals();
                ViewBag.Key = KEY;
                var LoanProductCollateral = await _LoanProductCollateralServices.GetLoanProductCollateral(KEY);
                return PartialView(partialView, LoanProductCollateral);

            }
        }

        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _LoanProductCollateralServices.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
    }
}