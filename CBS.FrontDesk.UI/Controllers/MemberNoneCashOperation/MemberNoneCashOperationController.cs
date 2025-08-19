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

    public class MemberNoneCashOperationController : BaseController
    {
        private readonly CashDeskServices _cashDeskService;
        private readonly AccountingServices _accountingServices;
        private readonly MemberNoneCashOperationServices _memberNoneCashOperationServices;
        
        private readonly ChartOfAccountServicesAnnex chartOfAccountServices;

        public MemberNoneCashOperationController(CashDeskServices cashDeskService = null, AccountingServices accountingServices = null, ChartOfAccountServicesAnnex chartOfAccountServices = null, MemberNoneCashOperationServices memberNoneCashOperationServices = null)
        {
            _cashDeskService = cashDeskService;
            _accountingServices = accountingServices;
            this.chartOfAccountServices=chartOfAccountServices;
            _memberNoneCashOperationServices=memberNoneCashOperationServices;
        }
        // GET: MemberNoneCashOperation
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult PendingOperations()
        {
            return View();
        }
        public ActionResult Operations()
        {
            ViewBag.Path="all_operations";
            return View();
        }
        public ActionResult MyOperations()
        {
            ViewBag.Path="my_requests";
            return View();
        }
        public ActionResult NoneCashOperations()
        {
            return View();
        }
        public ActionResult LoanRepayment()
        {
            return View();
        }
        public async Task<bool> GetChartOfAccounts()
        {
            var chartOfAccounts = await chartOfAccountServices.GetChartOfAccounts(true);
            ViewBag.chartOfAccounts = chartOfAccounts.ToList();
            return true;
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
                    await GetChartOfAccounts();
                    return PartialView(partialView, cashDesk);


                }
                else if (path == "cashin" || path == "cashout" || path == "cashoutsws" || path == "repayment" || path == "withdrawalnotification" || path== "loanapplicationfeepayment")
                {
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
                    cashDesk.AddMembersNoneCashOperationCommand.MemberName=cashDesk.Customer.name;
                    await GetChartOfAccounts();
                    ViewBag.Operation = path;
                    return PartialView(partialView, cashDesk);

                }
                else if (path == "pending_operations")
                {
                    ViewBag.Path="pending_operations";
                    var memberNoneCashOperations = await _memberNoneCashOperationServices.GetMemberNoneCashOperations("n/a","Pending");
               

                    return PartialView(partialView, new MemberNoneCashOperationCarrier { MemberNoneCashOperations=memberNoneCashOperations.ToList() });

                }
                else if (path == "all_operations")
                {
                    ViewBag.Path="all_operations";
                    var memberNoneCashOperations = await _memberNoneCashOperationServices.GetMemberNoneCashOperations("n/a", "all");

                    return PartialView(partialView, new MemberNoneCashOperationCarrier { MemberNoneCashOperations=memberNoneCashOperations.ToList() });

                }
                else if (path == "my_requests")
                {
                    ViewBag.Path="my_requests";
                    var memberNoneCashOperations = await _memberNoneCashOperationServices.GetMemberNoneCashOperations("n/a", "my_requests");

                    return PartialView(partialView, new MemberNoneCashOperationCarrier { MemberNoneCashOperations=memberNoneCashOperations.ToList() });

                }

                else if (path == "detail" || path=="get_validation")
                {
                    var memberNoneCashOperation = await _memberNoneCashOperationServices.GetMemberNoneCashOperation(KEY);

                    return PartialView(partialView, new MemberNoneCashOperationCarrier { MemberNoneCashOperation=memberNoneCashOperation });

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
        public async Task<ActionResult> PostRequestCash(List<BulkDeposit> deposits)
        {
            try
            {
                if (deposits != null)
                {

                    var data = await _memberNoneCashOperationServices.Create(deposits);
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

    }
}