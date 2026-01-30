using BusinessServices;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Session;
using CBS.FrontDesk.Data.Entity.Accounting_V2.DaillyCollectorCommission;
using CBS.FrontDesk.Data.Entity.CustomerManagement;
using CBS.FrontDesk.Data.Entity.ReportMembersFSeries;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
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

        #region helper methods for customer and date formating
        public async Task<IndividualProfile> GetCustomer(string customerId)
        {
            if (string.IsNullOrWhiteSpace(customerId))
                throw new ArgumentException("Customer reference cannot be null or empty.", nameof(customerId));

            var customer = await _CashDeskServicesService.GetCustomer(customerId);
            if (customer == null)
                return null;
        
            return customer;
        }
        // Helper method to parse transaction date string
        private string ParseTransactionDate(string dateString, string format)
        {
            if (DateTime.TryParse(dateString, out DateTime date))
            {
                return date.ToString(format);
            }

            // Try parsing with specific formats if needed
            string[] formats = {
            "yyyy-MM-ddTHH:mm:ss.fffZ",
            "yyyy-MM-ddTHH:mm:ss",
            "dd/MM/yyyy HH:mm:ss",
            "MM/dd/yyyy HH:mm:ss"
             };

            if (DateTime.TryParseExact(dateString, formats, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out DateTime parsedDate))
            {
                return parsedDate.ToString(format);
            }

            // Return empty string or original if cannot parse
            return "";
        }
        #endregion

        // 1. Build Account Statement Rows
        public async Task<List<AccountStatementRow>> BuildAccountStatementRows(ReportParameters parameters)
        {
            var cus = await GetCustomer(parameters.CustomerId);
            var transactions = await _reportService
                .GetCustomerTransactionsByAccountNumber(parameters.AccountTypeId);

            var rows = new List<AccountStatementRow>();

            decimal totalDebit = 0;
            decimal totalCredit = 0;
            decimal finalBalance = 0;

            // ✅ CASE 1: No transactions → header only
            if (!transactions.Any())
            {
                rows.Add(new AccountStatementRow
                {
                    BankName = GetBankName(),
                    BankCode = GetBankCode(),
                    BranchName = GetBranchName(),
                    BranchCode = GetBranchCode(),
                    Tell = "22316870",
                    Bp = "392",
                    ReportHeader = "ACCOUNT STATEMENT",
                    PeriodFrom = parameters.DateFrom.Year.ToString(),
                    PeriodTo = parameters.DateTo.Year.ToString(),
                    AccountNo = parameters.AccountTypeId,
                    Currency = "Central African CFA franc",
                    PrintedBy = GetUserFullName(),
                    PrintedOn = DateTime.Now.Year.ToString(),
                    Phone = cus.Phone,
                    BegginingBalance = 0
                });

                return rows;
            }

            // ✅ CASE 2: Transactions exist
            foreach (var transaction in transactions)
            {
                totalDebit += transaction.Debit;
                totalCredit += transaction.Credit;

                // 🔑 Use DB running balance (bank-correct)
                finalBalance = transaction.Balance;

                rows.Add(new AccountStatementRow
                {
                    // Header (Crystal uses first row)
                    BankName = GetBankName(),
                    BranchName = GetBranchName(),
                    BranchCode = GetBranchCode(),
                    Tell = "22316870",
                    Bp = "392",
                    ReportHeader = "ACCOUNT STATEMENT",
                    PeriodFrom = parameters.DateFrom.ToString("dd/MM/yyyy"),
                    PeriodTo = parameters.DateTo.ToString("dd/MM/yyyy"),
                    AccountNo = parameters.AccountTypeId,
                    Currency = "Central African CFA franc",
                    PrintedBy = GetUserFullName(),
                    PrintedOn = DateTime.Now.ToString("dd/MM/yyyy"),
                    Phone = cus.Phone,
                    AccountName = transaction.Account.AccountName,

                    // Transaction details
                    Date = ParseTransactionDate(transaction.CreatedDate, "dd/MM/yyyy"),
                    Time = ParseTransactionDate(transaction.CreatedDate, "HH:mm:ss"),
                    Description = transaction.Operation,
                    Reference = transaction.TransactionReference,
                    Representative = string.IsNullOrWhiteSpace(transaction.DepositorName)
                        ? "N/A"
                        : transaction.DepositorName,

                    Debit = transaction.Debit > 0 ? transaction.Debit.ToString("N1") : "0.0",
                    Credit = transaction.Credit > 0 ? transaction.Credit.ToString("N1") : "0.0",
                    Balance = transaction.Balance.ToString("N1"),

                    CustomerId = cus.CustomerId,
                    CustomerName = cus.FirstName + " " + cus.LastName,

                    OpeningBalance = transaction.Account.OpeningBalance,

                    Year = DateTime.Now.Year.ToString(),
                    TotalDebit = totalDebit.ToString("N1"),
                    TotalCredit = totalCredit.ToString("N1"),
                    TotalOperation = transactions.Count.ToString()
                });
            }

            // ✅ FINAL STEP: enforce SAME closing balance & total on all rows
            foreach (var row in rows)
            {
                row.ClosingBalance = finalBalance;
                row.Total = $"Balance as of {parameters.DateTo:dd/MM/yyyy}: {finalBalance:N1}";
            }

            return rows;
        }


        // 2. Build Member Situation Rows
        public async Task<List<MemberSituationRow>> BuildMemberSituationRows(ReportParameters parameters)
        {
                var memberData = await _reportService.GetMemberSituationData(parameters.AccountTypeId,
                    parameters.LoanId,
                    parameters.DateFrom,
                    parameters.DateTo);

                var rows = new List<MemberSituationRow>();

                // Header
                rows.Add(new MemberSituationRow
                {
                    ReportTitle = "MEMBER SITUATION REPORT",
                    BankName = GetBankName(),
                    BranchName = GetBranchName(),
                    PeriodFrom = parameters.DateFrom,
                    PeriodTo = parameters.DateTo,
                    PrintedBy = GetUserFullName(),
                    PrintedOn = DateTime.Now
                });

                // Accounts Summary
                foreach (var account in memberData.MemberAccounts)
                {
                    rows.Add(new MemberSituationRow
                    {
                        AccountNumber = account.AccountNumber,
                        AccountType = account.AccountType,
                        CurrentBalance = account.Balance,
                        //AvailableBalance = account.AvailableBalance,
                        //AccountStatus = account.Status,
                        //LastTransactionDate = account.LastTransactionDate
                    });
                }

                // Loans Summary
                foreach (var loan in memberData.MemberLoans)
                {
                    rows.Add(new MemberSituationRow
                    {
                        LoanNumber = loan.LoanNumber,
                        LoanType = loan.LoanType,
                        LoanAmount = loan.LoanAmount,
                        OutstandingBalance = loan.OutstandingBalance,
                        //NextPaymentDate = loan.NextPaymentDate,
                        //LoanStatus = loan.Status
                    });
                }

                return rows;
            }

            // 3. Build Loan Repayment Rows
            public async Task<List<LoanRepaymentRow>> BuildLoanRepaymentRows(ReportParameters parameters)
            {
                var repaymentData = await _reportService.GetLoanRepaymentData(
                    parameters.LoanId,
                    parameters.DateFrom,
                    parameters.DateTo);

                var rows = new List<LoanRepaymentRow>();

                // Header
                rows.Add(new LoanRepaymentRow
                {
                    ReportTitle = "LOAN REPAYMENT REPORT",
                    BankName = GetBankName(),
                    BranchName = GetBranchName(),
                    LoanNumber = repaymentData.LoanDetails.LoanNumber,
                    CustomerName = repaymentData.LoanDetails.CustomerName,
                    LoanAmount = repaymentData.LoanDetails.LoanAmount,
                    InterestRate = repaymentData.LoanDetails.InterestRate,
                 //   LoanDate = repaymentData.LoanDetails.LoanDate,
                    MaturityDate = repaymentData.LoanDetails.MaturityDate,
                    PrintedBy = GetUserFullName(),
                    PrintedOn = DateTime.Now
                });

                // Repayment Schedule
                foreach (var schedule in repaymentData.RepaymentSchedule)
                {
                    //var actualPayment = repaymentData.ActualRepayments
                    //    .FirstOrDefault(r => r.DueDate.Date == schedule.DueDate.Date);

                    rows.Add(new LoanRepaymentRow
                    {
                        InstallmentNumber = schedule.InstallmentNumber,
                        DueDate = schedule.DueDate,
                        PrincipalDue = schedule.PrincipalDue,
                        InterestDue = schedule.InterestDue,
                        TotalDue = schedule.TotalDue,
                        //ActualPaymentDate = actualPayment?.PaymentDate,
                        //AmountPaid = actualPayment?.AmountPaid ?? 0,
                        //PaymentStatus = actualPayment != null ? "Paid" : "Pending",
                        OutstandingBalance = schedule.OutstandingBalance
                    });
                }

                return rows;
            }

            // 4. Build Loan Situation Rows
            public async Task<List<LoanSituationRow>> BuildLoanSituationRows(ReportParameters parameters)
            {
                var situationData = await _reportService.GetLoanSituationData(
                    parameters.DateFrom,
                    parameters.DateTo);

                var rows = new List<LoanSituationRow>();

                // Header
                rows.Add(new LoanSituationRow
                {
                    ReportTitle = "LOAN PORTFOLIO SITUATION REPORT",
                    BankName = GetBankName(),
                    BranchName = GetBranchName(),
                    PeriodFrom = parameters.DateFrom,
                    PeriodTo = parameters.DateTo,
                    PrintedBy = GetUserFullName(),
                    PrintedOn = DateTime.Now,
                    TotalLoans = situationData.PortfolioSummary.TotalLoans,
                    TotalPortfolioValue = situationData.PortfolioSummary.TotalPortfolioValue,
                    AverageInterestRate = situationData.PortfolioSummary.AverageInterestRate,
                   // DelinquencyRate = situationData.DelinquencyData.DelinquencyRate
                });

                // Loan Portfolio Details
                foreach (var loan in situationData.LoanPortfolio)
                {
                    rows.Add(new LoanSituationRow
                    {
                        LoanNumber = loan.LoanNumber,
                        CustomerName = loan.CustomerName,
                        LoanType = loan.LoanType,
                        LoanAmount = loan.LoanAmount,
                        OutstandingBalance = loan.OutstandingBalance,
                        DisbursementDate = loan.DisbursementDate,
                        MaturityDate = loan.MaturityDate,
                        InterestRate = loan.InterestRate,
                        LoanStatus = loan.LoanStatus,
                        DaysPastDue = loan.DaysPastDue
                    });
                }

                return rows;
            }         
            public async Task<List<LoanSituationRow>> BuildAccountSituationRows(ReportParameters parameters)
            {
                var situationData = await _reportService.GetAccountSituationData(parameters.AccountId,
                    parameters.DateFrom,
                    parameters.DateTo);

                var rows = new List<LoanSituationRow>();

                // Header
                rows.Add(new LoanSituationRow
                {
                    ReportTitle = "Account PORTFOLIO SITUATION REPORT",
                    BankName = GetBankName(),
                    BranchName = GetBranchName(),
                    PeriodFrom = parameters.DateFrom,
                    PeriodTo = parameters.DateTo,
                    PrintedBy = GetUserFullName(),
                    PrintedOn = DateTime.Now,
                    TotalLoans = situationData.PortfolioSummary.TotalLoans,
                    TotalPortfolioValue = situationData.PortfolioSummary.TotalPortfolioValue,
                    AverageInterestRate = situationData.PortfolioSummary.AverageInterestRate,
                   // DelinquencyRate = situationData.DelinquencyData.DelinquencyRate
                });

                // Loan Portfolio Details
                foreach (var loan in situationData.LoanPortfolio)
                {
                    rows.Add(new LoanSituationRow
                    {
                        LoanNumber = loan.LoanNumber,
                        CustomerName = loan.CustomerName,
                        LoanType = loan.LoanType,
                        LoanAmount = loan.LoanAmount,
                        OutstandingBalance = loan.OutstandingBalance,
                        DisbursementDate = loan.DisbursementDate,
                        MaturityDate = loan.MaturityDate,
                        InterestRate = loan.InterestRate,
                        LoanStatus = loan.LoanStatus,
                        DaysPastDue = loan.DaysPastDue
                    });
                }

                return rows;
            }         
    }
}
