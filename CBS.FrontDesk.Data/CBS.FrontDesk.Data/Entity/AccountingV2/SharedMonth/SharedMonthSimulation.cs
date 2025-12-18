using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.AccountingV2.SharedMonth
{
    public class SharedMonthSimulation
    {
        public string BranchId { get; set; }
       
        public int Year { get; set; }
        public string IntrestDistributionType { get; set; }

        public string StartPeriodKey { get; set; }  // e.g. "1 Jan"
        public string EndPeriodKey { get; set; }    // e.g. "1 Feb"
       
        public string ProductId { get; set; }

      
        // Optional: if UI allows the user to choose a rate for this report.
        public decimal? RateOverride { get; set; }
    }


   

    public  class ShareMonthPsiReportLineDto
    {
        public string MemberReference { get; set; } 
        public string MemberName { get; set; } 
        public decimal Balance { get; set; }      // opening balance at StartPeriodKey
        public decimal Interest { get; set; }     // computed interest
        public decimal Gross { get; set; }        // here = Interest (before tax)
        public decimal Adjustment { get; set; }   // sum of adjustment-like tx in period
    }
}
