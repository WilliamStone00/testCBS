using CBS.FrontDesk.Data.Entity.Accounting_V2.IPS;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace CBS.BusinessService.Accounting_V2.IPS
{
    public class IPSClaimExcelExportGenerator
    {
        public static List<IPSClaimExportRecord> ConvertToExportData(List<IPSClaimExportRecord> tableData)
        {
            var exportList = new List<IPSClaimExportRecord>();

            if (tableData == null || !tableData.Any())
            {
                Console.WriteLine("No claims data provided for conversion");
                return exportList;
            }

            Console.WriteLine($"Processing {tableData.Count} claim records for export");

            foreach (var record in tableData)
            {
                // Format claim type display
                if (!string.IsNullOrEmpty(record.ClaimType))
                {
                    record.ClaimType = record.ClaimType == "LoanProtection" ? "Loan Protection" :
                                      record.ClaimType == "LifeSavings" ? "Life Savings" : record.ClaimType;
                }

                exportList.Add(record);
            }

            Console.WriteLine($"Successfully processed {exportList.Count} claim records");
            return exportList;
        }

        public void GenerateExcelWithSummary(List<IPSClaimExportRecord> claimsData, string filePath, string exportedBy, IPSClaimExportOptions exportOptions)
        {
            // Set EPPlus license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                // ===== SHEET 1: SUMMARY & OVERVIEW =====
                CreateSummarySheet(package, claimsData, exportedBy, exportOptions);

                // ===== SHEET 2: DETAILED DATA =====
                CreateDetailedSheet(package, claimsData, exportedBy, exportOptions);

                // ===== SHEET 3: BRANCH ANALYSIS =====
                CreateBranchAnalysisSheet(package, claimsData, exportedBy, exportOptions);

                // ===== SHEET 4: STATUS ANALYSIS =====
                CreateStatusAnalysisSheet(package, claimsData, exportedBy, exportOptions);

                // Save the file
                package.SaveAs(new FileInfo(filePath));

                Console.WriteLine($"Excel file generated successfully with {claimsData.Count} records");
                Console.WriteLine($"File saved to: {filePath}");
            }
        }

        public void GenerateSimpleExcel(List<IPSClaimExportRecord> claimsData, string filePath)
        {
            // Set EPPlus license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("IPS Claims");

                // Headers
                var headers = new[]
                {
                    "Claim ID", "Member ID", "Member Name", "Claim Type", "Branch",
                    "Claimed Amount", "Approved Amount", "Beneficiary", "Contact",
                    "Status", "Created Date", "Event Date", "Created By",
                    "Approved By", "Approved Date", "Posted By", "Posted Date", "Notes"
                };

                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = headers[i];
                    worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                    worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                    worksheet.Cells[1, i + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                // Data rows
                int row = 2;
                foreach (var claim in claimsData.OrderByDescending(x => x.CreatedDate))
                {
                    worksheet.Cells[row, 1].Value = claim.ClaimNumber ?? claim.Id;
                    worksheet.Cells[row, 2].Value = claim.MemberId;
                    worksheet.Cells[row, 3].Value = claim.MemberName;
                    worksheet.Cells[row, 4].Value = claim.ClaimType;
                    worksheet.Cells[row, 5].Value = claim.BranchName;
                    worksheet.Cells[row, 6].Value = claim.ClaimedAmount;
                    worksheet.Cells[row, 7].Value = claim.ApprovedAmount;
                    worksheet.Cells[row, 8].Value = claim.BeneficiaryName;
                    worksheet.Cells[row, 9].Value = claim.BeneficiaryContact;
                    worksheet.Cells[row, 10].Value = claim.Status;
                    worksheet.Cells[row, 11].Value = claim.CreatedDate;
                    worksheet.Cells[row, 12].Value = claim.EventDate;
                    worksheet.Cells[row, 13].Value = claim.CreatedBy;
                    worksheet.Cells[row, 14].Value = claim.ApprovedBy;
                    worksheet.Cells[row, 15].Value = claim.ApprovedDate;
                    worksheet.Cells[row, 16].Value = claim.PostedBy;
                    worksheet.Cells[row, 17].Value = claim.PostedDate;
                    worksheet.Cells[row, 18].Value = claim.Notes;

                    // Apply borders
                    for (int col = 1; col <= headers.Length; col++)
                    {
                        worksheet.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }

                    // Color code based on status
                    var statusColor = GetStatusColor(claim.Status);
                    worksheet.Cells[row, 10].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[row, 10].Style.Fill.BackgroundColor.SetColor(statusColor);

                    row++;
                }

                // Format columns
                worksheet.Cells[$"F2:F{row}"].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[$"G2:G{row}"].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[$"K2:L{row}"].Style.Numberformat.Format = "yyyy-mm-dd";
                worksheet.Cells[$"O2:P{row}"].Style.Numberformat.Format = "yyyy-mm-dd";

                // Auto-fit columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                package.SaveAs(new FileInfo(filePath));
            }
        }

        private void CreateSummarySheet(ExcelPackage package, List<IPSClaimExportRecord> claimsData, string exportedBy, IPSClaimExportOptions exportOptions)
        {
            var worksheet = package.Workbook.Worksheets.Add("Summary");

            // Set font
            var fontName = "Calibri";
            worksheet.Cells.Style.Font.Name = fontName;

            // ===== HEADER SECTION =====
            CreateHeaderSection(worksheet, exportedBy, exportOptions);

            int currentRow = 8; // Start after header

            // ===== KEY METRICS SECTION =====
            worksheet.Cells[$"A{currentRow}:H{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = "IPS CLAIMS MANAGEMENT - KEY METRICS";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Size = 14;
            worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
            currentRow += 2;

            // Calculate summary statistics
            var totalClaims = claimsData.Count;
            var totalClaimedAmount = claimsData.Sum(x => x.ClaimedAmount ?? 0);
            var totalApprovedAmount = claimsData.Sum(x => x.ApprovedAmount ?? 0);
            var registeredCount = claimsData.Count(x => x.Status == "Registered");
            var pendingCount = claimsData.Count(x => x.Status == "Pending");
            var approvedCount = claimsData.Count(x => x.Status == "Approved");
            var rejectedCount = claimsData.Count(x => x.Status == "Rejected");
            var postedCount = claimsData.Count(x =>
         x.Status == "Posted" || x.IsPosted == true
     );

            var loanProtectionCount = claimsData.Count(x => x.ClaimType == "Loan Protection" || x.ClaimType == "LoanProtection");
            var lifeSavingsCount = claimsData.Count(x => x.ClaimType == "Life Savings" || x.ClaimType == "LifeSavings");

            // Create metrics in 2x4 grid
            var metrics = new[]
            {
                new { Metric = "Total Claims", Value = totalClaims.ToString("N0"), Color = Color.LightBlue },
                new { Metric = "Total Claimed Amount", Value = totalClaimedAmount.ToString("N2"), Color = Color.LightGreen },
                new { Metric = "Total Approved Amount", Value = totalApprovedAmount.ToString("N2"), Color = Color.LightYellow },
                new { Metric = "Approval Rate", Value = totalClaims > 0 ? ((decimal)approvedCount / totalClaims).ToString("P1") : "0%", Color = Color.LightCoral },
                new { Metric = "Registered Claims", Value = registeredCount.ToString("N0"), Color = Color.LightBlue },
                new { Metric = "Pending Claims", Value = pendingCount.ToString("N0"), Color = Color.LightYellow },
                new { Metric = "Approved Claims", Value = approvedCount.ToString("N0"), Color = Color.LightGreen },
                new { Metric = "Rejected Claims", Value = rejectedCount.ToString("N0"), Color = Color.LightPink },
                new { Metric = "Posted Claims", Value = postedCount.ToString("N0"), Color = Color.LightGray },
                new { Metric = "Loan Protection", Value = loanProtectionCount.ToString("N0"), Color = Color.LightBlue },
                new { Metric = "Life Savings", Value = lifeSavingsCount.ToString("N0"), Color = Color.LightGreen },
                new { Metric = "Avg Claim Amount", Value = totalClaims > 0 ? (totalClaimedAmount / totalClaims).ToString("N2") : "0.00", Color = Color.LightYellow }
            };

            int startCol = 2;
            int metricRow = currentRow;

            for (int i = 0; i < metrics.Length; i++)
            {
                int row = metricRow + (i / 3);
                int col = startCol + (i % 3) * 2;

                // Metric cell
                worksheet.Cells[row, col].Value = metrics[i].Metric;
                worksheet.Cells[row, col].Style.Font.Bold = true;
                worksheet.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[row, col].Style.Fill.BackgroundColor.SetColor(metrics[i].Color);
                worksheet.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                // Value cell
                worksheet.Cells[row, col + 1].Value = metrics[i].Value;
                worksheet.Cells[row, col + 1].Style.Font.Bold = true;
                worksheet.Cells[row, col + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheet.Cells[row, col + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[row, col + 1].Style.Fill.BackgroundColor.SetColor(Color.White);
                worksheet.Cells[row, col + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            currentRow = metricRow + 4 + 2;

            // ===== RECOMMENDATIONS SECTION =====
            worksheet.Cells[$"A{currentRow}:H{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = "MANAGEMENT RECOMMENDATIONS";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Size = 14;
            worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
            currentRow += 2;

            var recommendations = new[]
            {
                "1. Review pending claims regularly to prevent delays",
                "2. Ensure all approved claims are posted within 48 hours",
                "3. Verify beneficiary information for all high-value claims",
                "4. Monitor rejection rates and identify patterns",
                "5. Schedule weekly reconciliation of posted claims",
                "6. Train staff on documentation requirements",
                "7. Implement escalation process for claims pending > 7 days",
                "8. Regular audit of approved vs claimed amounts"
            };

            foreach (var rec in recommendations)
            {
                worksheet.Cells[$"A{currentRow}:H{currentRow}"].Merge = true;
                worksheet.Cells[$"A{currentRow}"].Value = rec;
                worksheet.Cells[$"A{currentRow}"].Style.Font.Italic = true;
                currentRow++;
            }

            // Auto-fit columns
            worksheet.Cells["A:H"].AutoFitColumns();
        }

        private void CreateDetailedSheet(ExcelPackage package, List<IPSClaimExportRecord> claimsData, string exportedBy, IPSClaimExportOptions exportOptions)
        {
            var worksheet = package.Workbook.Worksheets.Add("Detailed Claims");

            // Set font
            var fontName = "Calibri";
            worksheet.Cells.Style.Font.Name = fontName;

            // ===== HEADER SECTION =====
            CreateHeaderSection(worksheet, exportedBy, exportOptions, "DETAILED CLAIMS LIST");

            int currentRow = 8;

            // ===== TABLE HEADERS =====
            var headers = new[]
            {
                "SN", "Claim ID", "Member ID", "Member Name", "Claim Type",
                "Branch", "Claimed Amount", "Approved Amount", "Beneficiary",
                "Contact", "Status", "Created Date", "Event Date", "Created By",
                "Approved By", "Approved Date", "Posted By", "Posted Date", "Notes"
            };

            int headerRow = currentRow;
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[headerRow, i + 1].Value = headers[i];
                worksheet.Cells[headerRow, i + 1].Style.Font.Bold = true;
                worksheet.Cells[headerRow, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[headerRow, i + 1].Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                worksheet.Cells[headerRow, i + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                worksheet.Cells[headerRow, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            // Data rows
            int dataRow = headerRow + 1;
            int serialNumber = 1;

            foreach (var claim in claimsData.OrderByDescending(x => x.CreatedDate))
            {
                worksheet.Cells[dataRow, 1].Value = serialNumber++;
                worksheet.Cells[dataRow, 2].Value = claim.ClaimNumber ?? claim.Id;
                worksheet.Cells[dataRow, 3].Value = claim.MemberId;
                worksheet.Cells[dataRow, 4].Value = claim.MemberName;
                worksheet.Cells[dataRow, 5].Value = claim.ClaimType;
                worksheet.Cells[dataRow, 6].Value = claim.BranchName;
                worksheet.Cells[dataRow, 7].Value = claim.ClaimedAmount;
                worksheet.Cells[dataRow, 8].Value = claim.ApprovedAmount;
                worksheet.Cells[dataRow, 9].Value = claim.BeneficiaryName;
                worksheet.Cells[dataRow, 10].Value = claim.BeneficiaryContact;
                worksheet.Cells[dataRow, 11].Value = claim.Status;
                worksheet.Cells[dataRow, 12].Value = claim.CreatedDate;
                worksheet.Cells[dataRow, 13].Value = claim.EventDate;
                worksheet.Cells[dataRow, 14].Value = claim.CreatedBy;
                worksheet.Cells[dataRow, 15].Value = claim.ApprovedBy;
                worksheet.Cells[dataRow, 16].Value = claim.ApprovedDate;
                worksheet.Cells[dataRow, 17].Value = claim.PostedBy;
                worksheet.Cells[dataRow, 18].Value = claim.PostedDate;
                worksheet.Cells[dataRow, 19].Value = claim.Notes;

                // Apply borders
                for (int col = 1; col <= headers.Length; col++)
                {
                    worksheet.Cells[dataRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                // Color code based on status
                var statusColor = GetStatusColor(claim.Status);
                worksheet.Cells[dataRow, 11].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[dataRow, 11].Style.Fill.BackgroundColor.SetColor(statusColor);

                // Highlight high-value claims
                if (claim.ClaimedAmount > 100000)
                {
                    worksheet.Cells[dataRow, 7].Style.Font.Bold = true;
                    worksheet.Cells[dataRow, 7].Style.Font.Color.SetColor(Color.Red);
                }

                dataRow++;
            }

            // Totals row
            if (claimsData.Any())
            {
                worksheet.Cells[dataRow, 6].Value = "TOTALS:";
                worksheet.Cells[dataRow, 6].Style.Font.Bold = true;
                worksheet.Cells[dataRow, 7].Value = claimsData.Sum(x => x.ClaimedAmount ?? 0);
                worksheet.Cells[dataRow, 8].Value = claimsData.Sum(x => x.ApprovedAmount ?? 0);
                worksheet.Cells[dataRow, 11].Value = $"Records: {claimsData.Count}";

                for (int col = 1; col <= headers.Length; col++)
                {
                    worksheet.Cells[dataRow, col].Style.Font.Bold = true;
                    worksheet.Cells[dataRow, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[dataRow, col].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
                    worksheet.Cells[dataRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
            }

            // Format numbers
            if (claimsData.Any())
            {
                worksheet.Cells[$"G{headerRow + 1}:H{dataRow}"].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[$"L{headerRow + 1}:M{dataRow}"].Style.Numberformat.Format = "yyyy-mm-dd";
                worksheet.Cells[$"P{headerRow + 1}:Q{dataRow}"].Style.Numberformat.Format = "yyyy-mm-dd";
            }

            // Auto-fit columns
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            // Freeze panes for easy scrolling
            worksheet.View.FreezePanes(headerRow + 1, 1);
        }

        private void CreateBranchAnalysisSheet(ExcelPackage package, List<IPSClaimExportRecord> claimsData, string exportedBy, IPSClaimExportOptions exportOptions)
        {
            var worksheet = package.Workbook.Worksheets.Add("Branch Analysis");

            // Set font
            var fontName = "Calibri";
            worksheet.Cells.Style.Font.Name = fontName;

            // ===== HEADER SECTION =====
            CreateHeaderSection(worksheet, exportedBy, exportOptions, "BRANCH PERFORMANCE ANALYSIS");

            int currentRow = 8;

            // ===== BRANCH SUMMARY TABLE =====
            worksheet.Cells[$"A{currentRow}:G{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = "BRANCH-WISE CLAIMS ANALYSIS";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Size = 14;
            worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
            currentRow += 2;

            // Branch summary headers
            var branchHeaders = new[] { "Branch", "Total Claims", "Registered", "Pending", "Approved", "Rejected", "Posted", "Total Claimed", "Total Approved", "Avg Processing Days" };
            for (int i = 0; i < branchHeaders.Length; i++)
            {
                worksheet.Cells[currentRow, 1 + i].Value = branchHeaders[i];
                worksheet.Cells[currentRow, 1 + i].Style.Font.Bold = true;
                worksheet.Cells[currentRow, 1 + i].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[currentRow, 1 + i].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                worksheet.Cells[currentRow, 1 + i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }
            currentRow++;

            // Group by branch
            var branchSummary = claimsData
                .GroupBy(x => x.BranchName)
                .Select(g => new
                {
                    BranchName = g.Key ?? "Unknown",
                    TotalClaims = g.Count(),
                    Registered = g.Count(x => x.Status == "Registered"),
                    Pending = g.Count(x => x.Status == "Pending"),
                    Approved = g.Count(x => x.Status == "Approved"),
                    Rejected = g.Count(x => x.Status == "Rejected"),
                    Posted = g.Count(x => x.Status == "Posted" || x.IsPosted == true),
                    TotalClaimed = g.Sum(x => x.ClaimedAmount ?? 0),
                    TotalApproved = g.Sum(x => x.ApprovedAmount ?? 0),
                    AvgProcessingDays = CalculateAverageProcessingDays(g.ToList())
                })
                .OrderByDescending(x => x.TotalClaims)
                .ToList();

            foreach (var branch in branchSummary)
            {
                worksheet.Cells[currentRow, 1].Value = branch.BranchName;
                worksheet.Cells[currentRow, 2].Value = branch.TotalClaims;
                worksheet.Cells[currentRow, 3].Value = branch.Registered;
                worksheet.Cells[currentRow, 4].Value = branch.Pending;
                worksheet.Cells[currentRow, 5].Value = branch.Approved;
                worksheet.Cells[currentRow, 6].Value = branch.Rejected;
                worksheet.Cells[currentRow, 7].Value = branch.Posted;
                worksheet.Cells[currentRow, 8].Value = branch.TotalClaimed;
                worksheet.Cells[currentRow, 9].Value = branch.TotalApproved;
                worksheet.Cells[currentRow, 10].Value = branch.AvgProcessingDays;

                for (int col = 1; col <= branchHeaders.Length; col++)
                {
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
                currentRow++;
            }

            // Format numbers
            if (branchSummary.Any())
            {
                worksheet.Cells[$"H{currentRow - branchSummary.Count}:I{currentRow - 1}"].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[$"J{currentRow - branchSummary.Count}:J{currentRow - 1}"].Style.Numberformat.Format = "0.0";
            }

            // Auto-fit columns
            worksheet.Cells["A:J"].AutoFitColumns();
        }

        private void CreateStatusAnalysisSheet(ExcelPackage package, List<IPSClaimExportRecord> claimsData, string exportedBy, IPSClaimExportOptions exportOptions)
        {
            var worksheet = package.Workbook.Worksheets.Add("Status Analysis");

            // Set font
            var fontName = "Calibri";
            worksheet.Cells.Style.Font.Name = fontName;

            // ===== HEADER SECTION =====
            CreateHeaderSection(worksheet, exportedBy, exportOptions, "STATUS ANALYSIS & WORKFLOW");

            int currentRow = 8;

            // ===== STATUS DISTRIBUTION =====
            worksheet.Cells[$"A{currentRow}:D{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = "CLAIMS STATUS DISTRIBUTION";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Size = 14;
            worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
            currentRow += 2;

            // Status summary headers
            var statusHeaders = new[] { "Status", "Count", "Percentage", "Total Amount", "Avg Amount" };
            for (int i = 0; i < statusHeaders.Length; i++)
            {
                worksheet.Cells[currentRow, 1 + i].Value = statusHeaders[i];
                worksheet.Cells[currentRow, 1 + i].Style.Font.Bold = true;
                worksheet.Cells[currentRow, 1 + i].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[currentRow, 1 + i].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                worksheet.Cells[currentRow, 1 + i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }
            currentRow++;

            // Group by status
            var totalClaims = claimsData.Count;
            var statusGroups = claimsData
                .GroupBy(x => x.Status)
                .Select(g => new
                {
                    Status = g.Key ?? "Unknown",
                    Count = g.Count(),
                    Percentage = (decimal)g.Count() / totalClaims,
                    TotalAmount = g.Sum(x => x.ClaimedAmount ?? 0),
                    AvgAmount = g.Average(x => x.ClaimedAmount ?? 0)
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            foreach (var status in statusGroups)
            {
                worksheet.Cells[currentRow, 1].Value = status.Status;
                worksheet.Cells[currentRow, 2].Value = status.Count;
                worksheet.Cells[currentRow, 3].Value = status.Percentage;
                worksheet.Cells[currentRow, 4].Value = status.TotalAmount;
                worksheet.Cells[currentRow, 5].Value = status.AvgAmount;

                // Color code the status cell
                var statusColor = GetStatusColor(status.Status);
                worksheet.Cells[currentRow, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[currentRow, 1].Style.Fill.BackgroundColor.SetColor(statusColor);

                for (int col = 1; col <= statusHeaders.Length; col++)
                {
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
                currentRow++;
            }

            // Format numbers
            if (statusGroups.Any())
            {
                worksheet.Cells[$"C{currentRow - statusGroups.Count}:C{currentRow - 1}"].Style.Numberformat.Format = "0.0%";
                worksheet.Cells[$"D{currentRow - statusGroups.Count}:E{currentRow - 1}"].Style.Numberformat.Format = "#,##0.00";
            }

            // Auto-fit columns
            worksheet.Cells["A:E"].AutoFitColumns();
        }

        private void CreateHeaderSection(ExcelWorksheet worksheet, string exportedBy, IPSClaimExportOptions exportOptions, string title = "IPS CLAIMS MANAGEMENT REPORT")
        {
            // ===== Bank/Company Header =====
            worksheet.Cells["A1:H1"].Merge = true;
            worksheet.Cells["A1"].Value = "INSURANCE CLAIMS MANAGEMENT SYSTEM";
            worksheet.Cells["A1"].Style.Font.Bold = true;
            worksheet.Cells["A1"].Style.Font.Size = 18;
            worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["A1"].Style.Font.Color.SetColor(Color.White);
            worksheet.Cells["A1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["A1"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 100, 0)); // Dark green
            worksheet.Row(1).Height = 30;

            // ===== Report Title =====
            worksheet.Cells["A2:H2"].Merge = true;
            worksheet.Cells["A2"].Value = title;
            worksheet.Cells["A2"].Style.Font.Bold = true;
            worksheet.Cells["A2"].Style.Font.Size = 14;
            worksheet.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["A2"].Style.Font.Color.SetColor(Color.Black);
            worksheet.Cells["A2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["A2"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 215, 0)); // Gold
            worksheet.Row(2).Height = 25;

            // ===== Export Information =====
            worksheet.Cells["A3:H3"].Merge = true;
            worksheet.Cells["A3"].Value = $"Exported By: {exportedBy} | Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            worksheet.Cells["A3"].Style.Font.Bold = true;
            worksheet.Cells["A3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Row(3).Height = 18;

            // ===== Date Range (if provided) =====
            if (!string.IsNullOrEmpty(exportOptions.StartDate) && !string.IsNullOrEmpty(exportOptions.EndDate))
            {
                worksheet.Cells["A4:H4"].Merge = true;
                worksheet.Cells["A4"].Value = $"Date Range: {exportOptions.StartDate} to {exportOptions.EndDate}";
                worksheet.Cells["A4"].Style.Font.Bold = true;
                worksheet.Cells["A4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                worksheet.Row(4).Height = 18;
            }

            // ===== Report Summary =====
            worksheet.Cells["A5:H5"].Merge = true;
            worksheet.Cells["A5"].Value = "Report Summary: Insurance Premium Savings (IPS) Claims Analysis";
            worksheet.Cells["A5"].Style.Font.Bold = true;
            worksheet.Cells["A5"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells["A5"].Style.Font.Italic = true;
            worksheet.Row(5).Height = 18;

            // ===== Confidential Notice =====
            worksheet.Cells["A6:H6"].Merge = true;
            worksheet.Cells["A6"].Value = "CONFIDENTIAL - For Internal Use Only";
            worksheet.Cells["A6"].Style.Font.Bold = true;
            worksheet.Cells["A6"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["A6"].Style.Font.Color.SetColor(Color.Red);
            worksheet.Cells["A6"].Style.Font.Italic = true;
            worksheet.Row(6).Height = 18;
        }

        private Color GetStatusColor(string status)
        {
            switch (status?.ToLower())
            {
                case "registered": return Color.FromArgb(173, 216, 230); // Light blue
                case "pending": return Color.FromArgb(255, 255, 224); // Light yellow
                case "approved": return Color.FromArgb(144, 238, 144); // Light green
                case "rejected": return Color.FromArgb(255, 182, 193); // Light pink
                case "posted": return Color.FromArgb(211, 211, 211); // Light gray
                default: return Color.White;
            }
        }

        private decimal CalculateAverageProcessingDays(List<IPSClaimExportRecord> claims)
        {
            var processedClaims = claims
                .Where(x => x.ApprovedDate.HasValue && x.CreatedDate != default)
                .ToList();

            if (!processedClaims.Any())
                return 0;

            var totalDays = processedClaims
                .Sum(x => (x.ApprovedDate.Value - x.CreatedDate).TotalDays);

            return (decimal)totalDays / processedClaims.Count;
        }
    }
}