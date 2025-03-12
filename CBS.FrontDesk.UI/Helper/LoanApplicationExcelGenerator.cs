using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.LoanConf;
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace CBS.FrontDesk.UI.Helper
{
    public static class LoanApplicationExcelGenerator
    {

        public static ExportFileResult GenerateLoanApplicationExcel(List<LoanApplication> loanApplications, Branch branch, string exportedBy, string fileTitle, string dateFrom, string dateTo)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Loan Applications");
                string period = string.IsNullOrEmpty(dateFrom) ? $"ALL-LOANS FROM {(branch?.Name?.ToUpper() ?? "ALL BRANCH")}" : $"{dateFrom} - {dateTo}";

                // Title
                worksheet.Cell(1, 1).Value = $"{fileTitle.ToUpper()} - LOAN APPLICATION ANALYSIS FOR {(branch?.Name?.ToUpper() ?? "ALL BRANCH")}";
                worksheet.Range(1, 1, 1, 25).Merge().Style.Font.FontSize = 14;

                // Export Details
                worksheet.Cell(2, 1).Value = $"Date Range: {period}";
                worksheet.Range(2, 1, 2, 25).Merge().Style.Font.Italic = true;

                worksheet.Cell(3, 1).Value = $"Export Date: {DateTime.Now:dd/MM/yyyy HH:mm:ss} BY {exportedBy}";
                worksheet.Range(3, 1, 3, 25).Merge().Style.Font.Italic = true;

                // Summary Section
                worksheet.Cell(5, 1).Value = "SUMMARY";
                worksheet.Range(5, 1, 5, 3).Merge().Style.Font.FontColor = XLColor.White;
                worksheet.Cell(5, 1).Style.Fill.BackgroundColor = XLColor.Chocolate;
                worksheet.Cell(5, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Summary Data
                var totalApplications = loanApplications.Count;
                var totalLoanAmount = loanApplications.Sum(l => l.Amount);
                var totalRefinancedAmount = loanApplications.Sum(l => l.RequestedAmount);
                var totalRestructuredBalance = loanApplications.Sum(l => l.RestructuredBalance);
                var totalCoverage = loanApplications.Sum(l => l.SavingAccountCoverageRate + l.PreferenceShareAccountCoverageAmount + l.DepositAccountCoverageAmount);
                var averageInterestRate = loanApplications.Average(l => l.InterestRate);
                var totalApprovedLoans = loanApplications.Count(l => l.IsApproved);
                var totalDisbursedLoans = loanApplications.Count(l => l.IsDisbursed);
                var totalPendingApprovals = loanApplications.Count(l => !l.IsApproved);
                var averageGracePeriod = loanApplications.Average(l => l.GracePeriod);
                var averageLoanDuration = loanApplications.Average(l => l.LoanDuration);

                var summaryData = new (string Label, object Value, string NumberFormat)[]
                {
                    ("1. Total Loan Applications", totalApplications, "#,##0"),
                    ("2. Total Loan Amount", totalLoanAmount, "#,##0.0"),
                    ("3. Total Refinanced Loans", totalRefinancedAmount, "#,##0.0"),
                    ("4. Total Restructured Balance", totalRestructuredBalance, "#,##0.0"),
                    ("5. Total Coverage Amount", totalCoverage, "#,##0.0"),
                    ("6. Average Interest Rate (%)", averageInterestRate, "0.00%"),
                    ("7. Approved Loan Applications", totalApprovedLoans, "#,##0"),
                    ("8. Disbursed Loan Applications", totalDisbursedLoans, "#,##0"),
                    ("9. Pending Loan Applications", totalPendingApprovals, "#,##0"),
                    ("10. Average Grace Period (Months)", averageGracePeriod, "#,##0"),
                    ("11. Average Loan Duration (Months)", averageLoanDuration, "#,##0")
                };

                int summaryRow = 6;
                foreach (var (label, value, format) in summaryData)
                {
                    worksheet.Cell(summaryRow, 1).Value = label;
                    worksheet.Cell(summaryRow, 2).Value = Convert.ToDouble(value);
                    worksheet.Cell(summaryRow, 2).Style.NumberFormat.Format = format;
                    worksheet.Cell(summaryRow, 2).Style.Font.SetBold();
                    worksheet.Range(summaryRow, 1, summaryRow, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    summaryRow++;
                }

                // Headers
                var headers = new[]
                {
                    "Application ID", "Customer Name", "Customer ID", "Application Date", "Approval Date", "Loan Type",
                    "Loan Category", "Loan Target", "Loan Amount", "Interest Rate (%)", "Grace Period", "Approval Status",
                    "Disbursement Status", "Loan Manager", "Branch Code", "Restructured Balance", "Refinanced Amount",
                    "Savings Coverage", "Deposit Coverage", "Preference Share Coverage", "Total Coverage", "Coverage %",
                    "Loan Purpose", "Disbursement Date", "Interest Paid Upfront"
                };

                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cell(14, i + 1).Value = headers[i];
                    worksheet.Cell(14, i + 1).Style.Font.SetBold().Fill.BackgroundColor = XLColor.LightGray;
                    worksheet.Cell(14, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(14, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }

                // Data Population
                int currentRow = 15;
                foreach (var group in loanApplications.GroupBy(l => new { l.LoanCategory, l.ApprovalStatus })
                         .OrderBy(g => g.Key.LoanCategory)
                         .ThenBy(g => g.Key.ApprovalStatus))
                {
                    // Group Header
                    worksheet.Cell(currentRow, 1).Value = $"CATEGORY: {group.Key.LoanCategory.ToUpper()} | STATUS: {group.Key.ApprovalStatus.ToUpper()}";
                    worksheet.Range(currentRow, 1, currentRow, headers.Length).Merge().Style.Font.SetBold();
                    currentRow++;

                    foreach (var loan in group.OrderBy(l => l.ApplicationDate))
                    {
                        decimal totalCoverageAmount = loan.SavingAccountCoverageRate + loan.PreferenceShareAccountCoverageAmount + loan.DepositAccountCoverageAmount;
                        decimal coveragePercentage = (loan.Amount > 0) ? (totalCoverageAmount / loan.Amount) * 100 : 0;

                        worksheet.Cell(currentRow, 1).Value = loan.Id;
                        worksheet.Cell(currentRow, 2).Value = loan.CustomerName;
                        worksheet.Cell(currentRow, 3).Value = loan.CustomerId;
                        worksheet.Cell(currentRow, 4).Value = loan.ApplicationDate.ToString("dd/MM/yyyy HH:mm:ss");
                        worksheet.Cell(currentRow, 5).Value = loan.ApprovalDate.ToString("dd/MM/yyyy HH:mm:ss") ?? "N/A";
                        worksheet.Cell(currentRow, 6).Value = loan.LoanType;
                        worksheet.Cell(currentRow, 7).Value = loan.LoanCategory;
                        worksheet.Cell(currentRow, 8).Value = loan.LoanTarget;
                        worksheet.Cell(currentRow, 9).Value = loan.Amount;
                        worksheet.Cell(currentRow, 10).Value = loan.InterestRate;
                        worksheet.Cell(currentRow, 11).Value = loan.GracePeriod;
                        worksheet.Cell(currentRow, 12).Value = loan.IsApproved ? "Approved" : "Pending";
                        worksheet.Cell(currentRow, 13).Value = loan.IsDisbursed ? "Yes" : "No";
                        worksheet.Cell(currentRow, 14).Value = loan.LoanManager;
                        worksheet.Cell(currentRow, 15).Value = loan.BranchCode;
                        worksheet.Cell(currentRow, 16).Value = loan.RestructuredBalance;
                        worksheet.Cell(currentRow, 17).Value = loan.RequestedAmount;
                        worksheet.Cell(currentRow, 18).Value = totalCoverageAmount;
                        worksheet.Cell(currentRow, 19).Value = coveragePercentage.ToString("0.0") + "%";
                        worksheet.Cell(currentRow, 20).Value = loan.LoanPurpose?.purposeName;
                        worksheet.Cell(currentRow, 21).Value = loan.DisbursementDate.ToString("dd/MM/yyyy HH:mm:ss") ?? "N/A";
                        worksheet.Cell(currentRow, 22).Value = loan.IsInterestPaidUpFront ? "Yes" : "No";

                        currentRow++;
                    }
                }

                worksheet.Columns().AdjustToContents();
                using (var stream = new MemoryStream()) { workbook.SaveAs(stream); return new ExportFileResult { Content = stream.ToArray(), ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", FileName = $"{fileTitle.Replace(" ", "_")}_{DateTime.Now:yyyyMMddHHmmss}.xlsx" }; }
            }
        }






        public class ExportFileResult
        {
            public byte[] Content { get; set; }
            public string ContentType { get; set; }
            public string FileName { get; set; }
        }
    }

}