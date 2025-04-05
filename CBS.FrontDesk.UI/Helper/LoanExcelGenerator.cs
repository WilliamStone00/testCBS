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
    public static class LoanExcelGenerator
    {
        public static ExportFileResult GenerateLoanExcelx(List<Loan> loanDetails, string branchName, string exportedBy, string fileTitle, string dateFrom, string dateTo)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Loan Analysis");

                // Title with File Title
                worksheet.Cell(1, 1).Value = $"{fileTitle.ToUpper()} - LOAN ANALYSIS FOR {branchName.ToUpper()}";
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                worksheet.Cell(1, 1).Style.Font.FontSize = 14;
                worksheet.Range(1, 1, 1, 45).Merge();
                worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Export Details with Date Range
                worksheet.Cell(2, 1).Value = $"Date Range: {dateFrom} - {dateTo}";
                worksheet.Cell(2, 1).Style.Font.Italic = true;
                worksheet.Cell(2, 1).Style.Font.FontSize = 10;
                worksheet.Range(2, 1, 2, 45).Merge();
                worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(3, 1).Value = $"Export Date: {DateTime.Now} BY {exportedBy}";
                worksheet.Cell(3, 1).Style.Font.Italic = true;
                worksheet.Cell(3, 1).Style.Font.FontSize = 10;
                worksheet.Range(3, 1, 3, 45).Merge();
                worksheet.Cell(3, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Summary Table
                worksheet.Cell(5, 1).Value = "Summary Table";
                worksheet.Cell(5, 1).Style.Font.Bold = true;
                worksheet.Range(5, 1, 5, 6).Merge();
                worksheet.Cell(5, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                var totalLoans = loanDetails.Count;
                var totalLoanVolume = loanDetails.Sum(l => l.LoanAmount);
                var totalRepayment = loanDetails.Sum(l => l.Paid);
                var totalBalance = loanDetails.Sum(l => l.Balance);
                var totalDueAmount = loanDetails.Sum(l => l.DueAmount);
                var percentageRefund = (totalRepayment / (totalLoanVolume > 0 ? totalLoanVolume : 1)) * 100;

                worksheet.Cell(6, 1).Value = "Total Loans (Number)";
                worksheet.Cell(6, 2).Value = totalLoans;

                worksheet.Cell(7, 1).Value = "Total Loans (Volume)";
                worksheet.Cell(7, 2).Value = totalLoanVolume;
                worksheet.Cell(7, 2).Style.NumberFormat.Format = "#,##0.0";

                worksheet.Cell(8, 1).Value = "Total Repayment (Volume)";
                worksheet.Cell(8, 2).Value = totalRepayment;
                worksheet.Cell(8, 2).Style.NumberFormat.Format = "#,##0.0";

                worksheet.Cell(9, 1).Value = "Total Balance (Volume)";
                worksheet.Cell(9, 2).Value = totalBalance;
                worksheet.Cell(9, 2).Style.NumberFormat.Format = "#,##0.0";

                worksheet.Cell(10, 1).Value = "Total Due Amount (Volume)";
                worksheet.Cell(10, 2).Value = totalDueAmount;
                worksheet.Cell(10, 2).Style.NumberFormat.Format = "#,##0.0";

                worksheet.Cell(11, 1).Value = "Percentage Refund";
                worksheet.Cell(11, 2).Value = percentageRefund;
                worksheet.Cell(11, 2).Style.NumberFormat.Format = "0.00%";

                // Headers
                var headers = new[]
                 {
                    "Loan Reference", "Customer Name", "Customer Reference", "Loan Date", "Loan Amount", "Interest Rate (%)",
                    "Interest Forcasted", "Accrual Interest", "Last Interest Calculated Date", "VAT Interest", "VAT",
                    "Penalty", "Paid", "Balance", "Loan Duration", "Maturity Date", "Due Amount", "AccountNumber",
                    "BranchCode", "LastPayment", "Last Refund Date", "Loan Manager", "Advanced Payment Days",
                    "Advanced Payment Amount", "Deliquent Days", "Deliquent Interest", "Deliquent Amount",
                    "Last Deliquecy Processed Date", "Deliquent Status", "LoanType", "Interest Amount Upfront",
                    "Savings", "OShares", "PShares", "Deposit", "Salary", "Shortee", "Co Obligor",
                    "Co Operation Gurantor", "Other Guarantee Fund", "Total Fund Guranteed",
                    "Percentage Of Liquidity Coverage", "Percentage Of Collateral Coverage",
                    "Percentage Of Over All Coverage", "Coverage Status"
                };


                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cell(13, i + 1).Value = headers[i];
                    worksheet.Cell(13, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(13, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                    worksheet.Cell(13, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(13, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(13, i + 1).Style.Border.OutsideBorderColor = XLColor.Black;
                }

                // Grouping, Ordering, and Data Population
                int currentRow = 14;

                foreach (var group in loanDetails
                             .GroupBy(l => new { l.LoanType, l.DeliquentStatus })
                             .OrderBy(g => g.Key.LoanType)
                             .ThenBy(g => g.Key.DeliquentStatus))
                {
                    worksheet.Cell(currentRow, 1).Value = $"Loan Type: {group.Key.LoanType}, Delinquency Status: {group.Key.DeliquentStatus}";
                    worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                    worksheet.Range(currentRow, 1, currentRow, 45).Merge();
                    currentRow++;

                    foreach (var loan in group.OrderBy(l => l.LoanDate))
                    {
                        decimal totalCoveredAmount = loan.Savings + loan.OShares + loan.PShares + loan.Deposit + loan.Salary +
                                                  loan.Shortee + loan.Co_Obligor + loan.Co_OperationGurantor + loan.OtherGuaranteeFund;

                        decimal overallCoveragePercentage = (loan.DueAmount > 0) ? (totalCoveredAmount / loan.DueAmount) * 100 : 0;
                        string coverageStatus = overallCoveragePercentage >= 100 ? "Covered" : "Not Covered";

                        worksheet.Cell(currentRow, 1).Value = loan.Id;
                        worksheet.Cell(currentRow, 2).Value = loan.CustomerName;
                        worksheet.Cell(currentRow, 3).Value = loan.CustomerId;
                        worksheet.Cell(currentRow, 4).Value = loan.LoanDate.ToString("dd/MM/yyyy, hh:mm:ss");
                        worksheet.Cell(currentRow, 5).Value = loan.LoanAmount;
                        worksheet.Cell(currentRow, 6).Value = loan.InterestRate;
                        worksheet.Cell(currentRow, 7).Value = loan.InterestForcasted;
                        worksheet.Cell(currentRow, 8).Value = loan.AccrualInterest;
                        worksheet.Cell(currentRow, 9).Value = loan.LastInterestCalculatedDate.ToString("dd/MM/yyyy, hh:mm:ss");
                        worksheet.Cell(currentRow, 10).Value = loan.Tax;
                        worksheet.Cell(currentRow, 11).Value = loan.VatRate;
                        worksheet.Cell(currentRow, 12).Value = loan.Penalty;
                        worksheet.Cell(currentRow, 13).Value = loan.Paid;
                        worksheet.Cell(currentRow, 14).Value = loan.Balance;
                        worksheet.Cell(currentRow, 15).Value = loan.LoanDuration;
                        worksheet.Cell(currentRow, 16).Value = loan.MaturityDate.ToString("dd/MM/yyyy, hh:mm:ss");
                        worksheet.Cell(currentRow, 17).Value = loan.DueAmount;
                        worksheet.Cell(currentRow, 18).Value = loan.AccountNumber;
                        worksheet.Cell(currentRow, 19).Value = loan.BranchCode;
                        worksheet.Cell(currentRow, 20).Value = loan.LastPayment;
                        worksheet.Cell(currentRow, 21).Value = loan.LastRefundDate.ToString("dd/MM/yyyy, hh:mm:ss");
                        worksheet.Cell(currentRow, 22).Value = loan.LoanManager;
                        worksheet.Cell(currentRow, 23).Value = loan.AdvancedPaymentDays;
                        worksheet.Cell(currentRow, 24).Value = loan.AdvancedPaymentAmount;
                        worksheet.Cell(currentRow, 25).Value = loan.DeliquentDays;
                        worksheet.Cell(currentRow, 26).Value = loan.DeliquentInterest;
                        worksheet.Cell(currentRow, 27).Value = loan.DeliquentAmount;
                        worksheet.Cell(currentRow, 28).Value = loan.LastDeliquecyProcessedDate?.ToString("dd/MM/yyyy, hh:mm:ss");
                        worksheet.Cell(currentRow, 29).Value = loan.DeliquentStatus;
                        worksheet.Cell(currentRow, 30).Value = loan.LoanType;
                        worksheet.Cell(currentRow, 31).Value = loan.InterestAmountUpfront;
                        worksheet.Cell(currentRow, 32).Value = loan.Savings;
                        worksheet.Cell(currentRow, 33).Value = loan.OShares;
                        worksheet.Cell(currentRow, 34).Value = loan.PShares;
                        worksheet.Cell(currentRow, 35).Value = loan.Deposit;
                        worksheet.Cell(currentRow, 36).Value = loan.Salary;
                        worksheet.Cell(currentRow, 37).Value = loan.Shortee;
                        worksheet.Cell(currentRow, 38).Value = loan.Co_Obligor;
                        worksheet.Cell(currentRow, 39).Value = loan.Co_OperationGurantor;
                        worksheet.Cell(currentRow, 40).Value = loan.OtherGuaranteeFund;
                        worksheet.Cell(currentRow, 41).Value = totalCoveredAmount;
                        worksheet.Cell(currentRow, 42).Value = loan.PercentageOfLiquidityCoverage;
                        worksheet.Cell(currentRow, 43).Value = loan.PercentageOfCollateralCoverage;
                        worksheet.Cell(currentRow, 44).Value = overallCoveragePercentage;
                        worksheet.Cell(currentRow, 45).Value = coverageStatus;

                        // Apply styles for current row
                        for (int col = 1; col <= 45; col++)
                        {
                            worksheet.Cell(currentRow, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            worksheet.Cell(currentRow, col).Style.Border.OutsideBorderColor = XLColor.Black;
                        }

                        // Format currency and percentage cells
                        worksheet.Cell(currentRow, 5).Style.NumberFormat.Format = "#,##0.0";  // Loan Amount
                        worksheet.Cell(currentRow, 6).Style.NumberFormat.Format = "0.0";    // Interest Rate
                        worksheet.Cell(currentRow, 17).Style.NumberFormat.Format = "#,##0.0"; // Due Amount
                        worksheet.Cell(currentRow, 44).Style.NumberFormat.Format = "0.0";    // Overall Coverage

                        currentRow++;
                    }

                    // Add Group Totals
                    worksheet.Cell(currentRow, 1).Value = "Group Total:";
                    worksheet.Cell(currentRow, 2).FormulaA1 = $"SUM({worksheet.Cell(14, 2).Address}:{worksheet.Cell(currentRow, 2)})";

                    currentRow++;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return new ExportFileResult
                    {
                        Content = stream.ToArray(),
                        ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        FileName = $"{fileTitle.Replace(" ", "_")}_LoanAnalysis_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
                    };
                }
            }
        }

        public static ExportFileResult GenerateLoanExcel(List<Loan> loanDetails, Branch branch, string exportedBy, string fileTitle, string dateFrom, string dateTo)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Loan Analysis");
                string period = dateFrom==string.Empty ? $"ALL-LOANS FROM {(branch?.Name?.ToUpper() ?? "ALL BRANCH")}" : $"{dateFrom} - {dateTo}";
                // Title with File Title
                worksheet.Cell(1, 1).Value = $"{fileTitle.ToUpper()} - LOAN ANALYSIS FOR {(branch?.Name?.ToUpper() ?? "ALL BRANCH")}";
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                worksheet.Cell(1, 1).Style.Font.FontSize = 14;
                worksheet.Range(1, 1, 1, 46).Merge();
                worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(1, 1).Style.Font.FontName = "Bahnschrift Light";
                worksheet.Cell(1, 1).Style.Font.FontSize = 14;
                // Export Details with Date Range
                worksheet.Cell(2, 1).Value = $"Date Range: {period}";
                worksheet.Cell(2, 1).Style.Font.Italic = false;
                worksheet.Cell(2, 1).Style.Font.FontSize = 10;
                worksheet.Cell(1, 1).Style.Font.FontName = "Bahnschrift Light";
                worksheet.Range(2, 1, 2, 46).Merge();
                worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                worksheet.Cell(3, 1).Value = $"Export Date: {DateTime.Now} BY {exportedBy}";
                worksheet.Cell(3, 1).Style.Font.Italic = false;
                worksheet.Cell(3, 1).Style.Font.FontSize = 10;
                worksheet.Cell(1, 1).Style.Font.FontName = "Bahnschrift Light";
                worksheet.Range(3, 1, 3, 46).Merge();
                worksheet.Cell(3, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;


                // Summary Title
                worksheet.Cell(5, 1).Value = "SUMMARY";
                worksheet.Cell(5, 1).Style.Font.Bold = true;
                worksheet.Cell(5, 1).Style.Fill.BackgroundColor = XLColor.Chocolate;
                worksheet.Cell(5, 1).Style.Font.FontColor = XLColor.White;
                worksheet.Cell(1, 1).Style.Font.FontName = "Bahnschrift Light";
                worksheet.Range(5, 1, 5, 2).Merge();
                worksheet.Cell(5, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Summary Data
                var totalLoans = loanDetails.Count;
                var totalLoanVolume = loanDetails.Sum(l => l.LoanAmount);
                var totalAccrualInterest = loanDetails.Sum(l => l.AccrualInterest);
                var totalDeliquentInterest = loanDetails.Sum(l => l.DeliquentInterest);
                var totalRepayment = loanDetails.Sum(l => l.Paid);
                var totalBalance = loanDetails.Sum(l => l.Balance);
                var totalDueAmount = loanDetails.Sum(l => l.DueAmount);
                var percentageRefund = (totalRepayment / (totalLoanVolume > 0 ? totalLoanVolume : 1)) * 100;

                // Summary Details with Borders
                var summaryData = new (string Label, object Value, string NumberFormat)[]
                {
                    ("1. Number of Loans", totalLoans, "#,##0"),
                    ("2. Volume of Loan", totalLoanVolume, "#,##0.0"),
                    ("3. Volume of Accrual Interest", totalAccrualInterest, "#,##0.0"),
                    ("4. Volume of Deliquent Interest", totalDeliquentInterest, "#,##0.0"),
                    ("5. Volume of Refund", totalRepayment, "#,##0.0"),
                    ("6. Percentage of Refund", percentageRefund, "0.0"),
                    ("7. Outstanding Balance", totalBalance, "#,##0.0"),
                    ("8. Total Due Amount", totalDueAmount, "#,##0.0")
                };

                int currentRowx = 6;
                foreach (var (label, value, format) in summaryData)
                {
                    
                    worksheet.Cell(currentRowx, 1).Value = label;
                    worksheet.Cell(currentRowx, 2).Value = Convert.ToDouble(value);  // Ensures consistent numeric format
                    worksheet.Cell(currentRowx, 2).Style.NumberFormat.Format = format;
                    worksheet.Cell(currentRowx, 2).Style.Font.Bold = true;
                    // Apply borders to both cells
                    worksheet.Range(currentRowx, 1, currentRowx, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Range(currentRowx, 1, currentRowx, 2).Style.Border.OutsideBorderColor = XLColor.Black;

                    currentRowx++;
                }



                // Headers
                var headers = new[]
                {
                    "Loan Reference", "Member's Name", "Member's Reference", "Loan Date", "Loan Amount", "Interest Rate (%)",
                    "Interest Forcasted", "Accrual Interest", "Last Interest Calculated Date", "VAT Amount", "VAT Rate",
                    "Penalty", "Paid", "Balance", "Loan Duration (M)", "Maturity Date", "Due Amount", "Account Number",
                    "Branch Code", "Last Payment", "Last Refund Date", "Loan Manager", "Advanced Payment Days",
                    "Advanced Payment Amount", "Deliquent Days", "Deliquent Interest", "Deliquent Amount",
                    "Last Deliquecy Processed Date", "Deliquent Status", "Loan Type", "Loan Category", "Interest Amount Upfront",
                    "Savings", "OShares", "PShares", "Deposit", "Salary", "Shortee", "Co Obligor",
                    "Co Operation Gurantor", "Other Guarantee Fund", "Total Fund Guranteed",
                    "Percentage Of Liquidity Coverage", "Percentage Of Collateral Coverage",
                    "Percentage Of Over All Coverage", "Coverage Status"
                };
        

                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cell(15, i + 1).Value = headers[i];
                    worksheet.Cell(15, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(15, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                    worksheet.Cell(15, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(15, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(15, i + 1).Style.Border.OutsideBorderColor = XLColor.Black;
                    worksheet.Cell(15, i + 1).Style.Font.FontName = "Bahnschrift Light";
                }

                // Grouping, Ordering, and Data Population
                int currentRow = 16;

                foreach (var branchGroup in loanDetails
                    .GroupBy(l => l.BranchCode)
                    .OrderBy(g => g.Key))
                {
                    var branchName = loanDetails.FirstOrDefault(l => l.BranchCode == branchGroup.Key)?.BranchName ?? "Unknown";

                    worksheet.Cell(currentRow, 1).Value = $"BRANCH: {branchName.ToUpper()}";
                    worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                    worksheet.Range(currentRow, 1, currentRow, 46).Merge();
                    currentRow++;

                    decimal branchLoanAmount = 0;
                    decimal branchPaid = 0;
                    decimal branchBalance = 0;
                    decimal branchDueAmount = 0;

                    foreach (var deliquentGroup in branchGroup.GroupBy(l => l.DeliquentStatus).OrderBy(g => g.Key))
                    {
                        worksheet.Cell(currentRow, 1).Value = $"  DELINQUENT STATUS: {deliquentGroup.Key?.ToUpper()}";
                        worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                        worksheet.Range(currentRow, 1, currentRow, 46).Merge();
                        currentRow++;

                        decimal groupLoanAmount = 0;
                        decimal groupInterestForecasted = 0;
                        decimal groupAccrualInterest = 0;
                        decimal groupVAT = 0;
                        decimal groupPenalty = 0;
                        decimal groupPaid = 0;
                        decimal groupBalance = 0;
                        decimal groupDueAmount = 0;

                        foreach (var loan in deliquentGroup.OrderBy(l => l.LoanDate))
                        {
                            decimal totalCoveredAmount = loan.Savings + loan.OShares + loan.PShares + loan.Deposit + loan.Salary +
                                                         loan.Shortee + loan.Co_Obligor + loan.Co_OperationGurantor + loan.OtherGuaranteeFund;

                            decimal overallCoveragePercentage = (loan.DueAmount > 0) ? (totalCoveredAmount / loan.DueAmount) * 100 : 0;
                            string coverageStatus = overallCoveragePercentage >= 100 ? "Covered" : "Not Covered";

                            worksheet.Cell(currentRow, 1).Value = loan.Id;
                            worksheet.Cell(currentRow, 2).Value = loan.CustomerName;
                            worksheet.Cell(currentRow, 3).Value = loan.CustomerId;
                            worksheet.Cell(currentRow, 4).Value = loan.LoanDate.ToString("dd/MM/yyyy, hh:mm:ss");
                            worksheet.Cell(currentRow, 5).Value = loan.LoanAmount;
                            worksheet.Cell(currentRow, 6).Value = loan.InterestRate;
                            worksheet.Cell(currentRow, 7).Value = loan.InterestForcasted;
                            worksheet.Cell(currentRow, 8).Value = loan.AccrualInterest;
                            worksheet.Cell(currentRow, 9).Value = loan.LastInterestCalculatedDate.ToString("dd/MM/yyyy, hh:mm:ss");
                            worksheet.Cell(currentRow, 10).Value = loan.Tax;
                            worksheet.Cell(currentRow, 11).Value = loan.VatRate;
                            worksheet.Cell(currentRow, 12).Value = loan.Penalty;
                            worksheet.Cell(currentRow, 13).Value = loan.Paid;
                            worksheet.Cell(currentRow, 14).Value = loan.Balance;
                            worksheet.Cell(currentRow, 15).Value = loan.LoanDuration;
                            worksheet.Cell(currentRow, 16).Value = loan.MaturityDate.ToString("dd/MM/yyyy, hh:mm:ss");
                            worksheet.Cell(currentRow, 17).Value = loan.DueAmount;
                            worksheet.Cell(currentRow, 18).Value = loan.AccountNumber;
                            worksheet.Cell(currentRow, 19).Value = loan.BranchCode;
                            worksheet.Cell(currentRow, 20).Value = loan.LastPayment;
                            worksheet.Cell(currentRow, 21).Value = loan.LastRefundDate.ToString("dd/MM/yyyy, hh:mm:ss");
                            worksheet.Cell(currentRow, 22).Value = loan.LoanManager;
                            worksheet.Cell(currentRow, 23).Value = loan.AdvancedPaymentDays;
                            worksheet.Cell(currentRow, 24).Value = loan.AdvancedPaymentAmount;
                            worksheet.Cell(currentRow, 25).Value = loan.DeliquentDays;
                            worksheet.Cell(currentRow, 26).Value = loan.DeliquentInterest;
                            worksheet.Cell(currentRow, 27).Value = loan.DeliquentAmount;
                            worksheet.Cell(currentRow, 28).Value = loan.LastDeliquecyProcessedDate?.ToString("dd/MM/yyyy, hh:mm:ss");
                            worksheet.Cell(currentRow, 29).Value = loan.DeliquentStatus;
                            worksheet.Cell(currentRow, 30).Value = loan.LoanType;
                            worksheet.Cell(currentRow, 31).Value = loan.LoanCategory;
                            worksheet.Cell(currentRow, 32).Value = loan.InterestAmountUpfront;
                            worksheet.Cell(currentRow, 33).Value = loan.Savings;
                            worksheet.Cell(currentRow, 34).Value = loan.OShares;
                            worksheet.Cell(currentRow, 35).Value = loan.PShares;
                            worksheet.Cell(currentRow, 36).Value = loan.Deposit;
                            worksheet.Cell(currentRow, 37).Value = loan.Salary;
                            worksheet.Cell(currentRow, 38).Value = loan.Shortee;
                            worksheet.Cell(currentRow, 39).Value = loan.Co_Obligor;
                            worksheet.Cell(currentRow, 40).Value = loan.Co_OperationGurantor;
                            worksheet.Cell(currentRow, 41).Value = loan.OtherGuaranteeFund;
                            worksheet.Cell(currentRow, 42).Value = totalCoveredAmount;
                            worksheet.Cell(currentRow, 43).Value = loan.PercentageOfLiquidityCoverage;
                            worksheet.Cell(currentRow, 44).Value = loan.PercentageOfCollateralCoverage;
                            worksheet.Cell(currentRow, 45).Value = overallCoveragePercentage;
                            worksheet.Cell(currentRow, 46).Value = coverageStatus;

                            for (int col = 1; col <= 46; col++)
                            {
                                worksheet.Cell(currentRow, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                                worksheet.Cell(currentRow, col).Style.Border.OutsideBorderColor = XLColor.Black;
                                worksheet.Cell(currentRow, col).Style.Font.FontName = "Bahnschrift Light";
                                worksheet.Cell(currentRow, col).Style.NumberFormat.Format = "#,##0.0";
                            }

                            // Subtotal accumulation
                            groupLoanAmount += loan.LoanAmount;
                            groupPaid += loan.Paid;
                            groupBalance += loan.Balance;
                            groupDueAmount += loan.DueAmount;
                            groupInterestForecasted += loan.InterestForcasted;
                            groupAccrualInterest += loan.AccrualInterest;
                            groupVAT += loan.Tax;
                            groupPenalty += loan.Penalty;


                            // Also accumulate for branch
                            branchLoanAmount += loan.LoanAmount;
                            branchPaid += loan.Paid;
                            branchBalance += loan.Balance;
                            branchDueAmount += loan.DueAmount;

                            currentRow++;
                        }

                        // Delinquent group subtotal row
                        worksheet.Cell(currentRow, 1).Value = "Subtotal:";
                        worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                        worksheet.Cell(currentRow, 5).Value = groupLoanAmount;
                        worksheet.Cell(currentRow, 7).Value = groupInterestForecasted;
                        worksheet.Cell(currentRow, 8).Value = groupAccrualInterest;
                        worksheet.Cell(currentRow, 10).Value = groupVAT;
                        worksheet.Cell(currentRow, 12).Value = groupPenalty;
                        worksheet.Cell(currentRow, 13).Value = groupPaid;
                        worksheet.Cell(currentRow, 14).Value = groupBalance;
                        worksheet.Cell(currentRow, 17).Value = groupDueAmount;
                        var summaryCols = new[] { 5, 7, 8, 10, 12, 13, 14, 17 };
                        foreach (var col in summaryCols)
                        {
                            worksheet.Cell(currentRow, col).Style.NumberFormat.Format = "#,##0.0";
                            worksheet.Cell(currentRow, col).Style.Font.Bold = true;
                            worksheet.Cell(currentRow, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            worksheet.Cell(currentRow, col).Style.Border.OutsideBorderColor = XLColor.Black;
                            worksheet.Cell(currentRow, col).Style.Font.FontName = "Bahnschrift Light";
                        }


                        for (int col = 1; col <= 46; col++)
                        {
                            worksheet.Cell(currentRow, col).Style.Font.Bold = true;
                            worksheet.Cell(currentRow, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            worksheet.Cell(currentRow, col).Style.Font.FontName = "Bahnschrift Light";
                            worksheet.Cell(currentRow, col).Style.NumberFormat.Format = "#,##0.0";
                        }

                        currentRow++;
                    }

                    // Branch total row
                    worksheet.Cell(currentRow, 1).Value = "Branch Total:";
                    worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                    worksheet.Cell(currentRow, 5).Value = branchLoanAmount;
                    worksheet.Cell(currentRow, 13).Value = branchPaid;
                    worksheet.Cell(currentRow, 14).Value = branchBalance;
                    worksheet.Cell(currentRow, 17).Value = branchDueAmount;

                    for (int col = 1; col <= 46; col++)
                    {
                        worksheet.Cell(currentRow, col).Style.Font.Bold = true;
                        worksheet.Cell(currentRow, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(currentRow, col).Style.Font.FontName = "Bahnschrift Light";
                        worksheet.Cell(currentRow, col).Style.NumberFormat.Format = "#,##0.0";
                    }

                    currentRow += 2;
                }
                // Move a few rows down after the last branch section
                currentRow += 1;

                // Title for Branch Performance
                worksheet.Cell(currentRow, 1).Value = $"BRANCH PERFORMANCE. [PERIOD: {period}]";
                worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = XLColor.DarkBlue;
                worksheet.Cell(currentRow, 1).Style.Font.FontColor = XLColor.White;
                worksheet.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(currentRow, 1).Style.Font.FontName = "Bahnschrift Light";
                worksheet.Range(currentRow, 1, currentRow, 2).Merge();
                currentRow++;

                // Header Row
                worksheet.Cell(currentRow, 1).Value = "BRANCHES";
                worksheet.Cell(currentRow, 2).Value = "NUMBER OF LOANS";

                worksheet.Range(currentRow, 1, currentRow, 2).Style.Fill.BackgroundColor = XLColor.LightGray;
                worksheet.Range(currentRow, 1, currentRow, 2).Style.Font.Bold = true;
                worksheet.Range(currentRow, 1, currentRow, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                worksheet.Range(currentRow, 1, currentRow, 2).Style.Font.FontName = "Bahnschrift Light";
                currentRow++;

                // Generate Branch Performance Data
                var branchPerformance = loanDetails
                    .GroupBy(l => new { l.BranchCode, l.BranchName })
                    .Select(g => new
                    {
                        Branch = $"{g.Key.BranchName} [{g.Key.BranchCode}]",
                        Count = g.Count()
                    })
                    .OrderByDescending(x => x.Count)
                    .ToList();

                // Populate Branch Performance Table
                foreach (var item in branchPerformance)
                {
                    worksheet.Cell(currentRow, 1).Value = item.Branch;
                    worksheet.Cell(currentRow, 2).Value = item.Count;

                    worksheet.Range(currentRow, 1, currentRow, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Range(currentRow, 1, currentRow, 2).Style.Font.FontName = "Bahnschrift Light";

                    currentRow++;
                }

                //        // Adjust column widths
                worksheet.Columns().AdjustToContents();
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return new ExportFileResult
                    {
                        Content = stream.ToArray(),
                        ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        FileName = $"{fileTitle.Replace(" ", "_")}_{(branch?.BranchCode?.Replace(" ", "_") ?? "All_Branches")}_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
                    };
                }
            }
        }




        //public static ExportFileResult GenerateLoanExcel(List<Loan> loanDetails, string branchName, string exportedBy, string fileTitle, string dateFrom, string dateTo)
        //{
        //    using (var workbook = new XLWorkbook())
        //    {
        //        var worksheet = workbook.Worksheets.Add("Loan Analysis");

        //        // Title with File Title
        //        worksheet.Cell(1, 1).Value = $"{fileTitle.ToUpper()} - LOAN ANALYSIS FOR {branchName.ToUpper()}";
        //        worksheet.Cell(1, 1).Style.Font.Bold = true;
        //        worksheet.Cell(1, 1).Style.Font.FontSize = 14;
        //        worksheet.Range(1, 1, 1, 45).Merge();
        //        worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        //        // Export Details with Date Range
        //        worksheet.Cell(2, 1).Value = $"Date Range: {dateFrom} - {dateTo}";
        //        worksheet.Cell(2, 1).Style.Font.Italic = true;
        //        worksheet.Cell(2, 1).Style.Font.FontSize = 10;
        //        worksheet.Range(2, 1, 2, 45).Merge();
        //        worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        //        worksheet.Cell(3, 1).Value = $"Export Date: {DateTime.Now} BY {exportedBy}";
        //        worksheet.Cell(3, 1).Style.Font.Italic = true;
        //        worksheet.Cell(3, 1).Style.Font.FontSize = 10;
        //        worksheet.Range(3, 1, 3, 45).Merge();
        //        worksheet.Cell(3, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        //        // Headers
        //        var headers = new[]
        //        {
        //    "Loan Reference", "Customer Name", "Customer Reference", "Loan Date", "Loan Amount", "Interest Rate (%)",
        //    "InterestForcasted", "Accrual Interest", "LastInterestCalculatedDate", "VAT Interest", "VAT", "Penalty", "Paid",
        //    "Balance", "LoanDuration", "MaturityDate", "Due Amount", "AccountNumber", "BranchCode", "LastPayment",
        //    "LastRefundDate", "LoanManager", "AdvancedPaymentDays", "AdvancedPaymentAmount", "DeliquentDays",
        //    "DeliquentInterest", "DeliquentAmount", "LastDeliquecyProcessedDate", "DeliquentStatus", "LoanType",
        //    "InterestAmountUpfront", "Savings", "OShares", "PShares", "Deposit", "Salary", "Shortee", "Co_Obligor",
        //    "Co_OperationGurantor", "OtherGuaranteeFund", "TotalFundGuranteed", "PercentageOfLiquidityCoverage",
        //    "PercentageOfCollateralCoverage", "PercentageOfOverAllCoverage", "Coverage Status"
        //};

        //        for (int i = 0; i < headers.Length; i++)
        //        {
        //            worksheet.Cell(5, i + 1).Value = headers[i];
        //            worksheet.Cell(5, i + 1).Style.Font.Bold = true;
        //            worksheet.Cell(5, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
        //            worksheet.Cell(5, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        //            worksheet.Cell(5, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        //            worksheet.Cell(5, i + 1).Style.Border.OutsideBorderColor = XLColor.Black;
        //        }

        //        // Data
        //        int currentRow = 6;
        //        foreach (var loan in loanDetails)
        //        {
        //            decimal totalCoveredAmount = loan.Savings + loan.OShares + loan.PShares + loan.Deposit + loan.Salary +
        //                                         loan.Shortee + loan.Co_Obligor + loan.Co_OperationGurantor + loan.OtherGuaranteeFund;

        //            decimal overallCoveragePercentage = (loan.DueAmount > 0) ? (totalCoveredAmount / loan.DueAmount) * 100 : 0;
        //            string coverageStatus = overallCoveragePercentage >= 100 ? "Covered" : "Not Covered";

        //            worksheet.Cell(currentRow, 1).Value = loan.Id;
        //            worksheet.Cell(currentRow, 2).Value = loan.CustomerName;
        //            worksheet.Cell(currentRow, 3).Value = loan.CustomerId;
        //            worksheet.Cell(currentRow, 4).Value = loan.LoanDate.ToString("dd/MM/yyyy, hh:mm:ss");
        //            worksheet.Cell(currentRow, 5).Value = loan.LoanAmount;
        //            worksheet.Cell(currentRow, 6).Value = loan.InterestRate;
        //            worksheet.Cell(currentRow, 7).Value = loan.InterestForcasted;
        //            worksheet.Cell(currentRow, 8).Value = loan.AccrualInterest;
        //            worksheet.Cell(currentRow, 9).Value = loan.LastInterestCalculatedDate.ToString("dd/MM/yyyy, hh:mm:ss");
        //            worksheet.Cell(currentRow, 10).Value = loan.Tax;
        //            worksheet.Cell(currentRow, 11).Value = loan.VatRate;
        //            worksheet.Cell(currentRow, 12).Value = loan.Penalty;
        //            worksheet.Cell(currentRow, 13).Value = loan.Paid;
        //            worksheet.Cell(currentRow, 14).Value = loan.Balance;
        //            worksheet.Cell(currentRow, 15).Value = loan.LoanDuration;
        //            worksheet.Cell(currentRow, 16).Value = loan.MaturityDate.ToString("dd/MM/yyyy, hh:mm:ss");
        //            worksheet.Cell(currentRow, 17).Value = loan.DueAmount;
        //            worksheet.Cell(currentRow, 18).Value = loan.AccountNumber;
        //            worksheet.Cell(currentRow, 19).Value = loan.BranchCode;
        //            worksheet.Cell(currentRow, 20).Value = loan.LastPayment;
        //            worksheet.Cell(currentRow, 21).Value = loan.LastRefundDate.ToString("dd/MM/yyyy, hh:mm:ss");
        //            worksheet.Cell(currentRow, 22).Value = loan.LoanManager;
        //            worksheet.Cell(currentRow, 23).Value = loan.AdvancedPaymentDays;
        //            worksheet.Cell(currentRow, 24).Value = loan.AdvancedPaymentAmount;
        //            worksheet.Cell(currentRow, 25).Value = loan.DeliquentDays;
        //            worksheet.Cell(currentRow, 26).Value = loan.DeliquentInterest;
        //            worksheet.Cell(currentRow, 27).Value = loan.DeliquentAmount;
        //            worksheet.Cell(currentRow, 28).Value = loan.LastDeliquecyProcessedDate?.ToString("dd/MM/yyyy, hh:mm:ss");
        //            worksheet.Cell(currentRow, 29).Value = loan.DeliquentStatus;
        //            worksheet.Cell(currentRow, 30).Value = loan.LoanType;
        //            worksheet.Cell(currentRow, 31).Value = loan.InterestAmountUpfront;
        //            worksheet.Cell(currentRow, 32).Value = loan.Savings;
        //            worksheet.Cell(currentRow, 33).Value = loan.OShares;
        //            worksheet.Cell(currentRow, 34).Value = loan.PShares;
        //            worksheet.Cell(currentRow, 35).Value = loan.Deposit;
        //            worksheet.Cell(currentRow, 36).Value = loan.Salary;
        //            worksheet.Cell(currentRow, 37).Value = loan.Shortee;
        //            worksheet.Cell(currentRow, 38).Value = loan.Co_Obligor;
        //            worksheet.Cell(currentRow, 39).Value = loan.Co_OperationGurantor;
        //            worksheet.Cell(currentRow, 40).Value = loan.OtherGuaranteeFund;
        //            worksheet.Cell(currentRow, 41).Value = totalCoveredAmount;
        //            worksheet.Cell(currentRow, 42).Value = loan.PercentageOfLiquidityCoverage;
        //            worksheet.Cell(currentRow, 43).Value = loan.PercentageOfCollateralCoverage;
        //            worksheet.Cell(currentRow, 44).Value = overallCoveragePercentage;
        //            worksheet.Cell(currentRow, 45).Value = coverageStatus;

        //            // Apply styles for current row
        //            for (int col = 1; col <= 45; col++)
        //            {
        //                worksheet.Cell(currentRow, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        //                worksheet.Cell(currentRow, col).Style.Border.OutsideBorderColor = XLColor.Black;
        //            }

        //            // Format currency and percentage cells
        //            worksheet.Cell(currentRow, 5).Style.NumberFormat.Format = "#,##0.0";  // Loan Amount
        //            worksheet.Cell(currentRow, 6).Style.NumberFormat.Format = "0.0";    // Interest Rate
        //            worksheet.Cell(currentRow, 17).Style.NumberFormat.Format = "#,##0.0"; // Due Amount
        //            worksheet.Cell(currentRow, 44).Style.NumberFormat.Format = "0.0";    // Overall Coverage

        //            currentRow++;
        //        }

        //        // Adjust column widths
        //        worksheet.Columns().AdjustToContents();

        //        // Save the file to memory
        //        using (var stream = new MemoryStream())
        //        {
        //            workbook.SaveAs(stream);
        //            return new ExportFileResult
        //            {
        //                Content = stream.ToArray(),
        //                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        //                FileName = $"{fileTitle.Replace(" ", "_")}_LoanAnalysis_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
        //            };
        //        }
        //    }
        //}


        public class ExportFileResult
        {
            public byte[] Content { get; set; }
            public string ContentType { get; set; }
            public string FileName { get; set; }
        }
    }

}