using CBS.BusinessService.Accounting_V2.AccntStatements;
using CBS.BusinessService.Accounting_V2.BranchAccountService;
using CBS.BusinessService.Accounting_V2.JournalEntries;
using CBS.BusinessService.Accounting_V2.TrialBalance;
using CBS.BusinessService.Config;

using CBS.FrontDesk.Data.Entity.Accounting_V2.Queries;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Reporting.FlatBaseE;
using CBS.FrontDesk.Data.Entity.Accounting_V2.TrialBalance;
using CBS.FrontDesk.Data.MockData;
using CBS.FrontDesk.Data.ReportDataSetDto;
using CBS.FrontDesk.UI.AppFiles.Reporting.Accounting;
using CBS.FrontDesk.UI.Controllers.Accounting_V2.TrialBalance;

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
    public class AccntStatementController : BaseController
    {
        private readonly JournalReceiptsService _accntStatementService;
        private readonly BranchAccountService _branchAccountService;
        private readonly BranchServices _branchServices;

        public AccntStatementController(
            JournalReceiptsService accntStatementService,
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
            // Convert entry date + time string into a full datetime instance
            var dateTimeString = $"{entryDate:yyyy-MM-dd} {timeOfOperation}";
            return DateTime.Parse(dateTimeString);
        }

        [HttpPost]
        public async Task<ActionResult> GenerateReport(AccountingV2ReportsFilter model)
        {
            try
            {
                // =====================================================
                // STEP 1 — Retrieve account statement dataset
                // =====================================================
                var response = await _accntStatementService.GetAccntStatement(model);

                // Stop if no dataset was returned
                if (response == null || !response.Any())
                {
                    Session["rptSource"] = null;
                    Session["BranchInfo"] = null;
                    return Json(new { success = false, message = "No data found for the selected filters." },
                        JsonRequestBehavior.AllowGet);
                }

                // Get branch and bank metadata for report header
                var BranchInformation = await _branchServices.GetBranch(model.BranchId);

                // =====================================================
                // STEP 2 — Construct static report header (bank + branch)
                // =====================================================
                var header = new BankHeaderInformation
                {
                    BankId = BranchInformation.Bank.Id,
                    BankBankCode = BranchInformation.Bank.BankCode,
                    BankName = BranchInformation.Bank.Name,
                    BankTelephone = BranchInformation.Bank.Telephone,
                    BankEmail = BranchInformation.Bank.Email,
                    BankAddress = BranchInformation.Bank.Address,
                    BankLogoUrl = BranchInformation.Bank.LogoUrl,
                    BankMotto = BranchInformation.Bank.Motto,
                    BankRegistrationNumber = BranchInformation.Bank.RegistrationNumber,
                    BankImmatriculationNumber = BranchInformation.Bank.ImmatriculationNumber,
                    BankPBox = BranchInformation.Bank.PBox,

                    BranchId = BranchInformation.Id,
                    BranchCode = BranchInformation.BranchCode,
                    BranchName = BranchInformation.Name,
                    BranchTelephone = BranchInformation.Telephone,
                    BranchEmail = BranchInformation.Email,
                    BranchAddress = BranchInformation.Address,
                    BranchLogoUrl = BranchInformation.LogoUrl,
                    BranchCapital = BranchInformation.Capital,
                    BranchRegistrationNumber = BranchInformation.RegistrationNumber,
                    BranchImmatriculationNumber = BranchInformation.ImmatriculationNumber,
                    BranchPBox = BranchInformation.PBox
                };

                // ==========================================================
                // STEP 3 — Flatten hierarchical statement into table form
                // ----------------------------------------------------------
                // • Each movement becomes a separate row for Crystal Reports
                // • Each row contains both movement data + bank/branch header
                //   to ensure report printing/exporting works under grouping
                // ==========================================================
                var data = response
                    .Where(acc => acc.Movements != null && acc.Movements.Any())
                    .SelectMany(acc => acc.Movements.Select(m =>
                    {
                        var item = new AccountStatementFlatItems
                        {
                            AccountNumber = acc.AccountNumber,
                            AccountName = m.AccountName,
                            OpeningBalance = acc.OpeningBalance,
                            ClosingBalance = acc.ClosingBalance,

                            AccountingDate = _accntStatementService.FormatDate(m.AccountingDate),
                            From = model.From,
                            To = model.To,
                            Year = DateTime.Now.Year.ToString(),

                            Time = TimeSpan.TryParse(m.TimeOfOperation, out var ts) ? ts : TimeSpan.Zero,
                            DayTime = GetDayTime(m.EntryDate, m.TimeOfOperation),

                            ReferenceNumber = m.Reference,
                            BranchId = m.BranchId,
                            CreditAmount = m.CR,
                            DebitAmount = m.DR,
                            Description = m.Narration,

                            BranchName = BranchInformation.Name,
                            BranchCode = BranchInformation.BranchCode,
                            Address = BranchInformation.Address,
                            BranchTel = BranchInformation.Telephone,
                            HeadOfficePhone = BranchInformation.Bank.Telephone,
                            BranchEmail = BranchInformation.Email,

                            EndingBalance = acc.ClosingBalance,
                            LogoUrl = BranchInformation.Bank.LogoUrl,
                            Motto = BranchInformation.Bank.Motto,
                            RegistrationNumber = BranchInformation.Bank.RegistrationNumber,
                            BankInitial = BranchInformation.BankInitial,
                            PBox = BranchInformation.PBox,
                            DisplayName = BranchInformation.DisplayName,

                            TotalDifference = m.TotalDifference,
                            TotalCR = m.TotalCR,
                            TotalDR = m.TotalDR,
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
                        };

                        // Attach bank + branch header info to movement record
                        ApplyHeader(item, header);
                        return item;
                    })).ToList();

                // Stop if no movement rows remain after flattening
                if (!data.Any())
                {
                    return Json(new { success = false, message = "No transactions found for the selected filters." },
                        JsonRequestBehavior.AllowGet);
                }

                // =====================================================
                // STEP 4 — Save dataset to session for Crystal viewer
                // =====================================================
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

        // Copies bank and branch header details into each flattened statement row
        private void ApplyHeader(AccountStatementFlatItems item, BankHeaderInformation header)
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

        [HttpPost]
        public ActionResult GetReport(string path)
        {
            // Configure report metadata for the Report Viewer
            this.HttpContext.Session["rptType"] = "ReportParameterLess";
            this.HttpContext.Session["ReportName"] = $"AccStatement.rpt";
            this.HttpContext.Session["rptpath"] = $"~/AppFiles/Accountingv2Reporting/ReportRPT/AccStatement.rpt";
            this.HttpContext.Session["rpttitle"] = $"AccountStatement";

            return Json(new { success = true, status = false, message = "Parameters OK." },
                JsonRequestBehavior.AllowGet);
        }




        public async Task<ActionResult> DownloadGeneralLedger(AccountingV2ReportsFilter model)
        {
            try
            {
                // 1. Fetch the journal entries based on the filter
                var response = await _accntStatementService.GetAccntStatement(model);
                if (response == null || !response.Any())
                {
                    return Json(new
                    {
                        success = false,
                        message = "No data available for export."
                    }, JsonRequestBehavior.AllowGet);
                }

                // 2. Get branch information (for report header)
                var branchInfo = await _branchServices.GetBranch(model.BranchId);
                if (branchInfo == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Branch information not found."
                    }, JsonRequestBehavior.AllowGet);
                }

                // 3. Build BankHeaderInformation from branch data
                var headerInfo = new BankHeaderInformation
                {
                    BankId = branchInfo.Bank?.Id,
                    BankBankCode = branchInfo.Bank?.BankCode,
                    BankCode = branchInfo.Bank?.BankCode,
                    BankName = branchInfo.Bank?.Name,
                    BankTelephone = branchInfo.Bank?.Telephone,
                    BankEmail = branchInfo.Bank?.Email,
                    BankAddress = branchInfo.Bank?.Address,
                    BankLogoUrl = branchInfo.Bank?.LogoUrl,
                    BankMotto = branchInfo.Bank?.Motto,
                    BankRegistrationNumber = branchInfo.Bank?.RegistrationNumber,
                    BankImmatriculationNumber = branchInfo.Bank?.ImmatriculationNumber,
                    BankPBox = branchInfo.Bank?.PBox ?? "",
                    BranchId = branchInfo.Id,
                    BranchCode = branchInfo.BranchCode,
                    BranchName = branchInfo.Name,
                    BranchTelephone = branchInfo.Telephone,
                    BranchEmail = branchInfo.Email,
                    BranchAddress = branchInfo.Address,
                    BranchLogoUrl = branchInfo.LogoUrl,
                    BranchCapital = branchInfo.Capital,
                    BranchRegistrationNumber = branchInfo.RegistrationNumber,
                    BranchImmatriculationNumber = branchInfo.ImmatriculationNumber,
                    BranchPBox = branchInfo.PBox ?? ""
                };

                // 4. Get the name of the user who exported the report
                string exportedBy = Session["FullName"]?.ToString() ?? "System";

                // 5. Flatten hierarchical statement into table form (same as GenerateReport)
                var flatItems = response
                    .Where(acc => acc.Movements != null && acc.Movements.Any())
                    .SelectMany(acc => acc.Movements.Select(m =>
                    {
                        TimeSpan timeValue = TimeSpan.Zero;
                        if (!string.IsNullOrEmpty(m.TimeOfOperation))
                        {
                            TimeSpan.TryParse(m.TimeOfOperation, out timeValue);
                        }

                        var item = new AccountStatementFlatItems
                        {
                            AccountNumber = acc.AccountNumber,
                            AccountName = m.AccountName,
                            OpeningBalance = acc.OpeningBalance,
                            ClosingBalance = acc.ClosingBalance,

                            AccountingDate = _accntStatementService.FormatDate(m.AccountingDate),
                            From = model.From,
                            To = model.To,
                            Year = DateTime.Now.Year.ToString(),

                            time = timeValue,  // Note: lowercase 'time' property
                            Time = timeValue,  // uppercase Time property if exists
                            DayTime = GetDayTime(m.EntryDate, m.TimeOfOperation),

                            ReferenceNumber = m.Reference,
                            BranchId = m.BranchId,
                            CreditAmount = m.CR,
                            DebitAmount = m.DR,
                            Description = m.Narration,

                            BranchName = branchInfo.Name,
                            BranchCode = branchInfo.BranchCode,
                            Address = branchInfo.Address,
                            BranchTel = branchInfo.Telephone,
                            HeadOfficePhone = branchInfo.Bank?.Telephone,
                            BranchEmail = branchInfo.Email,

                            EndingBalance = acc.ClosingBalance,
                            LogoUrl = branchInfo.Bank?.LogoUrl,
                            Motto = branchInfo.Bank?.Motto,
                            RegistrationNumber = branchInfo.Bank?.RegistrationNumber,
                            BankInitial = branchInfo.BankInitial,
                            PBox = branchInfo.PBox,
                            DisplayName = branchInfo.DisplayName,

                            TotalDifference = m.TotalDifference,
                            TotalCR = m.TotalCR,
                            TotalDR = m.TotalDR,
                            DrCr = m.DrCr,
                            Amount = m.Amount,
                            Balance = m.Balance,
                            Seq = m.Seq,
                            InitByName = m.UserName,
                            AuxiliaryRef = m.AuxiliaryRef,
                            EntryDate = m.EntryDate,
                            UserName = m.UserName,
                            Username = m.UserName, // Also set Username property
                            PrintedBy = _accntStatementService.GetUserFullName(),
                            InterbranchStatus = m.InterbranchStatus,
                            CounterpartyBranchId = m.CounterpartyBranchId,
                            TimeOfOperation = m.TimeOfOperation,
                            Currency = "XAF FRANCE CFA"
                        };

                        // Attach bank + branch header info to movement record
                        ApplyHeader(item, headerInfo);
                        return item;
                    }))
                    .ToList();

                // Stop if no movement rows remain after flattening
                if (!flatItems.Any())
                {
                    return Json(new
                    {
                        success = false,
                        message = "No transactions found for the selected filters."
                    }, JsonRequestBehavior.AllowGet);
                }

                // 6. Prepare a temporary file path
                string timestamp = DateTime.Now.ToString("ddMMyyyyHHmmss");
                string fileName = $"GeneralLedger_{timestamp}.xlsx";
                string directoryPath = Server.MapPath("~/TempFiles");
                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);
                string filePath = Path.Combine(directoryPath, fileName);

                // 7. Generate the Excel workbook using the custom generator
                var generator = new GeneralLedgerExcelExportGenerator();

                // Convert the flat items to GeneralLedger data (this method just returns the list)
                var ledgerData = GeneralLedgerExcelExportGenerator.ConvertToGeneralLedgerData(flatItems);

                // Call the main generation method
                generator.GenerateGeneralLedgerExcel(
                    ledgerData,         // List<AccountStatementFlatItems>
                    filePath,           // string filePath
                    exportedBy,         // string exportedBy
                    model,              // AccountingV2ReportsFilter (contains date range and AccountNumbers)
                    headerInfo          // BankHeaderInformation
                );

                // 8. Verify file creation and return it to the client
                if (!System.IO.File.Exists(filePath))
                    return Json(new { success = false, message = "Failed to generate Excel file." });

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
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

    }
}
