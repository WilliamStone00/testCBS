
using CBS.BusinessService.Accounting;
using CBS.BusinessService.Accounts;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.LoanConf;
using ClosedXML.Excel;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using Microsoft.AspNet.SignalR.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using static CBS.FrontDesk.UI.Controllers.ReportsController;
using static CBS.FrontDesk.UI.Controllers.ReportsController.Export;

namespace CBS.FrontDesk.UI.Controllers
{
   [CheckSessionTimeOutAttribute]

    public class ReportsController :  BaseController
    {
        private readonly AccountingServices _accountServices;

        public ReportsController()
        {
            _accountServices = new AccountingServices();
        }
        public void ReportParameterLess()
        {
            try
            {
                string strReportName = System.Web.HttpContext.Current.Session["ReportName"]?.ToString();
                var rptSource = System.Web.HttpContext.Current.Session["rptSource"];
                var rptpath = System.Web.HttpContext.Current.Session["rptpath"]?.ToString();
                var rpttitle = System.Web.HttpContext.Current.Session["rpttitle"]?.ToString();

                if (string.IsNullOrEmpty(strReportName) || rptSource == null || rptpath == null || rpttitle == null)
                {
                    Response.Write("<H2>No Report with such name found</H2>");
                    return;
                }

                ReportDocument rd = new ReportDocument();
                string strRptPath = Server.MapPath(rptpath);
                rd.Load(strRptPath);
                if (rptSource.GetType() != typeof(string))
                {
                    rd.SetDataSource(rptSource);
                }

                string year = System.Web.HttpContext.Current.Session["Year"]?.ToString() ?? "Non";
                string dates = System.Web.HttpContext.Current.Session["Dates"]?.ToString() ?? "Non";
                string strFromDate = System.Web.HttpContext.Current.Session["DateFrom"]?.ToString() ?? "Non";
                string strToDate = System.Web.HttpContext.Current.Session["DateTo"]?.ToString() ?? "Non";

                if (year != "Non")
                {
                    rd.SetParameterValue("param", $"Header summary: {year}");
                }

                if (dates != "Non" && !string.IsNullOrEmpty(strFromDate) && !string.IsNullOrEmpty(strToDate))
                {
                    rd.SetParameterValue("DateFrom", strFromDate);
                    rd.SetParameterValue("DateTo", strToDate);
                }

                string savedFileName = $"{rpttitle}-{DateTime.UtcNow.ToString("dd_mm_yyyy_hhmmss")}";
                rd.ExportToHttpResponse(ExportFormatType.PortableDocFormat, System.Web.HttpContext.Current.Response, false, savedFileName);
                CleanReport(rd);
            }
            catch (Exception ex)
            {
                // Log the exception
                // Handle specific exceptions if needed
                Response.Write("<H2>An error occurred while generating the report</H2>");
            }
        }

        public void CleanReport(ReportDocument rd)
        {
            rd.Close();
            rd.Clone();
            rd.Dispose();
            rd = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        public void ReportWithParameter()
        {
            try
            {
                bool isValid = true;
                string strReportName = System.Web.HttpContext.Current.Session["ReportName"].ToString();
                var rptSource = System.Web.HttpContext.Current.Session["rptSource"];
                var rptpath = System.Web.HttpContext.Current.Session["rptpath"].ToString();
                string strFromDate = System.Web.HttpContext.Current.Session["DateFrom"].ToString();     // Setting FromDate 
                string strToDate = System.Web.HttpContext.Current.Session["DateTo"].ToString();         // Setting ToDate    
                string strtitle = System.Web.HttpContext.Current.Session["rpttitle"].ToString();         // Setting ToDate    

                if (string.IsNullOrEmpty(strReportName))
                {
                    isValid = false;
                }
                if (isValid)
                {
                    ReportDocument rd = new ReportDocument();
                    string strRptPath = Server.MapPath(rptpath);
                    rd.Load(strRptPath);
                    if (rptSource != null && rptSource.GetType().ToString() != "System.String")
                        rd.SetDataSource(rptSource);
                    if (!string.IsNullOrEmpty(strFromDate))
                        rd.SetParameterValue("DateFrom", strFromDate);
                    if (!string.IsNullOrEmpty(strToDate))
                        rd.SetParameterValue("DateTo", strToDate);

                    string SavedFileName = string.Format($"{strtitle}-{DateTime.UtcNow.ToString("dd_mm_yyyy_hhmmss")}");
                    rd.ExportToHttpResponse(ExportFormatType.PortableDocFormat, System.Web.HttpContext.Current.Response, false, SavedFileName);
                    CleanReport(rd);
                }
                else
                {
                    Response.Write("<H2>Nothing Found; No Report name found</H2>");
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Error in formula PercentagePassed"))
                {
                    Response.Write("<H2>No Data was found</H2>");
                }
                else
                {
                    Response.Write(ex.ToString() + "<H2>Nothing Found; report session expired</H2>");
                }

            }
        }
        public ActionResult DownloadExcelFile()
        {
            var rptSource = System.Web.HttpContext.Current.Session["rptSource"];
            string strtitle = System.Web.HttpContext.Current.Session["rpttitle"].ToString();
            string rpTType = System.Web.HttpContext.Current.Session["rptType"].ToString();
            string rptpath = System.Web.HttpContext.Current.Session["rptpath"].ToString();
            var model = rptSource;
           
                if (rpTType == "EXCEL")
                {
                    Export export = new Export();
                    export.ToExcel(Response, model as IEnumerable<object>, strtitle);

                }
                else
                {

                    if (rptSource != "empty")
                    {
                        ReportDocument rd = new ReportDocument();
                        string strRptPath = Server.MapPath(rptpath);
                        rd.Load(strRptPath);

                        rd.SetDataSource(rptSource);
                        string SavedFileName = string.Format($"{strtitle}-{DateTime.UtcNow.Date.ToString("dd_mm_yyyy_hhmmss")}");
                        // Export the report to a byte array
                        Stream stream = rd.ExportToStream(ExportFormatType.PortableDocFormat);
                        byte[] bytes = new byte[stream.Length];
                        stream.Read(bytes, 0, bytes.Length);

                        // Clear the response and set the content type
                        Response.ClearContent();
                        Response.ClearHeaders();
                        Response.ContentType = "application/pdf";

                        // Write the report bytes to the response
                        Response.BinaryWrite(bytes);
                        Response.Flush();
                        Response.End();
                        //rd.ExportToHttpResponse(ExportFormatType.PortableDocFormat, System.Web.HttpContext.Current.Response, false, SavedFileName);
                        //CleanReport(rd);


                    }

                }
                return new EmptyResult();
          

        }

 

        public ActionResult DownloadExcelFileForTB4C()
        {
            var rptSource = System.Web.HttpContext.Current.Session["rptSource"];
            string strtitle = System.Web.HttpContext.Current.Session["rpttitle"].ToString();
            string rpTType = System.Web.HttpContext.Current.Session["rptType"].ToString();
            string rptpath = System.Web.HttpContext.Current.Session["rptpath"].ToString();
            var model = rptSource;

            if (rpTType == "EXCEL")
            {
                Export export = new Export();
                export.ToExcel(Response, model as IEnumerable<object>, strtitle);

            }
            else
            {

                if (rptSource != "empty")
                {
                    ReportDocument rd = new ReportDocument();
                    string strRptPath = Server.MapPath(rptpath);
                    rd.Load(strRptPath);

                    rd.SetDataSource(rptSource);
                    string SavedFileName = string.Format($"{strtitle}-{DateTime.UtcNow.Date.ToString("dd_mm_yyyy_hhmmss")}");
                    // Export the report to a byte array
                    Stream stream = rd.ExportToStream(ExportFormatType.PortableDocFormat);
                    byte[] bytes = new byte[stream.Length];
                    stream.Read(bytes, 0, bytes.Length);

                    // Clear the response and set the content type
                    Response.ClearContent();
                    Response.ClearHeaders();
                    Response.ContentType = "application/pdf";

                    // Write the report bytes to the response
                    Response.BinaryWrite(bytes);
                    Response.Flush();
                    Response.End();
                    //rd.ExportToHttpResponse(ExportFormatType.PortableDocFormat, System.Web.HttpContext.Current.Response, false, SavedFileName);
                    //CleanReport(rd);


                }

            }
            return new EmptyResult();


        }

        
        public ActionResult PrintAccountLedgerDtoInExcel()
        {
            //new Dto();
            //List<TrialBalance4ColumnDto> accounts = new List<TrialBalance4ColumnDto>();

            var rptSource = System.Web.HttpContext.Current.Session["rptSource"];
            var paths = System.Web.HttpContext.Current.Session["rptpath"].ToString();
            var rpttitle = System.Web.HttpContext.Current.Session["rpttitle"].ToString();

            List<AccountLedgerDto> accounts = (rptSource == "empty") ? new List<AccountLedgerDto>() : (List<AccountLedgerDto>)rptSource;
            AccountLedgerDto trialBalance = (rptSource == "empty") ? new AccountLedgerDto() : accounts[0];
            var rpt = " General Ledger as of the " + trialBalance.FromDate;


            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add(rpttitle);
                var headerStyle = workbook.Style;

                headerStyle.Font.Bold = true;
                headerStyle.Font.FontSize = 14;
                headerStyle.Font.FontColor = XLColor.Black;
                // Apply border to branch range
                headerStyle.Border.BottomBorder = XLBorderStyleValues.Thin;
                headerStyle.Border.LeftBorder = XLBorderStyleValues.Thin;
                headerStyle.Border.RightBorder = XLBorderStyleValues.Thin;
                headerStyle.Border.TopBorder = XLBorderStyleValues.Thin;
                // Print letterhead
                worksheet.Cell(1, 2).Value = trialBalance.BranchName;
                worksheet.Cell(2, 2).Value = $"{trialBalance.BranchLocation}";
                worksheet.Cell(3, 2).Value = $" {trialBalance.Capital}";
                worksheet.Cell(4, 2).Value = $"{trialBalance.ImmatriculationNumber}";
                worksheet.Cell(5, 2).Value = $"{trialBalance.WebSite}";
                worksheet.Cell(6, 2).Value = $" {trialBalance.BranchTelephone}";
                worksheet.Cell(7, 2).Value = $"{trialBalance.HeadOfficeTelePhone}";

                worksheet.Cell(1, 1).Value = "BranchName";
                worksheet.Cell(2, 1).Value = $"Address";
                worksheet.Cell(3, 1).Value = $"Capital";
                worksheet.Cell(4, 1).Value = $"Immatriculation Number";
                worksheet.Cell(5, 1).Value = $"Website";
                worksheet.Cell(6, 1).Value = $"Branch Telephone";
                worksheet.Cell(7, 1).Value = $"Head Office Telephone";
                // Apply header style

                var headerRange = worksheet.Range(1, 1, 7, 1);
                headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                headerRange.Style.Font.FontSize = 12;
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Alignment.WrapText = true;
                headerRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;

                var headerRange2 = worksheet.Range(1, 1, 7, 2);

                headerRange2.Style.Font.FontSize = 12;

                headerRange2.Style.Alignment.WrapText = true;
                headerRange2.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                headerRange2.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                headerRange2.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                headerRange2.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                // Print account details
                var titleRange = worksheet.Range("A10:D10");
                titleRange.Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                titleRange.Style.Font.Bold = true;
                titleRange.Style.Font.FontSize = 14;
                titleRange.Style.Font.FontColor = XLColor.Black;
                // Apply border to title range
                titleRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                titleRange.Value = rpt;
                // Print balance sheet header
                worksheet.Cell(12, 1).Value = "Account Number";
                worksheet.Cell(12, 2).Value = "Account Name";
                worksheet.Cell(12, 3).Value = "Current Balance";
                //worksheet.Cell(12, 4).Value = "Debit Balance";
                //worksheet.Cell(12, 5).Value = "Credit Balance";
                //worksheet.Cell(12, 6).Value = "Current Balance";


                // Apply header style
                var headerRange0 = worksheet.Range(12, 1, 12, 3);
                headerRange0.Style.Fill.BackgroundColor = XLColor.LightBlue;
                headerRange0.Style.Font.FontSize = 12;
                headerRange0.Style.Font.Bold = true;
                headerRange0.Style.Alignment.WrapText = true;
                headerRange0.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                headerRange0.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                headerRange0.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                headerRange0.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                var headerRange10 = worksheet.Range(13, 1, accounts.Count() + 13, 3);
                headerRange10.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                headerRange10.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                headerRange10.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                headerRange10.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                int row = 13;
                foreach (var account in accounts)
                {
                    worksheet.Cell(row, 1).Value = account.AccountNumber;
                    worksheet.Cell(row, 2).Value = account.AccountName;
                    worksheet.Cell(row, 3).Value = account.CurrentBalance;


                    row++;
                }

                worksheet.Columns().AdjustToContents();

                if ((rptSource == "empty"))
                {

                }
                else
                {
                    //var footerRange = worksheet.Range(accounts.Count() + 13, 2, accounts.Count() + 13, 8);
                    //footerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                    //footerRange.Style.Font.FontSize = 12;
                    //footerRange.Style.Font.Bold = true;
                    //footerRange.Style.Alignment.WrapText = false;
                    //footerRange.Style.Alignment.JustifyLastLine = false;
                    //footerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    //footerRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    //footerRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    //footerRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    //footerRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    //// Print totals
                    //worksheet.Cell(row, 2).Value = "Totals";
                    //worksheet.Cell(row, 3).Value = trialBalance.totalBeginningBalance.ToString();
                    //worksheet.Cell(row, 4).Value = trialBalance.totalDebitBalance.ToString();
                    //worksheet.Cell(row, 5).Value = trialBalance.totalCreditBalance.ToString();
                    //worksheet.Cell(row, 6).Value = trialBalance.totalEndingBalance.ToString();

                }


                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;

                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", _accountServices.GetBranchName() + "-" + trialBalance.BranchName + "GeneralLedger.xlsx");
                }
                //worksheet.Cell(row + 1, 1).Value = $"Ending Balance Sign: {trialBalance.EndingBalanceSigne}";

                // Save the workbook

            }
            //return new EmptyResult();
        }
        //public ActionResult PrintAccountNotPresent()
        //{
        //    //new Dto();
        //    //List<TrialBalance4ColumnDto> accounts = new List<TrialBalance4ColumnDto>();

        //    var rptSource = System.Web.HttpContext.Current.Session["account"+this.HttpContext.Session.SessionID];


        //    UploadAccountResultServiceResponse accounts = (rptSource == "empty") ? new UploadAccountResultServiceResponse() : (UploadAccountResultServiceResponse)rptSource;
        //    UploadAccountResult trialBalance = (rptSource == "empty") ? new UploadAccountResult() : accounts.apiResponseData;
        //     var rpt = $" List of Account not present in {trialBalance.BranchName} ";


        //    using (var workbook = new XLWorkbook())
        //    {
        //        var worksheet = workbook.Worksheets.Add("AccountNotPresent");
        //        var headerStyle = workbook.Style;

        //        headerStyle.Font.Bold = true;
        //        headerStyle.Font.FontSize = 14;
        //        headerStyle.Font.FontColor = XLColor.Black;
        //        // Apply border to branch range
        //        headerStyle.Border.BottomBorder = XLBorderStyleValues.Thin;
        //        headerStyle.Border.LeftBorder = XLBorderStyleValues.Thin;
        //        headerStyle.Border.RightBorder = XLBorderStyleValues.Thin;
        //        headerStyle.Border.TopBorder = XLBorderStyleValues.Thin;
        //        // Print letterhead
        //        worksheet.Cell(1, 2).Value = trialBalance.BranchName;
        //        worksheet.Cell(2, 2).Value = $"{trialBalance.BranchLocation}";
        //        worksheet.Cell(3, 2).Value = $" {trialBalance.Capital}";
        //        worksheet.Cell(4, 2).Value = $"{trialBalance.ImmatriculationNumber}";
        //        worksheet.Cell(5, 2).Value = $"{trialBalance.WebSite}";
        //        worksheet.Cell(6, 2).Value = $" {trialBalance.BranchTelephone}";
        //        worksheet.Cell(7, 2).Value = $"{trialBalance.HeadOfficeTelePhone}";

        //        worksheet.Cell(1, 1).Value = "BranchName";
        //        worksheet.Cell(2, 1).Value = $"Address";
        //        worksheet.Cell(3, 1).Value = $"Capital";
        //        worksheet.Cell(4, 1).Value = $"Immatriculation Number";
        //        worksheet.Cell(5, 1).Value = $"Website";
        //        worksheet.Cell(6, 1).Value = $"Branch Telephone";
        //        worksheet.Cell(7, 1).Value = $"Head Office Telephone";
        //        // Apply header style

        //        var headerRange = worksheet.Range(1, 1, 7, 1);
        //        headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
        //        headerRange.Style.Font.FontSize = 12;
        //        headerRange.Style.Font.Bold = true;
        //        headerRange.Style.Alignment.WrapText = true;
        //        headerRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        //        headerRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
        //        headerRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
        //        headerRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;

        //        var headerRange2 = worksheet.Range(1, 1, 7, 2);

        //        headerRange2.Style.Font.FontSize = 12;

        //        headerRange2.Style.Alignment.WrapText = true;
        //        headerRange2.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        //        headerRange2.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
        //        headerRange2.Style.Border.RightBorder = XLBorderStyleValues.Thin;
        //        headerRange2.Style.Border.TopBorder = XLBorderStyleValues.Thin;
        //        // Print account details
        //        var titleRange = worksheet.Range("A10:D10");
        //        titleRange.Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        //        titleRange.Style.Font.Bold = true;
        //        titleRange.Style.Font.FontSize = 14;
        //        titleRange.Style.Font.FontColor = XLColor.Black;
        //        // Apply border to title range
        //        titleRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        //        titleRange.Value = rpt;
        //        // Print balance sheet header
        //        worksheet.Cell(12, 1).Value = "Account Number";
        //        worksheet.Cell(12, 2).Value = "Account Name";
        //        //worksheet.Cell(12, 3).Value = "Current Balance";
        //        ////worksheet.Cell(12, 4).Value = "Debit Balance";
        //        ////worksheet.Cell(12, 5).Value = "Credit Balance";
        //        ////worksheet.Cell(12, 6).Value = "Current Balance";


        //        // Apply header style
        //        var headerRange0 = worksheet.Range(12, 1, 12, 3);
        //        headerRange0.Style.Fill.BackgroundColor = XLColor.LightBlue;
        //        headerRange0.Style.Font.FontSize = 12;
        //        headerRange0.Style.Font.Bold = true;
        //        headerRange0.Style.Alignment.WrapText = true;
        //        headerRange0.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        //        headerRange0.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
        //        headerRange0.Style.Border.RightBorder = XLBorderStyleValues.Thin;
        //        headerRange0.Style.Border.TopBorder = XLBorderStyleValues.Thin;
        //        var headerRange10 = worksheet.Range(13, 1, accounts.apiResponseData.ListOfAccount.Count() + 13,3);
        //        headerRange10.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        //        headerRange10.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
        //        headerRange10.Style.Border.RightBorder = XLBorderStyleValues.Thin;
        //        headerRange10.Style.Border.TopBorder = XLBorderStyleValues.Thin;
        //        int row = 13;
        //        foreach (var account in accounts.apiResponseData.ListOfAccount)
        //        {
        //            worksheet.Cell(row, 1).Value = account.AccountNumber;
        //            worksheet.Cell(row, 2).Value = account.AccountName;



        //            row++;
        //        }

        //        worksheet.Columns().AdjustToContents();

        //        if ((rptSource == "empty"))
        //        {

        //        }
        //        else
        //        {
        //            //var footerRange = worksheet.Range(accounts.Count() + 13, 2, accounts.Count() + 13, 8);
        //            //footerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
        //            //footerRange.Style.Font.FontSize = 12;
        //            //footerRange.Style.Font.Bold = true;
        //            //footerRange.Style.Alignment.WrapText = false;
        //            //footerRange.Style.Alignment.JustifyLastLine = false;
        //            //footerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        //            //footerRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        //            //footerRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
        //            //footerRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
        //            //footerRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
        //            //// Print totals
        //            //worksheet.Cell(row, 2).Value = "Totals";
        //            //worksheet.Cell(row, 3).Value = trialBalance.totalBeginningBalance.ToString();
        //            //worksheet.Cell(row, 4).Value = trialBalance.totalDebitBalance.ToString();
        //            //worksheet.Cell(row, 5).Value = trialBalance.totalCreditBalance.ToString();
        //            //worksheet.Cell(row, 6).Value = trialBalance.totalEndingBalance.ToString();

        //        }


        //        using (var stream = new MemoryStream())
        //        {
        //            if (string.IsNullOrEmpty(trialBalance.file_path))
        //            {
        //                return View();
        //            }

        //            if (!System.IO.File.Exists(trialBalance.file_path))
        //            {
        //                return View();
        //            }

        //            var fileName = Path.GetFileName(trialBalance.file_path);
        //            var mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"; // for .xlsx files

        //            // Read the file
        //            var fileBytes = System.IO.File.ReadAllBytes(trialBalance.file_path);
        //            workbook.SaveAs(stream);
        //            stream.Position = 0;
        //            // Set headers and return file in a single statement
        //            return File(fileBytes, mimeType, fileName);


        //            //return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", _accountServices.GetBranchName() + "-" + trialBalance.BranchName + "AccountNotFound.xlsx");
        //        }
        //        //worksheet.Cell(row + 1, 1).Value = $"Ending Balance Sign: {trialBalance.EndingBalanceSigne}";

        //        // Save the workbook

        //    }
        //    //return new EmptyResult();
        //}

 
        public ActionResult PrintJournalEntryDtoInExcel()
        {
            //new Dto();
            //List<TrialBalance4ColumnDto> accounts = new List<TrialBalance4ColumnDto>();

            var rptSource = System.Web.HttpContext.Current.Session["rptSource"];
            var paths = System.Web.HttpContext.Current.Session["rptpath"].ToString();
            var rpttitle = System.Web.HttpContext.Current.Session["rpttitle"].ToString();

            List<JournalEntryDto> accounts = (rptSource == "empty") ? new List<JournalEntryDto>() : (List<JournalEntryDto>)rptSource;
            JournalEntryDto trialBalance = (rptSource == "empty") ? new JournalEntryDto() : accounts[0];

            #region MyRegion
            //using (var workbook = new XLWorkbook())
            //{
            //    var worksheet = workbook.Worksheets.Add(rpttitle);
            //    var headerStyle = workbook.Style;
            //    headerStyle.Font.Bold = true;

            //    // Print letterhead
            //    worksheet.Cell(1, 2).Value = trialBalance.BranchName;
            //    worksheet.Cell(2, 2).Value = $"{trialBalance.BranchLocation}, {trialBalance.Address}";
            //    worksheet.Cell(3, 2).Value = $" {trialBalance.Capital}";
            //    worksheet.Cell(4, 2).Value = $"{trialBalance.ImmatriculationNumber}";
            //    worksheet.Cell(5, 2).Value = $"{trialBalance.WebSite}";

            //    worksheet.Cell(1, 1).Value = "BranchName";
            //    worksheet.Cell(2, 1).Value = $"Address";
            //    worksheet.Cell(3, 1).Value = $"Capital";
            //    worksheet.Cell(4, 1).Value = $"Immatriculation Number";
            //    worksheet.Cell(5, 1).Value = $"Website";

            //    // Apply header style
            //    worksheet.Range(1, 1, 5, 1).Style = headerStyle;

            //    // Add title in bold with font-18
            //    var titleStyle = workbook.Style;
            //    titleStyle.Font.Bold = true;
            //    titleStyle.Font.FontSize = 16;

            //    worksheet.Cell(7, 1).Value = rpttitle;
            //    worksheet.Cell(7, 1).Style = titleStyle;

            //    // Print balance sheet header
            //    worksheet.Cell(9, 1).Value = "Entry Date";
            //    worksheet.Cell(9, 2).Value = "Account Name";
            //    worksheet.Cell(9, 3).Value = "Account Number";
            //    //worksheet.Cell(9, 4).Value = "Description";
            //    worksheet.Cell(9, 4).Value = "DebitAmount";
            //    worksheet.Cell(9, 5).Value = "CreditAmount";
            //    // Apply header style
            //    worksheet.Range(9, 1, 7, 5).Style = headerStyle;

            //    // Print account details
            //    int row = 11;
            //    foreach (var account in accounts)
            //    {
            //        worksheet.Cell(row, 1).Value = account.EntryDatetime;
            //        worksheet.Cell(row, 2).Value = account.Reference;
            //        worksheet.Cell(row, 3).Value = account.AccountNumber;
            //        //worksheet.Cell(row, 4).Value = account.Description;
            //        worksheet.Cell(row, 5).Value = account.DebitAmount;
            //        worksheet.Cell(row, 6).Value = account.CreditAmount;
            //        row++;
            //    }

            //    worksheet.Columns().AdjustToContents();

            //    if ((rptSource == "empty"))
            //    {
            //    }
            //    else
            //    {
            //        // Print totals
            //        //worksheet.Cell(row, 2).Value = "Totals";
            //        //worksheet.Cell(row, 3).Value = trialBalance.totalBeginningBalance.ToString();
            //        //worksheet.Cell(row, 4).Value = trialBalance.totalDebitBalance.ToString();
            //        //worksheet.Cell(row, 5).Value = trialBalance.totalCreditBalance.ToString();
            //        //worksheet.Cell(row, 6).Value = trialBalance.totalEndingBalance.ToString();
            //    }

            //    using (var stream = new MemoryStream())
            //    {
            //        workbook.SaveAs(stream);
            //        stream.Position = 0;
            //        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{rpttitle}.xlsx");
            //    }
            //} 
            #endregion

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add(rpttitle);
                var headerStyle = workbook.Style;

                headerStyle.Font.Bold = true;
                headerStyle.Font.FontSize = 14;
                headerStyle.Font.FontColor = XLColor.Black;
                // Apply border to branch range
                headerStyle.Border.BottomBorder = XLBorderStyleValues.Thin;
                headerStyle.Border.LeftBorder = XLBorderStyleValues.Thin;
                headerStyle.Border.RightBorder = XLBorderStyleValues.Thin;
                headerStyle.Border.TopBorder = XLBorderStyleValues.Thin;
                // Print letterhead
                worksheet.Cell(1, 2).Value = trialBalance.BranchName;
                worksheet.Cell(2, 2).Value = $"{trialBalance.BranchLocation}";
                worksheet.Cell(3, 2).Value = $" {trialBalance.Capital}";
                worksheet.Cell(4, 2).Value = $"{trialBalance.ImmatriculationNumber}";
                worksheet.Cell(5, 2).Value = $"{trialBalance.WebSite}";
                worksheet.Cell(6, 2).Value = $" {trialBalance.BranchTelephone}";
                worksheet.Cell(7, 2).Value = $"{trialBalance.HeadOfficeTelePhone}";

                worksheet.Cell(1, 1).Value = "BranchName";
                worksheet.Cell(2, 1).Value = $"Address";
                worksheet.Cell(3, 1).Value = $"Capital";
                worksheet.Cell(4, 1).Value = $"Immatriculation Number";
                worksheet.Cell(5, 1).Value = $"Website";
                worksheet.Cell(6, 1).Value = $"Branch Telephone";
                worksheet.Cell(7, 1).Value = $"Head Office Telephone";
                // Apply header style

                var headerRange = worksheet.Range(1, 1, 7, 1);
                headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                headerRange.Style.Font.FontSize = 12;
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Alignment.WrapText = true;
                headerRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;

                var headerRange2 = worksheet.Range(1, 1, 7, 2);

                headerRange2.Style.Font.FontSize = 12;

                headerRange2.Style.Alignment.WrapText = true;
                headerRange2.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                headerRange2.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                headerRange2.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                headerRange2.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                // Print account details
                var titleRange = worksheet.Range("B10:F10");
                titleRange.Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                titleRange.Style.Font.Bold = true;
                titleRange.Style.Font.FontSize = 14;
                titleRange.Style.Font.FontColor = XLColor.Black;
                // Apply border to title range
                titleRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                titleRange.Value = $" {trialBalance.BranchName} Journal Entries from {trialBalance.FromDate} to {trialBalance.ToDate}";
                // Print balance sheet header

                worksheet.Cell(12, 1).Value = "Entry Date";
                worksheet.Cell(12, 2).Value = "Reference";
                worksheet.Cell(12, 3).Value = "Account Number";
                worksheet.Cell(12,4).Value = "Description";
                worksheet.Cell(12, 5).Value = "Debit Balance";
                worksheet.Cell(12, 6).Value = "Credit Balance";
             


                // Apply header style
                var headerRange0 = worksheet.Range(12, 1, 12, 8);
                headerRange0.Style.Fill.BackgroundColor = XLColor.LightBlue;
                headerRange0.Style.Font.FontSize = 12;
                headerRange0.Style.Font.Bold = true;
                headerRange0.Style.Alignment.WrapText = true;
                headerRange0.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                headerRange0.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                headerRange0.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                headerRange0.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                var headerRange10 = worksheet.Range(13, 1, accounts.Count() + 13, 8);
                headerRange10.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                headerRange10.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                headerRange10.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                headerRange10.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                int row = 13;
                foreach (var account in accounts)
                {
                    worksheet.Cell(row, 1).Value = account.EntryDate;
                    worksheet.Cell(row, 2).Value = account.Reference;
                    worksheet.Cell(row, 3).Value = account.AccountNumber;
                    worksheet.Cell(row, 4).Value = account.Description;
                    worksheet.Cell(row,5).Value = account.Debit;
                    worksheet.Cell(row, 6).Value = account.Credit;
               

                    row++;
                }

                worksheet.Columns().AdjustToContents();


                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;

                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", _accountServices.GetBranchName() + "-" + trialBalance.BranchName + "JE.xlsx");
                }
                //worksheet.Cell(row + 1, 1).Value = $"Ending Balance Sign: {trialBalance.EndingBalanceSigne}";

                // Save the workbook

            }

            //return new EmptyResult();
        }
        public ActionResult PrintTrialBalance4Column()
        {
            //new Dto();
            //List<TrialBalance4ColumnDto> accounts = new List<TrialBalance4ColumnDto>();


            var rptSource = System.Web.HttpContext.Current.Session["rptSource"];
            var paths = System.Web.HttpContext.Current.Session["rptpath"].ToString();
            var rpttitle = System.Web.HttpContext.Current.Session["rpttitle"].ToString();
 
            List<TrialBalance4ColumnDto> accounts = (rptSource == "empty") ? new List<TrialBalance4ColumnDto>() : (List<TrialBalance4ColumnDto>)rptSource;
            TrialBalance4ColumnDto trialBalance = (rptSource == "empty") ? new TrialBalance4ColumnDto() : accounts[0];

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add(rpttitle);
                var headerStyle = workbook.Style;

                headerStyle.Font.Bold = true;
                headerStyle.Font.FontSize = 14;
                headerStyle.Font.FontColor = XLColor.Black;
                // Apply border to branch range
                headerStyle.Border.BottomBorder = XLBorderStyleValues.Thin;
                headerStyle.Border.LeftBorder = XLBorderStyleValues.Thin;
                headerStyle.Border.RightBorder = XLBorderStyleValues.Thin;
                headerStyle.Border.TopBorder = XLBorderStyleValues.Thin;
                // Print letterhead
                worksheet.Cell(1, 2).Value = trialBalance.BranchName;
                worksheet.Cell(2, 2).Value = $"{trialBalance.BranchLocation}, {trialBalance.BranchAddress}";
                worksheet.Cell(3, 2).Value = $" {trialBalance.Capital}";
                worksheet.Cell(4, 2).Value = $"{trialBalance.ImmatriculationNumber}";
                worksheet.Cell(5, 2).Value = $"{trialBalance.WebSite}";
                worksheet.Cell(6, 2).Value = $" {trialBalance.BranchTelephone}";
                worksheet.Cell(7, 2).Value = $"{trialBalance.HeadOfficeTelePhone}";

                worksheet.Cell(1, 1).Value = "BranchName";
                worksheet.Cell(2, 1).Value = $"Address";
                worksheet.Cell(3, 1).Value = $"Capital";
                worksheet.Cell(4, 1).Value = $"Immatriculation Number";
                worksheet.Cell(5, 1).Value = $"Website";
                worksheet.Cell(6, 1).Value = $"Branch Telephone";
                worksheet.Cell(7, 1).Value = $"Head Office Telephone";
                // Apply header style

                var headerRange = worksheet.Range(1, 1, 7, 1);
                headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                headerRange.Style.Font.FontSize = 12;
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Alignment.WrapText = true;
                headerRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;

                var headerRange2 = worksheet.Range(1, 1, 7, 2);

                headerRange2.Style.Font.FontSize = 12;

                headerRange2.Style.Alignment.WrapText = true;
                headerRange2.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                headerRange2.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                headerRange2.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                headerRange2.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                // Print account details
                var titleRange = worksheet.Range("B10:G10");
                titleRange.Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                titleRange.Style.Font.Bold = true;
                titleRange.Style.Font.FontSize = 14;
                titleRange.Style.Font.FontColor = XLColor.Black;
                // Apply border to title range
                titleRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                titleRange.Value = $"  Trial Balance as at {trialBalance.FromDate} to {trialBalance.ToDate}";
                // Print balance sheet header
                worksheet.Cell(12, 1).Value = "Account Number";
                worksheet.Cell(12, 2).Value = "Account Name";
                worksheet.Cell(12, 3).Value = "Beginning Balance";
                worksheet.Cell(12, 4).Value = "Debit Balance";
                worksheet.Cell(12, 5).Value = "Credit Balance";
                worksheet.Cell(12, 6).Value = "Ending Balance";
  

                // Apply header style
                var headerRange0 = worksheet.Range(12, 1, 12, 8);
                headerRange0.Style.Fill.BackgroundColor = XLColor.LightBlue;
                headerRange0.Style.Font.FontSize = 12;
                headerRange0.Style.Font.Bold = true;
                headerRange0.Style.Alignment.WrapText = true;
                headerRange0.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                headerRange0.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                headerRange0.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                headerRange0.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                var headerRange10 = worksheet.Range(13, 1, accounts.Count() + 13, 8);
                headerRange10.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                headerRange10.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                headerRange10.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                headerRange10.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                int row = 13;
                foreach (var account in accounts)
                {
                    worksheet.Cell(row, 1).Value = account.AccountNumber;
                    worksheet.Cell(row, 2).Value = account.AccountName;
                    worksheet.Cell(row, 3).Value = account.BeginningBalance;
                   
                    worksheet.Cell(row, 4).Value = account.DebitBalance;
                    worksheet.Cell(row, 5).Value = account.CreditBalance;
                    worksheet.Cell(row, 6).Value = account.EndingBalance;
                  
                    row++;
                }
                
                worksheet.Columns().AdjustToContents();

                if ((rptSource == "empty"))
                {

                }
                else
                {
                    var footerRange = worksheet.Range(accounts.Count() + 13, 2, accounts.Count() + 13, 8);
                    footerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                    footerRange.Style.Font.FontSize = 12;
                    footerRange.Style.Font.Bold = true;
                    footerRange.Style.Alignment.WrapText = false;
                    footerRange.Style.Alignment.JustifyLastLine = false;
                    footerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    footerRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    footerRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    footerRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    footerRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    // Print totals
                    worksheet.Cell(row, 2).Value = "Totals";
                    worksheet.Cell(row, 3).Value = trialBalance.totalBeginningBalance.ToString();
                    worksheet.Cell(row, 4).Value = trialBalance.totalDebitBalance.ToString();
                    worksheet.Cell(row, 5).Value = trialBalance.totalCreditBalance.ToString();
                    worksheet.Cell(row, 6).Value = trialBalance.totalEndingBalance.ToString();
          
                }


                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;

                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", _accountServices.GetBranchName() + "-" + trialBalance.BranchName + "JournalENtries.xlsx");
                }
                //worksheet.Cell(row + 1, 1).Value = $"Ending Balance Sign: {trialBalance.EndingBalanceSigne}";

                // Save the workbook

            }

            //return new EmptyResult();
        }

        public ActionResult PrintTrialBalance6Column()
        {
            //new Dto();
            //List<TrialBalance4ColumnDto> accounts = new List<TrialBalance4ColumnDto>();


            var rptSource = System.Web.HttpContext.Current.Session["rptSource"];
            var paths = System.Web.HttpContext.Current.Session["rptpath"].ToString();
            var rpttitle = System.Web.HttpContext.Current.Session["rpttitle"].ToString();
            List<TrialBalance6ColumnDto> accounts = (rptSource=="empty")? new List<TrialBalance6ColumnDto>(): (List<TrialBalance6ColumnDto>)rptSource;
            TrialBalance6ColumnDto trialBalance = (rptSource == "empty") ? new TrialBalance6ColumnDto(): accounts[0];
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add(rpttitle);
                var headerStyle = workbook.Style;

                headerStyle.Font.Bold = true;
                headerStyle.Font.FontSize = 14;
                headerStyle.Font.FontColor = XLColor.Black;
                // Apply border to branch range
                headerStyle.Border.BottomBorder = XLBorderStyleValues.Thin;
                headerStyle.Border.LeftBorder = XLBorderStyleValues.Thin;
                headerStyle.Border.RightBorder = XLBorderStyleValues.Thin;
                headerStyle.Border.TopBorder = XLBorderStyleValues.Thin;
                // Print letterhead
                worksheet.Cell(1, 2).Value = trialBalance.branchName;
                worksheet.Cell(2, 2).Value = $"{trialBalance.branchLocation}, {trialBalance.branchAddress}";
                worksheet.Cell(3, 2).Value = $" {trialBalance.capital}";
                worksheet.Cell(4, 2).Value = $"{trialBalance.immatriculationNumber}";
                worksheet.Cell(5, 2).Value = $"{trialBalance.webSite}";
                worksheet.Cell(6, 2).Value = $" {trialBalance.branchTelephone}";
                worksheet.Cell(7, 2).Value = $"{trialBalance.headOfficeTelePhone}";

                worksheet.Cell(1, 1).Value = "BranchName";
                worksheet.Cell(2, 1).Value = $"Address";
                worksheet.Cell(3, 1).Value = $"Capital";
                worksheet.Cell(4, 1).Value = $"Immatriculation Number";
                worksheet.Cell(5, 1).Value = $"Website";
                worksheet.Cell(6, 1).Value = $"Branch Telephone";
                worksheet.Cell(7, 1).Value = $"Head Office Telephone";
                // Apply header style
             
                var headerRange = worksheet.Range(1, 1, 7, 1);
                headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                headerRange.Style.Font.FontSize = 12;
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Alignment.WrapText = true;
                headerRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;

                var headerRange2 = worksheet.Range(1, 1, 7, 2);

                headerRange2.Style.Font.FontSize = 12;
 
                headerRange2.Style.Alignment.WrapText = true;
                headerRange2.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                headerRange2.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                headerRange2.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                headerRange2.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                // Print account details
                var titleRange = worksheet.Range("B10:G10");
                titleRange.Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                titleRange.Style.Font.Bold = true;
                titleRange.Style.Font.FontSize = 14;
                titleRange.Style.Font.FontColor = XLColor.Black;
                // Apply border to title range
                titleRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                titleRange.Value = $"  Trial Balance as at {trialBalance.fromDate} to {trialBalance.toDate}";
                // Print balance sheet header
                worksheet.Cell(12, 1).Value = "Account Number";
                worksheet.Cell(12, 2).Value = "Account Name";
                worksheet.Cell(12, 3).Value = "Beginning Debit Balance";
                worksheet.Cell(12, 4).Value = "Beginning Credit Balance";
                worksheet.Cell(12, 5).Value = "Debit Balance";
                worksheet.Cell(12, 6).Value = "Credit Balance";
                worksheet.Cell(12, 7).Value = "Ending Debit Balance";
                worksheet.Cell(12, 8).Value = "Ending Credit Balance";

                // Apply header style
                var headerRange0 = worksheet.Range(12, 1, 12, 8);
                headerRange0.Style.Fill.BackgroundColor = XLColor.LightBlue;
                headerRange0.Style.Font.FontSize = 12;
                headerRange0.Style.Font.Bold = true;
                headerRange0.Style.Alignment.WrapText = true;
                headerRange0.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                headerRange0.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                headerRange0.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                headerRange0.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                var headerRange10 = worksheet.Range(13, 1, accounts.Count()+13, 8);
                headerRange10.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                headerRange10.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                headerRange10.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                headerRange10.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                int row = 13;
                foreach (var account in accounts)
                {
                    worksheet.Cell(row, 1).Value = account.accountNumber;
                    worksheet.Cell(row, 2).Value = account.accountName;
                    worksheet.Cell(row, 3).Value = account.beginningDebitBalance;
                    worksheet.Cell(row, 4).Value = account.beginningCreditBalance;
                    worksheet.Cell(row, 5).Value = account.debitBalance;
                    worksheet.Cell(row, 6).Value = account.creditBalance;
                    worksheet.Cell(row, 7).Value = account.endDebitBalance;
                    worksheet.Cell(row, 8).Value = account.endCreditBalance;
                    row++;
                }

                // Autofit columns
                worksheet.Columns().AdjustToContents();

                if ((rptSource == "empty"))
                {

                }
                else
                {
                    var footerRange = worksheet.Range(accounts.Count() + 13, 2, accounts.Count()+13, 8);
                    footerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                    footerRange.Style.Font.FontSize = 12;
                    footerRange.Style.Font.Bold = true;
                    footerRange.Style.Alignment.WrapText = false;
                    footerRange.Style.Alignment.JustifyLastLine= false;
                    footerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    footerRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    footerRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    footerRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    footerRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    // Print totals
                    worksheet.Cell(row, 2).Value = "Totals";
                    worksheet.Cell(row, 3).Value = trialBalance.totalBeginningDebitBalance.ToString();
                    worksheet.Cell(row, 4).Value = trialBalance.totalBeginningCreditBalance.ToString();
                    worksheet.Cell(row, 5).Value = trialBalance.totalDebitBalance.ToString();
                    worksheet.Cell(row, 6).Value = trialBalance.totalCreditBalance.ToString();
                    worksheet.Cell(row, 7).Value = trialBalance.totalEndDebitBalance.ToString();
                    worksheet.Cell(row, 8).Value = trialBalance.totalEndCreditBalance.ToString();
                }
              

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;

                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", _accountServices.GetBranchName() + "-" + trialBalance.branchName + "TB6C.xlsx");
                }
                //worksheet.Cell(row + 1, 1).Value = $"Ending Balance Sign: {trialBalance.EndingBalanceSigne}";
         
                // Save the workbook

            }

            //return new EmptyResult();
        }
        public class Export
        {
            public void ToExcel(HttpResponseBase response, IEnumerable<object> object_list, string strtitle)
            {
                try
                {
                    var grid = new System.Web.UI.WebControls.GridView();

                    // Ensure type safety
                    var nonNullClientsList = object_list?.Where(item => item != null) ?? Enumerable.Empty<object>();

                    grid.DataSource = nonNullClientsList;
                    grid.DataBind();

                    response.ClearContent();
                    response.AddHeader("content-disposition", $"attachment; filename={strtitle}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xls");
                    response.ContentType = "application/excel";

                    using (StringWriter sw = new StringWriter())
                    {
                        using (HtmlTextWriter htw = new HtmlTextWriter(sw))
                        {
                            grid.RenderControl(htw);
                            response.Write(sw.ToString());
                            response.End();
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception or handle it as needed
                    Console.WriteLine($"Error exporting to Excel: {ex.Message}");
                }
            }
        }

        public class LoanRepository
        {


            //private async Task<string> CreateLoanExportFile(IQueryable<Loan> query, string branch)
            //{
            //    try
            //    {
            //        var loans = await query.AsNoTracking()
            //                               .OrderBy(l => l.LoanDate)
            //                               .ToListAsync();



            //        // Create a new workbook
            //        var workbook = new XLWorkbook();

            //        // Add a worksheet
            //        var worksheet = workbook.Worksheets.Add("Loans");

            //        // Add title and branch name
            //        worksheet.Cell("A1").Value = $"Loan Situations as of {DateTime.Now.ToString("dd-MMM-yyyy, hh:mm:ss")}";
            //        worksheet.Cell("A2").Value = $"Branch: {branch}";

            //        // Merge, center, and bold the title and branch name, and set font size to 14 and text color to black
            //        var titleRange = worksheet.Range("A1:M1");
            //        titleRange.Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            //        titleRange.Style.Font.Bold = true;
            //        titleRange.Style.Font.FontSize = 14;
            //        titleRange.Style.Font.FontColor = XLColor.Black;
            //        // Apply border to title range
            //        titleRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            //        var branchRange = worksheet.Range("A2:M2");
            //        branchRange.Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            //        branchRange.Style.Font.Bold = true;
            //        branchRange.Style.Font.FontSize = 14;
            //        branchRange.Style.Font.FontColor = XLColor.Black;
            //        // Apply border to branch range
            //        branchRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            //        // Move headers below title and branch name
            //        int row = 4;

            //        // Add headers
            //        var headers = new string[]
            //        {
            //        "Acc. Number", "Name", "B.Code", "Loan Type", "L.Date", "Age", "D.Date",
            //        "I.Rate", "L.P Date", "Amount", "Paid", "Balance", "Duration"
            //        };

            //        // Set header row, bold headers, add light blue background, increase font size, and wrap text
            //        var headerRow = worksheet.Row(4);
            //        headerRow.Style.Font.Bold = true;
            //        headerRow.Style.Font.FontSize = 12;
            //        headerRow.Style.Alignment.WrapText = true;

            //        // Calculate end column
            //        int endColumn = headers.Length;

            //        // Apply background color, font size, and wrap text to the header range
            //        var headerRange = worksheet.Range(4, 1, 4, endColumn);
            //        headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
            //        headerRange.Style.Font.FontSize = 12;
            //        headerRange.Style.Alignment.WrapText = true;

            //        // Populate header values
            //        for (int i = 0; i < headers.Length; i++)
            //        {
            //            worksheet.Cell(4, i + 1).Value = headers[i];
            //        }

            //        // Add borders to headers
            //        for (int i = 0; i < headers.Length; i++)
            //        {
            //            worksheet.Cell(4, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            //        }

            //        // Increment row for the data
            //        row++;

            //        // Group loans by LoanType
            //        var groupedLoans = loans.GroupBy(l => l.LoanType);

            //        foreach (var loanTypeGroup in groupedLoans)
            //        {
            //            // Group loans within each LoanType group by duration
            //            var durations = loanTypeGroup.GroupBy(l => CalculateDurationGroup(l.DisbursementDate));

            //            foreach (var durationGroup in durations)
            //            {
            //                // Add loan type group header
            //                worksheet.Cell(row, 1).Value = loanTypeGroup.Key;
            //                worksheet.Range(row, 1, row, headers.Length).Merge();
            //                worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            //                worksheet.Range(row, 1, row, headers.Length).Style.Font.Bold = true; // Bold the title
            //                row++;

            //                // Add duration group header
            //                worksheet.Cell(row, 1).Value = durationGroup.Key;
            //                worksheet.Range(row, 1, row, headers.Length).Merge();
            //                worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            //                worksheet.Range(row, 1, row, headers.Length).Style.Font.Bold = true; // Bold the title
            //                row++;

            //                decimal totalLoanAmount = 0;
            //                decimal totalPaid = 0;
            //                decimal totalBalance = 0;

            //                foreach (var loan in durationGroup)
            //                {
            //                    var loanDuration = (DateTime.Now - loan.DisbursementDate).Days;

            //                    // Add loan details
            //                    worksheet.Cell(row, 1).Value = loan.CustomerId;
            //                    worksheet.Cell(row, 2).Value = loan.CustomerName;
            //                    worksheet.Cell(row, 3).Value = loan.BranchCode;
            //                    worksheet.Cell(row, 4).Value = loan.LoanType;
            //                    worksheet.Cell(row, 5).Value = loan.LoanDate.ToString("yyyy-MM-dd");
            //                    worksheet.Cell(row, 6).Value = loanDuration;
            //                    worksheet.Cell(row, 7).Value = loan.MaturityDate.ToString("yyyy-MM-dd");
            //                    worksheet.Cell(row, 8).Value = loan.InterestRate;
            //                    worksheet.Cell(row, 9).Value = loan.LastRefundDate.ToString("yyyy-MM-dd");
            //                    worksheet.Cell(row, 10).Value = loan.LoanAmount;
            //                    worksheet.Cell(row, 11).Value = loan.Paid;
            //                    worksheet.Cell(row, 12).Value = loan.Balance;
            //                    worksheet.Cell(row, 13).Value = loan.LoanDurarion;

            //                    // Apply borders to data cells
            //                    for (int i = 1; i <= headers.Length; i++)
            //                    {
            //                        worksheet.Cell(row, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            //                    }

            //                    // Calculate totals for this duration group
            //                    totalLoanAmount += loan.LoanAmount;
            //                    totalPaid += loan.Paid;
            //                    totalBalance += loan.Balance;

            //                    row++;
            //                }

            //                // Autofit column width for all columns
            //                worksheet.Columns().AdjustToContents();

            //                // Add totals row for the duration group
            //                worksheet.Cell(row, 9).Value = "TOTAL";
            //                worksheet.Cell(row, 10).Value = totalLoanAmount;
            //                worksheet.Cell(row, 11).Value = totalPaid;
            //                worksheet.Cell(row, 12).Value = totalBalance;
            //                worksheet.Range(row, 1, row, headers.Length).Style.Font.SetBold();

            //                // Apply borders to total row
            //                for (int i = 1; i <= headers.Length; i++)
            //                {
            //                    worksheet.Cell(row, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            //                }

            //                row++;
            //            }
            //        }

            //        // Footer calculations
            //        int totalLoans = loans.Count;
            //        decimal totalLoanVolume = loans.Sum(l => l.LoanAmount);
            //        decimal totalRefundVolume = loans.Sum(l => l.Paid);
            //        decimal percentageRefund = Math.Round((totalRefundVolume / totalLoanVolume) * 100, 1);
            //        decimal totalOutstanding = loans.Sum(l => l.Balance);

            //        // Add footer summary
            //        worksheet.Cell(row, 1).Value = "SUMMARY";
            //        worksheet.Range(row, 1, row, 2).Merge();
            //        worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            //        worksheet.Range(row, 1, row, 2).Style.Font.Bold = true;
            //        worksheet.Range(row, 1, row, 2).Style.Font.FontSize = 13;
            //        worksheet.Range(row, 1, row, 2).Style.Fill.BackgroundColor = XLColor.LightBlue;
            //        worksheet.Range(row, 1, row, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            //        row++;

            //        worksheet.Cell(row, 1).Value = "1. Number of Loans";
            //        worksheet.Cell(row, 2).Value = totalLoans;
            //        worksheet.Cell(row, 1).Style.Border.RightBorder = XLBorderStyleValues.Thin;
            //        worksheet.Cell(row, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            //        row++;

            //        worksheet.Cell(row, 1).Value = "2. Volume of Loan";
            //        worksheet.Cell(row, 2).Value = totalLoanVolume;
            //        worksheet.Cell(row, 1).Style.Border.RightBorder = XLBorderStyleValues.Thin;
            //        worksheet.Cell(row, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            //        worksheet.Cell(row, 2).Style.NumberFormat.Format = "[$XAF] #,##0.0";
            //        row++;

            //        worksheet.Cell(row, 1).Value = "3. Volume of Refund";
            //        worksheet.Cell(row, 2).Value = totalRefundVolume;
            //        worksheet.Cell(row, 1).Style.Border.RightBorder = XLBorderStyleValues.Thin;
            //        worksheet.Cell(row, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            //        worksheet.Cell(row, 2).Style.NumberFormat.Format = "[$XAF] #,##0.0";
            //        row++;

            //        worksheet.Cell(row, 1).Value = "4. Percentage of Refund";
            //        worksheet.Cell(row, 2).Value = percentageRefund;
            //        worksheet.Cell(row, 1).Style.Border.RightBorder = XLBorderStyleValues.Thin;
            //        worksheet.Cell(row, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            //        worksheet.Cell(row, 2).Style.NumberFormat.Format = "0.0"; // Percentage format
            //        row++;

            //        worksheet.Cell(row, 1).Value = "5. Outstanding Loan";
            //        worksheet.Cell(row, 2).Value = totalOutstanding;
            //        worksheet.Cell(row, 1).Style.Border.RightBorder = XLBorderStyleValues.Thin;
            //        worksheet.Cell(row, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            //        worksheet.Cell(row, 2).Style.NumberFormat.Format = "[$XAF] #,##0.0";
            //        row++;

            //        // Add border to the footer
            //        worksheet.Range(row - 6, 1, row, 2).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            //        worksheet.Range(row - 6, 1, row, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;



            //        // Autofit column width for all columns
            //        worksheet.Columns().AdjustToContents();

            //        // Save the workbook to the file
            //        workbook.SaveAs(filePath);

            //        // Return the download path
            //        var downloadPath = $"/exports/{fileName}";
            //        return downloadPath;
            //    }
            //    catch (Exception ex)
            //    {
            //        throw;
            //    }
            //}

            // Helper method to calculate duration group based on disbursement date

        }

    }



}