using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.ReportDataSetDto.LoanPortFolioDataSet
{
    public class FlatGroupBreakdown
    {
        public string GroupingKey { get; set; } // "term", "targetGroup", or "category"

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
    }

}
