using BusinessServices;
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
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.IO;
    using OfficeOpenXml;
    using OfficeOpenXml.Style;

    public class LoanExcelGenerator : BaseService
    {
        // -----------------------------------------------------------------------
        // MAIN GENERATION METHOD
        // -----------------------------------------------------------------------
        public void GenerateLoanExcel(
            List<Loan> loanDetails,
            string filePath,
            string exportedBy,
            string fileTitle,
            GetLoansDataTableQuery tableQuery)
        {
            string bank = GetBankName();
            string branchCode = GetBranchCode();
            string branchId = GetBranchID();
            string branchName = GetBranchName();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                // ===== SHEET 1: SUMMARY & OVERVIEW =====
                CreateSummarySheet(package, loanDetails, bank, branchCode, branchId, branchName,
                    exportedBy, fileTitle, tableQuery);

                // ===== SHEET 2: BRANCH SUMMARY =====
                CreateBranchSummarySheet(package, loanDetails, exportedBy, fileTitle, tableQuery);

                // ===== SHEET 3: LOAN DETAILS =====
                CreateLoanDetailsSheet(package, loanDetails, exportedBy, fileTitle, tableQuery);

                // ===== SHEETS FOR EACH BRANCH =====
                var branches = loanDetails
                    .GroupBy(x => new { x.BranchCode, x.BranchName })
                    .Select(g => new { BranchCode = g.Key.BranchCode, BranchName = g.Key.BranchName })
                    .ToList();

                foreach (var branch in branches)
                {
                    var branchData = loanDetails
                        .Where(x => x.BranchCode == branch.BranchCode && x.BranchName == branch.BranchName)
                        .ToList();
                    CreateBranchSheet(package, branchData, branch.BranchCode, branch.BranchName,
                        exportedBy, fileTitle, tableQuery);
                }

                package.SaveAs(new FileInfo(filePath));
            }
        }

        // -----------------------------------------------------------------------
        // SHEET: SUMMARY & OVERVIEW
        // -----------------------------------------------------------------------
        private void CreateSummarySheet(
            ExcelPackage package,
            List<Loan> loanDetails,
            string bank,
            string branchCode,
            string branchId,
            string branchName,
            string exportedBy,
            string fileTitle,
            GetLoansDataTableQuery tableQuery)
        {
            var worksheet = package.Workbook.Worksheets.Add("Summary & Overview");
            ApplyDefaultStyle(worksheet);

            int headerColumns = 3;
            string headerEndColumn = GetColumnLetter(headerColumns);
            int currentRow = CreateHeaderSection(worksheet, bank, branchCode, branchId, branchName,
                exportedBy, fileTitle, tableQuery, headerEndColumn);

            // ----- GENERAL SUMMARY -----
            worksheet.Cells[$"A{currentRow}:{headerEndColumn}{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = "GENERAL SUMMARY";
            SetSectionHeaderStyle(worksheet.Cells[$"A{currentRow}"], Color.LightBlue);
            currentRow += 2;

            int totalLoans = loanDetails.Count;
            int totalBranches = loanDetails.Select(x => x.BranchCode).Distinct().Count();
            decimal totalLoanVolume = loanDetails.Sum(l => l.LoanAmount);
            decimal totalRepaid = loanDetails.Sum(l => l.Paid);
            decimal totalBalance = loanDetails.Sum(l => l.Balance);
            decimal totalDue = loanDetails.Sum(l => l.DueAmount);
            decimal totalAccrual = loanDetails.Sum(l => l.AccrualInterest);
            decimal totalDelinquent = loanDetails.Sum(l => l.DeliquentInterest);
            decimal percentageRepaid = totalLoanVolume > 0 ? (totalRepaid / totalLoanVolume) * 100 : 0;

            var summaryData = new[]
            {
            new { Metric = "Total Loans (Count)", Value = totalLoans.ToString("N0"), Description = "Number of loan records" },
            new { Metric = "Total Branches", Value = totalBranches.ToString("N0"), Description = "Branches with loan activity" },
            new { Metric = "Total Loan Volume", Value = totalLoanVolume.ToString("N2"), Description = "Sum of all loan amounts" },
            new { Metric = "Total Repaid", Value = totalRepaid.ToString("N2"), Description = "Amount already paid" },
            new { Metric = "Outstanding Balance", Value = totalBalance.ToString("N2"), Description = "Current principal balance" },
            new { Metric = "Total Due Amount", Value = totalDue.ToString("N2"), Description = "Total amount due (principal + interest)" },
            new { Metric = "Total Accrual Interest", Value = totalAccrual.ToString("N2"), Description = "Accrued interest" },
            new { Metric = "Total Delinquent Interest", Value = totalDelinquent.ToString("N2"), Description = "Delinquent interest" },
            new { Metric = "Repayment Rate", Value = percentageRepaid.ToString("0.00") + "%", Description = "Percentage of loan volume repaid" }
        };

            currentRow = CreateTwoColumnTable(worksheet, currentRow, summaryData, "Metric", "Value", "Description");
            currentRow += 2;

            // ----- DELINQUENT STATUS DISTRIBUTION -----
            var statusDist = loanDetails
                .GroupBy(x => x.DeliquentStatus ?? "Unknown")
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            currentRow = CreateDistributionSection(worksheet, currentRow, headerEndColumn,
                "DELINQUENT STATUS DISTRIBUTION", statusDist, totalLoans, Color.LightYellow);
            currentRow += 2;

            // ----- LOAN TYPE DISTRIBUTION -----
            var loanTypeDist = loanDetails
                .Where(x => !string.IsNullOrEmpty(x.LoanType))
                .GroupBy(x => x.LoanType)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            if (loanTypeDist.Any())
            {
                currentRow = CreateDistributionSection(worksheet, currentRow, headerEndColumn,
                    "LOAN TYPE DISTRIBUTION", loanTypeDist, totalLoans, Color.LightGreen);
            }

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        }

        // -----------------------------------------------------------------------
        // SHEET: BRANCH SUMMARY
        // -----------------------------------------------------------------------
        private void CreateBranchSummarySheet(
            ExcelPackage package,
            List<Loan> loanDetails,
            string exportedBy,
            string fileTitle,
            GetLoansDataTableQuery tableQuery)
        {
            var worksheet = package.Workbook.Worksheets.Add("Branch Summary");
            ApplyDefaultStyle(worksheet);

            int headerColumns = 8;
            string headerEndColumn = GetColumnLetter(headerColumns);
            int currentRow = CreateHeaderSection(worksheet, GetBankName(), GetBranchCode(), GetBranchID(), GetBranchName(),
                exportedBy, fileTitle, tableQuery, headerEndColumn);

            worksheet.Cells[$"A{currentRow}:{headerEndColumn}{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = "BRANCH SUMMARY";
            SetSectionHeaderStyle(worksheet.Cells[$"A{currentRow}"], Color.LightCoral);
            currentRow += 2;

            var branchHeaders = new[] { "Branch Code", "Branch Name", "# Loans", "Volume", "Repaid", "Balance", "Due Amount", "Avg Loan" };
            for (int i = 0; i < branchHeaders.Length; i++)
            {
                worksheet.Cells[currentRow, i + 1].Value = branchHeaders[i];
                SetHeaderStyle(worksheet.Cells[currentRow, i + 1], Color.LightGreen);
            }
            currentRow++;

            var branchSummary = loanDetails
                .GroupBy(x => new { x.BranchCode, x.BranchName })
                .Select(g => new
                {
                    BranchCode = g.Key.BranchCode,
                    BranchName = g.Key.BranchName ?? "Unknown",
                    LoanCount = g.Count(),
                    Volume = g.Sum(x => x.LoanAmount),
                    Repaid = g.Sum(x => x.Paid),
                    Balance = g.Sum(x => x.Balance),
                    Due = g.Sum(x => x.DueAmount),
                    AvgLoan = g.Average(x => x.LoanAmount)
                })
                .OrderByDescending(x => x.Volume)
                .ToList();

            foreach (var b in branchSummary)
            {
                worksheet.Cells[currentRow, 1].Value = b.BranchCode;
                worksheet.Cells[currentRow, 2].Value = b.BranchName;
                worksheet.Cells[currentRow, 3].Value = b.LoanCount;
                worksheet.Cells[currentRow, 4].Value = b.Volume;
                worksheet.Cells[currentRow, 5].Value = b.Repaid;
                worksheet.Cells[currentRow, 6].Value = b.Balance;
                worksheet.Cells[currentRow, 7].Value = b.Due;
                worksheet.Cells[currentRow, 8].Value = b.AvgLoan;

                for (int col = 1; col <= headerColumns; col++)
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                currentRow++;
            }

            // Totals row
            if (branchSummary.Any())
            {
                worksheet.Cells[currentRow, 1].Value = "TOTALS / AVERAGES:";
                worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
                worksheet.Cells[currentRow, 3].Value = branchSummary.Sum(x => x.LoanCount);
                worksheet.Cells[currentRow, 4].Value = branchSummary.Sum(x => x.Volume);
                worksheet.Cells[currentRow, 5].Value = branchSummary.Sum(x => x.Repaid);
                worksheet.Cells[currentRow, 6].Value = branchSummary.Sum(x => x.Balance);
                worksheet.Cells[currentRow, 7].Value = branchSummary.Sum(x => x.Due);
                worksheet.Cells[currentRow, 8].Value = branchSummary.Average(x => x.AvgLoan);

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
        // SHEET: LOAN DETAILS (ALL ENTRIES)
        // -----------------------------------------------------------------------
        private void CreateLoanDetailsSheet(
            ExcelPackage package,
            List<Loan> loanDetails,
            string exportedBy,
            string fileTitle,
            GetLoansDataTableQuery tableQuery)
        {
            var worksheet = package.Workbook.Worksheets.Add("Loan Details");
            ApplyDefaultStyle(worksheet);

            int headerColumns = 47;
            string headerEndColumn = GetColumnLetter(headerColumns);
            int currentRow = CreateHeaderSection(worksheet, GetBankName(), GetBranchCode(), GetBranchID(), GetBranchName(),
                exportedBy, fileTitle, tableQuery, headerEndColumn);

            // ----- DELINQUENT STATUS OVERVIEW -----
            var statusOverview = loanDetails
                .GroupBy(x => x.DeliquentStatus ?? "Unknown")
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            currentRow = CreateDistributionSection(worksheet, currentRow, headerEndColumn,
                "DELINQUENT STATUS OVERVIEW", statusOverview, loanDetails.Count, Color.LightYellow);
            currentRow += 2;

            worksheet.Cells[$"A{currentRow}:{headerEndColumn}{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = "DETAILED LOAN ENTRIES";
            SetSectionHeaderStyle(worksheet.Cells[$"A{currentRow}"], Color.LightCoral);
            currentRow += 2;

            var headers = new[]
            {
            "SN", "Loan Ref", "Member Name", "Member Reference", "Loan Date", "Loan Amount", "Interest Rate",
            "Interest Forecast", "Accrual Interest", "Last Interest Calc", "VAT Amount", "VAT Rate",
            "Penalty", "Paid", "Balance", "Duration (M)", "Maturity Date", "Due Amount", "Account No",
            "Branch Code", "Last Payment", "Last Refund Date", "Loan Manager", "Adv Payment Days",
            "Adv Payment Amt", "Delinquent Days", "Delinquent Interest", "Delinquent Amt",
            "Last Delinquency Date", "Delinquent Status", "Loan Type", "Loan Category", "Interest Upfront",
            "Savings", "O-Shares", "P-Shares", "Deposit", "Salary", "Shortee", "Co-Obligor",
            "Co-Op Guarantor", "Other Guarantee", "Total Guaranteed", "Liquidity Cov %", "Collateral Cov %",
            "Overall Cov %", "Coverage Status"
        };

            int headerRow = currentRow;
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[headerRow, i + 1].Value = headers[i];
                SetHeaderStyle(worksheet.Cells[headerRow, i + 1], Color.LightGreen);
            }
            currentRow++;

            int sn = 1;
            foreach (var loan in loanDetails.OrderByDescending(x => x.LoanDate))
            {
                decimal totalCovered = loan.Savings + loan.OShares + loan.PShares + loan.Deposit + loan.Salary +
                                       loan.Shortee + loan.Co_Obligor + loan.Co_OperationGurantor + loan.OtherGuaranteeFund;
                decimal overallCoveragePct = loan.DueAmount > 0 ? (totalCovered / loan.DueAmount) * 100 : 0;
                string coverageStatus = overallCoveragePct >= 100 ? "Covered" : "Not Covered";

                worksheet.Cells[currentRow, 1].Value = sn++;
                worksheet.Cells[currentRow, 2].Value = loan.Id;
                worksheet.Cells[currentRow, 3].Value = loan.CustomerName;
                worksheet.Cells[currentRow, 4].Value = loan.CustomerId;
                worksheet.Cells[currentRow, 5].Value = loan.LoanDate.ToString("dd-MM-yyyy HH:mm:ss");
                worksheet.Cells[currentRow, 6].Value = loan.LoanAmount;
                worksheet.Cells[currentRow, 7].Value = loan.InterestRate;
                worksheet.Cells[currentRow, 8].Value = loan.InterestForcasted;
                worksheet.Cells[currentRow, 9].Value = loan.AccrualInterest;
                worksheet.Cells[currentRow, 10].Value = loan.LastInterestCalculatedDate?.ToString("dd-MM-yyyy HH:mm:ss") ?? "N/A";
                worksheet.Cells[currentRow, 11].Value = loan.Tax;
                worksheet.Cells[currentRow, 12].Value = loan.VatRate;
                worksheet.Cells[currentRow, 13].Value = loan.Penalty;
                worksheet.Cells[currentRow, 14].Value = loan.Paid;
                worksheet.Cells[currentRow, 15].Value = loan.Balance;
                worksheet.Cells[currentRow, 16].Value = loan.LoanDuration;
                worksheet.Cells[currentRow, 17].Value = loan.MaturityDate.ToString("dd-MM-yyyy HH:mm:ss");
                worksheet.Cells[currentRow, 18].Value = loan.DueAmount;
                worksheet.Cells[currentRow, 19].Value = loan.AccountNumber;
                worksheet.Cells[currentRow, 20].Value = loan.BranchCode;
                worksheet.Cells[currentRow, 21].Value = loan.LastPayment;
                worksheet.Cells[currentRow, 22].Value = loan.LastRefundDate.ToString("dd-MM-yyyy HH:mm:ss") ?? "N/A";
                worksheet.Cells[currentRow, 23].Value = loan.LoanManager;
                worksheet.Cells[currentRow, 24].Value = loan.AdvancedPaymentDays;
                worksheet.Cells[currentRow, 25].Value = loan.AdvancedPaymentAmount;
                worksheet.Cells[currentRow, 26].Value = loan.DeliquentDays;
                worksheet.Cells[currentRow, 27].Value = loan.DeliquentInterest;
                worksheet.Cells[currentRow, 28].Value = loan.DeliquentAmount;
                worksheet.Cells[currentRow, 29].Value = loan.LastDeliquecyProcessedDate?.ToString("dd-MM-yyyy HH:mm:ss") ?? "N/A";
                worksheet.Cells[currentRow, 30].Value = loan.DeliquentStatus;
                worksheet.Cells[currentRow, 31].Value = loan.LoanType;
                worksheet.Cells[currentRow, 32].Value = loan.LoanCategory;
                worksheet.Cells[currentRow, 33].Value = loan.InterestAmountUpfront;
                worksheet.Cells[currentRow, 34].Value = loan.Savings;
                worksheet.Cells[currentRow, 35].Value = loan.OShares;
                worksheet.Cells[currentRow, 36].Value = loan.PShares;
                worksheet.Cells[currentRow, 37].Value = loan.Deposit;
                worksheet.Cells[currentRow, 38].Value = loan.Salary;
                worksheet.Cells[currentRow, 39].Value = loan.Shortee;
                worksheet.Cells[currentRow, 40].Value = loan.Co_Obligor;
                worksheet.Cells[currentRow, 41].Value = loan.Co_OperationGurantor;
                worksheet.Cells[currentRow, 42].Value = loan.OtherGuaranteeFund;
                worksheet.Cells[currentRow, 43].Value = totalCovered;
                worksheet.Cells[currentRow, 44].Value = loan.PercentageOfLiquidityCoverage;
                worksheet.Cells[currentRow, 45].Value = loan.PercentageOfCollateralCoverage;
                worksheet.Cells[currentRow, 46].Value = overallCoveragePct;
                worksheet.Cells[currentRow, 47].Value = coverageStatus;

                for (int col = 1; col <= headerColumns; col++)
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                currentRow++;
            }

            // Format numeric columns
            worksheet.Cells[$"F{headerRow + 1}:F{currentRow - 1}"].Style.Numberformat.Format = "#,##0.00";
            worksheet.Cells[$"G{headerRow + 1}:G{currentRow - 1}"].Style.Numberformat.Format = "0.00";
            worksheet.Cells[$"H{headerRow + 1}:R{currentRow - 1}"].Style.Numberformat.Format = "#,##0.00";
            worksheet.Cells[$"AN{headerRow + 1}:AP{currentRow - 1}"].Style.Numberformat.Format = "0.00";

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            worksheet.View.FreezePanes(headerRow + 1, 1);
        }

        // -----------------------------------------------------------------------
        // SHEET: INDIVIDUAL BRANCH DETAILS
        // -----------------------------------------------------------------------
        private void CreateBranchSheet(
     ExcelPackage package,
     List<Loan> branchData,
     string branchCode,
     string branchName,
     string exportedBy,
     string fileTitle,
     GetLoansDataTableQuery tableQuery)
        {
            string sheetName = CleanSheetName($"{branchCode} - {branchName}");
            if (sheetName.Length > 31) sheetName = sheetName.Substring(0, 31);
            var worksheet = package.Workbook.Worksheets.Add(sheetName);
            ApplyDefaultStyle(worksheet);

            int headerColumns = 11; // number of data columns (excluding SN)
            string headerEndColumn = GetColumnLetter(headerColumns + 1); // +1 for SN
            int currentRow = CreateHeaderSection(worksheet, GetBankName(), branchCode, branchCode, branchName,
                exportedBy, fileTitle, tableQuery, headerEndColumn);

            // ===== DETAILED LOAN ENTRIES (now first) =====
            worksheet.Cells[$"A{currentRow}:{headerEndColumn}{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = $"DETAILED LOANS - {branchName}";
            SetSectionHeaderStyle(worksheet.Cells[$"A{currentRow}"], Color.LightCoral);
            currentRow += 2;

            var branchHeaders = new[]
            {
        "SN", "Loan Ref", "Member Name", "Loan Date", "Loan Amount", "Interest Rate",
        "Paid", "Balance", "Due Amount", "Status", "Loan Type"
    };

            int headerRow = currentRow;
            for (int i = 0; i < branchHeaders.Length; i++)
            {
                worksheet.Cells[headerRow, i + 1].Value = branchHeaders[i];
                SetHeaderStyle(worksheet.Cells[headerRow, i + 1], Color.LightGreen);
            }
            currentRow++;

            int sn = 1;
            foreach (var loan in branchData.OrderByDescending(l => l.LoanDate))
            {
                worksheet.Cells[currentRow, 1].Value = sn++;
                worksheet.Cells[currentRow, 2].Value = loan.Id;
                worksheet.Cells[currentRow, 3].Value = loan.CustomerName;   // Member Name
                worksheet.Cells[currentRow, 4].Value = loan.LoanDate.ToString("dd-MM-yyyy");
                worksheet.Cells[currentRow, 5].Value = loan.LoanAmount;
                worksheet.Cells[currentRow, 6].Value = loan.InterestRate;
                worksheet.Cells[currentRow, 7].Value = loan.Paid;
                worksheet.Cells[currentRow, 8].Value = loan.Balance;
                worksheet.Cells[currentRow, 9].Value = loan.DueAmount;
                worksheet.Cells[currentRow, 10].Value = loan.DeliquentStatus ?? "Current";
                worksheet.Cells[currentRow, 11].Value = loan.LoanType;

                for (int col = 1; col <= branchHeaders.Length; col++)
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                currentRow++;
            }

            currentRow += 2; // leave space before summary

            // ===== BRANCH SUMMARY (two‑column table) =====
            worksheet.Cells[$"A{currentRow}:{headerEndColumn}{currentRow}"].Merge = true;
            worksheet.Cells[$"A{currentRow}"].Value = $"BRANCH SUMMARY - {branchName}";
            SetSectionHeaderStyle(worksheet.Cells[$"A{currentRow}"], Color.LightBlue);
            currentRow += 2;

            int total = branchData.Count;
            decimal volume = branchData.Sum(l => l.LoanAmount);
            decimal repaid = branchData.Sum(l => l.Paid);
            decimal balance = branchData.Sum(l => l.Balance);
            decimal due = branchData.Sum(l => l.DueAmount);

            var summaryItems = new[]
            {
        new { Metric = "Total Loans", Value = total.ToString("N0") },
        new { Metric = "Loan Volume", Value = volume.ToString("N2") },
        new { Metric = "Repaid", Value = repaid.ToString("N2") },
        new { Metric = "Balance", Value = balance.ToString("N2") },
        new { Metric = "Due Amount", Value = due.ToString("N2") }
    };

            currentRow = CreateTwoColumnTable(worksheet, currentRow, summaryItems, "Metric", "Value");
            currentRow += 2;

            // ===== DELINQUENT STATUS DISTRIBUTION =====
            var statusDist = branchData
                .GroupBy(x => x.DeliquentStatus ?? "Unknown")
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToList();

            currentRow = CreateDistributionSection(worksheet, currentRow, headerEndColumn,
                "DELINQUENT STATUS DISTRIBUTION", statusDist, total, Color.LightYellow);

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            worksheet.View.FreezePanes(headerRow + 1, 1);
        }

        // -----------------------------------------------------------------------
        // HEADER SECTION (exactly as in the sample)
        // -----------------------------------------------------------------------
        private int CreateHeaderSection(
            ExcelWorksheet worksheet,
            string bank,
            string branchCode,
            string branchId,
            string branchName,
            string exportedBy,
            string fileTitle,
            GetLoansDataTableQuery tableQuery,
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
            worksheet.Cells["A5"].Value = $"{fileTitle.ToUpper()} - LOAN DETAILED REPORT";
            worksheet.Cells["A5"].Style.Font.Bold = true;
            worksheet.Cells["A5"].Style.Font.Size = 14;
            worksheet.Cells["A5"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells["A5"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Row(5).Height = 25;
            worksheet.Cells["A5"].Style.Font.Color.SetColor(Color.Black);
            worksheet.Cells["A5"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["A5"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 215, 0));

            // Date range from tableQuery
            if (tableQuery != null && tableQuery.StartDate.HasValue && tableQuery.EndDate.HasValue)
            {
                worksheet.Cells[$"A6:{headerEndColumn}6"].Merge = true;
                worksheet.Cells["A6"].Value = $"Date Range: {tableQuery.StartDate.Value:dd-MM-yyyy} to {tableQuery.EndDate.Value:dd-MM-yyyy}";
                worksheet.Cells["A6"].Style.Font.Bold = true;
                worksheet.Cells["A6"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                worksheet.Cells["A6"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Row(6).Height = 18;
                return 8; // content starts after header
            }

            return 7;
        }

        // -----------------------------------------------------------------------
        // STYLING HELPERS
        // -----------------------------------------------------------------------
        private void ApplyDefaultStyle(ExcelWorksheet worksheet)
        {
            worksheet.Cells.Style.Font.Name = "Bahnschrift SemiCondensed";
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

        // -----------------------------------------------------------------------
        // TABLE HELPERS
        // -----------------------------------------------------------------------
        private int CreateTwoColumnTable(
            ExcelWorksheet worksheet,
            int startRow,
            IEnumerable<dynamic> items,
            string col1Header,
            string col2Header,
            string col3Header = null)
        {
            int colCount = string.IsNullOrEmpty(col3Header) ? 2 : 3;
            int row = startRow;

            for (int i = 0; i < colCount; i++)
            {
                worksheet.Cells[row, i + 1].Value = i == 0 ? col1Header : i == 1 ? col2Header : col3Header;
                SetHeaderStyle(worksheet.Cells[row, i + 1], Color.LightGreen);
            }
            row++;

            foreach (var item in items)
            {
                worksheet.Cells[row, 1].Value = item.Metric;
                worksheet.Cells[row, 2].Value = item.Value;
                if (colCount == 3)
                    worksheet.Cells[row, 3].Value = item.Description;

                for (int col = 1; col <= colCount; col++)
                    worksheet.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                row++;
            }

            return row;
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
            foreach (var ch in invalidChars)
                name = name.Replace(ch, ' ');
            name = string.Join(" ", name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            return name.Trim();
        }
    }
}