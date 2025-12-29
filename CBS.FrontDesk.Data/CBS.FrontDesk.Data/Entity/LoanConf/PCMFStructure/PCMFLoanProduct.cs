using CBS.FrontDesk.Data.Entity.Overdraft;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.LoanConf.PCMFStructure
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Reflection;

    public sealed class PCMFLoanProductManagementObjects
    {
        // ============================
        // ✅ Core operations (Create/Update)
        // ============================
        public CreateLoanProductCommand CreateOrUpdate { get; set; } = new CreateLoanProductCommand();

        // ============================
        // ✅ Main entity
        // ============================
        public PCMFLoanProduct Product { get; set; } = new PCMFLoanProduct();

        // ============================
        // ✅ Lookups / Catalogs (needed by UI dropdowns)
        // ============================
        public List<LoanTargetCatalog> LoanTargets { get; set; } = new List<LoanTargetCatalog>();
        public List<PcmfLoanPurpose> PcmfLoanPurposes { get; set; } = new List<PcmfLoanPurpose>();

        // (Optional but recommended for UI)
        public List<LoanTerm> LoanTerms { get; set; } = new List<LoanTerm>();
        public List<AccountingProfileLight> AccountingProfiles { get; set; } = new List<AccountingProfileLight>();

        // ============================
        // ✅ Listing (DataTable / Index page)
        // ============================
        public List<PCMFLoanProduct> Products { get; set; } = new List<PCMFLoanProduct>();

        // ============================
        // ✅ Policy & Accounting management
        // ============================
        public LoanProductPolicy Policy { get; set; } = new LoanProductPolicy();
        public LoanProductAccountingProfile AccountingProfile { get; set; } = new LoanProductAccountingProfile();

        // ============================
        // ✅ Mapping helper (if you use it)
        // ============================
        public List<LoanTargetPcmfPopulationMap> TargetPopulationMaps { get; set; } = new List<LoanTargetPcmfPopulationMap>();

        // ============================
        // ✅ UI/Workflow state (to drive modal tabs/actions)
        // ============================
        public string ActiveTab { get; set; }                // "product", "policy", "accounting", "penalty", ...
        public string ServiceOption { get; set; }            // "insert", "update", "policy", "account_mapping", ...
        public string Message { get; set; }                  // optional UI message
        public bool IsEditMode => !string.IsNullOrWhiteSpace(CreateOrUpdate?.Id);

        // ============================
        // ✅ Convenience read-only helpers for display
        // ============================
        public string LoanTargetNameEn =>
            Product?.Target?.NameEn
            ?? LoanTargets?.Find(x => x.Id == CreateOrUpdate?.LoanTargetId)?.NameEn;

        public string PurposeNameEn =>
            Product?.Purpose?.NameEn
            ?? PcmfLoanPurposes?.Find(x => x.Id == CreateOrUpdate?.PcmfLoanPurposeId)?.NameEn;

        public OverdraftFacilityConfig OverdraftFacilityConfig { get; set; }=new OverdraftFacilityConfig();
        public List<LoanProductRepaymentCycle> LoanProductRepaymentCycles { get; set; }
        public List<LoanApplicationCollateral> LoanProductCollaterals { get; set; }

 
    }

    // ✅ Optional lightweight accounting profile for dropdowns
    public sealed class AccountingProfileLight
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class RequiredWhenAttribute : ValidationAttribute
    {
        private readonly string _dependentProperty;
        private readonly string _expectedValue;

        public RequiredWhenAttribute(string dependentProperty, string expectedValue)
        {
            _dependentProperty = dependentProperty;
            _expectedValue = expectedValue;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var instance = validationContext.ObjectInstance;
            var type = instance.GetType();

            var prop = type.GetProperty(_dependentProperty, BindingFlags.Public | BindingFlags.Instance);
            if (prop == null)
            {
                return new ValidationResult($"Validation configuration error: property '{_dependentProperty}' not found.");
            }

            var dependentValue = prop.GetValue(instance)?.ToString();
            var mustRequire = string.Equals(dependentValue, _expectedValue, StringComparison.OrdinalIgnoreCase);

            if (mustRequire && string.IsNullOrWhiteSpace(value?.ToString()))
            {
                var msg = !string.IsNullOrWhiteSpace(ErrorMessage)
                    ? ErrorMessage
                    : $"{validationContext.DisplayName} is required when {_dependentProperty} is {_expectedValue}.";

                return new ValidationResult(msg);
            }

            return ValidationResult.Success;
        }
    }

    public sealed class CreateLoanProductCommand
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Product Code is required.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Product Code must be between 2 and 50 characters.")]
        public string ProductCode { get; set; }

        [Required(ErrorMessage = "Product Name is required.")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Product Name must be between 3 and 200 characters.")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 1000 characters.")]
        public string Description { get; set; }

        /// <summary>
        /// Values: Classic, SSF, Overdraft, LineOfCredit
        /// </summary>
        [Required(ErrorMessage = "Loan Facility is required.")]
        [RegularExpression(
            "^(Classic|SSF|Overdraft|LineOfCredit)$",
            ErrorMessage = "Loan Facility must be one of: Classic, SSF, Overdraft, LineOfCredit.")]
        public string LoanFacility { get; set; }

        /// <summary>
        /// Required ONLY when LoanFacility == Classic
        /// </summary>
        [RequiredWhen(nameof(LoanFacility), "Classic",
            ErrorMessage = "Loan Term is required when Loan Facility is Classic.")]
        public string LoanTermId { get; set; }

        [Required(ErrorMessage = "PCMF Loan Purpose is required.")]
        public string PcmfLoanPurposeId { get; set; }

        [Required(ErrorMessage = "Loan Target is required.")]
        public string LoanTargetId { get; set; }

        public bool ActiveStatus { get; set; }

        /// <summary>
        /// Optional: Accounting profile / mapping profile to pre-bind GL mappings for this product.
        /// If not provided, the product can still be created, and mapping can be configured later.
        /// </summary>
        [StringLength(64, ErrorMessage = "Accounting Profile Id must not exceed 64 characters.")]

        public string ChartOfAccountIdForPrincipalAmount { get; set; }
        public string ChartOfAccountIdForTaxiblePrincipalAmount { get; set; }
        public string ChartOfAccountIdForTaxibleInterestReceived { get; set; }
        public string ChartOfAccountIdForInterestReceived { get; set; }
        public string ChartOfAccountIdForTax { get; set; }
        public string ChartOfAccountIdForPenalty { get; set; }
        public string ChartOfAccountIdForLoanTransition { get; set; }
    }

    public static class LoanFacilityType
    {
        public const string Classic = "Classic";
        public const string SSF = "SSF";
        public const string Overdraft = "Overdraft";
        public const string LineOfCredit = "LineOfCredit";
    }

    public sealed class LoanTargetCatalog
    {
        public string Id { get; set; }

        // Tenant scope (recommended)
        public string OrganizationId { get; set; }
        public string BankId { get; set; }

        // Stable code for integrations/reports (e.g. INDIVIDUAL, EMPLOYEE…)
        public string Code { get; set; }

        // Editable UI labels
        public string NameEn { get; set; }
        public string NameFr { get; set; }

        // ✅ Strict mapping to PCMF population
        public PcmfPopulation PcmfPopulation { get; set; }

        public bool IsActive { get; set; } = true;
        public string Description { get; set; }

        // Audit fields
        public DateTime? CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? DeletedDate { get; set; }
        public string DeletedBy { get; set; }
        public int ObjectState { get; set; }
        public bool IsDeleted { get; set; }
    }

   

    public sealed class PcmfLoanPurpose
    {
        public string Id { get; set; }

        /// <summary>
        /// Stable PCMF purpose key (do not change once seeded).
        /// Used as the canonical identifier for the purpose across deployments.
        /// </summary>
        public PcmfPurposeKey Key { get; set; }

        /// <summary>
        /// French display name.
        /// </summary>
        public string NameFr { get; set; }

        /// <summary>
        /// English display name.
        /// </summary>
        public string NameEn { get; set; }

        /// <summary>
        /// Optional French description shown in UI as guidance to staff.
        /// </summary>
        public string DescriptionFr { get; set; }

        /// <summary>
        /// Optional English description shown in UI as guidance to staff.
        /// </summary>
        public string DescriptionEn { get; set; }

        /// <summary>
        /// Whether this purpose is selectable in UI.
        /// If false, it remains in DB for history/reporting but is hidden from selection.
        /// </summary>
        public bool IsActive { get; set; } = true;

        // -------------------------
        // Credits (term-based)
        // -------------------------

        /// <summary>PCMF group code for Long-Term loans (301..309).</summary>
        public int? LtGroupCode { get; set; }  // 301..309

        /// <summary>PCMF group code for Medium-Term loans (311..319).</summary>
        public int? MtGroupCode { get; set; }  // 311..319

        /// <summary>PCMF group code for Short-Term loans (320..329).</summary>
        public int? CtGroupCode { get; set; }  // 320..329

        // -------------------------
        // Overdraft (not term-based)
        // -------------------------

        /// <summary>PCMF group code for Overdraft products (371..378).</summary>
        public int? OdGroupCode { get; set; }  // 371..378
    }

    public sealed class LoanProductFullDto
    {
        // --------------------
        // Core product
        // --------------------
        public string Id { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public bool ActiveStatus { get; set; }
        public string LoanTypeCategory { get; set; }

        // --------------------
        // PCMF classification (derived)
        // --------------------
        public string PcmfSection { get; set; }  // LT/MT/CT/OD (string for API friendliness)
        public int PcmfGroupCode { get; set; }
        public string PcmfPopulation { get; set; }
        public int PcmfBaseCode { get; set; }

        // --------------------
        // Foreign ids
        // --------------------
        public string LoanTermId { get; set; }
        public string LoanTargetId { get; set; }
        public string PcmfLoanPurposeId { get; set; }
        public string AccountingProfileId { get; set; }

        // --------------------
        // Included objects (always)
        // --------------------
        public LoanTerm Term { get; set; }
        public LoanTargetCatalog Target { get; set; }
        public PcmfLoanPurpose Purpose { get; set; }

        public LoanProductPolicy Policy { get; set; }
        public LoanProductAccountingProfile AccountingMapping { get; set; }

        // Optional convenience fields
        public List<string> RepaymentCycles { get; set; }

        // Additional fields from response
        public string LoanFacility { get; set; }

        // Audit fields
        public DateTime? CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? DeletedDate { get; set; }
        public string DeletedBy { get; set; }
        public int ObjectState { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class PCMFLoanProduct
    {
        public string Id { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public bool ActiveStatus { get; set; }
        public string LoanFacility { get; set; }

        public string LoanTypeCategory { get; set; }
        public string LoanTermId { get; set; }                // required except overdraft
        public virtual LoanTerm Term { get; set; }

        public string LoanTargetId { get; set; }
        public virtual LoanTargetCatalog Target { get; set; }

        public string PcmfLoanPurposeId { get; set; }
        public PcmfLoanPurpose Purpose { get; set; }

        // Stored classification (derived)
        public string PcmfSection { get; set; }        // LT/MT/CT/OD
        public int? PcmfGroupCode { get; set; }              // 301/311/320...
        public string PcmfPopulation { get; set; }  // derived from target
        public int? PcmfBaseCode { get; set; }// derived (ex: 301, 3201, 3711) - your rule

        // Optional accounting template link (if you keep it)
        public string AccountingProfileId { get; set; }

        // Policies (Option A: stored in Loan MS)
        public virtual LoanProductPolicy Policy { get; set; } = new LoanProductPolicy();

        // Accounting mappings (Option A: stored in Loan MS)
        public virtual LoanProductAccountingProfile AccountingMapping { get; set; }
        public string OverdraftFacilityConfigId { get; set; }
        public List<string> RepaymentCycles { get; set; }
        public virtual OverdraftFacilityConfig OverdraftFacilityConfig { get; set; }
        public virtual ICollection<LoanProductRepaymentCycle> LoanProductRepaymentCycles { get; set; }
        public virtual ICollection<LoanApplicationCollateral> LoanProductCollaterals { get; set; }

        // New fields from response
        public string LoanTargetRef { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? DeletedDate { get; set; }
        public string DeletedBy { get; set; }
        public int ObjectState { get; set; }
        public bool IsDeleted { get; set; }
    }

    public sealed class LoanProductAccountingProfile
    {
        public string Id { get; set; }
        public string LoanProductId { get; set; }

        // Snapshot for quick enforcement and reporting
        public int PcmfBaseCode { get; set; }
        public string PcmfSection { get; set; }
        public int PcmfGroupCode { get; set; }
        public string PcmfPopulation { get; set; }

        public string ChartOfAccountIdForFee { get; set; }
        public string ChartOfAccountIdForPrincipalAmount { get; set; }
        public string ChartOfAccountIdForPenalty { get; set; }

        public string ChartOfAccountIdForLoanTransition { get; set; }
        public string ChartOfAccountIdForWriteOffPrincipal { get; set; }
        public string ChartOfAccountIdForInterestReceived { get; set; }
        public string ChartOfAccountIdForProvisionMoreThanOneYear { get; set; }
        public string ChartOfAccountIdForProvisionMoreThanTwoYear { get; set; }
        public string ChartOfAccountIdForProvisionMoreThanThreeYear { get; set; }
        public string ChartOfAccountIdForProvisionMoreThanFourYear { get; set; }



        //  TAXIBLE LOAN GLs (Your new section)
        //
        public string ChartOfAccountIdForTaxiblePrincipalAmount { get; set; }
        public string ChartOfAccountIdForTaxibleInterestReceived { get; set; }
        public string ChartOfAccountIdForTax { get; set; }

        // --- Snapshots resolved from Accounting VII (normalized) ---
        public string PrincipalAccountNumber { get; set; }
        public string PrincipalAccountName { get; set; }

        public string TaxablePrincipalAccountNumber { get; set; }
        public string TaxablePrincipalAccountName { get; set; }
        public string NoneTaxibleAccountNumber { get; set; }
        public string AccountNumber { get; set; }

        // Audit fields
        public DateTime? CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? DeletedDate { get; set; }
        public string DeletedBy { get; set; }
        public int ObjectState { get; set; }
        public bool IsDeleted { get; set; }
    }

    public sealed class LoanProductPolicy
    {
        public string Id { get; set; }
        /// <summary>
        /// Target LoanProduct Id.
        /// </summary>
        public string LoanProductId { get; set; }

        public string TermId { get; set; }

        // --------------------------------------------------------------------
        // POLICY: Amount limits
        // --------------------------------------------------------------------

        public decimal LoanMinimumAmount { get; set; }
        public decimal LoanMaximumAmount { get; set; }

        // --------------------------------------------------------------------
        // POLICY: Interest configuration
        // --------------------------------------------------------------------

        /// <summary>Interest period (e.g., "Per Day", "Per Week", "Per Month", "Per Year").</summary>
        public string LoanInterestPeriod { get; set; }

        public decimal MinimumInterestRate { get; set; }
        public decimal MaximumInterestRate { get; set; }

        /// <summary>Interest must be paid up-front before disbursement.</summary>
        public bool InterestMustBePaidUpFront { get; set; }

        /// <summary>Start generating interest after disbursement.</summary>
        public bool StartGeneratingInterestAfterDisbustment { get; set; }

        /// <summary>If true, stop interest calculation at maturity date.</summary>
        public bool StopInterestCalculationAtLoanMaturityDate { get; set; }

        /// <summary>Extra stop rule: number of days to stop interest calculation (if used).</summary>
        public int NumberOfDaysToStopInterestCalculation { get; set; }

        // --------------------------------------------------------------------
        // POLICY: Duration / term constraints (product-level constraints)
        // NOTE: TermId is part of Core. These are policy rules on duration.
        // --------------------------------------------------------------------

        /// <summary>Duration period (e.g. "Days", "Weeks", "Months", "Years").</summary>
        public string LoanDurationPeriod { get; set; }

        public int MinimumDurationPeriod { get; set; }
        public int MaximumDurationPeriod { get; set; }

        /// <summary>Minimum number of repayment installments required by this product.</summary>
        public int MinimumNumberOfRepayment { get; set; }

        // --------------------------------------------------------------------
        // POLICY: Charges / fees rules
        // --------------------------------------------------------------------

        public bool IsPaidFeeBeforeProcessing { get; set; }

        public bool IsChargesApplied { get; set; }

        public decimal MinimumChargesToAppliedInPercentage { get; set; }
        public decimal MaximumChargesToAppliedPercentage { get; set; }
        public decimal DefaultChargeToAppliedPercentage { get; set; }

        public decimal MinimumChargesStartDayAfterLoanDueDate { get; set; }
        public decimal MaximumChargesStartDayAfterLoanDueDate { get; set; }

        /// <summary>Default charges start day after loan due date (your original default was 60).</summary>
        public decimal DefaulChargesStartDayAfterLoanDueDate { get; set; } = 60;

        /// <summary>Where charges are applied: "Interest" or "Balance".</summary>
        public string ChargesAreAppliedToInterestOrBalance { get; set; } = "Interest";

        public int ChargesStopAfterHowManyDaysFromStart { get; set; } = 30;

        // --------------------------------------------------------------------
        // POLICY: Waiver rules
        // --------------------------------------------------------------------

        public bool IsInterestWaiverApplied { get; set; }
        public decimal MinimumInterestWaiver { get; set; }
        public decimal MaximumInterestWaiver { get; set; }

        // --------------------------------------------------------------------
        // POLICY: Refinancing / top-up rules
        // --------------------------------------------------------------------

        public decimal MinimumPercentageRefundBeforeRefinancing { get; set; }
        public bool HasTopUp { get; set; }

        // --------------------------------------------------------------------
        // POLICY: Guarantee / collateral / eligibility rules
        // --------------------------------------------------------------------

        public bool RequiresGuarantor { get; set; }

        public bool ShorteeMustHaveFundToGuranteeLoan { get; set; }
        public decimal MinimumPercentageCoverageOfShortee { get; set; }

        public bool Co_obligorMustHaveFundToGuranteeLoan { get; set; }

        public bool IsRequiredCollateral { get; set; }
        public decimal MinimumCollateralPercentage { get; set; }

        public decimal MinimumDownPaymentPercentage { get; set; }

        // --------------------------------------------------------------------
        // POLICY: Required account eligibility rules
        // --------------------------------------------------------------------

        public bool IsRequiredShareAccount { get; set; }
        public bool IsRequiredSalaryccount { get; set; }
        public bool IsRequiredSavingAccount { get; set; }

        public bool IsRequresRegisteredPublicAuthority { get; set; }
        public bool IsRequredIrrivocableSalaryTransfer { get; set; }

        public decimal MinimumSavingAccountBalanceRateForTheRequestAmount { get; set; }
        public decimal MinimumSalaryAccountBalanceRateForTheRequestAmount { get; set; }
        public decimal MinimumShareAccountBalanceForTheRequestAmount { get; set; }

        // --------------------------------------------------------------------
        // POLICY: Blocking behavior during loan processing
        // (These are product behavior policies)
        // --------------------------------------------------------------------

        public bool BlockShareAccount { get; set; }
        public bool BlockSavingAccount { get; set; }
        public bool BlockSalaryAccount { get; set; }

        // --------------------------------------------------------------------
        // POLICY: Mortgage-specific rules (kept as policy because these are constraints)
        // --------------------------------------------------------------------

        public bool IsMortgage { get; set; }

        /// <summary>Require insurance before disbursement.</summary>
        public bool RequireInsurance { get; set; } = true;

        /// <summary>Require registered land title.</summary>
        public bool RequireTitleRegistration { get; set; } = true;

        /// <summary>Grace period in months (construction loans etc.).</summary>
        public int GracePeriodMonths { get; set; } = 3;

        /// <summary>Max loan amount as % of property value (LTV). Example: 0.80 = 80%.</summary>
        public decimal MaxLoanToValueRatio { get; set; } = 0.80M;

        /// <summary>Enable phased/staged disbursement.</summary>
        public bool EnablePhasedDisbursement { get; set; } = true;

        /// <summary>Minimum collateral coverage percent (your original default: 1.25 = 125%).</summary>
        public decimal MinCollateralCoveragePercent { get; set; } = 1.25M;

        /// <summary>Allow third-party ownership of collateral.</summary>
        public bool AllowThirdPartyOwnership { get; set; } = false;

        // --------------------------------------------------------------------
        // POLICY: Ordering / distribution (if used by your repayment allocation logic)
        // --------------------------------------------------------------------

        public int InterestOrder { get; set; }
        public int CapitalOrder { get; set; }
        public int FineOrder { get; set; }

        public decimal InterestRate { get; set; }
        public decimal CapitalRate { get; set; }
        public decimal FineRate { get; set; }

        // --------------------------------------------------------------------
        // POLICY: Delinquency / repayment settings
        // --------------------------------------------------------------------

        public string LoanDeliquencyPeriod { get; set; }
        public string RepaymentTypeName { get; set; }

        /// <summary>Penalty configuration id used by this product.</summary>
        public string PenaltyId { get; set; }

        // Audit fields
        public DateTime? CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? DeletedDate { get; set; }
        public string DeletedBy { get; set; }
        public int ObjectState { get; set; }
        public bool IsDeleted { get; set; }
    }

    public sealed class LoanTargetPcmfPopulationMap
    {
        public string Id { get; set; }

        // Scope (multi-tenant / multi-MFI)
        public string OrganizationId { get; set; }
        public string BankId { get; set; }
        public bool IsActive { get; set; } = true;

        // Your existing target
        public LoanTargets LoanTarget { get; set; }

        // PCMF target
        public PcmfPopulation PcmfPopulation { get; set; }

        public string Note { get; set; }

        // Audit fields
        public DateTime? CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? DeletedDate { get; set; }
        public string DeletedBy { get; set; }
        public int ObjectState { get; set; }
        public bool IsDeleted { get; set; }
    }

    // Note: Added LoanTerm class definition above

    public enum LoanTargets
    {
        Elected_Staff,
        Individual,
        Moral,
        Employee,
        Private_Sectors,
        Public_Sectors,
        CamCCUL_Staff
    }

    public enum LoanTermKind
    {
        ShortTerm = 1,
        MediumTerm = 2,
        LongTerm = 3
    }

    /// <summary>
    /// PCMF regulatory structure:
    /// LT => 301–309
    /// MT => 311–319
    /// CT => 320–329
    /// OD => 371–378 (Overdraft / comptes débiteurs)
    /// </summary>
    public enum PcmfSection
    {
        LT = 1,
        MT = 2,
        CT = 3,
        OD = 4
    }

    public enum LoanAccountingProfileStatus
    {
        Draft = 0,
        Validated = 1,
        Active = 2,
        Stale = 3,
        Disabled = 4
    }

    /// <summary>
    /// PCMF target population (beneficiary category).
    /// </summary>
    public enum PcmfPopulation
    {
        Societaire = 1,
        Client = 2,
        Apparente = 3,
        EMF = 4,
        EMFReseau = 5,
        Individual = 6,
        Group = 7,
        Moral = 8,
        PrivateSector = 9,
        PublicSector = 10,
        Employee = 11
    }

    /// <summary>
    /// Stable PCMF purpose keys.
    /// Names are stored in DB (FR/EN) so MFIs can adjust wording without breaking PCMF codes.
    /// </summary>
    public enum PcmfPurposeKey
    {
        Immobilier,
        Habitat,
        Equipement,
        MoratoireEtat,
        CampagneMoratoire,
        Consommation,
        CreditBail,
        AutresCredits,
        CreancesRattachees,

        // CT detailed
        Ct_EffetsCommerciauxEscomptes,
        Ct_Affacturage,
        Ct_ChequesLocauxEscomptes,
        Ct_MoratoiresConsolidesEtat,
        Tresorerie,
        Ct_Equipement,
        Ct_AvancesMarchesPublicsNanties,
        Ct_AutresCreditsAccompagnement,
        Ct_Campagne,
        Ct_Consommation,
        Ct_CreditBail,
        Ct_AutresCredits,
        Ct_CreancesRattachees,

        // OD
        Od_ComptesCourantsDebiteurs,
        Od_ComptesChequesDebiteurs,
        Od_ComptesOrdinairesDebiteurs,
        Od_DepotsGarantie,
        Od_AvancesDAT,
        Od_LoyersCreditBail,
        Od_DepotsGarantieCreditBail,
        Od_CreancesDettesRattachees
    }

    public sealed class GetPcmfLoanProductUiCatalogQuery
    {
        public string LoanTermId { get; set; }
        public string PcmfLoanPurposeId { get; set; }
        public string LoanTargetId { get; set; }

        // ✅ NEW
        public string LoanFacility { get; set; } = "Classic";

        // Optional: "en" / "fr"
        public string Lang { get; set; } = "en";
    }

    public sealed class PcmfLoanProductUiCatalogResponse
    {
        // Master lists (for dropdowns)
        public List<UiOptionDto> LoanTerms { get; set; }
        public List<UiOptionDto> Purposes { get; set; }
        public List<UiOptionDto> Targets { get; set; }

        // Preview classification (guides user + accounting mapping)
        public PcmfPreviewDto Preview { get; set; }

        // Rules/UI hints
        public bool IsOverdraft { get; set; }
        public bool IsLoanTermRequired { get; set; } = true;

        // Echo selections back (useful for UI)
        public string LoanTermId { get; set; }
        public string PcmfLoanPurposeId { get; set; }
        public string LoanTargetId { get; set; }
    }

    public sealed class UiOptionDto
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public string Hint { get; set; } // ex: "CT:320" or "Population: INDIVIDUAL"
    }

    public sealed class PcmfPreviewDto
    {
        public string Section { get; set; }  // "CT","MT","LT","OD"
        public int GroupCode { get; set; }
        public int BaseCode { get; set; }
        public string Population { get; set; }
    }

}