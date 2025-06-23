using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.ReportDataSetDto.LoanDeliquentAnalysis
{
    public class LoanDelinquencyReportDto
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

        public DateTime ReportDate { get; set; }
        public List<LoanDelinquencyCategorySummary> CategorySummaries { get; set; } = new List<LoanDelinquencyCategorySummary>();
        public List<DelinquencyByGenderSummary> GenderSummariesAfter60Days { get; set; } = new List<DelinquencyByGenderSummary>();
        public List<DelinquencyByLoanTypeSummary> LoanTypeSummariesAfter60Days { get; set; } = new List<DelinquencyByLoanTypeSummary>();
        public List<LoanDeliquentDto> LoanEntries { get; set; } = new List<LoanDeliquentDto>();
        public decimal TotalLoanBalance { get; set; }
        public int TotalLoanCount { get; set; }
        public decimal TotalDelinquentAmount { get; internal set; }
        public int TotalDelinquentLoanCount { get; internal set; }
        public List<FlatLoanDelinquencyRow> FlattenedRows { get; set; } = new List<FlatLoanDelinquencyRow>();

    }
    public class FlatLoanDelinquencyRow
    {
        public DateTime ReportDate { get; set; }
        public string Section { get; set; }

        // Loan-specific data (already exists)
        public string Category { get; set; }
        public int TotalCount { get; set; }
        public int MaleCount { get; set; }
        public int FemaleCount { get; set; }
        public int GroupCount { get; set; }
        public decimal TotalBalance { get; set; }
        public decimal MaleBalance { get; set; }
        public decimal FemaleBalance { get; set; }
        public decimal GroupBalance { get; set; }
        public decimal Percentage { get; set; }

        public string Gender { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DelinquentAmount { get; set; }
        public int DelinquentLoanCount { get; set; }

        public string LoanType { get; set; }

        // Branch metadata
        public string BranchName { get; set; }
        public string LogoUrl { get; set; }
        public string BranchCode { get; set; }
        public string BranchAddress { get; set; }
        public string BranchTelephone { get; set; }

        // Head office metadata
        public string HeadOfficeName { get; set; }
        public string HeadOfficeAddress { get; set; }
        public string HeadOfficeTelephone { get; set; }
        public string HeadOfficeEmail { get; set; }
        public string HeadOfficeWebSite { get; set; }
        public string HeadOfficeInitial { get; set; }
        public string HeadOfficeCode { get; set; }
    }


}
