using CBS.FrontDesk.Data.Entity.DataTable;
using System.ComponentModel.DataAnnotations;

namespace CBS.FrontDesk.Data.Entity.AccountingV2.BranchCashConfigV
{

    public class BranchCashConfig
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Branch is required.")]
        public string BranchId { get; set; }

        [Required(ErrorMessage = "Cash-in-hand (teller till) account is required.")]
        public string CashInHandAccountId { get; set; } // teller till

        [Required(ErrorMessage = "Vault account is required.")]
        public string VaultAccountId { get; set; }

        [Required(ErrorMessage = "Surplus income (overage) account is required.")]
        public string SurplusIncomeAccountId { get; set; } // overage

        [Required(ErrorMessage = "Revenue account is required.")]
        public string RevenueAccountId { get; set; }

        [Required(ErrorMessage = "Shortage expense account is required.")]
        public string ShortageExpenseAccountId { get; set; } // shortage

        // Optional toggle – no [Required] needed
        public bool RealTimeCashPosting { get; set; } = true; // cash ops impacted in real time

        [Required(ErrorMessage = "Partner account is required.")]
        public string PartnerAccountId { get; set; }

        [Required(ErrorMessage = "CamCCUL account is required.")]
        public string CamcculAccountId { get; set; }

        [Required(ErrorMessage = "Head Office liaison account is required.")]
        public string HeadOfficeLiaisonAccountId { get; set; }

        [Required(ErrorMessage = "Form fee income account is required.")]
        public string FormFeeIncomeAccountId { get; set; }

        [Required(ErrorMessage = "Loan transit account is required.")]
        public string LoanTransitAccountId { get; set; }

        [Required(ErrorMessage = "Cash-in commission account is required.")]
        public string CashInCommisionAccountIDAccountId { get; set; }

        [Required(ErrorMessage = "Cash-out commission account is required.")]
        public string CashOutCommisionAccountIDAccountId { get; set; }

        [Required(ErrorMessage = "Transfer commission account is required.")]
        public string TransfterCommisionAccountIDAccountId { get; set; }

        [Required(ErrorMessage = "VAT account is required.")]
        public string VATAccountId { get; set; }

        [Required(ErrorMessage = "Interest-generated-from account is required.")]
        public string InterestGeneratedFromAccountId { get; set; }

        [Required(ErrorMessage = "MoMo cash account is required.")]
        public string MomocashAccountId { get; set; }

        [Required(ErrorMessage = "MoMo cash commission GL account is required.")]
        public string MomocashCommissionGlId { get; set; }

        [Required(ErrorMessage = "Fallback suspense account is required.")]
        public string FallbackSuspenseAccountId { get; set; }
        [Required(ErrorMessage = "Cash reversal account is required.")]
        public string CashReversalAccountId { get; set; }

        public string SmsExpenseAccountId { get; set; }

    }


    public sealed class BranchCashConfigRowDto
    {
        public string Id { get; set; }
        public string BranchId { get; set; }
        public string BranchName { get; set; }

        public string CashInHandAccountName { get; set; }
        public string CashInHandAccountNumber { get; set; }

        public string VaultAccountName { get; set; }
        public string VaultAccountNumber { get; set; }

        public string SurplusIncomeAccountName { get; set; }
        public string SurplusIncomeAccountNumber { get; set; }

        public string ShortageExpenseAccountName { get; set; }
        public string ShortageExpenseAccountNumber { get; set; }

        public string RevenueAccountName { get; set; }
        public string RevenueAccountNumber { get; set; }

        public string PartnerAccountName { get; set; }
        public string PartnerAccountNumber { get; set; }

        public string CamcculAccountName { get; set; }
        public string CamcculAccountNumber { get; set; }

        public string HeadOfficeLiaisonAccountName { get; set; }
        public string HeadOfficeLiaisonAccountNumber { get; set; }

        public string FormFeeIncomeAccountName { get; set; }
        public string FormFeeIncomeAccountNumber { get; set; }

        public bool RealTimeCashPosting { get; set; }

        public string LoanTransitAccountName { get; set; }
        public string LoanTransitAccountNumber { get; set; }
        public string CashInCommisionAccountName { get; set; }
        public string CashInCommisionAccountNumber { get; set; }
        public string CashOutCommisionAccountName { get; set; }
        public string CashOutCommisionAccountNumber { get; set; }
        public string TransfterCommisionAccountName { get; set; }
        public string TransfterCommisionAccountNumber { get; set; }
        public string VATAccountName { get; set; }
        public string VATAccountNumber { get; set; }
        public string InterestGeneratedFromAccountName { get; set; }
        public string InterestGeneratedFromAccountNumber { get; set; }
        public string MomocashCommissionGlName { get; set; }
        public string MomocashCommissionGlNumber { get; set; }
        public string MomocashAccountNumber { get; set; }
        public string MomocashAccountName { get; set; }
        public string FallbackSuspenseAccountName { get; set; }
        public string FallbackSuspenseAccountNumber { get; set; }
        public string CashReversalAccountName { get; set; }
        public string CashReversalAccountNumber { get; set; }
    }

    // DTO used only by the Details partial
    public sealed class BranchCashConfigDetailsVm
    {
        public string Id { get; set; }
        public string Branch { get; set; }

        public bool RealTimeCashPosting { get; set; }

        // Core Mandatory Cash-Control Accounts
        public AccountPair CashInHand { get; set; }
        public AccountPair Vault { get; set; }
        public AccountPair SurplusIncome { get; set; }
        public AccountPair ShortageExpense { get; set; }

        // Revenue & Partner Settlement
        public AccountPair Revenue { get; set; }
        public AccountPair Partner { get; set; }
        public AccountPair Camccul { get; set; }
        public AccountPair FormFeeIncome { get; set; }

        // Inter-branch & Liaison
        public AccountPair HeadOfficeLiaison { get; set; }

        // Loan & Interest Fallback
        public AccountPair LoanTransit { get; set; }
        public AccountPair InterestGeneratedFrom { get; set; }

        // Commission Fallback Accounts
        public AccountPair CashInCommission { get; set; }
        public AccountPair CashOutCommission { get; set; }
        public AccountPair TransferCommission { get; set; }

        // VAT Backup Account
        public AccountPair VAT { get; set; }

        // Mobile Money (MoMoCash)
        public AccountPair Momocash { get; set; }
        public AccountPair MomocashCommission { get; set; }
        public AccountPair FallbackSuspense { get; set; }
        public AccountPair CashReversal { get; set; }
    }


    public sealed class AccountPair
    {
        public string Name { get; set; }
        public string Number { get; set; }
    }

    public class BranchCashConfigQueryDto
    {
        public DataTableOptions Options { get; set; }
        
        // Primary Identifier for direct lookup
        public string Id { get; set; }

        // Key filterable dimension
        public string BranchId { get; set; }

        // Status/Type filter
        public bool? RealTimeCashPosting { get; set; }

        // Account filters
        public string CashInHandAccountId { get; set; }
        public string VaultAccountId { get; set; }
        public string SurplusIncomeAccountId { get; set; }
        public string ShortageExpenseAccountId { get; set; }
        public string RevenueAccountId { get; set; }
        public string FormFeeIncomeAccountId { get; set; }

        public BranchCashConfigQueryDto()
        {
            Options = new DataTableOptions();
        }
    }
}