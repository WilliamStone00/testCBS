using CBS.BusinessService.AccountingV2.GLSystemReconciliation;
using CBS.BusinessService.AccountingV2.InterestProductConfig;
using CBS.BusinessService.Config;
using CBS.BusinessService.LoanP.Repayment;
using CBS.BusinessService.Repayment;
using CBS.FrontDesk.Data.Entity.AccountingV2.GLSystemReconciliation;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.LoanRepayment;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Refundement
{
    public class RefundController : Controller
    {
        private readonly InterestProductConfigService _interestProductConfigService;
        private readonly RefundServices _refundServices;
        private readonly BranchServices _branchServices;


        public RefundController(RefundServices refundServices, BranchServices branchServices, InterestProductConfigService interestProductConfigService)
        {
            _refundServices = refundServices;
            _branchServices = branchServices;
            _interestProductConfigService = interestProductConfigService;
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







        public async Task<JsonResult> PushRefund([FromBody] List<PartialBulkOperation> request)
        {
            if (request == null || !request.Any())
            {
                return Json(new { success = false, message = "No refund operations received." });
            }

            var result = await _refundServices.PushRefundsAsync(request);

            return Json(new
            {
                success = result.IsSuccess,
                message = result.IsSuccess
                    ? "Refunds pushed successfully."
                    : "Failed to push refunds."
            });
        }


        public async Task<JsonResult> GetProducts()
        {
            try
            {
                var products = await _interestProductConfigService.GetProductAsync();

                if (products == null || !products.Any())
                {
                    return Json(new
                    {
                        success = false,
                        message = "No products found."
                    }, JsonRequestBehavior.AllowGet); // <-- allow GET
                }

                var productDropdown = products
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id,
                        Text = p.Name
                    })
                    .OrderBy(x => x.Text)
                    .ToList();

                return Json(new
                {
                    success = true,
                    data = productDropdown
                }, JsonRequestBehavior.AllowGet); // <-- allow GET
            }
            catch (Exception ex)
            {
                // Log exception if needed
                return Json(new
                {
                    success = false,
                    statusCode = 500,
                    message = $"Failed to get products: {ex.Message}"
                }, JsonRequestBehavior.AllowGet); // <-- allow GET
            }
        }



        public async Task<ActionResult> ExportRefundData(ExportRefundRequest request)
        {
            try
            {
                // ✅ Build LoanRefundQuery from Filters
                var query = new LoanRefundQuery
                {
                    DataTableOptions = new DataTableOptions
                    {
                        draw = "1",
                        start = 0,
                        length = int.MaxValue // fetch all filtered rows
                    },

                    BranchId = request.Filters?.BranchId,
                    LoanId = request.Filters?.LoanId,
                    CustomerId = request.Filters?.CustomerId,
                    TransactionCode = request.Filters?.TransactionCode,
                    PaymentChannel = request.Filters?.PaymentChannel,
                    PaymentMethod = request.Filters?.PaymentMethod,
                    IsCompleted = request.Filters?.IsCompleted ?? false,
                    IsReversal = request.Filters?.IsReversal ?? false,
                    StartDate = request.Filters?.StartDate,
                    EndDate = request.Filters?.EndDate,
                    CheckPartialFlowRepayment = request.Filters?.CheckPartialFlowRepayment
                };

                // ✅ Fetch data from service
                var data = await _refundServices.GeRefundDataTableAsync(query);

                var refundList = JsonConvert.DeserializeObject<List<LoanRefundDto>>(
                    JsonConvert.SerializeObject(data.data));

                

                // ✅ Convert data for Excel
                var refundDataForExcel =
                    RefundDataTableExcelExportGenerator.ConvertToRefundData(refundList);

                if (!refundDataForExcel.Any())
                    return Json(new { success = false, message = "No valid refund data could be processed for export." });

                // ✅ Prepare file name
                string timestamp = DateTime.Now.ToString("ddMMyyyyHHmmss");
                string fileName = $"{request.ExportOptions?.FileName ?? "Refund_Report"}_{timestamp}.xlsx";
                string directoryPath = Server.MapPath("~/TempFiles");

                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);

                string filePath = Path.Combine(directoryPath, fileName);
                string exportedBy = Session["FullName"]?.ToString() ?? "System";

                // ✅ Generate Excel
                var exportGenerator = new RefundDataTableExcelExportGenerator();
                exportGenerator.GenerateRefundExcelFromTableData(
                    refundDataForExcel,
                    filePath,
                    exportedBy,
                    request.ExportOptions
                );

                if (!System.IO.File.Exists(filePath))
                    return Json(new { success = false, message = "Failed to generate Excel file." });

                // ✅ Return file
                byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                System.IO.File.Delete(filePath);

                return File(
                    fileBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName
                );
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"An error occurred while exporting refund data: {ex.Message}"
                });
            }
        }

    }
}
