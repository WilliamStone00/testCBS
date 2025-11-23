using CBS.BusinessService.Accounting_V2.AccntStatements;
using CBS.BusinessService.Accounting_V2.BranchAccountService;
using CBS.BusinessService.Accounting_V2.TrialBalance;
using CBS.BusinessService.Config;

using CBS.FrontDesk.Data.Entity.Accounting_V2.Queries;
using CBS.FrontDesk.Data.Entity.Accounting_V2.TrialBalance;
using CBS.FrontDesk.Data.MockData;
using CBS.FrontDesk.Data.ReportDataSetDto;
using CBS.FrontDesk.UI.AppFiles.Reporting.Accounting;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using CrystalDecisions.Web;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Office.Word;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Windows.Forms;

namespace CBS.FrontDesk.UI.Controllers.Accounting_V2.AccntStatement
{

    //[CheckSessionTimeOut]
    public class AccntStatementController : BaseController
    {
        private readonly JournalReceiptsService _accntStatementService;
        private readonly BranchAccountService _branchAccountService;
        private readonly BranchServices _branchServices;
        


        public AccntStatementController(JournalReceiptsService accntStatementService,
            BranchServices branchServices,
            BranchAccountService branchAccountService)
        {
            _branchServices = branchServices;
            _accntStatementService = accntStatementService;
            _branchAccountService = branchAccountService;
        }

        public async Task<ActionResult> Index()
        {
            return View();
        }

        public DateTime GetDayTime(DateTime entryDate, string timeOfOperation)
        {
            // Build full date + time text
            var dateTimeString = $"{entryDate:yyyy-MM-dd} {timeOfOperation}";

            // Convert to DateTime and return
            return DateTime.Parse(dateTimeString);
        }

        public async Task<ActionResult> GenerateReport(AccountingV2ReportsFilter model)
        {
            try
            {
                // 1. Call API
                var response = await _accntStatementService.GetAccntStatement(model);

                // 2. Validate
                if (response == null || !response.Any())
                {
                    Session["rptSource"] = null;
                    Session["BranchInfo"] = null;
                    return Json(new { success = false, message = "No data found for the selected filters." },
                        JsonRequestBehavior.AllowGet);
                }

                // 3. Get branch info
                var BranchInformation = await _branchServices.GetBranch(model.BranchId);

                // 4. Flatten all accounts that have movements
                var data = response
                    .Where(acc => acc.Movements != null && acc.Movements.Any()) // Skip empty accounts
                    .SelectMany(acc => acc.Movements.Select(m => new AccountStatementFlatItems
                    {
                        // Main account fields
                        AccountNumber = acc.AccountNumber,
                        AccountName = m.AccountName,
                        OpeningBalance = acc.OpeningBalance,
                        ClosingBalance = acc.ClosingBalance,

                        // Date
                        AccountingDate = _accntStatementService.FormatDate(m.AccountingDate),
                        From = model.From,
                        To = model.To,
                        Year = DateTime.Now.Year.ToString(),

                        // Time
                        Time = TimeSpan.TryParse(m.TimeOfOperation, out var ts) ? ts : TimeSpan.Zero,
                        DayTime = GetDayTime(m.EntryDate, m.TimeOfOperation),

                        // Movement info
                        ReferenceNumber = m.Reference,
                        BranchId = m.BranchId,
                        CreditAmount = m.CR,
                        DebitAmount = m.DR,
                        Description = m.Narration,
                        BranchName = BranchInformation.Name,
                        BranchCode = BranchInformation.BranchCode,
                        Address = BranchInformation.Address,
                        BranchTel  = BranchInformation.Telephone,
                        HeadOfficePhone = BranchInformation.HeadOfficeTelehoneNumber,
                        BranchEmail = BranchInformation.Email,
                        ImmatriculationNumber = BranchInformation.ImmatriculationNumber,
                        LogoUrl = BranchInformation.LogoUrl,
                        Motto = BranchInformation.Motto,
                        RegistrationNumber = BranchInformation.RegistrationNumber,
                        BankInitial = BranchInformation.BankInitial,
                        PBox =  BranchInformation.PBox,
                        DisplayName= BranchInformation.DisplayName,
                        DrCr = m.DrCr,
                        Amount = m.Amount,
                        Balance = m.Balance,
                        Seq = m.Seq,
                        InitByName = m.UserName,
                        AuxiliaryRef = m.AuxiliaryRef,
                        EntryDate = m.EntryDate,
                        UserName = m.UserName,
                        InterbranchStatus = m.InterbranchStatus,
                        CounterpartyBranchId = m.CounterpartyBranchId,
                        TimeOfOperation = m.TimeOfOperation,
                        Currency = "XAF FRANCE CFA",
                        PrintedBy = _accntStatementService.GetUserFullName()
                    })).ToList();

                // 5. Validate flat data
                if (!data.Any())
                {
                    return Json(new { success = false, message = "No transactions found for the selected filters." },
                        JsonRequestBehavior.AllowGet);
                }

                // 6. Save to session for report viewer
                Session["rptSource"] = data;
                Session["BranchInfo"] = BranchInformation;

                return Json(new { success = true, message = "Report generated successfully." },
                    JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }





        [HttpPost]
        public ActionResult GetReport(string path)
        {
            
            this.HttpContext.Session["rptType"] = "ReportParameterLess";
            this.HttpContext.Session["ReportName"] = $"AccStatement.rpt";
            this.HttpContext.Session["rptpath"] = $"~/AppFiles/Accountingv2Reporting/ReportRPT/AccStatement.rpt";
            this.HttpContext.Session["rpttitle"] = $"AccountStatement";
            return Json(new { success = true, status = false, message = "Parameters OK." }, JsonRequestBehavior.AllowGet);

        }


    }
}