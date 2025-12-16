using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Base;
using System;
using System.Collections.Generic;

namespace CBS.FrontDesk.Data.Entity.Accounting_V2.TrialBalance
{
    public class GenericReportResponseV2Dto
    {
        // List of trial balance lines
        public List<TrialBalanceV2Dto> Lines { get; set; } = new List<TrialBalanceV2Dto>();
        public List<JournalDtoEntriesV2Dto> JournalEntries { get; set; } = new List<JournalDtoEntriesV2Dto>();
        public List<AccountStatementResponse> AccntStatements { get; set; } = new List<AccountStatementResponse>();


        // Response metadata
    }



    public class AccountStatementResponse
    {
      
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public decimal OpeningBalance { get; set; }
        public List<JournalDtoEntriesV2Dto> Movements { get; set; }
        public decimal ClosingBalance { get; set; }
    }


    public class ReceiptDto
    {
        public string ReceiptId { get; set; } = null;
        public string JournalHeaderId { get; set; } = null;
        public string Reference { get; set; } = null;
        public string ReceiptNumber { get; set; } = null;
        public string Lang { get; set; } = "en";
        public bool FromTemp { get; set; } = true;
    }




    //======================== journal reciepts respone ==========================
    public class JournalReceiptsResponse
    {
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public decimal OpeningBalance { get; set; }
        public List<JournalDtoEntriesV2Dto> Movements { get; set; }
        public decimal ClosingBalance { get; set; }
    }


    public class JournalDtoEntriesV2Dto
    {

        public DateTime AccountingDate { get; set; }
        public string Reference { get; set; }
        public string BranchId { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public decimal DR { get; set; }
        public decimal CR { get; set; }
        public string Narration { get; set; }
        public string DrCr { get; set; }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
        public int Seq { get; set; }
        public string AuxiliaryRef { get; set; }
        public DateTime EntryDate { get; set; }
        public string UserName { get; set; }
        public bool InterbranchStatus { get; set; }
        public string CounterpartyBranchId { get; set; }
        public string TimeOfOperation { get; set; }




        // ─────────────────────────────────────────────────────────────
        // Per-line and aggregate totals
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Per-line difference (DR - CR).
        /// Useful when you want to see which side this line contributes to.
        /// </summary>
        public decimal RowDifference => DR - CR;

        /// <summary>
        /// Sum of all debit amounts in the journal result set
        /// (same value populated on every row by the query helper).
        /// </summary>
        public decimal TotalDR { get; set; }

        /// <summary>
        /// Sum of all credit amounts in the journal result set
        /// (same value populated on every row by the query helper).
        /// </summary>
        public decimal TotalCR { get; set; }

        /// <summary>
        /// Difference between total debits and total credits (TotalDR - TotalCR).
        /// In a balanced journal this should be zero.
        /// </summary>
        public decimal TotalDifference { get; set; }

        public decimal OpeningBalance { get; set; }
        public decimal EndingBalance { get; set; }

    }

    public class TrialBalanceV2Dto
    {
        // Reference types (always nullable in C# 7)
        public BranchInfo BranchInfo { get; set; } = null;
        public BranchAccountInfo BranchAccount { get; set; } = null;



        // Value types made nullable with ?
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string Sign { get; set; }

        public decimal OpeningDR { get; set; }
        public decimal OpeningCR { get; set; }
        public decimal PeriodDR { get; set; }
        public decimal PeriodCR { get; set; }
        public decimal ClosingDR { get; set; }
        public decimal ClosingCR { get; set; }

        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal EndingBalance { get; set; }
        public decimal ClosingBalanceFour { get; set; }
        public decimal BeginningBalance { get; set; }
        public string BeginningSide { get; set; } = "D"; // "D" or "C"


        public string EndingSide { get; set; } = "D"; // "D" or "C"
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }

        // 🔹 Global totals (mirroring your JSON: totalOpeningDR, ...)
        public decimal TotalOpeningDR { get; set; }
        public decimal TotalOpeningCR { get; set; }
        public decimal TotalMovementDR { get; set; }
        public decimal TotalMovementCR { get; set; }
        public decimal TotalClosingDR { get; set; }
        public decimal TotalClosingCR { get; set; }

        public decimal TotalOpeningDifference { get; set; }
        public decimal TotalMovementDifference { get; set; }
        public decimal TotalClosingDifference { get; set; }
    }


    public class CustomeResportItemS {

        public string BranchId { get; set; }
    }








    //======================== trial balance columns  ==========================


    public class TrialBalanceFourItemDto
    {
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public decimal BeginningBalance { get; set; }
        public string BeginningSide { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal EndingBalance { get; set; }
        public string EndingSide { get; set; }

        public decimal TotalBeginningBalance { get; set; }
        public string TotalBeginningSide { get; set; }
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal TotalEndingBalance { get; set; }
        public string TotalEndingSide { get; set; }
        public decimal TotalBeginningNet { get; set; }
        public decimal TotalEndingNet { get; set; }
        public decimal TotalMovementNet { get; set; }
    }


}
