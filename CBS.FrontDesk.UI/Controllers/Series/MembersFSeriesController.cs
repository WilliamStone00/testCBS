using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.BusinessService.CustomerManagement;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountOperation;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.ReportDataSetDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Series
{
    public class MembersFSeriesController : BaseController
    {
        // GET: MembersFSeries
        private readonly CashDeskServices _cashDeskService;
        private readonly AccountServices _accountServices;
        private readonly IndividualProfileServices _individualProfileServices;
        private readonly BranchServices _branchServices;
        public MembersFSeriesController(CashDeskServices cashDeskService, AccountServices accountServices = null, IndividualProfileServices individualProfileServices = null, BranchServices branchServices = null)
        {
            _cashDeskService = cashDeskService;
            _accountServices = accountServices;
            _individualProfileServices = individualProfileServices;
            _branchServices = branchServices;
        }
        public ActionResult Index()
        {
            return View();
        }
        public async Task<ActionResult> F5MembersAccountStatement()
        {
            return View();
        }
        public async Task<ActionResult> F2MembersGeneratSituation(string KEY)
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
            return View(cashDesk);
        }
        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = "_DataNotFound", string path = null, string serviceOption = null)
        {
            try
            {
                if (path == "list")
                {
                    var cashDesk = await _cashDeskService.GetMembers();
                    return PartialView(partialView, cashDesk);
                }
                else if (path == "members_profile")
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
                else if (path == "print_by_date")
                {
                    return PartialView(partialView, new CashDesk { CustomerId = KEY });

                }
                else if (path == "members_account")
                {
                    if (KEY == null || KEY == "")
                    {
                        ViewBag.message = "Empty data was submited. Please enter search criterial";
                        return PartialView("_DataNotFound", new CashDesk());
                    }
                    var cashDesk = await _cashDeskService.GetAccountByAccountNumberSearch(KEY,"F5");
                    if (cashDesk == null)
                    {

                        ViewBag.message = $"{KEY} was not found in the database.";
                        return PartialView("_DataNotFound", new CashDesk());
                    }
                    ViewBag.Operation = path;
                    return PartialView(partialView, cashDesk);

                }
                else if (path == "transactions")
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
                    var transactionHistories = await _cashDeskService.GetCustomerTransactionsByCustomerNumber(KEY);
                    cashDesk.Transactions = transactionHistories.ToList();
                    return PartialView(partialView, cashDesk);
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
        public async Task<ActionResult> GetReport(string rptType = null, string ReportName = null, string serviceoption = null, string reportpath = null, string fileTitle = null, string ReadOptions = null, string KEY = null, string path = null, string yearID = null, string datefrom = null, string dateto = null)
        {
            try
            {
                switch (path)
                {
                    case "receipts":
                        var receiptTransaction = await _accountServices.GetTransactionsAsync(KEY);
                        var receiptCustomer = receiptTransaction != null ? await _individualProfileServices.GetSingleCustomer(receiptTransaction.CustomerId) : null;
                        var receiptBranch = receiptCustomer != null ? await _branchServices.GetBranch(receiptCustomer.branchId) : null;
                        var receiptUser = receiptTransaction != null ? await _cashDeskService.RetrieveUserFromSession(receiptTransaction.Teller.inUsedByUserId) : null;

                        if (receiptTransaction == null)
                            return Json(new { success = false, status = false, message = "No transactions are found." }, JsonRequestBehavior.AllowGet);

                        if (receiptCustomer == null)
                            return Json(new { success = false, status = false, message = "Failed getting customer" }, JsonRequestBehavior.AllowGet);

                        var receiptRpt = _cashDeskService.MaprptSource(receiptTransaction, receiptBranch, receiptUser, receiptCustomer);
                        var receiptTransactionReports = new List<TransactionReportDS> { receiptRpt };
                        var receiptRptSource = receiptTransactionReports.ToList();

                        SetSessionVariables(receiptRptSource, "Receipts.rpt", "~/AppFiles/Reporting/Transactions/Reciepts/Receipts.rpt", $"{receiptTransaction.TransactionReference}");
                        break;

                    case "customer_account_transaction_rpt":
                        var accountTransactionHistories = await _accountServices.GetCustomerTransactionsByAccountNumber(KEY);

                        if (!accountTransactionHistories.Any())
                            return Json(new { success = false, status = false, message = "No transactions are found." }, JsonRequestBehavior.AllowGet);

                        var accountCustomer = await _individualProfileServices.GetSingleCustomer(accountTransactionHistories.FirstOrDefault().Account.CustomerId);
                        var accountBranch = await _branchServices.GetBranch(accountCustomer.branchId);
                        var accountNumber = accountTransactionHistories.Any() ? accountTransactionHistories.FirstOrDefault().AccountNumber : null;
                        var accountRpt = _accountServices.MaprptSource(accountTransactionHistories, accountBranch, accountCustomer);

                        SetSessionVariables(accountRpt, "IAccountStatement.rpt", "~/AppFiles/Reporting/Transactions/Statement/IAccountStatement.rpt", $"{accountNumber ?? "Unknown"}_Statement", "ReportParameterLess");

                        if (!accountTransactionHistories.Any())
                            return Json(new { success = true, status = false, message = "No data was found." }, JsonRequestBehavior.AllowGet);
                        break;

                    case "all_by_dates_customer_account_transaction":
                        var printDate = new PrintDate { CustomerID = KEY, DateFrom = datefrom, DateTo = dateto };
                        var dateTransactionHistories = await _accountServices.GetCustomerTransactionsByCustomerNumberAndByDates(printDate);

                        if (!dateTransactionHistories.Any())
                            return Json(new { success = false, status = false, message = "No transaction found" }, JsonRequestBehavior.AllowGet);

                        var dateCustomer = await _individualProfileServices.GetSingleCustomer(KEY);

                        if (dateCustomer == null)
                            return Json(new { success = false, status = false, message = "Failed getting customer" }, JsonRequestBehavior.AllowGet);

                        var dateBranch = await _branchServices.GetBranch(dateCustomer.branchId);
                        var dateRpt = _accountServices.MaprptSource(dateTransactionHistories, dateBranch, dateCustomer);

                        SetSessionVariables(dateRpt, "IAccountStatementGroupByAccounts.rpt", "~/AppFiles/Reporting/Transactions/Statement/IAccountStatementGroupByAccounts.rpt", $"{dateCustomer.name}_Statement");

                        SetSessionVariables(datefrom, dateto); // Set date range session variables

                        break;

                    default:
                        break;
                }

                return Json(new { success = true, status = false, message = "OK" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                return Json(new { success = false, status = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private void SetSessionVariables(object rptSource, string reportName, string reportPath, string reportTitle, string rptType = null)
        {
            this.HttpContext.Session["rptSource"] = rptSource;
            this.HttpContext.Session["ReportName"] = reportName;
            this.HttpContext.Session["rptpath"] = reportPath;
            this.HttpContext.Session["rpttitle"] = reportTitle;
            this.HttpContext.Session["rptType"] = rptType;
        }

        private void SetSessionVariables(string datefrom, string dateto)
        {
            this.HttpContext.Session["DateFrom"] = datefrom;
            this.HttpContext.Session["DateTo"] = dateto;
        }

    }
}