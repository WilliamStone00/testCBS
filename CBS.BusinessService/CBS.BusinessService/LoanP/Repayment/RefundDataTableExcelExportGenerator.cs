using BusinessServices;
using CBS.FrontDesk.Data.Entity.AccountingV2;
using CBS.FrontDesk.Data.Entity.LoanRepayment;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.IO;

namespace CBS.BusinessService.LoanP.Repayment
{

    public class RefundDataTableExcelExportGenerator : BaseService
    {
        // -----------------------------------------------------------------------
        // PUBLIC CONVERSION METHOD
        // -----------------------------------------------------------------------
        public static List<LoanRefundDto> ConvertToRefundData(List<LoanRefundDto> refundData)
        {
            return refundData ?? new List<LoanRefundDto>();
        }

        // -----------------------------------------------------------------------
        // MAIN GENERATION METHOD
        // -----------------------------------------------------------------------
        public void GenerateRefundExcelFromTableData(
            List<LoanRefundDto> refundData,
            string filePath,
            string exportedBy,
            ExportOptions exportOptions)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                CreateSummarySheet(package, refundData,
                    GetBankName(), GetBranchCode(), GetBranchID(), GetBranchName(),
                    exportedBy, exportOptions);

                CreateBranchSummarySheet(package, refundData, exportedBy, exportOptions);
                CreateRefundDetailsSheet(package, refundData, exportedBy, exportOptions);

                var branches = refundData
                    .GroupBy(x => new { x.BranchId, x.BranchName, x.BranchCode })
                    .Select(g => new { g.Key.BranchId, g.Key.BranchName, g.Key.BranchCode })
                    .ToList();

                foreach (var branch in branches)
                {
                    var branchData = refundData.Where(x => x.BranchId == branch.BranchId).ToList();
                    CreateBranchSheet(package, branchData,
                        branch.BranchId, branch.BranchName, branch.BranchCode,
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
            List<LoanRefundDto> refundData,
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
                exportedBy, exportOptions, "REFUND SUMMARY REPORT", headerEndColumn);

            // ----- GENERAL SUMMARY -----
            worksheet.Cells[$"A{currentRow}:{headerEndColumn}{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = "GENERAL SUMMARY";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Size = 14;
            worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
            currentRow += 2;

            int totalRefunds = refundData.Count;
            int totalBranches = refundData.Select(x => x.BranchId).Distinct().Count();
            int completed = refundData.Count(x => x.IsCompleted);
            int notCompleted = totalRefunds - completed;
            int reversals = refundData.Count(x => x.IsReversal);
            int nonReversals = totalRefunds - reversals;
            decimal totalAmount = refundData.Sum(x => x.Amount);
            decimal totalPrincipal = refundData.Sum(x => x.Principal);
            decimal totalInterest = refundData.Sum(x => x.Interest);
            decimal totalPenalty = refundData.Sum(x => x.Penalty);
            decimal totalTax = refundData.Sum(x => x.Tax);
            decimal totalBalance = refundData.Sum(x => x.Balance);

            var summaryData = new[]
            {
            new { Metric = "Total Refund Transactions", Value = totalRefunds.ToString("N0"), Description = "Number of refund records" },
            new { Metric = "Total Branches", Value = totalBranches.ToString("N0"), Description = "Branches with refund activity" },
            new { Metric = "Completed Refunds", Value = completed.ToString("N0"), Description = "Fully processed refunds" },
            new { Metric = "Pending Refunds", Value = notCompleted.ToString("N0"), Description = "Not yet completed" },
            new { Metric = "Reversal Transactions", Value = reversals.ToString("N0"), Description = "Marked as reversal" },
            new { Metric = "Non‑Reversal Transactions", Value = nonReversals.ToString("N0"), Description = "Regular refunds" },
            new { Metric = "Total Refund Amount", Value = totalAmount.ToString("N2"), Description = "Sum of all refund amounts" },
            new { Metric = "Total Principal", Value = totalPrincipal.ToString("N2"), Description = "Sum of principal portions" },
            new { Metric = "Total Interest", Value = totalInterest.ToString("N2"), Description = "Sum of interest portions" },
            new { Metric = "Total Penalty", Value = totalPenalty.ToString("N2"), Description = "Sum of penalty portions" },
            new { Metric = "Total Tax", Value = totalTax.ToString("N2"), Description = "Sum of tax portions" },
            new { Metric = "Total Outstanding Balance", Value = totalBalance.ToString("N2"), Description = "Remaining loan balance after refund" }
        };

            currentRow = CreateTwoColumnTable(worksheet, currentRow, summaryData, "Metric", "Value", "Description");
            currentRow += 2;

            // ----- COMPLETION STATUS DISTRIBUTION -----
            var completionStatus = refundData.GroupBy(x => x.IsCompleted ? "Completed" : "Not Completed")
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();
            currentRow = CreateDistributionSection(worksheet, currentRow, headerEndColumn,
                "COMPLETION STATUS DISTRIBUTION", completionStatus, totalRefunds, Color.LightYellow);
            currentRow += 2;

            // ----- REVERSAL STATUS DISTRIBUTION -----
            var reversalStatus = refundData.GroupBy(x => x.IsReversal ? "Reversal" : "Non‑Reversal")
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();
            currentRow = CreateDistributionSection(worksheet, currentRow, headerEndColumn,
                "REVERSAL STATUS DISTRIBUTION", reversalStatus, totalRefunds, Color.LightCoral);
            currentRow += 2;

            // ----- PAYMENT METHOD DISTRIBUTION -----
            var paymentMethods = refundData
                .Where(x => !string.IsNullOrEmpty(x.PaymentMethod))
                .GroupBy(x => x.PaymentMethod)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();
            if (paymentMethods.Any())
            {
                currentRow = CreateDistributionSection(worksheet, currentRow, headerEndColumn,
                    "PAYMENT METHOD DISTRIBUTION", paymentMethods, totalRefunds, Color.LightGreen);
                currentRow += 2;
            }

            // ----- PAYMENT CHANNEL DISTRIBUTION -----
            var paymentChannels = refundData
                .Where(x => !string.IsNullOrEmpty(x.PaymentChannel))
                .GroupBy(x => x.PaymentChannel)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();
            if (paymentChannels.Any())
            {
                currentRow = CreateDistributionSection(worksheet, currentRow, headerEndColumn,
                    "PAYMENT CHANNEL DISTRIBUTION", paymentChannels, totalRefunds, Color.Gold);
            }

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        }

        // -----------------------------------------------------------------------
        // SHEET: BRANCH SUMMARY
        // -----------------------------------------------------------------------
        private void CreateBranchSummarySheet(
            ExcelPackage package,
            List<LoanRefundDto> refundData,
            string exportedBy,
            ExportOptions exportOptions)
        {
            var worksheet = package.Workbook.Worksheets.Add("Branch Summary");
            ApplyDefaultStyle(worksheet);

            int headerColumns = 13;
            string headerEndColumn = GetColumnLetter(headerColumns);
            int currentRow = CreateHeaderSection(worksheet, GetBankName(), GetBranchCode(), GetBranchID(), GetBranchName(),
                exportedBy, exportOptions, "BRANCH SUMMARY REPORT", headerEndColumn);

            worksheet.Cells[$"A{currentRow}:{headerEndColumn}{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = "BRANCH SUMMARY";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Size = 14;
            worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.BackgroundColor.SetColor(Color.LightCoral);
            currentRow += 2;

            var branchHeaders = new[]
            {
            "Branch ID", "Branch Code", "Branch Name",
            "# Refunds", "Total Amount", "Total Principal", "Total Interest",
            "Total Penalty", "Total Tax", "Total Balance",
            "Completed", "Pending", "Reversals"
        };

            for (int i = 0; i < branchHeaders.Length; i++)
            {
                worksheet.Cells[currentRow, i + 1].Value = branchHeaders[i];
                SetHeaderStyle(worksheet.Cells[currentRow, i + 1], Color.LightGreen);
            }
            currentRow++;

            var branchSummary = refundData
                .GroupBy(x => new { x.BranchId, x.BranchCode, x.BranchName })
                .Select(g => new
                {
                    g.Key.BranchId,
                    g.Key.BranchCode,
                    g.Key.BranchName,
                    RefundCount = g.Count(),
                    TotalAmount = g.Sum(x => x.Amount),
                    TotalPrincipal = g.Sum(x => x.Principal),
                    TotalInterest = g.Sum(x => x.Interest),
                    TotalPenalty = g.Sum(x => x.Penalty),
                    TotalTax = g.Sum(x => x.Tax),
                    TotalBalance = g.Sum(x => x.Balance),
                    Completed = g.Count(x => x.IsCompleted),
                    Pending = g.Count(x => !x.IsCompleted),
                    Reversals = g.Count(x => x.IsReversal)
                })
                .OrderByDescending(x => x.TotalAmount)
                .ToList();

            foreach (var branch in branchSummary)
            {
                worksheet.Cells[currentRow, 1].Value = branch.BranchId;
                worksheet.Cells[currentRow, 2].Value = branch.BranchCode;
                worksheet.Cells[currentRow, 3].Value = branch.BranchName;
                worksheet.Cells[currentRow, 4].Value = branch.RefundCount;
                worksheet.Cells[currentRow, 5].Value = branch.TotalAmount;
                worksheet.Cells[currentRow, 6].Value = branch.TotalPrincipal;
                worksheet.Cells[currentRow, 7].Value = branch.TotalInterest;
                worksheet.Cells[currentRow, 8].Value = branch.TotalPenalty;
                worksheet.Cells[currentRow, 9].Value = branch.TotalTax;
                worksheet.Cells[currentRow, 10].Value = branch.TotalBalance;
                worksheet.Cells[currentRow, 11].Value = branch.Completed;
                worksheet.Cells[currentRow, 12].Value = branch.Pending;
                worksheet.Cells[currentRow, 13].Value = branch.Reversals;

                for (int col = 1; col <= headerColumns; col++)
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                currentRow++;
            }

            if (branchSummary.Any())
            {
                worksheet.Cells[currentRow, 1].Value = "TOTALS:";
                worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
                worksheet.Cells[currentRow, 4].Value = branchSummary.Sum(x => x.RefundCount);
                worksheet.Cells[currentRow, 5].Value = branchSummary.Sum(x => x.TotalAmount);
                worksheet.Cells[currentRow, 6].Value = branchSummary.Sum(x => x.TotalPrincipal);
                worksheet.Cells[currentRow, 7].Value = branchSummary.Sum(x => x.TotalInterest);
                worksheet.Cells[currentRow, 8].Value = branchSummary.Sum(x => x.TotalPenalty);
                worksheet.Cells[currentRow, 9].Value = branchSummary.Sum(x => x.TotalTax);
                worksheet.Cells[currentRow, 10].Value = branchSummary.Sum(x => x.TotalBalance);
                worksheet.Cells[currentRow, 11].Value = branchSummary.Sum(x => x.Completed);
                worksheet.Cells[currentRow, 12].Value = branchSummary.Sum(x => x.Pending);
                worksheet.Cells[currentRow, 13].Value = branchSummary.Sum(x => x.Reversals);

                for (int col = 1; col <= headerColumns; col++)
                {
                    worksheet.Cells[currentRow, col].Style.Font.Bold = true;
                    worksheet.Cells[currentRow, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[currentRow, col].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
            }

            worksheet.Cells[$"E{5}:J{currentRow}"].Style.Numberformat.Format = "#,##0.00";
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        }

        // -----------------------------------------------------------------------
        // SHEET: REFUND DETAILS (ALL ENTRIES)
        // -----------------------------------------------------------------------
        private void CreateRefundDetailsSheet(
            ExcelPackage package,
            List<LoanRefundDto> refundData,
            string exportedBy,
            ExportOptions exportOptions)
        {
            var worksheet = package.Workbook.Worksheets.Add("Refund Details");
            ApplyDefaultStyle(worksheet);

            int headerColumns = 18;
            string headerEndColumn = GetColumnLetter(headerColumns);
            int currentRow = CreateHeaderSection(worksheet, GetBankName(), GetBranchCode(), GetBranchID(), GetBranchName(),
                exportedBy, exportOptions, "REFUND TRANSACTIONS DETAILED REPORT", headerEndColumn);

            int totalRefunds = refundData.Count;
            var completionStatus = refundData.GroupBy(x => x.IsCompleted ? "Completed" : "Not Completed")
                .Select(g => new { Status = g.Key, Count = g.Count() }).ToList();
            currentRow = CreateDistributionSection(worksheet, currentRow, headerEndColumn,
                "COMPLETION STATUS OVERVIEW", completionStatus, totalRefunds, Color.LightYellow);
            currentRow += 2;

            worksheet.Cells[$"A{currentRow}:{headerEndColumn}{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = "DETAILED REFUND ENTRIES";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Size = 14;
            worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.BackgroundColor.SetColor(Color.LightCoral);
            currentRow += 2;

            var headers = new[]
            {
            "SN", "Transaction Code", "Loan ID", "Customer ID", "Member Name",
            "Branch", "Payment Method", "Payment Channel",
            "Completed", "Reversal",
            "Amount", "Principal", "Interest", "Penalty", "Tax", "Balance",
            "Date of Payment", "Created Date"
        };

            int headerRow = currentRow;
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[headerRow, i + 1].Value = headers[i];
                SetHeaderStyle(worksheet.Cells[headerRow, i + 1], Color.LightGreen);
            }
            currentRow++;

            int sn = 1;
            decimal totalAmount = 0, totalPrincipal = 0, totalInterest = 0,
                    totalPenalty = 0, totalTax = 0, totalBalance = 0;

            foreach (var refund in refundData.OrderByDescending(x => x.DateOfPayment))
            {
                worksheet.Cells[currentRow, 1].Value = sn++;
                worksheet.Cells[currentRow, 2].Value = refund.TransactionCode;
                worksheet.Cells[currentRow, 3].Value = refund.LoanId;
                worksheet.Cells[currentRow, 4].Value = refund.CustomerId;
                worksheet.Cells[currentRow, 5].Value = refund.MemberName;
                worksheet.Cells[currentRow, 6].Value = refund.BranchName;
                worksheet.Cells[currentRow, 7].Value = refund.PaymentMethod;
                worksheet.Cells[currentRow, 8].Value = refund.PaymentChannel;
                worksheet.Cells[currentRow, 9].Value = refund.IsCompleted ? "Yes" : "No";
                worksheet.Cells[currentRow, 10].Value = refund.IsReversal ? "Yes" : "No";
                worksheet.Cells[currentRow, 11].Value = refund.Amount;
                worksheet.Cells[currentRow, 12].Value = refund.Principal;
                worksheet.Cells[currentRow, 13].Value = refund.Interest;
                worksheet.Cells[currentRow, 14].Value = refund.Penalty;
                worksheet.Cells[currentRow, 15].Value = refund.Tax;
                worksheet.Cells[currentRow, 16].Value = refund.Balance;
                worksheet.Cells[currentRow, 17].Value = refund.DateOfPayment.ToString("dd-MM-yyyy HH:mm:ss");
                worksheet.Cells[currentRow, 18].Value = refund.CreatedDate.ToString("dd-MM-yyyy HH:mm:ss");

                for (int col = 1; col <= headerColumns; col++)
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                totalAmount += refund.Amount;
                totalPrincipal += refund.Principal;
                totalInterest += refund.Interest;
                totalPenalty += refund.Penalty;
                totalTax += refund.Tax;
                totalBalance += refund.Balance;

                currentRow++;
            }

            if (refundData.Any())
            {
                worksheet.Cells[currentRow, 1].Value = "TOTALS:";
                worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
                worksheet.Cells[currentRow, 11].Value = totalAmount;
                worksheet.Cells[currentRow, 12].Value = totalPrincipal;
                worksheet.Cells[currentRow, 13].Value = totalInterest;
                worksheet.Cells[currentRow, 14].Value = totalPenalty;
                worksheet.Cells[currentRow, 15].Value = totalTax;
                worksheet.Cells[currentRow, 16].Value = totalBalance;

                for (int col = 1; col <= headerColumns; col++)
                {
                    worksheet.Cells[currentRow, col].Style.Font.Bold = true;
                    worksheet.Cells[currentRow, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[currentRow, col].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
            }

            worksheet.Cells[$"K{headerRow + 1}:P{currentRow}"].Style.Numberformat.Format = "#,##0.00";
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            worksheet.View.FreezePanes(headerRow + 1, 1);
        }

        // -----------------------------------------------------------------------
        // SHEET: INDIVIDUAL BRANCH DETAILS (UPDATED ORDER + TOTALS ROW)
        // -----------------------------------------------------------------------
        private void CreateBranchSheet(
            ExcelPackage package,
            List<LoanRefundDto> branchData,
            string branchId,
            string branchName,
            string branchCode,
            string exportedBy,
            ExportOptions exportOptions)
        {
            string sheetName = CleanSheetName($"{branchCode} - {branchName}");
            if (sheetName.Length > 31) sheetName = sheetName.Substring(0, 31);
            var worksheet = package.Workbook.Worksheets.Add(sheetName);
            ApplyDefaultStyle(worksheet);

            int headerColumns = 17;
            string headerEndColumn = GetColumnLetter(headerColumns);
            int currentRow = CreateHeaderSection(worksheet, GetBankName(), branchCode, branchId, branchName,
                exportedBy, exportOptions, $"BRANCH REFUND REPORT - {branchName}", headerEndColumn);

            // =========================================================================
            // 1. DETAILED REFUND ENTRIES TABLE (IMMEDIATELY AFTER HEADER)
            // =========================================================================
            worksheet.Cells[$"A{currentRow}:{headerEndColumn}{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = $"DETAILED REFUND ENTRIES - {branchName}";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Size = 14;
            worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.BackgroundColor.SetColor(Color.LightCoral);
            currentRow += 2;

            var branchHeaders = new[]
            {
            "SN", "Transaction Code", "Loan ID", "Customer ID", "Member Name",
            "Payment Method", "Payment Channel", "Completed", "Reversal",
            "Amount", "Principal", "Interest", "Penalty", "Tax", "Balance",
            "Date of Payment", "Created Date"
        };

            int headerRow = currentRow;
            for (int i = 0; i < branchHeaders.Length; i++)
            {
                worksheet.Cells[headerRow, i + 1].Value = branchHeaders[i];
                SetHeaderStyle(worksheet.Cells[headerRow, i + 1], Color.LightGreen);
            }
            currentRow++;

            int sn = 1;
            decimal totalAmount = 0, totalPrincipal = 0, totalInterest = 0,
                    totalPenalty = 0, totalTax = 0, totalBalance = 0;

            foreach (var refund in branchData.OrderByDescending(x => x.DateOfPayment))
            {
                worksheet.Cells[currentRow, 1].Value = sn++;
                worksheet.Cells[currentRow, 2].Value = refund.TransactionCode;
                worksheet.Cells[currentRow, 3].Value = refund.LoanId;
                worksheet.Cells[currentRow, 4].Value = refund.CustomerId;
                worksheet.Cells[currentRow, 5].Value = refund.MemberName;
                worksheet.Cells[currentRow, 6].Value = refund.PaymentMethod;
                worksheet.Cells[currentRow, 7].Value = refund.PaymentChannel;
                worksheet.Cells[currentRow, 8].Value = refund.IsCompleted ? "Yes" : "No";
                worksheet.Cells[currentRow, 9].Value = refund.IsReversal ? "Yes" : "No";
                worksheet.Cells[currentRow, 10].Value = refund.Amount;
                worksheet.Cells[currentRow, 11].Value = refund.Principal;
                worksheet.Cells[currentRow, 12].Value = refund.Interest;
                worksheet.Cells[currentRow, 13].Value = refund.Penalty;
                worksheet.Cells[currentRow, 14].Value = refund.Tax;
                worksheet.Cells[currentRow, 15].Value = refund.Balance;
                worksheet.Cells[currentRow, 16].Value = refund.DateOfPayment.ToString("dd-MM-yyyy HH:mm:ss");
                worksheet.Cells[currentRow, 17].Value = refund.CreatedDate.ToString("dd-MM-yyyy HH:mm:ss");

                for (int col = 1; col <= headerColumns; col++)
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                totalAmount += refund.Amount;
                totalPrincipal += refund.Principal;
                totalInterest += refund.Interest;
                totalPenalty += refund.Penalty;
                totalTax += refund.Tax;
                totalBalance += refund.Balance;

                currentRow++;
            }

            // ----- Totals row (exactly like the image) -----
            if (branchData.Any())
            {
                worksheet.Cells[currentRow, 1].Value = "TOTAL:";
                worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
                worksheet.Cells[currentRow, 10].Value = totalAmount;
                worksheet.Cells[currentRow, 11].Value = totalPrincipal;
                worksheet.Cells[currentRow, 12].Value = totalInterest;
                worksheet.Cells[currentRow, 13].Value = totalPenalty;
                worksheet.Cells[currentRow, 14].Value = totalTax;
                worksheet.Cells[currentRow, 15].Value = totalBalance;

                for (int col = 1; col <= headerColumns; col++)
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

            // =========================================================================
            // 2. BRANCH SUMMARY (TWO-COLUMN METRICS) – now AFTER detailed table
            // =========================================================================
            worksheet.Cells[$"A{currentRow}:{headerEndColumn}{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = $"BRANCH SUMMARY - {branchName}";
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Size = 14;
            worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A{currentRow}"].Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
            currentRow += 2;

            int total = branchData.Count;
            int completed = branchData.Count(x => x.IsCompleted);
            int pending = total - completed;
            int reversals = branchData.Count(x => x.IsReversal);
            decimal sumAmount = branchData.Sum(x => x.Amount);
            decimal sumPrincipal = branchData.Sum(x => x.Principal);
            decimal sumInterest = branchData.Sum(x => x.Interest);
            decimal sumPenalty = branchData.Sum(x => x.Penalty);
            decimal sumTax = branchData.Sum(x => x.Tax);
            decimal sumBalance = branchData.Sum(x => x.Balance);

            var summaryItems = new[]
            {
            new { Metric = "Total Refund Transactions", Value = total.ToString("N0") },
            new { Metric = "Completed Refunds", Value = completed.ToString("N0") },
            new { Metric = "Pending Refunds", Value = pending.ToString("N0") },
            new { Metric = "Reversal Transactions", Value = reversals.ToString("N0") },
            new { Metric = "Total Refund Amount", Value = sumAmount.ToString("N2") },
            new { Metric = "Total Principal", Value = sumPrincipal.ToString("N2") },
            new { Metric = "Total Interest", Value = sumInterest.ToString("N2") },
            new { Metric = "Total Penalty", Value = sumPenalty.ToString("N2") },
            new { Metric = "Total Tax", Value = sumTax.ToString("N2") },
            new { Metric = "Total Outstanding Balance", Value = sumBalance.ToString("N2") }
        };

            currentRow = CreateTwoColumnTable(worksheet, currentRow, summaryItems, "Metric", "Value");
            currentRow += 2;

            // =========================================================================
            // 3. COMPLETION STATUS DISTRIBUTION (shows both Completed/Not Completed)
            // =========================================================================
            var completionStatus = branchData
                .GroupBy(x => x.IsCompleted ? "Completed" : "Not Completed")
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToList();
            currentRow = CreateDistributionSection(worksheet, currentRow, headerEndColumn,
                "COMPLETION STATUS", completionStatus, total, Color.LightYellow);
            currentRow += 2;

            // ----- Format currency columns (Amount through Balance) -----
            worksheet.Cells[$"J{headerRow + 1}:O{currentRow - 2}"].Style.Numberformat.Format = "#,##0.00";

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
