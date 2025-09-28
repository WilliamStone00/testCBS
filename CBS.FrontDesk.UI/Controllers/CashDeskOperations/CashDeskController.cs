using CBS.BusinessService;
using CBS.BusinessService.Accounting;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.ReportDataSetDto.LoanPortFolioDataSet;
using CBS.FrontDesk.Data.ReportDataSetDto;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DocumentFormat.OpenXml.Spreadsheet;
using ZXing.Common;

namespace CBS.FrontDesk.UI.Controllers.CashDeskOperations
{
    //[CheckSessionTimeOutAttribute]

    public class CashDeskController : BaseController
    {
        private readonly CashDeskServices _cashDeskService;
        private readonly AccountingServices _accountingServices;
        private readonly ChartOfAccountServicesAnnex chartOfAccountServices;
        private readonly BranchServices _branchServices;

        public CashDeskController(CashDeskServices cashDeskService = null, AccountingServices accountingServices = null, ChartOfAccountServicesAnnex chartOfAccountServices = null, BranchServices branchServices = null)
        {
            _cashDeskService = cashDeskService;
            _accountingServices = accountingServices;
            this.chartOfAccountServices=chartOfAccountServices;
            _branchServices=branchServices;
        }
        // GET: CashDesk
        //public ActionResult Index()
        //{
        //    return View();
        //}
        public ActionResult Cashin()
        {
            return View();
        }
        public ActionResult Cashout()
        {
            return View();
        }
        public ActionResult LoanRepaymentSimulation()
        {
            return View();
        }
        public ActionResult MomocashCollection()
        {
            return View();
        }
        public ActionResult NoneCashOperations()
        {
            return View();
        }
        // Public, parameterless endpoints for your dynamic menu URLs
        [HttpGet]
        public Task<ActionResult> OtherCashIn() => LoadOtherCash(mode: "cashin");

        [HttpGet]
        public Task<ActionResult> OtherCashOut() => LoadOtherCash(mode: "cashout");

        // One unified loader that prepares data for the same view
        private async Task<ActionResult> LoadOtherCash(string mode)
        {
            mode = (mode ?? "cashin").Trim().ToLowerInvariant();
            bool isCashIn = mode == "cashin";
            ViewBag.Branches = await _branchServices.GetLiaison();
            ViewBag.AccountIds = await chartOfAccountServices.GetGLAccountsQueryByBranch();
            ViewBag.Operation = "income_expense";
            ViewBag.OperationType = isCashIn ? "cashin" : "cashout";
            ViewBag.FormContext = isCashIn ? "OtherCashIn" : "OtherCashOut";
            return View(new CashDesk());
        }


        //public async Task<ActionResult> OtherCashTransactions()
        //{
        //    ViewBag.Operation = "income_expense";
        //    var cashDesk = await _cashDeskService.GetOtherCashDeskTransactions();
        //    ViewBag.Branches=await _branchServices.GetLiaison();
        //    //ViewBag.Members = _cashDeskService.LoadMembersToList(cashDesk.Customers);
        //    ViewBag.MemberAccounts = new SelectList(new List<StringValues>(), "None", "No-Account-Loaded");
        //    ViewBag.OperationType = "cashin";
        //    //await GetEventNames("FEE");
        //    ViewBag.AccountIds = await chartOfAccountServices.GetGLAccountsQueryByBranch();
        //    return View(cashDesk);
        //}
        public async Task<ActionResult> OtherCashMobileMoney()
        {
            var cashDesk = await _cashDeskService.GetOtherCashDeskMobileMoney();
            return View(cashDesk);
        }
        
        public async Task<ActionResult> Ajaxloader(string Key, string path)
        {
            if (path== "getmember")
            {
                var listing = await _cashDeskService.GetCustomer(Key);
                return Json(listing, JsonRequestBehavior.AllowGet);

            }
            else
            {
                var listing = await _cashDeskService.LoadMembersAccountByMemberReference(Key);
                return Json(listing, JsonRequestBehavior.AllowGet);

            }
        }

        //income_expense
        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = "_DataNotFound", string path = null, string serviceOption = null)
        {
            try
            {
                ViewBag.KEY = KEY;
                ViewBag.OperationType = "cashin";

                if (string.IsNullOrWhiteSpace(path))
                    path = "cashin";

                ViewBag.Operation = path;

                // 🔍 Handle member search by reference or ID
                if (path == "search")
                {
                    if (string.IsNullOrWhiteSpace(KEY))
                    {
                        ViewBag.message = "⚠️ No search input provided. Please enter a valid Member Reference or Account Number to proceed.\nResolution: Ensure the search field is filled before submitting.";
                        return PartialView("_DataNotFound", new CashDesk());
                    }

                    var cashDesk = await _cashDeskService.GetMember(KEY);
                    if (cashDesk == null)
                    {
                        ViewBag.message = $"❌ No data found for the reference '{KEY}'. The member may exist, but no ordinary member account is currently linked to this reference.\nResolution: Verify the member ID and ensure that the member has an active ordinary account.";
                        return PartialView("_DataNotFound", new CashDesk());
                    }

                    // Check if ordinary account exists
                    var hasOrdinaryAccount = cashDesk.BulkDeposits != null &&
                                             cashDesk.BulkDeposits.Any(x =>
                                                 !string.IsNullOrWhiteSpace(x.AccountType) &&
                                                 x.AccountType.ToLower().Contains("ordinary"));

                    if (!hasOrdinaryAccount)
                    {
                        ViewBag.message = $"✅ Member profile found for '{KEY}', but no ordinary member account is linked to this profile.\nResolution: Please ensure that an ordinary savings account is opened for this member.";
                        return PartialView("_DataNotFound", new CashDesk());
                    }

                    return PartialView(partialView, cashDesk);
                }

                // 💰 Handle operations like cashin, cashout, repayment, etc.
                var validPaths = new[]
                {
                    "cashin", "repayment", "cashout", "cashoutsws",
                    "withdrawalnotification", "loanapplicationfeepayment", "newsubcription"
                };

                if (validPaths.Contains(path))
                {
                    if (path.Contains("cashout"))
                        ViewBag.OperationType = "cashout";

                    if (string.IsNullOrWhiteSpace(KEY))
                    {
                        ViewBag.message = "⚠️ You submitted an empty value. Please enter a valid Account Number or Reference.\nResolution: Fill in a valid account number or member reference before retrying.";
                        return PartialView("_DataNotFound", new CashDesk());
                    }

                    var cashDesk = await _cashDeskService.GetAccountByAccountNumberSearch(KEY, path);
                    if (cashDesk == null)
                    {
                        ViewBag.message = $"❌ No account found for the input '{KEY}'.\nResolution: Confirm that the account number or reference is correct and linked to a valid account.";
                        return PartialView("_DataNotFound", new CashDesk());
                    }

                    // ✅ Check if all non-loan accounts start with MB
                    var nonLoanAccounts = cashDesk?.BulkDeposits?
                        .Where(x => x.AccountType != null && !x.AccountType.ToLower().Contains("loan"))
                        .ToList();

                    bool allAccountsAreMB = nonLoanAccounts != null && nonLoanAccounts.All(x =>
                        !string.IsNullOrWhiteSpace(x.AccountNumber) &&
                        x.AccountNumber.Trim().ToUpper().StartsWith("MB"));

                    ViewBag.AllAccountsAreMB = allAccountsAreMB;
                    ViewBag.AllAccountsAreMB = allAccountsAreMB;
                    ViewBag.ApprovedManualEntries = cashDesk.SelectedItemsApprovedUploads;

                    return PartialView(partialView, cashDesk);
                }

                // 🆕 New depositor form
                if (path == "new_depositor")
                {
                    return PartialView(partialView, new CashDesk());
                }

                // ❌ Unknown operation path
                ViewBag.message = $"❌ Invalid operation path '{path}' selected.\nResolution: Please choose a valid operation such as cashin, cashout, or repayment from the system menu.";
                return PartialView("_NoRecordFound", new CashDesk());
            }
            catch (Exception ex)
            {
                ViewBag.message = $"🚨 An unexpected error occurred: {ex.Message}\nResolution: Please contact system administrator if the issue persists.";
                return PartialView("_NoRecordFound", new CashDesk());
            }
        }
        [HttpPost]
        public async Task<ActionResult> PostRequestCash(List<BulkDeposit> deposits)
        {
            try
            {
                if (deposits != null)
                {
                    var data = await _cashDeskService.BulkDeposi(deposits);
                    if (data.Result)
                    {
                        string operationtype = deposits.FirstOrDefault().OperationType.ToLower();
                        string viewerUrl = PrepareReport(operationtype);
                        return Json(new { success = data.Result, redirectUrl = viewerUrl, status = data.MessageStatus, message = Messaging.MessageResult(data) });
                    }
                    return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

                }
                return Json(new { success = false, status = false, message = $"No data was submitted." });

            }
            catch (Exception ex)
            {
                return Json(new { success = false, status = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<ActionResult> OtherCashinPosting(AddOtherTransactionCommand otherTransactionCommand)
        {
            try
            {
                if (otherTransactionCommand != null)
                {
                    var data = await _cashDeskService.OtherCashin(otherTransactionCommand);
                    if (data.Result)
                    {
                        string operationtype = otherTransactionCommand.TransactionType.ToLower();
                        string viewerUrl = PrepareReport(operationtype);
                        return Json(new { success = data.Result, redirectUrl = viewerUrl, status = data.MessageStatus, message = Messaging.MessageResult(data) });
                    }
                    return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

                }
                return Json(new { success = false, status = false, message = $"No data was submitted." });

            }
            catch (Exception ex)
            {
                return Json(new { success = false, status = false, message = $"An error occurred: {ex.Message}" });
            }
        }


        public string PrepareReport(string mainReportType)
        {
            // STEP 1: Report title & path mapping
            var reportMappings = new Dictionary<string, (string path, string title)>
            {
                { "cashin", ("Transactions/Payment/MainReport.rpt", "CASH-IN") },
                { "withdrawal", ("Transactions/Payment/MainReport.rpt", "CASH-OUT") },
                { "cashoutsws", ("Transactions/Payment/MainReport.rpt", "CASH-OUT SPECIAL WITHDRAWAL SLIPS") },
                { "withdrawalnotification", ("Transactions/Payment/MainReport.rpt", "SAVING WITHDRAWAL NOTIFICATION FEE") },
                { "repayment", ("Transactions/Payment/MainReport.rpt", "LOAN REPAYMENT") },
                { "loanapplicationfeepayment", ("Transactions/Payment/MainReport.rpt", "LOAN APPLICATION FEE") },
                { "paymentreceipt", ("Transactions/Payment/MainReport.rpt", "PAYMENT RECEIPT") }
            };

            mainReportType = mainReportType.ToLower();
            var (relativeReportPath, reportTitle) = reportMappings.TryGetValue(mainReportType, out var mapping)
                ? mapping
                : ("NOTHING.rpt", "DEFAULTED");

            // STEP 2: Identify reports that require PaymentReciptDS with subreports and parameters
            var reportTypesWithSubReports = new HashSet<string>
            {
                "cashin", "withdrawal", "cashoutsws", "withdrawalnotification", "repayment", "loanapplicationfeepayment"
            };

            Dictionary<string, object> subReportData = null;
            var rptSource = HttpContext.Session["rptSource"] as List<PaymentReciptDS> ?? new List<PaymentReciptDS>();
            var firstItem = rptSource.FirstOrDefault() ?? new PaymentReciptDS();

            if (reportTypesWithSubReports.Contains(mainReportType))
            {
                // Subreport binding
                subReportData = new Dictionary<string, object>
                {
                    { "DenominationSubReport", firstItem.DenominationDs },
                    { "PaymentDetailSubReport", firstItem.PaymentDetailDs },
                    { "PaymentDetailSubReportLoan", firstItem.PaymentDetailDs }
                };

                Session["SubReportsData"] = subReportData;
            }
            else
            {
                // Report with no sub reports
            }
            
            // STEP 3: Parameter binding
            var parameters = new Dictionary<string, object>
            {
                { "AccountingDate", firstItem.AccountingDay.ToString("dd/MM/yyyy") },
                { "TransactionDate", firstItem.Date.ToString("dd/MM/yyyy hh:mm") },
                { "CurrentDate", DateTime.Now.ToString("dd/MM/yyyy hh:mm") },
                { "CurrentYear", DateTime.Now.Year.ToString() },
                { "PrintedBy", Session["FullName"]?.ToString() ?? "System" }
            };
            this.HttpContext.Session["rptType"] = "ReportParameterLess";
            this.HttpContext.Session["ReportName"] = $"MainReport.rpt";
            this.HttpContext.Session["rptpath"] = $"~/AppFiles/Reporting/Transactions/Payment/MainReport.rpt";
            this.HttpContext.Session["rpttitle"] = $"MemberReceipts";
            // 4) Add LogoUrl and WaterMarkUrl (from Session first, fallback to DS)
            string logoUrl = this.HttpContext.Session["LogoUrl"] as string ?? firstItem?.Logo;
            string watermarkUrl = this.HttpContext.Session["WaterMarkUrl"] as string ?? firstItem?.HeadOfficeWaterMark;

            if (!string.IsNullOrWhiteSpace(logoUrl))
                parameters["LogoUrl"] = logoUrl;

            if (!string.IsNullOrWhiteSpace(watermarkUrl))
                parameters["WaterMarkUrl"] = watermarkUrl;
            // STEP 4: Final Session setup and return viewer URL
            Session["MainData"] = rptSource;
            Session["ReportParameters"] = parameters;

            var reportPathParam = HttpUtility.UrlEncode(relativeReportPath);
            var reportNameParam = HttpUtility.UrlEncode(reportTitle);

            return Url.Content($"/ReportForm/ReportViewer.aspx?reportPath={reportPathParam}&reportName={reportNameParam}");
        }

        [HttpPost]
        public async Task<ActionResult> PostRequestCashTest(List<BulkDeposit> deposits)
        {
            try
            {
                if (deposits != null && deposits.Any())
                {
                    // Simulate a processing delay
                    await Task.Delay(500); // Mock async wait

                    // Simulate a successful result object
                    var mockResult = new
                    {
                        Result = true,
                        MessageStatus = true,
                        Message = $"✅ Transaction simulated successfully for {deposits.Count} account(s)."
                    };

                    return Json(new
                    {
                        success = mockResult.Result,
                        status = mockResult.MessageStatus,
                        message = mockResult.Message
                    });
                }

                return Json(new { success = false, status = false, message = "⚠️ No data was submitted." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, status = false, message = $"❌ An error occurred: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<ActionResult> GetReport(string path)
        {
            
            if (path=="loan")
            {
                this.HttpContext.Session["rptType"] = "ReportParameterLess";
                this.HttpContext.Session["ReportName"] = $"MainReportLoan.rpt";
                this.HttpContext.Session["rptpath"] = $"~/AppFiles/Reporting/Transactions/Payment/Loan/MainReportLoan.rpt";
                this.HttpContext.Session["rpttitle"] = $"MemberLoanReceipts";
                return Json(new { success = true, status = false, message = "Parameters OK." }, JsonRequestBehavior.AllowGet);

            }
            else
            {
                this.HttpContext.Session["rptType"] = "ReportParameterLess";
                this.HttpContext.Session["ReportName"] = $"MainReport.rpt";
                this.HttpContext.Session["rptpath"] = $"~/AppFiles/Reporting/Transactions/Payment/MainReport.rpt";
                this.HttpContext.Session["rpttitle"] = $"MemberReceipts";
                return Json(new { success = true, status = false, message = "Parameters OK." }, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpPost]
        public async Task<ActionResult> GetReportOtherPayment()
        {
            this.HttpContext.Session["rptType"] = "ReportParameterLess";
            this.HttpContext.Session["ReportName"] = $"OtherTransactionReciept.rpt";
            this.HttpContext.Session["rptpath"] = $"~/AppFiles/Reporting/Transactions/Reciepts/OtherTransactionReciept.rpt";
            this.HttpContext.Session["rpttitle"] = $"MemberReceiptsOtherPayment";
            return Json(new { success = true, status = false, message = "Parameters OK." }, JsonRequestBehavior.AllowGet);

        }
        [HttpGet]
        public async Task<ActionResult> GetOnboardingSummary(string customerId)
        {
            if (string.IsNullOrWhiteSpace(customerId))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Customer ID is required.");
            // newsubcription
            // Load your summary service (replace with actual logic or inject it)
            var onboardingDetail = await _cashDeskService.GetOnboardingDetailsAsync(customerId);

            if (onboardingDetail == null)
                return new HttpStatusCodeResult(HttpStatusCode.NotFound, "Onboarding data not found.");

            return PartialView("_OperationDesk", onboardingDetail);
        }

    }
}