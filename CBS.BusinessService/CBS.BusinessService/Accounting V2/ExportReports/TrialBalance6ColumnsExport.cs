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


        

        public int WriteExcelHeader(IXLWorksheet ws, dynamic firstRow, string exportedBy, int startRow)
        {
            string BankName = SafeGet(firstRow, "BankName");
            string BranchName = SafeGet(firstRow, "BranchName");
            string BranchCode = SafeGet(firstRow, "BranchCode");
            string BranchAddress = SafeGet(firstRow, "BranchAddress");
            string BranchTelephone = SafeGet(firstRow, "BranchPhone");
            string Bp = SafeGet(firstRow, "BranchCode");
            string PrintedBy = SafeGet(firstRow, "Username");
            string AccountingDate = SafeGet(firstRow, "AccountingDate");

            string PeriodFrom = SafeGet(firstRow, "From");
            string Mode = SafeGet(firstRow, "Mode");
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
            var line1 = ws.Range(row, 1, row, 9);
            line1.Merge();

            // Apply styling
            line1.Style.Font.SetBold().Font.SetFontSize(18);
            line1.Style.Font.FontColor = XLColor.White;
            line1.Style.Fill.BackgroundColor = XLColor.FromArgb(30, 60, 180);
            line1.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            line1.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;


            row++;

            // ───────────── Branch Name — Light Blue background + border
            ws.Cell(row, 1).Value = $"Branch Name: {BranchName} | Code {BranchCode}";
            var line3 = ws.Range(row, 1, row, 9);
            line3.Merge();
            line3.Style.Font.SetFontSize(12).Font.SetBold();
            line3.Style.Font.FontColor = XLColor.White;
            line3.Style.Fill.BackgroundColor = XLColor.FromArgb(73, 160, 232);
            line3.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
            line3.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            line3.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            row++;

            // ───────────── Exported Section Name
            ws.Cell(row, 1).Value = $"Exported On: {PrintDate} | Exported By {PrintedBy}";
            var line4 = ws.Range(row, 1, row, 9);
            line4.Merge();
            line4.Style.Font.SetFontSize(12).Font.SetBold();
            line4.Style.Font.FontColor = XLColor.Gray;
            line4.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            line4.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            row += 2;

            ws.Cell(row, 1).Value = $"🧾 TRIAL BALANCE 6 COLUMNS {Mode}";
            var line5 = ws.Range(row, 1, row, 9);
            line5.Merge();
            line5.Style.Font.SetFontSize(14).Font.SetBold();
            line5.Style.Font.FontColor = XLColor.Black;
            line5.Style.Fill.BackgroundColor = XLColor.FromArgb(135, 201, 204);
            line5.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
            line5.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            line5.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            var sep = ws.Range(row, 1, row, 9);
            sep.Merge();
            sep.Style.Border.BottomBorder = XLBorderStyleValues.Thick;


            row += 2;

            ws.Cell(row,1).Value = $"Address: {BranchAddress} | Tel: {BranchTelephone}";
            var line6 = ws.Range(row, 1, row, 3);
            line6.Style.Font.FontColor = XLColor.Black;
            row++;
            ws.Cell(row, 1).Value = $"Accounting Date: {AccountingDate}";
            var line7 = ws.Range(row, 1, row, 3);
            line7.Style.Font.FontColor = XLColor.Black;
            row++;

            ws.Cell(row, 1).Value = $"Status: {Mode}";
            var line8 = ws.Range(row, 1, row, 3);
            line8.Style.Font.FontColor = XLColor.Black;
            row++;

            ws.Cell(row, 1).Value = $"Report Period: {PeriodFrom}  To: {PeriodTo}";
            var line9 = ws.Range(row, 1, row, 3);
            line9.Style.Font.FontColor = XLColor.Black;
            row++;

            // ───────────────────────── HORIZONTAL LINE SEPARATOR ─────────────────────────
            var border = ws.Range(row, 1, row, 9);
            border.Merge();
            border.Style.Border.BottomBorder = XLBorderStyleValues.Thick;
            row++;


            return row;  // table starts here
        }


        public void ExportTb6(dynamic tb, string filePath, string exportedBy)
        {
            var data = tb;

            using (var workbook = new XLWorkbook())
            {
                workbook.Style.Font.FontName = "Bahnschrift SemiCondensed";
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
                ws.Column(1).Width = 5;

                // adding extra footer calculations
                ws.Cell(row,4).Value = totals.TotalOpeningDifference;
                ws.Cell(row, 6).Value = totals.TotalMovementDifference;
                ws.Cell(row, 8).Value = totals.TotalClosingDifference;


                ws.Range(row, 4, row, 5).Merge();
                ws.Range(row, 6, row, 7).Merge();
                ws.Range(row, 8, row, 9).Merge();
                ws.Range(row, 4, row, 9).Style.Border.SetOutsideBorder(XLBorderStyleValues.Medium);
                ws.Range(row, 4, row, 9).Style.Border.SetInsideBorder(XLBorderStyleValues.Thin);

                //styles
                //----------- group 1 -----------------------------
                ws.Range(row, 4, row, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Range(row, 4, row, 5).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                //----------- group 2 -----------------------------
                ws.Range(row, 6, row, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Range(row, 6, row, 7).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                //----------- group 3 -----------------------------
                ws.Range(row, 8, row, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Range(row, 8, row, 9).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                workbook.SaveAs(filePath);
            }
        }


      

    }
}
