using CBS.BusinessService.Accounting_V2.AccntStatements;
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

namespace CBS.FrontDesk.UI.Controllers.Accounting_V2.JournalEntries
{
    public class JournalEntriesController : BaseController
    {
        private readonly JournalEntriesService _journalEntriesService;
        private readonly BranchServices _branchServices;

        public JournalEntriesController(BranchServices branchServices)
        {
            _journalEntriesService = new JournalEntriesService();
            _branchServices = branchServices;
        }

        /// <summary>
        /// Loads the Journal Entries main page.
        /// </summary>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Generates Journal Entries report dataset based on user-selected filters.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> GenerateJournalEntries(AccountingV2ReportsFilter model)
        {
            try
            {
                // ==============================================================
                // STEP 1 — Retrieve journal entries dataset based on filters
                // ==============================================================                
                var entries = await _journalEntriesService.GetJournalEntriesAsync(model);

                // Retrieve branch metadata for report header display
                var BranchInformation = await _branchServices.GetBranch(model.BranchId);

                // Stop processing if no entries were returned
                if (entries == null || !entries.Any())
                {
                    this.HttpContext.Session["rptSource"] = null;
                    this.HttpContext.Session["BranchInfo"] = null;

                    return Json(new
                    {
                        success = false,
                        message = "No data found for the selected filters."
                    }, JsonRequestBehavior.AllowGet);
                }

                // ==============================================================
                // STEP 2 — Build static report header (bank and branch details)
                // ==============================================================                
                var header = new BankHeaderInformation
                {
                    BankId = BranchInformation.Bank?.Id,
                    BankBankCode = BranchInformation.Bank?.BankCode,
                    BankCode = BranchInformation.Bank?.BankCode,
                    BankName = BranchInformation.Bank?.Name,
                    BankTelephone = BranchInformation.Bank?.Telephone,
                    BankEmail = BranchInformation.Bank?.Email,
                    BankAddress = BranchInformation.Bank?.Address,
                    BankLogoUrl = BranchInformation.Bank?.LogoUrl,
                    BankMotto = BranchInformation.Bank?.Motto,
                    BankRegistrationNumber = BranchInformation.Bank?.RegistrationNumber,
                    BankImmatriculationNumber = BranchInformation.Bank?.ImmatriculationNumber,
                    BankPBox = BranchInformation.Bank?.PBox ?? "",

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
                    BranchPBox = BranchInformation.PBox ?? ""
                };


                // ==============================================================
                // STEP 3 — Transform journal entries into flat rows
                // --------------------------------------------------------------
                // • Each journal movement becomes one report record
                // • Each row includes:
                //     - Journal Movement data
                //     - Bank + Branch header for report rendering
                // ==============================================================                
                var data = entries
                    .Select(x =>
                    {
                        var item = new AccountStatementFlatItems
                        {
                            // Core account references
                            AccountNumber = x.AccountNumber,
                            AccountName = x.AccountName,
                            // Date filter fields for report header
                            AccountingDate = x.AccountingDate.ToString("dd-MM-yyyy"),
                            Year = DateTime.Now.Year.ToString(),
                            From = model.From,
                            To = model.To,
                            DateFrom = model.DateFrom,
                            DateTo = model.DateTo,

                            // Convert "hh:mm:ss.ffffff" → hh:mm:ss
                            time = TimeSpan.TryParse(x.TimeOfOperation, out var ts)
                                    ? TimeSpan.Parse(ts.ToString(@"hh\:mm\:ss"))
                                    : TimeSpan.Zero,

                            // Journal movement
                            ReferenceNumber = x.Reference,
                            BranchId = x.BranchId,
                            CreditAmount = x.CR,
                            DebitAmount = x.DR,
                            Description = x.Narration,
                            DrCr = x.DrCr,
                            Amount = x.Amount,
                            Balance = x.Balance,
                            Seq = x.Seq,
                            AuxiliaryRef = x.AuxiliaryRef,
                            EntryDate = x.EntryDate,
                            UserName = x.UserName,
                            InterbranchStatus = x.InterbranchStatus,
                            CounterpartyBranchId = x.CounterpartyBranchId,
                            TimeOfOperation = x.TimeOfOperation,
                            ReportName = "GENERAL ACCOUNTING JOURNAL",


                            // Branch and contact
                            BranchCode = BranchInformation.BranchCode,
                            Phone = BranchInformation.Telephone,
                            Address = BranchInformation.Address,
                            BranchName = BranchInformation.Name,

                            // Report display fields
                            Currency = "XAF FRANCE CFA",
                            PrintedBy = _journalEntriesService.GetUserFullName()
                        };

                        // Inject bank + branch header information
                        ApplyHeader(item, header);
                        return item;
                    })
                    .ToList();

                // Save flattened dataset for Crystal Report viewer
                this.HttpContext.Session["rptSource"] = data;
                this.HttpContext.Session["BranchInfo"] = BranchInformation;

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
        /// Attach bank and branch header into one statement row.
        /// Crystal Reports requires header metadata available on each record.
        /// </summary>
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

        /// <summary>
        /// Setup Crystal Report parameters for the Journal Entries report.
        /// </summary>
        [HttpPost]
        public ActionResult GetReport(string path)
        {
            this.HttpContext.Session["rptType"] = "ReportParameterLess";
            this.HttpContext.Session["ReportName"] = "JournalEntries.rpt";
            this.HttpContext.Session["rptpath"] = "~/AppFiles/Accountingv2Reporting/ReportRPT/JournalEntries.rpt";
            this.HttpContext.Session["rpttitle"] = "JEV2";

            return Json(new
            {
                success = true,
                message = "Report parameters set successfully."
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
