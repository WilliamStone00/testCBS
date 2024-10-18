using CBS.BusinessService.CustomerManagement;
using CBS.FrontDesk.Data.Message;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using CBS.BusinessService.Accounts;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
using CBS.BusinessService;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountOperation;
using ClosedXML.Excel;
using System.Collections.Generic;
using System.IO;
using CBS.FrontDesk.Data.Entity.Config;

namespace CBS.FrontDesk.UI.Controllers
{
    //[CheckSessionTimeOutAttribute]

    public class OperationController : BaseController
    {
        // GET: Operation/Transfer
        private readonly AccountServices _acountServices;
        private readonly TellerProvissioningServices _services;
        private readonly LoanServices _loanServices;
        private readonly BranchServices _branchServices;
        private readonly IndividualProfileServices _individualProfileServices;
        private readonly DailyTellerServices _dailyTellerServices;
        private readonly TellerServices _tellerServices;

        public OperationController(AccountServices acountServices, TellerProvissioningServices services = null, LoanServices loanServices = null, BranchServices branchServices = null, IndividualProfileServices individualProfileServices = null, TellerServices tellerServices = null, DailyTellerServices dailyTellerServices = null)
        {
            _acountServices = acountServices;
            _services = services;
            _loanServices = loanServices;
            _branchServices = branchServices;
            _individualProfileServices = individualProfileServices;
            _tellerServices = tellerServices;
            _dailyTellerServices = dailyTellerServices;
        }


        public async Task<ActionResult> AccountDetails(string KEY = null)
        {

            //502846231941202110
            ViewBag.Sources = _acountServices.GetPaymentSources();
            var account = await _acountServices.GetAccountByAccountNumber(KEY);
            return View(account);
        }
        public async Task<ActionResult> Deposit(string KEY = null)
        {
            //Statement

            ViewBag.Operation = "Deposit";
            ViewBag.Sources = _acountServices.GetPaymentSources();
            var account = await _acountServices.GetAccountByAccountNumber(KEY);
            return View(account);
        }
        public async Task<ActionResult> Statement(string KEY = null)
        {
            ViewBag.Operation = "Deposit";
            ViewBag.Sources = _acountServices.GetPaymentSources();
            var account = await _acountServices.GetAccountByAccountNumber(KEY);
            return View(account);
        }
        public async Task<ActionResult> LoanRepayment(string KEY = null)
        {
            ViewBag.Operation = "LoanRepayment";
            ViewBag.Sources = _acountServices.GetPaymentSources();
            var account = await _acountServices.GetAccountByAccountNumber(KEY);
            var loan = await _loanServices.GetLoanByCustomerID(new GetAllLoanByCustomerIdQuery { CustomerId = KEY, QueryParameter = "Open" });
            account.Loans = loan.ToList();
            account.LoanRepayments = loan.SelectMany(x => x.Refunds).ToList();
            return View(account);
        }
        //LoanRepayment
        public async Task<ActionResult> Withdrawal(string KEY = null)
        {
            ViewBag.Operation = "Withdrawal";
            ViewBag.Sources = _acountServices.GetPaymentSources();
            var account = await _acountServices.GetAccountByAccountNumber(KEY);
            return View(account);
        }
        public async Task<ActionResult> Transactions()
        {
            await LoadDropdowns();
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> OperationDownload(GetAllTransactionsByDatesAndBranchQuery request)
        {
            ModelState.Clear();
            TryValidateModel(request, nameof(request));

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select((e, index) => $"{index + 1}. {e.ErrorMessage}")
                    .ToList();
                string error = string.Join("<br/>", errors);
                return Json(new { success = false, message = error });
            }

            var branch = await _branchServices.GetBranch(request.BranchID.ToString());

            if (branch == null)
            {
                return Json(new { success = false, message = "Branch not found" });
            }

            var response = await _acountServices.GetTransactionsAsync(request);
            var data = _acountServices.MapTransactionHistoryToExport(response, branch).ToList();

            if (data == null || !data.Any())
            {
                return Json(new { success = false, message = "No data available for the selected query criteria." });
            }

            // Create Excel file in the same action
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add($"Transaction_History_{branch.BranchCode}");

                // File title and header information merged into a single cell
                worksheet.Cell(1, 1).Value =
                    $"{branch.Bank.Name.ToUpper()}\n" +
                    $"BRANCH: {branch.Name.ToUpper()}\n" +
                    $"BRANCH CODE: {branch.BranchCode}, TEL: {branch.Telephone}\n" +
                    $"LOCATION: {branch.Address.ToUpper()}\n" +
                    $"TRANSACTIONS FROM {request.DateFrom:d} TO {request.DateTo:d}\n" +
                    $"DATE PRINTED: {DateTime.Now:dd-MM-yyyy hh:mm:ss}".ToUpper() + $" BY {Session["FullName"]}";

                // Merging the first six rows into one single cell (1,1) to (6,19)
                var mergedRange = worksheet.Range(1, 1, 7, 19);
                mergedRange.Merge();

                // Set alignment
                mergedRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                mergedRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                mergedRange.Style.Alignment.WrapText = true;

                // Apply font styling
                mergedRange.Style.Font.Bold = false;
                mergedRange.Style.Font.FontSize = 12;
                mergedRange.Style.Font.FontName = "Bahnschrift Light";

                // Apply border styling (blue border and bold)
                mergedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
                mergedRange.Style.Border.OutsideBorderColor = XLColor.Blue;

                // Adjust all widths automatically to fit content
                worksheet.Columns().AdjustToContents();

                worksheet.Range(8, 1, 8, 19).Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
                worksheet.Range(8, 1, 8, 19).Style.Border.OutsideBorderColor = XLColor.Blue;

                // Adding table headers (including AccountType)
                var currentRow = 8; // Headers on row 8
                worksheet.Cell(currentRow, 1).Value = "ACC.Name";
                worksheet.Cell(currentRow, 2).Value = "ACC.Number";
                worksheet.Cell(currentRow, 3).Value = "ACC.Type"; // Added AccountType header
                worksheet.Cell(currentRow, 4).Value = "M.REF";
                worksheet.Cell(currentRow, 5).Value = "Date";
                worksheet.Cell(currentRow, 6).Value = "ACC.Date";
                worksheet.Cell(currentRow, 7).Value = "Amount";
                worksheet.Cell(currentRow, 8).Value = "Fee";
                worksheet.Cell(currentRow, 9).Value = "B.Forward";
                worksheet.Cell(currentRow, 10).Value = "Balance";
                worksheet.Cell(currentRow, 11).Value = "Reference";
                worksheet.Cell(currentRow, 12).Value = "Cashier";
                worksheet.Cell(currentRow, 13).Value = "Representatives";
                worksheet.Cell(currentRow, 14).Value = "Operation";
                worksheet.Cell(currentRow, 15).Value = "F.Charge";
                worksheet.Cell(currentRow, 16).Value = "S.Charge";
                worksheet.Cell(currentRow, 17).Value = "Debit";
                worksheet.Cell(currentRow, 18).Value = "Credit";
                worksheet.Cell(currentRow, 19).Value = "I.B";

                // Applying style to headers
                worksheet.Range(currentRow, 1, currentRow, 19).Style.Font.Bold = true;
                worksheet.Range(currentRow, 1, currentRow, 19).Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
                worksheet.Range(currentRow, 1, currentRow, 19).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                worksheet.Range(currentRow, 1, currentRow, 19).Style.Border.OutsideBorderColor = XLColor.Blue;

                // Write the data rows starting from row 9
                currentRow++; // Move to the next row for data
                foreach (var transaction in data)
                {
                    worksheet.Cell(currentRow, 1).Value = transaction.MemberName;
                    worksheet.Cell(currentRow, 2).Value = transaction.AccountNumber;
                    worksheet.Cell(currentRow, 3).Value = transaction.AccountType; // Added AccountType data
                    worksheet.Cell(currentRow, 4).Value = transaction.CustomerReference;
                    worksheet.Cell(currentRow, 5).Value = transaction.Date;
                    worksheet.Cell(currentRow, 6).Value = transaction.AccountingDate;
                    worksheet.Cell(currentRow, 7).Value = transaction.Amount;
                    worksheet.Cell(currentRow, 8).Value = transaction.Fee;
                    worksheet.Cell(currentRow, 9).Value = transaction.NewBalance;
                    worksheet.Cell(currentRow, 10).Value = transaction.Balance;
                    worksheet.Cell(currentRow, 11).Value = transaction.Reference;
                    worksheet.Cell(currentRow, 12).Value = transaction.TellerName;
                    worksheet.Cell(currentRow, 13).Value = transaction.ThirdPartyName;
                    worksheet.Cell(currentRow, 14).Value = transaction.Operation;
                    worksheet.Cell(currentRow, 15).Value = transaction.WithdrawalFormCharge;
                    worksheet.Cell(currentRow, 16).Value = transaction.OperationCharge;
                    worksheet.Cell(currentRow, 17).Value = transaction.Debit;
                    worksheet.Cell(currentRow, 18).Value = transaction.Credit;
                    worksheet.Cell(currentRow, 19).Value = transaction.InterBranch;

                    currentRow++;
                }

                worksheet.Columns("F", "I").Style.NumberFormat.Format = "#,##0";
                worksheet.Columns("Q", "R").Style.NumberFormat.Format = "#,##0";

                // Adding borders to the data area
                worksheet.Range(8, 1, currentRow - 1, 19).Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
                worksheet.Range(8, 1, currentRow - 1, 19).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                worksheet.Range(8, 1, currentRow - 1, 19).Style.Border.OutsideBorderColor = XLColor.Blue;

                worksheet.Columns().AdjustToContents();
                var fileName = $"Transaction_His_{branch.BranchCode}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

                // Return the file as an Excel download
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }
        public async Task<ActionResult> Transfers()
        {
            return View();
        }
        public async Task<ActionResult> TransferPending()
        {
            return View();
        }
        public async Task<ActionResult> TransferRequest()
        {
            //var account = await _acountServices.SourceAndDestinationAccount();
            //var conf = await _acountServices.GetSavingConfigurationAggregates();
            //ViewBag.Source = account;
            //ViewBag.Destination = account;
            //ViewBag.TransferTypes = conf.transferTypes;
            return View(new Account());
        }

        //Transactions
        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string serviceOption = null)
        {
            try
            {
                ViewBag.KEY = KEY;
                //if (path == "transactions")
                //{
                //    var account = await _acountServices.GetTransactionsAsync();
                //    return PartialView(partialView, account);
                //}
                //else if (path == "confirmation_request")
                if (path == "confirmation_request")
                {
                    var Statuses = await _acountServices.GetSavingConfigurationAggregates();
                    ViewBag.Status = Statuses.Statuses;
                    var transfer = await _acountServices.GetTransfer(KEY);
                    var account = new Account { Transfer = transfer, TransferConfirmation = new TransferConfirmation { TransferId = transfer.Id } };
                    return PartialView(partialView, account);
                }
                else if (path == "details")
                {
                    var transfer = await _acountServices.GetTransfer(KEY);
                    var account = new Account { Transfer = transfer };
                    return PartialView(partialView, account);
                }
                else if (path == "pending_request")
                {

                    var transfers = await _acountServices.GetPendingTransfers();
                    var account = new Account { Transfers = transfers };
                    return PartialView(partialView, account);
                }
                else if (path == "all_transfer_request")
                {
                    var transfers = await _acountServices.GetTransfers();
                    var account = new Account { Transfers = transfers };
                    return PartialView(partialView, account);
                }
                else if (path == "transfer_request")
                {
                    var account = await _acountServices.SourceAndDestinationAccount();
                    var conf = await _acountServices.GetSavingConfigurationAggregates();
                    ViewBag.Source = account;
                    ViewBag.Destination = account;
                    ViewBag.TransferTypes = conf.transferTypes;
                    return PartialView(partialView, new Account());
                }
                else
                {
                    var account = await _acountServices.GetAccountByAccountNumber(KEY);
                    ViewBag.Sources = _acountServices.GetPaymentSources();
                    return PartialView(partialView, account);
                }

            }
            catch (Exception ex)
            {

                TempData["ErrorMessage"] = ex.Message; // Store error message
                return RedirectToAction("Index", "Error"); // Redirect to error page
            }
        }
        [HttpPost]
        public async Task<ActionResult> Deposit(Account model)
        {
            if (model.OperationType == "Deposit")
            {

                var data = await _acountServices.Deposit(model.DepositRequest);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else if (model.OperationType == "InitialDeposit")
            {
                var data = await _acountServices.MakeInitialDeposit(model.AccountActivationRequest);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else if (model.OperationType == "Withdrawal")
            {

                var data = await _acountServices.Withdrawal(model.DepositRequest);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else if (model.OperationType == "LoanRepayment")
            {

                var data = await _acountServices.LoanRepayment(model.DepositRequest);

                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else if (model.OperationType == "Transfer")
            {
                var data = await _acountServices.Transfer(model.TransferRequest);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else if (model.OperationType == "TransferConfirmation")
            {
                var data = await _acountServices.TransferConfirmation(model.TransferConfirmation);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else
            {
                return Json(new { success = false, status = false, message = "Fill the required fields." });
            }
        }
        [HttpPost]
        public async Task<ActionResult> GetReport(string rptType = null, string ReportName = null, string serviceoption = null, string reportpath = null, string fileTitle = null, string ReadOptions = null, string KEY = null, string path = null, string yearID = null, string datefrom = null, string dateto = null)
        {
            if (path == "export_transactions")
            {
                //var account = await _acountServices.GetTransactionsAsync();
                //this.HttpContext.Session["rptSource"] = _acountServices.GetTransactionHistoryExports(account.TransactionHistories);
                //if (!account.TransactionHistories.Any())
                //{
                //    this.HttpContext.Session["rptSource"] = "empty";
                //}
                //this.HttpContext.Session["rptType"] = rptType;
                //this.HttpContext.Session["ReportName"] = $"{ReportName}.rpt";
                //this.HttpContext.Session["rptpath"] = $"~/{reportpath}/" + ReportName + ".rpt";
                //this.HttpContext.Session["rpttitle"] = $"{fileTitle}";

            }
            else if (path == "customer_account_transaction")
            {
                var transactionHistories = await _acountServices.GetCustomerTransactionsByAccountNumber(KEY);
                //this.HttpContext.Session["rptSource"] = _acountServices.GetTransactionHistoryExports(transactionHistories);
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
                var transactionHistories = await _acountServices.GetCustomerTransactionsByAccountNumber(KEY);
                //this.HttpContext.Session["rptSource"] = _acountServices.GetTransactionHistoryExports(transactionHistories);
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
                var transactionHistories = await _acountServices.GetCustomerTransactionsByAccountNumber(KEY);
                var customer = await _individualProfileServices.GetSingleCustomer(transactionHistories.FirstOrDefault().Account.CustomerId);
                if (customer == null)
                {
                    return Json(new { success = true, status = false, message = "Failed getting customer" }, JsonRequestBehavior.AllowGet);

                }
                var branch = await _branchServices.GetBranch(customer.CustomerId);
                var rpt = _acountServices.MaprptSource(transactionHistories, branch, customer);

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

        public async Task<ActionResult> GetLoan(string Key)
        {
            var loan = await _loanServices.GetLoan(Key);
            //return Json(loan, JsonRequestBehavior.AllowGet);
            return Json(loan, JsonRequestBehavior.AllowGet);

        }
        public async Task LoadDropdowns()
        {
            //var Users = await _dailyTellerServices.LoadDailyUsers();
            var Tellers = await _tellerServices.GetTellersStringValuesAsync();
            var Branches = await _branchServices.GetBranches();
            ViewBag.Branches = Branches;

            //ViewBag.Users = Users;
            ViewBag.Tellers = Tellers;
        }
    }
}