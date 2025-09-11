using CBS.FrontDesk.Data.Entity.DataSetLoanPortfolio;
using CBS.FrontDesk.Data.ReportDataSetDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.NLoan.Data.Dto.DataSetLoanPortfolio
{
    public class LoanDelinquencyReportResult
    {
        public List<DelinquencyByAgeAndGenderRow> ByAgeAndGender { get; set; }
        public List<DelinquencyByAgeAndLoanTypeRow> ByAgeAndLoanType { get; set; }
        public List<DelinquencyByZoneRow> ByZone { get; set; }
        public List<DelinquencyByLoanTermRow> ByLoanTerm { get; set; }
        public List<DelinquencyByTargetGroupRow> ByTargetGroup { get; set; }
        public List<DelinquencyByCategoryRow> ByCategory { get; set; }
        public decimal OverallDelinquencyRate { get; set; }
        public decimal PortfolioAtRisk { get; set; }

        public string Title_AgeAndGender { get; set; }
        public string Title_AgeAndLoanType { get; set; }
        public string Title_ByZone { get; set; }
        public string Title_ByLoanTerm { get; set; }
        public string Title_ByTargetGroup { get; set; }
        public string Title_ByCategory { get; set; }

        public List<LoanPortfolioDto> PortfolioDetails { get; set; }

        public LoanDelinquencyReportResult()
        {
            ByAgeAndGender = new List<DelinquencyByAgeAndGenderRow>();
            ByAgeAndLoanType = new List<DelinquencyByAgeAndLoanTypeRow>();
            ByZone = new List<DelinquencyByZoneRow>();
            ByLoanTerm = new List<DelinquencyByLoanTermRow>();
            ByTargetGroup = new List<DelinquencyByTargetGroupRow>();
            ByCategory = new List<DelinquencyByCategoryRow>();
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
