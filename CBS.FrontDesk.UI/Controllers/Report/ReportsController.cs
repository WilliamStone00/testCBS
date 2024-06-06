
using CBS.FrontDesk.Data.Entity.Accounting;
using ClosedXML.Excel;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

namespace CBS.FrontDesk.UI.Controllers
{
   //[CheckSessionTimeOutAttribute]

    public class ReportsController : Controller
    {

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
             var rpt = trialBalance.BranchName + " General Ledger as of the " + trialBalance.FromDate;
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add(rpttitle);
                var headerStyle = workbook.Style;
                headerStyle.Font.Bold = true;

                // Print letterhead
                worksheet.Cell(1, 2).Value = trialBalance.BranchName;
                worksheet.Cell(2, 2).Value = $"{trialBalance.BranchLocation}, {trialBalance.Address}";
                worksheet.Cell(3, 2).Value = $" {trialBalance.Capital}";
                worksheet.Cell(4, 2).Value = $"{trialBalance.ImmatriculationNumber}";
                worksheet.Cell(5, 2).Value = $"{trialBalance.WebSite}";

                worksheet.Cell(1, 1).Value = "BranchName";
                worksheet.Cell(2, 1).Value = $"Address";
                worksheet.Cell(3, 1).Value = $"Capital";
                worksheet.Cell(4, 1).Value = $"Immatriculation Number";
                worksheet.Cell(5, 1).Value = $"Website";

                // Apply header style
                worksheet.Range(1, 1, 5, 1).Style = headerStyle;

                // Add title in bold with font-18
                var titleStyle = workbook.Style;
                titleStyle.Font.Bold = true;
                titleStyle.Font.FontSize = 16;
             
                worksheet.Cell(7, 1).Value = rpttitle;
                worksheet.Cell(7, 1).Style = titleStyle;

                // Print balance sheet header
                worksheet.Cell(9, 1).Value = "Account Number";
                worksheet.Cell(9, 2).Value = "Account Name";
                worksheet.Cell(9, 3).Value = "Current Balance";

                // Apply header style
                worksheet.Range(9, 1, 7, 6).Style = headerStyle;

                // Print account details
                int row = 11;
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
                    // Print totals
                    //worksheet.Cell(row, 2).Value = "Totals";
                    //worksheet.Cell(row, 3).Value = trialBalance.totalBeginningBalance.ToString();
                    //worksheet.Cell(row, 4).Value = trialBalance.totalDebitBalance.ToString();
                    //worksheet.Cell(row, 5).Value = trialBalance.totalCreditBalance.ToString();
                    //worksheet.Cell(row, 6).Value = trialBalance.totalEndingBalance.ToString();
                }

                //worksheet.Cell(row + 1, 1).Value = $"Ending Balance Sign: {trialBalance.EndingBalanceSigne}";

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{rpttitle}.xlsx");
                }
            }

            //return new EmptyResult();
        }

        public ActionResult PrintJournalEntryDtoInExcel()
        {
            //new Dto();
            //List<TrialBalance4ColumnDto> accounts = new List<TrialBalance4ColumnDto>();

            var rptSource = System.Web.HttpContext.Current.Session["rptSource"];
            var paths = System.Web.HttpContext.Current.Session["rptpath"].ToString();
            var rpttitle = System.Web.HttpContext.Current.Session["rpttitle"].ToString();

            List<JournalEntryDto> accounts = (rptSource == "empty") ? new List<JournalEntryDto>() : (List<JournalEntryDto>)rptSource;
            JournalEntryDto trialBalance = (rptSource == "empty") ? new JournalEntryDto() : accounts[0];
            var rpt = trialBalance.BranchName + " General Ledger as of the " + trialBalance.FromDate;
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add(rpttitle);
                var headerStyle = workbook.Style;
                headerStyle.Font.Bold = true;

                // Print letterhead
                worksheet.Cell(1, 2).Value = trialBalance.BranchName;
                worksheet.Cell(2, 2).Value = $"{trialBalance.BranchLocation}, {trialBalance.Address}";
                worksheet.Cell(3, 2).Value = $" {trialBalance.Capital}";
                worksheet.Cell(4, 2).Value = $"{trialBalance.ImmatriculationNumber}";
                worksheet.Cell(5, 2).Value = $"{trialBalance.WebSite}";

                worksheet.Cell(1, 1).Value = "BranchName";
                worksheet.Cell(2, 1).Value = $"Address";
                worksheet.Cell(3, 1).Value = $"Capital";
                worksheet.Cell(4, 1).Value = $"Immatriculation Number";
                worksheet.Cell(5, 1).Value = $"Website";

                // Apply header style
                worksheet.Range(1, 1, 5, 1).Style = headerStyle;

                // Add title in bold with font-18
                var titleStyle = workbook.Style;
                titleStyle.Font.Bold = true;
                titleStyle.Font.FontSize = 16;

                worksheet.Cell(7, 1).Value = rpttitle;
                worksheet.Cell(7, 1).Style = titleStyle;

                // Print balance sheet header
                worksheet.Cell(9, 1).Value = "Entry Date";
                worksheet.Cell(9, 2).Value = "Account Name";
                worksheet.Cell(9, 3).Value = "Account Number";
                worksheet.Cell(9, 4).Value = "Description";
                worksheet.Cell(9, 5).Value = "DebitAmount";
                worksheet.Cell(9, 6).Value = "CreditAmount";
                // Apply header style
                worksheet.Range(9, 1, 7, 6).Style = headerStyle;

                // Print account details
                int row = 11;
                foreach (var account in accounts)
                {
                    worksheet.Cell(row, 1).Value = account.EntryDatetime;
                    worksheet.Cell(row, 2).Value = account.Reference;
                    worksheet.Cell(row, 3).Value = account.AccountNumber;
                    worksheet.Cell(row, 4).Value = account.Description;
                    worksheet.Cell(row, 5).Value = account.DebitAmount;
                    worksheet.Cell(row, 6).Value = account.CreditAmount;
                    row++;
                }

                worksheet.Columns().AdjustToContents();

                if ((rptSource == "empty"))
                {
                }
                else
                {
                    // Print totals
                    //worksheet.Cell(row, 2).Value = "Totals";
                    //worksheet.Cell(row, 3).Value = trialBalance.totalBeginningBalance.ToString();
                    //worksheet.Cell(row, 4).Value = trialBalance.totalDebitBalance.ToString();
                    //worksheet.Cell(row, 5).Value = trialBalance.totalCreditBalance.ToString();
                    //worksheet.Cell(row, 6).Value = trialBalance.totalEndingBalance.ToString();
                }
        //                public string AccountNumber { get; set; }
        //public string AccountName { get; set; }
        //public string DebitAmount { get; set; }
        //public string CreditAmount { get; set; }
        //public string Description { get; set; }
        //public string Reference { get; set; }
        //public string EntryDatetime { get; set; }
                //worksheet.Cell(row + 1, 1).Value = $"Ending Balance Sign: {trialBalance.EndingBalanceSigne}";

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{rpttitle}.xlsx");
                }
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
                worksheet.Range(1, 1, 7, 1).Style = headerStyle;
                // Print account details

                // Print balance sheet header
                worksheet.Cell(9, 1).Value = "Account Number";
                worksheet.Cell(9, 2).Value = "Account Name";
                worksheet.Cell(9, 3).Value = "Beginning Balance";
                worksheet.Cell(9, 4).Value = "Debit Balance";
                worksheet.Cell(9, 5).Value = "Credit Balance";
                worksheet.Cell(9, 6).Value = "Ending Balance";
                // Apply header style
                worksheet.Range(9, 1, 9, 6).Style = headerStyle;
                // Print account details
                int row = 10;
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
                    // Print totals
                    worksheet.Cell(row, 2).Value = "Totals";
                    worksheet.Cell(row, 3).Value = trialBalance.totalBeginningBalance.ToString();
                    worksheet.Cell(row, 4).Value = trialBalance.totalDebitBalance.ToString();
                    worksheet.Cell(row, 5).Value = trialBalance.totalCreditBalance.ToString();
                    worksheet.Cell(row, 6).Value = trialBalance.totalEndingBalance.ToString();
                }

     
                //worksheet.Cell(row + 1, 1).Value = $"Ending Balance Sign: {trialBalance.EndingBalanceSigne}";
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;

                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{rpttitle}.xlsx");
                }

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
                worksheet.Range(1, 1, 7, 1).Style = headerStyle;
                // Print account details

                // Print balance sheet header
                worksheet.Cell(9, 1).Value = "Account Number";
                worksheet.Cell(9, 2).Value = "Account Name";
                worksheet.Cell(9, 3).Value = "Beginning Debit Balance";
                worksheet.Cell(9, 4).Value = "Beginning Credit Balance";
                worksheet.Cell(9, 5).Value = "Debit Balance";
                worksheet.Cell(9, 6).Value = "Credit Balance";
                worksheet.Cell(9, 7).Value = "Ending Debit Balance";
                worksheet.Cell(9, 8).Value = "Ending Credit Balance";

                // Apply header style
                worksheet.Range(9, 1, 9, 8).Style = headerStyle;  // Print account details
                int row = 10;
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

                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "BalanceSheet.xlsx");
                }
                //worksheet.Cell(row + 1, 1).Value = $"Ending Balance Sign: {trialBalance.EndingBalanceSigne}";
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;

                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{rpttitle}.xlsx");
                }

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


    }
}