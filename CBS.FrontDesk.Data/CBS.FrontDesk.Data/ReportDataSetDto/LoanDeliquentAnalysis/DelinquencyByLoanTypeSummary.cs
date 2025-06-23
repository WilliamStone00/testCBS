using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.ReportDataSetDto.LoanDeliquentAnalysis
{
    public class DelinquencyByLoanTypeSummary
    {
        public string LoanType { get; set; } // "EXCEPTIONAL LOAN", "MAIN LOANS", "SPECIAL LOANS & AD"
        public decimal TotalAmount { get; set; }
        public decimal MaleAmount { get; set; }
        public decimal FemaleAmount { get; set; }
        public decimal GroupAmount { get; set; }
        public decimal PercentageOfTotalDelinquency { get; set; }
    }

}
