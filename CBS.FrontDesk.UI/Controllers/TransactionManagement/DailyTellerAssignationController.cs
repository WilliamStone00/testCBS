using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Message;
using ClosedXML.Excel;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.TransactionManagement
{

    [CheckSessionTimeOutAttribute]
    public class DailyTellerAssignationController : BaseController
    {
        // GET: DailyTellerAssignation
        private readonly DailyTellerServices _services;
        private readonly TellerServices _tellerServices;
        private readonly BranchServices _branchServices;
        public DailyTellerAssignationController(DailyTellerServices services, TellerServices tellerServices = null, BranchServices branchServices = null)
        {
            _services = services;
            _tellerServices = tellerServices;
            _branchServices = branchServices;
        }

        public async Task<ActionResult> Index()
        {
            await LoadDropdowns();
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(DailyTeller model)
        {
            if (model.Id == null)
            {
                var data = await _services.Create(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else
            {
                return await Update(model);
            }
        }

        //public async Task<ActionResult> GetReport(DailyTeller request)
        //{

        //}
        public async Task<ActionResult> TellerOpenningAndClossingStatusDownload(DailyTeller request)
        {
            // Call your service to get the data
            var response = await _services.TellerOpenningAndClossingOfDay(request.GetTellerOpenningAndClossingQuery);
            var Data = response.ToList(); // Convert to list if needed
            if (request.GetTellerOpenningAndClossingQuery.ByBracnch)
            {
                if (Data.Any())
                {
                    this.HttpContext.Session["RPTBranchName"] = Data.FirstOrDefault().BranchName;

                }
            }
            else
            {
                this.HttpContext.Session["RPTBranchName"] = "All Branches";
            }
            // Get the referrer URL
            string currentUrl = "/DailyTellerAssignation/TellerOpenningAndClossingStatus";
            if (Data == null || !Data.Any())
            {
                // Handle empty data scenario
                ViewBag.UrlToOpen = string.Empty;
                ViewBag.CurrentUrl = currentUrl;
                ViewBag.ErrorMessage = "No data available for the selected criteria.";
            }
            else
            {

                this.HttpContext.Session["rptSource"] = Data;
                this.HttpContext.Session["param_size"] = "3";
                this.HttpContext.Session["DateFrom"] = request.GetTellerOpenningAndClossingQuery.DateFrom;
                this.HttpContext.Session["DateTo"] = request.GetTellerOpenningAndClossingQuery.DateTo;
                this.HttpContext.Session["rptType"] = "ReportWithParameter";
                this.HttpContext.Session["ReportName"] = "OpeningAndClossingOfTellersRPT.rpt";
                this.HttpContext.Session["rptpath"] = "~/AppFiles/Reporting/Transactions/Tellers/OpeningAndClossingOfTellersRPT.rpt";
                this.HttpContext.Session["rpttitle"] = "AccountStatementTeller";

                // Construct the URL to redirect to the PDF
                string url = Url.Action("ReportWithParameter", "Reports"); // Adjust the controller name if different

                // Set the ViewBag variables
                ViewBag.UrlToOpen = url;
                ViewBag.CurrentUrl = currentUrl;
                ViewBag.ErrorMessage = string.Empty;
            }

            return View("OpenInNewWindow");
        }
        public async Task<ActionResult> DownloadTellerOperationsToExcel(DailyTeller request)
        {
            // Call your service to get the data
            var response = await _services.GetDailyOperation(request.GetAllTellerOperationsQuery);
            var Data = response.ToList(); // Convert to list if needed

            if (request.GetAllTellerOperationsQuery.IsPDF)
            {
                // Store data in session variables
                this.HttpContext.Session["rptSource"] = Data;
                this.HttpContext.Session["param_size"] = "4";
                this.HttpContext.Session["DateFrom"] = request.GetAllTellerOperationsQuery.DateFrom;
                this.HttpContext.Session["DateTo"] = request.GetAllTellerOperationsQuery.DateTo;
                this.HttpContext.Session["DatePrinted"] = DateTime.Now.ToLongDateString();
                this.HttpContext.Session["rptType"] = "ReportWithParameter";
                this.HttpContext.Session["ReportName"] = "TellerStatement.rpt";
                this.HttpContext.Session["rptpath"] = "~/AppFiles/Reporting/Transactions/Reciepts/TellerStatement.rpt";
                this.HttpContext.Session["rpttitle"] = "AccountStatementTeller";
                // Get the referrer URL
                string currentUrl = "/DailyTellerAssignation/DownloadTellerOperations";
                if (Data == null || !Data.Any())
                {
                    // Handle empty data scenario
                    ViewBag.UrlToOpen = string.Empty;
                    ViewBag.CurrentUrl = currentUrl;
                    ViewBag.ErrorMessage = "No data available for the selected criteria.";
                }
                else
                {
                    // Construct the URL to redirect to the PDF
                    string url = Url.Action("ReportWithParameter", "Reports"); // Adjust the controller name if different

                    // Set the ViewBag variables
                    ViewBag.UrlToOpen = url;
                    ViewBag.CurrentUrl = currentUrl;
                    ViewBag.ErrorMessage = string.Empty;
                }

                return View("OpenInNewWindow");

            }
            else
            {
                // Check if response is not empty
                if (response == null || !response.Any())
                {
                    // Handle empty response scenario (e.g., return a view with an error message)
                    return View("Error");
                }

                // Calculate summary values
                decimal openingBalance = response.FirstOrDefault().BalanceBF;
                decimal totalCredit = Data.Sum(item => item.Credit);
                decimal totalDebit = Data.Sum(item => item.Debit);
                decimal closingBalance = openingBalance + totalCredit - totalDebit;
                int totalTransactions = Data.Count;

                // Create a new workbook
                var wb = new XLWorkbook();

                // Add a worksheet
                var ws = wb.Worksheets.Add("DailyOperations");

                // Insert empty row between summary table and headers
                ws.Row(3).InsertRowsAbove(1);

                ws.Range("A1:F1").Merge().Value = $"Statement of account (TELLER). Printed: {DateTime.Now:dd-MM-yyyy hh:mm:ss}"; // Title above headers
                ws.Range("A1:F1").Style.Font.Bold = true;
                ws.Range("A1:F1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Range("A1:F1").Style.Fill.BackgroundColor = XLColor.LightGray; // Light brown background for title

                ws.Row(2).InsertRowsBelow(1);

                // Insert summary table
                ws.Cell(2, 1).Value = "Branch";
                ws.Cell(2, 2).Value = "Total Transactions";
                ws.Cell(2, 3).Value = "Opening Balance";
                ws.Cell(2, 4).Value = "Total Debit";
                ws.Cell(2, 5).Value = "Total Credit";
                ws.Cell(2, 6).Value = "Closing Balance";

                ws.Cell(3, 1).Value = response.FirstOrDefault().BranchName; // Replace with actual branch value if available
                ws.Cell(3, 2).Value = totalTransactions;
                ws.Cell(3, 3).Value = openingBalance;
                ws.Cell(3, 4).Value = totalCredit;
                ws.Cell(3, 5).Value = totalDebit;
                ws.Cell(3, 6).Value = closingBalance;

                // Format financial values (assuming they are numeric)
                ws.Cell(3, 3).Style.NumberFormat.Format = "#,##0";
                ws.Cell(3, 4).Style.NumberFormat.Format = "#,##0";
                ws.Cell(3, 5).Style.NumberFormat.Format = "#,##0";
                ws.Cell(3, 6).Style.NumberFormat.Format = "#,##0";

                // Bold summary table headers
                ws.Range("A2:F2").Style.Font.Bold = true;

                // Add borders to summary table (all borders)
                var summaryTableRange = ws.Range("A2:F3");
                summaryTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                summaryTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                // Populate data and format financial values
                int row = 6;

                // Insert a new row after the summary table
                ws.Row(4).InsertRowsBelow(1);
                ws.Range("A4:F4").Merge().Value = "Entries"; // Title above headers
                ws.Range("A4:F4").Style.Font.Bold = true;
                ws.Range("A4:F4").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Define column headers and style
                ws.Cell(5, 1).Value = "SN";
                ws.Cell(5, 2).Value = "Date";
                ws.Cell(5, 3).Value = "Narration";
                ws.Cell(5, 4).Value = "Debit";
                ws.Cell(5, 5).Value = "Credit";
                ws.Cell(5, 6).Value = "Balance";

                // Style headers (bold and borders)
                var headerRange = ws.Range("A5:F5");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                int serialNumber = 1; // Start the serial number from 1
                foreach (var item in Data)
                {
                    ws.Cell(row, 1).Value = serialNumber; // Assign the serial number
                    ws.Cell(row, 2).Value = item.Date;
                    ws.Cell(row, 3).Value = item.Naration;
                    ws.Cell(row, 4).Value = item.Credit;
                    ws.Cell(row, 5).Value = item.Debit;
                    ws.Cell(row, 6).Value = item.Balance;

                    // Format financial values (assuming they are numeric)
                    for (int col = 4; col <= 6; col++)
                    {
                        ws.Cell(row, col).Style.NumberFormat.Format = "#,##0";
                    }

                    // Add borders to data cells (all borders)
                    var dataRowRange = ws.Range($"A{row}:F{row}");
                    dataRowRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    dataRowRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    row++;
                    serialNumber++; // Increment the serial number for the next row
                }
                // Auto-fit columns for better readability
                ws.Columns().AdjustToContents();

                // Prepare memory stream to hold the Excel file

                // Prepare memory stream to hold the Excel file
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    // Save the workbook to the memory stream
                    wb.SaveAs(memoryStream);

                    // Generate dynamic Excel file name based on branch name and current date/time
                    string excelName = $"Operation_{response.FirstOrDefault().BranchName}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                    excelName = excelName.Replace(" ", "_");
                    // Set response headers
                    Response.Clear();
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("content-disposition", $"attachment; filename=\"{excelName}\"");
                    Response.BinaryWrite(memoryStream.ToArray());
                    Response.End();
                }

                return View();
            }

        }
        public async Task<ActionResult> DownloadTellerOperations()
        {
            await LoadDropdowns();
            return View();
        }
        public async Task<ActionResult> TellerOpenningAndClossingStatus()
        {
            var Branches = await _branchServices.GetBranches();
            ViewBag.Branches = Branches;
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Update(DailyTeller model)
        {
            var data = await _services.Update(model);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string dateFrom = null, string dateTo = null)
        {
            if (path == "search")
            {
                if (KEY != string.Empty)
                {
                    var data = await _services.GetDailyTellers(_services.GetDateTime(dateFrom), _services.GetDateTime(dateTo), KEY);
                    return PartialView(partialView, data.ToList());

                }
                else
                {
                    var data = await _services.GetDailyTellers(_services.GetDateTime(dateFrom), _services.GetDateTime(dateTo));
                    return PartialView(partialView, data.ToList());

                }
            }

            else if (path == "new")
            {
                await LoadDropdowns();
                return PartialView(partialView, new DailyTeller());
            }
            else
            {
                ViewBag.Key = KEY;
                await LoadDropdowns();
                var dailyTeller = await _services.GetDailyTeller(KEY);
                dailyTeller.UserId = $"{dailyTeller.UserId}@{dailyTeller.UserName}";
                return PartialView(partialView, dailyTeller);

            }


        }
        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _services.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }

        public async Task LoadDropdowns()
        {
            var Users = await _services.LoadDailyUsers();
            var Tellers = await _tellerServices.GetTellersStringValuesAsync();
            var Branches = await _branchServices.GetBranches();
            ViewBag.Branches = Branches;

            ViewBag.Users = Users;
            ViewBag.Tellers = Tellers;
        }
        //public async Task<ActionResult> Loa(string Key)
        //{
        //    var Users = await _services.LoadDailyUsers();
        //    var Tellers = await _tellerServices.GetTellersStringValuesAsync();
        //    var Branches = await _branchServices.GetBranches();
        //    ViewBag.Users = Users;
        //    ViewBag.Tellers = Tellers;
        //    ViewBag.Branches = Branches;

        //    return Json(data, JsonRequestBehavior.AllowGet);
        //}
    }

}