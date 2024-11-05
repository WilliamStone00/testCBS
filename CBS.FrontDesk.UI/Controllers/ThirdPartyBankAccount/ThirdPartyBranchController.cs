using CBS.BusinessService.Accounting;
using CBS.BusinessService.Config;
using CBS.BusinessService;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data;
using CBS.FrontDesk.UI.Models;
using ClosedXML.Excel;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using CBS.BusinessService.Services;
 
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.BusinessService.ThirdPartyBankAccount;

namespace CBS.FrontDesk.UI.Controllers.ThirdPartyBankAccount
{
 

    [CheckSessionTimeOutAttribute]
 
    public class ThirdPartyBranchController : BaseController
    {
        // GET: Collateral
        private readonly ThirdPartyBranchServices _ThirdPartyBranchServices;
        public ThirdPartyBranchController(ThirdPartyBranchServices CollateralServices)
        {
            _ThirdPartyBranchServices = CollateralServices;
        }

        public async Task<ActionResult> Index()
        {

            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Create(ThirdPartyBranch model)
        {
            if (model.Id == null)
            {
                if (ModelState.IsValid)
                {
                    var data = await _ThirdPartyBranchServices.Create(model);
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
        public async Task<ActionResult> Update(ThirdPartyBranch model)
        {
            if (ModelState.IsValid)
            {
                var data = await _ThirdPartyBranchServices.Update(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
        {
            ViewBag.Key = null;
            if (path == "list")
            {
                var data = await _ThirdPartyBranchServices.GetThirdPartyBranch(KEY);
                return PartialView(partialView, data);
            }

            else if (path == "new")
            {
                return PartialView(partialView, new Collateral());
            }
            else
            {
                ViewBag.Key = KEY;
                var Collateral = await _ThirdPartyBranchServices.GetThirdPartyBranch(KEY);
                return PartialView(partialView, Collateral);

            }
        }

        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _ThirdPartyBranchServices.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
    }
}