using System;
using System.ComponentModel.DataAnnotations;
namespace CBS.FrontDesk.UI.Controllers.Accounting_V2.TrialBalance
{
    public class TrialBalanceReportItem
    {
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }

        // 6-column TB raw values (per line)
        public decimal OpeningDebit { get; set; }
        public decimal OpeningCredit { get; set; }
        public decimal MovementDebit { get; set; }
        public decimal MovementCredit { get; set; }
        public decimal ClosingDebit { get; set; }
        public decimal ClosingCredit { get; set; }

        /// <summary>Net opening balance (Debit - Credit). Positive = debit, negative = credit.</summary>
        public decimal OpeningBalance { get; set; }

        /// <summary>Net closing balance (Debit - Credit). Positive = debit, negative = credit.</summary>
        public decimal ClosingBalance { get; set; }

        /// <summary>Total debits (OpeningDebit + MovementDebit).</summary>
        public decimal Debit { get; set; }

        /// <summary>Total credits (OpeningCredit + MovementCredit).</summary>
        public decimal Credit { get; set; }

        // 🔹 Per-line difference checks
        public decimal OpeningDifference { get; set; }      // OpeningDebit - OpeningCredit
        public decimal MovementDifference { get; set; }     // MovementDebit - MovementCredit
        public decimal ClosingDifference { get; set; }      // ClosingDebit - ClosingCredit
        public decimal TotalDifference { get; set; }        // Debit - Credit

        // 🔹 GLOBAL TOTALS (same values on every row, for footer use)
        public decimal TotalOpeningDebit { get; set; }
        public decimal TotalOpeningCredit { get; set; }
        public decimal TotalMovementDebit { get; set; }
        public decimal TotalMovementCredit { get; set; }
        public decimal TotalClosingDebit { get; set; }
        public decimal TotalClosingCredit { get; set; }

        public decimal TotalOpeningDifference { get; set; }   // ΣOpeningDR - ΣOpeningCR
        public decimal TotalMovementDifference { get; set; }  // ΣMovementDR - ΣMovementCR
        public decimal TotalClosingDifference { get; set; }   // ΣClosingDR - ΣClosingCR
        public decimal TotalGlobalDifference { get; set; }    // ΣDebit - ΣCredit

        public string Logo { get; set; } = string.Empty;

        public DateTime DayTime { get; set; }

        public string Address { get; set; }
        public string Mode { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; } = DateTime.Now.Date;

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime From { get; set; } = DateTime.Now.Date;

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime To { get; set; } = DateTime.Now.Date;

        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = "{0:hh\\:mm\\:ss}", ApplyFormatInEditMode = true)]
        public TimeSpan Time { get; set; } = DateTime.Now.TimeOfDay;

        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public string Username { get; set; }
        public string Phone { get; set; }
        public string Year { get; set; } = DateTime.Now.Year.ToString();
    }
}