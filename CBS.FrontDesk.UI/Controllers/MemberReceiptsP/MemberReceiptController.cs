using CBS.BusinessService;
using CBS.BusinessService.Accounts.MemberReceiptsP;
using CBS.BusinessService.Config;
using CBS.BusinessService.CustomerManagement;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.CMoney;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountOperation;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.UI.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static CBS.FrontDesk.Data.Entity.SavingProducts.AccountOperation.PaymentReceipt;

namespace CBS.FrontDesk.UI.Controllers.MemberReceiptsP
{
    //[CheckSessionTimeOutAttribute]
  
    public class MemberReceiptController : BaseController
    {
        private readonly MemberReceiptServices _services;
        private readonly BranchServices _branchServices;

        /// <summary>
        /// Initializes the <see cref="MemberReceiptController"/> with required services.
        /// </summary>
        public MemberReceiptController(MemberReceiptServices services, BranchServices branchServices)
        {
            _services = services;
            _branchServices = branchServices;
        }

        public async Task<ActionResult> Index()
        {
            var branches = await _branchServices.GetBranches();
            ViewBag.Branches = branches;
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> LoadDatatable(GetPaymentReceiptsDataTableQuery query)
        {
            try
            {
                var dataTable = await _services.GetDataTableAsync(query);
                var list = JsonConvert.DeserializeObject<List<PaymentReceipt>>(
                    JsonConvert.SerializeObject(dataTable.data)
                );

                return Json(new
                {
                    draw = query.Options.draw,
                    recordsTotal = dataTable.recordsTotal,
                    recordsFiltered = dataTable.recordsFiltered,
                    data = list
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, $"Server error: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<ActionResult> Download(GetPaymentReceiptsDataTableQuery query)
        {
            try
            {
                query.Options = new DataTableOptions
                {
                    pageSize = 10000,
                    start = 0,
                };

                var dataTable = await _services.GetDataTableAsync(query);
                var receiptList = JsonConvert.DeserializeObject<List<PaymentReceipt>>(
                    JsonConvert.SerializeObject(dataTable.data)
                );

                string exportedBy = Session["FullName"]?.ToString() ?? "System Export";
                return null;
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Error exporting payment receipts.");
            }
        }

        /// <summary>
        /// 📄 View detailed payment receipt by ID.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> ViewReceipt(string id)
        {
            try
            {
                var receipt = await _services.GetPaymentReceipt(id);
                if (receipt == null || string.IsNullOrWhiteSpace(receipt.Id))
                    return HttpNotFound("Payment receipt not found.");
                return PartialView("_ViewReceipt", receipt);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Error loading payment receipt.");
            }
        }

        /// <summary>
        /// 🔍 AJAX load all receipts for a specific member.
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> LoadReceiptsByMember(string memberReference)
        {
            try
            {
                var receipts = await _services.GetPaymentReceiptsByMemberReference(memberReference);
                return Json(receipts, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error loading receipts." }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 🔍 AJAX load receipts by member and date range.
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> LoadReceiptsByMemberAndDate(string memberReference, DateTime startDate, DateTime endDate)
        {
            try
            {
                var receipts = await _services.GetMemberReceiptbyMemberAndDateRanges(memberReference, startDate, endDate);
                return Json(receipts, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error loading receipts by date." }, JsonRequestBehavior.AllowGet);
            }
        }
    }

}