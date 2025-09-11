using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.LoanConf
{
    public class Collateral
    {
        public string id { get; set; }
        [Required]
        public string name { get; set; }
        
    }


    public class Penalty
    {
        public string Id { get; set; }
        // 🔹 Unique identifier for the penalty configuration rule.

        // ================================
        // 📌 Basic Penalty Details
        // ================================

        public string PenaltyName { get; set; }
        // 🔹 Human-readable name for the penalty (e.g., "Late Fee – Personal Loan").

        public string Description { get; set; }
        // 🔹 Optional additional explanation (e.g., "Applies after 5 days overdue").

        public string PenaltyType { get; set; }
        // 🔹 Classification of penalty type.
        // 👉 E.g., "Late_Repayment_Penalty", "Penalty_After_Maturity_Date", etc.

        public decimal PenaltyValue { get; set; }
        // 🔹 The numeric value of the penalty to apply.
        // 👉 If IsRate = true → interpreted as a percentage (e.g., 2%).
        // 👉 If IsRate = false → treated as fixed amount (e.g., 5,000 FCFA).

        public bool IsRate { get; set; } = false;
        // 🔹 Determines if PenaltyValue is a rate (%) or a flat amount.

        public bool IsEnabled { get; set; } = true;
        // 🔹 Indicates if this rule is currently active.
        // 👉 Use false to deactivate without deleting.

        // ================================
        // ⚙️ Penalty Calculation Rules
        // ================================

        public string CalculatePenaltyOn { get; set; }
        // 🔹 Target base to apply penalty on.
        // 👉 Options: "Overdue_Principal_Amount", "Overdue_Interest_Amount", 
        //             "Overdue_Daily_Interest_Amount", "Overdue_Principal_Plus_Interest",
        //             "Apply_Fixed_Penalty_Amount".
        // 👉 Used to determine what part of the loan balance is penalized.

        public decimal? PenaltyMaxCapAmount { get; set; }
        // 🔹 Optional maximum limit for penalty on a loan.
        // 👉 Prevents excessive charges (e.g., cap at 50,000 FCFA).

        public bool IsCumulative { get; set; } = true;
        // 🔹 If true, penalty is applied repeatedly as per recurrence config.
        // 👉 If false, only applies once per overdue cycle (or per rule window).

        public bool IsOneTimeOnly { get; set; }
        // 🔹 If true, penalty is applied only once during the entire loan lifetime.
        // 👉 Used for exit charges or maturity-related penalties.

        // ================================
        // 📅 Application Timing & Frequency
        // ================================

        public int GracePeriodInDaysBeforeApplyingPenalty { get; set; }
        // 🔹 How many grace days after due date before penalty applies.
        // 👉 Helps accommodate client flexibility or grace period policy.

        public int DaysToApplyPenalty { get; set; }
        // 🔹 Days overdue before enforcing penalty.
        // 👉 Works together with grace period (e.g., grace = 3, apply after = 5).

        public bool ApplyOnWeekends { get; set; } = true;
        // 🔹 If false, penalty is not applied on Saturday or Sunday.
        // 👉 Consider true for daily interest-based penalties.

        public bool WaivePenaltyOnBranchHolidays { get; set; } = false;
        // 🔹 If true, penalty is not charged on defined branch holidays.
        // 👉 This logic can be omitted if holiday logic is removed.

        public string RecurringPeriod { get; set; }
        // 🔹 Unit of time for recurrence: "Days", "Weeks", "Months", or "Years".
        // 👉 Used only when IsCumulative = true.

        public string RecuringInterval { get; set; }
        // 🔹 Interval in unit (e.g., "30" with "Days" = every 30 days).
        // 👉 Must be a valid positive number.

        // ================================
        // ⏳ Expiry Logic
        // ================================

        public bool HasExpiryAfterDays { get; set; } = false;
        // 🔹 Determines if the rule should expire automatically after a duration.

        public int? ExpireAfterDays { get; set; }
        // 🔹 If set and HasExpiryAfterDays = true, rule expires after this number of days from first application.

        // ================================
        // 🧾 Accounting / Fee Posting
        // ================================

        public string FeePostingAccountId { get; set; }
        // 🔹 Optional GL account ID for posting penalty charges.
        // 👉 Useful if revenue must be separated or tracked in financial statements.
    }

    //Entry fees
    //OtherFee
    //Payees
    //Penalty
    //Payment Methods
    //HasPayee
}
