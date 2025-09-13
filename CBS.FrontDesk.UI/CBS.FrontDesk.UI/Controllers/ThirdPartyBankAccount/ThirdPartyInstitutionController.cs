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
//using CBS.BusinessService.Services;
//using CBS.BusinessService.ThirdPartyInstitutionAccount;
 
using CBS.BusinessService.ThirdPartyBankAccount;

namespace CBS.FrontDesk.UI.Controllers.ThirdPartyBankAccount
{
 

    //[CheckSessionTimeOutAttribute]
 
    //public class ThirdPartyInstitutionController : BaseController
    //{
    //    // GET: Collateral
    //    private readonly ThirdPartyInstitutionServices _thirdPartyInstitutionServices;
    //    public ThirdPartyInstitutionController(ThirdPartyInstitutionServices CollateralServices)
    //    {
    //        _thirdPartyInstitutionServices = CollateralServices;
    //    }

    //    public async Task<ActionResult> Index()
    //    {

    //        return View();
    //    }
    //    [HttpPost]
    //    public async Task<ActionResult> Create(ThirdPartyInstitution model)
    //    {
    //        if (model.Id == null)
    //        {
    //            if (ModelState.IsValid)
    //            {
    //                var data = await _thirdPartyInstitutionServices.Create(model);
    //                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
    //            }
    //        }
    //        else
    //        {
    //            return await Update(model);
    //        }

    //        return Json(new { success = false, status = false, message = "Fill the required fields." });
    //    }
    //    [HttpPost]
    //    public async Task<ActionResult> Update(ThirdPartyInstitution model)
    //    {
    //        if (ModelState.IsValid)
    //        {
    //            var data = await _thirdPartyInstitutionServices.Update(model);
    //            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
    //        }

    //        return Json(new { success = false, status = false, message = "Fill the required fields." });
    //    }

    //    public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
    //    {
    //        ViewBag.Key = null;
    //        if (path == "list")
    //        {
    //            var data = await _thirdPartyInstitutionServices.GetThirdPartyInstitution(KEY);
    //            return PartialView(partialView, data);
    //        }

    //        else if (path == "new")
    //        {
    //            return PartialView(partialView, new ThirdPartyInstitution());
    //        }
    //        else
    //        {
    //            ViewBag.Key = KEY;
    //            var Collateral = await _thirdPartyInstitutionServices.GetThirdPartyInstitution(KEY);
    //            return PartialView(partialView, Collateral);

    //        }
    //    }

    //    public async Task<ActionResult> Delete(string KEY)
    //    {
    //        var data = await _thirdPartyInstitutionServices.Delete(KEY);
    //        return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
    //    }
    //}
}