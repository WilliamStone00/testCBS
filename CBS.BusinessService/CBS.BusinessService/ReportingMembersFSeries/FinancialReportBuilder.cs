using BusinessServices;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Session;
using CBS.FrontDesk.Data.Entity.Accounting_V2.DaillyCollectorCommission;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.CustomerManagement;
using CBS.FrontDesk.Data.Entity.ReportMembersFSeries;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
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

            // ✅ Correct call
            var accountStatement =
                await _reportService.GetCustomerTransactionsByAccountNumber(parameters);

            var rows = new List<TransactionStaement>();

            var transactions = accountStatement?.Transactions ?? new List<TransactionRaw>();
            var summary = accountStatement?.summary;

            decimal totalDebit = summary?.TotalDebit ?? 0;
            decimal totalCredit = summary?.TotalCredit ?? 0;
            decimal closingBalance = summary?.ClosingBalance ?? 0;

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
                    Telephone = cus?.Phone ?? "N/A",
                    OpeningBalance = accountStatement?.OpeningBalance.ToString("N1") ?? "0",
                    TotalDebit = "0",
                    TotalCredit = "0",
                    ClosingBalance = 0,
                    BalanceasOf = $"Balance as of {parameters.DateTo:dd/MM/yyyy}: 0"
                });

                return rows;
            }

            // =========================
            // CASE 2: TRANSACTIONS EXIST
            // =========================
            foreach (var transaction in transactions)
            {
                rows.Add(new TransactionStaement
                {
                    // -------- Account / Customer --------
                    AccountNumber = parameters.AccountId ?? transaction.AccountNumber,
                    AccountName = transaction.AccountType ?? "N/A",
                    CustomerId = cus?.CustomerId ?? "N/A",
                    CustomerName = cus != null ? $"{cus.FirstName} {cus.LastName}" : "N/A",
                    Telephone = cus?.Phone ?? "N/A",
                    Currreccy = "Central African CFA franc",
                    Village = cus?.town ?? "",

                    // -------- Branch --------
                    BranchName = GetBranchName(),
                    BranchCode = GetBranchCode(),
                    HeadOfficeName = GetBankName(),

                    // -------- Transaction --------
                    Date = transaction.CreatedDate,
                    Time = transaction.CreatedDate,
                    PrintedOn = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),

                    Description = transaction.Operation,
                    Reference = transaction.TransactionReference,
                    Representative = string.IsNullOrWhiteSpace(transaction.DepositorName)
                                        ? "N/A"
                                        : transaction.DepositorName,

                    DepositorName = string.IsNullOrWhiteSpace(transaction.DepositorName)
                                        ? "N/A"
                                        : transaction.DepositorName,

                    // -------- Amounts --------
                    Debit = transaction.Debit,
                    Credit = transaction.Credit,
                    Balance = transaction.Balance,
                    OpeningBalance = transaction.PreviousBalance.ToString("N1"),

                    // -------- Totals (FROM SUMMARY) --------
                    TotalDebit = totalDebit.ToString("N1"),
                    TotalCredit = totalCredit.ToString("N1"),
                    TotalOperation = transactions.Count.ToString(),

                    ClosingBalance = closingBalance,
                    BalanceasOf =
                        $"Balance as of {parameters.DateTo:dd/MM/yyyy}: {closingBalance:N1}",

                    // -------- Report --------
                    Printedfrom = parameters.DateFrom.ToString("dd/MM/yyyy"),
                    PrintedTo = parameters.DateTo.ToString("dd/MM/yyyy"),
                    Year = DateTime.Now.Year.ToString(),
                    PrintedBy = GetUserFullName(),
                    Address = cus?.Address ?? "",
                    HeadOfficeAddress = bra?.Address ?? "",
                    Logo = bra != null
                        ? PaymentReceiptMapping.GenerateAndSaveBankLogoImage(
                            bra.Bank?.LogoUrl, bra.Name)
                        : ""
                });
            }

            return rows;
        }



        // 2. Build Member Situation Rows
        public async Task<List<MemberSituationRow>> BuildMemberSituationRows(FinancialReportFilter  parameters)
        {
                var memberData = await _reportService.GetMemberSituationData(parameters.AccountId,
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
            public async Task<List<LoanRepaymentRow>> BuildLoanRepaymentRows(FinancialReportFilter parameters)
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
        public async Task<List<LoanSituationRow>> BuildLoanSituationRows(FinancialReportFilter parameters)
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
                //TotalLoans = situationData.PortfolioSummary.TotalLoans,
                //TotalPortfolioValue = situationData.PortfolioSummary.TotalPortfolioValue,
                //AverageInterestRate = situationData.PortfolioSummary.AverageInterestRate,
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
                    Address = cus.town,

                    Balance = acc.Balance,
                    NetBalance = acc.ActualBalance,
                    BlockedAmount = acc.BlockedAmount,
                    TotalBalance = situation.Summary.TotalActualBalance,
                    TotalBlockedAmount = situation.Summary.TotalBlockedAmount,
                    TotalActualBalance = situation.Summary.TotalActualBalance,

                    Year = DateTime.Now.Year.ToString(),

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

    }
}
