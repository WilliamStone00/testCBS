using CBS.FrontDesk.Data.Entity.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
    public class AccountingEntryServiceResponse
    {
        public List<AccountingEntry > Data { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public List<string> Errors { get; set; }
    }
    public class AccountingEntryQuery
    {
        public AccountingEntryDto AccountingEntry { get; set; }
        public List<Branch> Branchs { get; set; }
        public SystemQuery SystemQuery { get; set; }

    }

    public class AccountEntry
    {
        public string Reference { get; set; }
        public string AccountNumber { get; set; }
        public string Description { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
    }
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
        public string AccountId { get; set; }
        public string OperationType { get; set; }
        public string CrCurrentBalance { get; set; }
        public string DrCurrentBalance { get; set; }
        public string AccountNumber { get; set; }
        public string AccountNumberReference { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedDate { get; set; }
        
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
        public string EntryDateTime { get; set; }
        public string EntryDate { get; set; }
        public string Description { get; set; }
        public string Reference { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string Debit { get; set; }
        public string Credit { get; set; }
        public string AccountId { get; set; }
        public string CurrentBalance { get; set; }
        public string CreditAccountBalance { get; set; }
        public string DebitAccountBalance { get; set; }
        public string ReferenceID { get; set; }
        public double DrAmount { get; set; }
        public double CrAmount { get; set; }
    }


    public class SystemQuery
    {

        public DateTime ToDate { get; set; }  
        public DateTime FromDate { get; set; }  
        public string FileType { get; set; }
        public string ReportType { get; set; }
        public List<string> AccountIds { get; set; }
        public string BranchId { get; set; }
    }

    public static class BSCartegory
    {

        public static string Assets = "Assets";
        public static string LIABILITIES = "Liability";
    }
    public class GLQuery
    {

        public string FileType { get; set; }
        public string BranchId { get; set; }
    }
    public class JEQuery
    {
        public DateTime ToDate { get; set; }
        public DateTime FromDate { get; set; }
        public string FileType { get; set; }
        public string BranchId { get; set; }
    }
    public class BSQuery
    {
        public string BranchId { get; set; }
        public DateTime Date { get; set; }
        public string DocumentId { get; set; }
    }
    public class TrialBalance4Column
    {
        public DateTime ToDate { get; set; }
        public DateTime FromDate { get; set; }
        public string FileType { get; set; }
        public string ReportType { get; set; }

        public string BranchId { get; set; }
    }
}