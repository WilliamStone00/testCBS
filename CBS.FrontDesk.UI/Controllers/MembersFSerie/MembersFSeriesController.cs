using CBS.BusinessService;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.BusinessService.CustomerManagement;
using CBS.BusinessService.LoanP;
using CBS.BusinessService.Repayment;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.CustomerManagement;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountOperation;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.ReportDataSetDto;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Series
{
    //[CheckSessionTimeOutAttribute]

    public class MembersFSeriesController : BaseController
    {
        // GET: MembersFSeries
        private readonly CashDeskServices _cashDeskService;
        private readonly AccountServices _accountServices;
        private readonly LoanServices _loanServices;
        private readonly RefundServices _refundServices;

        private readonly IndividualProfileServices _individualProfileServices;
        private readonly BranchServices _branchServices;
        public MembersFSeriesController(CashDeskServices cashDeskService, AccountServices accountServices = null, IndividualProfileServices individualProfileServices = null, BranchServices branchServices = null, LoanServices loanServices = null, RefundServices refundServices = null)
        {
            _cashDeskService = cashDeskService;
            _accountServices = accountServices;
            _individualProfileServices = individualProfileServices;
            _branchServices = branchServices;
            _loanServices = loanServices;
            _refundServices = refundServices;
        }

        public async Task< bool> LoadMemberAccountsAndLoans(string key, string loanFilter)
        {
            // 1. Get CashDesk by account number
            var cashDesk = await _cashDeskService.GetAccountByAccountNumberSearch(key, "F5");

            // 2. Prepare Accounts dropdown
            ViewBag.Accounts = cashDesk.Accounts.Select(a => new SelectListItem
            {
                Value = a.id,
                Text = $"{a.accountNumber} - {a.accountName}"
            }).ToList();

            // 3. Load Loans for the member
            var loans = await _cashDeskService.GetMembersLoans(
                cashDesk.CustomerId,
                loanFilter = "All"
            );

            cashDesk.Loans = loans;

            // 4. Prepare Loans dropdown
            ViewBag.Loans = loans.Select(l => new SelectListItem
            {
                Value = l.Id,
                Text = $"{l.Id} - {l.LoanType}"
            }).ToList();
                 
                return true;
        }


        public async Task<ActionResult> Index()
        {
            ViewBag.Branches=await _branchServices.GetBranches();
            return View();
        }
        public async Task<ActionResult> F5MembersAccountStatement()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> LoadMembersData(GetCustomersForDataTableQuery query)
        {
            try
            {
                var dataTable = await _individualProfileServices.GetDataTableAsync(query, "MemberSituation");

                var customerList = JsonConvert.DeserializeObject<List<CustomerLightDto>>(
                    JsonConvert.SerializeObject(dataTable.data)
                );
                var branches = await _branchServices.GetBranches();
                var customers = _individualProfileServices.MapToDtoOrdered(customerList, branches.ToList()); // Optional: for client-side sorting/grouping

                return Json(new
                {
                    draw = query.Options?.draw ?? "1",
                    recordsTotal = dataTable.recordsTotal,
                    recordsFiltered = dataTable.recordsFiltered,
                    data = customers
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Error loading member data.");
            }
        }

        public async Task<ActionResult> F2MembersGeneratSituation(string KEY)
        {
            //if (KEY == null || KEY == "")
            //{
            //    ViewBag.message = "Empty data was submited. Please enter search criterial";
            //    return PartialView("_DataNotFound", new CashDesk());
            //}
            //var cashDesk = await _cashDeskService.GetMember(KEY);
            //if (cashDesk == null)
            //{
            //    ViewBag.message = $"{KEY} was not found in the database.";
            //    return PartialView("_DataNotFound", new CashDesk());
            //}

            if (KEY == null || KEY == "")
            {
                ViewBag.message = "Empty data was submited. Please enter search criterial";
                return PartialView("_DataNotFound", new CashDesk());
            }
            var cashDesk = await _cashDeskService.GetAccountByAccountNumberSearch(KEY, "F5");
            if (cashDesk == null)
            {

                ViewBag.message = $"{KEY} was not found in the database.";
                return PartialView("_DataNotFound", new CashDesk());
            }
            //ViewBag.Operation = path;
            return View(cashDesk);


        }



        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = "_DataNotFound", string path = null, string serviceOption = null)
        {
            try
            {
                //if (path == "list")
                //{
                //    var cashDesk = await _cashDeskService.GetMembers();
                //    return PartialView(partialView, cashDesk);
                //}
                //else
                if (path == "members_profile")
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
                    var cashDesk = await _cashDeskService.GetAccountByAccountNumberSearch(KEY, "F5");
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
                }else if(path == "Report")
                {
                    // var data = await _pcmfLoanPurposeService.GetByIdAsync(KEY);
                   await LoadMemberAccountsAndLoans(KEY, path);
                    return PartialView(partialView);
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
                        var receiptBranch = receiptCustomer != null ? await _branchServices.GetBranch(receiptCustomer.BranchId) : null;
                        var receiptUser = receiptTransaction != null ? await _cashDeskService.RetrieveUserFromSession(receiptTransaction.CreatedBy) : null;

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
                        var accountBranch = await _branchServices.GetBranch(accountCustomer.BranchId);
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

                        var dateBranch = await _branchServices.GetBranch(dateCustomer.BranchId);
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
        [HttpGet]
        //GetMembersLoans(string customerId, string queryParameter)
        //public async Task<ActionResult> GetFilteredLoans(string filter, string memberId)
        //{
        //    // Fetch all loans first
        //    var loans = await _cashDeskService.GetMembersLoans(memberId, filter); // Replace this with your actual loan retrieval logic

        //    return Json(loans, JsonRequestBehavior.AllowGet);
        //}
        public async Task<ActionResult> GetFilteredLoansPartial(string filter, string memberId)
        {
            var loans = await _cashDeskService.GetMembersLoans(memberId, filter);
            var cdesk = new CashDesk { CustomerId=memberId, Loans=loans };
            return PartialView("_LoansTablePartial", cdesk); // View name must match Razor file below
        }
        public async Task<ActionResult> LoanDetailsPartial(string loanId)
        {
            var loan = await _loanServices.GetLoan(loanId); // Include all accounts + loans
            var cashDesk = new CashDesk { CustomerId=loan.CustomerId, Loan=loan, Refunds=loan.Refunds };
            if (loan == null)
                return PartialView("_LoanNotFound");
            ViewBag.SelectedLoan = loan;
            return PartialView("_LoanDetailsModalPartial", cashDesk);
        }
        // e.g., MembersController (or LoanController) 
        [HttpGet]
        public async Task<ActionResult> MemberRefundsPartial(string customerId, DateTime? dateFrom, DateTime? dateTo)
        {
            if (string.IsNullOrWhiteSpace(customerId))
                return PartialView("_MemberLoanRefunds", Enumerable.Empty<Refund>());
            var refunds = await _refundServices
                .GetRefundsByCustomerId(customerId, true, null, dateFrom, dateTo);
            var cashDesk = new CashDesk { CustomerId=customerId, Refunds=refunds };
            return PartialView("_MemberLoanRefunds", cashDesk);
        }

        [HttpGet]
        public async Task<ActionResult> RefundDetailsPartial(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return new HttpStatusCodeResult(400, "Missing refund id.");

            var refund = await _refundServices.GetRefundById(id);
            if (refund == null) return HttpNotFound("Refund not found.");

            // Ensure navigations exist (adjust if your service already eager-loads)
            refund.RefundDetails     = refund.RefundDetails     ?? new List<RefundDetail>();
            refund.LoanAmortizations = refund.LoanAmortizations ?? new List<LoanAmortization>();
            var loan = refund.Loan;

            // Prefer refund’s lists, fall back to loan’s
            var amortList = (refund.LoanAmortizations?.Any() == true ? refund.LoanAmortizations
                             : loan?.LoanAmortizations) ?? new List<LoanAmortization>();
            var details = refund.RefundDetails ?? new List<RefundDetail>();

            // Header allocation
            var allocPrincipal = refund.Principal;
            var allocInterest = refund.Interest;
            var allocTax = refund.Tax;
            var allocPenalty = refund.Penalty;
            var allocTotal = allocPrincipal + allocInterest + allocTax + allocPenalty;

            // Detail totals
            var detCollected = details.Sum(d => d.CollectedAmount);
            var detPrin = details.Sum(d => d.PrincipalAmount);
            var detInt = details.Sum(d => d.Interest);
            var detTax = details.Sum(d => d.TaxAmount);
            var detPen = details.Sum(d => d.PenaltyAmount);
            var detBal = details.Sum(d => d.Balance);

            // Loan summary (guards)
            string loanStatus = loan?.LoanStatus ?? "N/A";
            string loanId = loan?.Id ?? refund.LoanId ?? "N/A";
            decimal loanAmount = loan?.LoanAmount ?? 0m;
            decimal loanPaid = loan?.Paid ?? 0m;
            decimal loanBalance = loan?.Balance ?? refund.Balance;
            decimal loanDue = loan?.DueAmount ?? 0m;
            decimal loanRate = loan?.InterestRate ?? 0m;
            DateTime? loanDate = loan?.LoanDate;
            DateTime? lastRefDt = loan?.LastRefundDate;

            // Amort summary
            int totalInst = amortList.Count;
            int completedInst = amortList.Count(a => a.IsCompleted);
            int overdueInst = amortList.Count(a => a.PreviousInstallmentDue ||
                                          string.Equals(a.Status, "Overdue", StringComparison.OrdinalIgnoreCase));
            decimal dueSum = amortList.Sum(a => a.Due);
            decimal paidSum = amortList.Sum(a => a.Paid);
            decimal balSum = amortList.Sum(a => a.Balance);
            DateTime? nextDue = amortList
                .Where(a => a.Due > 0 && !a.IsCompleted)
                .OrderBy(a => a.NextPaymentDate == default ? DateTime.MaxValue : a.NextPaymentDate)
                .Select(a => a.NextPaymentDate == default ? (DateTime?)null : a.NextPaymentDate)
                .FirstOrDefault();

            string RefundBadge() =>
                refund.IsReversal ? "badge bg-danger"
              : (refund.IsCompleted || refund.IsComplete) ? "badge bg-success"
              : "badge bg-warning text-dark";

            string LoanBadge(string s)
            {
                s = (s ?? string.Empty).ToLowerInvariant();
                switch (s)
                {
                    case "pending": return "badge bg-warning text-dark";
                    case "open":
                    case "disbursed": return "badge bg-primary";
                    case "rejected": return "badge bg-danger";
                    case "refinanced": return "badge bg-info text-dark";
                    case "rescheduled": return "badge bg-dark";
                    case "restructured": return "badge bg-secondary text-white";
                    default: return "badge bg-secondary";
                }
            }

            var vm = new RefundDetailsVM
            {
                Refund = refund,
                Loan   = loan,

                ProductName = refund.LoanProduct?.ProductName ?? refund.LoanProduct?.ProductName,
                LoanId      = loanId,
                LoanStatus  = loanStatus,
                RefundStatusBadgeClass = RefundBadge(),
                LoanStatusBadgeClass   = LoanBadge(loanStatus),

                LoanAmount   = loanAmount,
                LoanPaid     = loanPaid,
                LoanBalance  = loanBalance,
                LoanDueAmount= loanDue,
                LoanRate     = loanRate,
                LoanDate     = loanDate,
                LastRefundDate = lastRefDt,

                RefundDetails = details,
                Amortizations = amortList,

                AllocPrincipal = allocPrincipal,
                AllocInterest  = allocInterest,
                AllocTax       = allocTax,
                AllocPenalty   = allocPenalty,
                AllocTotal     = allocTotal,
                AllocationsConsistent = (refund.Amount == refund.Paid) && Math.Abs(refund.Amount - allocTotal) <= 0.5m,

                DetailCollected = detCollected,
                DetailPrincipal = detPrin,
                DetailInterest  = detInt,
                DetailTax       = detTax,
                DetailPenalty   = detPen,
                DetailBalance   = detBal,

                TotalInstallments     = totalInst,
                CompletedInstallments = completedInst,
                OverdueInstallments   = overdueInst,
                AmortDueSum           = dueSum,
                AmortPaidSum          = paidSum,
                AmortBalanceSum       = balSum,
                NextDueDate           = nextDue
            };

            // Attach VM to CashDesk
            var cashDesk = new CashDesk
            {
                CustomerId = refund.CustomerId,
                Refund     = refund,
                Loan       = loan,
                RefundVM   = vm
            };

            return PartialView("_RefundDetails", cashDesk);
        }


    }
}
