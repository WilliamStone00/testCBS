using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.CashCeilingManagement;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Message;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.TransactionManagement
{
    //[CheckSessionTimeOutAttribute]

    public class CashCeilingRequestController : BaseController
    {
        // GET: CashCeilingRequest
        private readonly CashCeilingRequestServices _services;
        private readonly BranchServices _branchServices;
        private readonly DailyTellerServices _dailyTellerServices;

        public CashCeilingRequestController(CashCeilingRequestServices services, BranchServices branchServices = null, DailyTellerServices dailyTellerServices = null)
        {
            _services = services;
            _branchServices = branchServices;
            _dailyTellerServices=dailyTellerServices;
        }

        public ActionResult Index()
        {
            //await GetValues();
            return View(new CashCeilingRequest());
        }
        public async Task<ActionResult> CashToVault()
        {
            var dailyTeller = await _dailyTellerServices.GetDailyTellerUser("Primary");
            if (dailyTeller == null)
            {
                ViewBag.message = "Access Denied: You are not assigned to operate a till for today. Please contact your administrator for assistance.";
                return View(new CashCeilingRequest());
            }
            else if (dailyTeller.IsPrimary)
            {
                ViewBag.RequestType = "Cash_To_Vault";
                return View(new CashCeilingRequest());
            }

            ViewBag.message = "Access Denied: Only primary till operators can initiate a cash transfer to the vault. Please contact your administrator if you believe this is an error.";
            return View(new CashCeilingRequest());
        }

        public async Task<ActionResult> SubTillToPrimaryTill()
        {
            var dailyTeller = await _dailyTellerServices.GetDailyTellerUser("Sub-Teller");
            if (dailyTeller == null)
            {
                ViewBag.message = "Access Denied: You are not assigned to operate a till for today. Please contact your administrator for assistance.";
                return View(new CashCeilingRequest());
            }
            else if (!dailyTeller.IsPrimary)
            {
                ViewBag.RequestType = "Subteller_Cash_To_PrimaryTeller";
                return View(new CashCeilingRequest());
            }

            ViewBag.message = "Access Denied: Only sub-till operators are authorized to transfer cash to the primary till. Please contact your administrator if assistance is required.";
            return View(new CashCeilingRequest());
        }



        public async Task<ActionResult> RequestValidation(string Key, string RequestType)
        {
            var cashCeilingRequest = await _services.GetCashCeilingRequest(Key);

            // Retrieve the user name from the session
            var userName = Session["FullName"]?.ToString() ?? "Unknown User";

            // GetAllowAnonymous the current date and time
            var validationDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            cashCeilingRequest.ValidationCashCeilingRequestCommand=new ValidationCashCeilingRequestCommand { Amount=cashCeilingRequest.CashoutRequestAmount, Id=cashCeilingRequest.Id, CurrencyNote=new Data.Entity.Accounting.CurrencyNotesRequest() };
            // Set ApprovedComment based on the RequestType
            switch (RequestType)
            {
                case "Cash_To_Vault":
                    cashCeilingRequest.ValidationCashCeilingRequestCommand.ApprovedComment =
                        $"Approved: Cash transfer to the vault by {cashCeilingRequest.RequestedBy} authorized to secure excess cash and maintain branch compliance. Validated by {userName} on {validationDate}.";
                    break;

                case "Subteller_Cash_To_PrimaryTeller":
                    cashCeilingRequest.ValidationCashCeilingRequestCommand.ApprovedComment =
                        $"Approved: Cash transfer from sub-till by {cashCeilingRequest.RequestedBy} to primary till  authorized as the sub-till has reached its maximum cash limit. This ensures smooth operations and compliance with cash management policies. Validated by {userName} on {validationDate}.";
                    break;

                default:
                    cashCeilingRequest.ValidationCashCeilingRequestCommand.ApprovedComment =
                        $"Approved: Request processed. Validated by {userName} on {validationDate}.";
                    break;
            }

            return View(cashCeilingRequest);
        }


        public async Task<ActionResult> InitiatorListing()
        {
            var userid = Session["UserID"].ToString();
            var data = await _services.GetCashCeilingRequests(userid,null,null);
            return View(data);
        }
        public async Task<ActionResult> Validation()
        {
            var data = await _services.GetCashCeilingRequests(null,"Pending",null);
            return View(data);
        }
        public ActionResult CashCeilingRequests()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Create(CashCeilingRequest model)
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

            // Handle the Create action
            if (model.Action == "Create")
            {
                var data = await _services.Create(model.AddCashCeilingRequestCommand);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }
            // Handle the Validate Request action
            else if (model.Action == "validate_request")
            {
                var data = await _services.ValidateRequest(model.ValidationCashCeilingRequestCommand);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }
            // Handle the Update action
            else
            {
                return await Update(model);
            }
        }




        [HttpPost]
        public async Task<ActionResult> Update(CashCeilingRequest model)
        {
            var data = await _services.Update(model.AddCashCeilingRequestCommand);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
        }
        private async Task GetValues()
        {
            var branches = await _branchServices.GetBranches();
            ViewBag.Branches = branches;
        }
        //subtilltoprimarytill
        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string branchid=null, string queryoption=null,string serviceOption=null)
        {
            if (path == "list")
            {
                var query = new GetAllCashCeilingRequestsQuery { BranchId=branchid , Status=queryoption };
                var data = await _services.GetCashCeilingRequests(query);


                return PartialView(partialView, data);
            }
            else if (path == "initiator_data")
            {
              
                var userid = Session["UserID"].ToString();
                var data = await _services.GetCashCeilingRequests(userid, null, serviceOption);
          

                return PartialView(partialView, data);
            }
            else if (path == "new")
            {
                await GetValues();
                return PartialView(partialView, new CashCeilingRequest());
            }
            else
            {
                await GetValues();
                ViewBag.Key = KEY;
                var CashCeilingRequest = await _services.GetCashCeilingRequest(KEY);
                CashCeilingRequest.AddCashCeilingRequestCommand=new AddCashCeilingRequestCommand { CashoutRequestAmount=CashCeilingRequest.CashoutRequestAmount, Id=CashCeilingRequest.Id, Requetcomment=CashCeilingRequest.Requetcomment, RequestType=CashCeilingRequest.RequestType };
                return PartialView(partialView, CashCeilingRequest);

            }
        }
        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _services.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetCashCeilingPartialView(string Key)
        {
            var data = await _services.GetCashCeilingRequest(Key);
            if (data == null)
            {
                return HttpNotFound();
            }

            return PartialView("_CeilingRequestDetailsPartial", data);
        }
        public async Task<ActionResult> GetBranch(string Key)
        {
            var branch = await _branchServices.GetBranch(Key);
            return Json(branch, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> Ajaxloader(string Key, string path)
        {
            if (Key != null)
            {
                var listing = await _branchServices.GetBranchesByBankId(Key);
                return Json(listing, JsonRequestBehavior.AllowGet);

            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }
    }

}