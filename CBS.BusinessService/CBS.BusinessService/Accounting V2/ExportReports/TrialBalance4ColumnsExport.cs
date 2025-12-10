using ClosedXML.Excel;
using System;

namespace CBS.BusinessService.Accounting_V2.ExportReports
{
    public class TrialBalance4ColumnsExport
    {
        private readonly BaseExcelHead _baseExcelHead;

        private const int StartColumn = 1;
        private const int EndColumn = 9;

        public TrialBalance4ColumnsExport(BaseExcelHead baseExcelHead)
        {
            _baseExcelHead = baseExcelHead;
        }

        public void ExportTb4(dynamic tb, string filePath, string exportedBy)
        {
            using (var workbook = new XLWorkbook())
            {
            workbook.Style.Font.FontName = "Bahnschrift SemiCondensed";

            var ws = workbook.Worksheets.Add("Trial Balance");
            var data = tb;

            int row = WriteHeader(ws, data, exportedBy);
            WriteTableHeader(ws, row);

            row++;
            WriteDataRows(ws, data, row, out row);
            WriteTotalsRow(ws, data[0], row);

            row++;
            ApplyColumnSizing(ws);
            WriteFooterTotals(ws, data[0], row);

            workbook.SaveAs(filePath);
            }
        }

        #region Header

        private int WriteHeader(IXLWorksheet ws, dynamic data, string exportedBy)
        {
            return _baseExcelHead.WriteExcelHeader(
                ws,
                data[0],
                exportedBy,
                1,
                "TRIAL BALANCE 4 COLUMNS"
            );
        }

        #endregion

        #region Table Header

        private void WriteTableHeader(IXLWorksheet ws, int row)
        {
            string[] headers =
            {
                "SN",
                "ACCOUNT NO",
                "ACCOUNT NAME",
                "BGN BALANCE",
                "SIDE",
                "DEBIT",
                "CREDIT",
                "END BALANCE",
                "SIDE"
            };

            for (int col = 0; col < headers.Length; col++)
                ws.Cell(row, StartColumn + col).Value = headers[col];

            var range = ws.Range(row, StartColumn, row, EndColumn);
            range.Style.Font.SetBold();
            range.Style.Fill.BackgroundColor = XLColor.LightGray;
            range.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
            range.Style.Border.SetInsideBorder(XLBorderStyleValues.Thin);
            range.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            range.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        }

        #endregion

        #region Data Rows

        private void WriteDataRows(IXLWorksheet ws, dynamic data, int startRow, out int endRow)
        {
            int row = startRow;
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

                ws.Range(row, 4, row, EndColumn)
                    .Style.NumberFormat.Format = "#,##0";

                ws.Range(row, 1, row, EndColumn)
                    .Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);

                if (row % 2 == 0)
                {
                    ws.Range(row, 1, row, EndColumn)
                      .Style.Fill.BackgroundColor = XLColor.FromArgb(245, 245, 245);
                }

                //// ✅ CENTER ALIGNMENT
                //_baseExcelHead.CenterColumn(ws, 1); // SN
                //_baseExcelHead.CenterColumn(ws, 5); // Beginning Side
                //_baseExcelHead.CenterColumn(ws, 9); // Ending Side

                row++;
            }

            endRow = row;
        }

        #endregion

        #region Totals Row

        private void WriteTotalsRow(IXLWorksheet ws, dynamic totals, int row)
        {
            ws.Cell(row, 2).Value = "TOTAL";

            ws.Cell(row, 4).Value = totals.TotalBeginningNet;
            ws.Cell(row, 5).Value = totals.TotalBeginningSide;
            ws.Cell(row, 6).Value = totals.TotalDebit;
            ws.Cell(row, 7).Value = totals.TotalCredit;
            ws.Cell(row, 8).Value = totals.TotalEndingNet;
            ws.Cell(row, 9).Value = totals.TotalEndingSide;

            ws.Range(row, 4, row, EndColumn)
                .Style.NumberFormat.Format = "#,##0";

            var range = ws.Range(row, 1, row, EndColumn);
            range.Style.Font.SetBold();
            range.Style.Fill.BackgroundColor = XLColor.FromArgb(230, 230, 230);
            range.Style.Border.SetOutsideBorder(XLBorderStyleValues.Medium);
            range.Style.Border.SetInsideBorder(XLBorderStyleValues.Thin);
        }

        #endregion

        #region Footer

        private void WriteFooterTotals(IXLWorksheet ws, dynamic totals, int row)
        {
            ws.Cell(row, 4).Value = "TOTAL MOVEMENT NET";
            ws.Cell(row, 6).Value = totals.TotalMovementNet;

            ws.Range(row, 4, row, 5).Merge();
            ws.Range(row, 6, row, 7).Merge();
            ws.Range(row, 8, row, 9).Merge();

            ws.Range(row, 4, row, 9)
                .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                .Fill.BackgroundColor = XLColor.FromArgb(245, 245, 245);
                
           
            ws.Range(row, 6, row, 7)
                .Style.Border.SetOutsideBorder(XLBorderStyleValues.Medium)
                .Border.SetInsideBorder(XLBorderStyleValues.Thin);
        }

        #endregion

        #region Columns

        private void ApplyColumnSizing(IXLWorksheet ws)
        {
            ws.Columns().AdjustToContents();

            foreach (var col in ws.ColumnsUsed())
            {
                col.Width = Math.Max(12, Math.Min(col.Width, 45));
            }

            ws.Column(1).Width = 5;
            ws.Column(5).Width = 5;
            ws.Column(9).Width = 5;


        }


        #endregion
    }
}
