using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting_V2.Reconciliation
{
    public class Reconciliation
    {
    }

    public class MemberAccountdrop
    {
        public string BranchCode { get; set; }
        public string Status { get; set; }
        public bool IncludeOnlyActive { get; set; }
    }

    public class dropdownResposne
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class LoanReconciliationDto
    {
        public string Id { get; set; }
        public string LoanApplicationId { get; set; }
        public decimal Principal { get; set; }
        public decimal LoanAmount { get; set; }
        public decimal InterestForcasted { get; set; }
        public decimal InterestRate { get; set; }
        public decimal LastPayment { get; set; }
        public decimal Paid { get; set; }
        public decimal Balance { get; set; }
        public decimal DueAmount { get; set; }
        public decimal RequestedAmount { get; set; }
        public decimal RestructuredBalance { get; set; }
        public decimal AccrualInterest { get; set; }
        public decimal LastCalculatedInterest { get; set; }
        public decimal AccrualInterestPaid { get; set; }
        public decimal TotalPrincipalPaid { get; set; }
        public decimal Tax { get; set; }
        public decimal TaxPaid { get; set; }
        public decimal FeePaid { get; set; }
        public decimal Fee { get; set; }
        public decimal Penalty { get; set; }
        public decimal PenaltyPaid { get; set; }
        public DateTime? DisbursementDate { get; set; }
        public DateTime? FirstInstallmentDate { get; set; }
        public DateTime? NextInstallmentDate { get; set; }
        public DateTime? LoanDate { get; set; }
        public bool IsLoanDisbursted { get; set; }
        public string DisbursmentStatus { get; set; }
        public DateTime? LastInterestCalculatedDate { get; set; }
        public DateTime? LastRefundDate { get; set; }
        public DateTime? LastEventData { get; set; }
        public string CustomerId { get; set; }
        public string LoanManager { get; set; }
        public string LoanStatus { get; set; }
        public bool IsRestructured { get; set; }
        public string NewLoanId { get; set; }
        public bool IsWriteOffLoan { get; set; }
        public bool IsDeliquentLoan { get; set; }
        public bool IsCurrentLoan { get; set; }
        public DateTime? MaturityDate { get; set; }
        public string OrganizationId { get; set; }
        public string BranchId { get; set; }
        public string BankId { get; set; }
        public string LoanType { get; set; }
        public bool IsUpload { get; set; }
        public string BranchCode { get; set; }
        public string LoanId { get; set; }
        public string CustomerName { get; set; }
        public int LoanDuration { get; set; }
    }

    public class GetBalance 
    {
        public string Balance { get; set; }
        public string AccountBalance { get; set; }
        public string mode { get; set; }
        public string accountTypeId { get; set; }
        public bool loan { get; set; }
        public bool member { get; set; }
        public string BranchAccount { get; set; }
        public string BranchId { get; set; }
        public string LoanAccountType { get; set; }
        public string MemberAccountType { get; set; }
    }
}
