using BusinessServices;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Session;
using CBS.FrontDesk.Data.Entity.Accounting_V2.DaillyCollectorCommission;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.CustomerManagement;
using CBS.FrontDesk.Data.Entity.ReportMembersFSeries;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
using DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.Spreadsheet;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.ReportingMembersFSeries
{
    public class FinancialReportBuilder : BaseService
    {
        private readonly FinancialReportService _reportService;
        private readonly CashDeskServices _CashDeskServicesService;

        public FinancialReportBuilder(FinancialReportService reportService, CashDeskServices cashDeskServices)
        {
            _reportService = reportService;
            _CashDeskServicesService = cashDeskServices;
        }

        #region helper methods for customer and date formatting
        public async Task<IndividualProfile> GetCustomer(string customerId)
        {
            if (string.IsNullOrWhiteSpace(customerId))
                throw new ArgumentException("Customer reference cannot be null or empty.", nameof(customerId));

            var customer = await _CashDeskServicesService.GetCustomer(customerId);
            if (customer == null)
                return null;

            return customer;
        }

        public async Task<Branch> GetBranchandbank()
        {
            Branch branch = _CashDeskServicesService.RetrieveBranchFromSession();

            if (branch == null)
                return null;

            return branch;
        }

        // Helper method to parse transaction date string
        public static class DateFormatter
        {
            public static string Format(DateTime date, string format)
            {
                return date.ToString(format, CultureInfo.InvariantCulture);
            }

            public static string Format(DateTime? date, string format)
            {
                return date.HasValue
                    ? date.Value.ToString(format, CultureInfo.InvariantCulture)
                    : string.Empty;
            }
        }

        public static class DateTimeHelper
        {
            public static DateTime ExtractDate(DateTime value)
                => value.Date;

            public static DateTime ExtractTime(DateTime value)
                => DateTime.Today.Add(value.TimeOfDay);
        }

        // to format decimal places 
        public static class NumberFormatter
        {
            public static decimal ToDecimalPlaces(decimal value, int decimalPlaces)
            {
                return Math.Round(value, decimalPlaces, MidpointRounding.AwayFromZero);
            }

            public static decimal? ToDecimalPlaces(decimal? value, int decimalPlaces)
            {
                if (!value.HasValue) return null;
                return Math.Round(value.Value, decimalPlaces, MidpointRounding.AwayFromZero);
            }

            // Optional: double support
            public static double ToDecimalPlaces(double value, int decimalPlaces)
            {
                return Math.Round(value, decimalPlaces, MidpointRounding.AwayFromZero);
            }
        }
        #endregion

        public async Task<List<TransactionStaement>> BuildAccountStatementRows(FinancialReportFilter parameters)
        {
            var bra = await GetBranchandbank();
            var cus = await GetCustomer(parameters.MemberReference);
            var accountStatement = await _reportService.GetCustomerTransactionsByAccountNumber(parameters);

            var rows = new List<TransactionStaement>();

            var openingBalance = accountStatement?.OpeningBalance ?? 0m;

            // ✅ Order transactions ONCE
            var transactions = (accountStatement?.Transactions ?? new List<TransactionRaw>())
                .OrderBy(x => x.AccountingDate)
                .ThenBy(x => x.TransactionReference)
                .ToList();

            // ✅ Generate logo once
            var logoPath = bra != null
                ? PaymentReceiptMapping.GenerateAndSaveBankLogoImage(bra.Bank?.LogoUrl, bra.Name)
                : "";

            // =========================
            // CASE 1: NO TRANSACTIONS
            // =========================
            if (!transactions.Any())
            {
                rows.Add(new TransactionStaement
                {
                    BranchName = GetBranchName(),
                    BranchCode = GetBranchCode(),
                    HeadOfficeName = GetBankName(),
                    AccountNumber = parameters.AccountId ?? parameters.AccountNumber,
                    Currreccy = "Central African CFA franc",
                    PrintedBy = GetUserFullName(),
                    PrintedOn = DateTime.Now.ToString("dd/MM/yyyy"),
                    Telephone = cus?.Phone ?? "-",

                    OpeningBalance = openingBalance.ToString("N1"),
                    Balance = openingBalance,
                    ClosingBalance = openingBalance,

                    TotalDebit = "0.0",
                    TotalCredit = "0.0",
                    TotalOperation = "0",
                    BalanceasOf = $"Balance as of {parameters.DateTo:dd/MM/yyyy}: {openingBalance:N1}"
                });

                return rows;
            }

            // =========================
            // CASE 2: TRANSACTIONS EXIST
            // =========================
            decimal runningBalance = openingBalance;
            decimal totalDebit = 0m;
            decimal totalCredit = 0m;
      

            foreach (var t in transactions)
            {
                var balanceBefore = runningBalance;

                runningBalance += t.Credit - t.Debit;
                totalDebit += t.Debit;
                totalCredit += t.Credit;

                rows.Add(new TransactionStaement
                {
                    // -------- Account / Customer --------
                    AccountNumber = parameters.AccountId ?? t.AccountNumber,
                    AccountName = t.AccountType ?? "-",
                    CustomerId = cus?.CustomerId ?? "-",
                    CustomerName = cus != null ? $"{cus.FirstName} {cus.LastName}" : "-",
                    Telephone = cus?.Phone ?? "-",
                    Currreccy = "Central African CFA franc",
                    Village = cus?.town ?? "",
                    BranchAddress = bra.Address,
                    BranchTelephone = bra.Telephone,

                    // -------- Branch --------
                    BranchName = GetBranchName(),
                    BranchCode = GetBranchCode(),
                    HeadOfficeName = GetBankName(),

                    // -------- Transaction --------
                    Date = t.AccountingDate,
                    Time = t.AccountingDate,
                    PrintedOn = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                    Description = t.Operation,
                    Reference = t.TransactionReference,
                    Representative = string.IsNullOrWhiteSpace(t.DepositorName) ? "-" : t.DepositorName,
                    DepositorName = string.IsNullOrWhiteSpace(t.DepositorName) ? "-" : t.DepositorName,

                    // -------- Amounts --------
                    Debit = t.Debit,
                    Credit = t.Credit,
                    OpeningBalance = balanceBefore.ToString("N1"),
                    Balance = runningBalance,

                    // -------- Totals (CONSISTENT) --------
                    TotalDebit = totalDebit.ToString("N1"),
                    TotalCredit = totalCredit.ToString("N1"),
                    TotalOperation = transactions.Count.ToString(),
                    ClosingBalance = accountStatement.summary.ClosingBalance,

                    BalanceasOf = $"Balance as of {parameters.DateTo:dd/MM/yyyy}: {runningBalance:N1}",
                    
                    // -------- Report --------
                    Printedfrom = parameters.DateFrom.ToString("dd/MM/yyyy"),
                    PrintedTo = parameters.DateTo.ToString("dd/MM/yyyy"),
                    Year = $"2021 - {DateTime.Now.Year}",
                    PrintedBy = GetUserFullName(),
                    Address = cus?.Address ?? "",
                    HeadOfficeAddress = bra?.Address ?? "",
                    Logo = logoPath
                });
            }

            return rows;
        }

        // 2. Build Member Situation Rows
        public async Task<MemberSituationMainRpt> BuildMemberSituationRows(FinancialReportFilter filter)
        {
            var backend = await _reportService.GetMemberSituationRaw(filter);
            var bra = await GetBranchandbank();
            var cus = await GetCustomer(filter.MemberReference);

            // ✅ Generate logo once
            var logoPath = bra != null
                ? PaymentReceiptMapping.GenerateAndSaveBankLogoImage(bra.Bank?.LogoUrl, bra.Name)
                : "";


            var situation = backend.MemberSituation;
            var firstLoan = situation.Loans?.FirstOrDefault();

            return new MemberSituationMainRpt
            {
                // ===============================
                // INSTITUTION / HEADER
                // ===============================

                BranchName = GetBranchName(),
                BranchCode = GetBranchCode(),
                HeadOfficeName = GetBankName(),
                BranchAddress = bra.Address,
                BranchTelephone = bra.Telephone,
                Year = $"2021 - {DateTime.Now.Year}",
                CustomerId = cus.CustomerId,

                Logo = logoPath,

                CustomerName = cus != null ? $"{cus.FirstName} {cus.LastName}" : "-",
                CNI = cus.IDNumber ?? "-",
                Telephone = cus.Phone ?? "-",
                PrintedBy = GetUserFullName(),

                TotalBalance = backend.MemberSituation.Summary.TotalBalance,
                TotalBlockedAmount = backend.MemberSituation.Summary.TotalBlockedAmount,
                TotalLiquidSavings = backend.MemberSituation.Summary.TotalLiquidSavings,
                Actual = backend.MemberSituation.Summary.Actual,
                LoanAndCoverageGapAmount = backend.MemberSituation.Summary.LoanAndCoverageGapAmount,
                TotalLoanBalance = backend.MemberSituation.Summary.TotalLoanBalance,
                LoanCount = backend.MemberSituation.Summary.LoansCount.ToString(),
                TotalPaid = backend.MemberSituation.Summary.TotalPaid,
                SavingsAgainstLoanRatio = backend.MemberSituation.Summary.SavingsAgainstLoanRatio,
                LoanRecommendation = backend.MemberSituation.Summary.LoanRecommendation,
                LoanRiskLevel = backend.MemberSituation.Summary.LoanRiskLevel,

                // ===============================
                // SUBREPORT 1 – ACCOUNTS
                // ===============================
                AccountSituations = situation.Accounts
                    .Select(a => new AccountSnapshot
                    {
                        AccountId = a.AccountId,
                        AccountNumber = a.AccountNumber,
                        AccountType = a.AccountType,
                        Balance = a.Balance,
                        BlockedAmount = a.BlockedAmount,
                        ActualBalance = a.ActualBalance,
                        LastTransactionDate = a.SnapshotDate,
                        LastTransactedAmount = a.lastTransactedAmount
                    })
                    .ToList(),

                // ===============================
                // SUBREPORT 2 – LOANS
                // ===============================
                LoanHistories = situation.Loans
                    .Select(l => new LoanSituationRow
                    {
                        LoanAccount = l.LoanAccount,
                        LoanType = l.LoanType,
                        LoanBalance = l.Balance,
                        Interest = l.Interest,
                        InterestRate = l.InterestRate,
                        LastRepaymentDate = l.LastRepaymentDate,
                        LoanDate = l.LoanDate,
                        DisbursementDate = l.DisbursementDate,
                        NumberOfInstallment = l.DelDays,
                        LoanAmount = l.Principal,
                        LoanRepaymentAmount = l.LastRepaymentAmount,
                        DelInterest = l.DelInterest,
                        DeliquenceAmount = l.DelAmount,
                        Deliquencedays = l.DelDays
                    })
                    .ToList()
            };
        }


        // 3. Build Loan Repayment Rows
        public async Task<List<LoanRepaymentRow>> BuildLoanRepaymentRows(FinancialReportFilter parameters)
        {
            var repaymentData = await _reportService.GetLoanRepaymentData(parameters);
            var bra = await GetBranchandbank();
            var cus = await GetCustomer(parameters.MemberReference);


            var rows = new List<LoanRepaymentRow>();


            // ✅ Generate logo once
            var logoPath = bra != null
                ? PaymentReceiptMapping.GenerateAndSaveBankLogoImage(bra.Bank?.LogoUrl, bra.Name)
                : "";

            // =========================
            // REPAYMENT TRANSACTIONS
            // =========================
            if (repaymentData.RepaymentLines != null)
            {
                foreach (var trx in repaymentData.RepaymentLines)
                {
                    rows.Add(new LoanRepaymentRow
                    {
                        Logo = logoPath,
                        ReportTitle = "LOAN REPAYMENT",
                        BankName = GetBankName(),
                        BranchName = GetBranchName(),
                        BranchCode = GetBankCode(),
                        BranchAddress = bra.Address,
                        BranchTellephone = bra.Telephone,
                        MembersName = cus != null ? $"{cus.FirstName} {cus.LastName}" : "-",
                        MembersReference   = cus?.CustomerId ?? "-",
                        Phone = cus?.Phone ?? "-",
                        Address = cus?.Address ?? "-",
                        PeriodFrom = parameters.DateFrom,
                        PeriodTo = parameters.DateTo,
                        PrintedBy = GetUserFullName(),
                        PrintedOn = DateTime.Now,
                        Id = trx.Id,
                        Comment = trx.Comment,
                        PaymentChannel = trx.PaymentChannel,
                        PaymentMethod = trx.PaymentMethod,
                        Tax = trx.Tax,
                
                        LoanId = trx.LoanId,
                        LoanAccountNumber = trx.LoanAccountNumber,
                        LoanType = trx.LoanType,
                        LoanAmount = trx.LoanAmount,
                        //  LoanAccountNumber = trx.AccountNumber,

                        Date = trx.DateOfPayment.ToString("dd/MM/yyyy HH:mm:ss"),

                        DateOfPayment = trx.DateOfPayment,
                        Principal = trx.Principal,
                        Interest = trx.Interest,
                        Penalty = trx.Penalty,
                        Balance = trx.Balance,
                        Amount = trx.Amount,
                        Year = $"2021 - {DateTime.Now.Year}",
                        Status = trx.completeStatus,
                        TotalPrincipal = repaymentData.Summary.TotalPrincipal,
                        TotalInterest = repaymentData.Summary.TotalInterest,
                        TotalPenalty = repaymentData.Summary.TotalPenalty,
                        TotalPaid = repaymentData.Summary.TotalPaid,
                        TotalVat = repaymentData.Summary.TotalVat
                    });
                }
            }          
            return rows;
        }


        // 4. Build Loan Situation Rows
        public async Task<List<LoanSituationRow>> BuildLoanSituationRows(FinancialReportFilter parameters)
        {
            // Get branch and customer info for headers
            var bra = await GetBranchandbank();
            var cus = await GetCustomer(parameters.MemberReference);

            // Fetch loan history report - now returns ReportData directly
            var reportData = await _reportService.GetLoanSituationData(parameters);
            var loanHistory = reportData?.LoanHistory;


            // ✅ Generate logo once
            var logoPath = bra != null
                ? PaymentReceiptMapping.GenerateAndSaveBankLogoImage(bra.Bank?.LogoUrl, bra.Name)
                : "";

            var rows = new List<LoanSituationRow>();

            if (loanHistory?.Loans != null)
            {
                var metrics = loanHistory.Metrics;
                foreach (var loan in loanHistory.Loans)
                {
                    rows.Add(new LoanSituationRow
                    {
                        Logo = logoPath,
                        // Header info
                        BranchName = GetBranchName(),
                        BranchCode = GetBranchCode() ?? "-",
                        BankName = GetBankName() ?? "-",
                        PrintedBy = GetUserFullName(),
                        PrintedOn = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                        Periodfrom = parameters.DateFrom.ToString("dd/MM/yyyy"),
                        PeriodTo = parameters.DateTo.ToString("dd/MM/yyyy"),
                        BranchTellephone = bra.Telephone,
                        BranchAddress = bra.Address,
                        Phone = cus.Phone,
                        Address  = cus.Address,
                        LoanNumber = loan.AccountNumber,
                        // Loan identifiers
                        LoanId = loan.LoanId,
                        ContractCode = loan.ContractCode,
                        LoanType = loan.LoanType,
                        Status = loan.Status,
                        LoanAmount = loan.LoanAmount,
                        // Loan amounts and balances
                        Balance = loan.Balance,
                        Principal = loan.Principal,
                        Interest = loan.Interest,
                        InterestRate = loan.InterestRate,
                        Penalty = loan.Penalty,
                        TotalRepayment = loan.TotalRepayment,
                        Tax = loan.Vat,
                        Vat = loan.Vat,
                        DueAmount = loan.DueAmount,
                        LastRepaymentAmount = loan.LastRepaymentAmount,
                        LastRepaymentDate = loan.LastRepaymentDate,
                        NextRepaymentDate = loan.NextRepaymentDate,
                        LoanDate = loan.LoanDate,
                        Instalment = loan.Installment,

                        DelDays = loan.DelDays,
                        DelAmount = loan.DelAmount,
                        DelInterest = loan.DelInterest,
                        LoanAccount = loan.AccountNumber,
                        // Member info
                        MemberReference = loan.MemberReference,
                        MembersName = loan.MembersName,
                        MembersBranch = loan.MembersBranch,
                        MemberBranchCode = loan.MemberBranchCode,
                                           
                        // Loan timeline
                        DisbursementDate = loan.DisbursementDate,

                        // Portfolio summary (from metrics)
                        TotalLoans = metrics?.TotalLoans ?? 0,
                        TotalOutstanding = metrics?.TotalOutstanding ?? 0,
                        ActiveLoans = metrics?.ActiveLoans ?? 0,
                        DelinquentLoans = metrics?.DelinquentLoans ?? 0,
                        PerAmount = metrics?.ParAmount ?? 0,

                        TotalPortfolioValue = metrics?.TotalOutstanding ?? 0m,
                        AverageInterestRate = metrics != null && metrics.TotalLoans > 0
                            ? metrics.ParAmount / metrics.TotalLoans
                            : 0m,
                        DelinquencyRate = metrics != null && metrics.TotalLoans > 0
                            ? (decimal)metrics.DelinquentLoans / metrics.TotalLoans * 100
                            : 0m
                    });
                }
            }

            return rows;
        }



        // 5. Build Account Situation Rows
        public async Task<List<TransactionStaement>> BuildAccountSituationRows(FinancialReportFilter parameters)
        {
            var situationData = await _reportService.GetAccountSituationData(parameters);

            var bra = await GetBranchandbank();
            var cus = await GetCustomer(parameters.MemberReference);

            var rows = new List<TransactionStaement>();

            if (situationData?.AccountSituation == null)
                return rows;

            var situation = situationData.AccountSituation;

            // =========================
            // ACCOUNT SITUATION ROWS
            // =========================
            foreach (var acc in situation.Accounts)
            {
                rows.Add(new TransactionStaement
                {
                    // ===== HEADER FIELDS =====
                    ReportTitle = "ACCOUNT PORTFOLIO SITUATION REPORT",
                    CNI = "N/A",

                    BranchName = GetBranchName(),
                    BranchCode = GetBranchCode(),
                    HeadOfficeName = GetBankName(),
                    HeadOfficeAddress = bra.Address,
                    HeadOfficeTelephone = bra.Telephone,

                    AccountName = parameters.AccountIds != null && parameters.AccountIds.Any()
                        ? string.Join(" - ", parameters.AccountIds)
                        : "N/A",

                    CustomerId = cus.CustomerId,
                    CustomerName = cus.FirstName + " " + cus.LastName,
                    Telephone = cus.Phone,

                    PrintedBy = GetUserFullName(),
                    PrintedOn = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),

                    Printedfrom = situation.OperationDate.ToString("dd/MM/yyyy"),
                    PrintedTo = situation.OperationDate.ToString("dd/MM/yyyy"),

                    Currreccy = "Central African CFA franc",
                    Address = cus.Address,

                    Balance = acc.Balance,
                    NetBalance = acc.ActualBalance,
                    BlockedAmount = acc.BlockedAmount,
                    TotalBalance = situation.Summary.TotalActualBalance,
                    TotalBlockedAmount = situation.Summary.TotalBlockedAmount,
                    TotalActualBalance = situation.Summary.TotalActualBalance,

                    Year = $"2021 - {DateTime.Now.Year}",


                    // ===== DETAIL FIELDS =====
                    AccountNumber = acc.AccountNumber,
                    AccountType = acc.AccountType,
                    OpeningBalance = acc.Balance.ToString(),
                    Charges = acc.BlockedAmount,
                    Amount = acc.ActualBalance,
                    TotalOperation = situation.Accounts.Count.ToString(),
                    Date = acc.SnapshotDate,
                    Time = acc.SnapshotDate,
                    Logo = bra != null
                        ? PaymentReceiptMapping.GenerateAndSaveBankLogoImage(
                            bra.Bank?.LogoUrl, bra.Name)
                        : ""
                });
            }

            return rows;
        }

        // 6. Build Interest Report Rows
        public async Task<List<InterestReportRow>> BuildInterestRows(FinancialReportFilter parameters)
        {
            var interestData = await _reportService.GetInterestReportData(
                parameters.DateFrom,
                parameters.DateTo,
                parameters.MemberReference);

            var bra = await GetBranchandbank();
            var cus = await GetCustomer(parameters.MemberReference);

            var rows = new List<InterestReportRow>();

            // Generate logo
            var logoPath = bra != null
                ? PaymentReceiptMapping.GenerateAndSaveBankLogoImage(bra.Bank?.LogoUrl, bra.Name)
                : "";

            // Header
            rows.Add(new InterestReportRow
            {
                ReportTitle = "INTEREST REPORT",
                BankName = GetBankName(),
                BranchName = GetBranchName(),
                BranchCode = GetBranchCode(),
                HeadOfficeAddress = bra?.Address ?? "",
                HeadOfficeTelephone = bra?.Telephone ?? "",
                PeriodFrom = parameters.DateFrom,
                PeriodTo = parameters.DateTo,
                PrintedBy = GetUserFullName(),
                PrintedOn = DateTime.Now,
                Currency = "Central African CFA franc",
                Logo = logoPath,
                CustomerId = cus?.CustomerId ?? "",
                CustomerName = cus != null ? $"{cus.FirstName} {cus.LastName}" : "",
                Telephone = cus?.Phone ?? ""
            });

            // Interest Details
            foreach (var item in interestData.InterestDetails)
            {
                rows.Add(new InterestReportRow
                {
                    AccountNumber = item.AccountNumber,
                    AccountName = item.AccountName,
                    CustomerName = item.CustomerName,
                    CustomerId = item.CustomerId,
                    InterestRate = item.InterestRate,
                    PrincipalAmount = item.PrincipalAmount,
                    InterestAmount = item.InterestAmount,
                    CalculatedFrom = item.CalculatedFrom,
                    CalculatedTo = item.CalculatedTo,
                    DaysCount = item.DaysCount,
                    TransactionDate = item.TransactionDate,
                    TransactionReference = item.TransactionReference,
                    TransactionType = item.TransactionType,
                    Status = item.Status
                });
            }

            // Summary
            if (interestData.Summary != null)
            {
                rows.Add(new InterestReportRow
                {
                    TotalInterestAmount = interestData.Summary.TotalInterestAmount,
                    TotalTransactions = interestData.Summary.TotalTransactions,
                    AverageInterestRate = interestData.Summary.AverageInterestRate
                });
            }

            return rows;
        }

        // 7. Build VAT Report Rows
        public async Task<List<VatReportRow>> BuildVatRows(FinancialReportFilter parameters)
        {
            var vatData = await _reportService.GetVatReportData(
                parameters.DateFrom,
                parameters.DateTo,
                parameters.MemberReference);

            var bra = await GetBranchandbank();
            var cus = await GetCustomer(parameters.MemberReference);

            var rows = new List<VatReportRow>();

            // Generate logo
            var logoPath = bra != null
                ? PaymentReceiptMapping.GenerateAndSaveBankLogoImage(bra.Bank?.LogoUrl, bra.Name)
                : "";

            // Header
            rows.Add(new VatReportRow
            {
                ReportTitle = "VALUE ADDED TAX (VAT) REPORT",
                BankName = GetBankName(),
                BranchName = GetBranchName(),
                BranchCode = GetBranchCode(),
                HeadOfficeAddress = bra?.Address ?? "",
                HeadOfficeTelephone = bra?.Telephone ?? "",
                PeriodFrom = parameters.DateFrom,
                PeriodTo = parameters.DateTo,
                PrintedBy = GetUserFullName(),
                PrintedOn = DateTime.Now,
                Currency = "Central African CFA franc",
                VatRate = "18%", // Default VAT rate, adjust as needed
                Logo = logoPath,
                CustomerId = cus?.CustomerId ?? "",
                CustomerName = cus != null ? $"{cus.FirstName} {cus.LastName}" : "",
                Telephone = cus?.Phone ?? ""
            });

            // VAT Details
            foreach (var item in vatData.VatDetails)
            {
                rows.Add(new VatReportRow
                {
                    TransactionDate = item.TransactionDate,
                    TransactionReference = item.TransactionReference,
                    InvoiceNumber = item.InvoiceNumber,
                    CustomerName = item.CustomerName,
                    CustomerTaxId = item.CustomerTaxId,
                    TaxableAmount = item.TaxableAmount,
                    VatAmount = item.VatAmount,
                    TotalAmount = item.TotalAmount,
                    TransactionType = item.TransactionType,
                    Status = item.Status
                });
            }

            // Summary
            if (vatData.Summary != null)
            {
                rows.Add(new VatReportRow
                {
                    TotalTaxableAmount = vatData.Summary.TotalTaxableAmount,
                    TotalVatAmount = vatData.Summary.TotalVatAmount,
                    TotalAmountSummary = vatData.Summary.TotalAmount,
                    TotalTransactions = vatData.Summary.TotalTransactions
                });
            }

            return rows;
        }

        // 8. Build Penalty Report Rows
        public async Task<List<PenaltyReportRow>> BuildPenaltyRows(FinancialReportFilter parameters)
        {
            var penaltyData = await _reportService.GetPenaltyReportData(
                parameters.DateFrom,
                parameters.DateTo,
                parameters.MemberReference);

            var bra = await GetBranchandbank();
            var cus = await GetCustomer(parameters.MemberReference);

            var rows = new List<PenaltyReportRow>();

            // Generate logo
            var logoPath = bra != null
                ? PaymentReceiptMapping.GenerateAndSaveBankLogoImage(bra.Bank?.LogoUrl, bra.Name)
                : "";

            // Header
            rows.Add(new PenaltyReportRow
            {
                ReportTitle = "PENALTY REPORT",
                BankName = GetBankName(),
                BranchName = GetBranchName(),
                BranchCode = GetBranchCode(),
                HeadOfficeAddress = bra?.Address ?? "",
                HeadOfficeTelephone = bra?.Telephone ?? "",
                PeriodFrom = parameters.DateFrom,
                PeriodTo = parameters.DateTo,
                PrintedBy = GetUserFullName(),
                PrintedOn = DateTime.Now,
                Currency = "Central African CFA franc",
                Logo = logoPath,
                CustomerId = cus?.CustomerId ?? "",
                CustomerName = cus != null ? $"{cus.FirstName} {cus.LastName}" : "",
                Telephone = cus?.Phone ?? ""
            });

            // Penalty Details
            foreach (var item in penaltyData.PenaltyDetails)
            {
                rows.Add(new PenaltyReportRow
                {
                    AccountNumber = item.AccountNumber,
                    AccountName = item.AccountName,
                    CustomerName = item.CustomerName,
                    CustomerId = item.CustomerId,
                    LoanNumber = item.LoanNumber,
                    PenaltyType = item.PenaltyType,
                    PenaltyRate = item.PenaltyRate,
                    PrincipalAmount = item.PrincipalAmount,
                    PenaltyAmount = item.PenaltyAmount,
                    DaysOverdue = item.DaysOverdue,
                    DueDate = item.DueDate,
                    PenaltyDate = item.PenaltyDate,
                    TransactionReference = item.TransactionReference,
                    Status = item.Status,
                    Remarks = item.Remarks
                });
            }

            // Summary
            if (penaltyData.Summary != null)
            {
                rows.Add(new PenaltyReportRow
                {
                    TotalPenaltyAmount = penaltyData.Summary.TotalPenaltyAmount,
                    TotalTransactions = penaltyData.Summary.TotalTransactions,
                    AveragePenaltyRate = penaltyData.Summary.AveragePenaltyRate
                });
            }

            return rows;
        }
    }
}