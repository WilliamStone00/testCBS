using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.NLoan.Data.Dto.DataSetLoanPortfolio
{
    public class DelinquencyCellData
    {
        public int Num { get; set; }                // Number of loans by group
        public decimal Amount { get; set; }         // Principal amount (DeliquentAmount or Balance)
        public decimal Percentage { get; set; }     // % of total amount
        public int InterestNum { get; set; }        // Number of loans contributing to interest
        public decimal Interest { get; set; }       // Interest amount
        public decimal IPercentage { get; set; }    // % of total interest
    }

}
