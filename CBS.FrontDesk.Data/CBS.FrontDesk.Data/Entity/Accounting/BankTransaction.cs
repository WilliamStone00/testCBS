using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
    public class BankTransaction
    {
        public string Id { get; set; }
        public string AccountId { get; set; }
        public string Balance { get; set; }
        public decimal Amount { get; set; }
        public string ReferenceId { get; set; }
        public string TransactionType { get; set; }
        public string BankTransactionReference { get; set; }
        public string Description { get; set; }
        public string FileUpload { get; set; }
        public string ValueDate { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedDate { get; set; }
        
             public string BranchId { get; set; }
    }
}
