using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.ReportDataSetDto.LoanDeliquentAnalysis
{
    public class LoanDelinquencyEntry
    {
        public string LoanId { get; set; }
        public string MemberId { get; set; }
        public string Gender { get; set; } // "MALE", "FEMALE", "GROUPS"
        public string LoanType { get; set; } // "EXCEPTIONAL LOAN", "MAIN LOANS", "SPECIAL LOANS & AD"
        public decimal OutstandingBalance { get; set; }
        public int DaysInArrears { get; set; }
        public DateTime ReportingDate { get; set; }
        public string BranchId { get; set; } // Optional
    }

}
