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
                "ACCOUNT STATEMENT SUMMARY", headerEndColumn);

            // ----- GENERAL SUMMARY -----
            worksheet.Cells[$"A{currentRow}:{headerEndColumn}{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = "GENERAL SUMMARY";
            SetSectionHeaderStyle(worksheet.Cells[$"A{currentRow}"], Color.LightBlue);
            currentRow += 2;

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
                new { Metric = "Net Movement", Value = netMovement.ToString("N2"), Description = "Credit - Debit" },
                new { Metric = "Closing Balance", Value = closingBalance.ToString("N2"), Description = "Balance of last transaction" }
            };

            currentRow = CreateTwoColumnTable(worksheet, currentRow, summaryData, "Metric", "Value", "Description");
            currentRow += 2;

            // ----- TRANSACTION TYPE DISTRIBUTION (DR/CR) -----
            var typeDistribution = data
                .GroupBy(x => string.IsNullOrEmpty(x.DrCr) ? "Unknown" : x.DrCr)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            currentRow = CreateDistributionSection(worksheet, currentRow, headerEndColumn,
                "TRANSACTION TYPE DISTRIBUTION", typeDistribution, totalTransactions, Color.LightYellow);
            currentRow += 2;

            // ----- TOP ACCOUNTS BY VOLUME -----
            var topAccounts = data
                .Where(x => !string.IsNullOrEmpty(x.AccountNumber))
                .GroupBy(x => new { x.AccountNumber, x.AccountName })
                .Select(g => new { Account = $"{g.Key.AccountNumber} - {g.Key.AccountName}", Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToList();

            if (topAccounts.Any())
            {
                worksheet.Cells[$"A{currentRow}:{headerEndColumn}{currentRow}"].Merge = true;
                worksheet.Cells[$"A{currentRow}"].Value = "TOP 10 ACCOUNTS BY TRANSACTION COUNT";
                SetSectionHeaderStyle(worksheet.Cells[$"A{currentRow}"], Color.LightGreen);
                currentRow += 2;

                var headers = new[] { "Account", "Transaction Count" };
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[currentRow, i + 1].Value = headers[i];
                    SetHeaderStyle(worksheet.Cells[currentRow, i + 1], Color.LightGreen);
                }
                currentRow++;

                foreach (var acc in topAccounts)
                {
                    worksheet.Cells[currentRow, 1].Value = acc.Account;
                    worksheet.Cells[currentRow, 2].Value = acc.Count;
                    for (int col = 1; col <= 2; col++)
                        worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    currentRow++;
                }
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

            worksheet.Cells[$"A{currentRow}:{headerEndColumn}{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = "BRANCH SUMMARY";
            SetSectionHeaderStyle(worksheet.Cells[$"A{currentRow}"], Color.LightCoral);
            currentRow += 2;

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

            var branchSummary = data
                .GroupBy(x => new
                {
                    BranchId = x.BranchId ?? "Unknown",
                    BranchCode = x.BranchCode ?? "",
                    BranchName = string.IsNullOrWhiteSpace(x.BranchName) ? "Unknown Branch" : x.BranchName
                })
                .Select(g => new
                {
                    g.Key.BranchId,
                    g.Key.BranchCode,
                    g.Key.BranchName,
                    TransactionCount = g.Count(),
                    TotalDebit = g.Sum(x => x.DebitAmount),
                    TotalCredit = g.Sum(x => x.CreditAmount)
                })
                .OrderByDescending(x => x.TotalCredit + x.TotalDebit)
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
                "ACCOUNT STATEMENT DETAILS", headerEndColumn);

            int totalTransactions = data.Count;
            var typeDist = data
                .GroupBy(x => string.IsNullOrEmpty(x.DrCr) ? "Unknown" : x.DrCr)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToList();

            currentRow = CreateDistributionSection(worksheet, currentRow, headerEndColumn,
                "TRANSACTION TYPE OVERVIEW", typeDist, totalTransactions, Color.LightYellow);
            currentRow += 2;

            worksheet.Cells[$"A{currentRow}:{headerEndColumn}{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = "DETAILED TRANSACTIONS";
            SetSectionHeaderStyle(worksheet.Cells[$"A{currentRow}"], Color.LightCoral);
            currentRow += 2;

            // NEW HEADER ORDER as requested
            var headers = new[]
            {
                "SN",
                "Accounting Date",
                "Time",
                "Account Number",
                "Account Name",
                "Member Reference",
                "Reference",
                "Description",
                "Debit (DR)",
                "Credit (CR)"
            };

            int headerColumnsCount = headers.Length;
            headerEndColumn = GetColumnLetter(headerColumnsCount);

            int headerRow = currentRow;
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[headerRow, i + 1].Value = headers[i];
                SetHeaderStyle(worksheet.Cells[headerRow, i + 1], Color.LightGreen);
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

            foreach (var item in validData.OrderBy(x => x.Seq))
            {
                worksheet.Cells[currentRow, 1].Value = sn++;
                worksheet.Cells[currentRow, 2].Value = item.AccountingDate;
                worksheet.Cells[currentRow, 3].Value = item.time.ToString(@"hh\:mm\:ss");
                worksheet.Cells[currentRow, 4].Value = item.AccountNumber;
                worksheet.Cells[currentRow, 5].Value = item.AccountName;
                worksheet.Cells[currentRow, 6].Value = item.AuxiliaryRef; // Member Reference
                worksheet.Cells[currentRow, 7].Value = item.ReferenceNumber;
                worksheet.Cells[currentRow, 8].Value = item.Description;
                worksheet.Cells[currentRow, 9].Value = item.DebitAmount;
                worksheet.Cells[currentRow, 10].Value = item.CreditAmount;

                for (int col = 1; col <= headerColumnsCount; col++)
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                totalDebit += item.DebitAmount;
                totalCredit += item.CreditAmount;
                currentRow++;
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
                }
            }

            worksheet.Cells[$"I{headerRow + 1}:J{currentRow}"].Style.Numberformat.Format = "#,##0.00";
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
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
                $"BRANCH ACCOUNT STATEMENT - {safeBranchName}", headerEndColumn);

            // ----- DETAILED ENTRIES HEADER -----
            worksheet.Cells[$"A{currentRow}:{headerEndColumn}{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = $"DETAILED TRANSACTIONS - {safeBranchName}";
            SetSectionHeaderStyle(worksheet.Cells[$"A{currentRow}"], Color.LightCoral);
            currentRow += 2;

            // NEW HEADER ORDER as requested
            var branchHeaders = new[]
            {
                "SN",
                "Accounting Date",
                "Time",
                "Account Number",
                "Account Name",
                "Member Reference",
                "Reference",
                "Description",
                "Debit (DR)",
                "Credit (CR)"
            };

            int headerColumnsCount = branchHeaders.Length;
            headerEndColumn = GetColumnLetter(headerColumnsCount);

            int headerRow = currentRow;
            for (int i = 0; i < branchHeaders.Length; i++)
            {
                worksheet.Cells[headerRow, i + 1].Value = branchHeaders[i];
                SetHeaderStyle(worksheet.Cells[headerRow, i + 1], Color.LightGreen);
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

            foreach (var item in validData.OrderBy(x => x.Seq))
            {
                worksheet.Cells[currentRow, 1].Value = sn++;
                worksheet.Cells[currentRow, 2].Value = item.AccountingDate;
                worksheet.Cells[currentRow, 3].Value = item.time.ToString(@"hh\:mm\:ss");
                worksheet.Cells[currentRow, 4].Value = item.AccountNumber;
                worksheet.Cells[currentRow, 5].Value = item.AccountName;
                worksheet.Cells[currentRow, 6].Value = item.AuxiliaryRef; // Member Reference
                worksheet.Cells[currentRow, 7].Value = item.ReferenceNumber;
                worksheet.Cells[currentRow, 8].Value = item.Description;
                worksheet.Cells[currentRow, 9].Value = item.DebitAmount;
                worksheet.Cells[currentRow, 10].Value = item.CreditAmount;

                for (int col = 1; col <= headerColumnsCount; col++)
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                totalDebit += item.DebitAmount;
                totalCredit += item.CreditAmount;
                currentRow++;
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
                new { Metric = "Total Transactions", Value = total.ToString("N0") },
                new { Metric = "Total Debit", Value = sumDebit.ToString("N2") },
                new { Metric = "Total Credit", Value = sumCredit.ToString("N2") },
                new { Metric = "Net Movement", Value = net.ToString("N2") },
                new { Metric = "Closing Balance", Value = closingBal.ToString("N2") }
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

            worksheet.Cells[$"I{headerRow + 1}:J{currentRow}"].Style.Numberformat.Format = "#,##0.00";
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
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

            worksheet.Cells[$"A3:{headerEndColumn}3"].Merge = true;
            worksheet.Cells["A3"].Value = $"Exported By: {exportedBy}";
            worksheet.Cells["A3"].Style.Font.Bold = true;
            worksheet.Cells["A3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells["A3"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Row(3).Height = 18;

            // Row 4 now contains both Export Date and Date Range (if available)
            worksheet.Cells[$"A4:{headerEndColumn}4"].Merge = true;

            string exportDateText = $"Export Date: {DateTime.Now:dd-MM-yyyy HH:mm:ss}";

            if (filter != null &&
                filter.DateFrom != default(DateTime) &&
                filter.DateTo != default(DateTime))
            {
                worksheet.Cells["A4"].Value = $"{exportDateText} | Date Range: {filter.DateFrom:dd/MM/yyyy} to {filter.DateTo:dd/MM/yyyy}";
            }
            else
            {
                worksheet.Cells["A4"].Value = exportDateText;
            }

            worksheet.Cells["A4"].Style.Font.Bold = true;
            worksheet.Cells["A4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells["A4"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Row(4).Height = 18;

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

            return 6; // Header ends at row 5, so next row is 6
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
}