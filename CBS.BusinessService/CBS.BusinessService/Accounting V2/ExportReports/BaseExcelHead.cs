using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounting_V2.ExportReports
{
    public class BaseExcelHead
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

        public int WriteExcelHeader(IXLWorksheet ws, dynamic firstRow, string exportedBy, int startRow, string PRTittle = "TRIAL BALANCE 6 COLUMNS")
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

            string ReportTitle = PRTittle;
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

            ws.Cell(row, 1).Value = $"🧾 {ReportTitle} {Mode}";
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

            ws.Cell(row, 1).Value = $"Address: {BranchAddress} | Tel: {BranchTelephone}";
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
    }
}
