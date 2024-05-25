using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.LoanConf
{
    public class LoanApplicationFee
    {
        public string Id { get; set; }
        public string FeeRangeId { get; set; }
        public decimal Amount { get; set; }
        public bool IsPaid { get; set; }
        public string LoanApplicationId { get; set; }
        public FeeRange FeeRanges { get; set; }
        public LoanApplication LoanApplication { get; set; }
    }

}
