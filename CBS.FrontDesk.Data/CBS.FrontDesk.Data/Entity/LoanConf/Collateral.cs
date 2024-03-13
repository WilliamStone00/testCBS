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
        public string Id { get; set; }
        public string LoanProductId { get; set; }
        [Required]
        public string PenaltyType { get; set; }//Late_Repayment_Penalty Or Penalty_After_Maturity_Date
        public string Description { get; set; }
        [Required]
        public string PenaltyName { get; set; }
        [Required]
        public decimal PenaltyValue { get; set; }
        public bool IsRate { get; set; }
        public bool IsEnabled { get; set; }
        [Required]
        public string CalculatePenaltyOn { get; set; }//Overdue_Principal_Amount,Overdue_Interest_Amount,Overdue_Principal_Plus_Interest_Plus_Amount,
        public bool WaivePenaltyOnBranchHolidays { get; set; }
        public int GracePeriodInDaysBeforeApplyingPenalty { get; set; }
        public string RecuringInterval { get; set; }// Every 1 to 365 Days
        public string RecurringPeriod { get; set; }//Days, Months, Years, Weeks
        public int DaysToApplyPenalty { get; set; }
        public LoanProduct LoanProduct { get; set; }
    }

    //Entry fees
    //OtherFee
    //Payees
    //Penalty
    //Payment Methods
    //HasPayee
}
