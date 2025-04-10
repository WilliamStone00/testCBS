using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.NLoan.Data.Dto.DataSetLoanPortfolio
{
    public class DelinquencyByAgeAndLoanTypeRow
    {
        public string Ageing { get; set; }

        public DelinquencyCellData MainLoans { get; set; }
        public DelinquencyCellData ExceptionalLoans { get; set; }
        public DelinquencyCellData SSF { get; set; } 
        public DelinquencyCellData Total { get; set; }
        public DelinquencyCellData TotalInterest { get; set; }
    }

}
