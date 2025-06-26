using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.ReportDataSetDto.LoanDeliquentAnalysis
{
    public class LoanDeliquentDto
    {

        public string Id { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string LoanApplicationId { get; set; }
        public string LoanType { get; set; }
        public string BranchId { get; set; }
        public decimal TotalRepayment { get; set; }
        public decimal TotalPrincipalCollected { get; set; }
        public decimal Principal { get; set; }
        public decimal LoanAmount { get; set; }
        public decimal TotalRepayVAT { get; set; }
        public decimal TotalRepayInterest { get; set; }
        public decimal TotalRepayPenalty { get; set; }
        public decimal Balance { get; set; }
        public decimal DueAmount { get; set; }
        public decimal InterestRate { get; set; }
        public decimal AccrualInterest { get; set; }
        public decimal DeliquentAmount { get; set; }
        public decimal DeliquentInterest { get; set; }
        public int DeliquentDays { get; set; }
        public decimal Fine { get; set; }
        public decimal VAT { get; set; }
        public int Installments { get; set; }
        public string LoanAccountNumber { get; set; }
        public string LoanManager { get; set; }
        public DateTime LoanDate { get; set; }
        public DateTime LastRepaymentDate { get; set; }
        public decimal LastRepaymentAmount { get; set; }
        public DateTime MaturityDate { get; set; }
        public string LoanStatus { get; set; }
        public string PurposeName { get; set; }
        public string ProductType { get; set; }
        public string ApplicationType { get; set; }
        public string TermName { get; set; }
        public string LoanCategory { get; set; }
        public string LoanTarget { get; set; }
        public string DeliquentStatus { get; set; }
        public decimal SavingBalance { get; set; }
        public string LoanDuration { get; set; }
        public string LoanDeliquencyConfigurationId { get; set; }
        public string DelinquencyConfigName { get; set; }
        public decimal InterestForcasted { get; set; }
        public string LegalForm { get; set; }
        public string Gender { get; set; }
        public string PhoneNumber { get; set; }
        public string FullName { get; set; }
        public int Age { get; set; }
        public int DaysFrom { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        // 🔽 Summary Properties for Report Footer Calculations 🔽
        public int TotalCurrentLoanCount { get; set; }
        public int TotalDelinquentLoanCount { get; set; }
        public int TotalLoanCount => TotalCurrentLoanCount + TotalDelinquentLoanCount;
        public decimal TotalCapital { get; set; }
        public decimal TotalBalance { get; set; }
        public decimal TotalInterest { get; set; }
        public decimal TotalCurrentCapital { get; set; }
        public decimal TotalDelinquentCapital { get; set; }
        //  public decimal TotalCapital => TotalCurrentCapital + TotalDelinquentCapital;

        public decimal TotalCurrentBalance { get; set; }
        public decimal TotalDelinquentBalance { get; set; }
        //public decimal TotalBalance => TotalCurrentBalance + TotalDelinquentBalance;

        public decimal TotalCurrentInterest { get; set; }
        public decimal TotalDelinquentInterest { get; set; }
        //public decimal TotalInterest => TotalCurrentInterest + TotalDelinquentInterest;

        public string PortfolioInsight { get; set; }
        public decimal TotalSavings { get; set; }
        public decimal DefaultRate { get; set; }
        public decimal LoanToSavingsRatio { get; set; }
        public decimal DelinquencyToSavingsRatio { get; set; }
        public decimal SavingsCoverageRatio { get; set; }
        public string LiquidityRiskLevel { get; set; }
        public string LoanToSavingsRatioExplanation { get; set; }
        public string DelinquencyToSavingsRatioExplanation { get; set; }
        public string SavingsCoverageRatioExplanation { get; set; }
        public string DefaultRateExplanation { get; set; }
        public string InsightMethodology { get; set; }
        public decimal TotalOutstandingLoanBalance { get; set; }

        public string Logo { get; set; }
        public string BranchAddress { get; set; }
        public string BranchTelephone { get; set; }
        public string HeadOfficeName { get; set; }
        public string HeadOfficeAddress { get; set; }
        public string HeadOfficeTelephone { get; set; }
        public string HeadOfficeEmail { get; set; }
        public string HeadOfficeWebSite { get; set; }
        public string HeadOfficeInitial { get; set; }
        public string HeadOfficeCode { get; set; }
    }
}
