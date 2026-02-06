using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.BulkOperation
{
    public class SimulateCashOutOrCashInBulkOperation
    {
        // Form fields
        public string AccountCartId { get; set; }
        public DateTime AccountDate { get; set; }
        public string TransferType { get; set; }
        public string OperationTitle { get; set; }
        public string SimulationDescription { get; set; }
        public string InputMethod { get; set; }
        public string OperationScope { get; set; }
        public string SimulationType { get; set; }
        public string BranchId { get; set; }
        public string BulkCashStrategy { get; set; }  
        
        public string AccountCartId2 { get; set; }
        public DateTime AccountDate2 { get; set; }
        public string TransferType2 { get; set; }
        public string OperationTitle2 { get; set; }
        public string SimulationDescription2 { get; set; }
        public string InputMethod2 { get; set; }
        public string OperationScope2 { get; set; }
        public string SimulationType2 { get; set; }
        public string BranchId2 { get; set; }
        public string BulkCashStrategy2 { get; set; }
        public string FileUploadId { get; set; }
        public bool ShouldImpactAccounting { get; set; } = true;

        // Collections
        public List<Branch> Branches { get; set; } = new List<Branch>();
        public List<BulkOperationAccount> Accounts { get; set; } = new List<BulkOperationAccount>();
        public List<BulkOperationAccount2> AccountIIs { get; set; } = new List<BulkOperationAccount2>();
    }



    public class BulkOperationAccount
    {
        public string BranchName { get; set; }
        //public string BranchId { get; set; }
        public string MemberReference { get; set; }
        public string AccountType { get; set; }
       // public string AccountTypeId { get; set; }
        public decimal Amount { get; set; }
    }  
    
    public class BulkOperationAccount2
    {
        public string BranchCode { get; set; }
        public string MemberReference { get; set; }
        public string AccountType { get; set; }
        public decimal Amount { get; set; }  // Using decimal for financial values
    }

}
