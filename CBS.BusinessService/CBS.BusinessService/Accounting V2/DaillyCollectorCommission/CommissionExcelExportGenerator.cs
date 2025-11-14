using CBS.FrontDesk.Data.Entity.Accounting_V2.DaillyCollectorCommission;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace CBS.BusinessService.Accounting_V2.Affiliate
{
    public static class CommissionExcelExportGenerator
    {
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

            // Export info
            //worksheet.Cells["A2"].Value = $"Exported By: {exportOptions?.FileName ?? "System"}";
            //worksheet.Cells["A2:E2"].Merge = true;
            //worksheet.Cells["A2"].Style.Font.Italic = true;

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