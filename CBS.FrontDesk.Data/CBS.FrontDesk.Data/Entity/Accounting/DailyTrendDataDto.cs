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
}
