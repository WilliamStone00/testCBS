using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.LoanConf
{
    public class Collateral
    {
        public string id { get; set; }
        [Required]
        public string name { get; set; }
        [Required]
        public string description { get; set; }
    }
    public class LoanCollateral
    {
        public string id { get; set; }
        [Required]
        public string collateralId { get; set; }
        [Required]
        public string loanApplicationId { get; set; }
        [Required]
        public string value { get; set; }
    }

    public class Penalty
    {
        public string id { get; set; }
        [Required]
        public string penaltyType { get; set; }
        [Required]
        public string penaltyName { get; set; }
        [Required]
        public string description { get; set; }
        public double flateAmount { get; set; }
        public bool isRate { get; set; }
        public double percentage { get; set; }
        public int daysToApplyPenalty { get; set; }
        public bool isEnabled { get; set; }
        public string accountingRuleId { get; set; }
        public string organizationId { get; set; }
        public string bankId { get; set; }
        public string branchId { get; set; }
    }

    //Entry fees
    //OtherFee
    //Payees
    //Penalty
    //Payment Methods
    //HasPayee
}
