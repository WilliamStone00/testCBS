using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.AccountingV2.EndOfYearClosure
{
    public class EndOfYear
    {

        public string AccountingYearId { get; set; }
        public string BranchId { get; set; }
        public string StartedByUserId { get; set; }
        public string StartedBy { get; set; }
        public string Remarks { get; set; }

        public string AdjustmentType { get; set; }

        public string OperationCode { get; set; }

  

        public string CounterpartyBranchId { get; set; } = null;

        public DateTime? AccountingDate { get; set; }

        public string PostMode { get; set; } = "HOLD_FOR_APPROVAL";
        public string Narration { get; set; }
        //----------dtails------------------
        public string Reference { get; set; }
        public string State { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }


        public JournalPayloadEnd Payload { get; set; } = new JournalPayloadEnd();

        // Optional (not in JSON but kept for possible internal use)
        public string OperationType { get; set; }
        public string WorkTicket { get; set; }
        public string Id { get; set; }
        public string BranchName { get; set; }
        public string CorrelationId { get; set; }
        public string ExternalApplicationName { get; set; }
        public string AuxiliaryReference { get; set; } = "AUX-IB-RECLASS-10";
    }

    public class JournalPayloadEnd
    {
        public string Memo { get; set; } = "Manual interbranch treatment (notify destination for completion";
        public bool AllowUnbalanced { get; set; } = false;
        public List<JournalEntryLineEnd> Entries { get; set; } = new List<JournalEntryLineEnd>();
    }

    public class JournalEntryLineEnd
    {
        public string AffiliateAccountId { get; set; }

        public string Naration { get; set; }

        public bool Dr { get; set; }
        public bool Cr { get; set; }
        public decimal Amount { get; set; }
    }



    public class EndOfYearTask
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsMandatory { get; set; }
        public bool IsActive { get; set; }
        public string Category { get; set; }

        public string Comment { get; set; }
        public string AccountingYearId { get; set; }
      


    }


    public class EndOfYearTaskViewModel
    {
        public List<EndOfYearTask> Tasks { get; set; } = new List<EndOfYearTask>();
       
    }


    public class YearEndChecklistDefinitionQuery
    {
        public DataTableOptions Options { get; set; }
        public YearEndChecklistDefinitionQuery() { Options = new DataTableOptions(); }

        public string BranchId { get; set; } = null;
        public string AccountingYearId { get; set; }

        //public string OperationCode { get; set; }
        //public string Reference { get; set; } = null;
        //public DateTime? StartAccountingDate { get; set; }
        //public DateTime? EndAccountingDate { get; set; }
        //public DateTime? StartDate { get; set; }    // For filtering creation date range
        //public DateTime? EndDate { get; set; }
        //public string TicketSource { get; set; }
    }





}
