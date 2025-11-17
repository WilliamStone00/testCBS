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




    public class JournalDtoEntriesV2Dto
    {
        public DateTime AccountingDate { get; set; }
        public string Reference { get; set; }
        public string BranchId { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public decimal Dr { get; set; }
        public decimal Cr { get; set; }
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

        public decimal? OpeningDR { get; set; }
        public decimal? OpeningCR { get; set; }
        public decimal? PeriodDR { get; set; }
        public decimal? PeriodCR { get; set; }
        public decimal? ClosingDR { get; set; }
        public decimal? ClosingCR { get; set; }

        // Optional computed helpers
        public decimal OpeningBalance => (OpeningDR ?? 0) - (OpeningCR ?? 0);
        public decimal PeriodBalance => (OpeningDR ?? 0) - (OpeningCR ?? 0);
        public decimal ClosingBalance => (ClosingDR ?? 0) - (ClosingCR ?? 0);
    }


    public class CustomeResportItemS {

        public string BranchId { get; set; }
    }

}
