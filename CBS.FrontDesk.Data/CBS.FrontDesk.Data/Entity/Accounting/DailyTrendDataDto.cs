using System;
using System.Collections.Generic;

namespace CBS.FrontDesk.Data
{

    public class AccountBalanInitConfiguration
    {
        public InitInfoDto InitInfoDto { get; set; }
        public List<AccountInitDto> AccountInitDtos { get; set; }
    }
    public class GetInfoDto
    {
        public string BranchId { get; set; }
        public string ProductId { get; set; }
    }
    public class InitInfoDto
    {
        public string ScenarioId { get; set; }
        public string ProductId { get; set; }

        public decimal SubmittedMemberBalance { get; set; }
        public decimal SubmittedGLBalance { get; set; }

        public string BranchId { get; set; }
    }
    public class AccountInitDto
    {
        public string Id { get; set; }
        public string ScenarioId { get; set; }
        public string ProductType { get; set; }
        public string ProductId { get; set; }
        public string AccountType { get; set; }

        public decimal SubmittedMemberBalance { get; set; }
        public decimal ActualMemberBalance { get; set; }
        public decimal SubmittedGLBalance { get; set; }
        public decimal ActualGLBalance { get; set; }
        public decimal Difference { get; set; }

        public string TargetGLAccount { get; set; }
        public string SuspenseGLAccount { get; set; }
        public string Narration { get; set; }

        public string SubmittedBy { get; set; }
        public DateTime SubmittedAt { get; set; }

        public string BranchId { get; set; }
        public string BranchCode { get; set; }
        public string BranchName { get; set; }
    }

    public class ZeroOutMemberBalanceSimulationDto
    {
        public string Id { get; set; }
        public string MemberName { get; set; }
        public string MemberReference { get; set; }
        public string AccountNumber { get; set; }
        public string AccountType { get; set; }
        public decimal Balance { get; set; }
        public decimal AmountToCredit { get; set; }
        public string BranchId { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public decimal TotalBalance { get; set; }
        public decimal NetBalance { get; set; }
        public string ZeroOutMemberBalanceId { get; set; }
        public string TransferStatus { get; set; }
        public string TransferMessage { get; set; }
        public DateTime TransferDate { get; set; }
        public string ApprovalStatus { get; set; }
        public DateTime ApprovalDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
    }
    //public class ZeroOutMemberBalanceSimulationDto
    //{
    //    public string Id { get; set; }
    //    public string MemberName { get; set; }
    //    public string MemberReference { get; set; }
    //    public string AccountNumber { get; set; }
    //    public string AccountType { get; set; }
    //    public decimal Balance { get; set; }
    //    public decimal AmountToCredit { get; set; }
    //    public string BranchId { get; set; }
    //    public string BranchName { get; set; }
    //    public string BranchCode { get; set; }
    //    public decimal TotalBalance { get; set; }
    //    public decimal NetBalance { get; set; }
    //    public string ZeroOutMemberBalanceId { get; set; }
    //    public string TransferStatus { get; set; }
    //    public string TransferMessage { get; set; }
    //    public DateTime TransferDate { get; set; }
    //    public string ApprovalStatus { get; set; }
    //    public DateTime ApprovalDate { get; set; }
    //    public DateTime CreatedDate { get; set; }
    //    public string CreatedBy { get; set; }

    //    public virtual ICollection<ZeroOutMemberBalanceSimulationDetailDto> ZeroOutMemberBalanceSimulationDetails { get; set; }
    //}

    public class ZeroOutMemberBalanceSimulationDetailDto
    {
        public string Id { get; set; }
        public string MemberName { get; set; }
        public string MemberReference { get; set; }
        public string AccountNumber { get; set; }
        public string AccountType { get; set; }
        public decimal Balance { get; set; }
        public decimal AmountToCredit { get; set; }
        public string BranchId { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public decimal TotalBalance { get; set; }
        public decimal NetBalance { get; set; }
        public string ZeroOutMemberBalanceId { get; set; }
        public string TransferStatus { get; set; }
        public string TransferMessage { get; set; }
        public DateTime TransferDate { get; set; }
        public string ApprovalStatus { get; set; }
        public DateTime ApprovalDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
    }
    public class ZeroOutMemberBalanceSimulationCommand
    {
        public string AccountType { get; set; }
        public string StartAccount { get; set; }
        public string EndAccount { get; set; }
        public string Description { get; set; }
        public string BankCode { get; set; }
        public string BranchCode { get; set; }
        public string BranchId { get; set; }
        public string BankId { get; set; }
        public string  BankName { get; set; }
        public string BranchName { get; set; }
        public string CreatedBy { get; set; }
    }
    public class ZeroOutMemberBalanceConfirmationCommand
    {
        public string SimulationId { get; set; }
        public string ApprovalStatus { get; set; }
        public string ApprovalStatusDescription { get; set; }
        public string ApprovalBy { get; set; }
    }
}
