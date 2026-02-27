using BusinessServices;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Queries;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Reporting.FlatBaseE;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace CBS.FrontDesk.UI.Controllers.Accounting_V2.AccntStatement
{
    public class AccountStatementExcelExportGenerator : BaseService
    {
        // -----------------------------------------------------------------------
        // PUBLIC CONVERSION METHOD
        // -----------------------------------------------------------------------
        public static List<AccountStatementFlatItems> ConvertToAccountStatementData(List<AccountStatementFlatItems> data)
        {
            return data ?? new List<AccountStatementFlatItems>();
        }

        // -----------------------------------------------------------------------
        // MAIN GENERATION METHOD
        // -----------------------------------------------------------------------
        public void GenerateAccountStatementExcel(
            List<AccountStatementFlatItems> data,
            string filePath,
            string exportedBy,
            AccountingV2ReportsFilter filter,
            BankHeaderInformation headerInfo)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                CreateSummarySheet(package, data, headerInfo, exportedBy, filter);
                CreateBranchSummarySheet(package, data, headerInfo, exportedBy, filter);
                CreateAccountDetailsSheet(package, data, headerInfo, exportedBy, filter);

                // Group by branch for per‑branch sheets
                var branches = data
                    .Select(x => new
                    {
                        BranchId = x.BranchId ?? "Unknown",
                        BranchName = string.IsNullOrWhiteSpace(x.BranchName) ? "Unknown Branch" : x.BranchName,
                        BranchCode = x.BranchCode ?? ""
                    })
                    .GroupBy(b => new { b.BranchId, b.BranchName, b.BranchCode })
                    .Select(g => new { g.Key.BranchId, g.Key.BranchName, g.Key.BranchCode })
                    .ToList();

                foreach (var branch in branches)
                {
                    var branchData = data.Where(x =>
                        (x.BranchId ?? "Unknown") == branch.BranchId &&
                        (string.IsNullOrWhiteSpace(x.BranchName) ? "Unknown Branch" : x.BranchName) == branch.BranchName)
                        .ToList();

                    CreateBranchSheet(package, branchData, headerInfo,
                        branch.BranchId, branch.BranchName, branch.BranchCode,
                        exportedBy, filter);
                }

                package.SaveAs(new FileInfo(filePath));
            }
        }

        // -----------------------------------------------------------------------
        // SHEET: SUMMARY & OVERVIEW
        // -----------------------------------------------------------------------
        private void CreateSummarySheet(
      ExcelPackage package,
      List<AccountStatementFlatItems> data,
      BankHeaderInformation headerInfo,
      string exportedBy,
      AccountingV2ReportsFilter filter)
        {
            var worksheet = package.Workbook.Worksheets.Add("Summary & Overview");
            ApplyDefaultStyle(worksheet);

            int headerColumns = 4;
            string headerEndColumn = GetColumnLetter(headerColumns);
            int currentRow = CreateHeaderSection(worksheet, headerInfo, exportedBy, filter,
                "GENERAL ACCOUNTING JOURNAL SUMMARY", headerEndColumn);

            // Add a small gap after header
            currentRow += 1;

            int totalTransactions = data.Count;
            int totalBranches = data.Select(x => x.BranchId).Distinct().Count();
            decimal totalDebit = data.Sum(x => x.DebitAmount);
            decimal totalCredit = data.Sum(x => x.CreditAmount);
            decimal netMovement = totalCredit - totalDebit;
            decimal closingBalance = data.OrderByDescending(x => x.Seq).FirstOrDefault()?.Balance ?? 0;

            var summaryData = new[]
            {
        new { Metric = "Total Transactions", Value = totalTransactions.ToString("N0"), Description = "Number of journal movements" },
        new { Metric = "Branches Involved", Value = totalBranches.ToString("N0"), Description = "Distinct branches" },
        new { Metric = "Total Debit (DR)", Value = totalDebit.ToString("N2"), Description = "Sum of all debit amounts" },
        new { Metric = "Total Credit (CR)", Value = totalCredit.ToString("N2"), Description = "Sum of all credit amounts" },
        new { Metric = "Net Movement", Value = netMovement.ToString("N2"), Description = "Credit - Debit" }
        //new { Metric = "Closing Balance", Value = closingBalance.ToString("N2"), Description = "Balance of last transaction" }
    };

            currentRow = CreateTwoColumnTable(worksheet, currentRow, summaryData, "Metric", "Value", "Description");
            currentRow += 1;

            // ----- TRANSACTION TYPE DISTRIBUTION (DR/CR) -----
            var typeDistribution = data
                .GroupBy(x => string.IsNullOrEmpty(x.DrCr) ? "Unknown" : x.DrCr)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            currentRow = CreateDistributionSection(worksheet, currentRow, headerEndColumn,
                "TRANSACTION TYPE DISTRIBUTION", typeDistribution, totalTransactions, Color.LightYellow);
            currentRow += 1;

            // ----- ALL GL ACCOUNTS IMPACTED -----
            var allAccounts = data
                .Where(x => !string.IsNullOrEmpty(x.AccountNumber))
                .GroupBy(x => new { x.AccountNumber, x.AccountName })
                .Select(g => new {
                    Account = $"{g.Key.AccountNumber} - {g.Key.AccountName}",
                    Count = g.Count(),
                    TotalDebit = g.Sum(x => x.DebitAmount),
                    TotalCredit = g.Sum(x => x.CreditAmount)
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            if (allAccounts.Any())
            {
                worksheet.Cells[$"A{currentRow}:{headerEndColumn}{currentRow}"].Merge = true;
                worksheet.Cells[$"A{currentRow}"].Value = "GL ACCOUNTS IMPACTED";
                SetSectionHeaderStyle(worksheet.Cells[$"A{currentRow}"], Color.LightGreen);
                currentRow += 2;

                // Expanded headers to include debit/credit totals
                var headers = new[] { "Account", "Transaction Count", "Total Debit", "Total Credit" };

                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[currentRow, i + 1].Value = headers[i];
                    SetHeaderStyle(worksheet.Cells[currentRow, i + 1], Color.LightGreen);
                }
                currentRow++;

                decimal grandTotalDebit = 0;
                decimal grandTotalCredit = 0;

                foreach (var acc in allAccounts)
                {
                    worksheet.Cells[currentRow, 1].Value = acc.Account;
                    worksheet.Cells[currentRow, 2].Value = acc.Count;
                    worksheet.Cells[currentRow, 3].Value = acc.TotalDebit;
                    worksheet.Cells[currentRow, 4].Value = acc.TotalCredit;

                    for (int col = 1; col <= 4; col++)
                        worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                    grandTotalDebit += acc.TotalDebit;
                    grandTotalCredit += acc.TotalCredit;
                    currentRow++;
                }

                // Add grand totals row
                worksheet.Cells[currentRow, 1].Value = "GRAND TOTALS:";
                worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
                worksheet.Cells[currentRow, 2].Value = allAccounts.Sum(x => x.Count);
                worksheet.Cells[currentRow, 3].Value = grandTotalDebit;
                worksheet.Cells[currentRow, 4].Value = grandTotalCredit;

                for (int col = 1; col <= 4; col++)
                {
                    worksheet.Cells[currentRow, col].Style.Font.Bold = true;
                    worksheet.Cells[currentRow, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[currentRow, col].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                // Format the debit/credit columns
                worksheet.Cells[$"C{currentRow - allAccounts.Count}:D{currentRow}"].Style.Numberformat.Format = "#,##0.00";
            }

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        }

        // -----------------------------------------------------------------------
        // SHEET: BRANCH SUMMARY
        // -----------------------------------------------------------------------
        private void CreateBranchSummarySheet(
      ExcelPackage package,
      List<AccountStatementFlatItems> data,
      BankHeaderInformation headerInfo,
      string exportedBy,
      AccountingV2ReportsFilter filter)
        {
            var worksheet = package.Workbook.Worksheets.Add("Branch Summary");
            ApplyDefaultStyle(worksheet);

            int headerColumns = 7;
            string headerEndColumn = GetColumnLetter(headerColumns);
            int currentRow = CreateHeaderSection(worksheet, headerInfo, exportedBy, filter,
                "BRANCH SUMMARY REPORT", headerEndColumn);

            // Add a small gap after header
            currentRow += 1;

            var branchHeaders = new[]
            {
        "Branch ID", "Branch Code", "Branch Name",
        "# Transactions", "Total Debit (DR)", "Total Credit (CR)", "Net Movement"
    };

            for (int i = 0; i < branchHeaders.Length; i++)
            {
                worksheet.Cells[currentRow, i + 1].Value = branchHeaders[i];
                SetHeaderStyle(worksheet.Cells[currentRow, i + 1], Color.LightGreen);
            }
            currentRow++;

            // Use headerInfo for branch details instead of data values
            string branchId = headerInfo.BranchId ?? "Unknown";
            string branchCode = headerInfo.BranchCode ?? "";
            string branchName = headerInfo.BranchName ?? "Unknown Branch";

            var branchSummary = data
                .GroupBy(x => new
                {
                    BranchId = branchId,
                    BranchCode = branchCode,
                    BranchName = branchName
                })
                .Select(g => new
                {
                    BranchId = branchId,
                    BranchCode = branchCode,
                    BranchName = branchName,
                    TransactionCount = g.Count(),
                    TotalDebit = g.Sum(x => x.DebitAmount),
                    TotalCredit = g.Sum(x => x.CreditAmount)
                })
                .ToList();

            foreach (var branch in branchSummary)
            {
                worksheet.Cells[currentRow, 1].Value = branch.BranchId;
                worksheet.Cells[currentRow, 2].Value = branch.BranchCode;
                worksheet.Cells[currentRow, 3].Value = branch.BranchName;
                worksheet.Cells[currentRow, 4].Value = branch.TransactionCount;
                worksheet.Cells[currentRow, 5].Value = branch.TotalDebit;
                worksheet.Cells[currentRow, 6].Value = branch.TotalCredit;
                worksheet.Cells[currentRow, 7].Value = branch.TotalCredit - branch.TotalDebit;

                for (int col = 1; col <= headerColumns; col++)
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                currentRow++;
            }

            if (branchSummary.Any())
            {
                worksheet.Cells[currentRow, 1].Value = "TOTALS:";
                worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
                worksheet.Cells[currentRow, 4].Value = branchSummary.Sum(x => x.TransactionCount);
                worksheet.Cells[currentRow, 5].Value = branchSummary.Sum(x => x.TotalDebit);
                worksheet.Cells[currentRow, 6].Value = branchSummary.Sum(x => x.TotalCredit);
                worksheet.Cells[currentRow, 7].Value = branchSummary.Sum(x => x.TotalCredit - x.TotalDebit);

                for (int col = 1; col <= headerColumns; col++)
                {
                    worksheet.Cells[currentRow, col].Style.Font.Bold = true;
                    worksheet.Cells[currentRow, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[currentRow, col].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
            }

            worksheet.Cells[$"E{5}:G{currentRow}"].Style.Numberformat.Format = "#,##0.00";
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        }

        // -----------------------------------------------------------------------
        // SHEET: ACCOUNT DETAILS (ALL TRANSACTIONS) - UPDATED COLUMN ORDER
        // -----------------------------------------------------------------------
        private void CreateAccountDetailsSheet(
     ExcelPackage package,
     List<AccountStatementFlatItems> data,
     BankHeaderInformation headerInfo,
     string exportedBy,
     AccountingV2ReportsFilter filter)
        {
            var worksheet = package.Workbook.Worksheets.Add("Account Details");
            ApplyDefaultStyle(worksheet);

            // Set header columns to 10 for the new layout
            int headerColumns = 10;
            string headerEndColumn = GetColumnLetter(headerColumns);
            int currentRow = CreateHeaderSection(worksheet, headerInfo, exportedBy, filter,
                "GENERAL ACCOUNTING JOURNAL", headerEndColumn);

            // Add a small gap after header
            

            // NEW HEADER ORDER as requested - updated column names
            var headers = new[]
            {
        "SN",
        "DATE",
        "TIME",
        "ACCOUNT No",
        "ACCOUNT NAME",
        "AUX.REF",
        "REFERENCE",
        "NARATION",
        "DEBIT (DR)",
        "CREDIT (CR)"
    };

            int headerColumnsCount = headers.Length;
            headerEndColumn = GetColumnLetter(headerColumnsCount);

            int headerRow = currentRow;
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[headerRow, i + 1].Value = headers[i];
                SetHeaderStyle(worksheet.Cells[headerRow, i + 1], Color.LightGreen);

                // Enable wrap text for header cells as well (optional)
                worksheet.Cells[headerRow, i + 1].Style.WrapText = true;
            }
            currentRow++;

            int sn = 1;
            decimal totalDebit = 0, totalCredit = 0;

            // Filter out any rows that might be header text masquerading as data
            var validData = data.Where(x =>
                !string.IsNullOrEmpty(x.AccountNumber) ||
                !string.IsNullOrEmpty(x.ReferenceNumber) ||
                x.DebitAmount != 0 ||
                x.CreditAmount != 0).ToList();

            int firstDataRow = currentRow;

            foreach (var item in validData.OrderBy(x => x.Seq))
            {
                worksheet.Cells[currentRow, 1].Value = sn++;
                worksheet.Cells[currentRow, 2].Value = item.AccountingDate;
                worksheet.Cells[currentRow, 3].Value = item.time.ToString(@"hh\:mm\:ss");
                worksheet.Cells[currentRow, 4].Value = item.AccountNumber;
                worksheet.Cells[currentRow, 5].Value = item.AccountName;
                worksheet.Cells[currentRow, 6].Value = item.AuxiliaryRef; // AUX.REF
                worksheet.Cells[currentRow, 7].Value = item.ReferenceNumber; // REFERENCE
                worksheet.Cells[currentRow, 8].Value = item.Description; // DESCRIPTION
                worksheet.Cells[currentRow, 9].Value = item.DebitAmount; // DEBIT (DR)
                worksheet.Cells[currentRow, 10].Value = item.CreditAmount; // CREDIT (CR)

                // Set vertical alignment to top for better readability with wrapped text
                worksheet.Cells[currentRow, 8].Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                for (int col = 1; col <= headerColumnsCount; col++)
                {
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    // Set vertical alignment to center for non-description columns
                    if (col != 8)
                        worksheet.Cells[currentRow, col].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }

                totalDebit += item.DebitAmount;
                totalCredit += item.CreditAmount;
                currentRow++;
            }

            // Apply text wrapping to all description cells in one go
            if (validData.Any())
            {
                string descriptionColumnRange = $"H{firstDataRow}:H{currentRow - 1}";
                worksheet.Cells[descriptionColumnRange].Style.WrapText = true;
            }

            if (validData.Any())
            {
                worksheet.Cells[currentRow, 1].Value = "TOTALS:";
                worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
                worksheet.Cells[currentRow, 9].Value = totalDebit;
                worksheet.Cells[currentRow, 10].Value = totalCredit;

                for (int col = 1; col <= headerColumnsCount; col++)
                {
                    worksheet.Cells[currentRow, col].Style.Font.Bold = true;
                    worksheet.Cells[currentRow, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[currentRow, col].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    worksheet.Cells[currentRow, col].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }
            }

            // Set column widths for optimal display
            worksheet.Column(1).Width = 6;   // SN
            worksheet.Column(2).Width = 12;  // DATE
            worksheet.Column(3).Width = 10;  // TIME
            worksheet.Column(4).Width = 18;  // ACCOUNT No
            worksheet.Column(5).Width = 35;  // ACCOUNT NAME
            worksheet.Column(6).Width = 18;  // AUX.REF
            worksheet.Column(7).Width = 22;  // REFERENCE
            worksheet.Column(8).Width = 40;  // DESCRIPTION (with wrap)
            worksheet.Column(9).Width = 15;  // DEBIT (DR)
            worksheet.Column(10).Width = 15; // CREDIT (CR)

            // Enable auto-fit for rows to accommodate wrapped text
            if (validData.Any())
            {
                for (int row = firstDataRow; row <= currentRow; row++)
                {
                    worksheet.Row(row).Height = -1; // Auto-fit row height
                }
            }

            worksheet.Cells[$"I{headerRow + 1}:J{currentRow}"].Style.Numberformat.Format = "#,##0.00";
            worksheet.View.FreezePanes(headerRow + 1, 1);
        }

        // -----------------------------------------------------------------------
        // SHEET: INDIVIDUAL BRANCH DETAILS - UPDATED COLUMN ORDER
        // -----------------------------------------------------------------------
        private void CreateBranchSheet(
     ExcelPackage package,
     List<AccountStatementFlatItems> branchData,
     BankHeaderInformation headerInfo,
     string branchId,
     string branchName,
     string branchCode,
     string exportedBy,
     AccountingV2ReportsFilter filter)
        {
            // Use header's branch name if the data's branch name is missing or "Unknown Branch"
            string safeBranchName;
            if (string.IsNullOrWhiteSpace(branchName) || branchName == "Unknown Branch")
            {
                safeBranchName = headerInfo?.BranchName ?? "Unknown Branch";
            }
            else
            {
                safeBranchName = branchName;
            }

            string sheetName = CleanSheetName(safeBranchName);
            if (string.IsNullOrWhiteSpace(sheetName)) sheetName = "Branch";
            if (sheetName.Length > 31) sheetName = sheetName.Substring(0, 31);

            var worksheet = package.Workbook.Worksheets.Add(sheetName);
            ApplyDefaultStyle(worksheet);

            // Set header columns to 10 for the new layout
            int headerColumns = 10;
            string headerEndColumn = GetColumnLetter(headerColumns);
            int currentRow = CreateHeaderSection(worksheet, headerInfo, exportedBy, filter,
                $"GENERAL ACCOUNTING JOURNAL - {safeBranchName}", headerEndColumn);

            // Add a small gap after header (instead of the removed DETAILED TRANSACTIONS header)
           

            // NEW HEADER ORDER as requested
            var branchHeaders = new[]
            {
        "SN",
        "DATE",
        "TIME",
        "ACCOUNT No",
        "ACCOUNT NAME",
        "AUX.REF",
        "REFERENCE",
        "NARATION",
        "DEBIT (DR)",
        "CREDIT (CR)"
    };

            int headerColumnsCount = branchHeaders.Length;
            headerEndColumn = GetColumnLetter(headerColumnsCount);

            int headerRow = currentRow;
            for (int i = 0; i < branchHeaders.Length; i++)
            {
                worksheet.Cells[headerRow, i + 1].Value = branchHeaders[i];
                SetHeaderStyle(worksheet.Cells[headerRow, i + 1], Color.LightGreen);
                // Enable wrap text for header cells as well
                worksheet.Cells[headerRow, i + 1].Style.WrapText = true;
            }
            currentRow++;

            int sn = 1;
            decimal totalDebit = 0, totalCredit = 0;

            // Filter out any rows that might be header text
            var validData = branchData.Where(x =>
                !string.IsNullOrEmpty(x.AccountNumber) ||
                !string.IsNullOrEmpty(x.ReferenceNumber) ||
                x.DebitAmount != 0 ||
                x.CreditAmount != 0).ToList();

            int firstDataRow = currentRow;

            foreach (var item in validData.OrderBy(x => x.Seq))
            {
                worksheet.Cells[currentRow, 1].Value = sn++;
                worksheet.Cells[currentRow, 2].Value = item.AccountingDate;
                worksheet.Cells[currentRow, 3].Value = item.time.ToString(@"hh\:mm\:ss");
                worksheet.Cells[currentRow, 4].Value = item.AccountNumber;
                worksheet.Cells[currentRow, 5].Value = item.AccountName;
                worksheet.Cells[currentRow, 6].Value = item.AuxiliaryRef; // AUX.REF
                worksheet.Cells[currentRow, 7].Value = item.ReferenceNumber; // REFERENCE
                worksheet.Cells[currentRow, 8].Value = item.Description; // NARATION
                worksheet.Cells[currentRow, 9].Value = item.DebitAmount; // DEBIT (DR)
                worksheet.Cells[currentRow, 10].Value = item.CreditAmount; // CREDIT (CR)

                // Set vertical alignment to top for description cells
                worksheet.Cells[currentRow, 8].Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                for (int col = 1; col <= headerColumnsCount; col++)
                {
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    // Set vertical alignment to center for non-description columns
                    if (col != 8)
                        worksheet.Cells[currentRow, col].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }

                totalDebit += item.DebitAmount;
                totalCredit += item.CreditAmount;
                currentRow++;
            }

            // Apply text wrapping to all description cells in one go
            if (validData.Any())
            {
                string descriptionColumnRange = $"H{firstDataRow}:H{currentRow - 1}";
                worksheet.Cells[descriptionColumnRange].Style.WrapText = true;
            }

            // Totals row
            if (validData.Any())
            {
                worksheet.Cells[currentRow, 1].Value = "TOTALS:";
                worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
                worksheet.Cells[currentRow, 9].Value = totalDebit;
                worksheet.Cells[currentRow, 10].Value = totalCredit;

                for (int col = 1; col <= headerColumnsCount; col++)
                {
                    worksheet.Cells[currentRow, col].Style.Font.Bold = true;
                    worksheet.Cells[currentRow, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[currentRow, col].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    worksheet.Cells[currentRow, col].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }
                currentRow += 2;
            }
            else
            {
                currentRow += 2;
            }

            // ----- BRANCH SUMMARY (TWO-COLUMN METRICS) -----
            worksheet.Cells[$"A{currentRow}:{headerEndColumn}{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = $"BRANCH SUMMARY - {safeBranchName}";
            SetSectionHeaderStyle(worksheet.Cells[$"A{currentRow}"], Color.LightBlue);
            currentRow += 2;

            int total = validData.Count;
            decimal sumDebit = validData.Sum(x => x.DebitAmount);
            decimal sumCredit = validData.Sum(x => x.CreditAmount);
            decimal net = sumCredit - sumDebit;
            decimal closingBal = validData.OrderByDescending(x => x.Seq).FirstOrDefault()?.Balance ?? 0;

            var summaryItems = new[]
            {
        new { Metric = "Total Entries", Value = total.ToString("N0") },
        new { Metric = "Total Debit", Value = sumDebit.ToString("N2") },
        new { Metric = "Total Credit", Value = sumCredit.ToString("N2") },
        new { Metric = "Net Movement", Value = net.ToString("N2") }
    };

            currentRow = CreateTwoColumnTable(worksheet, currentRow, summaryItems, "Metric", "Value");
            currentRow += 2;

            // ----- DR/CR DISTRIBUTION -----
            var typeDist = validData
                .GroupBy(x => string.IsNullOrEmpty(x.DrCr) ? "Unknown" : x.DrCr)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToList();

            currentRow = CreateDistributionSection(worksheet, currentRow, headerEndColumn,
                "TRANSACTION TYPE DISTRIBUTION", typeDist, total, Color.LightYellow);

            // Set column widths for optimal display
            worksheet.Column(1).Width = 6;   // SN
            worksheet.Column(2).Width = 12;  // DATE
            worksheet.Column(3).Width = 10;  // TIME
            worksheet.Column(4).Width = 18;  // ACCOUNT No
            worksheet.Column(5).Width = 35;  // ACCOUNT NAME
            worksheet.Column(6).Width = 18;  // AUX.REF
            worksheet.Column(7).Width = 22;  // REFERENCE
            worksheet.Column(8).Width = 40;  // NARATION (with wrap)
            worksheet.Column(9).Width = 15;  // DEBIT (DR)
            worksheet.Column(10).Width = 15; // CREDIT (CR)

            // Enable auto-fit for rows to accommodate wrapped text
            if (validData.Any())
            {
                // Auto-fit all data rows including the totals row
                for (int row = firstDataRow; row <= currentRow - 1; row++)
                {
                    worksheet.Row(row).Height = -1; // Auto-fit row height
                }
            }

            // Format currency columns
            worksheet.Cells[$"I{headerRow + 1}:J{currentRow}"].Style.Numberformat.Format = "#,##0.00";

            worksheet.View.FreezePanes(headerRow + 1, 1);
        }

        // -----------------------------------------------------------------------
        // HELPER METHODS
        // -----------------------------------------------------------------------
        private void ApplyDefaultStyle(ExcelWorksheet worksheet)
        {
            worksheet.Cells.Style.Font.Name = "Bahnschrift SemiCondensed";
        }

        private int CreateHeaderSection(
    ExcelWorksheet worksheet,
    BankHeaderInformation header,
    string exportedBy,
    AccountingV2ReportsFilter filter,
    string reportTitle,
    string headerEndColumn)
        {
            worksheet.Cells[$"A1:{headerEndColumn}1"].Merge = true;
            worksheet.Cells["A1"].Value = header.BankName;
            worksheet.Cells["A1"].Style.Font.Bold = true;
            worksheet.Cells["A1"].Style.Font.Size = 18;
            worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells["A1"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Row(1).Height = 30;
            worksheet.Cells["A1"].Style.Font.Color.SetColor(Color.White);
            worksheet.Cells["A1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["A1"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 100, 0));

            // Row 2 - Branch Information
            worksheet.Cells[$"A2:{headerEndColumn}2"].Merge = true;
            worksheet.Cells["A2"].Value = $"Branch Code: {header.BranchCode} | Branch: {header.BranchName} | Branch ID: {header.BranchId}";
            worksheet.Cells["A2"].Style.Font.Bold = true;
            worksheet.Cells["A2"].Style.Font.Size = 12;
            worksheet.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells["A2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Row(2).Height = 22;
            worksheet.Cells["A2"].Style.Font.Color.SetColor(Color.White);
            worksheet.Cells["A2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["A2"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(70, 130, 180));

            // Row 3 - PERIOD (if available)
            if (filter != null &&
                filter.DateFrom != default(DateTime) &&
                filter.DateTo != default(DateTime))
            {
                worksheet.Cells[$"A3:{headerEndColumn}3"].Merge = true;
                worksheet.Cells["A3"].Value = $"PRINTING PERIOD : {filter.DateFrom:dd-MM-yyyy}  To   {filter.DateTo:dd-MM-yyyy}";
                worksheet.Cells["A3"].Style.Font.Size = 14;
                worksheet.Cells["A3"].Style.Font.Bold = true;
                worksheet.Cells["A3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                worksheet.Cells["A3"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Row(3).Height = 18;
                worksheet.Cells["A3"].Style.Font.Color.SetColor(Color.Black);
            }
            else
            {
                // If no period, leave row 3 blank or show a message
                worksheet.Cells[$"A3:{headerEndColumn}3"].Merge = true;
                worksheet.Cells["A3"].Value = "";
                worksheet.Row(3).Height = 18;
            }

            // Row 4 - Exported By
            worksheet.Cells[$"A4:{headerEndColumn}4"].Merge = true;
            worksheet.Cells["A4"].Value = $"Exported By: {exportedBy}";
            worksheet.Cells["A4"].Style.Font.Bold = true;
            worksheet.Cells["A4"].Style.Font.Size = 12;
            worksheet.Cells["A4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells["A4"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Row(4).Height = 18;

            // Row 5 - Export Date
            worksheet.Cells[$"A5:{headerEndColumn}5"].Merge = true;
            string exportDateText = $"Export Date: {DateTime.Now:dd-MM-yyyy HH:mm:ss}";
            worksheet.Cells["A5"].Value = exportDateText;
            worksheet.Cells["A5"].Style.Font.Bold = true;
            worksheet.Cells["A5"].Style.Font.Size = 12;
            worksheet.Cells["A5"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells["A5"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Row(5).Height = 18;

            // Row 6 - Address and Phone
            worksheet.Cells[$"A6:{headerEndColumn}6"].Merge = true;
            string address = !string.IsNullOrEmpty(header.BranchAddress) ? header.BranchAddress : "N/A";
            string phone = !string.IsNullOrEmpty(header.BranchTelephone) ? header.BranchTelephone : "N/A";
            worksheet.Cells["A6"].Value = $"Address: {address} | Tel: {phone}";
            worksheet.Cells["A6"].Style.Font.Bold = true;
            worksheet.Cells["A6"].Style.Font.Size = 11;
            worksheet.Cells["A6"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells["A6"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Row(6).Height = 18;
            worksheet.Cells["A6"].Style.Font.Color.SetColor(Color.Black);
            worksheet.Cells["A6"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["A6"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(240, 240, 240)); // Light gray background

            // Row 7 - Report Title
            worksheet.Cells[$"A7:{headerEndColumn}7"].Merge = true;
            worksheet.Cells["A7"].Value = reportTitle;
            worksheet.Cells["A7"].Style.Font.Bold = true;
            worksheet.Cells["A7"].Style.Font.Size = 14;
            worksheet.Cells["A7"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells["A7"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Row(7).Height = 25;
            worksheet.Cells["A7"].Style.Font.Color.SetColor(Color.Black);
            worksheet.Cells["A7"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["A7"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 215, 0)); // Gold color

            return 8; // Header ends at row 7, so next row is 8
        }

        private void SetSectionHeaderStyle(ExcelRange cell, Color backgroundColor)
        {
            cell.Style.Font.Bold = true;
            cell.Style.Font.Size = 14;
            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(backgroundColor);
        }

        private void SetHeaderStyle(ExcelRange cell, Color backgroundColor)
        {
            cell.Style.Font.Bold = true;
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(backgroundColor);
            cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        }

        private int CreateTwoColumnTable(ExcelWorksheet worksheet, int startRow, IEnumerable<dynamic> items,
            string col1Header, string col2Header, string col3Header = null)
        {
            int colCount = string.IsNullOrEmpty(col3Header) ? 2 : 3;
            for (int i = 0; i < colCount; i++)
            {
                worksheet.Cells[startRow, i + 1].Value = i == 0 ? col1Header : i == 1 ? col2Header : col3Header;
                SetHeaderStyle(worksheet.Cells[startRow, i + 1], Color.LightGreen);
            }
            startRow++;

            foreach (var item in items)
            {
                worksheet.Cells[startRow, 1].Value = item.Metric;
                worksheet.Cells[startRow, 2].Value = item.Value;
                if (colCount == 3)
                    worksheet.Cells[startRow, 3].Value = item.Description;

                for (int col = 1; col <= colCount; col++)
                    worksheet.Cells[startRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                startRow++;
            }
            return startRow;
        }

        private int CreateDistributionSection(
            ExcelWorksheet worksheet,
            int currentRow,
            string headerEndColumn,
            string title,
            IEnumerable<dynamic> distributionData,
            int total,
            Color backgroundColor)
        {
            worksheet.Cells[$"A{currentRow}:{headerEndColumn}{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = title;
            SetSectionHeaderStyle(worksheet.Cells[$"A{currentRow}"], backgroundColor);
            currentRow += 2;

            var headers = new[] { "Category", "Count", "Percentage" };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[currentRow, i + 1].Value = headers[i];
                SetHeaderStyle(worksheet.Cells[currentRow, i + 1], Color.LightGreen);
            }
            currentRow++;

            foreach (var item in distributionData)
            {
                decimal percentage = total > 0 ? (decimal)item.Count / total : 0;
                worksheet.Cells[currentRow, 1].Value = item.Status;
                worksheet.Cells[currentRow, 2].Value = item.Count;
                worksheet.Cells[currentRow, 3].Value = percentage;

                for (int col = 1; col <= 3; col++)
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                currentRow++;
            }

            if (distributionData.Any())
                worksheet.Cells[$"C{currentRow - distributionData.Count()}:C{currentRow - 1}"]
                    .Style.Numberformat.Format = "0.0%";

            return currentRow;
        }

        // -----------------------------------------------------------------------
        // UTILITY HELPERS
        // -----------------------------------------------------------------------
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

        private int GetColumnNumber(string columnLetter)
        {
            columnLetter = columnLetter.ToUpper();
            int number = 0;
            for (int i = 0; i < columnLetter.Length; i++)
                number = number * 26 + (columnLetter[i] - 'A' + 1);
            return number;
        }

        private string CleanSheetName(string name)
        {
            var invalidChars = new char[] { '\\', '/', '*', '?', ':', '[', ']' };
            foreach (var invalidChar in invalidChars)
                name = name.Replace(invalidChar, ' ');
            name = string.Join(" ", name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            return name.Trim();
        }
    }
    public class GeneralLedgerExcelExportGenerator : BaseService
    {
        // -----------------------------------------------------------------------
        // PUBLIC CONVERSION METHOD
        // -----------------------------------------------------------------------
        public static List<AccountStatementFlatItems> ConvertToGeneralLedgerData(List<AccountStatementFlatItems> data)
        {
            return data ?? new List<AccountStatementFlatItems>();
        }

        // -----------------------------------------------------------------------
        // MAIN GENERATION METHOD
        // -----------------------------------------------------------------------
        public void GenerateGeneralLedgerExcel(
            List<AccountStatementFlatItems> data,
            string filePath,
            string exportedBy,
            AccountingV2ReportsFilter filter,
            BankHeaderInformation headerInfo)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                // Validate that we have account numbers to process
                if (filter == null || filter.AccountNumbers == null || !filter.AccountNumbers.Any())
                {
                    throw new ArgumentException("At least one account number must be provided in the filter.");
                }

                // Filter data by the account numbers from the filter
                var filteredData = data.Where(x => filter.AccountNumbers.Contains(x.AccountNumber)).ToList();

                // Create summary sheet based on filtered data
                CreateSummarySheet(package, filteredData, headerInfo, exportedBy, filter);

                // Create statistics sheet based on filtered data
                CreateStatisticsSheet(package, filteredData, headerInfo, exportedBy, filter);

                // Create a sheet for EACH account number in the filter list (even if no transactions)
                foreach (var accountNumber in filter.AccountNumbers)
                {
                    // Get transactions for this account (may be empty)
                    var accountData = filteredData
                        .Where(x => x.AccountNumber == accountNumber)
                        .OrderBy(x => x.Date)
                        .ThenBy(x => x.Seq)
                        .ToList();

                    // Find account name from any matching record, or use a default
                    string accountName = accountData.FirstOrDefault()?.AccountName ?? "Unknown Account";

                    CreateAccountSheet(package, accountData, headerInfo,
                        accountNumber, accountName,
                        exportedBy, filter);
                }

                package.SaveAs(new FileInfo(filePath));
            }
        }

        // -----------------------------------------------------------------------
        // SHEET: SUMMARY & OVERVIEW
        // -----------------------------------------------------------------------
        private void CreateSummarySheet(
            ExcelPackage package,
            List<AccountStatementFlatItems> data,
            BankHeaderInformation headerInfo,
            string exportedBy,
            AccountingV2ReportsFilter filter)
        {
            var worksheet = package.Workbook.Worksheets.Add("Summary");
            ApplyDefaultStyle(worksheet);

            int headerColumns = 8;
            string headerEndColumn = GetColumnLetter(headerColumns);
            int currentRow = CreateHeaderSection(worksheet, headerInfo, exportedBy, filter,
                "GENERAL LEDGER SUMMARY REPORT", headerEndColumn);

            currentRow += 1;

            // Account Statistics (only for the accounts in the filter)
            var accountGroups = data
                .Where(x => !string.IsNullOrEmpty(x.AccountNumber))
                .GroupBy(x => new { x.AccountNumber, x.AccountName })
                .Select(g => new
                {
                    AccountNumber = g.Key.AccountNumber,
                    AccountName = g.Key.AccountName ?? "Unknown Account",
                    TransactionCount = g.Count(),
                    TotalDebit = g.Sum(x => x.DebitAmount),
                    TotalCredit = g.Sum(x => x.CreditAmount),
                    NetMovement = g.Sum(x => x.CreditAmount) - g.Sum(x => x.DebitAmount),
                    OpeningBalance = g.OrderBy(x => x.Seq).FirstOrDefault() != null ? g.OrderBy(x => x.Seq).First().OpeningBalance : 0,
                    ClosingBalance = g.OrderByDescending(x => x.Seq).FirstOrDefault() != null ? g.OrderByDescending(x => x.Seq).First().ClosingBalance : 0
                })
                .OrderBy(x => x.AccountNumber)
                .ToList();

            // Also include accounts that have no transactions (zero balances)
            var allRequestedAccounts = filter.AccountNumbers ?? new List<string>();
            foreach (var accNum in allRequestedAccounts)
            {
                if (!accountGroups.Any(a => a.AccountNumber == accNum))
                {
                    accountGroups.Add(new
                    {
                        AccountNumber = accNum,
                        AccountName = "Unknown Account",
                        TransactionCount = 0,
                        TotalDebit = 0m,
                        TotalCredit = 0m,
                        NetMovement = 0m,
                        OpeningBalance = 0m,
                        ClosingBalance = 0m
                    });
                }
            }

            // Summary metrics
            int totalAccounts = accountGroups.Count;
            int totalTransactions = data.Sum(x => x.DebitAmount != 0 || x.CreditAmount != 0 ? 1 : 0);
            decimal totalDebit = data.Sum(x => x.DebitAmount);
            decimal totalCredit = data.Sum(x => x.CreditAmount);
            decimal netMovement = totalCredit - totalDebit;

            var summaryData = new[]
            {
            new { Metric = "Report Date", Value = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"), Description = "Generation timestamp" },
            new { Metric = "Total Accounts", Value = totalAccounts.ToString("N0"), Description = "Number of GL accounts" },
            new { Metric = "Total Transactions", Value = totalTransactions.ToString("N0"), Description = "Total journal entries" },
            new { Metric = "Total Debit (DR)", Value = totalDebit.ToString("N2"), Description = "Sum of all debits" },
            new { Metric = "Total Credit (CR)", Value = totalCredit.ToString("N2"), Description = "Sum of all credits" },
            new { Metric = "Net Movement", Value = netMovement.ToString("N2"), Description = "Credit - Debit" }
        };

            currentRow = CreateTwoColumnTable(worksheet, currentRow, summaryData, "Metric", "Value", "Description");
            currentRow += 2;

            // Account Summary Table
            worksheet.Cells[$"A{currentRow}:{headerEndColumn}{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = "ACCOUNT SUMMARY";
            SetSectionHeaderStyle(worksheet.Cells[$"A{currentRow}"], Color.LightBlue);
            currentRow += 2;

            var headers = new[]
            {
            "ACCOUNT No", "ACCOUNT NAME", "TRANSACTIONS",
            "OPENING BAL", "DEBIT (DR)", "CREDIT (CR)", "NET MOVEMENT", "CLOSING BAL"
        };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[currentRow, i + 1].Value = headers[i];
                SetHeaderStyle(worksheet.Cells[currentRow, i + 1], Color.LightGreen);
            }
            currentRow++;

            foreach (var acc in accountGroups.OrderBy(a => a.AccountNumber))
            {
                worksheet.Cells[currentRow, 1].Value = acc.AccountNumber;
                worksheet.Cells[currentRow, 2].Value = acc.AccountName;
                worksheet.Cells[currentRow, 3].Value = acc.TransactionCount;
                worksheet.Cells[currentRow, 4].Value = acc.OpeningBalance;
                worksheet.Cells[currentRow, 5].Value = acc.TotalDebit;
                worksheet.Cells[currentRow, 6].Value = acc.TotalCredit;
                worksheet.Cells[currentRow, 7].Value = acc.NetMovement;
                worksheet.Cells[currentRow, 8].Value = acc.ClosingBalance;

                // Color coding for net movement
                if (acc.NetMovement > 0)
                    worksheet.Cells[currentRow, 7].Style.Font.Color.SetColor(Color.Green);
                else if (acc.NetMovement < 0)
                    worksheet.Cells[currentRow, 7].Style.Font.Color.SetColor(Color.Red);

                for (int col = 1; col <= 8; col++)
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                currentRow++;
            }

            // Grand Total Row
            if (accountGroups.Any())
            {
                worksheet.Cells[currentRow, 1].Value = "TOTALS:";
                worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
                worksheet.Cells[currentRow, 2].Value = $"{accountGroups.Count} accounts";
                worksheet.Cells[currentRow, 3].Value = accountGroups.Sum(x => x.TransactionCount);
                worksheet.Cells[currentRow, 4].Value = accountGroups.Sum(x => x.OpeningBalance);
                worksheet.Cells[currentRow, 5].Value = accountGroups.Sum(x => x.TotalDebit);
                worksheet.Cells[currentRow, 6].Value = accountGroups.Sum(x => x.TotalCredit);
                worksheet.Cells[currentRow, 7].Value = accountGroups.Sum(x => x.NetMovement);
                worksheet.Cells[currentRow, 8].Value = accountGroups.Sum(x => x.ClosingBalance);

                for (int col = 1; col <= 8; col++)
                {
                    worksheet.Cells[currentRow, col].Style.Font.Bold = true;
                    worksheet.Cells[currentRow, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[currentRow, col].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
            }

            // Format numbers
            worksheet.Cells[$"D{5}:H{currentRow}"].Style.Numberformat.Format = "#,##0.00";
            worksheet.Cells[$"C{5}:C{currentRow}"].Style.Numberformat.Format = "#,##0";

            // Set column widths
            worksheet.Column(1).Width = 18;
            worksheet.Column(2).Width = 35;
            worksheet.Column(3).Width = 12;
            worksheet.Column(4).Width = 15;
            worksheet.Column(5).Width = 15;
            worksheet.Column(6).Width = 15;
            worksheet.Column(7).Width = 15;
            worksheet.Column(8).Width = 15;

            worksheet.View.FreezePanes(5, 1);
        }

        // -----------------------------------------------------------------------
        // SHEET: STATISTICS & ANALYSIS
        // -----------------------------------------------------------------------
        private void CreateStatisticsSheet(
            ExcelPackage package,
            List<AccountStatementFlatItems> data,
            BankHeaderInformation headerInfo,
            string exportedBy,
            AccountingV2ReportsFilter filter)
        {
            var worksheet = package.Workbook.Worksheets.Add("Statistics");
            ApplyDefaultStyle(worksheet);

            int headerColumns = 4;
            string headerEndColumn = GetColumnLetter(headerColumns);
            int currentRow = CreateHeaderSection(worksheet, headerInfo, exportedBy, filter,
                "GENERAL LEDGER STATISTICS", headerEndColumn);

            currentRow += 1;

            // Transaction Distribution by Type
            var typeDistribution = data
                .GroupBy(x => string.IsNullOrEmpty(x.DrCr) ? "Unknown" : x.DrCr)
                .Select(g => new { Type = g.Key, Count = g.Count(), Amount = g.Sum(x => x.DrCr == "DR" ? x.DebitAmount : x.CreditAmount) })
                .ToList();

            worksheet.Cells[$"A{currentRow}:{headerEndColumn}{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = "TRANSACTION TYPE DISTRIBUTION";
            SetSectionHeaderStyle(worksheet.Cells[$"A{currentRow}"], Color.LightGreen);
            currentRow += 2;

            var typeHeaders = new[] { "Transaction Type", "Count", "Percentage", "Total Amount" };
            for (int i = 0; i < typeHeaders.Length; i++)
            {
                worksheet.Cells[currentRow, i + 1].Value = typeHeaders[i];
                SetHeaderStyle(worksheet.Cells[currentRow, i + 1], Color.LightGreen);
            }
            currentRow++;

            int totalTransactions = data.Count;
            foreach (var type in typeDistribution)
            {
                worksheet.Cells[currentRow, 1].Value = type.Type;
                worksheet.Cells[currentRow, 2].Value = type.Count;
                worksheet.Cells[currentRow, 3].Value = totalTransactions > 0 ? (double)type.Count / totalTransactions : 0;
                worksheet.Cells[currentRow, 4].Value = type.Amount;

                for (int col = 1; col <= 4; col++)
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                currentRow++;
            }

            worksheet.Cells[$"C{currentRow - typeDistribution.Count}:C{currentRow - 1}"].Style.Numberformat.Format = "0.0%";
            worksheet.Cells[$"D{currentRow - typeDistribution.Count}:D{currentRow - 1}"].Style.Numberformat.Format = "#,##0.00";

            currentRow += 2;

            // Monthly Activity Summary (if dates available)
            var monthlyData = data
                .Where(x => x.Date != default(DateTime))
                .GroupBy(x => new { x.Date.Year, x.Date.Month })
                .Select(g => new
                {
                    Period = $"{g.Key.Year}-{g.Key.Month:D2}",
                    TransactionCount = g.Count(),
                    TotalDebit = g.Sum(x => x.DebitAmount),
                    TotalCredit = g.Sum(x => x.CreditAmount)
                })
                .OrderBy(x => x.Period)
                .ToList();

            if (monthlyData.Any())
            {
                worksheet.Cells[$"A{currentRow}:{headerEndColumn}{currentRow}"].Merge = true;
                worksheet.Cells[$"A{currentRow}"].Value = "MONTHLY ACTIVITY SUMMARY";
                SetSectionHeaderStyle(worksheet.Cells[$"A{currentRow}"], Color.LightBlue);
                currentRow += 2;

                var monthlyHeaders = new[] { "Period", "Transactions", "Total Debit", "Total Credit", "Net Movement" };
                for (int i = 0; i < monthlyHeaders.Length; i++)
                {
                    worksheet.Cells[currentRow, i + 1].Value = monthlyHeaders[i];
                    SetHeaderStyle(worksheet.Cells[currentRow, i + 1], Color.LightGreen);
                }
                currentRow++;

                foreach (var month in monthlyData)
                {
                    worksheet.Cells[currentRow, 1].Value = month.Period;
                    worksheet.Cells[currentRow, 2].Value = month.TransactionCount;
                    worksheet.Cells[currentRow, 3].Value = month.TotalDebit;
                    worksheet.Cells[currentRow, 4].Value = month.TotalCredit;
                    worksheet.Cells[currentRow, 5].Value = month.TotalCredit - month.TotalDebit;

                    for (int col = 1; col <= 5; col++)
                        worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    currentRow++;
                }
            }

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        }

        // -----------------------------------------------------------------------
        // SHEET: INDIVIDUAL ACCOUNT DETAILS
        // -----------------------------------------------------------------------
        // -----------------------------------------------------------------------
        // SHEET: INDIVIDUAL ACCOUNT DETAILS
        // -----------------------------------------------------------------------
        private void CreateAccountSheet(
    ExcelPackage package,
    List<AccountStatementFlatItems> accountData,
    BankHeaderInformation headerInfo,
    string accountNumber,
    string accountName,
    string exportedBy,
    AccountingV2ReportsFilter filter)
        {
            // Create sheet name from account number and name (limited to 31 chars)
            string baseSheetName = $"{accountNumber}_{accountName}";
            string sheetName = CleanSheetName(baseSheetName);
            if (sheetName.Length > 31)
                sheetName = sheetName.Substring(0, 28) + "...";

            var worksheet = package.Workbook.Worksheets.Add(sheetName);
            ApplyDefaultStyle(worksheet);

            // Calculate account statistics (zero if no data)
            decimal openingBalance = accountData.Any() ? accountData.OrderBy(x => x.Seq).First().OpeningBalance : 0;
            decimal closingBalance = accountData.Any() ? accountData.OrderByDescending(x => x.Seq).First().ClosingBalance : 0;
            decimal totalDebit = accountData.Sum(x => x.DebitAmount);
            decimal totalCredit = accountData.Sum(x => x.CreditAmount);
            int transactionCount = accountData.Count;

            // Header section - Using the exact CreateHeaderSection method
            int headerColumns = 9; // A to I
            string headerEndColumn = GetColumnLetter(headerColumns);

            int currentRow = CreateHeaderSection(worksheet, headerInfo, exportedBy, filter,
                $"GENERAL LEDGER - {accountNumber}", headerEndColumn);

            // Create a two-column table for account information (with borders)
            currentRow += 1; // Add a small gap

            // Create account info table with borders
            var accountInfo = new[]
            {
        new { Label = "ACCOUNT NO:", Value = accountNumber },
        new { Label = "ACCOUNT NAME:", Value = accountName },
        new { Label = "BEGINNING BALANCE:", Value = openingBalance.ToString("N2") },
        new { Label = "ENDING BALANCE:", Value = closingBalance.ToString("N2") },
        new { Label = "CURRENCY:", Value = accountData.FirstOrDefault()?.Currency ?? "XAF FRANCE CFA" },
        new { Label = "ACCOUNTING DATE:", Value = DateTime.Now.ToString("dd-MM-yyyy") }
    };

            // Add headers for the account info table
            worksheet.Cells[currentRow, 1].Value = "FIELD";
            worksheet.Cells[currentRow, 2].Value = "VALUE";
            SetHeaderStyle(worksheet.Cells[currentRow, 1], Color.LightBlue);
            SetHeaderStyle(worksheet.Cells[currentRow, 2], Color.LightBlue);
            currentRow++;

            foreach (var info in accountInfo)
            {
                worksheet.Cells[currentRow, 1].Value = info.Label;
                worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
                worksheet.Cells[currentRow, 2].Value = info.Value;

                // Apply borders to both columns
                worksheet.Cells[currentRow, 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                worksheet.Cells[currentRow, 2].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                // Set alignment
                worksheet.Cells[currentRow, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Cells[currentRow, 2].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                currentRow++;
            }

            // Empty row for spacing
            currentRow++;

            // Transactions Table Headers
            var headers = new[]
            {
        "SN", "DATE", "TIME", "REPRESENTATIVE", "REFERENCE",
        "DESCRIPTION", "DEBIT", "CREDIT", "BALANCE"
    };

            int headerRow = currentRow;
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[headerRow, i + 1].Value = headers[i];
                SetHeaderStyle(worksheet.Cells[headerRow, i + 1], Color.LightGreen);
                worksheet.Cells[headerRow, i + 1].Style.WrapText = true;
            }
            currentRow++;

            int firstDataRow = currentRow;

            if (accountData.Any())
            {
                int sn = 1;
                decimal runningBalance = openingBalance;

                foreach (var item in accountData.OrderBy(x => x.Date).ThenBy(x => x.Seq))
                {
                    worksheet.Cells[currentRow, 1].Value = sn++;
                    worksheet.Cells[currentRow, 2].Value = item.Date.ToString("dd-MM-yyyy");
                    worksheet.Cells[currentRow, 3].Value = item.time.ToString(@"hh\:mm\:ss");

                    worksheet.Cells[currentRow, 4].Value = item.AuxiliaryRef;
                    worksheet.Cells[currentRow, 5].Value = item.ReferenceNumber;
                    worksheet.Cells[currentRow, 6].Value = item.Description;
                    worksheet.Cells[currentRow, 7].Value = item.DebitAmount;  // Always show, zero appears as 0
                    worksheet.Cells[currentRow, 8].Value = item.CreditAmount; // Always show, zero appears as 0

                    runningBalance = item.Balance;
                    worksheet.Cells[currentRow, 9].Value = runningBalance;

                    worksheet.Cells[currentRow, 6].Style.WrapText = true;
                    worksheet.Cells[currentRow, 6].Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                    for (int col = 1; col <= headers.Length; col++)
                    {
                        worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        if (col != 6)
                            worksheet.Cells[currentRow, col].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    }

                    currentRow++;
                }
            }
            else
            {
                // No transactions: show a single row indicating no data
                worksheet.Cells[currentRow, 1].Value = "No transactions for this account";
                worksheet.Cells[currentRow, 1, currentRow, headers.Length].Merge = true;
                worksheet.Cells[currentRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[currentRow, 1].Style.Font.Italic = true;
                for (int col = 1; col <= headers.Length; col++)
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                currentRow++;
            }

            // Totals row (only if there are transactions)
            if (accountData.Any())
            {
                worksheet.Cells[currentRow, 1].Value = "TOTALS:";
                worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
                worksheet.Cells[currentRow, 7].Value = totalDebit;
                worksheet.Cells[currentRow, 8].Value = totalCredit;

                for (int col = 1; col <= headers.Length; col++)
                {
                    worksheet.Cells[currentRow, col].Style.Font.Bold = true;
                    worksheet.Cells[currentRow, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[currentRow, col].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    worksheet.Cells[currentRow, col].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }
                currentRow++;
            }

            // Auto-fit rows for wrapped text
            for (int row = firstDataRow; row <= currentRow; row++)
            {
                worksheet.Row(row).Height = -1;
            }

            // Format number columns
            worksheet.Cells[$"G{headerRow + 1}:I{currentRow}"].Style.Numberformat.Format = "#,##0.00";

            // Set column widths with increased widths for all columns
            worksheet.Column(1).Width = 10;   // SN - increased from 8
            worksheet.Column(2).Width = 14;   // DATE - increased from 12
            worksheet.Column(3).Width = 12;   // TIME - increased from 10
            worksheet.Column(4).Width = 25;   // REPRESENTATIVE - increased from 20
            worksheet.Column(5).Width = 25;   // REFERENCE - increased from 22
            worksheet.Column(6).Width = 70;   // DESCRIPTION - increased from 60
            worksheet.Column(7).Width = 18;   // DEBIT - increased from 15
            worksheet.Column(8).Width = 18;   // CREDIT - increased from 15
            worksheet.Column(9).Width = 20;   // BALANCE - increased from 18

            // Also increase widths for the account info table columns
            worksheet.Column(1).Width = 25; // For FIELD column (Label)
            worksheet.Column(2).Width = 35; // For VALUE column

            worksheet.View.FreezePanes(headerRow + 1, 1);

            // Add account summary below transactions
            currentRow += 2;

            worksheet.Cells[$"A{currentRow}:C{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = "ACCOUNT SUMMARY";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Size = 14;
            worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
            currentRow += 2;

            var summaryItems = new[]
            {
        new { Label = "Total Transactions:", Value = transactionCount.ToString("N0") },
        new { Label = "Total Debit:", Value = totalDebit.ToString("N2") },
        new { Label = "Total Credit:", Value = totalCredit.ToString("N2") },
        new { Label = "Net Movement:", Value = (totalCredit - totalDebit).ToString("N2") },
        new { Label = "Opening Balance:", Value = openingBalance.ToString("N2") },
        new { Label = "Closing Balance:", Value = closingBalance.ToString("N2") }
    };

            // Add headers for summary table
            worksheet.Cells[currentRow, 1].Value = "METRIC";
            worksheet.Cells[currentRow, 2].Value = "VALUE";
            SetHeaderStyle(worksheet.Cells[currentRow, 1], Color.LightBlue);
            SetHeaderStyle(worksheet.Cells[currentRow, 2], Color.LightBlue);
            currentRow++;

            foreach (var item in summaryItems)
            {
                worksheet.Cells[currentRow, 1].Value = item.Label;
                worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
                worksheet.Cells[currentRow, 2].Value = item.Value;

                for (int col = 1; col <= 2; col++)
                {
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    worksheet.Cells[currentRow, col].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }
                currentRow++;
            }

            // Set widths for summary columns
            worksheet.Column(1).Width = 25; // Metric column
            worksheet.Column(2).Width = 25; // Value column
        }

        // -----------------------------------------------------------------------
        // HELPER METHODS
        // -----------------------------------------------------------------------
        private void ApplyDefaultStyle(ExcelWorksheet worksheet)
        {
            worksheet.Cells.Style.Font.Name = "Bahnschrift SemiCondensed";
        }

        // EXACT CreateHeaderSection method as provided
        // Updated CreateHeaderSection (row 4 removed)
        private int CreateHeaderSection(
            ExcelWorksheet worksheet,
            BankHeaderInformation header,
            string exportedBy,
            AccountingV2ReportsFilter filter,
            string reportTitle,
            string headerEndColumn)
        {
            worksheet.Cells[$"A1:{headerEndColumn}1"].Merge = true;
            worksheet.Cells["A1"].Value = header.BankName;
            worksheet.Cells["A1"].Style.Font.Bold = true;
            worksheet.Cells["A1"].Style.Font.Size = 18;
            worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells["A1"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Row(1).Height = 30;
            worksheet.Cells["A1"].Style.Font.Color.SetColor(Color.White);
            worksheet.Cells["A1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["A1"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 100, 0));

            worksheet.Cells[$"A2:{headerEndColumn}2"].Merge = true;
            worksheet.Cells["A2"].Value = $"Branch Code: {header.BranchCode} | Branch: {header.BranchName} | Branch ID: {header.BranchId}";
            worksheet.Cells["A2"].Style.Font.Bold = true;
            worksheet.Cells["A2"].Style.Font.Size = 12;
            worksheet.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells["A2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Row(2).Height = 22;
            worksheet.Cells["A2"].Style.Font.Color.SetColor(Color.White);
            worksheet.Cells["A2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["A2"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(70, 130, 180));

            if (filter != null &&
                filter.DateFrom != default(DateTime) &&
                filter.DateTo != default(DateTime))
            {
                worksheet.Cells[$"A3:{headerEndColumn}3"].Merge = true;
                worksheet.Cells["A3"].Value = $"PRINTING PERIOD : {filter.DateFrom:dd-MM-yyyy}  To   {filter.DateTo:dd-MM-yyyy}";
                worksheet.Cells["A3"].Style.Font.Size = 14;
                worksheet.Cells["A3"].Style.Font.Bold = true;
                worksheet.Cells["A3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                worksheet.Cells["A3"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Row(3).Height = 18;
                worksheet.Cells["A3"].Style.Font.Color.SetColor(Color.Black);
            }
            else
            {
                worksheet.Cells[$"A3:{headerEndColumn}3"].Merge = true;
                worksheet.Cells["A3"].Value = "";
                worksheet.Row(3).Height = 18;
            }

            // Row 4 - Export Date
            worksheet.Cells[$"A4:{headerEndColumn}4"].Merge = true;
            string exportDateText = $"Export Date: {DateTime.Now:dd-MM-yyyy HH:mm:ss}";
            worksheet.Cells["A4"].Value = exportDateText;
            worksheet.Cells["A4"].Style.Font.Bold = true;
            worksheet.Cells["A4"].Style.Font.Size = 12;
            worksheet.Cells["A4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells["A4"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Row(4).Height = 18;

            // Row 5 - Report Title (previously row 6)
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

            return 5; // next row after header
        }

        private void SetSectionHeaderStyle(ExcelRange cell, Color backgroundColor)
        {
            cell.Style.Font.Bold = true;
            cell.Style.Font.Size = 14;
            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(backgroundColor);
        }

        private void SetHeaderStyle(ExcelRange cell, Color backgroundColor)
        {
            cell.Style.Font.Bold = true;
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(backgroundColor);
            cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        }

        private int CreateTwoColumnTable(ExcelWorksheet worksheet, int startRow, IEnumerable<dynamic> items,
    string col1Header, string col2Header, string col3Header = null)
        {
            int colCount = string.IsNullOrEmpty(col3Header) ? 2 : 3;

            // Set headers
            for (int i = 0; i < colCount; i++)
            {
                worksheet.Cells[startRow, i + 1].Value = i == 0 ? col1Header : i == 1 ? col2Header : col3Header;
                SetHeaderStyle(worksheet.Cells[startRow, i + 1], Color.LightGreen);
            }

            // Set column widths for better spacing
            worksheet.Column(1).Width = 20; // Metric column
            worksheet.Column(2).Width = 18; // Value column
            if (colCount == 3)
            {
                worksheet.Column(3).Width = 60; // Description column - increased width
                worksheet.Column(3).Style.WrapText = true; // Enable text wrapping
            }

            startRow++;

            foreach (var item in items)
            {
                worksheet.Cells[startRow, 1].Value = item.Metric ?? item.Label;
                worksheet.Cells[startRow, 2].Value = item.Value;
                if (colCount == 3)
                    worksheet.Cells[startRow, 3].Value = item.Description;

                for (int col = 1; col <= colCount; col++)
                    worksheet.Cells[startRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                startRow++;
            }
            return startRow;
        }

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

        private string CleanSheetName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "Account";
            var invalidChars = new char[] { '\\', '/', '*', '?', ':', '[', ']' };
            foreach (var invalidChar in invalidChars)
                name = name.Replace(invalidChar, ' ');
            name = string.Join(" ", name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            return name.Trim();
        }
    }
}