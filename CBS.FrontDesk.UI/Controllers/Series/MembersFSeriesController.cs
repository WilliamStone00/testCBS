using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.BusinessService.CustomerManagement;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
using CBS.FrontDesk.Data.Message;
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
                else if (path == "members_account")
                {
                    if (KEY == null || KEY == "")
                    {
                        ViewBag.message = "Empty data was submited. Please enter search criterial";
                        return PartialView("_DataNotFound", new CashDesk());
                    }
                    var cashDesk = await _cashDeskService.GetAccountByAccountNumberSearch(KEY);
                    if (cashDesk == null)
                    {

                        ViewBag.message = $"{KEY} was not found in the database.";
                        return PartialView("_DataNotFound", new CashDesk());
                    }
                    ViewBag.Operation = path;
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
            if (path == "export_transactions")
            {
                var account = await _accountServices.GetTransactionsAsync();
                this.HttpContext.Session["rptSource"] = _accountServices.GetTransactionHistoryExports(account.TransactionHistories);
                if (!account.TransactionHistories.Any())
                {
                    this.HttpContext.Session["rptSource"] = "empty";
                }
                this.HttpContext.Session["rptType"] = rptType;
                this.HttpContext.Session["ReportName"] = $"{ReportName}.rpt";
                this.HttpContext.Session["rptpath"] = $"~/{reportpath}/" + ReportName + ".rpt";
                this.HttpContext.Session["rpttitle"] = $"{fileTitle}";

            }
            else if (path == "customer_account_transaction")
            {
                var transactionHistories = await _accountServices.GetCustomerTransactionsByAccountNumber(KEY);
                this.HttpContext.Session["rptSource"] = _accountServices.GetTransactionHistoryExports(transactionHistories);
                if (!transactionHistories.Any())
                {
                    this.HttpContext.Session["rptSource"] = "empty";
                }
                this.HttpContext.Session["rptType"] = rptType;
                this.HttpContext.Session["ReportName"] = $"{ReportName}.rpt";
                this.HttpContext.Session["rptpath"] = $"~/{reportpath}/" + ReportName + ".rpt";
                this.HttpContext.Session["rpttitle"] = $"{fileTitle}";
            }
            else if (path == "transactions_by_dates")
            {
                var transactionHistories = await _accountServices.GetCustomerTransactionsByAccountNumber(KEY);
                this.HttpContext.Session["rptSource"] = _accountServices.GetTransactionHistoryExports(transactionHistories);
                if (!transactionHistories.Any())
                {
                    this.HttpContext.Session["rptSource"] = "empty";
                }
                this.HttpContext.Session["rptType"] = rptType;
                this.HttpContext.Session["ReportName"] = $"{ReportName}.rpt";
                this.HttpContext.Session["rptpath"] = $"~/{reportpath}/" + ReportName + ".rpt";
                this.HttpContext.Session["rpttitle"] = $"{fileTitle}";
            }
            else if (path == "receipts")
            {
                this.HttpContext.Session["rptType"] = "ReportParameterLess";
                this.HttpContext.Session["ReportName"] = $"Receipts.rpt";
                this.HttpContext.Session["rptpath"] = $"~/AppFiles/Reporting/Transactions/Reciepts/Receipts.rpt";
                this.HttpContext.Session["rpttitle"] = $"MemberReceipts";
            }
            else if (path == "customer_account_transaction_rpt")
            {
                var transactionHistories = await _accountServices.GetCustomerTransactionsByAccountNumber(KEY);
                var customer = await _individualProfileServices.GetSingleCustomer(transactionHistories.FirstOrDefault().Account.CustomerId);
                if (customer == null)
                {
                    return Json(new { success = true, status = false, message = "Failed getting customer" }, JsonRequestBehavior.AllowGet);

                }
                var branch = await _branchServices.GetBranch(customer.branchId);
                var rpt = _accountServices.MaprptSource(transactionHistories, branch, customer);

                string accountnumber = null;
                if (!transactionHistories.Any())
                {
                    return Json(new { success = true, status = false, message = "No data was found." }, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    accountnumber = transactionHistories.FirstOrDefault().AccountNumber;
                }
                this.HttpContext.Session["rptSource"] = rpt;
                this.HttpContext.Session["rptType"] = "ReportParameterLess";
                this.HttpContext.Session["ReportName"] = $"IAccountStatement.rpt";
                this.HttpContext.Session["rptpath"] = $"~/AppFiles/Reporting/Transactions/Statement/IAccountStatement.rpt";
                this.HttpContext.Session["rpttitle"] = $"{accountnumber}_Statement";
            }
            else if (path == "by_date_history")
            {
                //this.HttpContext.Session["rptSource"] = _helper._object.Receipts;
                //this.HttpContext.Session["DateFrom"] = datefrom;
                //this.HttpContext.Session["DateTo"] = dateto;
            }


            return Json(new { success = true, status = false, message = "OK" }, JsonRequestBehavior.AllowGet);

        }
    }
}