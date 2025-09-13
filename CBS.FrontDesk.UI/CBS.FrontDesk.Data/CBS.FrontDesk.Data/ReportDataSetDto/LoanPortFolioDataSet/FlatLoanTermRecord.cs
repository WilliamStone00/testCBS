using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.ReportDataSetDto.LoanPortFolioDataSet
{
    public class FlatLoanTermRecord
    {
        public string Term { get; set; }

        public int MaleCount { get; set; }
        public decimal MaleAmount { get; set; }
        public decimal MalePercentage { get; set; }

        public int FemaleCount { get; set; }
        public decimal FemaleAmount { get; set; }
        public decimal FemalePercentage { get; set; }

        public int GroupCount { get; set; }
        public decimal GroupAmount { get; set; }
        public decimal GroupPercentage { get; set; }

        public int TotalCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalPercentage { get; set; }
        public decimal MaleInterest { get; set; }
        public decimal MaleIPercentage { get; set; }
        public decimal FemaleInterest { get; set; }
        public decimal FemaleIPercentage { get; set; }
        public decimal GroupInterest { get; set; }
        public decimal GroupIPercentage { get; set; }
        public object TotalInterest { get; set; }
        public object TotalIPercentage { get; set; }
        public int MaleInterestCount { get; set; }
        public int FemaleInterestCount { get; set; }
        public int GroupInterestCount { get; set; }
        public int TotalInterestCount { get; set; }
    }

}
