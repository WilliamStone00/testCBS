using CBS.FrontDesk.Data.ReportDataSetDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.DataSetLoanPortfolio
{
    public class LoanPortfolioDto:HeadOffice
    {
        public string Id { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string LoanApplicationId { get; set; }
        public string LoanType { get; set; }
        public string BranchId { get; set; }
        public decimal Principal { get; set; }
        public decimal LoanAmount { get; set; }
        public decimal Balance { get; set; }
        public decimal DueAmount { get; set; }
        public decimal InterestRate { get; set; }
        public decimal AccrualInterest { get; set; }
        public decimal DeliquentAmount { get; set; }
        public decimal DeliquentInterest { get; set; }
        public int DeliquentDays { get; set; }
        public decimal Fine { get; set; }
        public decimal VAT { get; set; }
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
        public string LoanDuration { get; set; }
        public string LoanDeliquencyConfigurationId { get; set; }
        public string DelinquencyConfigName { get; set; }
        public decimal InterestForcasted { get; set; }
        public string LegalForm { get; set; }
        public string Gender { get; set; }
        public string PhoneNumber { get; set; }
        public string FullName { get; set; }
        public int Age { get; set; }
        public decimal TotalRepayVAT { get; set; }
        public decimal TotalRepayInterest { get; set; }
        public decimal TotalRepayPenalty { get; set; }

        // ✅ Derived / Calculated Properties

        public decimal TotalPrincipalCollected { get; set; }

        public decimal TotalRepayment { get; set; }

        public decimal TotalInDefault => DeliquentAmount + DeliquentInterest;

        public decimal OutstandingBalance => Balance + AccrualInterest + Fine + VAT;

        public decimal LiquidityRatio { get; set; }

        public decimal OutstandingBalanceRatio { get; set; }

        public decimal DefaultRatio { get; set; }

        public string LiquidityStatus { get; set; }

        public string HealthStatus { get; set; }

    }

}
