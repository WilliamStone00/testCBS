using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.CashCeilingManagement;
using CBS.FrontDesk.Data.Entity.CashChangeManagement;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Message;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.TransactionManagement
{
    [CheckSessionTimeOutAttribute]

    public class CashChangeManagementController : BaseController
    {
        // GET: CashChangeManagement
        private readonly CashChangeHistoryServices _services;
        private readonly BranchServices _branchServices;
        private readonly DailyTellerServices _dailyTellerServices;

        public CashChangeManagementController(CashChangeHistoryServices services, BranchServices branchServices = null, DailyTellerServices dailyTellerServices = null)
        {
            _services = services;
            _branchServices = branchServices;
            _dailyTellerServices=dailyTellerServices;
        }

        public ActionResult Index()
        {
            //await GetValues();
            return View();
        }
        public ActionResult ChangeDenominationVault()
        {
            ViewBag.Context="vault";
            //var dailyTeller = await _dailyTellerServices.GetDailyTellerUser("Primary");
            //if (dailyTeller == null)
            //{
            //    ViewBag.message = "Access Denied: You are not assigned to operate a till for today. Please contact your administrator for assistance.";
            //    return View(new CashChangeHistory());
            //}
            //else if (dailyTeller.IsPrimary)
            //{
            //    ViewBag.RequestType = "Cash_To_Vault";
            //    return View(new CashChangeHistory());
            //}

            //ViewBag.message = "Access Denied: Only primary till operators can initiate a cash transfer to the vault. Please contact your administrator if you believe this is an error.";
            return View(new CashChangeHistory());
        }

        public ActionResult ChangeDenominationSubTeller()
        {
            ViewBag.Context="sub_teller";
            return View(new CashChangeHistory());
        }
        public ActionResult ChangeDenominationPrimaryTeller()
        {
            ViewBag.Context="primary_teller";
            return View(new CashChangeHistory());
        }


        [HttpPost]
        public async Task<ActionResult> Create(CashChangeHistory model)
        {
            // Validate the model state
            if (!ModelState.IsValid)
            {
                // Construct a detailed validation error message
                var errorMessages = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                // If model validation fails, return detailed validation errors as JSON response
                return Json(new { success = false, message = "Validation failed: " + errorMessages });
            }

            var data = await _services.Create(model);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });


        }




        //public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string branchid=null, string queryoption=null,string serviceOption=null)
        //{
        //    if (path == "list")
        //    {
        //        var query = new GetAllCashCeilingRequestsQuery { BranchId=branchid , Status=queryoption };
        //        var data = await _services.GetCashCeilingRequests(query);


        //        return PartialView(partialView, data);
        //    }
        //    else if (path == "initiator_data")
        //    {
              
        //        var userid = Session["UserID"].ToString();
        //        var data = await _services.GetCashCeilingRequests(userid, null, serviceOption);
          

        //        return PartialView(partialView, data);
        //    }
        //    else if (path == "new")
        //    {
        //        await GetValues();
        //        return PartialView(partialView, new CashCeilingRequest());
        //    }
        //    else
        //    {
        //        await GetValues();
        //        ViewBag.Key = KEY;
        //        var CashCeilingRequest = await _services.GetCashCeilingRequest(KEY);
        //        CashCeilingRequest.AddCashCeilingRequestCommand=new AddCashCeilingRequestCommand { CashoutRequestAmount=CashCeilingRequest.CashoutRequestAmount, Id=CashCeilingRequest.Id, Requetcomment=CashCeilingRequest.Requetcomment, RequestType=CashCeilingRequest.RequestType };
        //        return PartialView(partialView, CashCeilingRequest);

        //    }
        //}
      
       
    }

}