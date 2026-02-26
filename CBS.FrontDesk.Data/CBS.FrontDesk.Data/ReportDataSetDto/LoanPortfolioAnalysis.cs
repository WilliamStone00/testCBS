using CBS.FrontDesk.Data.Entity.DataSetLoanPortfolio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.ReportDataSetDto
{

    public class DeliquentLoanSummaryDto
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
        public decimal TotalForecastedInterest { get; set; }

        public decimal PercentageOfPortfolioPrincipal { get; set; }
        public decimal PercentageOfPortfolioInterest { get; set; }
        public decimal PercentageOfPortfolioForecastedInterest { get; set; }
        public decimal TotalArrears => TotalDelinquentPrincipal + TotalDelinquentInterest;

        public decimal PortfolioAtRiskPercentage =>
            TotalOutstandingPrincipal > 0 ? Math.Round(TotalDelinquentPrincipal / TotalOutstandingPrincipal * 100, 2) : 0;
    }
    public class LoanPortfolioOverviewDto
    {
        // Totals
        public int TotalLoans { get; set; }
        public decimal TotalPrincipal { get; set; }
        public decimal TotalInterest { get; set; }
        public decimal TotalForecastedInterest { get; set; }
        public decimal TotalBalance { get; set; }

        // Current
        public int CurrentLoanCount { get; set; }
        public decimal CurrentPrincipal { get; set; }
        public decimal CurrentBalance { get; set; }

        // Delinquent
        public int DelinquentLoanCount { get; set; }
        public decimal DelinquentPrincipal { get; set; }
        public decimal DelinquentInterest { get; set; }
        public decimal DelinquentBalance { get; set; }

        // Key indicators
        public decimal PortfolioAtRiskPercentage { get; set; }       // (Delinquent Principal / Total Principal)
        public decimal DefaultRate { get; set; }                      // (Delinquent Loans / Total Loans)

        public decimal Arrears => DelinquentPrincipal + DelinquentInterest;
    }

    public class CurrentLoanSummaryDto
    {
        public int TotalLoans { get; set; }
        public decimal TotalPrincipal { get; set; }
        public decimal TotalInterest { get; set; }
        public decimal TotalForecastedInterest { get; set; }
        public decimal TotalOutstandingPrincipal { get; set; }
        public decimal TotalBalance { get; set; }

        public decimal PercentageBalance { get; set; }
        public decimal PercentageOfPortfolioPrincipal { get; set; }
        public decimal PercentageOfPortfolioInterest { get; set; }
        public decimal PercentageOfPortfolioForecastedInterest { get; set; }

        public decimal PortfolioHealthRatio =>
            TotalOutstandingPrincipal > 0
                ? Math.Round(TotalBalance / TotalOutstandingPrincipal * 100, 2)
                : 0;
    }

    public class GenerateLoanPortfolioReportCommand
    {
        public string BranchId { get; set; }

        public string ReportType { get; set; }
        public string StartDate { get; set; }

        public string EndDate { get; set; }

        public string SubReportType { get; set; } // Comma-separated for multi-select

        public string ReportDownloadType { get; set; } // PDF, Excel, Word

        public string MainReportType { get; set; } // All, CurrentLoan, DelinquentLoan, LoanByPurpose

        // ✅ New: Apply filtering based on selected loan portfolio category (e.g., Gender, Type)
        public bool FilterByParam { get; set; }

        public string QueryParam { get; set; } // e.g., "loanbygender", "loanbyproduct"

        public string QueryParamValue { get; set; } // e.g., "Male", "Micro Loan"
    }


    public class LoanPortfolioAnalysis
    {
        public string Logo { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public string BranchAddress { get; set; }
        public string BranchTelephone { get; set; }
        public string HeadOfficeName { get; set; }
        public string HeadOfficeAddress { get; set; }
        public string HeadOfficeTelephone { get; set; }
        public string HeadOfficeEmail { get; set; }
        public string HeadOfficeWebSite { get; set; }
        public string HeadOfficeInitial { get; set; }
        public string HeadOfficeCode { get; set; }
        // ========== LoanPortfolioOverviewDto Properties ==========
        public int TotalLoans { get; set; }
        public decimal TotalPrincipal { get; set; }
        public decimal TotalInterest { get; set; }
        public decimal TotalForecastedInterest { get; set; }
        public decimal TotalBalance { get; set; }

        public int CurrentLoanCount { get; set; }
        public decimal CurrentPrincipal { get; set; }
        public decimal CurrentBalance { get; set; }

        public int DelinquentLoanCount { get; set; }
        public decimal DelinquentPrincipal { get; set; }
        public decimal DelinquentInterest { get; set; }
        public decimal DelinquentBalance { get; set; }

        public decimal PortfolioAtRiskPercentage_Overview { get; set; }
        public decimal DefaultRate { get; set; }
        public decimal Arrears_Overview => DelinquentPrincipal + DelinquentInterest;

        // ========== CurrentLoanSummaryDto Properties ==========
        public int CurrentTotalLoans { get; set; }
        public decimal CurrentTotalPrincipal { get; set; }
        public decimal CurrentTotalInterest { get; set; }
        public decimal CurrentTotalForecastedInterest { get; set; }
        public decimal CurrentTotalOutstandingPrincipal { get; set; }
        public decimal CurrentTotalBalance { get; set; }

        public decimal CurrentPercentageBalance { get; set; }
        public decimal CurrentPercentageOfPortfolioPrincipal { get; set; }
        public decimal CurrentPercentageOfPortfolioInterest { get; set; }
        public decimal CurrentPercentageOfPortfolioForecastedInterest { get; set; }

        public decimal PortfolioHealthRatio =>
            CurrentTotalOutstandingPrincipal > 0
                ? Math.Round(CurrentTotalBalance / CurrentTotalOutstandingPrincipal * 100, 2)
                : 0;

        // ========== DeliquentLoanSummaryDto Properties ==========
        public decimal DelinquentTotalPrincipal { get; set; }
        public decimal DelinquentTotalInterest { get; set; }
        public int DelinquentTotalLoans { get; set; }
        public int TotalDelinquentLoans { get; set; }
        public decimal DelinquentTotalOutstandingPrincipal { get; set; }
        public decimal TotalDelinquentPrincipal { get; set; }
        public decimal TotalDelinquentInterest { get; set; }
        public decimal DelinquentTotalBalance { get; set; }

        public decimal DelinquentPercentageBalance { get; set; }
        public decimal DelinquentPercentageDelinquentPrincipal { get; set; }
        public decimal DelinquentPercentageDelinquentInterest { get; set; }
        public decimal DelinquentPercentageArrears { get; set; }

        public decimal DelinquentTotalForecastedInterest { get; set; }

        public decimal DelinquentPercentageOfPortfolioPrincipal { get; set; }
        public decimal DelinquentPercentageOfPortfolioInterest { get; set; }
        public decimal DelinquentPercentageOfPortfolioForecastedInterest { get; set; }

        public decimal TotalArrears => TotalDelinquentPrincipal + TotalDelinquentInterest;

        public decimal PortfolioAtRiskPercentage =>
            DelinquentTotalOutstandingPrincipal > 0
                ? Math.Round(TotalDelinquentPrincipal / DelinquentTotalOutstandingPrincipal * 100, 2)
                : 0;

        public LoanPortfolioOverviewDto PortfolioOverview { get; set; }
        public CurrentLoanSummaryDto CurrentLoanSummary { get; set; }
        public DeliquentLoanSummaryDto DeliquentLoanSummary { get; set; }
        public List<DelinquentAgingSummaryDto> AgingAnalysis { get; set; }
        public List<DelinquentGenderAgingDto> GenderAgingAnalysis { get; set; }
        public List<DelinquentBorrowerTypeDto> GroupDelinquency { get; set; }
        public List<DelinquentBorrowerTypeDto> IndividualDelinquency { get; set; }
        public List<DelinquentBorrowerTypeDto> LoanTypeDelinquency { get; set; }
        public List<DelinquentGenderAgingDto> MemberAgeDelinquency { get; set; }
        public List<LoanPortfolioDto> LoanPortfolios { get; set; }
        public List<LoanTargetGenderAnalysisDto> LoanTargetGenderAnalysis { get; set; }
        public List<LoanProductTypeTargetGenderAnalysisDto> LoanProductTypeTargetGenderAnalysis { get; set; }
        public List<LoanTermProductTargetGenderAnalysisDto> LoanTermProductTargetGenderAnalysis { get; set; }
        public List<LoanCategoryTermProductTargetGenderAnalysisDto> LoanCategoryTermProductTargetGenderAnalysis { get; set; }
        public LoanPortfolioAnalysis()
        {
            PortfolioOverview=new LoanPortfolioOverviewDto();
            CurrentLoanSummary=new CurrentLoanSummaryDto();
            DeliquentLoanSummary=new DeliquentLoanSummaryDto();
            AgingAnalysis=new List<DelinquentAgingSummaryDto>();
            GenderAgingAnalysis=new List<DelinquentGenderAgingDto>();
            GroupDelinquency=new List<DelinquentBorrowerTypeDto>();
            IndividualDelinquency=new List<DelinquentBorrowerTypeDto>();
            LoanTypeDelinquency=new List<DelinquentBorrowerTypeDto>();
            MemberAgeDelinquency=new List<DelinquentGenderAgingDto>();
            LoanPortfolios=new List<LoanPortfolioDto>();
            LoanTargetGenderAnalysis=new List<LoanTargetGenderAnalysisDto>();
            LoanProductTypeTargetGenderAnalysis=new List<LoanProductTypeTargetGenderAnalysisDto>();
            LoanTermProductTargetGenderAnalysis=new List<LoanTermProductTargetGenderAnalysisDto>();
            LoanCategoryTermProductTargetGenderAnalysis=new List<LoanCategoryTermProductTargetGenderAnalysisDto>();
        }
    }



    public class LoanCategoryTermProductTargetGenderAnalysisDto
    {
        public string LoanCategory { get; set; }
        public string TermName { get; set; }
        public string ProductType { get; set; }
        public string LoanTarget { get; set; }
        public string Gender { get; set; }

        public int LoanCount { get; set; }
        public decimal Principal { get; set; }
        public decimal DelinquentPrincipal { get; set; }
        public decimal DelinquentInterest { get; set; }
        public decimal Interest { get; set; }
        public decimal Balance { get; set; }

        public decimal PercentageOfLoans { get; set; }
        public decimal PercentageOfPrincipal { get; set; }
        public decimal PercentageDelinquentPrincipal { get; set; }
        public decimal PercentageDelinquentInterest { get; set; }
        public decimal PercentageBalance { get; set; }
        public decimal PercentageArrears { get; set; }
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
    public class LoanTargetGenderAnalysisDto : DelinquentGenderAgingDto
    {


    }
    public class LoanTermProductTargetGenderAnalysisDto
    {
        public string TermName { get; set; }
        public string ProductType { get; set; }
        public string LoanTarget { get; set; }
        public string Gender { get; set; }

        public int LoanCount { get; set; }
        public decimal Principal { get; set; }
        public decimal DelinquentPrincipal { get; set; }
        public decimal DelinquentInterest { get; set; }
        public decimal Interest { get; set; }
        public decimal Balance { get; set; }

        public decimal PercentageOfLoans { get; set; }
        public decimal PercentageOfPrincipal { get; set; }
        public decimal PercentageDelinquentPrincipal { get; set; }
        public decimal PercentageDelinquentInterest { get; set; }
        public decimal PercentageBalance { get; set; }
        public decimal PercentageArrears { get; set; }
    }

    public class LoanProductTypeTargetGenderAnalysisDto
    {
        public string ProductType { get; set; }
        public string LoanTarget { get; set; }
        public string Gender { get; set; }

        public int LoanCount { get; set; }
        public decimal Principal { get; set; }
        public decimal DelinquentPrincipal { get; set; }
        public decimal DelinquentInterest { get; set; }
        public decimal Interest { get; set; }
        public decimal Balance { get; set; }

        public decimal PercentageOfLoans { get; set; }
        public decimal PercentageOfPrincipal { get; set; }
        public decimal PercentageDelinquentPrincipal { get; set; }
        public decimal PercentageDelinquentInterest { get; set; }
        public decimal PercentageBalance { get; set; }
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
