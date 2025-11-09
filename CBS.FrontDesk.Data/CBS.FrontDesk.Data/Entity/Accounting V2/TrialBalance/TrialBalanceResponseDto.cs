using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Base;
using System;
using System.Collections.Generic;

namespace CBS.FrontDesk.Data.Entity.Accounting_V2.TrialBalance
{
    public class TrialBalanceResponseDto
    {
        // List of trial balance lines
        public List<TrialBalanceDto> Lines { get; set; } = new List<TrialBalanceDto>();

        // Response metadata
  
    }

    public class TrialBalanceDto
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
}
