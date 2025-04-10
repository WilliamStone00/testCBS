using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.ReportDataSetDto.LoanPortFolioDataSet
{
    public class FlatAgeLoanTypeRecord
    {
        public string Ageing { get; set; }

        public int MainLoanCount { get; set; }
        public decimal MainLoanAmount { get; set; }
        public decimal MainLoanPercentage { get; set; }

        public int ExceptionalLoanCount { get; set; }
        public decimal ExceptionalLoanAmount { get; set; }
        public decimal ExceptionalLoanPercentage { get; set; }

        public int SSFCount { get; set; }
        public decimal SSFAmount { get; set; }
        public decimal SSFPercentage { get; set; }

        public int TotalCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalPercentage { get; set; }
        public decimal MainLoanInterest { get; set; }
        public decimal MainLoanIPercentage { get; set; }
        public decimal ExceptionalLoanInterest { get; set; }
        public decimal ExceptionalLoanIPercentage { get; set; }
        public decimal SSFInterest { get; set; }
        public decimal SSFIPercentage { get; set; }
        public object TotalInterest { get; set; }
        public object TotalIPercentage { get; set; }
        public int MainLoanInterestCount { get; set; }
        public int SSFInterestCount { get; set; }
        public int ExceptionalLoanInterestCount { get; set; }
        public int TotalInterestCount { get; set; }
    }

}
