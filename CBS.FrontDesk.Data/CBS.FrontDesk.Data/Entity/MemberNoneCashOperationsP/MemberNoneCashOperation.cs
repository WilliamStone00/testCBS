using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.MemberNoneCashOperationsP
{
    public class MemberNoneCashOperation 
    {
        public string Id { get; set; }
        public string MemberReference { get; set; }
        public string AccountNUmber { get; set; }
        public string ChartOfAccountGLId { get; set; }
        public decimal Amount { get; set; }
        public string Note { get; set; }
        public string InitiatedByUSerId { get; set; }
        public string OperationType { get; set; }//Deposit Or Withdrawal
        public string InitiatedUserName { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime AccountingDate { get; set; }
        public string ApprovedByUSerId { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime ApprovalDate { get; set; }
        public string ApprovalStatus { get; set; }
        public string MemberName { get; set; }
        public string Source { get; set; }
        public string ApprovalComment { get; set; }
        public string BranchId { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public string AccountId { get; set; }
        public string TransactionReference { get; set; }
    }
    public class AddMemberNoneCashOperationCommand
    {
        public string MemberReference { get; set; }
        public string AccountNUmber { get; set; }
        public decimal Amount { get; set; }
        public string Note { get; set; }
        public string ChartOfAccountId { get; set; }
        public string BookingDirection { get; set; }
        public string MemberName { get; set; }
    }
    public class ValidateMemberNoneCashOperationCommand
    {
        public string OperationId { get; set; }
        public bool IsApproved { get; set; }
        public string ValidationComment { get; set; }
    }
    public class GetAllMemberNoneCashOperationsQuery
    {
        public string Status { get; set; }
        public string BranchId { get; set; }

        public GetAllMemberNoneCashOperationsQuery(string status, string branchId = null)
        {
            Status = status;
            BranchId = branchId;
        }
    }
}
