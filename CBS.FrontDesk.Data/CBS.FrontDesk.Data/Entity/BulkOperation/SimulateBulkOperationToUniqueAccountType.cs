using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.BulkOperation
{
    public class SimulateBulkOperationToUniqueAccountType
    {


        public string SourceAccountType { get; set; }
        public string StartAccount { get; set; }
        public string EndAccount { get; set; }
        public string DestinationAccountId { get; set; }
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


        public SimulateBulkOperationToUniqueAccountType(SimulateBulkOperation simulate)
        {
            BankCode = simulate.BankCode;
            BranchCode = simulate.BranchCode;
            BranchId = simulate.BranchId;
            BankId = simulate.BankId;
            BankName = simulate.BankName;
            BranchName = simulate.BranchName;
            SimulationType = simulate.SimulationType;
            SourceAccountMaxAmount = simulate.SourceAccountMaxAmount;
            Description = simulate.Description;
            DestinationAccountId = simulate.DestinationAccountId;
            SourceAccountMinAmount = simulate.SourceAccountMinAmount;
            SourceAccountType = simulate.SourceAccountType;
            EndAccount = simulate.EndAccount;
            StartAccount = simulate.StartAccount;
            TargetAmount = simulate.TargetAmount;
        }



    }
}
