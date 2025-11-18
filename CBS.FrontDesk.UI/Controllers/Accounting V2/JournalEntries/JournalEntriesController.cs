using CBS.BusinessService.Accounting_V2.AccntStatements;
using CBS.BusinessService.Accounting_V2.JournalEntries;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Queries;
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
        /// Loads the main Journal Entries page.
        /// </summary>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Fetch Journal Entries based on filter.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> GenerateJournalEntries(AccountingV2ReportsFilter model)
        {
            try
            {
                var entries = await _journalEntriesService.GetJournalEntriesAsync(model);

                // 2. Get branch info
                var BranchInformation = await _branchServices.GetBranch(model.BranchId);

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

                var data = entries
    .OrderBy(s => s.AccountingDate)
    .ThenBy(s => s.EntryDate)
    .ThenBy(s => s.AccountNumber)
    .ThenBy(s => s.Reference)
    .ThenBy(s => s.Seq)
    .ThenBy(s => s.Dr)     // Debit
    .ThenBy(s => s.Cr)     // Credit
    .Select(x => new AccountStatementFlatItems
    {
        // -------- Parent Fields --------
        AccountNumber = x.AccountNumber,
        AccountName = x.AccountName,
        // -------- Custom Date Fields --------
        AccountingDate = x.AccountingDate.ToString("dd-MM-yyyy"),
        Year = DateTime.Now.Year.ToString(),
        From = model.From,
        To =  model.To,
        DateFrom  =  model.DateFrom,
        DateTo  =  model.DateTo,

        // Convert string "15:57:57.7830455" to TimeSpan hh:mm:ss
        time = TimeSpan.TryParse(x.TimeOfOperation, out var ts)
                    ? TimeSpan.Parse(ts.ToString(@"hh\:mm\:ss"))
                    : TimeSpan.Zero,

        // -------- Movement Fields --------
        ReferenceNumber = x.Reference,
        BranchId = x.BranchId,
        CreditAmount = x.Cr,
        DebitAmount = x.Dr,
        Description = x.Narration,
        DrCr = x.DrCr,
        Amount = x.Amount,
        Balance = x.Balance,
        Seq = x.Seq,
        
        AuxiliaryRef = x.AuxiliaryRef,   // ✅ FIXED (was UserName)
        EntryDate = x.EntryDate,
        UserName = x.UserName,
        InterbranchStatus = x.InterbranchStatus,
        CounterpartyBranchId = x.CounterpartyBranchId,
        TimeOfOperation = x.TimeOfOperation,
        BranchCode = BranchInformation.BranchCode,
        Phone = BranchInformation.Telephone,
        Address = BranchInformation.Address,
        BranchName = BranchInformation.Name,
        // Static fields
        Currency = "XAF FRANCE CFA",
        PrintedBy = _journalEntriesService.GetUserFullName()
    })
    .ToList();

                // Save to session
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
        /// Prepare report session values (Journal Entries Report)
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
