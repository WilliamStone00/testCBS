using CBS.BusinessService.Accounting_V2.AccntStatements;
using CBS.BusinessService.Accounting_V2.BalanceSheet;
using CBS.BusinessService.Accounting_V2.JournalEntries;
using CBS.BusinessService.Config;

using CBS.FrontDesk.Data.Entity.Accounting_V2.Queries;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Reporting.FlatBaseE;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.UI.Controllers.Accounting_V2.AccntStatement;
using Microsoft.AspNet.SignalR.Hosting;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Accounting_V2.BalanceSheet
{
    public class BalanceSheetController : BaseController
    {
        private readonly BalanceSheetService _balanceSheetService;
        private readonly BranchServices _branchServices;

        public BalanceSheetController(BalanceSheetService balanceSheetService, BranchServices branchServices)
        {
            _balanceSheetService = balanceSheetService;
            _branchServices = branchServices;
        }

        /// <summary>
        /// Loads the Balance Sheet main page.
        /// </summary>
        public ActionResult Index()
        {
            return View();
        }


        /// <summary>
        /// Generates Balance Sheet report dataset based on user-selected filters.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> GenerateBalanceSheet(AccountingV2ReportsFilter model)
        {
            try
            {
                // ==============================================================
                // STEP 1 — Retrieve balance sheet dataset based on filters
                // ==============================================================           
                var data = await _balanceSheetService.BuildBalanceSheetDataset(model);
                var n = "test";


               if(data == null)
                {
                    this.HttpContext.Session["rptSource"] = null;
                    this.HttpContext.Session["BranchInfo"] = null;
                    return Json(new { success = true, status  = 404, message = "Journal Entries loaded." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    this.HttpContext.Session["rptSource"] = data;
                  
                }



                return Json(new { success = true, message = "Journal Entries loaded." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Setup Crystal Report parameters for the Journal Entries report.
        /// </summary>
        [HttpPost]
        public ActionResult GetReport(string path)
        {
            this.HttpContext.Session["rptType"] = "ReportParameterLess";
            this.HttpContext.Session["ReportName"] = "BalanceSheet.rpt";
            this.HttpContext.Session["rptpath"] = "~/AppFiles/Accountingv2Reporting/ReportRPT/BalanceSheet.rpt";
            this.HttpContext.Session["rpttitle"] = "BS";

            return Json(new
            {
                success = true,
                message = "Report parameters set successfully."
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
