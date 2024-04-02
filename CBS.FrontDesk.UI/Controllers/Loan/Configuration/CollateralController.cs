
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
    public class CollateralController : BaseController
    {
        // GET: Collateral
        private readonly CollateralServices _CollateralServices;
        public CollateralController(CollateralServices CollateralServices)
        {
            _CollateralServices = CollateralServices;
        }

        public async Task<ActionResult> Index()
        {

            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Create(Collateral model)
        {
            if (model.id==null)
            {
                if (ModelState.IsValid)
                {
                    var data = await _CollateralServices.Create(model);
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
        public async Task<ActionResult> Update(Collateral model)
        {
            if (ModelState.IsValid)
            {
                var data = await _CollateralServices.Update(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
        {
            ViewBag.Key = null;
            if (path == "list")
            {
                var data = await _CollateralServices.GetCollaterals();
                return PartialView(partialView, data);
            }

            else if (path == "new")
            {
                return PartialView(partialView, new Collateral());
            }
            else
            {
                ViewBag.Key = KEY;
                var Collateral = await _CollateralServices.GetCollateral(KEY);
                return PartialView(partialView, Collateral);

            }
        }

        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _CollateralServices.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
    }
}