using CBS.BusinessService.Accounts;
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

    //[CheckSessionTimeOutAttribute]

    public class TransactionReversalController : BaseController
    {
        // GET: TransactionReversal
        private readonly TransactionResversalServices _services;

        public TransactionReversalController(TransactionResversalServices services)
        {
            _services = services;
        }
        //Validation
        public async Task<ActionResult> Index()
        {
            return View();
        }
        //InitiatedPendingRequest
        public async Task<ActionResult> PendingRequest()
        {
            var reversalRequestQuery = new GetAllReversalRequestQuery { BranchId = Session["BranchID"].ToString(), DateFrom = null, DateTo = null, IsBranch = true, IsByDate = false, QueryString = "Pending" };
            var pending = await _services.GetReversalRequests(reversalRequestQuery);
            var transactionReversal = new TransactionReversal { ReversalRequests = pending.ToList() };
            return View(transactionReversal);
        }
        public async Task<ActionResult> PendingApprovals()
        {
            var reversalRequestQuery = new GetAllReversalRequestQuery { BranchId = Session["BranchID"].ToString(), DateFrom = null, DateTo = null, IsBranch = true, IsByDate = false, QueryString = "Validated" };
            var pending = await _services.GetReversalRequests(reversalRequestQuery);
            var transactionReversal = new TransactionReversal { ReversalRequests = pending.ToList() };
            return View(transactionReversal);
        }
        public async Task<ActionResult> InitiatedPendingRequest()
        {
            var reversalRequestQuery = new GetAllReversalRequestQuery { BranchId = Session["BranchID"].ToString(), DateFrom = null, DateTo = null, IsBranch = true, IsByDate = false, QueryString = "Pending_Validated_Approved" };
            var pending = await _services.GetReversalRequests(reversalRequestQuery);
            var transactionReversal = new TransactionReversal { ReversalRequests = pending.ToList() };
            return View(transactionReversal);
        }
        public async Task<ActionResult> RequestValidationForm(string key)
        {
            var transactionReversal = new TransactionReversal();
            var reversalRequest = await _services.GetReversalRequest(key);
            transactionReversal.ReversalRequest = reversalRequest;
            transactionReversal.Transactions = reversalRequest.Transactions;
            transactionReversal.ValidationReversalRequestCommand = new ValidationReversalRequestCommand { Id = key, Status = "Validated", ValidationComment = $"Validated the transaction reversal request; all checks completed successfully, and the request is validated by {Session["FullName"]}" };
            return View(transactionReversal);
        }
        public async Task<ActionResult> RequestTreatmentForm(string key)
        {
            var reversalRequest = await _services.GetReversalRequest(key);
            var transactionReversal = new TransactionReversal();
            transactionReversal.CashCompletionOfReversalCommand = new CashCompletionOfReversalCommand { Id = key, };
            transactionReversal.ReversalRequest = reversalRequest;
            transactionReversal.ReversalRequest = reversalRequest;
            transactionReversal.Transactions = reversalRequest.Transactions;
            return View(transactionReversal);
        }
        //public async Task<ActionResult> PendingApprovals()
        //{
        //    var reversalRequestQuery = new GetAllReversalRequestQuery { BranchId = Session["BranchID"].ToString(), DateFrom = null, DateTo = null, IsBranch = true, IsByDate = false, QueryString = "Validated" };
        //    var pending = await _services.GetReversalRequests(reversalRequestQuery);
        //    return View(pending.ToList());
        //}
        public async Task<ActionResult> RequestApprovalForm(string key)
        {
            var transactionReversal = new TransactionReversal();
            var reversalRequest = await _services.GetReversalRequest(key);
            transactionReversal.ReversalRequest = reversalRequest;
            transactionReversal.Transactions = reversalRequest.Transactions;
            transactionReversal.ApprovedReversalRequestCommand = new ApprovedReversalRequestCommand { Id = key, Status = "Validated", ApprovedComment = $"Approved. All checks completed successfully, and the request is Approved by {Session["FullName"]}" };

            return View(transactionReversal);
        }
        public async Task<ActionResult> RequestReversalForm()
        {
            return View(new TransactionReversal());
        }
        [HttpPost]
        public async Task<ActionResult> Create(TransactionReversal model)
        {
            if (model.Option == "create")
            {
                var data = await _services.Create(model.AddReversalRequestCommand);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }
            else if(model.Option == "validate")
            {
                var data = await _services.Validate(model.ValidationReversalRequestCommand);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }
            else if (model.Option == "approved")
            {
                var data = await _services.Approve(model.ApprovedReversalRequestCommand);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }
            else if (model.Option == "treatement")
            {
                var data = await _services.TreateTransaction(model.CashCompletionOfReversalCommand);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }
            return Json(new { success = false, message = "Invalid option." });

        }
       
        //public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string dateFrom = null, string dateTo = null)
        //{
        //    if (path == "search")
        //    {
        //        if (KEY != string.Empty)
        //        {
        //            var data = await _services.GetCashReplenishmentSubTellers(_services.GetDateTime(dateFrom), _services.GetDateTime(dateTo), KEY);
        //            return PartialView(partialView, data.ToList());

        //        }
        //        else
        //        {
        //            var data = await _services.GetCashReplenishmentSubTellers(_services.GetDateTime(dateFrom), _services.GetDateTime(dateTo));
        //            return PartialView(partialView, data.ToList());

        //        }
        //    }

        //    else if (path == "new")
        //    {
        //        return PartialView(partialView, new CashReplenishmentSubTeller());
        //    }
        //    else
        //    {
        //        var Guaranty = await _services.GetCashReplenishmentSubTeller(KEY);
        //        return PartialView(partialView, Guaranty);

        //    }
        //}
        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _services.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }


    }

}