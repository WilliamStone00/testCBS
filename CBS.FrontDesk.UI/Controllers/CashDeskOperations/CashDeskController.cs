using CBS.BusinessService.Accounting;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
using CBS.FrontDesk.Data.Message;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.CashDeskOperations
{
    [CheckSessionTimeOutAttribute]

    public class CashDeskController : BaseController
    {
        private readonly CashDeskServices _cashDeskService;
        private readonly AccountingServices _accountingServices;
        private readonly ChartOfAccountServicesAnnex chartOfAccountServices;


        public CashDeskController(CashDeskServices cashDeskService = null, AccountingServices accountingServices = null, ChartOfAccountServicesAnnex chartOfAccountServices = null)
        {
            _cashDeskService = cashDeskService;
            _accountingServices = accountingServices;
            this.chartOfAccountServices=chartOfAccountServices;
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
        
        public async Task<ActionResult> OtherCashTransactions()
        {
            ViewBag.Operation = "income_expense";
            var cashDesk = await _cashDeskService.GetOtherCashDeskTransactions();
            //ViewBag.Members = _cashDeskService.LoadMembersToList(cashDesk.Customers);
            ViewBag.MemberAccounts = new SelectList(new List<StringValues>(), "None", "No-Account-Loaded");
            await GetEventNames("FEE");
            //ViewBag.EventCodes = await _accountingServices.GetEventNames("INCOME");
            return View(cashDesk);
        }
        public async Task<ActionResult> OtherCashMobileMoney()
        {
            var cashDesk = await _cashDeskService.GetOtherCashDeskMobileMoney();
            return View(cashDesk);
        }
        //OtherCashMobileMoney
        public async Task<ActionResult> ExpenseOtherPayment()
        {
            ViewBag.Operation = "income_expense";
            var cashDesk = await _cashDeskService.GetOtherCashDeskTransactions();
            //ViewBag.Members = _cashDeskService.LoadMembersToList(cashDesk.Customers);
            ViewBag.MemberAccounts = new SelectList(new List<StringValues>(), "None", "No-Account-Loaded");
            await GetEventNames("EXPENSE");
            //ViewBag.EventCodes = await _accountingServices.GetEventNames("INCOME");
            return View(cashDesk);
        }
        private async Task GetEventNames(string operationType)
        {
            ViewBag.EventCodes = await _accountingServices.GetEventNamesOtherCashIn(operationType);

        }
        public async Task<ActionResult> Ajaxloader(string Key,string path)
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
                if (true)
                {

                }
                ViewBag.OperationType="cashin";
                ViewBag.KEY = KEY;
                if (path=="")
                {
                    path = "cashin";
                }
                if (path == "search")
                {
                    if (KEY == null || KEY == "")
                    {
                        ViewBag.message = "Empty data was submited. Please enter search criterial";
                        return PartialView("_DataNotFound", new CashDesk());
                    }
                    var cashDesk = await _cashDeskService.GetMember(KEY);
                    if (cashDesk == null)
                    {
                        ViewBag.message = $"{KEY} was not found in the database.";
                        return PartialView("_DataNotFound", new CashDesk());
                    }
                    return PartialView(partialView, cashDesk);


                }
                else if (path == "cashin" ||path=="repayment"|| path == "cashout" || path == "cashoutsws" || path == "repayment" || path == "withdrawalnotification" || path== "loanapplicationfeepayment" || path=="newsubcription")
                {

                    if (path.Contains("cashout"))
                    {
                        ViewBag.OperationType="cashout";
                    }
                    //newsubcription
                    if (KEY == null || KEY == "")
                    {
                        ViewBag.message = "Empty data was submited. Please enter search criterial";
                        return PartialView("_DataNotFound", new CashDesk());
                    }

                    var cashDesk = await _cashDeskService.GetAccountByAccountNumberSearch(KEY, path);
                    if (cashDesk == null)
                    {

                        ViewBag.message = $"{KEY} was not found in the database.";
                        return PartialView("_DataNotFound", new CashDesk());
                    }
                    var nonLoanAccounts = cashDesk?.BulkDeposits?
                    .Where(x => x.AccountType != null && !x.AccountType.ToLower().Contains("loan"))
                    .ToList();

                    bool allAccountsAreMB = true;

                    if (nonLoanAccounts != null && nonLoanAccounts.Any())
                    {

                        foreach (var acc in nonLoanAccounts)
                        {
                            if (string.IsNullOrWhiteSpace(acc.AccountNumber) || !acc.AccountNumber.Trim().ToUpper().StartsWith("MB"))
                            {
                                allAccountsAreMB = false;
                                
                            }
                            else
                            {
                                allAccountsAreMB=true;
                                break;
                            }
                        }

                    }

                    ViewBag.AllAccountsAreMB = allAccountsAreMB;
                    ViewBag.Operation = path;

                    ViewBag.Operation = path;
                    return PartialView(partialView, cashDesk);

                }
                else if (path == "new_depositor")
                {
                    return PartialView(partialView, new CashDesk());

                }
                ViewBag.message = "Invalid option selected";
                return PartialView("_NoRecordFound", new CashDesk());

            }
            catch (Exception ex)
            {

                ViewBag.message = ex.Message; // Store error message
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