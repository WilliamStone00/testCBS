using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.ReportDataSetDto.LoanDeliquentAnalysis
{
    public class LoanDelinquencyCategorySummary
    {
        public string Category { get; set; } // e.g., "Current loans", "1–29 days", etc.
        public int TotalCount { get; set; }
        public int MaleCount { get; set; }
        public int FemaleCount { get; set; }
        public int GroupCount { get; set; }
        public decimal TotalBalance { get; set; }
        public decimal MaleBalance { get; set; }
        public decimal FemaleBalance { get; set; }
        public decimal GroupBalance { get; set; }
        public decimal PercentageOfTotalLoans { get; set; }
    }

}
