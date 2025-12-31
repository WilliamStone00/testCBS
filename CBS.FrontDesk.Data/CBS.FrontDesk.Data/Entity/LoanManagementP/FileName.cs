using CBS.FrontDesk.Data.Entity.LoanConf;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.LoanManagementP
{
    public sealed class InstitutionScoreModelDto
    {
        public string Facility { get; set; } = "";
        public int OverScoreBonus { get; set; } = 15;
        public int MediumRiskBuffer { get; set; } = 10;

        public List<ScoreBandDto> Bands { get; set; } = new List<ScoreBandDto>();
        public List<ScoringAssessmentDto> Assessments { get; set; } = new List<ScoringAssessmentDto>();
    }

    public sealed class ScoreBandDto
    {
        public decimal MaxAmount { get; set; }
        public int RequiredScore { get; set; }
    }
    public sealed class OldLoanPolicyInfoDto
    {
        public string LoanId { get; set; }
        public string Facility { get; set; }
        public string ProductId { get; set; }

        public decimal Amount { get; set; }     // original amount
        public decimal Balance { get; set; }    // remaining capital/balance
    }

    public sealed class ScoringAssessmentDto
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public int Points { get; set; }
    }

    public sealed class InstitutionScoreConfigDto
    {
        public int OverScoreBonus { get; set; } = 15;
        public int MediumRiskBuffer { get; set; } = 10;

        // required score bands based on requested amount
        public List<ScoreBandDto> Bands { get; set; } = new List<ScoreBandDto>();
    }

    public sealed class NewLoanApplicationRequest
    {
        // Core identity
        [Required] public string MemberId { get; set; }
        [Required] public string Facility { get; set; } // CLASSIC | SSF (NEW loan for now)
        [Required] public string Mode { get; set; } = "NEW";        // NEW only for now

        // Product + Classification
        [Required] public string InstitutionLoanTypeId { get; set; }
        [Required] public string LoanProductId { get; set; }

        // ✅ REQUIRED: Requested amount + interest rate
        [Range(1, double.MaxValue)] public decimal RequestedAmount { get; set; }
        [Range(0.0001, 999999)] public decimal InterestRate { get; set; }

        // Terms
        [Required] public string RepaymentPeriod { get; set; }
        [Required] public string RepaymentType { get; set; }
        [Range(0, 3650)] public int GracePeriodDays { get; set; }
        [Required] public string DisbursementType { get; set; }

        // Multi-selects
        public List<string> Charges { get; set; }
        public List<string> ScoreAssessmentIds { get; set; }

        // ✅ NEW: scoring summary computed/displayed in UI
        public ScoringSummaryDto Scoring { get; set; } = new ScoringSummaryDto();

        // Risk flags
        public bool RequiresDownPayment { get; set; }
        public bool HasGuarantor { get; set; }
        public bool HasCollateral { get; set; }
        public bool RequiresDocuments { get; set; }

        public decimal SavingsCoverageAmount { get; set; }
        public decimal OrdinarySharesAmountProvided { get; set; }
        public decimal TotalDownpaymentAmountProvided { get; set; }

        public string DocumentNotes { get; set; }

        // Recovery
        public bool HasRecoveryMechanism { get; set; }
        public List<string> RecoveryAccounts { get; set; }
        public List<RecoveryBlockDto> RecoveryBlocks { get; set; } = new List<RecoveryBlockDto>();
        public List<ManagerRecoveryDecisionDto> ManagerRecoveryDecision { get; set; } = new List<ManagerRecoveryDecisionDto>();

        // ✅ Documents in review (metadata only; actual bytes should be uploaded separately)
        public List<LoanDocumentDto> Documents { get; set; } = new List<LoanDocumentDto>();
    }

    public sealed class ScoringSummaryDto
    {
        /// <summary>Badge text, e.g. "NO RISK - Loan is covered".</summary>
        public string StatusCaption { get; set; }

        /// <summary>Normalized status code: OK / WARN / FAIL / UNKNOWN.</summary>
        public string StatusCode { get; set; }

        public int Score { get; set; }
        public int Required { get; set; }
        public int Delta { get; set; }

        /// <summary>Longer insight/recommendation text shown in UI.</summary>
        public string Insight { get; set; }

        /// <summary>Total points computed from selected assessments (optional if you compute server-side too).</summary>
        public int TotalSelectedPoints { get; set; }
    }

    public sealed class RecoveryBlockDto
    {
        [Required] public string AccountId { get; set; }
        [Range(1, double.MaxValue)] public decimal BlockAmount { get; set; }
    }

    public sealed class ManagerRecoveryDecisionDto
    {
        [Required] public string AccountId { get; set; }
        public string Decision { get; set; }     // APPROVE_BLOCK / DO_NOT_BLOCK / PENDING
        public decimal ManagerAmount { get; set; }
    }

    public sealed class LoanDocumentDto
    {
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public long Size { get; set; }

        // optional future: if you upload to server and return token/url
        public string TempToken { get; set; }
        public string PreviewUrl { get; set; }
    }

    public sealed class RefinanceLoanApplicationRequest
    {
        public NewLoanApplicationRequest AddLoanApplicationCommand { get; set; } = new NewLoanApplicationRequest();
        public string OldLoanId { get; set; }
        public string Reason { get; set; }
    }
    public sealed class RescheduleLoanApplicationRequest
    {
        public RescheduleLoanCommand RescheduleCommand { get; set; } = new RescheduleLoanCommand();
    }

    public sealed class RescheduleLoanCommand
    {
        public string OldLoanId { get; set; }
        public decimal RemainingCapitalBalance { get; set; }     // remaining principal
        public int NewNumberOfRepayments { get; set; }          // recomputed duration
        public string RepaymentCircle { get; set; } // Per Month/Week etc
        public string RepaymentType { get; set; }
        public string Reason { get; set; }
    }
    public sealed class OverdraftApplicationRequest
    {
        public AddOverdraftCommand AddOverdraftCommand { get; set; } = new AddOverdraftCommand();
    }
    public sealed class OverdraftAccount
    {
        public string AccountNumber { get; set; }
        public string AccountId { get; set; }
        public decimal AvilableBalance { get; set; }
        public decimal Consummed { get; set; }
        public decimal AccountLimit { get; set; }
    }
    public sealed class AddOverdraftCommand
    {
        public string MemberId { get; set; }
        public OverdraftAccount OverdraftAccount { get; set; }
        public decimal OverdraftLimit { get; set; }
        public decimal InterestRate { get; set; }
        public int DurationDays { get; set; }                 // recommended
        public string Purpose { get; set; }
        public LoanScoringSummaryDto Scoring { get; set; }
    }
    public sealed class LineOfCreditApplicationRequest
    {
        public AddLineOfCreditCommand AddLineOfCreditCommand { get; set; } = new AddLineOfCreditCommand();
    }

    public sealed class AddLineOfCreditCommand
    {
        public string MemberId { get; set; }
        public OverdraftAccount OverdraftAccount { get; set; }
        public decimal ApprovedLimit { get; set; }
        public decimal InterestRate { get; set; }
        public int TenorMonths { get; set; }                  // recommended
        public string Purpose { get; set; }
        public LoanScoringSummaryDto Scoring { get; set; } = new LoanScoringSummaryDto();
    }
    public sealed class LoanScoringSummaryDto
    {
        public string StatusCaption { get; set; }
        public string StatusCode { get; set; } // OK | WARN | FAIL | UNKNOWN
        public int Score { get; set; }
        public int Required { get; set; }
        public int Delta { get; set; }
        public string Insight { get; set; }
        public int TotalSelectedPoints { get; set; }
    }

    public sealed class LoanDocumentMetaDto
    {
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public long Size { get; set; }
    }

}
