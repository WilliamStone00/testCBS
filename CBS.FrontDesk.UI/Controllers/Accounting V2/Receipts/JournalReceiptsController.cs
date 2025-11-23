using CBS.BusinessService.Accounting_V2.BranchAccountService;
using CBS.BusinessService.Accounting_V2.JournalReceiptsReports;
using CBS.BusinessService.Accounting_V2.TrialBalance;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Queries;
using CBS.FrontDesk.Data.Entity.AccountingV2;
using CBS.FrontDesk.Data.MockData;
using CBS.FrontDesk.Data.ReportDataSetDto;
using CBS.FrontDesk.UI.AppFiles.Reporting.Accounting;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using CrystalDecisions.Web;
using DocumentFormat.OpenXml.Office.Word;
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

    //[CheckSessionTimeOut]
    public class JournalReceiptsController : BaseController
    {
        private readonly JournalReceiptsService _journalReceiptsService;
        private readonly BranchAccountService _branchAccountService;
        private readonly BranchServices _branchServices;
        


        public JournalReceiptsController(JournalReceiptsService journalReceiptsService,
            BranchServices branchServices,
            BranchAccountService branchAccountService)
        {
            _branchServices = branchServices;
            _journalReceiptsService = journalReceiptsService;
            _branchAccountService = branchAccountService;
        }

        public async Task<ActionResult> Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> GenerateReciept(ReceiptV2Filter filter)
        {
            try
            {
                
                var response = await _journalReceiptsService.GetReceiptAsync(filter);

                var path = Server.MapPath("~/AppFiles/Images/Logog2X.jpeg").Replace("\\", "/");

                if (response == null)
                {
                    this.HttpContext.Session["rptSource"] = null;
                    return Json(new { success = false, message = "No data found for the selected filters." }, JsonRequestBehavior.AllowGet);
                }

                decimal amount = response.TotalDebit;
                long whole = (long)amount;
                string words = whole.ToWords().Transform(To.TitleCase) + "Francs";

                var data = response.Entries.Select(x => new ReceiptFlatItems
                {
                    Id = response.Id,
                    BranchId = response.BranchId,
                    BranchCode = response.BranchCode,
                    BranchName = response.BranchName,
                    Reference = response.Reference,
                    Number = response.Number,
                    Logo     = path,
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
                    // payload
                    PayloadDisplayTitle = response.Payload?.DisplayTitle,
                    PayloadDescription = response.Payload?.Description,
                    PayloadLinesJson = JsonConvert.SerializeObject(response.Payload?.Lines),
                    PayloadCashDenominationsJson = JsonConvert.SerializeObject(response.Payload?.CashDenominations),
                    PayloadExtraMetadataJson = JsonConvert.SerializeObject(response.Payload?.ExtraMetadata),
                    PayloadVersion = response.PayloadVersion,
                    PayloadHash = response.PayloadHash,
                    PrintedBy = _journalReceiptsService.GetUserFullName(),
                    Dr = x.Dr,
                    Cr = x.Cr,
                    Description = x.Description,

                    // entries
                    EntriesJson = JsonConvert.SerializeObject(response.Entries)

                }).ToList();
                //this.HttpContext.Session["rptSource"] = data;
                return Json(new { success = true, message = $"Report file not found." });

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
            this.HttpContext.Session["ReportName"] = $"JournalReceipts";
            this.HttpContext.Session["rptpath"] = $"~/AppFiles/Accountingv2Reporting/ReportRPT/JournalReceipts.rpt";
            this.HttpContext.Session["rpttitle"] = $"JBR";
            return Json(new { success = true, status = false, message = "Parameters OK." }, JsonRequestBehavior.AllowGet);




        }


    }
}