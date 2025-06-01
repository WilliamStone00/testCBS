

using CBS.FrontDesk.Data.Entity.Config;

namespace CBS.FrontDesk.Data.Entity.BulkOperation
{
    public class SimulateBulkOperationToSpecificAccountType
    {
        public string SourceAccountType { get; set; }
        public string StartAccount { get; set; }
        public string EndAccount { get; set; }
        public string DestinationAccountType { get; set; }
        public decimal TargetAmount { get; set; }
        public decimal SourceAccountMinAmount { get; set; }
        public decimal SourceAccountMaxAmount { get; set; }
        public string Description { get; set; }
        public string SimulationType { get; set; }
        public string BankCode { get; set; }
        public string BranchCode { get; set; }
        public string BranchId { get; set; }
        public string BankId { get; set; }
        public string BankName { get; set; }
        public string BranchName { get; set; }
        public string EventCode { get; set; }

        public SimulateBulkOperationToSpecificAccountType(SimulateBulkOperation simulate,string eventCode,Branch branch)
        {
            BankCode = branch.Bank.BankCode;
            BranchCode = branch.BranchCode;
            BranchId = branch.Id;
            BankId = branch.Bank.Id;
            BankName = branch.Bank.Name;
            BranchName = branch.Name;
            SimulationType = simulate.SimulationType;
            SourceAccountMaxAmount = simulate.SourceAccountMaxAmount;
            Description = simulate.Description;
            DestinationAccountType = simulate.DestinationAccountType;
            SourceAccountMinAmount = simulate.SourceAccountMinAmount;
            SourceAccountType = simulate.SourceAccountType;
            EndAccount = simulate.EndAccount;
            StartAccount = simulate.StartAccount;
            TargetAmount = simulate.TargetAmount;
            EventCode=eventCode;
        }
    }
}
