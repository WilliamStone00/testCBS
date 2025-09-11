using CBS.FrontDesk.Data.Entity.LoanConf;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Overdraft
{
    public class OverdraftFacilityConfig : BaseEntity
    {
        public string Id { get; set; } // Unique ID for the overdraft configuration (typically a GUID or MongoDB ObjectId)

        // ==============================
        // ✅ Eligibility & Product Binding
        // ==============================
        public string Name { get; set; }

        public string ProductId { get; set; }
        // The ID of the main account product this config applies to (e.g., Salary or Current account).

        public bool IsSalaryAccountProduct { get; set; } = false;
        // Marks this as a salary-based product (used for applying salary-specific overdraft rules).

        public bool RequireSalaryAccountLink { get; set; }
        // Ensures that only accounts with a linked salary account are eligible for overdraft.

        public int MinimumCreditScoreRequired { get; set; }
        // Minimum credit score required to activate overdraft for this product.

        public decimal MaxOverdraftAsPercentageOfSalary { get; set; } = 100;
        // Internal policy cap: e.g., limit overdraft to 75% of average salary over last 3 months.

        public decimal MaxPercentageAllowedByState { get; set; }
        // Regulatory cap set by national authority: e.g., law may restrict OD to max 50% of salary.

        // ==============================
        // ✅ Limit & Pricing Rules
        // ==============================

        public decimal DefaultOverdraftLimit { get; set; }
        // Base overdraft amount assigned to new accounts under this product (e.g., 50,000 FCFA).

        public decimal DefaultOverdraftFeeRate { get; set; }
        // Annual or monthly interest rate charged on overdraft balance (e.g., 3.5%).

        public bool EnableTieredInterestRate { get; set; } = false;
        // Whether interest varies based on usage brackets (enabled = true if `InterestTiers` used).

        public virtual ICollection<OverdraftInterestTier> InterestTiers { get; set; }
        // List of interest tiers (e.g., 1–50% usage = 2%, 51–100% = 5%).

        public decimal MaximumUtilizationPercentage { get; set; } = 90;
        // Defines how much of the OD limit can be used (e.g., restrict to 90% of approved limit).

        // ==============================
        // ✅ Disbursement & Usage Access
        // ==============================

        public bool AutoDisburseToLinkedAccount { get; set; }
        // If true, overdraft gets disbursed to a linked account automatically (e.g., on salary date).

        public bool AllowPartialDraw { get; set; }
        // Allows the user to draw only part of the overdraft instead of full amount at once.

        public bool AllowExceedingLimitWithFee { get; set; }
        // If enabled, user can exceed the OD limit temporarily but pays `ExceedLimitFeeRate`.

        public decimal ExceedLimitFeeRate { get; set; }
        // Fee/interest charged on the excess amount used beyond the overdraft limit.

        public bool StopTransactionsWhenOverLimit { get; set; }
        // If true, blocks further transactions when the OD limit is exceeded (strict enforcement).

        // ==============================
        // ✅ Tenure & Expiry Controls
        // ==============================

        public int MaximumOverdraftTenureInDays { get; set; }
        // Maximum duration the overdraft can stay active before it expires (e.g., 180 days).

        public bool HasExpiry { get; set; } = false;
        // Whether the overdraft should have a fixed expiry date.

        public bool IsRenewable { get; set; } = true;
        // Whether the overdraft can be renewed upon expiry.

        // ==============================
        // ✅ Repayment & Recovery
        // ==============================

        public bool AutoRecoverOnDeposit { get; set; }
        // If enabled, overdraft is repaid automatically when customer receives deposits.

        public bool AllowManualRepayment { get; set; }
        // Allows customer to initiate repayment manually (via mobile, teller, etc.).

        public int GracePeriodInDays { get; set; }
        // Number of days before recovery or interest accrual starts after OD is used.

        // ==============================
        // ✅ Replenishment Logic
        // ==============================

        public int AutoReplenishmentFrequencyInDays { get; set; }
        // How often the available OD limit resets or is refreshed (e.g., every 30 days).

        // ==============================
        // ✅ Top-Up Request & Fee Management
        // ==============================

        public bool AllowTopUpRequest { get; set; }
        // Enables customer to request an increase in the OD limit.

        public decimal MaxTopUpLimit { get; set; }
        // Cap on how much extra the customer can request as a top-up.

        public decimal TopUpFeePercentage { get; set; }
        // Fee charged on top-up amount (e.g., 2% of top-up amount).

        // public List<OverdraftTopUpFeeSlab>? TopUpFeeSlabs { get; set; }
        // (Optional): Allows defining multiple top-up fee bands.

        // ==============================
        // ✅ Notification & Alerts
        // ==============================

        public bool NotifyCustomerOnUsage { get; set; }
        // If true, customer gets notified (SMS/email) when using overdraft.

        public int NotificationThresholdPercentage { get; set; }
        // Send notification when usage reaches this percentage of the limit (e.g., 80%).

        // ==============================
        // ✅ Income-Based Defaulting (Optional)
        // ==============================

        public virtual ICollection<OverdraftLimitBand> IncomeBasedLimitBands { get; set; }
        // Defines OD limits based on income/salary bands (e.g., 0–100K = 10K OD, 100K+ = 30K OD).

        // ==============================
        // ✅ Operational Settings
        // ==============================

        public bool IsCentralised { get; set; }
        // If true, config is managed at HO and shared across branches.

        public string BranchId { get; set; }
        // If not centralized, config applies only to the given branch.

        // ==============================
        // ✅ Audit
        // ==============================

        public bool ApplyPenaltyToThisConfig { get; set; }
        public virtual ICollection<Penalty> PenaltyId { get; set; }
        public bool IsActive { get; set; } = true;
        // Whether this configuration is currently in use or has been deactivated.
    }

    public class OverdraftInterestTier
    {
        public string Id { get; set; }
        public decimal UsageFromPercentage { get; set; } // e.g. 0%
        public decimal UsageToPercentage { get; set; }   // e.g. 50%
        public decimal InterestRate { get; set; }        // e.g. 2%
        public string BranchId { get; set; }
        public string OverdraftFacilityConfigId { get; set; }
        public bool IsCentralise { get; set; }
        public virtual OverdraftFacilityConfig OverdraftFacilityConfig { get; set; }
    }
    public class OverdraftLimitBand
    {
        public string Id { get; set; }
        public decimal MonthlyIncomeFrom { get; set; }   // e.g. 100000
        public decimal MonthlyIncomeTo { get; set; }     // e.g. 300000
        public decimal ApprovedLimit { get; set; }       // e.g. 75000
        public string BranchId { get; set; }
        public string OverdraftFacilityConfigId { get; set; }
        public bool IsCentralised { get; set; }
        public virtual OverdraftFacilityConfig OverdraftFacilityConfig { get; set; }

    }

}
