using BusinessServices;
using CBS.FrontDesk.Data.Entity.AccountingV2;
using CBS.FrontDesk.Data.Entity.AccountingV2.GLSystemReconciliation;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace CBS.BusinessService.AccountingV2.GLSystemReconciliation
{




    public class ReconciliationExcelExportGenerator : BaseService
    {
        // -----------------------------------------------------------------------
        // PUBLIC CONVERSION METHOD
        // -----------------------------------------------------------------------
        public static List<Reconciliation> ConvertToReconciliationData(List<Reconciliation> data)
        {
            return data ?? new List<Reconciliation>();
        }

        // -----------------------------------------------------------------------
        // MAIN GENERATION METHOD
        // -----------------------------------------------------------------------
        public void GenerateReconciliationExcel(
            List<Reconciliation> reconciliationData,
            string filePath,
            string exportedBy,
            ExportOptions exportOptions)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                CreateSummarySheet(package, reconciliationData,
                    GetBankName(), GetBranchCode(), GetBranchID(), GetBranchName(),
                    exportedBy, exportOptions);

                CreateBranchSummarySheet(package, reconciliationData, exportedBy, exportOptions);
                CreateReconciliationDetailsSheet(package, reconciliationData, exportedBy, exportOptions);

                var branches = reconciliationData
                    .GroupBy(x => x.BranchName)
                    .Select(g => new { BranchName = g.Key })
                    .ToList();

                foreach (var branch in branches)
                {
                    var branchData = reconciliationData.Where(x => x.BranchName == branch.BranchName).ToList();
                    // Branch code/id are not available; pass empty strings
                    CreateBranchSheet(package, branchData,
                        branchId:"", branchName: branch.BranchName, branchCode: "",
                        exportedBy, exportOptions);
                }

                package.SaveAs(new FileInfo(filePath));
            }
        }

        // -----------------------------------------------------------------------
        // SHEET: SUMMARY & OVERVIEW
        // -----------------------------------------------------------------------
        private void CreateSummarySheet(
            ExcelPackage package,
            List<Reconciliation> data,
            string bank,
            string branchCode,
            string branchId,
            string branchName,
            string exportedBy,
            ExportOptions exportOptions)
        {
            var worksheet = package.Workbook.Worksheets.Add("Summary & Overview");
            ApplyDefaultStyle(worksheet);

            int headerColumns = 3;
            string headerEndColumn = GetColumnLetter(headerColumns);
            int currentRow = CreateHeaderSection(worksheet, bank, branchCode, branchId, branchName,
                exportedBy, exportOptions, "RECONCILIATION SUMMARY REPORT", headerEndColumn);

            // ----- GENERAL SUMMARY -----
            worksheet.Cells[$"A{currentRow}:{headerEndColumn}{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = "GENERAL SUMMARY";
            SetSectionHeaderStyle(worksheet.Cells[$"A{currentRow}"], Color.LightBlue);
            currentRow += 2;

            int totalRecords = data.Count;
            int totalBranches = data.Select(x => x.BranchName).Distinct().Count();

            var statusGroups = data.GroupBy(x => x.Status ?? "Unknown").ToDictionary(g => g.Key, g => g.Count());
            int pending = statusGroups.TryGetValue("Pending", out int pendingVal) ? pendingVal : 0;
            int completed = statusGroups.TryGetValue("Completed", out int completedVal) ? completedVal : 0;
            int failed = statusGroups.TryGetValue("Failed", out int failedVal) ? failedVal : 0;
            int other = totalRecords - pending - completed - failed;

            var summaryData = new[]
            {
            new { Metric = "Total Reconciliation Records", Value = totalRecords.ToString("N0"), Description = "Number of transactions" },
            new { Metric = "Total Branches", Value = totalBranches.ToString("N0"), Description = "Branches with reconciliation activity" },
            new { Metric = "Pending Records", Value = pending.ToString("N0"), Description = "Awaiting processing" },
            new { Metric = "Completed Records", Value = completed.ToString("N0"), Description = "Successfully processed" },
            new { Metric = "Failed Records", Value = failed.ToString("N0"), Description = "Processing failed" },
            new { Metric = "Other Status", Value = other.ToString("N0"), Description = "Other status values" }
        };

            currentRow = CreateTwoColumnTable(worksheet, currentRow, summaryData, "Metric", "Value", "Description");
            currentRow += 2;

            // ----- STATUS DISTRIBUTION -----
            var statusDistribution = data
                .GroupBy(x => x.Status ?? "Unknown")
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            currentRow = CreateDistributionSection(worksheet, currentRow, headerEndColumn,
                "STATUS DISTRIBUTION", statusDistribution, totalRecords, Color.LightYellow);
            currentRow += 2;

            // ----- OPERATION CODE DISTRIBUTION -----
            var opCodeDistribution = data
                .Where(x => !string.IsNullOrEmpty(x.OperationCode))
                .GroupBy(x => x.OperationCode)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            if (opCodeDistribution.Any())
            {
                currentRow = CreateDistributionSection(worksheet, currentRow, headerEndColumn,
                    "OPERATION CODE DISTRIBUTION", opCodeDistribution, totalRecords, Color.LightGreen);
            }

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        }

        // -----------------------------------------------------------------------
        // SHEET: BRANCH SUMMARY
        // -----------------------------------------------------------------------
        private void CreateBranchSummarySheet(
            ExcelPackage package,
            List<Reconciliation> data,
            string exportedBy,
            ExportOptions exportOptions)
        {
            var worksheet = package.Workbook.Worksheets.Add("Branch Summary");
            ApplyDefaultStyle(worksheet);

            int headerColumns = 6;
            string headerEndColumn = GetColumnLetter(headerColumns);
            int currentRow = CreateHeaderSection(worksheet, GetBankName(), GetBranchCode(), GetBranchID(), GetBranchName(),
                exportedBy, exportOptions, "BRANCH SUMMARY REPORT", headerEndColumn);

            worksheet.Cells[$"A{currentRow}:{headerEndColumn}{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = "BRANCH SUMMARY";
            SetSectionHeaderStyle(worksheet.Cells[$"A{currentRow}"], Color.LightCoral);
            currentRow += 2;

            var branchHeaders = new[]
            {
            "Branch Name", "# Records", "Pending", "Completed", "Failed", "Other"
        };

            for (int i = 0; i < branchHeaders.Length; i++)
            {
                worksheet.Cells[currentRow, i + 1].Value = branchHeaders[i];
                SetHeaderStyle(worksheet.Cells[currentRow, i + 1], Color.LightGreen);
            }
            currentRow++;

            var branchSummary = data
                .GroupBy(x => x.BranchName)
                .Select(g => new
                {
                    BranchName = g.Key,
                    RecordCount = g.Count(),
                    Pending = g.Count(x => x.Status == "Pending"),
                    Completed = g.Count(x => x.Status == "Completed"),
                    Failed = g.Count(x => x.Status == "Failed"),
                    Other = g.Count(x => x.Status != "Pending" && x.Status != "Completed" && x.Status != "Failed")
                })
                .OrderByDescending(x => x.RecordCount)
                .ToList();

            foreach (var branch in branchSummary)
            {
                worksheet.Cells[currentRow, 1].Value = branch.BranchName;
                worksheet.Cells[currentRow, 2].Value = branch.RecordCount;
                worksheet.Cells[currentRow, 3].Value = branch.Pending;
                worksheet.Cells[currentRow, 4].Value = branch.Completed;
                worksheet.Cells[currentRow, 5].Value = branch.Failed;
                worksheet.Cells[currentRow, 6].Value = branch.Other;

                for (int col = 1; col <= headerColumns; col++)
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                currentRow++;
            }

            if (branchSummary.Any())
            {
                worksheet.Cells[currentRow, 1].Value = "TOTALS:";
                worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
                worksheet.Cells[currentRow, 2].Value = branchSummary.Sum(x => x.RecordCount);
                worksheet.Cells[currentRow, 3].Value = branchSummary.Sum(x => x.Pending);
                worksheet.Cells[currentRow, 4].Value = branchSummary.Sum(x => x.Completed);
                worksheet.Cells[currentRow, 5].Value = branchSummary.Sum(x => x.Failed);
                worksheet.Cells[currentRow, 6].Value = branchSummary.Sum(x => x.Other);

                for (int col = 1; col <= headerColumns; col++)
                {
                    worksheet.Cells[currentRow, col].Style.Font.Bold = true;
                    worksheet.Cells[currentRow, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[currentRow, col].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
            }

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        }

        // -----------------------------------------------------------------------
        // SHEET: RECONCILIATION DETAILS (ALL ENTRIES)
        // -----------------------------------------------------------------------
        private void CreateReconciliationDetailsSheet(
            ExcelPackage package,
            List<Reconciliation> data,
            string exportedBy,
            ExportOptions exportOptions)
        {
            var worksheet = package.Workbook.Worksheets.Add("Reconciliation Details");
            ApplyDefaultStyle(worksheet);

            int headerColumns = 8;
            string headerEndColumn = GetColumnLetter(headerColumns);
            int currentRow = CreateHeaderSection(worksheet, GetBankName(), GetBranchCode(), GetBranchID(), GetBranchName(),
                exportedBy, exportOptions, "RECONCILIATION TRANSACTIONS DETAILED REPORT", headerEndColumn);

            int totalRecords = data.Count;
            var statusDistribution = data
                .GroupBy(x => x.Status ?? "Unknown")
                .Select(g => new { Status = g.Key, Count = g.Count() }).ToList();

            currentRow = CreateDistributionSection(worksheet, currentRow, headerEndColumn,
                "STATUS OVERVIEW", statusDistribution, totalRecords, Color.LightYellow);
            currentRow += 2;

            worksheet.Cells[$"A{currentRow}:{headerEndColumn}{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = "DETAILED RECONCILIATION ENTRIES";
            SetSectionHeaderStyle(worksheet.Cells[$"A{currentRow}"], Color.LightCoral);
            currentRow += 2;

            var headers = new[]
            {
            "SN", "Reference", "Operation Code", "Branch Name", "Status",
            "Operation By", "Created Date", "Accounting Date"
        };

            int headerRow = currentRow;
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[headerRow, i + 1].Value = headers[i];
                SetHeaderStyle(worksheet.Cells[headerRow, i + 1], Color.LightGreen);
            }
            currentRow++;

            int sn = 1;

            foreach (var rec in data.OrderByDescending(x => x.CreatedDate))
            {
                worksheet.Cells[currentRow, 1].Value = sn++;
                worksheet.Cells[currentRow, 2].Value = rec.Reference;
                worksheet.Cells[currentRow, 3].Value = rec.OperationCode;
                worksheet.Cells[currentRow, 4].Value = rec.BranchName;
                worksheet.Cells[currentRow, 5].Value = rec.Status;
                worksheet.Cells[currentRow, 6].Value = rec.OperationBy;
                worksheet.Cells[currentRow, 7].Value = rec.CreatedDate?.ToString("dd-MM-yyyy HH:mm:ss");
                worksheet.Cells[currentRow, 8].Value = rec.AccountingDate?.ToString("dd-MM-yyyy HH:mm:ss");

                for (int col = 1; col <= headerColumns; col++)
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                currentRow++;
            }

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            worksheet.View.FreezePanes(headerRow + 1, 1);
        }

        // -----------------------------------------------------------------------
        // SHEET: INDIVIDUAL BRANCH DETAILS (with safe sheet naming)
        // -----------------------------------------------------------------------
        private void CreateBranchSheet(
            ExcelPackage package,
            List<Reconciliation> branchData,
            string branchId,      // Not used, kept for signature compatibility
            string branchName,
            string branchCode,    // Not used
            string exportedBy,
            ExportOptions exportOptions)
        {
            // --- Safe sheet name generation ---
            string rawName = !string.IsNullOrWhiteSpace(branchName) ? branchName : "Unknown Branch";
            string sheetName = CleanSheetName(rawName);
            if (string.IsNullOrWhiteSpace(sheetName))
            {
                sheetName = "Branch"; // ultimate fallback
            }
            if (sheetName.Length > 31) sheetName = sheetName.Substring(0, 31);
            // ---------------------------------

            var worksheet = package.Workbook.Worksheets.Add(sheetName);
            ApplyDefaultStyle(worksheet);

            var branchHeaders = new[]
            {
            "SN", "Reference", "Operation Code", "Status",
            "Operation By", "Created Date", "Accounting Date"
        };
            int headerColumns = branchHeaders.Length;
            string headerEndColumn = GetColumnLetter(headerColumns);
            int currentRow = CreateHeaderSection(worksheet, GetBankName(), branchCode, branchId, branchName,
                exportedBy, exportOptions, $"BRANCH RECONCILIATION REPORT - {branchName}", headerEndColumn);

            // ----- DETAILED ENTRIES -----
            worksheet.Cells[$"A{currentRow}:{headerEndColumn}{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = $"DETAILED RECONCILIATION ENTRIES - {branchName}";
            SetSectionHeaderStyle(worksheet.Cells[$"A{currentRow}"], Color.LightCoral);
            currentRow += 2;

            int headerRow = currentRow;
            for (int i = 0; i < branchHeaders.Length; i++)
            {
                worksheet.Cells[headerRow, i + 1].Value = branchHeaders[i];
                SetHeaderStyle(worksheet.Cells[headerRow, i + 1], Color.LightGreen);
            }
            currentRow++;

            int sn = 1;

            foreach (var rec in branchData.OrderByDescending(x => x.CreatedDate))
            {
                worksheet.Cells[currentRow, 1].Value = sn++;
                worksheet.Cells[currentRow, 2].Value = rec.Reference;
                worksheet.Cells[currentRow, 3].Value = rec.OperationCode;
                worksheet.Cells[currentRow, 4].Value = rec.Status;
                worksheet.Cells[currentRow, 5].Value = rec.OperationBy;
                worksheet.Cells[currentRow, 6].Value = rec.CreatedDate?.ToString("dd-MM-yyyy HH:mm:ss");
                worksheet.Cells[currentRow, 7].Value = rec.AccountingDate?.ToString("dd-MM-yyyy HH:mm:ss");

                for (int col = 1; col <= headerColumns; col++)
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                currentRow++;
            }

            currentRow += 2;

            // ----- BRANCH SUMMARY (TWO-COLUMN METRICS) -----
            worksheet.Cells[$"A{currentRow}:{headerEndColumn}{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = $"BRANCH SUMMARY - {branchName}";
            SetSectionHeaderStyle(worksheet.Cells[$"A{currentRow}"], Color.LightBlue);
            currentRow += 2;

            int total = branchData.Count;
            int pending = branchData.Count(x => x.Status == "Pending");
            int completed = branchData.Count(x => x.Status == "Completed");
            int failed = branchData.Count(x => x.Status == "Failed");
            int other = total - pending - completed - failed;

            var summaryItems = new[]
            {
            new { Metric = "Total Records", Value = total.ToString("N0") },
            new { Metric = "Pending", Value = pending.ToString("N0") },
            new { Metric = "Completed", Value = completed.ToString("N0") },
            new { Metric = "Failed", Value = failed.ToString("N0") },
            new { Metric = "Other Status", Value = other.ToString("N0") }
        };

            currentRow = CreateTwoColumnTable(worksheet, currentRow, summaryItems, "Metric", "Value");
            currentRow += 2;

            // ----- STATUS DISTRIBUTION -----
            var statusDist = branchData
                .GroupBy(x => x.Status ?? "Unknown")
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToList();

            currentRow = CreateDistributionSection(worksheet, currentRow, headerEndColumn,
                "STATUS DISTRIBUTION", statusDist, total, Color.LightYellow);

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            worksheet.View.FreezePanes(headerRow + 1, 1);
        }

        // -----------------------------------------------------------------------
        // HELPER METHODS (unchanged)
        // -----------------------------------------------------------------------
        private void ApplyDefaultStyle(ExcelWorksheet worksheet)
        {
            worksheet.Cells.Style.Font.Name = "Bahnschrift SemiCondensed";
        }

        private int CreateHeaderSection(
            ExcelWorksheet worksheet,
            string bank,
            string branchCode,
            string branchId,
            string branchName,
            string exportedBy,
            ExportOptions exportOptions,
            string reportTitle,
            string headerEndColumn)
        {
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

            worksheet.Cells[$"A2:{headerEndColumn}2"].Merge = true;
            worksheet.Cells["A2"].Value = $"Branch Code: {branchCode} | Branch: {branchName} | Branch ID: {branchId}";
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

            worksheet.Cells[$"A4:{headerEndColumn}4"].Merge = true;
            worksheet.Cells["A4"].Value = $"Export Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
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

            if (exportOptions != null &&
                !string.IsNullOrEmpty(exportOptions.StartDate) &&
                !string.IsNullOrEmpty(exportOptions.EndDate))
            {
                worksheet.Cells[$"A6:{headerEndColumn}6"].Merge = true;
                worksheet.Cells["A6"].Value = $"Date Range: {exportOptions.StartDate} to {exportOptions.EndDate}";
                worksheet.Cells["A6"].Style.Font.Bold = true;
                worksheet.Cells["A6"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                worksheet.Cells["A6"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Row(6).Height = 18;
                return 8;
            }

            return 7;
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
