using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.LoanConf
{
    public class LoanApplicationCollateral
    {
        public string Id { get; set; }

        public string LoanApplicationId { get; set; }
        [Required]
        public string LoanProductCollateralId { get; set; }
        public string CustomerId { get; set; }
        [Required]
        public decimal Amount { get; set; }
        public string CollateralCategory { get; set; }
        public string Venalvalue { get; set; }
        public string Marketvalue  { get; set; }
        public string Location { get; set; }
        public string Address { get; set; }
        public string Reference { get; set; }
        public virtual LoanProductCollateral LoanProductCollateral { get; set; }
        public virtual LoanApplication LoanApplication { get; set; }

    }
}
