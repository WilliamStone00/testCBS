using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using CBS.FrontDesk.Data.Entity.Accounting_V2.FallBack;

namespace CBS.BusinessService.Accounting_V2.Affiliate
{
    public class FallbackExcelExportGenerator
    {
        public static List<FallbackExportRecord> ConvertToFallbackData(List<FallbackExportRecord> tableData)
        {
            // Since we're now using strongly-typed entities, we can return the data directly
            // This method is kept for consistency with the existing pattern
            var fallbackList = new List<FallbackExportRecord>();

            if (tableData == null || !tableData.Any())
            {
                Console.WriteLine("No fallback table data provided for conversion");
                return fallbackList;
            }

            Console.WriteLine($"Processing {tableData.Count} fallback records for export");

            // Add any additional processing or validation here if needed
            foreach (var record in tableData)
            {
                // Ensure consistent status display
                if (string.IsNullOrEmpty(record.Status))
                {
                    record.Status = record.IsBranchAccountAutoCreated ? "Auto-Created" : "Manual";
                }

                // Ensure auto-created status is set
                if (string.IsNullOrEmpty(record.AutoCreatedStatus))
                {
                    record.AutoCreatedStatus = record.IsBranchAccountAutoCreated ? "Auto-Created" : "Manual";
                }

                fallbackList.Add(record);
            }

            Console.WriteLine($"Successfully processed {fallbackList.Count} fallback records");
            return fallbackList;
        }

        public void GenerateFallbackExcelFromTableData(List<FallbackExportRecord> fallbackData, string filePath, string exportedBy, FallbackExportOptions exportOptions)
        {
            // Set EPPlus license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                // Create main worksheet
                CreateFallbackMainSheet(package, fallbackData, exportedBy, exportOptions);

                // Create summary worksheet if requested
                if (exportOptions?.IncludeStatistics == true)
                {
                    CreateFallbackSummarySheet(package, fallbackData, exportedBy, exportOptions);
                }

                // Save the file
                package.SaveAs(new FileInfo(filePath));

                Console.WriteLine($"Fallback Excel file generated successfully with {fallbackData.Count} records");
                Console.WriteLine($"File saved to: {filePath}");
            }
        }

        private void CreateFallbackMainSheet(ExcelPackage package, List<FallbackExportRecord> fallbackData, string exportedBy, FallbackExportOptions exportOptions)
        {
            var worksheet = package.Workbook.Worksheets.Add("Fallback Records");

            // Set font
            var fontName = "Bahnschrift SemiCondensed";
            worksheet.Cells.Style.Font.Name = fontName;

            // Header Section
            CreateFallbackHeaderSection(worksheet, exportedBy, exportOptions, fallbackData.Count);

            int currentRow = 6;

            // Table Headers
            var headers = new[]
            {
                "SN", "Branch Name", "Operation Code", "Reference",
                "Supposed GL Account", "Supposed GL Name",
                "Fallback GL Account", "Fallback GL Name",
                "Amount", "Status", "Created Date", "Resolved On", "Resolved By",
                "Account Type", "Resolution Tips"
            };

            int headerRow = currentRow;
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[headerRow, i + 1].Value = headers[i];
                worksheet.Cells[headerRow, i + 1].Style.Font.Bold = true;
                worksheet.Cells[headerRow, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[headerRow, i + 1].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                worksheet.Cells[headerRow, i + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                worksheet.Cells[headerRow, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            // Data rows
            int dataRow = headerRow + 1;
            int serialNumber = 1;
            decimal totalAmount = 0;
            int resolvedCount = 0;
            int unresolvedCount = 0;

            foreach (var record in fallbackData.OrderBy(x => x.BranchName).ThenBy(x => x.CreatedDate))
            {
                worksheet.Cells[dataRow, 1].Value = serialNumber++;
                worksheet.Cells[dataRow, 2].Value = record.BranchName;
                worksheet.Cells[dataRow, 3].Value = record.OperationCode;
                worksheet.Cells[dataRow, 4].Value = record.Reference;
                worksheet.Cells[dataRow, 5].Value = record.SupposedGlAccountNumber;
                worksheet.Cells[dataRow, 6].Value = record.SupposedGlAccountName;
                worksheet.Cells[dataRow, 7].Value = record.FallbackBranchAccountNumber;
                worksheet.Cells[dataRow, 8].Value = record.FallbackBranchAccountName;
                worksheet.Cells[dataRow, 9].Value = record.Amount;
                worksheet.Cells[dataRow, 10].Value = record.Status;
                worksheet.Cells[dataRow, 11].Value = record.CreatedDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "-";
                worksheet.Cells[dataRow, 12].Value = record.ResolvedOn?.ToString("yyyy-MM-dd HH:mm:ss") ?? "-";
                worksheet.Cells[dataRow, 13].Value = record.ResolvedBy ?? "-";
                worksheet.Cells[dataRow, 14].Value = record.AutoCreatedStatus;
                worksheet.Cells[dataRow, 15].Value = record.ResolutionTips;

                // Apply borders
                for (int col = 1; col <= headers.Length; col++)
                {
                    worksheet.Cells[dataRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                // Apply conditional formatting for status
                if (record.Status == "Resolved")
                {
                    worksheet.Cells[dataRow, 10].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[dataRow, 10].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                }
                else
                {
                    worksheet.Cells[dataRow, 10].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[dataRow, 10].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
                }

                totalAmount += record.Amount;
                if (record.Status == "Resolved")
                    resolvedCount++;
                else
                    unresolvedCount++;

                dataRow++;
            }

            // Totals row
            if (fallbackData.Any())
            {
                worksheet.Cells[dataRow, 1].Value = "TOTALS:";
                worksheet.Cells[dataRow, 1].Style.Font.Bold = true;
                worksheet.Cells[dataRow, 9].Value = totalAmount;
                worksheet.Cells[dataRow, 10].Value = $"Resolved: {resolvedCount}, Pending: {unresolvedCount}";

                for (int col = 1; col <= headers.Length; col++)
                {
                    worksheet.Cells[dataRow, col].Style.Font.Bold = true;
                    worksheet.Cells[dataRow, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[dataRow, col].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
                    worksheet.Cells[dataRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
            }

            // Format numbers and dates
            if (fallbackData.Any())
            {
                worksheet.Cells[$"I{headerRow + 1}:I{dataRow}"].Style.Numberformat.Format = "#,##0.00";
            }

            // Auto-fit columns
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            // Freeze panes for easy scrolling
            worksheet.View.FreezePanes(headerRow + 1, 1);
        }

        private void CreateFallbackSummarySheet(ExcelPackage package, List<FallbackExportRecord> fallbackData, string exportedBy, FallbackExportOptions exportOptions)
        {
            var worksheet = package.Workbook.Worksheets.Add("Summary & Statistics");

            // Set font
            var fontName = "Bahnschrift SemiCondensed";
            worksheet.Cells.Style.Font.Name = fontName;

            // Header Section
            CreateFallbackHeaderSection(worksheet, exportedBy, exportOptions, fallbackData.Count);

            int currentRow = 6;

            // Summary Statistics
            worksheet.Cells[$"A{currentRow}:G{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = "FALLBACK RECONCILIATION SUMMARY";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Size = 14;
            worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
            currentRow += 2;

            // Calculate statistics
            var totalRecords = fallbackData.Count;
            var totalAmount = fallbackData.Sum(x => x.Amount);
            var resolvedCount = fallbackData.Count(x => x.Status == "Resolved");
            var unresolvedCount = totalRecords - resolvedCount;
            var autoCreatedCount = fallbackData.Count(x => x.IsBranchAccountAutoCreated);
            var branches = fallbackData.Select(x => x.BranchName).Distinct().Count();
            var operationCodes = fallbackData.Select(x => x.OperationCode).Distinct().Count();

            // Statistics table headers
            var statsHeaders = new[] { "Metric", "Value", "Description" };
            for (int i = 0; i < statsHeaders.Length; i++)
            {
                worksheet.Cells[currentRow, 1 + i].Value = statsHeaders[i];
                worksheet.Cells[currentRow, 1 + i].Style.Font.Bold = true;
                worksheet.Cells[currentRow, 1 + i].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[currentRow, 1 + i].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                worksheet.Cells[currentRow, 1 + i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }
            currentRow++;

            // Statistics data
            var statsData = new[]
            {
                new { Metric = "Total Records", Value = totalRecords.ToString("N0"), Description = "Number of fallback records" },
                new { Metric = "Total Amount", Value = totalAmount.ToString("N2"), Description = "Sum of all amounts" },
                new { Metric = "Resolved Records", Value = resolvedCount.ToString("N0"), Description = "Records marked as resolved" },
                new { Metric = "Pending Records", Value = unresolvedCount.ToString("N0"), Description = "Records pending reconciliation" },
                new { Metric = "Auto-Created Accounts", Value = autoCreatedCount.ToString("N0"), Description = "Accounts auto-created by system" },
                new { Metric = "Manual Accounts", Value = (totalRecords - autoCreatedCount).ToString("N0"), Description = "Manually configured accounts" },
                new { Metric = "Branches Involved", Value = branches.ToString("N0"), Description = "Unique branches with fallback records" },
                new { Metric = "Operation Types", Value = operationCodes.ToString("N0"), Description = "Unique operation codes" },
                new { Metric = "Resolution Rate", Value = totalRecords > 0 ? ((decimal)resolvedCount / totalRecords).ToString("P1") : "0%", Description = "Percentage of resolved records" }
            };

            foreach (var item in statsData)
            {
                worksheet.Cells[currentRow, 1].Value = item.Metric;
                worksheet.Cells[currentRow, 2].Value = item.Value;
                worksheet.Cells[currentRow, 3].Value = item.Description;

                for (int col = 1; col <= 3; col++)
                {
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
                currentRow++;
            }

            currentRow += 2;

            // Branch Breakdown
            worksheet.Cells[$"A{currentRow}:G{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = "BRANCH BREAKDOWN";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Size = 14;
            worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.BackgroundColor.SetColor(Color.LightCoral);
            currentRow += 2;

            // Branch breakdown headers
            var branchHeaders = new[] { "Branch Name", "Records", "Resolved", "Pending", "Total Amount", "Avg Amount" };
            for (int i = 0; i < branchHeaders.Length; i++)
            {
                worksheet.Cells[currentRow, 1 + i].Value = branchHeaders[i];
                worksheet.Cells[currentRow, 1 + i].Style.Font.Bold = true;
                worksheet.Cells[currentRow, 1 + i].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[currentRow, 1 + i].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                worksheet.Cells[currentRow, 1 + i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }
            currentRow++;

            // Branch breakdown data
            var branchSummary = fallbackData
                .GroupBy(x => x.BranchName)
                .Select(g => new
                {
                    BranchName = g.Key,
                    Records = g.Count(),
                    Resolved = g.Count(x => x.Status == "Resolved"),
                    Pending = g.Count(x => x.Status != "Resolved"),
                    TotalAmount = g.Sum(x => x.Amount),
                    AvgAmount = g.Average(x => x.Amount)
                })
                .OrderByDescending(x => x.TotalAmount)
                .ToList();

            foreach (var branch in branchSummary)
            {
                worksheet.Cells[currentRow, 1].Value = branch.BranchName;
                worksheet.Cells[currentRow, 2].Value = branch.Records;
                worksheet.Cells[currentRow, 3].Value = branch.Resolved;
                worksheet.Cells[currentRow, 4].Value = branch.Pending;
                worksheet.Cells[currentRow, 5].Value = branch.TotalAmount;
                worksheet.Cells[currentRow, 6].Value = branch.AvgAmount;

                for (int col = 1; col <= 6; col++)
                {
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
                currentRow++;
            }

            // Format numbers
            if (branchSummary.Any())
            {
                worksheet.Cells[$"E{currentRow - branchSummary.Count}:F{currentRow - 1}"].Style.Numberformat.Format = "#,##0.00";
            }

            // Auto-fit columns
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        }

        private void CreateFallbackHeaderSection(ExcelWorksheet worksheet, string exportedBy, FallbackExportOptions exportOptions, int recordCount)
        {
            // Title
            worksheet.Cells["A1:G1"].Merge = true;
            worksheet.Cells["A1"].Value = "FALLBACK RECONCILIATION REPORT";
            worksheet.Cells["A1"].Style.Font.Bold = true;
            worksheet.Cells["A1"].Style.Font.Size = 16;
            worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["A1"].Style.Font.Color.SetColor(Color.White);
            worksheet.Cells["A1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["A1"].Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);

            // Export Information
            worksheet.Cells["A2"].Value = "Exported By:";
            worksheet.Cells["B2"].Value = exportedBy;
            worksheet.Cells["A3"].Value = "Export Date:";
            worksheet.Cells["B3"].Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            worksheet.Cells["A4"].Value = "Total Records:";
            worksheet.Cells["B4"].Value = recordCount.ToString("N0");

            worksheet.Cells["A2:A4"].Style.Font.Bold = true;

            if (exportOptions != null && !string.IsNullOrEmpty(exportOptions.FileName))
            {
                worksheet.Cells["D2"].Value = "Report:";
                worksheet.Cells["E2"].Value = exportOptions.FileName;
                worksheet.Cells["D2"].Style.Font.Bold = true;
            }

            if (exportOptions != null && !string.IsNullOrEmpty(exportOptions.StartDate) && !string.IsNullOrEmpty(exportOptions.EndDate))
            {
                worksheet.Cells["D3"].Value = "Date Range:";
                worksheet.Cells["E3"].Value = $"{exportOptions.StartDate} to {exportOptions.EndDate}";
                worksheet.Cells["D3"].Style.Font.Bold = true;
            }
        }
    }
}