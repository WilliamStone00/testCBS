using CBS.BusinessService.Accounting_V2.BranchAccountService;
using CBS.BusinessService.Accounting_V2.JournalReceiptsReports;
using CBS.BusinessService.Config;

using CBS.FrontDesk.Data.Entity.Accounting_V2.Queries;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Reporting.FlatBaseE;

using Humanizer;
using Newtonsoft.Json;
using System;
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
                // STEP 1 — Retrieve dataset
                var response = await _journalReceiptsService.GetReceiptAsync(filter);

                if (response == null || response.Entries == null || !response.Entries.Any())
                {
                    Session["rptSource"] = null;
                    Session["BranchInfo"] = null;
                    return Json(new { success = false, message = "No data found for the selected filters." },
                        JsonRequestBehavior.AllowGet);
                }

                // STEP 2 — Retrieve branch + bank metadata
                var BranchInformation = await _branchServices.GetBranch(response.BranchId);

                // Convert numeric amount to words
                decimal amount = response.TotalDebit;
                string words = ((long)amount).ToWords().Transform(To.TitleCase) + " Francs";

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

                var tdebit = response.Entries.Count(x => x.Dr > 0);
                var tcredit = response.Entries.Count(x => x.Cr > 0);

                // STEP 3 — Flatten receipt rows for Crystal Reports
                var data = response.Entries.Select(x =>
                {
                    var entry = response.IssuedAtLocal.ToString("MMM dd yyyy hh:mm:ss.fff tt");
                    

                    var item = new ReceiptFlatItems
                    {
                        Id = response.Id,
                        BranchId = response.BranchId,
                        BranchCode = response.BranchCode,
                        BranchName = response.BranchName,
                        Reference = response.Reference,
                        ReferenceNumber = response.Reference,
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
                        AuxiliaryRef = response.IssuedBy.ToUpper(),
                        TotalDebit = response.TotalDebit,
                        TotalCredit = response.TotalCredit,
                        CashInAmount = response.CashInAmount  ?? 0,
                        CashOutAmount = response.CashOutAmount ?? 0,
                        NetAmount = response.NetAmount ?? 0,
                        IsInterBranch = response.IsInterBranch,
                        CounterpartyBranchId = response.CounterpartyBranchId,
                        CounterpartyBranchCode = response.CounterpartyBranchCode,
                        CounterpartyBranchName = response.CounterpartyBranchName,
                        PrintedCount = response.PrintedCount,
                        IsReprint = response.IsReprint,
                        AmountInWords = words,
                       
                        Date = x.Date,
                        TCredit = tcredit,
                        TDebit = tdebit,
                        ReadableDate = entry,
                       
                        // Movement line
                        AccountNumber = x.AccountNumber,
                        AccountName = x.AccountName,
                        Dr = x.Dr,
                        Cr = x.Cr,
                        Description = x.Description,

                        PrintedBy = _journalReceiptsService.GetUserFullName()
                    };

                    // Attach bank & branch header
                    ApplyHeader(item, header);
                    return item;
                }).ToList();


                // STEP 4 — Save dataset to session
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

        /// <summary>
        /// Copies bank and branch metadata to each flattened row.
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
        /// Sets Crystal Report metadata.
        /// </summary>
        [HttpPost]
        public ActionResult GetReport(string path)
        {
            Session["rptType"] = "ReportParameterLess";
            Session["ReportName"] = "JournalReceipts.rpt";
            Session["rptpath"] = "~/AppFiles/Accountingv2Reporting/ReportRPT/JournalReceipts.rpt";
            Session["rpttitle"] = "JER3";

            return Json(new
            {
                success = true,
                message = "Parameters OK."
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
