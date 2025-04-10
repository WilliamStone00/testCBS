using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.NLoan.Data.Dto.DataSetLoanPortfolio
{
    public class DelinquencyByLoanTermRow
    {
        public string Term { get; set; }

        public DelinquencyCellData Male { get; set; }
        public DelinquencyCellData Female { get; set; }
        public DelinquencyCellData Group { get; set; }
        public DelinquencyCellData Total { get; set; }
        public DelinquencyCellData TotalInterest { get; set; }
    }

}
