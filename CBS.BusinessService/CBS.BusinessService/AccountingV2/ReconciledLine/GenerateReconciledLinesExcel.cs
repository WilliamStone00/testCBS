using BusinessServices;
using CBS.FrontDesk.Data.Entity.AccountingV2.ReconciledLine;
using ClosedXML.Excel;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.AccountingV2.ReconciledLine
{
    public class GenerateReconciledLinesExcelExport : BaseService
    {
        public void GenerateReconciledLinesExcel(List<Reconciled> reconciledLines, string filePath, string exportedBy, string referenceNumber)
        {
            using (var workbook = new XLWorkbook())
            {
                workbook.Style.Font.FontName = "Bahnschrift SemiCondensed";

                var ws = workbook.Worksheets.Add("Reconciled Lines");
                int row = 1;

                // ============================================================
                // FETCH BANK + BRANCH
                // ============================================================
                string bank = GetBankName();
                string branchCode = GetBranchCode();
                string branchName = GetBranchName();

                // ============================================================
                // HEADER SECTION - CORRECTED TO SPAN ALL COLUMNS (A-K)
                // ============================================================
                ws.Cell(row, 1).Value = bank;
                ws.Range(row, 1, row, 11).Merge().Style  // Changed from 8 to 11 (A-K)
                    .Font.SetBold()
                    .Font.SetFontSize(18)
                    .Font.SetFontColor(XLColor.White)
                    .Fill.SetBackgroundColor(XLColor.FromArgb(0, 100, 0))
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                row++;

                ws.Cell(row, 1).Value = $"Branch Name: {branchName}   |   Code: {branchCode}";
                ws.Range(row, 1, row, 11).Merge().Style  // Changed from 8 to 11 (A-K)
                    .Font.SetBold()
                    .Font.SetFontColor(XLColor.White)
                    .Fill.SetBackgroundColor(XLColor.FromArgb(70, 130, 180))
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                row++;

                ws.Cell(row, 1).Value = $"Exported On: {DateTime.Now:dd/MM/yyyy HH:mm}   |   Exported By: {exportedBy}   |   Reference: {referenceNumber}";
                ws.Range(row, 1, row, 11).Merge().Style  // Changed from 8 to 11 (A-K)
                    .Font.SetBold()
                    .Font.SetFontColor(XLColor.Gray);
                row += 2;

                // ============================================================
                // SUMMARY SECTION - CORRECTED TO SPAN ALL COLUMNS
                // ============================================================
                var sectionTitle = ws.Cell(row, 1);
                sectionTitle.Value = "📊 RECONCILED LINES SUMMARY";
                ws.Range(row, 1, row, 11).Merge();  // Changed from 8 to 11 (A-K)
                ApplySectionTitleStyle(sectionTitle);
                row += 2;

                // Summary Statistics
                var summaryData = new[]
                {
                    ("Total Lines", reconciledLines.Count.ToString()),
                    ("Total Debit Amount", reconciledLines.Sum(r => r.DebitAmount).ToString("N2")),
                    ("Total Credit Amount", reconciledLines.Sum(r => r.CreditAmount).ToString("N2")),
                    ("Total Balance", reconciledLines.Sum(r => r.Balance).ToString("N2")),
                    ("Date Range", $"{reconciledLines.Min(r => r.EntryDate):dd/MM/yyyy} to {reconciledLines.Max(r => r.EntryDate):dd/MM/yyyy}")
                };

                foreach (var item in summaryData)
                {
                    ws.Cell(row, 1).Value = $"{item.Item1}: {item.Item2}";
                    ws.Range(row, 1, row, 11).Merge();  // Changed from 2 to 11
                    row++;
                }

                row += 1;

                // ============================================================
                // RECONCILED LINES DETAILS SECTION - CORRECTED
                // ============================================================
                var detailsTitle = ws.Cell(row, 1);
                detailsTitle.Value = "📋 RECONCILED LINES DETAILS";
                ws.Range(row, 1, row, 11).Merge();  // Changed from 8 to 11 (A-K)
                ApplySectionTitleStyle(detailsTitle);
                row += 2;

                // Table Headers
                var headers = new[]
                {
                    "Line #",
                    "Account Number",
                    "Account Name",
                    "Description",
                    "Dr/Cr",
                    "Amount",
                    "Balance",
                    "Entry Date",
                    "Branch",
                    "Created By",
                    "Status"
                };

                row = CreateTableHeaderRow(ws, row, headers);

                // Table Data
                foreach (var line in reconciledLines.OrderBy(r => r.LineNum))
                {
                    ws.Cell(row, 1).Value = line.LineNum;
                    ws.Cell(row, 2).Value = line.AccountNumber;
                    ws.Cell(row, 3).Value = line.AccountName;
                    ws.Cell(row, 4).Value = line.Description;
                    ws.Cell(row, 5).Value = line.DrCr;

                    // Amount formatting based on Dr/Cr
                    if (line.DrCr?.ToLower() == "debit")
                    {
                        ws.Cell(row, 6).Value = line.DebitAmount;
                    }
                    else
                    {
                        ws.Cell(row, 6).Value = line.CreditAmount;
                    }

                    ws.Cell(row, 7).Value = line.Balance;
                    ws.Cell(row, 8).Value = line.EntryDate.ToString("dd/MM/yyyy HH:mm");
                    ws.Cell(row, 9).Value = line.BranchName;
                    ws.Cell(row, 10).Value = line.CreatedBy;

                    // Status based on approval/rejection
                    string status = "Pending";
                    if (!string.IsNullOrEmpty(line.ApprovedBy))
                        status = $"Approved by {line.ApprovedBy} on {line.ApprovalDate:dd/MM/yyyy}";
                    else if (!string.IsNullOrEmpty(line.RejectedBy))
                        status = $"Rejected by {line.RejectedBy} on {line.RejectedDate:dd/MM/yyyy}";

                    ws.Cell(row, 11).Value = status;

                    // Format number columns - FIXED: Use #,##0 instead of #,##0.00 for whole numbers
                    ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0";  // Changed from #,##0.00
                    ws.Cell(row, 7).Style.NumberFormat.Format = "#,##0";  // Changed from #,##0.00

                    // Apply borders
                    ApplyTableCellBorders(ws, row, 1, 11);

                    row++;
                }

                // ============================================================
                // TOTALS ROW - CORRECTED POSITION (should be in columns E, F, G)
                // ============================================================
                if (reconciledLines.Any())
                {
                    ws.Cell(row, 4).Value = "TOTAL:";  // Changed from column 4 to align with Dr/Cr column
                    ws.Cell(row, 4).Style
                        .Font.SetBold()
                        .Font.SetFontColor(XLColor.DarkGreen)
                        .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

                    ws.Cell(row, 5).Value = reconciledLines.Sum(r => r.DebitAmount);
                    ws.Cell(row, 5).Style
                        .Font.SetBold()
                        .NumberFormat.Format = "#,##0";  // Changed from #,##0.00

                    ws.Cell(row, 6).Value = reconciledLines.Sum(r => r.CreditAmount);
                    ws.Cell(row, 6).Style
                        .Font.SetBold()
                        .NumberFormat.Format = "#,##0";  // Changed from #,##0.00

                    ApplyTableCellBorders(ws, row, 4, 6);
                    row++;
                }

                // Auto-size columns
                ws.Columns().AdjustToContents();

                // Set reasonable column widths
                foreach (var column in ws.ColumnsUsed())
                {
                    if (column.ColumnNumber() == 4) // Description column
                        column.Width = Math.Min(Math.Max(column.Width, 20), 60);
                    else if (column.ColumnNumber() == 11) // Status column
                        column.Width = Math.Min(Math.Max(column.Width, 20), 50);
                    else if (column.ColumnNumber() == 2) // Account Number column
                        column.Width = Math.Max(column.Width, 15);
                    else if (column.ColumnNumber() == 3) // Account Name column
                        column.Width = Math.Min(Math.Max(column.Width, 20), 40);
                    else if (column.Width < 10)
                        column.Width = 10;
                    else if (column.Width > 30)
                        column.Width = 30;
                }

                // ============================================================
                // FOOTER - CORRECTED TO SPAN ALL COLUMNS
                // ============================================================
                var footer = ws.Cell(row, 1);
                footer.Value = $"Generated on {DateTime.Now:yyyy-MM-dd HH:mm} | Total Records: {reconciledLines.Count}";
                footer.Style.Font.Bold = true;
                footer.Style.Font.FontColor = XLColor.Gray;
                ws.Range(row, 1, row, 11).Merge();  // Changed from 11 to 11 (already correct)

                workbook.SaveAs(filePath);
            }
        }

        // Helper method for section titles
        private void ApplySectionTitleStyle(IXLCell cell)
        {
            cell.Style
                .Font.SetBold()
                .Font.SetFontSize(14)
                .Font.SetFontColor(XLColor.White)
                .Fill.SetBackgroundColor(XLColor.FromArgb(47, 84, 150))
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        }

        // Helper method for table headers
        private int CreateTableHeaderRow(IXLWorksheet ws, int row, string[] headers)
        {
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(row, i + 1).Value = headers[i];
                ws.Cell(row, i + 1).Style
                    .Font.SetBold()
                    .Font.SetFontColor(XLColor.White)
                    .Fill.SetBackgroundColor(XLColor.FromArgb(79, 129, 189))
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                ApplyTableCellBorders(ws, row, i + 1, i + 1);
            }
            return row + 1;
        }

        // Helper method for cell borders
        private void ApplyTableCellBorders(IXLWorksheet ws, int row, int startCol, int endCol)
        {
            var range = ws.Range(row, startCol, row, endCol);
            range.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
            range.Style.Border.SetInsideBorder(XLBorderStyleValues.Thin);
        }
    }

    


    public class ReconciledExcelExportGenerator : BaseService
    {
        public void GenerateReconciledExcel(List<Reconciled> reconciledData, string filePath, string exportedBy, ExportOptions exportOptions)
        {
            // Set EPPlus license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                // Get bank/branch info
                string bank = GetBankName();
                string branchcode = GetBranchCode();
                string branchid = GetBranchID();
                string branchname = GetBranchName();

                // ===== SHEET 1: SUMMARY & OVERVIEW =====
                if (exportOptions.IncludeSummary)
                {
                    CreateSummarySheet(package, reconciledData, bank, branchcode, branchid, branchname, exportedBy, exportOptions);
                }

                // ===== SHEET 2: DETAILED RECONCILED ENTRIES =====
                CreateDetailedSheet(package, reconciledData, bank, branchcode, branchid, branchname, exportedBy, exportOptions);

                // ===== SHEET 3: BY BRANCH ANALYSIS =====
                CreateBranchAnalysisSheet(package, reconciledData, bank, branchcode, branchid, branchname, exportedBy, exportOptions);

                // ===== SHEET 4: BY ACCOUNT ANALYSIS =====
                CreateAccountAnalysisSheet(package, reconciledData, bank, branchcode, branchid, branchname, exportedBy, exportOptions);

                // ===== SHEET 5: INTERBRANCH TRANSACTIONS =====
                var interbranchData = reconciledData.Where(x => x.InterbranchStatus).ToList();
                if (interbranchData.Any())
                {
                    CreateInterbranchSheet(package, interbranchData, bank, branchcode, branchid, branchname, exportedBy, exportOptions);
                }

                // Save the file
                package.SaveAs(new FileInfo(filePath));
            }
        }

        private void CreateSummarySheet(ExcelPackage package, List<Reconciled> data, string bank, string branchcode, string branchid, string branchname, string exportedBy, ExportOptions exportOptions)
        {
            var worksheet = package.Workbook.Worksheets.Add("Summary & Overview");

            // Set font
            var fontName = "Bahnschrift SemiCondensed";
            worksheet.Cells.Style.Font.Name = fontName;

            // ===== HEADER SECTION =====
            int headerColumns = 4;
            string headerEndColumn = GetColumnLetter(headerColumns);
            int currentRow = CreateHeaderSection(worksheet, bank, branchcode, branchid, branchname, exportedBy, exportOptions, "RECONCILED ENTRIES SUMMARY REPORT", headerEndColumn);

            // ===== GENERAL SUMMARY SECTION =====
            worksheet.Cells[$"A{currentRow}:{headerEndColumn}{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = "GENERAL SUMMARY";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Size = 14;
            worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
            currentRow += 2;

            // Calculate summary statistics
            var totalEntries = data.Count;
            var totalDebit = data.Sum(x => x.DebitAmount);
            var totalCredit = data.Sum(x => x.CreditAmount);
            var totalAmount = data.Sum(x => x.Amount);
            var totalBalance = data.Sum(x => x.Balance);
            var branchesCount = data.Select(x => x.BranchName).Distinct().Count();
            var accountsCount = data.Select(x => new { x.AccountNumber, x.AccountName }).Distinct().Count();
            var interbranchCount = data.Count(x => x.InterbranchStatus);

            // Summary table headers
            var summaryHeaders = new[] { "Metric", "Value", "Description" };
            for (int i = 0; i < summaryHeaders.Length; i++)
            {
                worksheet.Cells[currentRow, 1 + i].Value = summaryHeaders[i];
                worksheet.Cells[currentRow, 1 + i].Style.Font.Bold = true;
                worksheet.Cells[currentRow, 1 + i].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[currentRow, 1 + i].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                worksheet.Cells[currentRow, 1 + i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }
            currentRow++;

            // Summary data - fix division by zero
            var summaryData = new[]
            {
            new { Metric = "Total Entries", Value = totalEntries.ToString("N0"), Description = "Number of reconciled entries" },
            new { Metric = "Total Debit Amount", Value = totalDebit.ToString("N2"), Description = "Sum of all debit amounts" },
            new { Metric = "Total Credit Amount", Value = totalCredit.ToString("N2"), Description = "Sum of all credit amounts" },
            new { Metric = "Total Transaction Amount", Value = totalAmount.ToString("N2"), Description = "Sum of all transaction amounts" },
            new { Metric = "Total Balance", Value = totalBalance.ToString("N2"), Description = "Sum of all balances" },
            new { Metric = "Branches Involved", Value = branchesCount.ToString("N0"), Description = "Number of branches with entries" },
            new { Metric = "Accounts Involved", Value = accountsCount.ToString("N0"), Description = "Number of unique accounts" },
            new { Metric = "Inter-branch Transactions", Value = interbranchCount.ToString("N0"), Description = "Cross-branch transactions" },
            new { Metric = "Average Debit", Value = totalEntries > 0 ? (totalDebit / totalEntries).ToString("N2") : "0.00", Description = "Average debit per entry" },
            new { Metric = "Average Credit", Value = totalEntries > 0 ? (totalCredit / totalEntries).ToString("N2") : "0.00", Description = "Average credit per entry" }
        };

            foreach (var item in summaryData)
            {
                worksheet.Cells[currentRow, 1].Value = item.Metric;
                worksheet.Cells[currentRow, 2].Value = item.Value;
                worksheet.Cells[currentRow, 3].Value = item.Description;

                for (int col = 1; col <= headerColumns; col++)
                {
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
                currentRow++;
            }

            currentRow += 2;

            // ===== DEBIT/CREDIT RATIO SECTION =====
            CreateDebitCreditRatioSection(worksheet, data, ref currentRow, headerEndColumn);

            currentRow += 2;

            // ===== TOP 10 ACCOUNTS BY VOLUME =====
            CreateTopAccountsSection(worksheet, data, ref currentRow, headerEndColumn);

            // Auto-fit columns
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        }

        private void CreateDetailedSheet(ExcelPackage package, List<Reconciled> data, string bank, string branchcode, string branchid, string branchname, string exportedBy, ExportOptions exportOptions)
        {
            var worksheet = package.Workbook.Worksheets.Add("Detailed Entries");

            // Set font
            var fontName = "Bahnschrift SemiCondensed";
            worksheet.Cells.Style.Font.Name = fontName;

            // ===== HEADER SECTION =====
            int headerColumns = 9; // SN (1), Entry Date (2), Reference (3), Auxiliary Ref (4), Branch (5), Account (6), Account Name (7), Dr/Cr (8), Amount (9)
            string headerEndColumn = GetColumnLetter(headerColumns);

            // Start header section
            int currentRow = 1;

            // ===== BANK HEADER =====
            worksheet.Cells[$"A{currentRow}:{headerEndColumn}{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = bank;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Size = 18;
            worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 100, 0)); // Dark green
            worksheet.Cells[$"A{currentRow}"].Style.Font.Color.SetColor(Color.White);
            worksheet.Row(currentRow).Height = 30;
            currentRow++;

            // ===== BRANCH INFO =====
            worksheet.Cells[$"A{currentRow}:{headerEndColumn}{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = $"Branch Code: {branchcode} | Branch: {branchname} | Branch ID: {branchid}";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Size = 12;
            worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(70, 130, 180)); // Steel blue
            worksheet.Cells[$"A{currentRow}"].Style.Font.Color.SetColor(Color.White);
            worksheet.Row(currentRow).Height = 22;
            currentRow++;

            // ===== EXPORT INFO =====
            worksheet.Cells[$"A{currentRow}:{headerEndColumn}{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = $"Exported By: {exportedBy}";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Row(currentRow).Height = 18;
            currentRow++;

            worksheet.Cells[$"A{currentRow}:{headerEndColumn}{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = $"Export Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Row(currentRow).Height = 18;
            currentRow++;

            // ===== MAIN REPORT TITLE =====
            worksheet.Cells[$"A{currentRow}:{headerEndColumn}{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = "DETAILED RECONCILED ENTRIES";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Size = 14;
            worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 215, 0)); // Gold
            worksheet.Cells[$"A{currentRow}"].Style.Font.Color.SetColor(Color.Black);
            worksheet.Row(currentRow).Height = 25;
            currentRow++;

            // ===== SECTION TITLE (matching image) =====
            worksheet.Cells[$"A{currentRow}:{headerEndColumn}{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = "DETAILED RECONCILED ENTRIES";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Size = 12;
            worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Row(currentRow).Height = 20;
            currentRow++;

            // ===== SUBTITLE (matching image) =====
            worksheet.Cells[$"A{currentRow}:{headerEndColumn}{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = "RECONCILED ENTRIES DETAIL";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Size = 10;
            worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Row(currentRow).Height = 18;
            currentRow++;

            // ===== TABLE HEADERS =====
            // Headers matching the image exactly
            var headers = new[]
            {
        "SN", "Entry Date", "Reference", "Auxiliary Ref", "Branch", "Account",
        "Account Name", "Dr/Cr", "Amount"
    };

            int headerRow = currentRow;

            // Set header row properties
            worksheet.Row(headerRow).Height = 35;

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[headerRow, i + 1].Value = headers[i];
                worksheet.Cells[headerRow, i + 1].Style.Font.Bold = true;
                worksheet.Cells[headerRow, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[headerRow, i + 1].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                worksheet.Cells[headerRow, i + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                worksheet.Cells[headerRow, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[headerRow, i + 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Cells[headerRow, i + 1].Style.WrapText = true;
            }
            currentRow++;

            // ===== TABLE DATA =====
            int serialNumber = 1;
            int dataStartRow = currentRow;

            foreach (var entry in data.OrderBy(x => x.EntryDate).ThenBy(x => x.Seq))
            {
                // Set row height for data rows
                worksheet.Row(currentRow).Height = 25;

                // Fill data - matching image format
                worksheet.Cells[currentRow, 1].Value = serialNumber++;
                worksheet.Cells[currentRow, 2].Value = entry.EntryDate.ToString("yyyy-MM-dd HH:mm:ss");
                worksheet.Cells[currentRow, 3].Value = entry.ReferenceNumber ?? "N/A";
                worksheet.Cells[currentRow, 4].Value = entry.AuxiliaryRef ?? "N/A";
                worksheet.Cells[currentRow, 5].Value = entry.BranchName ?? "N/A";
                worksheet.Cells[currentRow, 6].Value = entry.AccountNumber ?? "N/A";
                worksheet.Cells[currentRow, 7].Value = entry.AccountName ?? "N/A";
                worksheet.Cells[currentRow, 8].Value = entry.DrCr == "DR" ? "Debit" : "Credit"; // Match image format
                worksheet.Cells[currentRow, 9].Value = entry.Amount;

                // Color code Dr/Cr - match image styling
                if (entry.DrCr == "DR")
                {
                    worksheet.Cells[currentRow, 8].Style.Font.Color.SetColor(Color.Red);
                    worksheet.Cells[currentRow, 8].Style.Font.Bold = true;
                }
                else if (entry.DrCr == "CR")
                {
                    worksheet.Cells[currentRow, 8].Style.Font.Color.SetColor(Color.Green);
                    worksheet.Cells[currentRow, 8].Style.Font.Bold = true;
                }

                // Add borders and alignment to all cells in the row
                for (int col = 1; col <= headers.Length; col++)
                {
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                    // Different alignments based on column type
                    if (col == 1 || col == 8 || col == 9) // SN, Dr/Cr, Amount
                    {
                        worksheet.Cells[currentRow, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }
                    else // Text columns
                    {
                        worksheet.Cells[currentRow, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    }

                    worksheet.Cells[currentRow, col].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }

                currentRow++;
            }

            int dataEndRow = currentRow - 1;

            // ===== TOTALS ROW =====
            if (data.Any())
            {
                // Merge first 7 cells for "TOTALS:" label (matching image)
                worksheet.Cells[currentRow, 1, currentRow, 7].Merge = true;
                worksheet.Cells[currentRow, 1].Value = "TOTALS:";
                worksheet.Cells[currentRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
                worksheet.Cells[currentRow, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[currentRow, 1].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);

                worksheet.Cells[currentRow, 8].Value = ""; // Dr/Cr column
                worksheet.Cells[currentRow, 9].Value = data.Sum(x => x.Amount);

                // Style the totals row cells
                for (int col = 1; col <= headers.Length; col++)
                {
                    worksheet.Cells[currentRow, col].Style.Font.Bold = true;
                    worksheet.Cells[currentRow, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[currentRow, col].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                    if (col == 8 || col == 9) // Dr/Cr, Amount
                    {
                        worksheet.Cells[currentRow, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }

                    worksheet.Cells[currentRow, col].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }
            }

            // ===== FORMATTING =====

            // 1. Format numbers (matching image)
            if (data.Any())
            {
                // Format amount column (column I/9) with negative numbers in parentheses
                var amountRange = worksheet.Cells[$"I{dataStartRow}:I{dataEndRow + 1}"];
                amountRange.Style.Numberformat.Format = "#,##0.00;[Red](#,##0.00)";

                // Format date column to match image (yyyy-MM-dd HH:mm:ss)
                worksheet.Column(2).Style.Numberformat.Format = "yyyy-MM-dd HH:mm:ss";

                // Auto-fit numbers
                amountRange.AutoFitColumns();
            }

            // 2. Set optimal column widths (matching image proportions)
            worksheet.Column(1).Width = 8;   // SN
            worksheet.Column(2).Width = 18;  // Entry Date
            worksheet.Column(3).Width = 25;  // Reference
            worksheet.Column(4).Width = 18;  // Auxiliary Ref
            worksheet.Column(5).Width = 20;  // Branch
            worksheet.Column(6).Width = 15;  // Account
            worksheet.Column(7).Width = 30;  // Account Name
            worksheet.Column(8).Width = 10;  // Dr/Cr
            worksheet.Column(9).Width = 18;  // Amount

            // 3. Enable grid lines
            worksheet.View.ShowGridLines = true;

            // 4. Set zoom level for optimal viewing
            worksheet.View.ZoomScale = 100;

            // 5. Add autofilter for column sorting/filtering
            //if (data.Any())
            //{
            //    worksheet.Cells[$"A{headerRow}:{headerEndColumn}{headerRow}"].AutoFilter = true;
            //}

            // 6. Set print area for proper printing
            if (data.Any())
            {
                int printEndRow = data.Any() ? currentRow : headerRow;
                worksheet.PrinterSettings.PrintArea = worksheet.Cells[$"A1:{headerEndColumn}{printEndRow}"];
                worksheet.PrinterSettings.FitToPage = true;
                worksheet.PrinterSettings.FitToWidth = 1;
            }

            // 7. Alternate row shading for better readability
            if (data.Any())
            {
                for (int row = dataStartRow; row <= dataEndRow; row++)
                {
                    if (row % 2 == 0) // Even rows - light gray
                    {
                        for (int col = 1; col <= headers.Length; col++)
                        {
                            worksheet.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Cells[row, col].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(248, 248, 248));
                        }
                    }
                }
            }

            // 8. Wrap text for long account names
            worksheet.Column(7).Style.WrapText = true;

            // Auto-fit any columns that might have overflow
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            // 9. Set sheet view properties
            worksheet.View.ShowHeaders = true;
            worksheet.View.ShowGridLines = true;
        }
        private void CreateBranchAnalysisSheet(ExcelPackage package, List<Reconciled> data, string bank, string branchcode, string branchid, string branchname, string exportedBy, ExportOptions exportOptions)
        {
            var worksheet = package.Workbook.Worksheets.Add("Branch Analysis");

            // Set font
            var fontName = "Bahnschrift SemiCondensed";
            worksheet.Cells.Style.Font.Name = fontName;

            // ===== HEADER SECTION =====
            int headerColumns = 7;
            string headerEndColumn = GetColumnLetter(headerColumns);
            int currentRow = CreateHeaderSection(worksheet, bank, branchcode, branchid, branchname, exportedBy, exportOptions, "BRANCH ANALYSIS REPORT", headerEndColumn);

            // ===== BRANCH SUMMARY =====
            worksheet.Cells[$"A{currentRow}:{headerEndColumn}{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = "BRANCH-WISE SUMMARY";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Size = 14;
            worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
            currentRow += 2;

            // Group by branch - fix division by zero
            var branchSummary = data.GroupBy(x => new { x.BranchId, x.BranchName })
                                   .Select(g => new
                                   {
                                       BranchId = g.Key.BranchId,
                                       BranchName = g.Key.BranchName,
                                       TotalEntries = g.Count(),
                                       TotalDebit = g.Sum(x => x.DebitAmount),
                                       TotalCredit = g.Sum(x => x.CreditAmount),
                                       TotalAmount = g.Sum(x => x.Amount),
                                       AvgDebit = g.Count() > 0 ? g.Average(x => x.DebitAmount) : 0,
                                       AvgCredit = g.Count() > 0 ? g.Average(x => x.CreditAmount) : 0
                                   })
                                   .OrderByDescending(x => x.TotalEntries)
                                   .ToList();

            // Branch summary headers
            var headers = new[] { "SN", "Branch", "Total Entries", "Total Debit", "Total Credit", "Net Amount", "Avg Debit" };
            int headerRow = currentRow;
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[headerRow, i + 1].Value = headers[i];
                worksheet.Cells[headerRow, i + 1].Style.Font.Bold = true;
                worksheet.Cells[headerRow, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[headerRow, i + 1].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                worksheet.Cells[headerRow, i + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }
            currentRow++;

            // Branch summary data
            int sn = 1;
            foreach (var branch in branchSummary)
            {
                worksheet.Cells[currentRow, 1].Value = sn++;
                worksheet.Cells[currentRow, 2].Value = branch.BranchName ?? branch.BranchId;
                worksheet.Cells[currentRow, 3].Value = branch.TotalEntries;
                worksheet.Cells[currentRow, 4].Value = branch.TotalDebit;
                worksheet.Cells[currentRow, 5].Value = branch.TotalCredit;
                worksheet.Cells[currentRow, 6].Value = branch.TotalAmount;
                worksheet.Cells[currentRow, 7].Value = branch.AvgDebit;

                for (int col = 1; col <= headerColumns; col++)
                {
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
                currentRow++;
            }

            // Format numbers
            if (branchSummary.Any())
            {
                worksheet.Cells[$"D{headerRow + 1}:G{currentRow - 1}"].Style.Numberformat.Format = "#,##0.00";
            }

            // REMOVED: BRANCH PERFORMANCE INDICATORS section

            // Auto-fit columns
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        }

        private void CreateAccountAnalysisSheet(ExcelPackage package, List<Reconciled> data, string bank, string branchcode, string branchid, string branchname, string exportedBy, ExportOptions exportOptions)
        {
            var worksheet = package.Workbook.Worksheets.Add("Account Analysis");

            // Set font
            var fontName = "Bahnschrift SemiCondensed";
            worksheet.Cells.Style.Font.Name = fontName;

            // ===== HEADER SECTION =====
            int headerColumns = 8;
            string headerEndColumn = GetColumnLetter(headerColumns);
            int currentRow = CreateHeaderSection(worksheet, bank, branchcode, branchid, branchname, exportedBy, exportOptions, "ACCOUNT ANALYSIS REPORT", headerEndColumn);

            // ===== ACCOUNT SUMMARY =====
            worksheet.Cells[$"A{currentRow}:{headerEndColumn}{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = "ACCOUNT-WISE SUMMARY";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Size = 14;
            worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.BackgroundColor.SetColor(Color.LightSeaGreen);
            currentRow += 2;

            // Group by account
            var accountSummary = data.GroupBy(x => new { x.AccountNumber, x.AccountName })
                                    .Select(g => new
                                    {
                                        AccountNumber = g.Key.AccountNumber,
                                        AccountName = g.Key.AccountName,
                                        TotalEntries = g.Count(),
                                        TotalDebit = g.Sum(x => x.DebitAmount),
                                        TotalCredit = g.Sum(x => x.CreditAmount),
                                        TotalAmount = g.Sum(x => x.Amount),
                                        DrCount = g.Count(x => x.DrCr == "DR"),
                                        CrCount = g.Count(x => x.DrCr == "CR")
                                    })
                                    .OrderByDescending(x => x.TotalAmount)
                                    .Take(50) // Top 50 accounts
                                    .ToList();

            // Account summary headers
            var headers = new[] { "SN", "Account #", "Account Name", "Entries", "Total Debit", "Total Credit", "Net Balance", "Dr/Cr Ratio" };
            int headerRow = currentRow;
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[headerRow, i + 1].Value = headers[i];
                worksheet.Cells[headerRow, i + 1].Style.Font.Bold = true;
                worksheet.Cells[headerRow, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[headerRow, i + 1].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                worksheet.Cells[headerRow, i + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }
            currentRow++;

            // Account summary data
            int sn = 1;
            foreach (var account in accountSummary)
            {
                decimal ratio = account.DrCount > 0 ? (decimal)account.CrCount / account.DrCount : 0;

                worksheet.Cells[currentRow, 1].Value = sn++;
                worksheet.Cells[currentRow, 2].Value = account.AccountNumber ?? "N/A";
                worksheet.Cells[currentRow, 3].Value = account.AccountName ?? "N/A";
                worksheet.Cells[currentRow, 4].Value = account.TotalEntries;
                worksheet.Cells[currentRow, 5].Value = account.TotalDebit;
                worksheet.Cells[currentRow, 6].Value = account.TotalCredit;
                worksheet.Cells[currentRow, 7].Value = account.TotalAmount;
                worksheet.Cells[currentRow, 8].Value = ratio;

                for (int col = 1; col <= headerColumns; col++)
                {
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
                currentRow++;
            }

            // Format numbers
            if (accountSummary.Any())
            {
                worksheet.Cells[$"E{headerRow + 1}:G{currentRow - 1}"].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[$"H{headerRow + 1}:H{currentRow - 1}"].Style.Numberformat.Format = "0.00";
            }

            // Auto-fit columns
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        }

        private void CreateInterbranchSheet(ExcelPackage package, List<Reconciled> data, string bank, string branchcode, string branchid, string branchname, string exportedBy, ExportOptions exportOptions)
        {
            var worksheet = package.Workbook.Worksheets.Add("Interbranch Transactions");

            // Set font
            var fontName = "Bahnschrift SemiCondensed";
            worksheet.Cells.Style.Font.Name = fontName;

            // ===== HEADER SECTION =====
            int headerColumns = 7; // Reduced to match the image (SN, From Branch, To Branch, Transactions, Total Amount, Avg Amount, Last Transaction)
            string headerEndColumn = GetColumnLetter(headerColumns);
            int currentRow = CreateHeaderSection(worksheet, bank, branchcode, branchid, branchname, exportedBy, exportOptions, "INTERBRANCH TRANSACTIONS REPORT", headerEndColumn);

            // ===== INTERBRANCH SUMMARY =====
            // MERGE ALL COLUMNS FOR TITLE
            worksheet.Cells[$"A{currentRow}:{headerEndColumn}{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = "INTERBRANCH TRANSACTIONS";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Size = 14;
            worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.BackgroundColor.SetColor(Color.Gold);
            currentRow += 2;

            // Group by branch pairs
            var interbranchSummary = data.Where(x => x.InterbranchStatus)
                .GroupBy(x => new {
                    FromBranch = x.BranchName,
                    ToBranch = x.CounterpartyBranchName
                })
                .Select(g => new
                {
                    FromBranch = g.Key.FromBranch,
                    ToBranch = g.Key.ToBranch,
                    TotalTransactions = g.Count(),
                    TotalAmount = g.Sum(x => x.Amount),
                    AvgAmount = g.Count() > 0 ? g.Average(x => x.Amount) : 0,
                    LastTransactionDate = g.Max(x => x.EntryDate)
                })
                .OrderByDescending(x => x.TotalAmount)
                .ToList();

            // Interbranch summary headers - MATCHING IMAGE COLUMNS
            var headers = new[] { "SN", "From Branch", "To Branch", "Transactions", "Total Amount", "Avg Amount", "Last Transaction" };
            int headerRow = currentRow;

            // Set header row height for better scrolling
            worksheet.Row(headerRow).Height = 35;

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[headerRow, i + 1].Value = headers[i];
                worksheet.Cells[headerRow, i + 1].Style.Font.Bold = true;
                worksheet.Cells[headerRow, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[headerRow, i + 1].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                worksheet.Cells[headerRow, i + 1].Style.Border.BorderAround(ExcelBorderStyle.Thick);
                worksheet.Cells[headerRow, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[headerRow, i + 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Cells[headerRow, i + 1].Style.WrapText = true; // Enable text wrapping
            }
            currentRow++;

            // Interbranch summary data
            int sn = 1;
            foreach (var transaction in interbranchSummary)
            {
                // Set row height for data rows
                worksheet.Row(currentRow).Height = 25;

                worksheet.Cells[currentRow, 1].Value = sn++;
                worksheet.Cells[currentRow, 2].Value = transaction.FromBranch ?? "N/A";
                worksheet.Cells[currentRow, 3].Value = transaction.ToBranch ?? "N/A";
                worksheet.Cells[currentRow, 4].Value = transaction.TotalTransactions;
                worksheet.Cells[currentRow, 5].Value = transaction.TotalAmount;
                worksheet.Cells[currentRow, 6].Value = transaction.AvgAmount;
                worksheet.Cells[currentRow, 7].Value = transaction.LastTransactionDate.ToString("yyyy-MM-dd HH:mm:ss");

                // Center align all cells and add borders
                for (int col = 1; col <= headers.Length; col++)
                {
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    worksheet.Cells[currentRow, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[currentRow, col].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }
                currentRow++;
            }

            // Format numbers
            if (interbranchSummary.Any())
            {
                worksheet.Cells[$"E{headerRow + 1}:F{currentRow - 1}"].Style.Numberformat.Format = "#,##0.00";
            }

            // ===== SCROLL ARRANGEMENT ENHANCEMENTS =====

            // 1. Set column widths for optimal scrolling
            worksheet.Column(1).Width = 8;  // SN
            worksheet.Column(2).Width = 30; // From Branch
            worksheet.Column(3).Width = 25; // To Branch
            worksheet.Column(4).Width = 15; // Transactions
            worksheet.Column(5).Width = 20; // Total Amount
            worksheet.Column(6).Width = 15; // Avg Amount
            worksheet.Column(7).Width = 25; // Last Transaction

            // 2. Freeze header row for easy scrolling
            worksheet.View.FreezePanes(headerRow + 1, 1);

            // 3. Set zoom level for better viewing
            worksheet.View.ZoomScale = 100;

            // 4. Enable selection of entire rows for easier scrolling
            worksheet.View.ShowGridLines = true;

            // 5. Add filter for column sorting/filtering
            //if (interbranchSummary.Any())
            //{
            //    worksheet.Cells[$"A{headerRow}:{headerEndColumn}{headerRow}"].AutoFilter = true;
            //}

            // 6. Set print area for scrolling
            worksheet.PrinterSettings.FitToPage = true;
            worksheet.PrinterSettings.FitToWidth = 1;

            // Auto-fit remaining columns if needed
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        }

        private void CreateDebitCreditRatioSection(ExcelWorksheet worksheet, List<Reconciled> data, ref int currentRow, string headerEndColumn)
        {
            worksheet.Cells[$"A{currentRow}:{headerEndColumn}{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = "DEBIT/CREDIT ANALYSIS";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Size = 14;
            worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.BackgroundColor.SetColor(Color.LightSalmon);
            currentRow += 2;

            var debitCount = data.Count(x => x.DrCr == "DR");
            var creditCount = data.Count(x => x.DrCr == "CR");
            var totalDebitAmount = data.Where(x => x.DrCr == "DR").Sum(x => x.Amount);
            var totalCreditAmount = data.Where(x => x.DrCr == "CR").Sum(x => x.Amount);

            var ratioHeaders = new[] { "Type", "Count", "Percentage", "Total Amount", "Average Amount" };
            for (int i = 0; i < ratioHeaders.Length; i++)
            {
                worksheet.Cells[currentRow, 1 + i].Value = ratioHeaders[i];
                worksheet.Cells[currentRow, 1 + i].Style.Font.Bold = true;
                worksheet.Cells[currentRow, 1 + i].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[currentRow, 1 + i].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                worksheet.Cells[currentRow, 1 + i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }
            currentRow++;

            var ratioData = new[]
            {
            new { Type = "DEBIT (DR)", Count = debitCount, Amount = totalDebitAmount },
            new { Type = "CREDIT (CR)", Count = creditCount, Amount = totalCreditAmount },
            new { Type = "TOTAL", Count = debitCount + creditCount, Amount = totalDebitAmount + totalCreditAmount }
        };

            foreach (var item in ratioData)
            {
                decimal percentage = data.Count > 0 ? (decimal)item.Count / data.Count : 0;
                decimal avgAmount = item.Count > 0 ? item.Amount / item.Count : 0;

                worksheet.Cells[currentRow, 1].Value = item.Type;
                worksheet.Cells[currentRow, 2].Value = item.Count;
                worksheet.Cells[currentRow, 3].Value = percentage;
                worksheet.Cells[currentRow, 4].Value = item.Amount;
                worksheet.Cells[currentRow, 5].Value = avgAmount;

                for (int col = 1; col <= 5; col++)
                {
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
                currentRow++;
            }

            // Format percentages and numbers
            worksheet.Cells[$"C{currentRow - 3}:C{currentRow - 1}"].Style.Numberformat.Format = "0.0%";
            worksheet.Cells[$"D{currentRow - 3}:E{currentRow - 1}"].Style.Numberformat.Format = "#,##0.00";
        }

        private void CreateTopAccountsSection(ExcelWorksheet worksheet, List<Reconciled> data, ref int currentRow, string headerEndColumn)
        {
            worksheet.Cells[$"A{currentRow}:{headerEndColumn}{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = "TOP 10 ACCOUNTS BY TRANSACTION VOLUME";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Size = 14;
            worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.BackgroundColor.SetColor(Color.LightPink);
            currentRow += 2;

            // Fix division by zero
            var topAccounts = data.GroupBy(x => new { x.AccountNumber, x.AccountName })
                                 .Select(g => new
                                 {
                                     AccountNumber = g.Key.AccountNumber,
                                     AccountName = g.Key.AccountName,
                                     TransactionCount = g.Count(),
                                     TotalAmount = g.Sum(x => x.Amount),
                                     AvgAmount = g.Count() > 0 ? g.Average(x => x.Amount) : 0
                                 })
                                 .OrderByDescending(x => x.TotalAmount)
                                 .Take(10)
                                 .ToList();

            var accountHeaders = new[] { "Rank", "Account #", "Account Name", "Transactions", "Total Amount", "Avg Amount", "% of Total" };
            for (int i = 0; i < accountHeaders.Length; i++)
            {
                worksheet.Cells[currentRow, 1 + i].Value = accountHeaders[i];
                worksheet.Cells[currentRow, 1 + i].Style.Font.Bold = true;
                worksheet.Cells[currentRow, 1 + i].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[currentRow, 1 + i].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                worksheet.Cells[currentRow, 1 + i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }
            currentRow++;

            decimal totalAllAmount = data.Sum(x => x.Amount);
            int rank = 1;
            foreach (var account in topAccounts)
            {
                decimal percentage = totalAllAmount > 0 ? account.TotalAmount / totalAllAmount : 0;

                worksheet.Cells[currentRow, 1].Value = rank++;
                worksheet.Cells[currentRow, 2].Value = account.AccountNumber ?? "N/A";
                worksheet.Cells[currentRow, 3].Value = account.AccountName ?? "N/A";
                worksheet.Cells[currentRow, 4].Value = account.TransactionCount;
                worksheet.Cells[currentRow, 5].Value = account.TotalAmount;
                worksheet.Cells[currentRow, 6].Value = account.AvgAmount;
                worksheet.Cells[currentRow, 7].Value = percentage;

                for (int col = 1; col <= 7; col++)
                {
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
                currentRow++;
            }

            // Format numbers and percentages
            if (topAccounts.Any())
            {
                int count = topAccounts.Count;
                worksheet.Cells[$"E{currentRow - count}:F{currentRow - 1}"].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[$"G{currentRow - count}:G{currentRow - 1}"].Style.Numberformat.Format = "0.0%";
            }
        }

        private int CreateHeaderSection(ExcelWorksheet worksheet, string bank, string branchcode, string branchid, string branchname, string exportedBy, ExportOptions exportOptions, string reportTitle, string headerEndColumn)
        {
            // ===== Bank Information at the TOP =====
            worksheet.Cells[$"A1:{headerEndColumn}1"].Merge = true;
            worksheet.Cells["A1"].Value = bank;
            worksheet.Cells["A1"].Style.Font.Bold = true;
            worksheet.Cells["A1"].Style.Font.Size = 18;
            worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells["A1"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Row(1).Height = 30;
            worksheet.Cells["A1"].Style.Font.Color.SetColor(Color.White);
            worksheet.Cells["A1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["A1"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 100, 0));

            // ===== Bank Code and Branch Information =====
            worksheet.Cells[$"A2:{headerEndColumn}2"].Merge = true;
            worksheet.Cells["A2"].Value = $"Branch Code: {branchcode} | Branch: {branchname} | Branch ID: {branchid}";
            worksheet.Cells["A2"].Style.Font.Bold = true;
            worksheet.Cells["A2"].Style.Font.Size = 12;
            worksheet.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells["A2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Row(2).Height = 22;
            worksheet.Cells["A2"].Style.Font.Color.SetColor(Color.White);
            worksheet.Cells["A2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["A2"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(70, 130, 180));

            // ===== Export meta =====
            worksheet.Cells[$"A3:{headerEndColumn}3"].Merge = true;
            worksheet.Cells["A3"].Value = $"Exported By: {exportedBy}";
            worksheet.Cells["A3"].Style.Font.Bold = true;
            worksheet.Cells["A3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells["A3"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Row(3).Height = 18;

            worksheet.Cells[$"A4:{headerEndColumn}4"].Merge = true;
            worksheet.Cells["A4"].Value = $"Export Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            worksheet.Cells["A4"].Style.Font.Bold = true;
            worksheet.Cells["A4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells["A4"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Row(4).Height = 18;

            // ===== Report Title =====
            worksheet.Cells[$"A5:{headerEndColumn}5"].Merge = true;
            worksheet.Cells["A5"].Value = reportTitle;
            worksheet.Cells["A5"].Style.Font.Bold = true;
            worksheet.Cells["A5"].Style.Font.Size = 14;
            worksheet.Cells["A5"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells["A5"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Row(5).Height = 25;
            worksheet.Cells["A5"].Style.Font.Color.SetColor(Color.Black);
            worksheet.Cells["A5"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["A5"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 215, 0));

            // Optional date range row
            if (exportOptions != null && !string.IsNullOrEmpty(exportOptions.StartDate) && !string.IsNullOrEmpty(exportOptions.EndDate))
            {
                worksheet.Cells[$"A6:{headerEndColumn}6"].Merge = true;
                worksheet.Cells["A6"].Value = $"Date Range: {exportOptions.StartDate} to {exportOptions.EndDate}";
                worksheet.Cells["A6"].Style.Font.Bold = true;
                worksheet.Cells["A6"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                worksheet.Cells["A6"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Row(6).Height = 18;
                return 8; // Return the starting row for content
            }

            return 7; // Return the starting row for content
        }

        // Helper method to convert column number to letter
        private string GetColumnLetter(int columnNumber)
        {
            string columnLetter = "";
            while (columnNumber > 0)
            {
                int modulo = (columnNumber - 1) % 26;
                columnLetter = Convert.ToChar('A' + modulo) + columnLetter;
                columnNumber = (columnNumber - modulo) / 26;
            }
            return columnLetter;
        }
    }

}