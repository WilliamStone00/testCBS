using CBS.BusinessService;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.ReportDataSetDto;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Mapping;
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
            // GetAllowAnonymous the referrer URL
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
                string url = Url.Action("ReportWithParameter", "Reports"); // Adjust the controller Name if different

                // Set the ViewBag variables
                ViewBag.UrlToOpen = url;
                ViewBag.CurrentUrl = currentUrl;
                ViewBag.ErrorMessage = string.Empty;
            }

            return View("OpenInNewWindow");
        }
        public async Task<ActionResult> TellerOpenningAndClossingStatus()
        {
            var Branches = await _branchServices.GetBranches();
            ViewBag.Branches = Branches;
            return View();
        }
        public async Task<ActionResult> TillCashStatusDownload(GetTillStatusQuery request)
        {
            // Clear ModelState errors for properties you don't want to validate
            ModelState.Clear();

            // Manually add the validation errors for `GetTillStatusQuery`
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

            if (request.ByBranch)
            {
                request.QueryParameter = request.BranchId;
            }
            else
            {
                request.QueryParameter = request.TellerId;
            }

            // Call your service to get the data
            var response = await _services.TillCashStatus(request);
            var data = response.ToList(); // Convert to list if needed

            // Set session variables based on the request
            if (request.ByBranch)
            {
                if (data.Any())
                {
                    this.HttpContext.Session["RPTBranchName"] = data.FirstOrDefault()?.BranchName;
                }
            }
            else
            {
                this.HttpContext.Session["RPTBranchName"] = "For All Branches";
            }

            // Check if data is available
            if (data == null || !data.Any())
            {
                return Json(new { success = false, message = "No data available for the selected query criteria." });

            }
            // If data is available, proceed with setting session variables and redirecting to the report
            this.HttpContext.Session["rptSource"] = data;
            this.HttpContext.Session["param_size"] = "5";
            this.HttpContext.Session["DateFrom"] = request.DateFrom;
            this.HttpContext.Session["DateTo"] = request.DateTo;
            this.HttpContext.Session["PrintedBy"] = Session["FullName"].ToString();
            this.HttpContext.Session["rptType"] = "ReportWithParameter";
            this.HttpContext.Session["ReportName"] = "AllOpenAndClossingOfTillRpt.rpt";
            this.HttpContext.Session["rptpath"] = $"~/AppFiles/Reporting/Transactions/Tellers/AllOpenAndClossingOfTillRpt.rpt";
            this.HttpContext.Session["rpttitle"] = $"TillStatus";

            // Construct the URL to redirect to the PDF
            string url = Url.Action("ReportWithParameter", "Reports"); // Adjust the controller Name if different

            // Set the ViewBag variables for the URL to open the report
            ViewBag.UrlToOpen = url;
            ViewBag.CurrentUrl = "/DailyTellerAssignation/TillCashStatus"; // The current URL
            ViewBag.ErrorMessage = string.Empty;

            // Return the view that opens the report in a new window
            return Json(new { success = true, message = "Success." });
        }


        public async Task<ActionResult> TillCashStatus()
        {
            var Branches = await _branchServices.GetBranches();
            var Tellers = await _tellerServices.GetTellersStringValuesAsync();
            ViewBag.HasError = false;
            ViewBag.Branches = Branches;
            ViewBag.Tellers = Tellers;
            return View();
        }
        public async Task<ActionResult> DownloadTellerOperationsToExcel(DailyTeller request)
        {
            // Clear model state errors if any
            ModelState.Clear();
            // Validate the GetAllTellerOperationsQuery object
            TryValidateModel(request.GetAllTellerOperationsQuery, nameof(request.GetAllTellerOperationsQuery));
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select((e, index) => $"{index + 1}. {e.ErrorMessage}")
                    .ToList();

                string error = string.Join("<br/>", errors);
                return Json(new { success = false, message = error });
            }

            if (request.IsPrimary)
            {
                request.GetAllTellerOperationsQuery.QueryString = request.GetAllTellerOperationsQuery.BranchId;
            }

            var response = await _services.GetDailyOperation(request.GetAllTellerOperationsQuery);
            var data = response.ToList();
            if (data == null || !data.Any())
            {
                return Json(new { success = false, message = "No data available for the selected criteria." });
            }
            if (request.GetAllTellerOperationsQuery.IsPDF)
            {
                string relativeReportPath = "/Transactions/Reciepts/TellerStatement.rpt";
                string reportTitle = "TELLER'S CASH OPERATION STATEMENT";
                // PDF generation logic (you can keep this unchanged)
                this.HttpContext.Session["MainData"] = data;
                // Convert start and end date strings to DateTime safely
                DateTime startDate = DateTime.TryParse(request.GetAllTellerOperationsQuery.DateFrom, out var sDate)
                    ? sDate
                    : DateTime.MinValue;

                DateTime endDate = DateTime.TryParse(request.GetAllTellerOperationsQuery.DateTo, out var eDate)
                    ? eDate
                    : DateTime.MinValue;

                // Format to dd/MM/yyyy
                var parameters = new Dictionary<string, object>
                {
                    { "DateFrom", startDate.ToString("dd/MM/yyyy") },
                    { "DateTo", endDate.ToString("dd/MM/yyyy") },
                    { "CurrentYear", DateTime.Now.Year.ToString() },
                    { "DateNow", DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss") },
                    { "PrintedBy", Session["FullName"]?.ToString() ?? "System" }
                };
                //DateNow
                Session["ReportParameters"] = parameters;
                // ✅ Return viewer URL to the AJAX call
                string reportPathParam = HttpUtility.UrlEncode(relativeReportPath);
                string reportNameParam = HttpUtility.UrlEncode(reportTitle);
                string viewerUrl = Url.Content($"/ReportForm/ReportViewer.aspx?reportPath={reportPathParam}&reportName={reportNameParam}");
                return Json(new { success = true, redirectUrl = viewerUrl });
            }
            else /*if (request.GetAllTellerOperationsQuery.Excel)*/
            {
               

                // Calculate summary values
                decimal openingBalance = data.FirstOrDefault()?.BalanceBF ?? 0;
                decimal totalCredit = data.Sum(item => item.Credit);
                decimal totalDebit = data.Sum(item => item.Debit);
                decimal closingBalance = openingBalance + totalCredit - totalDebit;
                int totalTransactions = data.Count;

                // Create a new workbook
                using (var wb = new XLWorkbook())
                {
                    // Add a worksheet and configure headers, formatting, etc.
                    var ws = wb.Worksheets.Add("DailyOperations");

                    // Title and Summary Table
                    ws.Range("A1:F1").Merge().Value = $"Statement of account (TELLER). Printed: {DateTime.Now:dd-MM-yyyy hh:mm:ss}";
                    ws.Range("A1:F1").Style.Font.Bold = true;
                    ws.Range("A1:F1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range("A1:F1").Style.Fill.BackgroundColor = XLColor.LightGray;

                    ws.Cell(2, 1).Value = "Branch";
                    ws.Cell(2, 2).Value = "Total Transactions";
                    ws.Cell(2, 3).Value = "Opening Balance";
                    ws.Cell(2, 4).Value = "Total Debit";
                    ws.Cell(2, 5).Value = "Total Credit";
                    ws.Cell(2, 6).Value = "Closing Balance";

                    ws.Cell(3, 1).Value = data.FirstOrDefault()?.BranchName ?? "N/A";
                    ws.Cell(3, 2).Value = totalTransactions;
                    ws.Cell(3, 3).Value = openingBalance;
                    ws.Cell(3, 4).Value = totalDebit;
                    ws.Cell(3, 5).Value = totalCredit;
                    ws.Cell(3, 6).Value = closingBalance;

                    ws.Range("A2:F2").Style.Font.Bold = true;

                    // Add borders to summary table
                    ws.Range("A2:F3").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    ws.Range("A2:F3").Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Define headers for data
                    ws.Cell(5, 1).Value = "SN";
                    ws.Cell(5, 2).Value = "Date";
                    ws.Cell(5, 3).Value = "Narration";
                    ws.Cell(5, 4).Value = "Debit";
                    ws.Cell(5, 5).Value = "Credit";
                    ws.Cell(5, 6).Value = "Balance";

                    ws.Range("A5:F5").Style.Font.Bold = true;
                    ws.Range("A5:F5").Style.Border.BottomBorder = XLBorderStyleValues.Thin;

                    // Populate data
                    int row = 6;
                    int serialNumber = 1;
                    foreach (var item in data)
                    {
                        ws.Cell(row, 1).Value = serialNumber;
                        ws.Cell(row, 2).Value = item.Date;
                        ws.Cell(row, 3).Value = item.Naration;
                        ws.Cell(row, 4).Value = item.Debit;
                        ws.Cell(row, 5).Value = item.Credit;
                        ws.Cell(row, 6).Value = item.Balance;

                        for (int col = 4; col <= 6; col++)
                        {
                            ws.Cell(row, col).Style.NumberFormat.Format = "#,##0";
                        }

                        row++;
                        serialNumber++;
                    }

                    ws.Columns().AdjustToContents();

                    // Export to Excel
                    using (var memoryStream = new MemoryStream())
                    {
                        wb.SaveAs(memoryStream);
                        string excelName = $"Operation_{data.FirstOrDefault()?.BranchName}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                        excelName = excelName.Replace(" ", "_");

                        // Save the workbook and return the file path
                        string filePath = Path.Combine(Server.MapPath("~/AppFiles/Exports"), excelName);
                        wb.SaveAs(filePath);

                        // Return the file URL
                        return Json(new { success = true, ispdf=false, message = $"/Reports/DownloadFromJquery?Filename={excelName}" });

                        

                    }

                    //return new EmptyResult();
                }
            }
            //else
            //{
            //    return Json(new { success = false, message = "Invalid request." });
            //}
        }
       

        //public async Task<ActionResult> DownloadTellerOperationsToExcel(DailyTeller request)
        //{
        //    // Call your service to get the data
        //    ModelState.Clear();

        //    // Manually add the validation errors for `GetTillStatusQuery`
        //    TryValidateModel(request.GetAllTellerOperationsQuery, nameof(request.GetAllTellerOperationsQuery));

        //    if (!ModelState.IsValid)
        //    {
        //        var errors = ModelState.Values
        //            .SelectMany(v => v.Errors)
        //            .Select((e, index) => $"{index + 1}. {e.ErrorMessage}")
        //            .ToList();

        //        string error = string.Join("<br/>", errors);
        //        return Json(new { success = false, message = error });
        //    }

        //    if (request.IsPrimary)
        //    {
        //        request.GetAllTellerOperationsQuery.QueryString = request.GetAllTellerOperationsQuery.BranchId;
        //    }

        //    var response = await _services.GetDailyOperation(request.GetAllTellerOperationsQuery);
        //    var Data = response.ToList(); // Convert to list if needed


        //    if (request.GetAllTellerOperationsQuery.IsPDF)
        //    {
        //        //// Store data in session variables
        //        //this.HttpContext.Session["rptSource"] = Data;
        //        //this.HttpContext.Session["param_size"] = "4";
        //        //this.HttpContext.Session["DateFrom"] = request.GetAllTellerOperationsQuery.DateFrom;
        //        //this.HttpContext.Session["DateTo"] = request.GetAllTellerOperationsQuery.DateTo;
        //        //this.HttpContext.Session["DatePrinted"] = DateTime.Now.ToLongDateString();
        //        //this.HttpContext.Session["rptType"] = "ReportWithParameter";
        //        //this.HttpContext.Session["ReportName"] = "TellerStatement.rpt";
        //        //this.HttpContext.Session["rptpath"] = "~/AppFiles/Reporting/Transactions/Reciepts/TellerStatement.rpt";
        //        //this.HttpContext.Session["rpttitle"] = "AccountStatementTeller";
        //        //// GetAllowAnonymous the referrer URL
        //        //string currentUrl = "/DailyTellerAssignation/DownloadTellerOperations";
        //        //if (Data == null || !Data.Any())
        //        //{
        //        //    // Handle empty data scenario
        //        //    ViewBag.UrlToOpen = string.Empty;
        //        //    ViewBag.CurrentUrl = currentUrl;
        //        //    ViewBag.ErrorMessage = "No data available for the selected criteria.";
        //        //}
        //        //else
        //        //{
        //        //    // Construct the URL to redirect to the PDF
        //        //    string url = Url.Action("ReportWithParameter", "Reports"); // Adjust the controller Name if different

        //        //    // Set the ViewBag variables
        //        //    ViewBag.UrlToOpen = url;
        //        //    ViewBag.CurrentUrl = currentUrl;
        //        //    ViewBag.ErrorMessage = string.Empty;
        //        //}

        //        //return View("OpenInNewWindow");


        //        // Clear ModelState errors for properties you don't want to validate

        //        // Call your service to get the data
        //        var data = response.ToList(); // Convert to list if needed

        //        // Set session variables based on the request
        //        if (request.GetAllTellerOperationsQuery.IsByBranch)
        //        {
        //            if (data.Any())
        //            {
        //                this.HttpContext.Session["RPTBranchName"] = data.FirstOrDefault()?.BranchName;
        //            }
        //        }
        //        else
        //        {
        //            this.HttpContext.Session["RPTBranchName"] = "For All Branches";
        //        }

        //        // Check if data is available
        //        if (data == null || !data.Any())
        //        {
        //            return Json(new { success = false, message = "No data available for the selected query criteria." });

        //        }

        //        this.HttpContext.Session["rptSource"] = Data;
        //        this.HttpContext.Session["param_size"] = "4";
        //        this.HttpContext.Session["DateFrom"] = request.GetAllTellerOperationsQuery.DateFrom;
        //        this.HttpContext.Session["DateTo"] = request.GetAllTellerOperationsQuery.DateTo;
        //        this.HttpContext.Session["DatePrinted"] = DateTime.Now.ToLongDateString();
        //        this.HttpContext.Session["rptType"] = "ReportWithParameter";
        //        this.HttpContext.Session["ReportName"] = "TellerStatement.rpt";
        //        this.HttpContext.Session["rptpath"] = "~/AppFiles/Reporting/Transactions/Reciepts/TellerStatement.rpt";
        //        this.HttpContext.Session["rpttitle"] = $"Till_F5_{Data.FirstOrDefault().TellerName}";
        //        // Construct the URL to redirect to the PDF
        //        string url = Url.Action("ReportWithParameter", "Reports"); // Adjust the controller Name if different

        //        // Set the ViewBag variables for the URL to open the report
        //        ViewBag.UrlToOpen = url;
        //        ViewBag.CurrentUrl = "/DailyTellerAssignation/DownloadTellerOperationsToExcel"; // The current URL
        //        ViewBag.ErrorMessage = string.Empty;
        //        // Return the view that opens the report in a new window
        //        return Json(new { success = true, message = "Success." });

        //    }
        //    else
        //    {
        //        // Call your service to get the data

        //        // Check if response is not empty
        //        if (response == null || !response.Any())
        //        {
        //            // Handle empty response scenario (e.g., return a view with an error message)
        //            return Json(new { success = false, message = "Failed." });
        //        }

        //        // Calculate summary values
        //        decimal openingBalance = response.FirstOrDefault().BalanceBF;
        //        decimal totalCredit = Data.Sum(item => item.Credit);
        //        decimal totalDebit = Data.Sum(item => item.Debit);
        //        decimal closingBalance = openingBalance + totalCredit - totalDebit;
        //        int totalTransactions = Data.Count;

        //        // Create a new workbook
        //        var wb = new XLWorkbook();

        //        // Add a worksheet
        //        var ws = wb.Worksheets.Add("DailyOperations");

        //        // Insert empty row between summary table and headers
        //        ws.Row(3).InsertRowsAbove(1);

        //        ws.Range("A1:F1").Merge().Value = $"Statement of account (TELLER). Printed: {DateTime.Now:dd-MM-yyyy hh:mm:ss}"; // Title above headers
        //        ws.Range("A1:F1").Style.Font.Bold = true;
        //        ws.Range("A1:F1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        //        ws.Range("A1:F1").Style.Fill.BackgroundColor = XLColor.LightGray; // Light brown background for title

        //        ws.Row(2).InsertRowsBelow(1);

        //        // Insert summary table
        //        ws.Cell(2, 1).Value = "Branch";
        //        ws.Cell(2, 2).Value = "Total Transactions";
        //        ws.Cell(2, 3).Value = "Opening Balance";
        //        ws.Cell(2, 4).Value = "Total Debit";
        //        ws.Cell(2, 5).Value = "Total Credit";
        //        ws.Cell(2, 6).Value = "Closing Balance";

        //        ws.Cell(3, 1).Value = response.FirstOrDefault().BranchName; // Replace with actual branch value if available
        //        ws.Cell(3, 2).Value = totalTransactions;
        //        ws.Cell(3, 3).Value = openingBalance;
        //        ws.Cell(3, 4).Value = totalCredit;
        //        ws.Cell(3, 5).Value = totalDebit;
        //        ws.Cell(3, 6).Value = closingBalance;

        //        // Format financial values (assuming they are numeric)
        //        ws.Cell(3, 3).Style.NumberFormat.Format = "#,##0";
        //        ws.Cell(3, 4).Style.NumberFormat.Format = "#,##0";
        //        ws.Cell(3, 5).Style.NumberFormat.Format = "#,##0";
        //        ws.Cell(3, 6).Style.NumberFormat.Format = "#,##0";

        //        // Bold summary table headers
        //        ws.Range("A2:F2").Style.Font.Bold = true;

        //        // Add borders to summary table (all borders)
        //        var summaryTableRange = ws.Range("A2:F3");
        //        summaryTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        //        summaryTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        //        // Populate data and format financial values
        //        int row = 6;

        //        // Insert a new row after the summary table
        //        ws.Row(4).InsertRowsBelow(1);
        //        ws.Range("A4:F4").Merge().Value = "Entries"; // Title above headers
        //        ws.Range("A4:F4").Style.Font.Bold = true;
        //        ws.Range("A4:F4").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        //        // Define column headers and style
        //        ws.Cell(5, 1).Value = "SN";
        //        ws.Cell(5, 2).Value = "Date";
        //        ws.Cell(5, 3).Value = "Narration";
        //        ws.Cell(5, 4).Value = "Debit";
        //        ws.Cell(5, 5).Value = "Credit";
        //        ws.Cell(5, 6).Value = "Balance";

        //        // Style headers (bold and borders)
        //        var headerRange = ws.Range("A5:F5");
        //        headerRange.Style.Font.Bold = true;
        //        headerRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        //        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        //        headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        //        int serialNumber = 1; // Start the serial number from 1
        //        foreach (var item in Data)
        //        {
        //            ws.Cell(row, 1).Value = serialNumber; // Assign the serial number
        //            ws.Cell(row, 2).Value = item.Date;
        //            ws.Cell(row, 3).Value = item.Naration;
        //            ws.Cell(row, 4).Value = item.Credit;
        //            ws.Cell(row, 5).Value = item.Debit;
        //            ws.Cell(row, 6).Value = item.Balance;

        //            // Format financial values (assuming they are numeric)
        //            for (int col = 4; col <= 6; col++)
        //            {
        //                ws.Cell(row, col).Style.NumberFormat.Format = "#,##0";
        //            }

        //            // Add borders to data cells (all borders)
        //            var dataRowRange = ws.Range($"A{row}:F{row}");
        //            dataRowRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        //            dataRowRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        //            row++;
        //            serialNumber++; // Increment the serial number for the next row
        //        }
        //        // Auto-fit columns for better readability
        //        ws.Columns().AdjustToContents();

        //        // Prepare memory stream to hold the Excel file

        //        // Prepare memory stream to hold the Excel file
        //        using (MemoryStream memoryStream = new MemoryStream())
        //        {
        //            // Save the workbook to the memory stream
        //            wb.SaveAs(memoryStream);

        //            // Generate dynamic Excel file Name based on branch Name and current date/time
        //            string excelName = $"Operation_{response.FirstOrDefault().BranchName}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
        //            excelName = excelName.Replace(" ", "_");
        //            // Set response headers
        //            Response.Clear();
        //            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        //            Response.AddHeader("content-disposition", $"attachment; filename=\"{excelName}\"");
        //            Response.BinaryWrite(memoryStream.ToArray());
        //            Response.End();
        //        }

        //        return View();
        //    }

        //}
        public async Task<ActionResult> DownloadTellerOperations()
        {
            await LoadDropdowns();
            ViewBag.Tellers = new List<StringValues>();
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
                var data = await _services.GetDailyTellers(KEY);
                return PartialView(partialView, data.ToList());

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
        public async Task<ActionResult> Ajaxloader(string Key, string path)
        {
            var listing = await _tellerServices.GetTellersDroupDownListByBranchId(Key);
            return Json(listing, JsonRequestBehavior.AllowGet);
        }
    }

}