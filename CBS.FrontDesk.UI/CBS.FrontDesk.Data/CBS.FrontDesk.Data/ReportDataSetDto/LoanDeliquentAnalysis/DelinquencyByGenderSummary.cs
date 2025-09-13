using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.ReportDataSetDto.LoanDeliquentAnalysis
{
    public class DelinquencyByGenderSummary
    {
        public string Gender { get; set; } // "MALE", "FEMALE", "GROUPS"
        public decimal TotalAmount { get; set; }
        public decimal DelinquentAmount { get; set; }
        public int DelinquentLoanCount { get; set; }
        public decimal PercentageOfTotalDelinquency { get; set; }
    }

}
