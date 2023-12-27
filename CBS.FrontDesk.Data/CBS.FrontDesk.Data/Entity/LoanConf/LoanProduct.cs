using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.LoanConf
{
 

    public class LoanProduct
    {
        public string loanProductId { get; set; }
        public string productCode { get; set; }
        public string productName { get; set; }
        public string installmentTypeId { get; set; }
        public string fundingLineId { get; set; }
        public string taxId { get; set; }
        public string guaranteePackId { get; set; }
        public string documentPackId { get; set; }
        public string scheduleTypeId { get; set; }
        public int numberOfInstallmentMin { get; set; }
        public int numberOfInstallmentMax { get; set; }
        public string currencyId { get; set; }
        public int numberOfGracePeriodMin { get; set; }
        public int numberOfGracePeriodMax { get; set; }
        public double gracePeriodPercentageMin { get; set; }
        public double gracePeriodPercentageMax { get; set; }
        public double loanAmountMin { get; set; }
        public double loanAmountMax { get; set; }
        public string isRate { get; set; }
        public double interestRateMin { get; set; }
        public double interestRateMax { get; set; }
        public double minPercentageGuarantee { get; set; }
        public double minPercentageCollateral { get; set; }
        public double creditInsuranceMin { get; set; }
        public double creditInsuranceMax { get; set; }
        public double feeOlbMin { get; set; }
        public double feeOlbMax { get; set; }
        public string feeOlbAccountingRuleId { get; set; }
        public double feeOverduePrincipalMin { get; set; }
        public double feeOverduePrincipalMax { get; set; }
        public string feeOverduePrincipalAccountingRuleId { get; set; }
        public double feeOverdueInterestMax { get; set; }
        public double feeOverdueInterestMin { get; set; }
        public string feeOverdueInterestAccountingRuleId { get; set; }
        public bool activeStatus { get; set; }
        public bool hasTopUp { get; set; }
        public double topUpAmountMax { get; set; }
        public double topUpAmountMin { get; set; }
        public string topUpAmountAccountingRuleId { get; set; }
        public string isRearlyPartialRepaymentFeeType { get; set; }
        public double earlyPartialRepaymentFeeRate { get; set; }
        public string earlyPartialRepaymentFeeAccountingRuleId { get; set; }
        public double earlyTotalRepaymentFeeRate { get; set; }
        public string earlyTotalRepaymentFeeRateType { get; set; }
        public string earlyTotalRepaymentFeeRateAccountingRuleId { get; set; }
        public List<string> penaltyIds { get; set; }
        public List<string> fees { get; set; }
        public List<string> customerProfiles { get; set; }
        public List<string> accountingRuleIds { get; set; }
    }

    public class LoanProductConfigurationAgregates
    {
        public List<Tax> Taxes { get; set; }=new List<Tax>();
        public List<Fee> Fees { get; set; }=new List<Fee>();
        public List<Penalty> Penalties { get; set; }= new List<Penalty>();
        public List<GurantiPack> GuranteePackes { get; set; } = new List<GurantiPack>();
        public List<DocumentPack> DocumentPackes { get; set; } = new List<DocumentPack>();
        public List<FundingLine> FundingLines { get; set; } = new List<FundingLine>();
        public List<InstallmentType> InstallmentTypes { get; set; }= new List<InstallmentType>();
        public List<OtherFee> OtherFees { get; set; } = new List<OtherFee>();
        
    }

}
