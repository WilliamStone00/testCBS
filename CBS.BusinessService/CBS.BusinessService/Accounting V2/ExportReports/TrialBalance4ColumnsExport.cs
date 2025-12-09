using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounting_V2.ExportReports
{
    public class TrialBalance4ColumnsExport
    {
        private readonly BaseExcelHead baseExcelHead;
        public TrialBalance4ColumnsExport(BaseExcelHead baseExcelHead)
        {
            this.baseExcelHead = baseExcelHead;
        }

        public void ExportTb4(dynamic tb, string filePath, string exportedBy)
        {
            var data = tb;

            using (var workbook = new XLWorkbook())
            {
                workbook.Style.Font.FontName = "Bahnschrift SemiCondensed";
                var ws = workbook.Worksheets.Add("Trial Balance");

                // 1️⃣ HEADER (BANK / BRANCH / PERIOD etc.)
                int row = baseExcelHead.WriteExcelHeader(ws, data[0], exportedBy, 1, "TRIAL BALANCE 4 COLUMNS");

                // 2️⃣ TABLE MAIN HEADERS (ROW row)
                ws.Cell(row, 1).Value = "SN";
                ws.Cell(row, 2).Value = "ACCOUNT NO";
                ws.Cell(row, 3).Value = "ACCOUNT NAME";
                ws.Cell(row, 4).Value = "BGN BALANCE";
                ws.Cell(row, 5).Value = "SIDE";
                ws.Cell(row, 6).Value = "DEBIT";
                ws.Cell(row, 7).Value = "CREDIT";
                ws.Cell(row, 8).Value = "END BALANCE";
                ws.Cell(row, 9).Value = "SIDE";



                // Style header area (2 rows high, 9 columns wide) – light gray
                var headerRange = ws.Range(row, 1, row, 9);
                headerRange.Style.Font.SetBold();
                headerRange.Style.Fill.SetBackgroundColor(XLColor.LightGray);
                headerRange.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                headerRange.Style.Border.SetInsideBorder(XLBorderStyleValues.Thin);
                headerRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                headerRange.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);

                row++; // first data row

                // 3️⃣ DATA ROWS (all detail lines)
                int sn = 1;
                foreach (var x in data)
                {
                    ws.Cell(row, 1).Value = sn++;
                    ws.Cell(row, 2).Value = x.AccountNumber;
                    ws.Cell(row, 3).Value = x.AccountName;
                    ws.Cell(row, 4).Value = x.BeginningBalance;
                    ws.Cell(row, 5).Value = x.BeginningBookingDirection;
                    ws.Cell(row, 6).Value = x.Debit;
                    ws.Cell(row, 7).Value = x.Credit;
                    ws.Cell(row, 8).Value = x.EndingBalance;
                    ws.Cell(row, 9).Value = x.EndingBookingDirection;

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

                ws.Cell(row, 4).Value = totals.TotalBeginningNet;
                ws.Cell(row, 5).Value = totals.TotalBeginningSide;
                ws.Cell(row, 6).Value = totals.TotalDebit;
                ws.Cell(row, 7).Value = totals.TotalCredit;
                ws.Cell(row, 8).Value = totals.TotalEndingNet;
                ws.Cell(row, 9).Value = totals.TotalEndingSide;

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


                    // styles ============== styles
                    //ws.Column(1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    //ws.Column(1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;


                    //ws.Column(5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    //ws.Column(5).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                    //ws.Column(9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    //ws.Column(9).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;


                    // styles ============== styles


                }
                ws.Column(1).Width = 5;
                ws.Column(5).Width = 5;
                ws.Column(9).Width = 5;

              

                // adding extra footer calculations
                ws.Cell(row, 4).Value = "TOTAL MOVEMENT NET";
                ws.Cell(row, 6).Value = totals.TotalMovementNet;
                


                ws.Range(row, 4, row, 5).Merge();
                ws.Range(row, 6, row, 7).Merge();
                ws.Range(row, 8, row, 9).Merge();
                ws.Range(row, 6, row, 7).Style.Border.SetOutsideBorder(XLBorderStyleValues.Medium);
                ws.Range(row, 6, row, 7).Style.Border.SetInsideBorder(XLBorderStyleValues.Thin);

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
