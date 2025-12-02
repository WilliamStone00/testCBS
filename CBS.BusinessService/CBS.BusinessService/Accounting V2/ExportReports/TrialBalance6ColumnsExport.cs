using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounting_V2.ExportReports
{


    public class TrialBalance6ColumnsExport
    {
        public string SafeGet(dynamic obj, string propertyName)
        {
            try
            {
                // Check if property exists using reflection
                var prop = obj.GetType().GetProperty(propertyName);
                if (prop == null)
                    return ""; // property does not exist

                var value = prop.GetValue(obj, null);
                return value?.ToString() ?? ""; // property exists but null
            }
            catch
            {
                return ""; // safe fallback — never throw
            }
        }


        //public int WriteExcelHeader(IXLWorksheet ws, dynamic firstRow, string exportedBy, int startRow)
        //{
        //    string BranchName = SafeGet(firstRow, "BranchName");
        //    string BranchCode = SafeGet(firstRow, "BranchCode");
        //    string BankName = SafeGet(firstRow, "BankName");
        //    string Status = SafeGet(firstRow, "Mode");
        //    string OperationCode = SafeGet(firstRow, "OperationCode");
        //    string BranchTelephone = SafeGet(firstRow, "BranchPhone");
        //    string BranchEmail =SafeGet(firstRow, "BranchEmail");
        //    string BranchAddress = SafeGet(firstRow, "BranchAddress");
        //    string PrintedBy = SafeGet(firstRow, "Username");
        //    string Period = $"FROM {SafeGet(firstRow, "From")} TO {SafeGet(firstRow, "To")}";
        //    string ReportName = "TRIAL BALANCE 6 COLUMNS";
        //    string exportedInfoDN = $"Exported ON: {DateTime.Now:dd/MM/yyyy HH:mm}   |   By: {PrintedBy}";



        //    int row = startRow;

        //    // ───────────── BANK NAME (FIRST LINE) — Blue background
        //    ws.Cell(row, 1).Value = BankName;
        //    var line1 = ws.Range(row, 1, row, 6);
        //    line1.Merge();
        //    line1.Style.Font.SetBold().Font.SetFontSize(18);
        //    line1.Style.Font.FontColor = XLColor.White;
        //    line1.Style.Fill.BackgroundColor = XLColor.FromArgb(30, 60, 180);
        //    row++;



        //    // ───────────── Branch Name — Light Blue background + border
        //    ws.Cell(row, 1).Value = $"Branch Name: {BranchName}";
        //    var line3 = ws.Range(row, 1, row, 6);
        //    line3.Merge();
        //    line3.Style.Font.SetFontSize(13).Font.SetBold();
        //    line3.Style.Fill.BackgroundColor = XLColor.FromArgb(210, 230, 255);
        //    line3.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
        //    row++;

        //    // ───────────── Period — Yellow background + italic
        //    ws.Cell(row, 1).Value = Period;
        //    var line4 = ws.Range(row, 1, row, 6);
        //    line4.Merge();
        //    line4.Style.Font.SetFontSize(12);
        //    line4.Style.Fill.BackgroundColor = XLColor.LightYellow;
        //    row++;



        //    row += 2;

        //    return row; // return where the table section should start
        //}

        //public int WriteExcelHeader(IXLWorksheet ws, dynamic firstRow, string exportedBy, int startRow)
        //{
        //    string BranchName = SafeGet(firstRow, "BranchName");
        //    string BranchAddress = SafeGet(firstRow, "BranchAddress");
        //    string BranchTelephone = SafeGet(firstRow, "BranchPhone");
        //    string Bp = SafeGet(firstRow, "BranchCode");
        //    string PrintedBy = SafeGet(firstRow, "Username");

        //    string BankName = SafeGet(firstRow, "BankName");

        //    string PeriodFrom = SafeGet(firstRow, "From");
        //    string PeriodTo = SafeGet(firstRow, "To");
        //    string PrintDate = DateTime.Now.ToString("dd/MM/yyyy hh:mm tt");

        //    string ReportTitle = "TRIAL BALANCE 6 COLUMNS";
        //    string SubTitle = "( TEMPORAL REPORT )";
        //    string PeriodLabel = $"Period     {PeriodFrom}     To     {PeriodTo}";

        //    int row = startRow;

        //    // ───────────────────────────── LEFT BANK HEADER ─────────────────────────────
        //    ws.Cell(row, 1).Value = BankName;
        //    ws.Cell(row, 1).Style.Font.SetBold().Font.SetFontSize(15);
        //    row++;

        //    ws.Cell(row, 1).Value = BranchName;
        //    row++;

        //    ws.Cell(row, 1).Value = BranchAddress;
        //    row++;

        //    ws.Cell(row, 1).Value = $"BP.: {Bp}        {BranchTelephone}";
        //    row++;

        //    ws.Cell(row, 1).Value = $"Print Date        {PrintDate}";
        //    row++;

        //    // ───────────────────────────── RIGHT SIDE NAME ─────────────────────────────
        //    ws.Cell(startRow + 4, 8).Value = PrintedBy;   // same row as Print Date
        //    var right = ws.Range(startRow + 4, 8, startRow + 4, 8);
        //    right.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

        //    row++;
        //    row++;

        //    // ───────────────────────────── MAIN TITLE ─────────────────────────────
        //    ws.Cell(row, 1).Value = ReportTitle;
        //    var title = ws.Range(row, 1, row, 8);
        //    title.Merge();
        //    title.Style.Font.SetBold().Font.SetFontSize(15);
        //    title.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        //    row++;

        //    // ───────────────────────────── SUBTITLE ─────────────────────────────
        //    ws.Cell(row, 1).Value = SubTitle;
        //    var subtitle = ws.Range(row, 1, row, 8);
        //    subtitle.Merge();
        //    subtitle.Style.Font.SetBold().Font.SetFontSize(12);
        //    subtitle.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        //    row++;

        //    // ────────── HORIZONTAL LINE ──────────
        //    var separator = ws.Range(row, 1, row, 8);
        //    separator.Merge();
        //    separator.Style.Border.BottomBorder = XLBorderStyleValues.Thick;
        //    row++;

        //    // ───────────────────────────── PERIOD LINE ─────────────────────────────
        //    ws.Cell(row, 1).Value = PeriodLabel;
        //    var period = ws.Range(row, 1, row, 8);
        //    period.Merge();
        //    period.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        //    period.Style.Font.SetFontSize(11);
        //    row++;

        //    row += 2;
        //    return row;
        //}

        public int WriteExcelHeader(IXLWorksheet ws, dynamic firstRow, string exportedBy, int startRow)
        {
            string BankName = SafeGet(firstRow, "BankName");
            string BranchName = SafeGet(firstRow, "BranchName");
            string BranchAddress = SafeGet(firstRow, "BranchAddress");
            string BranchTelephone = SafeGet(firstRow, "BranchPhone");
            string Bp = SafeGet(firstRow, "BranchCode");
            string PrintedBy = SafeGet(firstRow, "Username");

            string PeriodFrom = SafeGet(firstRow, "From");
            string PeriodTo = SafeGet(firstRow, "To");
            string PrintDate = DateTime.Now.ToString("dd/MM/yyyy hh:mm tt");

            string ReportTitle = "TRIAL BALANCE 6 COLUMNS";
            string SubTitle = "( TEMPORAL REPORT )";
            string PeriodString = $"Period      {PeriodFrom}     To     {PeriodTo}";

            int row = startRow;

            // ───────────────────────── LEFT SIDE HEADER BLOCK ─────────────────────────
            // ───────────── BANK NAME (FIRST LINE) — Blue background
            ws.Cell(row, 1).Value = BankName;

            // Merge from column 1 to column 6 on the same row
            var line1 = ws.Range(row, 1, row, 6);
            line1.Merge();

            // Apply styling
            line1.Style.Font.SetBold().Font.SetFontSize(18);
            line1.Style.Font.FontColor = XLColor.White;
            line1.Style.Fill.BackgroundColor = XLColor.FromArgb(30, 60, 180);
            line1.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            line1.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;


            row++;

            // ───────────── Branch Name — Light Blue background + border
            ws.Cell(row, 1).Value = $"Branch Name: {BranchName}";
            var line3 = ws.Range(row, 1, row, 6);
            line3.Merge();
            line3.Style.Font.SetFontSize(13).Font.SetBold();
            line3.Style.Font.FontColor = XLColor.Black;
            line3.Style.Fill.BackgroundColor = XLColor.FromArgb(210, 230, 255);
            line3.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
            line3.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            line3.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            row++;

            ws.Cell(row, 1).Value = BranchAddress;
            row++;

            ws.Cell(row, 1).Value = $"BP.: {Bp}       {BranchTelephone}";
            row++;

            ws.Cell(row, 1).Value = $"Print Date      {PrintDate}";

            // ───────────────────────── RIGHT SIDE NAME ─────────────────────────
            ws.Cell(row, 9).Value = PrintedBy;
            ws.Cell(row, 9).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
            row += 2; // spacing

            // ───────────────────────── REPORT TITLE ─────────────────────────
            ws.Cell(row, 1).Value = ReportTitle;
            var titleRange = ws.Range(row, 1, row, 9);
            titleRange.Merge();
            titleRange.Style.Font.SetBold().Font.SetFontSize(15);
            titleRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            row++;

            // ───────────────────────── SUBTITLE ─────────────────────────
            ws.Cell(row, 1).Value = SubTitle;
            var subRange = ws.Range(row, 1, row, 9);
            subRange.Merge();
            subRange.Style.Font.SetBold().Font.SetFontSize(12);
            subRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            row++;

            // ───────────────────────── HORIZONTAL LINE SEPARATOR ─────────────────────────
            var sep = ws.Range(row, 1, row, 9);
            sep.Merge();
            sep.Style.Border.BottomBorder = XLBorderStyleValues.Thick;
            row++;

            // ───────────────────────── PERIOD LINE ─────────────────────────
            ws.Cell(row, 1).Value = PeriodString;
            var pRange = ws.Range(row, 1, row, 9);
            pRange.Merge();
            pRange.Style.Font.SetFontSize(11)
                  .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            row++;

            row += 2;
            return row;  // table starts here
        }


        public void ExportTb6(dynamic tb, string filePath, string exportedBy)
        {
            var data = tb;

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Trial Balance");

                // 1️⃣ HEADER (BANK / BRANCH / PERIOD etc.)
                int row = WriteExcelHeader(ws, data[0], exportedBy, 1);

                // 2️⃣ TABLE MAIN HEADERS (ROW row)
                ws.Cell(row, 1).Value = "SN";
                ws.Cell(row, 2).Value = "ACCOUNT NO";
                ws.Cell(row, 3).Value = "ACCOUNT NAME";
                ws.Cell(row, 4).Value = "OPENING BALANCE";
                ws.Cell(row, 6).Value = "MOVEMENTS";
                ws.Cell(row, 8).Value = "CLOSING BALANCE";

                // Merge main header cells
                ws.Range(row, 1, row + 1, 1).Merge(); // SN
                ws.Range(row, 2, row + 1, 2).Merge(); // ACCOUNT NO
                ws.Range(row, 3, row + 1, 3).Merge(); // ACCOUNT NAME
                ws.Range(row, 4, row, 5).Merge();     // OPENING BALANCE (4–5)
                ws.Range(row, 6, row, 7).Merge();     // MOVEMENTS (6–7)
                ws.Range(row, 8, row, 9).Merge();     // CLOSING BALANCE (8–9) ✅ FIX

                // 2️⃣b SUBHEADERS (ROW row + 1)
                int subRow = row + 1;
                ws.Cell(subRow, 4).Value = "DEBIT";
                ws.Cell(subRow, 5).Value = "CREDIT";
                ws.Cell(subRow, 6).Value = "DEBIT";
                ws.Cell(subRow, 7).Value = "CREDIT";
                ws.Cell(subRow, 8).Value = "DEBIT";
                ws.Cell(subRow, 9).Value = "CREDIT";

                // Style header area (2 rows high, 9 columns wide) – light gray
                var headerRange = ws.Range(row, 1, subRow, 9);
                headerRange.Style.Font.SetBold();
                headerRange.Style.Fill.SetBackgroundColor(XLColor.LightGray);
                headerRange.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                headerRange.Style.Border.SetInsideBorder(XLBorderStyleValues.Thin);
                headerRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                headerRange.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);

                row = subRow + 1; // first data row

                // 3️⃣ DATA ROWS (all detail lines)
                int sn = 1;
                foreach (var x in data)
                {
                    ws.Cell(row, 1).Value = sn++;
                    ws.Cell(row, 2).Value = x.AccountNumber;
                    ws.Cell(row, 3).Value = x.AccountName;
                    ws.Cell(row, 4).Value = x.OpeningDebit;
                    ws.Cell(row, 5).Value = x.OpeningCredit;
                    ws.Cell(row, 6).Value = x.MovementDebit;
                    ws.Cell(row, 7).Value = x.MovementCredit;
                    ws.Cell(row, 8).Value = x.ClosingDebit;
                    ws.Cell(row, 9).Value = x.ClosingCredit;

                    // Number formatting for amount columns
                    ws.Range(row, 4, row, 9).Style.NumberFormat.Format = "#,##0";

                    // Borders
                    ws.Range(row, 1, row, 9).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);

                    // Alternating row background
                    if (row % 2 == 0)
                        ws.Range(row, 1, row, 9).Style.Fill.BackgroundColor = XLColor.FromArgb(245, 245, 245);

                    row++;
                }

                // 4️⃣ TOTALS ROW (using totals from data[0])
                var totals = data[0];

                ws.Cell(row, 1).Value = "";
                ws.Cell(row, 2).Value = "TOTAL";
                ws.Cell(row, 3).Value = "";

                ws.Cell(row, 4).Value = totals.TotalOpeningDebit;
                ws.Cell(row, 5).Value = totals.TotalOpeningCredit;
                ws.Cell(row, 6).Value = totals.TotalMovementDebit;
                ws.Cell(row, 7).Value = totals.TotalMovementCredit;
                ws.Cell(row, 8).Value = totals.TotalClosingDebit;
                ws.Cell(row, 9).Value = totals.TotalClosingCredit;

                ws.Range(row, 4, row, 9).Style.NumberFormat.Format = "#,##0";

                var totalsRange = ws.Range(row, 1, row, 9);
                totalsRange.Style.Font.SetBold();
                totalsRange.Style.Fill.SetBackgroundColor(XLColor.FromArgb(230, 230, 230));
                totalsRange.Style.Border.SetOutsideBorder(XLBorderStyleValues.Medium);
                totalsRange.Style.Border.SetInsideBorder(XLBorderStyleValues.Thin);

                row++;

                // 5️⃣ AUTO-SIZE COLUMNS
                ws.Columns().AdjustToContents();
                foreach (var column in ws.ColumnsUsed())
                {
                    if (column.Width < 12) column.Width = 12;
                    if (column.Width > 45) column.Width = 45;
                }

                workbook.SaveAs(filePath);
            }
        }


        //public void ExportTb6(dynamic tb, string filePath, string exportedBy)
        //{
        //    var data = tb;

        //    using (var workbook = new XLWorkbook())
        //    {
        //        var ws = workbook.Worksheets.Add("Trial Balance");

        //        // 1️⃣ HEADER BLOCK
        //        int row = WriteExcelHeader(ws, data[0], exportedBy, 1);

        //        // 2️⃣ TABLE MAIN HEADERS
        //        ws.Cell(row, 1).Value = "SN";
        //        ws.Cell(row, 2).Value = "ACCOUNT NO";
        //        ws.Cell(row, 3).Value = "ACCOUNT NAME";
        //        ws.Cell(row, 4).Value = "OPENING BALANCE";
        //        ws.Cell(row, 6).Value = "MOVEMENTS";
        //        ws.Cell(row, 8).Value = "CLOSING BALANCE";

        //        // Merge major titles
        //        ws.Range(row, 1, row + 1, 1).Merge(); // SN
        //        ws.Range(row, 2, row + 1, 2).Merge(); // Account No
        //        ws.Range(row, 3, row + 1, 3).Merge(); // Account Name
        //        ws.Range(row, 4, row, 5).Merge();     // Opening (DEBIT/CREDIT)
        //        ws.Range(row, 6, row, 7).Merge();     // Movements (DEBIT/CREDIT)
        //                                              // Column 8 is already one column wide — no merge required

        //        // Subheaders row
        //        int subRow = row + 1;
        //        ws.Cell(subRow, 4).Value = "DEBIT";
        //        ws.Cell(subRow, 5).Value = "CREDIT";
        //        ws.Cell(subRow, 6).Value = "DEBIT";
        //        ws.Cell(subRow, 7).Value = "CREDIT";
        //        ws.Cell(subRow, 8).Value = "DEBIT";
        //        ws.Cell(subRow, 9).Value = "CREDIT"; // REMOVE → no column 9
        //        ws.Cell(subRow, 8).Value = "CREDIT"; // FIXED CREDIT UNDER COLUMN 8

        //        // Style full header area
        //        var headerRange = ws.Range(row, 1, subRow, 8);
        //        headerRange.Style.Font.SetBold();
        //        headerRange.Style.Fill.SetBackgroundColor(XLColor.LightGray);
        //        headerRange.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
        //        headerRange.Style.Border.SetInsideBorder(XLBorderStyleValues.Thin);
        //        headerRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        //        headerRange.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);

        //        row = subRow + 1;

        //        // 3️⃣ DATA ROWS — print all except totals
        //        int sn = 1;
        //        foreach (var x in data) // skip totals
        //        {
        //            ws.Cell(row, 1).Value = sn++;
        //            ws.Cell(row, 2).Value = x.AccountNumber;
        //            ws.Cell(row, 3).Value = x.AccountName;
        //            ws.Cell(row, 4).Value = x.OpeningDebit;
        //            ws.Cell(row, 5).Value = x.OpeningCredit;
        //            ws.Cell(row, 6).Value = x.MovementDebit;
        //            ws.Cell(row, 7).Value = x.MovementCredit;
        //            ws.Cell(row, 8).Value = x.ClosingDebit;
        //            ws.Cell(row, 9).Value = x.ClosingCredit; // REMOVE → no column 9
        //            ws.Cell(row, 8).Value = x.ClosingCredit; // FIXED

        //            ws.Range(row, 4, row, 8).Style.NumberFormat.Format = "#,##0";
        //            ws.Range(row, 1, row, 8).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);

        //            if ((row % 2) == 0)
        //                ws.Range(row, 1, row, 8).Style.Fill.BackgroundColor = XLColor.FromArgb(245, 245, 245);

        //            row++;
        //        }

        //        // 4️⃣ TOTALS ROW — last item
        //        var totals = data[0];
        //        ws.Cell(row, 1).Value = "";
        //        ws.Cell(row, 2).Value = "TOTAL";
        //        ws.Cell(row, 3).Value = "";
        //        ws.Cell(row, 4).Value = totals.TotalOpeningDebit;
        //        ws.Cell(row, 5).Value = totals.TotalOpeningCredit;
        //        ws.Cell(row, 6).Value = totals.TotalMovementDebit;
        //        ws.Cell(row, 7).Value = totals.TotalMovementCredit;
        //        ws.Cell(row, 8).Value = totals.TotalClosingDebit;
        //        ws.Cell(row, 9).Value = totals.TotalClosingCredit; // REMOVE
        //        ws.Cell(row, 8).Value = totals.TotalClosingCredit; // FIXED

        //        ws.Range(row, 4, row, 8).Style.NumberFormat.Format = "#,##0";

        //        var totalsRange = ws.Range(row, 1, row, 8);
        //        totalsRange.Style.Font.SetBold();
        //        totalsRange.Style.Fill.SetBackgroundColor(XLColor.FromArgb(230, 230, 230));
        //        totalsRange.Style.Border.SetOutsideBorder(XLBorderStyleValues.Medium);
        //        totalsRange.Style.Border.SetInsideBorder(XLBorderStyleValues.Thin);

        //        row++;

        //        // 5️⃣ FORMAT COLUMNS
        //        ws.Columns().AdjustToContents();
        //        foreach (var column in ws.ColumnsUsed())
        //        {
        //            if (column.Width < 12) column.Width = 12;
        //            if (column.Width > 45) column.Width = 45;
        //        }

        //        workbook.SaveAs(filePath);
        //    }
        //}


        //public void ExportTb6(dynamic tb, string filePath, string exportedBy)
        //{
        //    var data = tb;

        //    using (var workbook = new XLWorkbook())
        //    {
        //        var ws = workbook.Worksheets.Add("Trial Balance");

        //        // 1️⃣ WRITE HEADER (separated)
        //        int row = WriteExcelHeader(ws, data[0], exportedBy, 1);

        //        // 2️⃣ TABLE HEADERS
        //        ws.Cell(row, 1).Value = "SN";
        //        ws.Cell(row, 2).Value = "ACCOUNT NO";
        //        ws.Cell(row, 3).Value = "ACCOUNT NAME";
        //        ws.Cell(row, 4).Value = "OPENING BALANCE";
        //        ws.Cell(row, 6).Value = "MOVEMENTS";
        //        ws.Cell(row, 8).Value = "CLOSING BALANCE";

        //        ws.Range(row, 1, row + 1, 1).Merge(); // SN
        //        ws.Range(row, 2, row + 1, 2).Merge(); // Acc No
        //        ws.Range(row, 3, row + 1, 3).Merge(); // Acc Name
        //        ws.Range(row, 4, row, 5).Merge();     // Opening Balance
        //        ws.Range(row, 6, row, 7).Merge();     // Movements
        //        ws.Range(row, 8, row, 9).Merge();     // Closing Balance

        //        // Subheaders
        //        int subRow = row + 1;
        //        ws.Cell(subRow, 4).Value = "DEBIT";
        //        ws.Cell(subRow, 5).Value = "CREDIT";
        //        ws.Cell(subRow, 6).Value = "DEBIT";
        //        ws.Cell(subRow, 7).Value = "CREDIT";
        //        ws.Cell(subRow, 8).Value = "DEBIT";
        //        ws.Cell(subRow, 9).Value = "CREDIT";



        //        var headerRange = ws.Range(row, 1, row, 8);
        //        headerRange.Style.Font.SetBold();
        //        headerRange.Style.Fill.SetBackgroundColor(XLColor.LightGreen);
        //        headerRange.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
        //        headerRange.Style.Border.SetInsideBorder(XLBorderStyleValues.Thin);

        //        row++;

        //        // 3️⃣ DATA ROWS
        //        foreach (var x in data)
        //        {
        //            ws.Cell(row, 1).Value = x.AccountNumber;
        //            ws.Cell(row, 2).Value = x.AccountName;
        //            ws.Cell(row, 3).Value = x.OpeningDebit;
        //            ws.Cell(row, 4).Value = x.OpeningCredit;
        //            ws.Cell(row, 5).Value = x.MovementDebit;
        //            ws.Cell(row, 6).Value = x.MovementCredit;
        //            ws.Cell(row, 7).Value = x.ClosingDebit;
        //            ws.Cell(row, 8).Value = x.ClosingCredit;

        //            // Sum totals


        //            // Format number columns
        //            ws.Range(row, 3, row, 8).Style.NumberFormat.Format = "#,##0";

        //            // Borders for row
        //            ws.Range(row, 1, row, 8).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);

        //            // Alternating row color
        //            if ((row % 2) == 0)
        //                ws.Range(row, 1, row, 8).Style.Fill.BackgroundColor = XLColor.FromArgb(245, 245, 245);

        //            row++;
        //        }

        //        // 4️⃣ TOTALS ROW (last record)
        //        var totals = data[0];

        //        ws.Cell(row, 1).Value = "";
        //        ws.Cell(row, 2).Value = "TOTAL";
        //        ws.Cell(row, 3).Value = totals.TotalOpeningDebit;
        //        ws.Cell(row, 4).Value = totals.TotalOpeningCredit;
        //        ws.Cell(row, 5).Value = totals.TotalMovementDebit;
        //        ws.Cell(row, 6).Value = totals.TotalMovementCredit;
        //        ws.Cell(row, 7).Value = totals.TotalClosingDebit;
        //        ws.Cell(row, 8).Value = totals.TotalClosingCredit;



        //        // Format numbers
        //        ws.Range(row, 3, row, 8).Style.NumberFormat.Format = "#,##0";

        //        // Style totals row
        //        var totalsRange = ws.Range(row, 1, row, 8);
        //        totalsRange.Style.Font.SetBold();
        //        totalsRange.Style.Fill.SetBackgroundColor(XLColor.FromArgb(230, 230, 230));
        //        totalsRange.Style.Border.SetOutsideBorder(XLBorderStyleValues.Medium);
        //        totalsRange.Style.Border.SetInsideBorder(XLBorderStyleValues.Thin);

        //        row++;




        //        // 4️⃣ AUTO-SIZE COLUMNS
        //        ws.Columns().AdjustToContents();

        //        workbook.SaveAs(filePath);
        //    }
        //}

    }
}
