using CBS.FrontDesk.Data.Entity.DataSetLoanPortfolio;
using CBS.NLoan.Data.Dto.DataSetLoanPortfolio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.ReportDataSetDto.LoanPortFolioDataSet
{
 
    public class LoanDelinquencyReportResultRPT
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
        public List<FlatAgeGenderRecord> Flat_AgeAndGender { get; set; }
        public List<FlatAgeLoanTypeRecord> Flat_AgeAndLoanType { get; set; }
        public List<FlatZoneRecord> Flat_ByZone { get; set; }
        public List<FlatLoanTermRecord> Flat_ByLoanTerm { get; set; }
        public List<FlatTargetGroupRecord> Flat_ByTargetGroup { get; set; }
        public List<FlatCategoryRecord> Flat_ByCategory { get; set; }

        // Summary & Titles
        public decimal OverallDelinquencyRate { get; set; }
        public decimal PortfolioAtRisk { get; set; }

        public string Title_AgeAndGender { get; set; }
        public string Title_AgeAndLoanType { get; set; }
        public string Title_ByZone { get; set; }
        public string Title_ByLoanTerm { get; set; }
        public string Title_ByTargetGroup { get; set; }
        public string Title_ByCategory { get; set; }

        public List<LoanPortfolioDto> PortfolioDetails { get; set; }

        public LoanDelinquencyReportResultRPT()
        {
           

            // Initialize flat lists
            Flat_AgeAndGender = new List<FlatAgeGenderRecord>();
            Flat_AgeAndLoanType = new List<FlatAgeLoanTypeRecord>();
            Flat_ByZone = new List<FlatZoneRecord>();
            Flat_ByLoanTerm = new List<FlatLoanTermRecord>();
            Flat_ByTargetGroup = new List<FlatTargetGroupRecord>();
            Flat_ByCategory = new List<FlatCategoryRecord>();

            PortfolioDetails = new List<LoanPortfolioDto>();

            OverallDelinquencyRate = 0m;
            PortfolioAtRisk = 0m;

            Title_AgeAndGender = "Delinquency According to Ages and Gender";
            Title_AgeAndLoanType = "Delinquency According to Ages and Loan Types";
            Title_ByZone = "Delinquency According to Zones";
            Title_ByLoanTerm = "Delinquency According to Loan Terms";
            Title_ByTargetGroup = "Delinquency According to Target Groups";
            Title_ByCategory = "Delinquency According to Categories";
        }
    }

}
