using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.LoanConf
{
    public class LoanDeliquencyConfiguration
    {
        public string Id { get; set; }
        [Required]
        public int DaysFrom { get; set; }
        [Required]

        public int DaysTo { get; set; }
        [Required]

        public string Status { get; set; }//Normal,Bad loan, Due loan, Over due loan, unracoverable loan, Write off loan
        [Required]

        public string Name { get; set; }// Par 0-30, Par 31-60, Par 61-90, Par 91-120
        public bool SendSMStoClient { get; set; }
        public bool SendMail { get; set; }
        public bool SendSMS { get; set; }
        public bool ApplyFine { get; set; }
        public bool AffectScoring { get; set; }
        public bool ReportToCreditOffice { get; set; }

    }

    public class PortfolioSummaryDto
    {
        public decimal TotalPrincipal { get; set; }
        public decimal TotalInterest { get; set; }
        public int TotalLoans { get; set; }
        public int TotalDelinquentLoans { get; set; }
        public decimal TotalOutstandingPrincipal { get; set; }
        public decimal TotalDelinquentPrincipal { get; set; }
        public decimal TotalDelinquentInterest { get; set; }
        public decimal TotalBalance { get; set; }
        public decimal PercentageBalance { get; set; }
        public decimal PercentageDelinquentPrincipal { get; set; }
        public decimal PercentageDelinquentInterest { get; set; }
        public decimal PercentageArrears { get; set; }

        public decimal TotalArrears => TotalDelinquentPrincipal + TotalDelinquentInterest;

        public decimal PortfolioAtRiskPercentage =>
            TotalOutstandingPrincipal > 0 ? Math.Round(TotalDelinquentPrincipal / TotalOutstandingPrincipal * 100, 2) : 0;
    }


    public class DelinquentLoanReportDto
    {
        public PortfolioSummaryDto PortfolioSummary { get; set; }
        public List<DelinquentAgingSummaryDto> AgingAnalysis { get; set; }
        public List<DelinquentGenderAgingDto> GenderAgingAnalysis { get; set; }
        public List<DelinquentBorrowerTypeDto> GroupDelinquency { get; set; }
        public List<DelinquentBorrowerTypeDto> IndividualDelinquency { get; set; }
        public List<DelinquentBorrowerTypeDto> LoanTypeDelinquency { get; set; }
        public List<DelinquentGenderAgingDto> MemberAgeDelinquency { get; set; } // Updated to reflect age range breakdown

        public DelinquentLoanReportDto()
        {
            PortfolioSummary = new PortfolioSummaryDto();
            AgingAnalysis = new List<DelinquentAgingSummaryDto>();
            GenderAgingAnalysis = new List<DelinquentGenderAgingDto>();
            GroupDelinquency = new List<DelinquentBorrowerTypeDto>();
            IndividualDelinquency = new List<DelinquentBorrowerTypeDto>();
            LoanTypeDelinquency = new List<DelinquentBorrowerTypeDto>();
            MemberAgeDelinquency = new List<DelinquentGenderAgingDto>();
        }
    }

    public class DelinquentLoanReportSingleDto
    {
        public PortfolioSummaryDto PortfolioSummary { get; set; }
        public DelinquentAgingSummaryDto AgingAnalysis { get; set; }
        public DelinquentGenderAgingDto GenderAgingAnalysis { get; set; }
        public DelinquentBorrowerTypeDto GroupDelinquency { get; set; }
        public DelinquentBorrowerTypeDto IndividualDelinquency { get; set; }
        public DelinquentBorrowerTypeDto LoanTypeDelinquency { get; set; }
        public DelinquentGenderAgingDto MemberAgeDelinquency { get; set; }

        public DelinquentLoanReportSingleDto()
        {
            PortfolioSummary = new PortfolioSummaryDto();
            AgingAnalysis = new DelinquentAgingSummaryDto();
            GenderAgingAnalysis = new DelinquentGenderAgingDto();
            GroupDelinquency = new DelinquentBorrowerTypeDto();
            IndividualDelinquency = new DelinquentBorrowerTypeDto();
            LoanTypeDelinquency = new DelinquentBorrowerTypeDto();
            MemberAgeDelinquency=new DelinquentGenderAgingDto();
        }
    }

    public class DelinquentAgingSummaryDto
    {
        public string AgingBucket { get; set; }
        public int DelinquentLoanCount { get; set; }
        public decimal DelinquentPrincipal { get; set; }
        public decimal DelinquentInterest { get; set; }
        public decimal Balance { get; set; }
        public decimal Principal { get; set; }
        public decimal PercentageBalance { get; set; }

        public decimal TotalArrears => DelinquentPrincipal + DelinquentInterest;
        public decimal PercentageOfLoans { get; set; }
        public decimal PercentageOfPrincipal { get; set; }
        public decimal PercentageOfInterest { get; set; }
        public decimal PercentageOfDelinquentPrincipal { get; set; }
        public decimal PercentageArrears { get; set; }
    }
    public class DelinquentGenderAgingDto
    {
        public string AgingBucket { get; set; }
        public string Gender { get; set; }
        public int LoanCount { get; set; }
        public decimal Principal { get; set; }
        public decimal Balance { get; set; }
        public decimal Interest { get; set; }
        public decimal PercentageOfLoans { get; set; }
        public decimal PercentageOfPrincipal { get; set; }
        public decimal PercentageDelinquentInterest { get; set; }
        public decimal PercentageBalance { get; set; }
        public decimal DelinquentPrincipal { get; set; }
        public decimal PercentageDelinquentPrincipal { get; set; }
        public decimal DelinquentInterest { get; set; }
        public decimal PercentageArrears { get; set; }

    }
    public class DelinquentBorrowerTypeDto
    {
        public string BorrowerName { get; set; }             // For LoanTypeDelinquency, this holds the loan type name
        public string Branch { get; set; }                   // Branch name or "All"
        public string LoanType { get; set; }
        public decimal Balance { get; set; }
        public decimal Principal { get; set; }

        // Actual loan type label
        public int LoanCount { get; set; }                   // Number of delinquent loans
        public decimal DelinquentPrincipal { get; set; }     // Total outstanding principal
        public decimal DelinquentInterest { get; set; }      // Total delinquent interest
        // New percentage-based metrics
        public decimal PercentageOfLoans { get; set; }       // % of total delinquent loan count
        public decimal PercentageOfPrincipal { get; set; }   // % of total delinquent principal
        public decimal PercentageOfInterest { get; set; }    // % of total delinquent interest
        public decimal PercentageBalance { get; set; }
        public decimal PercentageDelinquentPrincipal { get; set; }
        public decimal PercentageDelinquentInterest { get; set; }
        public decimal PercentageArrears { get; set; }

    }
}
