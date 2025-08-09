using CBS.FrontDesk.Data.Entity.SalaryManagement;
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CBS.FrontDesk.UI.Helper
{
    public static class SalarySheetExcelExporter
    {
        public static void ExportToExcel(
            List<SalaryAnalysisResultDetailNew> salaryDetails,
            string branchName,
            string filePath,
            string exportedDate,
            string exportedBy,
            string salaryCode)
        {
            var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Salary Sheet");

            // === HEADER ===
            ws.Cell(1, 1).Value = $"SALARY SHEET FOR {branchName.ToUpper()}";
            ws.Range(1, 1, 1, 22).Merge().Style
                .Font.SetBold()
                .Font.SetFontSize(14)
                .Font.SetFontName("Bahnschrift Light")
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);

            ws.Cell(2, 1).Value = $"Exported: {exportedDate} BY {exportedBy}";
            ws.Range(2, 1, 2, 22).Merge().Style.Font.SetFontName("Bahnschrift Light");

            ws.Cell(3, 1).Value = $"Salary Code: {salaryCode}";
            ws.Range(3, 1, 3, 22).Merge().Style.Font.SetFontName("Bahnschrift Light");

            int groupHeaderRow = 5;
            int subHeaderRow = 6;

            // === GROUP HEADERS ===
            var groupHeaders = new Dictionary<int, string>
            {
                { 9, "MAIN_LOAN" },
                { 11, "EXCEPTIONAL_LOAN" },
                { 13, "ELECTED_STAFF_OFFICIALS" },
                { 15, "SPECIAL_LOANS_&_OVERDRAFT" },
                { 17, "MICRO_LOAN" },
                { 19, "SSF" }
            };

            foreach (var kvp in groupHeaders)
            {
                int colStart = kvp.Key;
                int colEnd = colStart + 1;
                string text = kvp.Value;

                var range = ws.Range(groupHeaderRow, colStart, groupHeaderRow, colEnd);
                range.Merge();
                range.Value = text;

                range.Style.Fill.BackgroundColor = XLColor.LightGray;
                range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                range.Style.Alignment.WrapText = true;
                range.Style.Font.SetBold();
                range.Style.Font.FontName = "Bahnschrift Light";
                range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            // Fill remaining blank cells
            for (int i = 1; i <= 8; i++) ws.Cell(groupHeaderRow, i).Value = "";
            ws.Cell(groupHeaderRow, 21).Value = "";
            ws.Cell(groupHeaderRow, 22).Value = "";

            ws.Row(groupHeaderRow).Height = 35;

            // === SUB HEADERS ===
            var headers = new[]
            {
            "ACC. NO", "NAME", "AMOUNT", "STANDING AMOUNT", "SAVINGS", "DEPOSIT", "NET SALARY", "SHARES",
            "LOAN 1", "INT", "LOAN 2", "INT", "LOAN 3", "INT", "LOAN 4", "INT", "LOAN 8", "INT",
            "SP SAV", "INT", "CHARGES", "CONTROL"
        };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(subHeaderRow, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.SetBold();
                cell.Style.Font.FontName = "Bahnschrift Light";
                cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                cell.Style.Alignment.WrapText = true;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            ws.Row(subHeaderRow).Height = 30;

            // === DATA ROWS ===
            int row = subHeaderRow + 1;
            foreach (var item in salaryDetails)
            {
                ws.Cell(row, 1).Value = item.CustomerId;
                ws.Cell(row, 2).Value = item.MemberName;
                ws.Cell(row, 3).Value = item.NetSalary;
                ws.Cell(row, 4).Value = item.StandingOrderAmount;
                ws.Cell(row, 5).Value = item.Savings;
                ws.Cell(row, 6).Value = item.Deposit;
                ws.Cell(row, 7).Value = item.Salary;
                ws.Cell(row, 8).Value = item.Shares;

                ws.Cell(row, 9).Value = item.Loan1_Main_Loan_Capital;
                ws.Cell(row, 10).Value = item.Loan1_Main_Loan_Interest;

                ws.Cell(row, 11).Value = item.Loan2_Exceptional_Loan_Capital;
                ws.Cell(row, 12).Value = item.Loan2_Exceptional_Loan_Interest;

                ws.Cell(row, 13).Value = item.Loan3_Elected_Staff_Capital;
                ws.Cell(row, 14).Value = item.Loan3_Elected_Staff_Interest;

                ws.Cell(row, 15).Value = item.Loan4_Special_Loan_With_Overdraft_Capital;
                ws.Cell(row, 16).Value = item.Loan4_Special_Loan_With_Overdraft_Interest;

                ws.Cell(row, 17).Value = item.Loan8_Micro_Loan_Capital;
                ws.Cell(row, 18).Value = item.Loan8_Micro_Loan_Interest;

                ws.Cell(row, 19).Value = item.SpSavingCapital;
                ws.Cell(row, 20).Value = item.SpSavingInterest;

                ws.Cell(row, 21).Value = item.Charges;

                ws.Cell(row, 22).FormulaA1 = $"=IF(ROUND(SUM(E{row}:U{row}),0)=ROUND(C{row},0),\"✔\",\"✘\")";
                ws.Cell(row, 22).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                for (int col = 3; col <= 21; col++)
                {
                    ws.Cell(row, col).Style.NumberFormat.Format = "#,##0";
                    ws.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                }

                for (int col = 1; col <= 22; col++)
                {
                    ws.Cell(row, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    ws.Cell(row, col).Style.Font.FontName = "Bahnschrift Light";
                }

                row++;
            }

            // === TOTAL ROW ===
            ws.Cell(row, 1).Value = "TOTAL";
            ws.Range(row, 1, row, 2).Merge().Style.Font.SetBold().Font.FontName = "Bahnschrift Light";

            for (int col = 3; col <= 21; col++)
            {
                string colLetter = ws.Column(col).ColumnLetter();
                var cell = ws.Cell(row, col);
                cell.FormulaA1 = $"SUM({colLetter}{subHeaderRow + 1}:{colLetter}{row - 1})";
                cell.Style.NumberFormat.Format = "#,##0";
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            for (int col = 1; col <= 2; col++)
            {
                ws.Cell(row, col).Style.Fill.BackgroundColor = XLColor.LightGray;
                ws.Cell(row, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            var controlCell = ws.Cell(row, 22);
            controlCell.FormulaA1 = $"=IF(ROUND(SUM(E{row}:U{row}),0)=ROUND(C{row},0),\"✔\",\"✘\")";
            controlCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            controlCell.Style.Font.SetBold();
            controlCell.Style.Fill.BackgroundColor = XLColor.White;

            // === CONDITIONAL FORMATTING ===
            var controlRange = ws.Range(subHeaderRow + 1, 22, row, 22);
            controlRange.AddConditionalFormat()
                .WhenEquals("✔")
                .Fill.SetBackgroundColor(XLColor.LightGreen)
                .Font.SetFontColor(XLColor.DarkGreen);

            controlRange.AddConditionalFormat()
                .WhenEquals("✘")
                .Fill.SetBackgroundColor(XLColor.LightPink)
                .Font.SetFontColor(XLColor.Red);

            // Auto-fit columns
            ws.Columns().AdjustToContents();

            // Save file
            workbook.SaveAs(filePath);
        }
    }

}