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
            string BranchName = SafeGet(firstRow, "BranchName");
            string BranchCode = SafeGet(firstRow, "BranchCode");
            string BankName = SafeGet(firstRow, "BankName");
            string Status = SafeGet(firstRow, "Mode");
            string OperationCode = SafeGet(firstRow, "OperationCode");
            string BranchTelephone = SafeGet(firstRow, "BranchPhone");
            string BranchEmail =SafeGet(firstRow, "BranchEmail");
            string BranchAddress = SafeGet(firstRow, "BranchAddress");
            string PrintedBy = SafeGet(firstRow, "Username");
            string Period = $"FROM {SafeGet(firstRow, "From")} TO {SafeGet(firstRow, "To")}";
            string ReportName = "TRIAL BALANCE 6 COLUMNS";
            string exportedInfoDN = $"Exported ON: {DateTime.Now:dd/MM/yyyy HH:mm}   |   By: {PrintedBy}";



            int row = startRow;

            // ───────────── BANK NAME (FIRST LINE) — Blue background
            ws.Cell(row, 1).Value = BankName;
            var line1 = ws.Range(row, 1, row, 6);
            line1.Merge();
            line1.Style.Font.SetBold().Font.SetFontSize(18);
            line1.Style.Font.FontColor = XLColor.White;
            line1.Style.Fill.BackgroundColor = XLColor.FromArgb(30, 60, 180);
            row++;

            // ───────────── MAIN BRANCH HEADER — Dark Green background
            ws.Cell(row, 1).Value = $"BRANCH: {BranchName} (Code: {BranchCode})";
            var line2 = ws.Range(row, 1, row, 6);
            line2.Merge();
            line2.Style.Font.SetBold();
            line2.Style.Font.SetFontSize(15);
            line2.Style.Font.FontColor = XLColor.White;
            line2.Style.Fill.BackgroundColor = XLColor.FromArgb(0, 100, 0);
            row++;

            // ───────────── Branch Name — Light Blue background + border
            ws.Cell(row, 1).Value = $"Branch Name: {BranchName}";
            var line3 = ws.Range(row, 1, row, 6);
            line3.Merge();
            line3.Style.Font.SetFontSize(13).Font.SetBold();
            line3.Style.Fill.BackgroundColor = XLColor.FromArgb(210, 230, 255);
            line3.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
            row++;

            // ───────────── Period — Yellow background + italic
            ws.Cell(row, 1).Value = Period;
            var line4 = ws.Range(row, 1, row, 6);
            line4.Merge();
            line4.Style.Font.SetItalic();
            line4.Style.Font.SetFontSize(12);
            line4.Style.Fill.BackgroundColor = XLColor.LightYellow;
            row++;



            row += 2;

            return row; // return where the table section should start
        }



        public void ExportTb6(dynamic tb, string filePath, string exportedBy)
        {
            var data = tb;

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Trial Balance");

                // 1️⃣ WRITE HEADER (separated)
                int row = WriteExcelHeader(ws, data[0], exportedBy, 1);

                // 2️⃣ TABLE HEADERS
                ws.Cell(row, 1).Value = "Account Number";
                ws.Cell(row, 2).Value = "Account Name";
                ws.Cell(row, 3).Value = "Opening Debit";
                ws.Cell(row, 4).Value = "Opening Credit";
                ws.Cell(row, 5).Value = "Closing Debit";
                ws.Cell(row, 6).Value = "Closing Credit";

                for (int col = 1; col <= 6; col++)
                {
                    ws.Cell(row, col).Style
                        .Font.SetBold()
                        .Fill.SetBackgroundColor(XLColor.LightGreen)
                        .Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                }
                row++;

                // 3️⃣ DATA ROWS
                foreach (var x in data)
                {
                    ws.Cell(row, 1).Value = x.AccountNumber;
                    ws.Cell(row, 2).Value = x.AccountName;
                    ws.Cell(row, 3).Value = x.OpeningDebit;
                    ws.Cell(row, 4).Value = x.OpeningCredit;
                    ws.Cell(row, 5).Value = x.ClosingDebit;
                    ws.Cell(row, 6).Value = x.ClosingCredit;

                    ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0";
                    ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0";
                    ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0";
                    ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0";

                    for (int col = 1; col <= 6; col++)
                        ws.Cell(row, col).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);

                    row++;
                }

                // 4️⃣ AUTO-SIZE COLUMNS
                ws.Columns().AdjustToContents();

                workbook.SaveAs(filePath);
            }
        }

    }
}
