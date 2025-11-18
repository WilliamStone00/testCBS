using CBS.FrontDesk.Data.Entity.Accounting_V2.DaillyCollectorCommission;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using BusinessServices;

namespace CBS.BusinessService.Accounting_V2.Affiliate
{
    public  class CommissionExcelExportGenerator : BaseService
    {
        // ===================================================================
        // DATA TABLE EXPORT METHODS (The ones we're focusing on)
        // ===================================================================

        // Export Data table - IMPROVED VERSION
        public static List<DataTableResponse> ConvertToCommissionData(List<DataTableResponse> tableData)
        {
            var commissionList = new List<DataTableResponse>();

            if (tableData == null || !tableData.Any())
            {
                Console.WriteLine("No table data provided for conversion");
                return commissionList;
            }

            Console.WriteLine($"Converting {tableData.Count} records to CommissionData");

            foreach (var item in tableData)
            {
                try
                {
                    // Use dynamic access with fallbacks
                    var commission = new DataTableResponse
                    {
                        // Use TryGetProperty helper for safe property access
                       // Id = TryGetProperty(item, "id", "Id")?.ToString() ?? "",
                        //CollectorId = TryGetProperty(item, "collectorId", "CollectorId")?.ToString() ?? "",
                        CollectorName = TryGetProperty(item, "collectorName", "CollectorName")?.ToString() ?? "",
                        CollectorPhoneNumber = TryGetProperty(item, "collectorPhoneNumber", "CollectorPhoneNumber")?.ToString() ?? "",
                        CollectorAccountNumber = TryGetProperty(item, "collectorAccountNumber", "CollectorAccountNumber")?.ToString() ?? "",
                        MemberReference = TryGetProperty(item, "memberReference", "MemberReference")?.ToString() ?? "",
                       // BranchId = TryGetProperty(item, "branchId", "BranchId")?.ToString() ?? "",
                        BranchCode = TryGetProperty(item, "branchCode", "BranchCode")?.ToString() ?? "",
                        BranchName = TryGetProperty(item, "branchName", "BranchName")?.ToString() ?? "",
                        Year = Convert.ToInt32(TryGetProperty(item, "year", "Year") ?? DateTime.Now.Year),
                        Month = Convert.ToInt32(TryGetProperty(item, "month", "Month") ?? DateTime.Now.Month),
                        ReferenceNumber = TryGetProperty(item, "referenceNumber", "ReferenceNumber")?.ToString() ?? "",
                        CollectorShareAmount = Convert.ToDecimal(TryGetProperty(item, "collectorShareAmount", "CollectorShareAmount") ?? 0m),
                        IncentiveAmount = Convert.ToDecimal(TryGetProperty(item, "incentiveAmount", "IncentiveAmount") ?? 0m),
                        TotalPaidAmountToCollector = Convert.ToDecimal(TryGetProperty(item, "totalPaidAmountToCollector", "TotalPaidAmountToCollector") ?? 0m),
                        TotalCommissionShared = Convert.ToDecimal(TryGetProperty(item, "totalCommissionShared", "TotalCommissionShared") ?? 0m),
                        AmountPaid = Convert.ToDecimal(TryGetProperty(item, "amountPaid", "AmountPaid") ?? 0m),
                        Currency = TryGetProperty(item, "currency", "Currency")?.ToString() ?? "XAF",
                        DatePaid = ParseDateTimeOffset(TryGetProperty(item, "datePaid", "DatePaid")) ?? DateTimeOffset.Now,
                        Description = TryGetProperty(item, "description", "Description")?.ToString() ?? "Monthly collector commission",
                        PaymentSource = TryGetProperty(item, "paymentSource", "PaymentSource")?.ToString() ?? "BackOffice_Operation",
                        ProcessedBy = TryGetProperty(item, "processedBy", "ProcessedBy")?.ToString() ?? "",
                       // ProcessedByUserId = TryGetProperty(item, "processedByUserId", "ProcessedByUserId")?.ToString() ?? "",
                        CreatedDate = ParseDateTimeOffset(TryGetProperty(item, "createdDate", "CreatedDate")) ?? DateTimeOffset.Now,
                        //CreatedBy = TryGetProperty(item, "createdBy", "CreatedBy")?.ToString() ?? "",
                        //ModifiedDate = ParseDateTimeOffset(TryGetProperty(item, "modifiedDate", "ModifiedDate")) ?? DateTimeOffset.MinValue,
                        //ModifiedBy = TryGetProperty(item, "modifiedBy", "ModifiedBy")?.ToString() ?? "",
                        //DeletedDate = ParseDateTimeOffset(TryGetProperty(item, "deletedDate", "DeletedDate")),
                        //DeletedBy = TryGetProperty(item, "deletedBy", "DeletedBy")?.ToString(),
                        //IsDeleted = Convert.ToBoolean(TryGetProperty(item, "isDeleted", "IsDeleted") ?? false)
                    };

                    commissionList.Add(commission);
                    Console.WriteLine($"Successfully converted record: {commission.Id} - {commission.CollectorName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error converting commission data: {ex.Message}");
                    // Log the problematic item for debugging
                    try
                    {
                        string itemJson = Newtonsoft.Json.JsonConvert.SerializeObject(item);
                        Console.WriteLine($"Problematic item: {itemJson}");
                    }
                    catch
                    {
                        Console.WriteLine("Could not serialize problematic item");
                    }
                }
            }

            Console.WriteLine($"Successfully converted {commissionList.Count} out of {tableData.Count} records");
            return commissionList;
        }

        public void GenerateCommissionExcelFromTableData(List<DataTableResponse> commissionData, string filePath, string exportedBy, ExportOptions exportOptions)
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
                CreateSummarySheet(package, commissionData, bank, branchcode, Branchid, branchname, exportedBy, exportOptions);

                // ===== SHEETS FOR EACH BRACH: Create individual sheets for each branch =====
                var branches = commissionData.GroupBy(x => new { x.BranchCode, x.BranchName })
                                           .Select(g => new { BranchCode = g.Key.BranchCode, BranchName = g.Key.BranchName })
                                           .ToList();

                foreach (var branch in branches)
                {
                    var branchData = commissionData.Where(x => x.BranchCode == branch.BranchCode && x.BranchName == branch.BranchName).ToList();
                    CreateBranchSheet(package, branchData, branch.BranchCode, branch.BranchName, exportedBy, exportOptions);
                }

                // Save the file
                package.SaveAs(new FileInfo(filePath));

                Console.WriteLine($"Excel file generated successfully with {commissionData.Count} records across {branches.Count} branches");
                Console.WriteLine($"File saved to: {filePath}");
            }
        }
        private void CreateSummarySheet(ExcelPackage package, List<DataTableResponse> commissionData, string bank, string branchcode, string Branchid, string branchname, string exportedBy, ExportOptions exportOptions)
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
            worksheet.Cells[$"A{currentRow}"].Value = "GENERAL SUMMARY";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Size = 14;
            worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
            currentRow += 2;

            // Calculate summary statistics
            var totalCollectors = commissionData.Select(x => x.CollectorName).Distinct().Count();
            var totalBranches = commissionData.Select(x => new { x.BranchCode, x.BranchName }).Distinct().Count();
            var totalMonths = commissionData.Select(x => x.Month).Distinct().Count();
            var totalYears = commissionData.Select(x => x.Year).Distinct().Count();
            var totalAmountPaid = commissionData.Sum(x => x.AmountPaid);
            var totalCollectorShare = commissionData.Sum(x => x.CollectorShareAmount);
            var totalIncentive = commissionData.Sum(x => x.IncentiveAmount);
            var totalPaidToCollector = commissionData.Sum(x => x.TotalPaidAmountToCollector);
            var totalCommissionShared = commissionData.Sum(x => x.TotalCommissionShared);
            var totalRecords = commissionData.Count;

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
        new { Metric = "Total Records", Value = totalRecords.ToString("N0"), Description = "Number of commission records" },
        new { Metric = "Total Collectors", Value = totalCollectors.ToString("N0"), Description = "Unique collectors involved" },
        new { Metric = "Total Branches", Value = totalBranches.ToString("N0"), Description = "Branches with commission data" },
        new { Metric = "Total Months", Value = totalMonths.ToString("N0"), Description = "Months covered in data" },
        new { Metric = "Total Years", Value = totalYears.ToString("N0"), Description = "Years covered in data" },
        new { Metric = "Total Amount Paid", Value = totalAmountPaid.ToString("N2"), Description = "Sum of all amounts paid" },
        new { Metric = "Total Collector Share", Value = totalCollectorShare.ToString("N2"), Description = "Sum of collector shares" },
        new { Metric = "Total Incentive Amount", Value = totalIncentive.ToString("N2"), Description = "Sum of incentive amounts" },
        new { Metric = "Total Paid to Collectors", Value = totalPaidToCollector.ToString("N2"), Description = "Sum paid to all collectors" },
        new { Metric = "Total Commission Shared", Value = totalCommissionShared.ToString("N2"), Description = "Sum of all commissions shared" }
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

            // ===== DISTRIBUTION BREAKDOWN SECTION =====
            worksheet.Cells[$"A{currentRow}:G{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = "DISTRIBUTION BREAKDOWN";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Size = 14;
            worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
            currentRow += 2;

            // Explanation text for distribution breakdown
            worksheet.Cells[$"A{currentRow}:E{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = "Commission Distribution Explanation:";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Italic = true;
            currentRow++;

            worksheet.Cells[$"A{currentRow}:E{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = "• Total Commission Shared is distributed 60% to Daily Collectors and 40% to Branches";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Italic = true;
            currentRow++;

            worksheet.Cells[$"A{currentRow}:E{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = "• Daily Collectors receive their share PLUS any incentive amounts";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Italic = true;
            currentRow++;

            worksheet.Cells[$"A{currentRow}:E{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = "• Branches receive only their 40% share (no incentives)";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Italic = true;
            currentRow++;

            worksheet.Cells[$"A{currentRow}:E{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = "• Total Amount Paid = Incentive Amount + Daily Collector Share for collectors, and Branch Share for branches";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Italic = true;
            currentRow++;

            // Distribution breakdown headers
            var distributionHeaders = new[] { "Stakeholder", "Percentage", "Incentive Amount", "Daily Collector Share", "Total Amount Paid" };
            for (int i = 0; i < distributionHeaders.Length; i++)
            {
                worksheet.Cells[currentRow, 1 + i].Value = distributionHeaders[i];
                worksheet.Cells[currentRow, 1 + i].Style.Font.Bold = true;
                worksheet.Cells[currentRow, 1 + i].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[currentRow, 1 + i].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                worksheet.Cells[currentRow, 1 + i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                worksheet.Cells[currentRow, 1 + i].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }
            currentRow++;

            // Calculate distribution breakdown
            var totalCommission = totalCommissionShared;
            var dailyCollectorPercentage = 0.60m; // 60%
            var branchPercentage = 0.40m; // 40%

            var dailyCollectorShare = totalCommission * dailyCollectorPercentage;
            var branchShare = totalCommission * branchPercentage;

            // Daily Collector row
            worksheet.Cells[currentRow, 1].Value = "Daily Collector";
            worksheet.Cells[currentRow, 2].Value = dailyCollectorPercentage;
            worksheet.Cells[currentRow, 3].Value = totalIncentive;
            worksheet.Cells[currentRow, 4].Value = dailyCollectorShare;
            worksheet.Cells[currentRow, 5].Value = totalIncentive + dailyCollectorShare;

            for (int col = 1; col <= 5; col++)
            {
                worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }
            currentRow++;

            // Branch row
            worksheet.Cells[currentRow, 1].Value = "Branch";
            worksheet.Cells[currentRow, 2].Value = branchPercentage;
            worksheet.Cells[currentRow, 3].Value = 0; // null/incentive amount for branch
            worksheet.Cells[currentRow, 4].Value = branchShare;
            worksheet.Cells[currentRow, 5].Value = branchShare;

            for (int col = 1; col <= 5; col++)
            {
                worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }
            currentRow++;

            // TOTAL DISTRIBUTION row
            worksheet.Cells[currentRow, 1].Value = "TOTAL DISTRIBUTION:";
            worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
            worksheet.Cells[currentRow, 2].Value = 1.00m; // 100%
            worksheet.Cells[currentRow, 3].Value = totalIncentive;
            worksheet.Cells[currentRow, 4].Value = totalCommission; // Total commission shared
            worksheet.Cells[currentRow, 5].Value = totalIncentive + totalCommission;

            for (int col = 1; col <= 5; col++)
            {
                worksheet.Cells[currentRow, col].Style.Font.Bold = true;
                worksheet.Cells[currentRow, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[currentRow, col].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
                worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            // Format numbers and percentages
            int distributionStartRow = currentRow - 2;
            worksheet.Cells[$"B{distributionStartRow}:B{currentRow}"].Style.Numberformat.Format = "0.0%";
            worksheet.Cells[$"C{distributionStartRow}:E{currentRow}"].Style.Numberformat.Format = "#,##0.00";

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
            var branchHeaders = new[] { "Branch Code", "Branch Name", "Records", "Collectors", "Total Amount", "Avg per Collector" };
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
            var branchSummary = commissionData.GroupBy(x => new { x.BranchCode, x.BranchName })
                                            .Select(g => new
                                            {
                                                BranchCode = g.Key.BranchCode,
                                                BranchName = g.Key.BranchName,
                                                Records = g.Count(),
                                                Collectors = g.Select(x => x.CollectorName).Distinct().Count(),
                                                TotalAmount = g.Sum(x => x.AmountPaid),
                                                AvgPerCollector = g.Sum(x => x.AmountPaid) / g.Select(x => x.CollectorName).Distinct().Count()
                                            })
                                            .OrderByDescending(x => x.TotalAmount)
                                            .ToList();

            foreach (var branch in branchSummary)
            {
                worksheet.Cells[currentRow, 1].Value = branch.BranchCode;
                worksheet.Cells[currentRow, 2].Value = branch.BranchName;
                worksheet.Cells[currentRow, 3].Value = branch.Records;
                worksheet.Cells[currentRow, 4].Value = branch.Collectors;
                worksheet.Cells[currentRow, 5].Value = branch.TotalAmount;
                worksheet.Cells[currentRow, 6].Value = branch.AvgPerCollector;

                for (int col = 1; col <= 6; col++)
                {
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
                currentRow++;
            }

            // Format numbers
            worksheet.Cells[$"E{currentRow - branchSummary.Count}:F{currentRow - 1}"].Style.Numberformat.Format = "#,##0.00";

            // Auto-fit columns
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        }

        private void CreateBranchSheet(ExcelPackage package, List<DataTableResponse> branchData, string branchCode, string branchName, string exportedBy, ExportOptions exportOptions)
        {
            // Clean sheet name (Excel has restrictions on sheet names)
            var cleanSheetName = CleanSheetName($"{branchCode} - {branchName}");
            if (cleanSheetName.Length > 31) cleanSheetName = cleanSheetName.Substring(0, 31);

            var worksheet = package.Workbook.Worksheets.Add(cleanSheetName);

            // Set font
            var fontName = "Bahnschrift SemiCondensed";
            worksheet.Cells.Style.Font.Name = fontName;

            // ===== HEADER SECTION =====
            CreateHeaderSection(worksheet, GetBankName(), branchCode, GetBranchID(), branchName, exportedBy, exportOptions);

            int currentRow = 8; // Start after header

            // ===== BRANCH SUMMARY SECTION =====
            worksheet.Cells[$"A{currentRow}:G{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = $"BRANCH SUMMARY - {branchName} ({branchCode})";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Size = 14;
            worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
            currentRow += 2;

            // Calculate branch statistics
            var totalCollectors = branchData.Select(x => x.CollectorName).Distinct().Count();
            var totalMonths = branchData.Select(x => x.Month).Distinct().Count();
            var totalAmountPaid = branchData.Sum(x => x.AmountPaid);
            var totalCollectorShare = branchData.Sum(x => x.CollectorShareAmount);
            var totalIncentive = branchData.Sum(x => x.IncentiveAmount);
            var totalPaidToCollector = branchData.Sum(x => x.TotalPaidAmountToCollector);
            var totalCommissionShared = branchData.Sum(x => x.TotalCommissionShared);

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
        new { Metric = "Total Records", Value = branchData.Count.ToString("N0") },
        new { Metric = "Total Collectors", Value = totalCollectors.ToString("N0") },
        new { Metric = "Total Months", Value = totalMonths.ToString("N0") },
        new { Metric = "Total Amount Paid", Value = totalAmountPaid.ToString("N2") },
        new { Metric = "Total Collector Share", Value = totalCollectorShare.ToString("N2") },
        new { Metric = "Total Incentive Amount", Value = totalIncentive.ToString("N2") },
        new { Metric = "Total Paid to Collectors", Value = totalPaidToCollector.ToString("N2") },
        new { Metric = "Total Commission Shared", Value = totalCommissionShared.ToString("N2") }
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

            // ===== BRANCH DISTRIBUTION BREAKDOWN SECTION =====
            worksheet.Cells[$"A{currentRow}:G{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = $"BRANCH DISTRIBUTION BREAKDOWN - {branchName}";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Size = 14;
            worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
            currentRow += 2;

            // Explanation text for branch distribution breakdown
            worksheet.Cells[$"A{currentRow}:E{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = $"Distribution for {branchName} ({branchCode}):";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Italic = true;
            currentRow++;

            worksheet.Cells[$"A{currentRow}:E{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = "• This branch receives 40% of the Total Commission Shared generated by its collectors";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Italic = true;
            currentRow++;

            worksheet.Cells[$"A{currentRow}:E{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = "• Daily Collectors in this branch receive 60% of commission + any incentives";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Italic = true;
            currentRow++;

            worksheet.Cells[$"A{currentRow}:E{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = "• Branch share is calculated as 40% of Total Commission Shared from this branch's activities";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Italic = true;
            currentRow++;

            // Branch distribution breakdown headers
            var distributionHeaders = new[] { "Stakeholder", "Percentage", "Incentive Amount", "Daily Collector Share", "Total Amount Paid" };
            for (int i = 0; i < distributionHeaders.Length; i++)
            {
                worksheet.Cells[currentRow, 1 + i].Value = distributionHeaders[i];
                worksheet.Cells[currentRow, 1 + i].Style.Font.Bold = true;
                worksheet.Cells[currentRow, 1 + i].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[currentRow, 1 + i].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                worksheet.Cells[currentRow, 1 + i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                worksheet.Cells[currentRow, 1 + i].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }
            currentRow++;

            // Calculate branch-specific distribution breakdown
            var branchCommission = totalCommissionShared;
            var dailyCollectorPercentage = 0.60m; // 60%
            var branchPercentage = 0.40m; // 40%

            var branchDailyCollectorShare = branchCommission * dailyCollectorPercentage;
            var branchBranchShare = branchCommission * branchPercentage;

            // Daily Collector row for this branch
            worksheet.Cells[currentRow, 1].Value = "Daily Collector";
            worksheet.Cells[currentRow, 2].Value = dailyCollectorPercentage;
            worksheet.Cells[currentRow, 3].Value = totalIncentive;
            worksheet.Cells[currentRow, 4].Value = branchDailyCollectorShare;
            worksheet.Cells[currentRow, 5].Value = totalIncentive + branchDailyCollectorShare;

            for (int col = 1; col <= 5; col++)
            {
                worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }
            currentRow++;

            // Branch row for this branch
            worksheet.Cells[currentRow, 1].Value = "Branch";
            worksheet.Cells[currentRow, 2].Value = branchPercentage;
            worksheet.Cells[currentRow, 3].Value = 0; // null/incentive amount for branch
            worksheet.Cells[currentRow, 4].Value = branchBranchShare;
            worksheet.Cells[currentRow, 5].Value = branchBranchShare;

            for (int col = 1; col <= 5; col++)
            {
                worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }
            currentRow++;

            // TOTAL DISTRIBUTION row for this branch
            worksheet.Cells[currentRow, 1].Value = "TOTAL DISTRIBUTION:";
            worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
            worksheet.Cells[currentRow, 2].Value = 1.00m; // 100%
            worksheet.Cells[currentRow, 3].Value = totalIncentive;
            worksheet.Cells[currentRow, 4].Value = branchCommission; // Total commission shared for this branch
            worksheet.Cells[currentRow, 5].Value = totalIncentive + branchCommission;

            for (int col = 1; col <= 5; col++)
            {
                worksheet.Cells[currentRow, col].Style.Font.Bold = true;
                worksheet.Cells[currentRow, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[currentRow, col].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
                worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            // Format numbers and percentages for branch distribution
            int branchDistributionStartRow = currentRow - 2;
            worksheet.Cells[$"B{branchDistributionStartRow}:B{currentRow}"].Style.Numberformat.Format = "0.0%";
            worksheet.Cells[$"C{branchDistributionStartRow}:E{currentRow}"].Style.Numberformat.Format = "#,##0.00";

            currentRow += 2;

            // ===== DETAILED DATA TABLE =====
            worksheet.Cells[$"A{currentRow}:G{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = "DETAILED COMMISSION DATA";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Size = 14;
            worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.BackgroundColor.SetColor(Color.LightCoral);
            currentRow += 2;

            // Detailed data headers
            var headers = new[]
            {
        "SN", "Collector Name", "Collector Phone", "Collector Account", "Member Reference",
        "Year", "Month", "Reference Number", "Collector Share", "Incentive Amount",
        "Total Paid to Collector", "Total Commission Shared", "Amount Paid", "Currency",
        "Date Paid", "Description", "Payment Source", "Processed By", "Created Date"
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

            foreach (var commission in branchData.OrderBy(x => x.CollectorName).ThenBy(x => x.Year).ThenBy(x => x.Month))
            {
                worksheet.Cells[dataRow, 1].Value = serialNumber++;
                worksheet.Cells[dataRow, 2].Value = commission.CollectorName;
                worksheet.Cells[dataRow, 3].Value = commission.CollectorPhoneNumber;
                worksheet.Cells[dataRow, 4].Value = commission.CollectorAccountNumber;
                worksheet.Cells[dataRow, 5].Value = commission.MemberReference;
                worksheet.Cells[dataRow, 6].Value = commission.Year;
                worksheet.Cells[dataRow, 7].Value = commission.Month;
                worksheet.Cells[dataRow, 8].Value = commission.ReferenceNumber;
                worksheet.Cells[dataRow, 9].Value = commission.CollectorShareAmount;
                worksheet.Cells[dataRow, 10].Value = commission.IncentiveAmount;
                worksheet.Cells[dataRow, 11].Value = commission.TotalPaidAmountToCollector;
                worksheet.Cells[dataRow, 12].Value = commission.TotalCommissionShared;
                worksheet.Cells[dataRow, 13].Value = commission.AmountPaid;
                worksheet.Cells[dataRow, 14].Value = commission.Currency;
                worksheet.Cells[dataRow, 15].Value = commission.DatePaid.ToString("yyyy-MM-dd");
                worksheet.Cells[dataRow, 16].Value = commission.Description;
                worksheet.Cells[dataRow, 17].Value = commission.PaymentSource;
                worksheet.Cells[dataRow, 18].Value = commission.ProcessedBy;
                worksheet.Cells[dataRow, 19].Value = commission.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss");

                // Apply borders
                for (int col = 1; col <= headers.Length; col++)
                {
                    worksheet.Cells[dataRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                totalAmount += commission.AmountPaid;
                dataRow++;
            }

            // Totals row
            if (branchData.Any())
            {
                worksheet.Cells[dataRow, 1].Value = "TOTALS:";
                worksheet.Cells[dataRow, 1].Style.Font.Bold = true;
                worksheet.Cells[dataRow, 9].Value = branchData.Sum(x => x.CollectorShareAmount);
                worksheet.Cells[dataRow, 10].Value = branchData.Sum(x => x.IncentiveAmount);
                worksheet.Cells[dataRow, 11].Value = branchData.Sum(x => x.TotalPaidAmountToCollector);
                worksheet.Cells[dataRow, 12].Value = branchData.Sum(x => x.TotalCommissionShared);
                worksheet.Cells[dataRow, 13].Value = totalAmount;

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
                worksheet.Cells[$"I{headerRow + 1}:M{dataRow}"].Style.Numberformat.Format = "#,##0.00";
            }

            // Auto-fit columns
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            // Freeze panes for easy scrolling
            worksheet.View.FreezePanes(headerRow + 1, 1);
        }

        private void CreateHeaderSection(ExcelWorksheet worksheet, string bank, string branchcode, string Branchid, string branchname, string exportedBy, ExportOptions exportOptions)
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
            worksheet.Cells["A5"].Value = "DAILY COLLECTOR COMMISSION REPORT";
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


        // ===================================================================
        // HELPER METHODS FOR DATA TABLE EXPORT
        // ===================================================================

        // Helper method to safely try multiple property names
        private static object TryGetProperty(dynamic obj, params string[] propertyNames)
        {
            if (obj == null) return null;

            foreach (var propName in propertyNames)
            {
                try
                {
                    // Check if it's an ExpandoObject or similar
                    if (obj is IDictionary<string, object> dict)
                    {
                        if (dict.ContainsKey(propName) && dict[propName] != null)
                            return dict[propName];
                    }
                    else
                    {
                        // Try to get property via reflection
                        var property = obj.GetType().GetProperty(propName);
                        if (property != null)
                        {
                            var value = property.GetValue(obj, null);
                            if (value != null) return value;
                        }
                    }
                }
                catch
                {
                    // Continue to next property name
                }
            }
            return null;
        }

        // Improved date parsing
        private static DateTimeOffset? ParseDateTimeOffset(object dateValue)
        {
            if (dateValue == null) return null;

            try
            {
                if (dateValue is DateTimeOffset dto) return dto;
                if (dateValue is DateTime dt) return new DateTimeOffset(dt);

                string dateString = dateValue.ToString();
                if (DateTimeOffset.TryParse(dateString, out DateTimeOffset result))
                    return result;

                return null;
            }
            catch
            {
                return null;
            }
        }

        // Helper methods for safe property access
        private static string GetSafeString(dynamic obj, params string[] propertyNames)
        {
            foreach (var propName in propertyNames)
            {
                try
                {
                    var value = GetPropertyValue(obj, propName);
                    if (value != null && !string.IsNullOrEmpty(value.ToString()))
                        return value.ToString();
                }
                catch
                {
                    // Continue to next property name
                }
            }
            return null;
        }

        private static int? GetSafeInt(dynamic obj, params string[] propertyNames)
        {
            foreach (var propName in propertyNames)
            {
                try
                {
                    var value = GetPropertyValue(obj, propName);
                    if (value != null)
                    {
                        if (int.TryParse(value.ToString(), out int result))
                            return result;
                    }
                }
                catch
                {
                    // Continue to next property name
                }
            }
            return null;
        }

        private static decimal? GetSafeDecimal(dynamic obj, params string[] propertyNames)
        {
            foreach (var propName in propertyNames)
            {
                try
                {
                    var value = GetPropertyValue(obj, propName);
                    if (value != null)
                    {
                        if (decimal.TryParse(value.ToString(), out decimal result))
                            return result;
                    }
                }
                catch
                {
                    // Continue to next property name
                }
            }
            return null;
        }

        private static bool? GetSafeBool(dynamic obj, params string[] propertyNames)
        {
            foreach (var propName in propertyNames)
            {
                try
                {
                    var value = GetPropertyValue(obj, propName);
                    if (value != null)
                    {
                        if (bool.TryParse(value.ToString(), out bool result))
                            return result;
                        // Handle "true"/"false" strings and 1/0 integers
                        if (value.ToString().ToLower() == "true" || value.ToString() == "1")
                            return true;
                        if (value.ToString().ToLower() == "false" || value.ToString() == "0")
                            return false;
                    }
                }
                catch
                {
                    // Continue to next property name
                }
            }
            return null;
        }

        private static dynamic GetPropertyValue(dynamic obj, params string[] propertyNames)
        {
            foreach (var propName in propertyNames)
            {
                try
                {
                    // Check if property exists and has value
                    if (obj != null)
                    {
                        var property = obj.GetType().GetProperty(propName);
                        if (property != null)
                        {
                            var value = property.GetValue(obj, null);
                            if (value != null)
                                return value;
                        }

                        // Try as dictionary
                        if (obj is IDictionary<string, object> dict && dict.ContainsKey(propName))
                            return dict[propName];
                    }
                }
                catch
                {
                    // Continue to next property name
                }
            }
            return null;
        }

        // ===================================================================
        // ORIGINAL COMMISSION EXPORT METHODS (Leave as-is)
        // ===================================================================

        public static void GenerateCommissionExcel(CollectorComissionResponse commissionData,
            string filePath, string exportedBy, ExportOptions exportOptions = null)
        {
            // Set EPPlus license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // DEBUG: Check what data is received
            System.Diagnostics.Debug.WriteLine($"Commission Data Received:");
            System.Diagnostics.Debug.WriteLine($"- Collector: {commissionData?.CollectorName}");
            System.Diagnostics.Debug.WriteLine($"- SharedAmounts Count: {commissionData?.SharedAmounts?.Count}");

            if (commissionData?.SharedAmounts != null)
            {
                foreach (var item in commissionData.SharedAmounts)
                {
                    System.Diagnostics.Debug.WriteLine($"  - {item.Stakeholder}: {item.Percentage}% = {item.Amount}");
                }
            }

            using (var package = new ExcelPackage())
            {
                // Apply Bahnschrift SemiCondensed font to all worksheets
                var fontName = "Bahnschrift SemiCondensed";

                // Create Summary Worksheet with Distribution Breakdown
                CreateSummaryWithDistributionWorksheet(package, commissionData, exportedBy, exportOptions, fontName);

                // Create Details Worksheet with date filtering and SN column
                CreateDetailsWorksheet(package, commissionData, exportOptions, fontName);

                // Save the Excel file
                package.SaveAs(new FileInfo(filePath));
            }
        }

        private static void CreateSummaryWithDistributionWorksheet(ExcelPackage package,
            CollectorComissionResponse data, string exportedBy, ExportOptions exportOptions, string fontName)
        {
            var worksheet = package.Workbook.Worksheets.Add("Summary & Distribution");

            // Apply font to all cells
            worksheet.Cells.Style.Font.Name = fontName;

            // Title Section
            worksheet.Cells["A1"].Value = "COMMISSION SUMMARY REPORT";
            worksheet.Cells["A1:F1"].Merge = true;
            worksheet.Cells["A1"].Style.Font.Bold = true;
            worksheet.Cells["A1"].Style.Font.Size = 18;
            worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["A1"].Style.Font.Color.SetColor(Color.White);
            worksheet.Cells["A1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["A1"].Style.Fill.BackgroundColor.SetColor(Color.DarkGreen);

            // Export Information with Date Range
            worksheet.Cells["A2"].Value = "Exported By:";
            worksheet.Cells["B2"].Value = exportedBy;
            worksheet.Cells["A3"].Value = "Export Date:";
            worksheet.Cells["B3"].Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            if (exportOptions != null && !string.IsNullOrEmpty(exportOptions.StartDate) && !string.IsNullOrEmpty(exportOptions.EndDate))
            {
                worksheet.Cells["A4"].Value = "Date Range:";
                worksheet.Cells["B4"].Value = $"{exportOptions.StartDate} to {exportOptions.EndDate}";
                worksheet.Cells["A2:A4"].Style.Font.Bold = true;
            }
            else
            {
                worksheet.Cells["A2:A3"].Style.Font.Bold = true;
            }

            // Collector Information
            int row = 6;
            worksheet.Cells[$"A{row}"].Value = "COLLECTOR INFORMATION";
            worksheet.Cells[$"A{row}:F{row}"].Merge = true;
            worksheet.Cells[$"A{row}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{row}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A{row}"].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
            row++;

            worksheet.Cells[$"A{row}"].Value = "Collector Name:";
            worksheet.Cells[$"B{row}"].Value = data.CollectorName ?? "N/A";
            worksheet.Cells[$"D{row}"].Value = "Branch:";
            worksheet.Cells[$"E{row}"].Value = data.BranchName ?? "N/A";
            row++;

            worksheet.Cells[$"A{row}"].Value = "Period:";
            worksheet.Cells[$"B{row}"].Value = data.Month ?? "N/A";
            worksheet.Cells[$"D{row}"].Value = "Total Members:";
            worksheet.Cells[$"E{row}"].Value = data.TotalMembersWithActivity;
            row++;

            // Activity Overview
            row++;
            worksheet.Cells[$"A{row}"].Value = "ACTIVITY OVERVIEW";
            worksheet.Cells[$"A{row}:F{row}"].Merge = true;
            worksheet.Cells[$"A{row}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{row}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A{row}"].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
            row++;

            // Create overview table
            var overviewHeaders = new[] { "Description", "Amount" };
            for (int i = 0; i < overviewHeaders.Length; i++)
            {
                worksheet.Cells[row, 1 + i].Value = overviewHeaders[i];
                worksheet.Cells[row, 1 + i].Style.Font.Bold = true;
                worksheet.Cells[row, 1 + i].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[row, 1 + i].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                worksheet.Cells[row, 1 + i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }
            row++;

            var overviewData = new[]
            {
                new { Description = "Total Savings Collected", Amount = data.TotalValueCollected },
                new { Description = "Total Fee Charged", Amount = data.TotalFeeCharged },
                new { Description = "Total Amount to Distribute", Amount = data.TotalAmountToDistribute }
            };

            foreach (var item in overviewData)
            {
                worksheet.Cells[row, 1].Value = item.Description;
                worksheet.Cells[row, 2].Value = item.Amount;
                worksheet.Cells[row, 2].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[row, 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                worksheet.Cells[row, 2].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                row++;
            }

            // DISTRIBUTION BREAKDOWN SECTION
            row += 2;
            worksheet.Cells[$"A{row}"].Value = "DISTRIBUTION BREAKDOWN";
            worksheet.Cells[$"A{row}:F{row}"].Merge = true;
            worksheet.Cells[$"A{row}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{row}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A{row}"].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
            row++;

            // Distribution headers
            var distributionHeaders = new[] { "Stakeholder", "Percentage", "Amount" };
            for (int i = 0; i < distributionHeaders.Length; i++)
            {
                worksheet.Cells[row, 1 + i].Value = distributionHeaders[i];
                worksheet.Cells[row, 1 + i].Style.Font.Bold = true;
                worksheet.Cells[row, 1 + i].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[row, 1 + i].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                worksheet.Cells[row, 1 + i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }
            row++;

            // Distribution data
            decimal totalDistributionAmount = 0;
            bool hasDistributionData = false;

            if (data?.SharedAmounts != null && data.SharedAmounts.Count > 0)
            {
                foreach (var distribution in data.SharedAmounts)
                {
                    if (distribution != null)
                    {
                        worksheet.Cells[row, 1].Value = distribution.Stakeholder ?? "N/A";
                        worksheet.Cells[row, 2].Value = (distribution.Percentage / 100m);
                        worksheet.Cells[row, 3].Value = distribution.Amount;

                        for (int col = 1; col <= 3; col++)
                        {
                            worksheet.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        }

                        totalDistributionAmount += distribution.Amount;
                        row++;
                        hasDistributionData = true;
                    }
                }

                if (hasDistributionData)
                {
                    // Distribution totals row
                    worksheet.Cells[row, 1].Value = "TOTAL DISTRIBUTION:";
                    worksheet.Cells[row, 1].Style.Font.Bold = true;
                    worksheet.Cells[row, 3].Value = totalDistributionAmount;

                    for (int col = 1; col <= 3; col++)
                    {
                        worksheet.Cells[row, col].Style.Font.Bold = true;
                        worksheet.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[row, col].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
                        worksheet.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }

                    // Format numbers for distribution section
                    int distributionStartRow = row - data.SharedAmounts.Count(d => d != null);
                    worksheet.Cells[$"B{distributionStartRow}:B{row}"].Style.Numberformat.Format = "0.0%";
                    worksheet.Cells[$"C{distributionStartRow}:C{row}"].Style.Numberformat.Format = "#,##0.00";
                }
            }
            else
            {
                worksheet.Cells[row, 1].Value = "No distribution data available";
                worksheet.Cells[row, 1].Style.Font.Italic = true;
                worksheet.Cells[$"A{row}:C{row}"].Merge = true;
                worksheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            // Auto-fit columns
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        }

        private static void CreateDetailsWorksheet(ExcelPackage package, CollectorComissionResponse data,
            ExportOptions exportOptions, string fontName)
        {
            var worksheet = package.Workbook.Worksheets.Add("Savers Details");

            // Apply font to all cells
            worksheet.Cells.Style.Font.Name = fontName;

            // Title with date range info
            string title = "SAVERS COMMISSION DETAILS";
            if (exportOptions != null && !string.IsNullOrEmpty(exportOptions.StartDate) && !string.IsNullOrEmpty(exportOptions.EndDate))
            {
                title += $" ({exportOptions.StartDate} to {exportOptions.EndDate})";
            }

            worksheet.Cells["A1"].Value = title;
            worksheet.Cells["A1:E1"].Merge = true;
            worksheet.Cells["A1"].Style.Font.Bold = true;
            worksheet.Cells["A1"].Style.Font.Size = 16;
            worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["A1"].Style.Font.Color.SetColor(Color.White);
            worksheet.Cells["A1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["A1"].Style.Fill.BackgroundColor.SetColor(Color.DarkGreen);

            // Headers with SN column and Transaction Date
            var headers = new[] { "SN", "Transaction Date", "Member ID", "Member Name", "Total Activity Amount", "Fee Charged" };
            int headerRow = 4;
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[headerRow, i + 1].Value = headers[i];
                worksheet.Cells[headerRow, i + 1].Style.Font.Bold = true;
                worksheet.Cells[headerRow, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[headerRow, i + 1].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                worksheet.Cells[headerRow, i + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            // Data
            int dataRow = headerRow + 1;
            decimal totalActivity = 0;
            decimal totalFee = 0;
            int serialNumber = 1;

            // Filter member stats by date range if provided
            var filteredMemberStats = data?.MemberStats;
            if (exportOptions != null && !string.IsNullOrEmpty(exportOptions.StartDate) && !string.IsNullOrEmpty(exportOptions.EndDate))
            {
                var startDate = DateTime.Parse(exportOptions.StartDate);
                var endDate = DateTime.Parse(exportOptions.EndDate).AddDays(1).AddSeconds(-1); // Include entire end date

                filteredMemberStats = data?.MemberStats?
                    .Where(m => m.LastTransactionDate >= startDate && m.LastTransactionDate <= endDate)
                    .ToList();
            }

            if (filteredMemberStats != null && filteredMemberStats.Count > 0)
            {
                foreach (var member in filteredMemberStats)
                {
                    worksheet.Cells[dataRow, 1].Value = serialNumber++; // SN column
                    worksheet.Cells[dataRow, 2].Value = member.LastTransactionDate.ToString();
                    worksheet.Cells[dataRow, 3].Value = member.MemberId ?? "-";
                    worksheet.Cells[dataRow, 4].Value = member.MemberName ?? "-";
                    worksheet.Cells[dataRow, 5].Value = member.TotalActivityAmount;
                    worksheet.Cells[dataRow, 6].Value = member.FeeCharged;

                    // Apply borders to all cells in the row
                    for (int col = 1; col <= headers.Length; col++)
                    {
                        worksheet.Cells[dataRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }

                    totalActivity += member.TotalActivityAmount;
                    totalFee += member.FeeCharged;
                    dataRow++;
                }

                // Totals Row
                worksheet.Cells[dataRow, 1].Value = "TOTALS:";
                worksheet.Cells[dataRow, 1].Style.Font.Bold = true;
                worksheet.Cells[dataRow, 5].Value = totalActivity;
                worksheet.Cells[dataRow, 6].Value = totalFee;

                // Style totals row
                for (int col = 1; col <= headers.Length; col++)
                {
                    worksheet.Cells[dataRow, col].Style.Font.Bold = true;
                    worksheet.Cells[dataRow, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[dataRow, col].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
                    worksheet.Cells[dataRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                // Add summary note for date range
                if (exportOptions != null && !string.IsNullOrEmpty(exportOptions.StartDate) && !string.IsNullOrEmpty(exportOptions.EndDate))
                {
                    dataRow += 2;
                    worksheet.Cells[dataRow, 1].Value = $"Note: Totals shown are for the selected date range only ({exportOptions.StartDate} to {exportOptions.EndDate})";
                    worksheet.Cells[dataRow, 1].Style.Font.Italic = true;
                    worksheet.Cells[$"A{dataRow}:E{dataRow}"].Merge = true;
                }
            }
            else
            {
                worksheet.Cells[dataRow, 1].Value = "No commission data available for the selected criteria";
                worksheet.Cells[dataRow, 1].Style.Font.Italic = true;
                worksheet.Cells[$"A{dataRow}:E{dataRow}"].Merge = true;
                worksheet.Cells[dataRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            // Format numbers
            if (filteredMemberStats != null && filteredMemberStats.Count > 0)
            {
                worksheet.Cells[$"E{headerRow + 1}:F{dataRow}"].Style.Numberformat.Format = "#,##0.00";
            }

            // Auto-fit columns
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        }
    }

   
       
}