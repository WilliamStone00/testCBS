using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.AccountingV2
{




    public class JournalEntry
    {
        public string OperationCode { get; set; }

        public string BranchId { get; set; }

        public string CounterpartyBranchId { get; set; } = null;

        public DateTime AccountingDate { get; set; }

        public string PostMode { get; set; }
        public string Narration { get; set; }
        //----------dtails------------------
        public string Reference { get; set; }
        public string State { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }


        public JournalPayload Payload { get; set; } = new JournalPayload();

        // Optional (not in JSON but kept for possible internal use)
        public string OperationType { get; set; }
        public string WorkTicket { get; set; }
        public string Id { get; set; }
        public string BranchName { get; set; }
        public string CorrelationId { get; set; }
        public string ExternalApplicationName { get; set; }
    }

    public class JournalPayload
    {
        public string Memo { get; set; }
        public bool AllowUnbalanced { get; set; } = false;
        public List<JournalEntryLine> Entries { get; set; } = new List<JournalEntryLine>();
    }

    public class JournalEntryLine
    {
        public string AffiliateAccountId { get; set; }

        public string Naration { get; set; }

        public bool Dr { get; set; }
        public bool Cr { get; set; }
        public decimal Amount { get; set; }
    }
    //public class JournalEntry
    //{
    //    public string Id { get; set; }
    //    public string OperationCode { get; set; }
    //    public string BranchId { get; set; }
    //    public string BranchName { get; set; }
    //    public string Reference { get; set; }
    //    public string CounterpartyBranchId { get; set; } // optional
    //    public DateTime AccountingDate { get; set; }
    //    public string PostMode { get; set; }
    //    public string CorrelationId { get; set; }
    //    public string Narrative { get; set; } // matches JSON "narrative"
    //    public string ExternalOperationType { get; set; } // maps ExternalApplicationName in some examples
    //    public JournalPayload Payload { get; set; } // optional
    //    public string Stage { get; set; } // Temp / Reconciled
    //    public bool IsBalanced { get; set; }
    //    public decimal TotalDebit { get; set; }
    //    public decimal TotalCredit { get; set; }
    //    public List<JournalEntryLine> Lines { get; set; } = new List<JournalEntryLine>();
    //    public List<JournalEntryLine> TempRLines { get; set; } = new List<JournalEntryLine>();
    //    public DateTime CreatedAt { get; set; }
    //    public DateTime? UpdatedAt { get; set; }
    //}

    //public class JournalPayload
    //{
    //    public string Memo { get; set; }
    //    public bool AllowUnbalanced { get; set; }
    //    public List<JournalEntryLine> Entries { get; set; } = new List<JournalEntryLine>();
    //}

    //public class JournalEntryLine
    //{
    //    public string AffiliateAccountId { get; set; }
    //    public string Naration { get; set; } // keep spelling consistent with your JSON
    //    public bool Dr { get; set; }
    //    public bool Cr { get; set; }
    //    public decimal Amount { get; set; }
    //}

    public class JournalEntryQuery
    {
        public DataTableOptions Options { get; set; }
        public JournalEntryQuery() { Options = new DataTableOptions(); }

        public string BranchId { get; set; } = null;
        public string OperationCode { get; set; }
        public string Reference { get; set; } = null;
        public DateTime? StartAccountingDate { get; set; }
        public DateTime? EndAccountingDate { get; set; }
        public DateTime? StartDate { get; set; }    // For filtering creation date range
        public DateTime? EndDate { get; set; }
    }


    public class AccountDto
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string AffiliateAccountId { get; set; }
        public string AffiliateAccountName { get; set; }
        public string Name { get; set; }
        public string Class { get; set; }
        public bool PostingAllowed { get; set; }
        public int Depth { get; set; }
        public string Path { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsDeleted { get; set; }
        public List<AccountDto> Children { get; set; }
    }


}

