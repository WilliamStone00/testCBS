using CBS.BusinessService.Accounting;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.MemberNoneCashOperationsP;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.UI.Helper;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Services.Description;
using System.Web.UI.WebControls;

namespace CBS.FrontDesk.UI.Controllers.CashDeskOperations
{
    //[CheckSessionTimeOutAttribute]

    public class LoanRepaymentBackOfficeController : BaseController
    {
        private readonly CashDeskServices _cashDeskService;
        private readonly MemberNoneCashOperationServices _memberNoneCashOperationServices;
        
        private readonly ChartOfAccountServicesAnnex chartOfAccountServices;

        public LoanRepaymentBackOfficeController(CashDeskServices cashDeskService = null, ChartOfAccountServicesAnnex chartOfAccountServices = null, MemberNoneCashOperationServices memberNoneCashOperationServices = null)
        {
            _cashDeskService = cashDeskService;
            this.chartOfAccountServices=chartOfAccountServices;
            _memberNoneCashOperationServices=memberNoneCashOperationServices;
        }
        public ActionResult Index()
        {
            return View();
        }
        public async Task<ActionResult> LedgerLoanRepayment()
        {
            await GetChartOfAccounts();
            return View();
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
        public async Task<ActionResult> DownloadFile(string fileId,string path)
        {


            try
            {
                // Fetch salary analysis details and file upload data
                var memberNoneCashOperations = await _memberNoneCashOperationServices.GetMemberNoneCashOperations(fileId, path);
                // Define file path and branch name
                string fileName = $"NoneCashOperationForMembers_{path}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                string directoryPath = Server.MapPath("~/TempFiles");

                // Ensure the directory exists
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                string filePath = Path.Combine(directoryPath, fileName);
                string exportedDate = DateTime.Now.ToString();
                string exportedBy = Session["FullName"].ToString();
                // Generate Excel file
                MemberNoneCashOperationExcelGenerator.GenerateOperationExcel(memberNoneCashOperations.ToList(), filePath, exportedDate, exportedBy, path);
                // Return the file for download
                byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                System.IO.File.Delete(filePath); // Clean up temporary file

                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                // Log the exception and return an error response
                Console.WriteLine($"Error generating Excel file: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while generating the Excel file." }, JsonRequestBehavior.AllowGet);
            }
        }

       
        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = "_DataNotFound", string path = null, string serviceOption = null)
        {
            try
            {
                ViewBag.KEY = KEY;
                if (KEY == null || KEY == "")
                {
                    ViewBag.message = "Empty data was submited. Please enter search criterial";
                    return PartialView("_DataNotFound", new CashDesk());
                }
                if (path=="loan_repayment_gl")
                {
                    await GetChartOfAccounts();
                }
                path="repayment";
                var cashDesk = await _cashDeskService.GetAccountByAccountNumberSearch(KEY, path);
                if (cashDesk == null)
                {

                    ViewBag.message = $"{KEY} was not found in the database.";
                    return PartialView("_DataNotFound", new CashDesk());
                }
                cashDesk.AddMembersNoneCashOperationCommand.MemberName=cashDesk.Customer.name;
                ViewBag.Operation = path;
                return PartialView(partialView, cashDesk);

            }
            catch (Exception ex)
            {

                ViewBag.message = ex.Message; // Store error message
                return PartialView("_NoRecordFound", new CashDesk());
            }
        }
        

         [HttpPost]
        public async Task<ActionResult> SubmitValidation(ValidateMemberNoneCashOperationCommand command)
        {
            try
            {
                var data = await _memberNoneCashOperationServices.ValidateMemberNoneCashOperation(command);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            catch (Exception ex)
            {
                return Json(new { success = false, status = false, message = $"An error occurred: {ex.Message}" });
            }
        }
        public async Task<ActionResult> DeleteOperation(string KEY)
        {
            var data = await _memberNoneCashOperationServices.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public async Task<ActionResult> PostRequestCash(List<AccountToBeDebited> accountToBeDebiteds, List<LoanToBeRefunded> loanToBeRefundeds)
        {
            try
            {
                if (accountToBeDebiteds.Any() && loanToBeRefundeds.Any())
                {
                    var deposits = new List<BulkDeposit> { new BulkDeposit {Amount=loanToBeRefundeds.FirstOrDefault().Capital,  AccountToBeDebiteds=accountToBeDebiteds, LoanToBeRefundeds=loanToBeRefundeds, OperationType="LoanRepaymentByLocalAccountNoneCash", AccountNumber=loanToBeRefundeds.FirstOrDefault().LoanId, CustomerId=loanToBeRefundeds.FirstOrDefault().MemberRefence, Principal=loanToBeRefundeds.FirstOrDefault().Capital, Note=loanToBeRefundeds.FirstOrDefault().Note,LoanId=loanToBeRefundeds.FirstOrDefault().LoanId, Total=accountToBeDebiteds.Sum(x=>x.Amount), Interest=loanToBeRefundeds.FirstOrDefault().Interest,  Penalty=loanToBeRefundeds.FirstOrDefault().Penalty, Tax=Math.Abs(loanToBeRefundeds.FirstOrDefault().Vat), VAT=Math.Abs(loanToBeRefundeds.FirstOrDefault().Vat) } };
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
        public async Task<bool> GetChartOfAccounts()
        {
            var chartOfAccounts = await chartOfAccountServices.GetChartOfAccounts();
            ViewBag.chartOfAccounts = chartOfAccounts.ToList();
            return true;
        }
    }
}