using CBS.FrontDesk.Data.Entity.LoanConf;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.LoanManagementP
{

    public sealed class OldLoanPolicyInfoDto
    {
        public string LoanId { get; set; }
        public string Facility { get; set; }
        public string ProductId { get; set; }

        public decimal Amount { get; set; }     // original amount
        public decimal Balance { get; set; }    // remaining capital/balance
    }

    public class ScoringAssessmentDto
    {
        public string Id { get; set; }
        public string Name { get; set; }

        // scoring
        public int Points { get; set; }             // max points for the item
        public decimal Weight { get; set; } = 1m;   // weight multiplier (section profile)
        public bool IsPenalty { get; set; }         // negative score item
        public int MinPoints { get; set; } = 0;     // allow negative: e.g. -30
        public bool Mandatory { get; set; }         // hard-stop if failed

        // organization / UI
        public string Category { get; set; }        // e.g. "Capacity", "KYC", "Collateral"
        public string EvidenceHint { get; set; }    // what officer must attach/confirm
        public string AppliesTo { get; set; }       // e.g. "ALL", "BUSINESS", "SALARY", "OD_LOC"
    }

    public class InstitutionScoreConfigDto
    {
        public int OverScoreBonus { get; set; }
        public int MediumRiskBuffer { get; set; }
        public List<ScoreBandDto> Bands { get; set; }

        // ✅ must exist
        public List<ScoreAssessmentDto> Assessments { get; set; }
    }

    public class ScoreAssessmentDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Points { get; set; }
    }

    public class InstitutionScoreModelDto
    {
        public string Facility { get; set; }
        public int OverScoreBonus { get; set; }
        public int MediumRiskBuffer { get; set; }
        public List<ScoreBandDto> Bands { get; set; }=new List<ScoreBandDto>();
        public List<ScoringAssessmentDto> Assessments { get; set; } = new List<ScoringAssessmentDto>();

        // new
        public List<string> MandatoryChecks { get; set; } = new List<string>();
        public List<ScoreDecisionRuleDto> DecisionRules { get; set; } = new List<ScoreDecisionRuleDto>();
    }

    public class ScoreDecisionRuleDto
    {
        public string Code { get; set; }          // APPROVE / REVIEW / REJECT
        public int MinScore { get; set; }         // score threshold
        public int MaxMandatoryFailed { get; set; } = 0;
        public string Message { get; set; }
    }

    public sealed class ScoreBandDto
    {
        public decimal MaxAmount { get; set; }
        public int RequiredScore { get; set; }
    }
    //public sealed class InstitutionScoreConfigDto
    //{
    //    public int OverScoreBonus { get; set; } = 15;
    //    public int MediumRiskBuffer { get; set; } = 10;

    //    // required score bands based on requested amount
    //    public List<ScoreBandDto> Bands { get; set; } = new List<ScoreBandDto>();
    //}

    public sealed class NewLoanApplicationRequest
    {
        // Core identity
        [Required] public string MemberId { get; set; }
        [Required] public string Facility { get; set; } // CLASSIC | SSF | Overdraft | LineOfCredit (NEW loan for now)
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
        // =====================================================
        // ✅ NEW: Guarantors / Co-obligors / Other security parties
        // =====================================================

        /// <summary>Physical persons guaranteeing the loan (moral guarantors).</summary>
        public List<MoralGuarantor> MoralGuarantors { get; set; } = new List<MoralGuarantor>();

        /// <summary>Co-obligors who may optionally have an account blocked.</summary>
        public List<Coobligor> Coobligors { get; set; } = new List<Coobligor>();

        /// <summary>Shotee records (same structure as coobligor in your model).</summary>
        public List<Shotee> Shotees { get; set; } = new List<Shotee>();

        /// <summary>Inter-cooperation guarantees (note your model has Blockccount typo).</summary>
        public List<InterCooperation> InterCooperations { get; set; } = new List<InterCooperation>();

        /// <summary>Collateral items provided for the application.</summary>
        public List<LoanApplicationCollateral> Collaterals { get; set; } = new List<LoanApplicationCollateral>();
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

    public class MoralGuarantor
    {
        public string CompanyOrGroupName { get; set; }
        public string IdCardNumber { get; set; }
        public string ExpireDate { get; set; }
        public string IssueDate { get; set; }
        public string Relationship { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public decimal GuaranteeAmount { get; set; }
    }
    public class Coobligor
    {
        public string MemerberName { get; set; }
        public string MemerberReference { get; set; }
        public string IdCardNumber { get; set; }
        public string ExpireDate { get; set; }
        public string IssueDate { get; set; }
        public string Relationship { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public decimal GuaranteeAmount { get; set; }
        public string AccountNumber { get; set; }
        public decimal Amount { get; set; }
        public bool BlockAccount { get; set; }
    }
    public class Shotee
    {
        public string MemerberName { get; set; }
        public string MemerberReference { get; set; }
        public string IdCardNumber { get; set; }
        public string ExpireDate { get; set; }
        public string IssueDate { get; set; }
        public string Relationship { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public decimal GuaranteeAmount { get; set; }
        public string AccountNumber { get; set; }
        public decimal Amount { get; set; }
        public bool BlockAccount { get; set; }
    }
    public class InterCooperation
    {
        public string MemerberName { get; set; }
        public string MemerberReference { get; set; }
        public string AffiliateOrInstitutionName { get; set; }
        public string IdCardNumber { get; set; }
        public string ExpireDate { get; set; }
        public string IssueDate { get; set; }
        public string Relationship { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public decimal GuaranteeAmount { get; set; }
        public string AccountNumber { get; set; }
        public decimal Amount { get; set; }
        public bool Blockccount { get; set; }
    }

    public class LoanApplicationCollateral
    {
        public string Name { get; set; }
        public string CollateraTypeId { get; set; }
        public decimal Amount { get; set; }
        public string Location { get; set; }
        public string Address { get; set; }
        public string ReferenceNumber { get; set; }
    }

    public enum ChargeCalcType
    {
        Fixed = 1,
        Rate = 2,
        Range = 3
    }
    public sealed class ChargeSelectionDto
    {
        [Required] public string FeeId { get; set; }

        // ✅ default computed by server (what system suggested)
        [Range(0, double.MaxValue)]
        public decimal SystemCharge { get; set; }

        // ✅ user final amount (must be >= SystemCharge)
        [Range(0, double.MaxValue)]
        public decimal UserInputedCharge { get; set; }

        [Required] public string Mode { get; set; } // BEFORE_APPRAISAL | AFTER_DISBURSEMENT

        public ChargeCalcType ChargeCalcType { get; set; }
    }

    public sealed class ChargeRangeTier
    {
        public decimal FromAmount { get; set; }
        public decimal ToAmount { get; set; } // inclusive
        public decimal FeeAmount { get; set; } // fixed fee when amount is in range
    }
    public sealed class LoanChargeDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Mode { get; set; } // BEFORE_APPRAISAL | AFTER_DISBURSEMENT

        public ChargeCalcType CalcType { get; set; }

        public decimal? FixedAmount { get; set; }
        public decimal? RatePercent { get; set; } // 2.5 = 2.5%
        public List<ChargeRangeTier> Ranges { get; set; } = new List<ChargeRangeTier>();

        public decimal? MinFee { get; set; }
        public decimal? MaxFee { get; set; }

        // ✅ computed
        public decimal SystemCharge { get; set; } // default computed by system for requested amount
    }

}
