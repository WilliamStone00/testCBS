using CBS.BusinessService.AccountingV2.GLSystemReconciliation;
using CBS.BusinessService.Config;
using CBS.BusinessService.Repayment;
using CBS.FrontDesk.Data.Entity.AccountingV2.GLSystemReconciliation;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.LoanRepayment;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Refundement
{
    public class RefundController : Controller
    {

        private readonly RefundServices _refundServices;
        private readonly BranchServices _branchServices;


        public RefundController(RefundServices refundServices, BranchServices branchServices)
        {
            _refundServices = refundServices;
            _branchServices = branchServices;
        }
        // GET: Refund
        public async Task<ActionResult> Index()
        {
            await loader();
            return View();
        }

        public async Task<bool> loader()
        {

            var branches = await _branchServices.GetBranches();
            ViewBag.Branches = branches;


            ViewBag.PaymentMethods = new List<SelectListItem>
{
    new SelectListItem { Value = "Cash", Text = "Cash" },
    new SelectListItem { Value = "Cash_Desk_JointDeposit", Text = "Cash Desk Joint Deposit" },
    new SelectListItem { Value = "Back-Office Recovery", Text = "Back-Office Recovery" },
    new SelectListItem { Value = "Salary Deduction", Text = "Salary Deduction" }
};

            // Payment Channels dropdown
            ViewBag.PaymentChannels = new List<SelectListItem>
{
    new SelectListItem { Value = "Web_Portal", Text = "Web Portal" },
    new SelectListItem { Value = "Back-Office loan Repayment", Text = "Back-Office Loan Repayment" },
    new SelectListItem { Value = "Standing Order", Text = "Standing Order" }
};

            return true;
        }

       
        public async Task<JsonResult> LoadRefundmentData(LoanRefundQuery query)
        {
            try
            {
                var data = await _refundServices.GeRefundDataTableAsync(query);

                var Refund = JsonConvert.DeserializeObject<List<LoanRefundDto>>(
                    JsonConvert.SerializeObject(data.data));

                

                return Json(new
                {

                    draw = data.draw ,
                    recordsTotal = data.recordsTotal,
                    recordsFiltered = data.recordsFiltered,
                    data = Refund,
                    success = true,
                    message = " DataTable loaded successfully"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    draw = query?.DataTableOptions?.draw ?? "1",
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new List<object>(),
                    error = ex.Message
                });
            }
        }


 


      
        public async Task<JsonResult> PushRefund([FromBody] PushRefundRequest request)
        {
            if (request?.Ids == null || request.Ids.Count == 0)
            {
                return Json(new { success = false, message = "No refund IDs received." });
            }

            try
            {
                // Call your service to process refunds
                var result = await _refundServices.PushRefundsAsync(request.Ids);

                if (result.IsSuccess)
                {
                    return Json(new { success = true, message = "Refunds pushed successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to push refunds." });
                }
            }
            catch (Exception ex)
            {
                // Log exception
                return Json(new
                {
                    success = false,
                    statusCode = 500,
                    message = $"Push record failed: {ex.Message}"
                });
            }
        }
    }
}
