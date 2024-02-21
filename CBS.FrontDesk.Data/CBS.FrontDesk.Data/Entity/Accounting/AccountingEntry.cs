using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
 
    public class AccountingEntry 
    {
        // Unique ID number for the entry=
        public string Id { get; set; }

        // Date the accounting entry was created
        public DateTime EntryDate { get; set; }
        // Effective date for posting the accounting impact
        public DateTime ValueDate { get; set; }
        // Type of entry (Debit or Credit)
        public string EntryType { get; set; }
        // Currency denomination 
        public string Currency { get; set; }
        // Text description explaining the purpose of the transaction
        public string Description { get; set; }
        // ID linking to source documents related to transaction
        public string ReferenceID { get; set; }
        // Status of workflow ( Posted,  Reversed)
        public string Status { get; set; }
        // Source system or module that generated the entry
        public string Source { get; set; }
        public string BankId { get; set; } // Related branch 
        public string BranchId { get; set; } // Related branch 
        public string DrAccountId { get; set; }
        public string DrAccountNumber { get; set; }
        public string CrAccountId { get; set; }
        public string CrAccountNumber { get; set; }
        public decimal DrAmount { get; set; }
        public decimal CrAmount { get; set; }
        public decimal CurrentBalance { get; set; }

        public string OperationType { get; set; }
        public string CrCurrentBalance { get; set; }
        public string DrCurrentBalance { get; set; }

        public void TagedAsReversed(List<AccountingEntry> entries, string userId)
        {
            foreach (var entry in entries)
                Status = "Reversed";
        }
    }

    public class AccountingEntryDto
    {
        // Unique ID number for the entry=
        //public string Id { get; set; }
        public string EntryDate { get; set; }
        public string Description { get; set; }
        public string TransactionReference { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string DebitAmount { get; set; }
        public string CreditAmount { get; set; }
        //public string CrCurrentBalance { get; set; }
        //public string DrCurrentBalance { get; set; }
   public string CurrentBalance { get; set; }
        public string CreditAccountBalance { get; set; }
        public string DebitAccountBalance { get; set; }
    }
}
