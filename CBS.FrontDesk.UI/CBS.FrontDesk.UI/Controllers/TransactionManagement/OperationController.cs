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
            var branch = new Branch();
            if (string.IsNullOrEmpty(request.BranchID))
            {
                if (_branchServices.IsHeadOffice())
                {
                    request.BranchID="n/a";
                }
            }
            else
            {
                branch = await _branchServices.GetBranch(request.BranchID.ToString());

                if (branch == null)
                {
                    return Json(new { success = false, message = "Branch not found" });
                }
            }

            var response = await _acountServices.GetTransactionsAsync(request);
            var data = await _acountServices.MapTransactionHistoryToExport(response);

            if (data == null || !data.Any())
            {
                return Json(new { success = false, message = "No data available for the selected query criteria." });
            }

            // Create Excel file in the same action
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add($"Transactions_{branch.BranchCode}");
                string branchName = "ALL BRANCHES";
                string branchCode = "-";
                string branchTel = "-";
                string branchLocation = "-";
                string bankName = "-";

                if (request.BranchID != "n/a")
                {
                    var selectedBranch = data.FirstOrDefault();
                    if (selectedBranch != null)
                    {
                        branchName = selectedBranch.BankName.ToUpper();
                        branchCode = selectedBranch.BranchCode;
                        branchTel = selectedBranch.BranchTel ?? "N/A";
                        branchLocation = selectedBranch.BranchCode?.ToUpper() ?? "N/A";
                        bankName = selectedBranch.BrnachName ?? "N/A";
                    }
                }

                // Title with File Title
                worksheet.Cell(1, 1).Value = $"TRANSACTIONS {branchName}";
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                worksheet.Cell(1, 1).Style.Font.FontSize = 14;
                worksheet.Range(1, 1, 1, 19).Merge();
                worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(1, 1).Style.Font.FontName = "Bahnschrift Light";
                worksheet.Cell(1, 1).Style.Font.FontSize = 14;
                // Export Details with Date Range
                worksheet.Cell(2, 1).Value = $"Date Range: {request.DateFrom.ToShortDateString()} - {request.DateTo.ToShortDateString()}";
                worksheet.Cell(2, 1).Style.Font.Italic = true;
                worksheet.Cell(2, 1).Style.Font.FontSize = 10;
                worksheet.Cell(2, 1).Style.Font.FontName = "Bahnschrift Light";
                worksheet.Range(2, 1, 2, 19).Merge();
                worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                worksheet.Cell(3, 1).Value = $"Export Date: {DateTime.Now} BY {Session["FullName"].ToString()}";
                worksheet.Cell(3, 1).Style.Font.Italic = true;
                worksheet.Cell(3, 1).Style.Font.FontSize = 10;
                worksheet.Cell(3, 1).Style.Font.FontName = "Bahnschrift Light";
                worksheet.Range(3, 3, 3, 19).Merge();
                worksheet.Cell(3, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                // Summary Title
                int summaryRow = 5;
                worksheet.Cell(summaryRow, 1).Value = "SUMMARY";
                worksheet.Cell(summaryRow, 1).Style.Font.Bold = true;
                worksheet.Cell(summaryRow, 1).Style.Fill.BackgroundColor = XLColor.Chocolate;
                worksheet.Cell(summaryRow, 1).Style.Font.FontColor = XLColor.White;
                worksheet.Cell(1, 1).Style.Font.FontName = "Bahnschrift Light";
                worksheet.Range(summaryRow, 1, summaryRow, 2).Merge();
                worksheet.Cell(summaryRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;


                summaryRow++;
                // Adding summary values
                worksheet.Cell(summaryRow, 1).Value = "Total Transactions:";
                worksheet.Cell(summaryRow, 2).Value = data.Count();
                worksheet.Cell(summaryRow, 2).Style.NumberFormat.Format = "#,##0";

                summaryRow++;
                worksheet.Cell(summaryRow, 1).Value = "Total Volume:";
                worksheet.Cell(summaryRow, 2).Value = data.Sum(t => t.Amount);
                worksheet.Cell(summaryRow, 2).Style.NumberFormat.Format = "#,##0.0";

                summaryRow++;
                worksheet.Cell(summaryRow, 1).Value = "Total Branches:";
                worksheet.Cell(summaryRow, 2).Value = data.Select(t => t.BranchCode).Distinct().Count();

                summaryRow++;
                worksheet.Cell(summaryRow, 1).Value = "Number of Operations Groups:";
                worksheet.Cell(summaryRow, 2).Value = data.Select(t => t.Operation).Distinct().Count();

                summaryRow++;
                worksheet.Cell(summaryRow, 1).Value = "Sum of Operations Group:";
                worksheet.Cell(summaryRow, 2).Value = data.Sum(t => t.OperationCharge);
                worksheet.Cell(summaryRow, 2).Style.NumberFormat.Format = "#,##0.0";
                worksheet.Cell(summaryRow, 2).Style.Font.FontName = "Bahnschrift Light";
                // Apply border to summary section
                worksheet.Range(5, 1, summaryRow, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                worksheet.Range(5, 1, summaryRow, 2).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                worksheet.Range(5, 1, summaryRow, 2).Style.Border.OutsideBorderColor = XLColor.Black;

                summaryRow += 2; // Skip a row after the summary

                // Headers
                int headerRow = summaryRow;
                worksheet.Cell(headerRow, 1).Value = "ACC.Name";
                worksheet.Cell(headerRow, 2).Value = "ACC.Number";
                worksheet.Cell(headerRow, 3).Value = "ACC.Type";
                worksheet.Cell(headerRow, 4).Value = "M.REF";
                worksheet.Cell(headerRow, 5).Value = "Date";
                worksheet.Cell(headerRow, 6).Value = "ACC.Date";
                worksheet.Cell(headerRow, 7).Value = "Amount";
                worksheet.Cell(headerRow, 8).Value = "Fee";
                worksheet.Cell(headerRow, 9).Value = "B.Forward";
                worksheet.Cell(headerRow, 10).Value = "Balance";
                worksheet.Cell(headerRow, 11).Value = "Reference";
                worksheet.Cell(headerRow, 12).Value = "Cashier";
                worksheet.Cell(headerRow, 13).Value = "Representatives";
                worksheet.Cell(headerRow, 14).Value = "Operation";
                worksheet.Cell(headerRow, 15).Value = "F.Charge";
                worksheet.Cell(headerRow, 16).Value = "S.Charge";
                worksheet.Cell(headerRow, 17).Value = "Debit";
                worksheet.Cell(headerRow, 18).Value = "Credit";
                worksheet.Cell(headerRow, 19).Value = "I.B";
                worksheet.Cell(headerRow, 19).Style.Font.FontName = "Bahnschrift Light";
                worksheet.Range(headerRow, 1, headerRow, 19).Style.Font.Bold = true;
                worksheet.Range(headerRow, 1, headerRow, 19).Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
                worksheet.Range(headerRow, 1, headerRow, 19).Style.Border.OutsideBorderColor = XLColor.Black;


                // Apply Borders and Formatting
                var headerRange = worksheet.Range(headerRow, 1, headerRow, 19);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Font.FontSize = 12;
                headerRange.Style.Font.FontName = "Bahnschrift";
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray; // Optional: Light Gray Background

                // Apply Borders to Each Cell in Header
                headerRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;


                // Apply uppercase transformation programmatically (Alternative)
                for (int col = 1; col <= 19; col++)
                {
                    var cell = worksheet.Cell(headerRow, col);
                    cell.Value = cell.Value.ToString().ToUpper();
                }

                int dataStartRow = headerRow + 1;
                int currentRow = dataStartRow;

                foreach (var branchGroup in data.GroupBy(t => t.BrnachName))
                {
                    worksheet.Cell(currentRow, 1).Value = $"BRANCH: {branchGroup.Key.ToUpper()}";
                    worksheet.Cell(currentRow, 1).Style.Font.FontName = "Bahnschrift Light";
                    worksheet.Range(currentRow, 1, currentRow, 19).Merge();
                    worksheet.Range(currentRow, 1, currentRow, 19).Style.Font.Bold = true;
                    worksheet.Range(currentRow, 1, currentRow, 19).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Range(currentRow, 1, currentRow, 19).Style.Fill.BackgroundColor = XLColor.LightGray;
                    currentRow++;

                    foreach (var transaction in branchGroup)
                    {
                        worksheet.Cell(currentRow, 1).Value = transaction.MemberName;
                        worksheet.Cell(currentRow, 2).Value = transaction.AccountNumber;
                        worksheet.Cell(currentRow, 3).Value = transaction.AccountType;
                        worksheet.Cell(currentRow, 4).Value = transaction.CustomerReference;
                        worksheet.Cell(currentRow, 5).Value = transaction.Date;
                        worksheet.Cell(currentRow, 6).Value = transaction.AccountingDate;
                        worksheet.Cell(currentRow, 7).Value = transaction.Amount;
                        worksheet.Cell(currentRow, 8).Value = transaction.Fee;
                        worksheet.Cell(currentRow, 9).Value = transaction.NewBalance;
                        worksheet.Cell(currentRow, 10).Value = transaction.Balance;
                        worksheet.Cell(currentRow, 11).Value = transaction.Reference;
                        worksheet.Cell(currentRow, 12).Value = transaction.TellerBranchName;
                        //worksheet.Cell(currentRow, 13).Value = transaction.re;
                        worksheet.Cell(currentRow, 14).Value = transaction.Operation;
                        worksheet.Cell(currentRow, 15).Value = transaction.WithdrawalFormCharge;
                        worksheet.Cell(currentRow, 16).Value = transaction.OperationCharge;
                        worksheet.Cell(currentRow, 17).Value = transaction.Debit;
                        worksheet.Cell(currentRow, 18).Value = transaction.Credit;
                        worksheet.Cell(currentRow, 19).Value = transaction.InterBranch;
                        // Apply styles for current row
                        for (int col = 1; col <= 19; col++)
                        {
                            worksheet.Cell(currentRow, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            worksheet.Cell(currentRow, col).Style.Font.FontName = "Bahnschrift Light";
                            worksheet.Cell(currentRow, col).Style.NumberFormat.Format = "#,##0.0";  // Currency formatting
                            worksheet.Cell(currentRow, col).Style.Border.OutsideBorderColor = XLColor.Black;
                        }
                        currentRow++;
                    }

                    worksheet.Cell(currentRow, 1).Value = $"TOTAL FOR {branchGroup.Key.ToUpper()}";
                    worksheet.Cell(currentRow, 7).Value = branchGroup.Sum(t => t.Amount);
                    worksheet.Cell(currentRow, 7).Style.NumberFormat.Format = "#,##0.0";
                    worksheet.Cell(currentRow, 7).Style.Font.FontName = "Bahnschrift Light";
                    worksheet.Range(currentRow, 1, currentRow, 6).Merge();
                    worksheet.Range(currentRow, 1, currentRow, 19).Style.Font.Bold = true;
                    worksheet.Range(currentRow, 1, currentRow, 19).Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
                    worksheet.Range(currentRow, 1, currentRow, 19).Style.Fill.BackgroundColor = XLColor.LightGray;
                    currentRow++;
                    currentRow += 1;  // Leave a blank row before the next group
                    // Apply borders for totals
                    for (int col = 1; col <= 19; col++)
                    {
                        worksheet.Cell(currentRow, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(currentRow, col).Style.Border.OutsideBorderColor = XLColor.Black;
                        worksheet.Cell(currentRow, col).Style.Font.FontName = "Bahnschrift Light";
                    }

                   
                }

                worksheet.Columns().AdjustToContents();

                // Save the file
                var fileName = $"Transactions_{branchCode}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

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