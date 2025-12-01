using CBS.BusinessService.Accounting_V2.BranchAccountService;
using CBS.BusinessService.Accounting_V2.ExportReports;
using CBS.BusinessService.Accounting_V2.TrialBalance;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Queries;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Reporting.FlatBaseE;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.ReportDataSetDto;
using CBS.FrontDesk.UI.AppFiles.Reporting.Accounting;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using CrystalDecisions.Web;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Accounting_V2.TrialBalance
{
    public class TrialBalance6ColumnsController : BaseController
    {
        private readonly TrialBalances6ColumnService _trialBalanceService;
        private readonly BranchAccountService _branchAccountService;
        private readonly BranchServices _branchServices;
        private readonly TrialBalance6ColumnsExport _repoExcel;

        public TrialBalance6ColumnsController(
            TrialBalances6ColumnService trialBalanceService,
            BranchServices branchServices,
            TrialBalance6ColumnsExport repoexcel,
            BranchAccountService branchAccountService)
        {
            _branchServices = branchServices;
            _trialBalanceService = trialBalanceService;
            _repoExcel = repoexcel;
            _branchAccountService = branchAccountService;
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> GenerateTrialBalance(AccountingV2ReportsFilter model)
        {
            try
            {
                // Always reset the report source at the beginning
                HttpContext.Session["rptSource"] = null;

                // ─────────────────────────────────────────────
                // 1) Load trial balance (6 columns)
                // ─────────────────────────────────────────────
                var response = await _trialBalanceService.BuildTrialBalanceDataset(model);
                

                if (response == null || !response.Any())
                {
                    return Json(
                        new { success = false, message = "No records found for the selected filters." },
                        JsonRequestBehavior.AllowGet);
                }



               


             
                    string fileName = $"TrialBalance_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                    string directoryPath = Server.MapPath("~/TempFiles");

                    // Check if directory exists before creating it
                    if (!Directory.Exists(directoryPath))
                        Directory.CreateDirectory(directoryPath);

                    string fullPath = Path.Combine(directoryPath, fileName);

                    _repoExcel.ExportTb6(response, fullPath, _trialBalanceService.GetUserFullName());

               

                // ─────────────────────────────────────────────
                // 5) Push prepared dataset to session for Crystal
                // ─────────────────────────────────────────────
                HttpContext.Session["rptSource"] = response;

                return Json(
                    new { success = true, message = "Trial balance report ready.", downloadFile = fileName },
                    JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // You can plug in your logger here if needed
                return Json(
                    new { success = false, message = ex.Message },
                    JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult Download(string file)
        {
            string fullPath = Path.Combine(Server.MapPath("~/TempFiles"), file);
            byte[] fileBytes = System.IO.File.ReadAllBytes(fullPath);
            System.IO.File.Delete(fullPath);

            return File(fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                file);
        }


        [HttpPost]
        public ActionResult GetReport(string path)
        {
            // Path pointing to the .rpt file for Crystal Reports
            var reportPath = Server.MapPath("~/AppFiles/Accountingv2Reporting/ReportRPT/TrialBalance6Columns.rpt");

            if (!System.IO.File.Exists(reportPath))
                return Json(new { success = false, message = "Report template file missing." }, JsonRequestBehavior.AllowGet);

            this.HttpContext.Session["rptType"] = "ReportParameterLess";
            this.HttpContext.Session["ReportName"] = "TrialBalance6Columns.rpt";
            this.HttpContext.Session["rptpath"] = "~/AppFiles/Accountingv2Reporting/ReportRPT/TrialBalance6Columns.rpt";
            this.HttpContext.Session["rpttitle"] = "TB6";

            return Json(new { success = true, message = "Report parameters set successfully." }, JsonRequestBehavior.AllowGet);
        }

        // Map bank and branch header details into each report row
        private void ApplyHeader(TrialBalanceReportItem item, BankHeaderInformation header)
        {
            item.BankId = header.BankId;
            item.BankBankCode = header.BankBankCode;
            item.BankName = header.BankName;
            item.BankTelephone = header.BankTelephone;
            item.BankEmail = header.BankEmail;
            item.BankAddress = header.BankAddress;
            item.BankLogoUrl = header.BankLogoUrl;
            item.BankMotto = header.BankMotto;
            item.BankRegistrationNumber = header.BankRegistrationNumber;
            item.BankImmatriculationNumber = header.BankImmatriculationNumber;
            item.BankPBox = header.BankPBox;

            item.BranchId = header.BranchId;
            item.BranchCode = header.BranchCode;
            item.BranchName = header.BranchName;
            item.BranchTelephone = header.BranchTelephone;
            item.BranchEmail = header.BranchEmail;
            item.BranchAddress = header.BranchAddress;
            item.BranchLogoUrl = header.BranchLogoUrl;
            item.BranchCapital = header.BranchCapital;
            item.BranchRegistrationNumber = header.BranchRegistrationNumber;
            item.BranchImmatriculationNumber = header.BranchImmatriculationNumber;
            item.BranchPBox = header.BranchPBox;
        }
    }
}
