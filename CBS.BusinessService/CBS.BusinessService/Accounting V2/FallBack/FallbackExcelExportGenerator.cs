using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using CBS.FrontDesk.Data.Entity.Accounting_V2.FallBack;
using BusinessServices;

namespace CBS.BusinessService.Accounting_V2.Affiliate
{
    public class FallbackExcelExportGenerator : BaseService
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
            string bank = GetBankName();
            string branchcode = GetBranchCode();
            string Branchid = GetBranchID();
            string branchname = GetBranchName();

            // Set EPPlus license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                // ===== SHEET 1: SUMMARY & OVERVIEW =====
                CreateSummarySheet(package, fallbackData, bank, branchcode, Branchid, branchname, exportedBy, exportOptions);

                // ===== SHEETS FOR EACH BRANCH: Create individual sheets for each branch =====
                var branches = fallbackData.GroupBy(x => new { x.BranchId, x.BranchName })
                                         .Select(g => new { BranchId = g.Key.BranchId, BranchName = g.Key.BranchName })
                                         .ToList();

                foreach (var branch in branches)
                {
                    var branchData = fallbackData.Where(x => x.BranchId == branch.BranchId && x.BranchName == branch.BranchName).ToList();
                    CreateBranchSheet(package, branchData, branch.BranchId, branch.BranchName, exportedBy, exportOptions);
                }

                // Save the file
                package.SaveAs(new FileInfo(filePath));

                Console.WriteLine($"Excel file generated successfully with {fallbackData.Count} records across {branches.Count} branches");
                Console.WriteLine($"File saved to: {filePath}");
            }
        }

        private void CreateSummarySheet(ExcelPackage package, List<FallbackExportRecord> fallbackData, string bank, string branchcode, string Branchid, string branchname, string exportedBy, FallbackExportOptions exportOptions)
        {
            var worksheet = package.Workbook.Worksheets.Add("Summary & Overview");

            // Set font
            var fontName = "Bahnschrift SemiCondensed";
            worksheet.Cells.Style.Font.Name = fontName;

            // ===== HEADER SECTION =====
            CreateHeaderSection(worksheet, bank, branchcode, Branchid, branchname, exportedBy, exportOptions);

            int currentRow = 8; // Start after header

            // ===== GENERAL SUMMARY SECTION =====
            worksheet.Cells[$"A{currentRow}:G{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = "FALLBACK RECONCILIATION SUMMARY";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Size = 14;
            worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
            currentRow += 2;

            // Calculate summary statistics
            var totalRecords = fallbackData.Count;
            var totalAmount = fallbackData.Sum(x => x.Amount);
            var resolvedCount = fallbackData.Count(x => x.Status == "Resolved");
            var unresolvedCount = totalRecords - resolvedCount;
            var autoCreatedCount = fallbackData.Count(x => x.IsBranchAccountAutoCreated);
            var branches = fallbackData.Select(x => new { x.BranchId, x.BranchName }).Distinct().Count();
            var operationCodes = fallbackData.Select(x => x.OperationCode).Distinct().Count();

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

            // Summary data
            var summaryData = new[]
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

            foreach (var item in summaryData)
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

            // ===== RESOLUTION GUIDELINES SECTION =====
            worksheet.Cells[$"A{currentRow}:G{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = "RESOLUTION GUIDELINES & TIPS";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Size = 14;
            worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
            currentRow += 2;

            // Resolution tips and guidelines
            var resolutionTips = new[]
            {
                "How to resolve fallback transactions:",
                "1) Open the 'Fallback Affiliate Mapping' console for the specific branch",
                "2) Filter by Reference / Operation to locate the record",
                "3) Check the 'Supposed GL' (number & name) and confirm the correct Branch GL",
                "4) Post a manual GL journal: DR/CR the suspense/auto GL, CR/DR the correct GL for the amount shown",
                "5) Update the Affiliate→Branch mapping so future postings use the correct GL automatically",
                "6) Mark the fallback record as 'Resolved' in the console",
                "",
                "NOTE: Auto-created GL accounts were generated by the system because no fallback suspense GL",
                "was configured or the configured GL did not exist at the time of transaction"
            };

            foreach (var tip in resolutionTips)
            {
                worksheet.Cells[$"A{currentRow}:G{currentRow}"].Merge = true;
                worksheet.Cells[$"A{currentRow}"].Value = tip;
                if (tip.StartsWith("How to resolve") || tip.StartsWith("NOTE:"))
                {
                    worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
                }
                else if (tip.StartsWith("1)") || tip.StartsWith("2)") || tip.StartsWith("3)") || tip.StartsWith("4)") || tip.StartsWith("5)") || tip.StartsWith("6)"))
                {
                    worksheet.Cells[$"A{currentRow}"].Style.Font.Italic = true;
                }
                currentRow++;
            }

            currentRow += 2;

            // ===== BRANCH SUMMARY SECTION =====
            worksheet.Cells[$"A{currentRow}:G{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = "BRANCH SUMMARY";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Size = 14;
            worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.BackgroundColor.SetColor(Color.LightCoral);
            currentRow += 2;

            // Branch summary headers
            var branchHeaders = new[] { "Branch ID", "Branch Name", "Records", "Resolved", "Pending", "Total Amount", "Avg Amount" };
            for (int i = 0; i < branchHeaders.Length; i++)
            {
                worksheet.Cells[currentRow, 1 + i].Value = branchHeaders[i];
                worksheet.Cells[currentRow, 1 + i].Style.Font.Bold = true;
                worksheet.Cells[currentRow, 1 + i].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[currentRow, 1 + i].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                worksheet.Cells[currentRow, 1 + i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }
            currentRow++;

            // Branch summary data
            var branchSummary = fallbackData.GroupBy(x => new { x.BranchId, x.BranchName })
                                          .Select(g => new
                                          {
                                              BranchId = g.Key.BranchId,
                                              BranchName = g.Key.BranchName,
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
                worksheet.Cells[currentRow, 1].Value = branch.BranchId;
                worksheet.Cells[currentRow, 2].Value = branch.BranchName;
                worksheet.Cells[currentRow, 3].Value = branch.Records;
                worksheet.Cells[currentRow, 4].Value = branch.Resolved;
                worksheet.Cells[currentRow, 5].Value = branch.Pending;
                worksheet.Cells[currentRow, 6].Value = branch.TotalAmount;
                worksheet.Cells[currentRow, 7].Value = branch.AvgAmount;

                for (int col = 1; col <= 7; col++)
                {
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
                currentRow++;
            }

            // Format numbers
            worksheet.Cells[$"F{currentRow - branchSummary.Count}:G{currentRow - 1}"].Style.Numberformat.Format = "#,##0.00";

            // Auto-fit columns
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        }

        private void CreateBranchSheet(ExcelPackage package, List<FallbackExportRecord> branchData, string branchId, string branchName, string exportedBy, FallbackExportOptions exportOptions)
        {
            // Clean sheet name (Excel has restrictions on sheet names)
            var cleanSheetName = CleanSheetName($"{branchId} - {branchName}");
            if (cleanSheetName.Length > 31) cleanSheetName = cleanSheetName.Substring(0, 31);

            var worksheet = package.Workbook.Worksheets.Add(cleanSheetName);

            // Set font
            var fontName = "Bahnschrift SemiCondensed";
            worksheet.Cells.Style.Font.Name = fontName;

            // ===== HEADER SECTION =====
            CreateHeaderSection(worksheet, GetBankName(), branchId, GetBranchID(), branchName, exportedBy, exportOptions);

            int currentRow = 8; // Start after header

            // ===== BRANCH SUMMARY SECTION =====
            worksheet.Cells[$"A{currentRow}:G{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = $"BRANCH SUMMARY - {branchName} ({branchId})";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Size = 14;
            worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
            currentRow += 2;

            // Calculate branch statistics
            var totalRecords = branchData.Count;
            var totalAmount = branchData.Sum(x => x.Amount);
            var resolvedCount = branchData.Count(x => x.Status == "Resolved");
            var unresolvedCount = totalRecords - resolvedCount;
            var autoCreatedCount = branchData.Count(x => x.IsBranchAccountAutoCreated);
            var operationCodes = branchData.Select(x => x.OperationCode).Distinct().Count();

            // Branch summary table
            var branchSummaryHeaders = new[] { "Metric", "Value" };
            for (int i = 0; i < branchSummaryHeaders.Length; i++)
            {
                worksheet.Cells[currentRow, 1 + i].Value = branchSummaryHeaders[i];
                worksheet.Cells[currentRow, 1 + i].Style.Font.Bold = true;
                worksheet.Cells[currentRow, 1 + i].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[currentRow, 1 + i].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                worksheet.Cells[currentRow, 1 + i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }
            currentRow++;

            var branchSummaryData = new[]
            {
                new { Metric = "Total Records", Value = totalRecords.ToString("N0") },
                new { Metric = "Total Amount", Value = totalAmount.ToString("N2") },
                new { Metric = "Resolved Records", Value = resolvedCount.ToString("N0") },
                new { Metric = "Pending Records", Value = unresolvedCount.ToString("N0") },
                new { Metric = "Auto-Created Accounts", Value = autoCreatedCount.ToString("N0") },
                new { Metric = "Manual Accounts", Value = (totalRecords - autoCreatedCount).ToString("N0") },
                new { Metric = "Operation Types", Value = operationCodes.ToString("N0") },
                new { Metric = "Resolution Rate", Value = totalRecords > 0 ? ((decimal)resolvedCount / totalRecords).ToString("P1") : "0%" }
            };

            foreach (var item in branchSummaryData)
            {
                worksheet.Cells[currentRow, 1].Value = item.Metric;
                worksheet.Cells[currentRow, 2].Value = item.Value;

                for (int col = 1; col <= 2; col++)
                {
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
                currentRow++;
            }

            currentRow += 2;

            // ===== BRANCH RESOLUTION TIPS SECTION =====
            worksheet.Cells[$"A{currentRow}:G{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = $"RESOLUTION GUIDELINES - {branchName}";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Size = 14;
            worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
            currentRow += 2;

            // Branch-specific resolution tips
            var branchResolutionTips = new[]
            {
                $"For branch {branchName} ({branchId}):",
                "1) Review all pending fallback records in this branch",
                "2) Identify patterns in operation codes and GL accounts",
                "3) Update affiliate-to-branch mappings to prevent future fallbacks",
                "4) Resolve high-amount transactions first",
                "5) Ensure proper GL account configuration for common operations",
                "",
                "Common resolution steps:",
                "- Verify supposed GL account exists and is active",
                "- Create missing GL accounts if necessary",
                "- Update transaction mappings in the system",
                "- Post corrective journal entries for resolved items"
            };

            foreach (var tip in branchResolutionTips)
            {
                worksheet.Cells[$"A{currentRow}:G{currentRow}"].Merge = true;
                worksheet.Cells[$"A{currentRow}"].Value = tip;
                if (tip.StartsWith("For branch") || tip.StartsWith("Common resolution"))
                {
                    worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
                }
                else if (tip.StartsWith("1)") || tip.StartsWith("2)") || tip.StartsWith("3)") || tip.StartsWith("4)") || tip.StartsWith("5)") || tip.StartsWith("-"))
                {
                    worksheet.Cells[$"A{currentRow}"].Style.Font.Italic = true;
                }
                currentRow++;
            }

            currentRow += 2;

            // ===== DETAILED DATA TABLE =====
            worksheet.Cells[$"A{currentRow}:G{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = "DETAILED FALLBACK RECORDS";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Size = 14;
            worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.BackgroundColor.SetColor(Color.LightCoral);
            currentRow += 2;

            // Detailed data headers
            var headers = new[]
            {
                "SN", "Operation Code", "Reference",
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
            //decimal totalAmount = 0;

            foreach (var record in branchData.OrderBy(x => x.OperationCode).ThenBy(x => x.CreatedDate))
            {
                worksheet.Cells[dataRow, 1].Value = serialNumber++;
                worksheet.Cells[dataRow, 2].Value = record.OperationCode;
                worksheet.Cells[dataRow, 3].Value = record.Reference;
                worksheet.Cells[dataRow, 4].Value = record.SupposedGlAccountNumber;
                worksheet.Cells[dataRow, 5].Value = record.SupposedGlAccountName;
                worksheet.Cells[dataRow, 6].Value = record.FallbackBranchAccountNumber;
                worksheet.Cells[dataRow, 7].Value = record.FallbackBranchAccountName;
                worksheet.Cells[dataRow, 8].Value = record.Amount;
                worksheet.Cells[dataRow, 9].Value = record.Status;
                worksheet.Cells[dataRow, 10].Value = record.CreatedDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "-";
                worksheet.Cells[dataRow, 11].Value = record.ResolvedOn?.ToString("yyyy-MM-dd HH:mm:ss") ?? "-";
                worksheet.Cells[dataRow, 12].Value = record.ResolvedBy ?? "-";
                worksheet.Cells[dataRow, 13].Value = record.AutoCreatedStatus;
                worksheet.Cells[dataRow, 14].Value = record.ResolutionTips;

                // Apply borders
                for (int col = 1; col <= headers.Length; col++)
                {
                    worksheet.Cells[dataRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                // Apply conditional formatting for status
                if (record.Status == "Resolved")
                {
                    worksheet.Cells[dataRow, 9].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[dataRow, 9].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                }
                else
                {
                    worksheet.Cells[dataRow, 9].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[dataRow, 9].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
                }

                totalAmount += record.Amount;
                dataRow++;
            }

            // Totals row
            if (branchData.Any())
            {
                worksheet.Cells[dataRow, 1].Value = "TOTALS:";
                worksheet.Cells[dataRow, 1].Style.Font.Bold = true;
                worksheet.Cells[dataRow, 8].Value = totalAmount;
                worksheet.Cells[dataRow, 9].Value = $"Resolved: {resolvedCount}, Pending: {unresolvedCount}";

                for (int col = 1; col <= headers.Length; col++)
                {
                    worksheet.Cells[dataRow, col].Style.Font.Bold = true;
                    worksheet.Cells[dataRow, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[dataRow, col].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
                    worksheet.Cells[dataRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
            }

            // Format numbers
            if (branchData.Any())
            {
                worksheet.Cells[$"H{headerRow + 1}:H{dataRow}"].Style.Numberformat.Format = "#,##0.00";
            }

            // Auto-fit columns
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            // Freeze panes for easy scrolling
            worksheet.View.FreezePanes(headerRow + 1, 1);
        }

        private void CreateHeaderSection(ExcelWorksheet worksheet, string bank, string branchcode, string Branchid, string branchname, string exportedBy, FallbackExportOptions exportOptions)
        {
            const string lastColumnLetter = "AE";

            // ===== Bank Information at the TOP =====
            worksheet.Cells[$"A1:{lastColumnLetter}1"].Merge = true;
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
            worksheet.Cells[$"A2:{lastColumnLetter}2"].Merge = true;
            worksheet.Cells["A2"].Value = $"Branch Code: {branchcode} | Branch: {branchname} | Branch ID: {Branchid}";
            worksheet.Cells["A2"].Style.Font.Bold = true;
            worksheet.Cells["A2"].Style.Font.Size = 12;
            worksheet.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells["A2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Row(2).Height = 22;
            worksheet.Cells["A2"].Style.Font.Color.SetColor(Color.White);
            worksheet.Cells["A2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["A2"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(70, 130, 180));

            // ===== Export meta =====
            worksheet.Cells[$"A3:{lastColumnLetter}3"].Merge = true;
            worksheet.Cells["A3"].Value = $"Exported By: {exportedBy}";
            worksheet.Cells["A3"].Style.Font.Bold = true;
            worksheet.Cells["A3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells["A3"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Row(3).Height = 18;

            worksheet.Cells[$"A4:{lastColumnLetter}4"].Merge = true;
            worksheet.Cells["A4"].Value = $"Export Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            worksheet.Cells["A4"].Style.Font.Bold = true;
            worksheet.Cells["A4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells["A4"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Row(4).Height = 18;

            // ===== Report Title =====
            worksheet.Cells[$"A5:{lastColumnLetter}5"].Merge = true;
            worksheet.Cells["A5"].Value = "FALLBACK RECONCILIATION REPORT";
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
                worksheet.Cells[$"A6:{lastColumnLetter}6"].Merge = true;
                worksheet.Cells["A6"].Value = $"Date Range: {exportOptions.StartDate} to {exportOptions.EndDate}";
                worksheet.Cells["A6"].Style.Font.Bold = true;
                worksheet.Cells["A6"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                worksheet.Cells["A6"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Row(6).Height = 18;
            }
        }

        private string CleanSheetName(string name)
        {
            // Excel sheet name restrictions
            var invalidChars = new char[] { '\\', '/', '*', '?', ':', '[', ']' };
            foreach (var invalidChar in invalidChars)
            {
                name = name.Replace(invalidChar, ' ');
            }

            // Remove extra spaces and trim
            name = string.Join(" ", name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));

            return name.Trim();
        }
    }
}