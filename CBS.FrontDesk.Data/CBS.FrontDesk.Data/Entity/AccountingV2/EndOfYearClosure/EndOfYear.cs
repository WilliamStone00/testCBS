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

      

        public string OperationCode { get; set; }
        public string Year { get; set; }
        public string AdjustmentType { get; set; }


        public string CounterpartyBranchId { get; set; } = null;

        public DateTime? AccountingDate { get; set; }

        public string PostMode { get; set; } 
        public string Narration { get; set; }
        //----------dtails------------------
        public string Reference { get; set; }
        public string State { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string Id { get; set; }

        public string CorrelationId { get; set; }
        public string ExternalApplicationName { get; set; }
        public string AuxiliaryReference { get; set; }

        public JournalPayloadEnd Payload { get; set; } = new JournalPayloadEnd();

        // Optional (not in JSON but kept for possible internal use)

     
        public string HOBranchId { get; set; }
        public string BranchLiaisonGlId { get; set; }
        public string HoLiaisonGlId { get; set; }
        public string HoResultGlId { get; set; }
        public string AccountingYear { get; set; }
   
    }

    public class JournalPayloadEnd
    {
        public string Memo { get; set; } 
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

        public string AdjustmentType { get; set; }

    }



  


    


    public class ReviewClosureRequest
    {
        public string AccountingYearId { get; set; }
        public string BranchId { get; set; }
        public List<ChecklistItem> CheckListModel { get; set; } = new List<ChecklistItem>();
        public string CompletedByUserId { get; set; } // optional
    }

    public class ChecklistItem
    {
        public string YearEndChecklistDefinitionId { get; set; }
        public bool IsCompleted { get; set; }
        public string Comment { get; set; }
    }



   

    public class CloseOfYear
    {
        public string BranchId { get; set; }
        public string HOBranchId { get; set; }
        public string BranchLiaisonGlId { get; set; }
        public string HoLiaisonGlId { get; set; }
        public string HoResultGlId { get; set ; }
        public string AccountingYear { get; set; }
        public DateTime AccountingDate { get; set; }

    }


    public class CloseYearInitiate
    {
        public string AccountingYearId { get; set; } 

        public string BranchId { get; set; }

        public string StartedByUserId { get; set; }
        public string StartedBy { get; set; }

        public string Remarks { get; set; }
    }


    public class YearClosureStatus
    {
        public Guid Id { get; set; }

        public string BranchId { get; set; }

        public string AccountingYearId { get; set; }

        public int Step { get; set; }

        public string Status { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime LastModifiedDate { get; set; }
    }



}
