using CBS.BusinessService.Accounting_V2.BranchAccountService;
using CBS.BusinessService.Accounting_V2.JournalReceiptsReports;
using CBS.BusinessService.Accounting_V2.TrialBalance;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Queries;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Reporting.FlatBaseE;
using CBS.FrontDesk.Data.Entity.AccountingV2;
using CBS.FrontDesk.Data.MockData;
using CBS.FrontDesk.Data.ReportDataSetDto;
using CBS.FrontDesk.UI.AppFiles.Reporting.Accounting;
using CBS.FrontDesk.UI.Controllers.Accounting_V2.AccntStatement;

using Humanizer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Accounting_V2.Receipts
{
    public class JournalReceiptsController : BaseController
    {
        private readonly JournalReceiptsService _journalReceiptsService;
        private readonly BranchAccountService _branchAccountService;
        private readonly BranchServices _branchServices;

        public JournalReceiptsController(
            JournalReceiptsService journalReceiptsService,
            BranchServices branchServices,
            BranchAccountService branchAccountService)
        {
            _branchServices = branchServices;
            _journalReceiptsService = journalReceiptsService;
            _branchAccountService = branchAccountService;
        }

        /// <summary>
        /// Loads the Journal Receipts page.
        /// </summary>
        public async Task<ActionResult> Index()
        {
            return View();
        }

        /// <summary>
        /// Generates a Journal Receipt dataset for printing based on the selected filters.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> GenerateReciept(ReceiptV2Filter filter)
        {
            try
            {
                // ==============================================================
                // STEP 1 — Retrieve receipt dataset from backend service
                // ==============================================================                
                var response = await _journalReceiptsService.GetReceiptAsync(filter);

                if (response == null)
                {
                    this.HttpContext.Session["rptSource"] = null;
                    return Json(new { success = false, message = "No data found for the selected filters." },
                        JsonRequestBehavior.AllowGet);
                }

                // ==============================================================
                // STEP 2 — Retrieve bank & branch metadata for report header
                // ==============================================================                
                var BranchInformation = await _branchServices.GetBranch(_branchServices.GetBranchID());

                // Convert numeric amount to amount in words
                decimal amount = response.TotalDebit;
                long whole = (long)amount;
                string words = whole.ToWords().Transform(To.TitleCase) + " Francs";

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

                // ==============================================================
                // STEP 3 — Flatten receipt lines so Crystal Report can bind easily
                // --------------------------------------------------------------
                // • Each row includes the receipt header + one movement/entry
                // • Header (bank + branch) merged per-row using ApplyHeader()
                // ==============================================================
                var data = response.Entries.Select(x =>
                {
                    var item = new ReceiptFlatItems
                    {
                        Id = response.Id,
                        BranchId = response.BranchId,
                        BranchCode = response.BranchCode,
                        BranchName = response.BranchName,
                        Reference = response.Reference,
                        Number = response.Number,
                        Title = response.Title,
                        Memo = response.Memo,
                        OperationCode = response.OperationCode,
                        OperationLabel = response.OperationLabel,
                        JournalHeaderId = response.JournalHeaderId,
                        JournalReference = response.JournalReference,
                        AccountingDate = response.AccountingDate,
                        IssuedAtUtc = response.IssuedAtUtc,
                        IssuedAtLocal = response.IssuedAtLocal,
                        Payor = response.Payor,
                        MemberNumber = response.MemberNumber,
                        MemberName = response.MemberName,
                        TellerName = response.TellerName,
                        TillName = response.TillName,
                        IssuedBy = response.IssuedBy,
                        Currency = response.Currency,
                        TotalDebit = response.TotalDebit,
                        TotalCredit = response.TotalCredit,
                        CashInAmount = response.CashInAmount,
                        CashOutAmount = response.CashOutAmount,
                        NetAmount = response.NetAmount,
                        IsInterBranch = response.IsInterBranch,
                        CounterpartyBranchId = response.CounterpartyBranchId,
                        CounterpartyBranchCode = response.CounterpartyBranchCode,
                        CounterpartyBranchName = response.CounterpartyBranchName,
                        PrintedCount = response.PrintedCount,
                        IsReprint = response.IsReprint,
                        AmountInWords = words,

                        // Embedded payload information (JSON formatted)
                        PayloadDisplayTitle = response.Payload?.DisplayTitle,
                        PayloadDescription = response.Payload?.Description,
                        PayloadLinesJson = JsonConvert.SerializeObject(response.Payload?.Lines),
                        PayloadCashDenominationsJson = JsonConvert.SerializeObject(response.Payload?.CashDenominations),
                        PayloadExtraMetadataJson = JsonConvert.SerializeObject(response.Payload?.ExtraMetadata),
                        PayloadVersion = response.PayloadVersion,
                        PayloadHash = response.PayloadHash,

                        PrintedBy = _journalReceiptsService.GetUserFullName(),

                        // Movement line
                        Dr = x.Dr,
                        Cr = x.Cr,
                        Description = x.Description,

                        // Embed original entries collection for display
                        EntriesJson = JsonConvert.SerializeObject(response.Entries)
                    };

                    // Merge bank & branch header into flat dataset row
                    ApplyHeader(item, header);
                    return item;
                }).ToList();

                // ⚠ NOTE: report binding commented because final Crystal section not shown
                this.HttpContext.Session["rptSource"] = data;
                return Json(new { success = true, message = $"Report file not found." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Copies bank and branch metadata into a single flattened receipt row.
        /// Report requires header fields available on every row.
        /// </summary>
        private void ApplyHeader(ReceiptFlatItems item, BankHeaderInformation header)
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
        /// Sets required Crystal Report metadata for rendering Journal Receipt.
        /// </summary>
        [HttpPost]
        public ActionResult GetReport(string path)
        {
            this.HttpContext.Session["rptType"] = "ReportParameterLess";
            this.HttpContext.Session["ReportName"] = $"CrystalReport1.rpt";
            this.HttpContext.Session["rptpath"] = $"~/AppFiles/Accountingv2Reporting/ReportRPT/CrystalReport1.rpt";
            this.HttpContext.Session["rpttitle"] = $"JBR";

            return Json(new { success = true, status = false, message = "Parameters OK." },
                JsonRequestBehavior.AllowGet);
        }
    }
}
