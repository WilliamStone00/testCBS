using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.LoanConf
{
    public class Loan
    {
        // Unique identifier for the loan
        public string id { get; set; }

        // Identifier referencing the loan application
        public string loanApplicationId { get; set; }

        // Total loan amount
        public double amount { get; set; }

        // Interest rate associated with the loan
        public double interestRate { get; set; }

        // Accumulated interest on the loan
        public double Interest { get; set; }

        // Total amount refunded
        public double totalRefunded { get; set; }

        // Remaining balance on the loan
        public double balance { get; set; }

        // Grace period allowed before late fees are applied
        public int GracePeriodOfLateFees { get; set; }

        // Date when the loan amount was disbursed
        public DateTime disbursementDate { get; set; }

        // Identifier referencing the customer associated with the loan
        public string customerid { get; set; }

        // Flag indicating if the loan has a predefined repayment schedule
        public bool isLoanSchedule { get; set; }

        // Flag indicating if the loan is active
        public bool isCurrentLoan { get; set; }

        // Date when the loan matures
        public DateTime maturityDate { get; set; }

        // Identifier referencing the organization associated with the loan
        public string organizationId { get; set; }

        // Identifier referencing the branch associated with the loan
        public string branchId { get; set; }

        // Identifier referencing the bank associated with the loan
        public string bankId { get; set; }

        // Identifier referencing the loan officer handling the loan
        public string LoanOfficerId { get; set; }

        // Flag indicating if the loan has been written off
        public bool WrittenOff { get; set; }

        // Flag indicating if the loan is considered a "bad loan"
        public bool BadLoan { get; set; }

        // Penalty based on overdue principal amount
        public double NonRepaymentPenaltiesBasedOnOverduePrincipal { get; set; }

        // Penalty based on the initial loan amount
        public double NonRepaymentPenaltiesBasedOnInitialAmount { get; set; }

        // Penalty based on the outstanding loan balance
        public double NonRepaymentPenaltiesBasedOnOlb { get; set; }

        // Total anticipated repayment penalties
        public double AnticipatedTotalRepaymentPenalties { get; set; }

        // Penalty based on overdue interest
        public double NonRepaymentPenaltiesBasedOnOverdueInterest { get; set; }

        // Anticipated partial repayment penalties
        public double AnticipatedPartialRepaymentPenalties { get; set; }

        // Number of drawings/transactions under a line of credit
        public int NumberOfDrawingsLineOfCredit { get; set; }

        // Total amount utilized under a line of credit
        public double AmountUnderLineOfCredit { get; set; }

        // Maturity date for a line of credit
        public DateTime MaturityLineOfCredit { get; set; }

        // Anticipated partial repayment base
        public double AnticipatedPartialRepaymentBase { get; set; }

        // Anticipated total repayment base
        public double AnticipatedTotalRepaymentBase { get; set; }

        // Flag indicating if the loan schedule has changed
        public bool ScheduleChanged { get; set; }

        // Flag indicating if insurance is applied to the loan
        public bool Insurance { get; set; }

        // Effective interest rate on the loan
        public double EffectiveInterestRate { get; set; }
    }

}
