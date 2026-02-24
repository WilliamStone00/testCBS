using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting_V2.API.IPSReporting
{
    public class IPSReport
    {
        public string BranchId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }



    public class LpDeductionSummary
    {
        public decimal ExcessLoanBalance { get; set; }
        public decimal LoansOverAgeThreshold { get; set; }
        public decimal LoansToOrganizations { get; set; }
        public decimal OtherLPDeductions { get; set; }
        public decimal TotalLPDeductions { get; set; }
        public decimal LPInsurableAmount { get; set; }
        public decimal LPPremiumDue { get; set; }
    }

    public class LsDeductionSummary
    {
        public decimal ExcessSavingsBalance { get; set; }
        public decimal OtherLSDeductions { get; set; }
        public decimal TotalLSDeductions { get; set; }
        public decimal LSInsurableAmount { get; set; }
        public decimal LSPremiumDue { get; set; }
    }

    public class CombinedSummary
    {
        public decimal TotalOutstandingLoans { get; set; }
        public decimal TotalSharesAndSavings { get; set; }
        public decimal TotalInsurableAmount { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal TotalPremiumsDue { get; set; }
    }

    public class InsurancePremiumData
    {
        public string BranchId { get; set; }
        public string BranchName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime GeneratedDate { get; set; }
        public string Currency { get; set; }
        public decimal MaxLoanProtectionAmount { get; set; }
        public decimal LoanProtectionRate { get; set; }
        public int MaxAgeForCoverage { get; set; }
        public decimal MaxLifeSavingsAmount { get; set; }
        public decimal LifeSavingsRate { get; set; }
        public string IpsConfigId { get; set; }
        public int IpsConfigYear { get; set; }
        public string IpsConfigBranchId { get; set; }
        public string IpsConfigBranchName { get; set; }
        public int LpTotalNumberOfOutstandingLoans { get; set; }
        public decimal LpTotalAmountOfOutstandingLoans { get; set; }
        public decimal LpExcessLoanBalance { get; set; }
        public decimal LpLoansOverAgeThreshold { get; set; }
        public decimal LpLoansToOrganizations { get; set; }
        public List<object> LpOtherDeductions { get; set; } = new List<object>();
        public decimal LpTotalOtherDeductions { get; set; }
        public decimal LpTotalDeductions { get; set; }
        public decimal LpTotalFromReverseSide { get; set; }
        public decimal LpInsurableLoans { get; set; }
        public decimal LpPremiumDue { get; set; }
        public int LsTotalNumberOfMembers { get; set; }
        public decimal LsTotalSharesAndSavings { get; set; }
        public decimal LsExcessBalance { get; set; }
        public List<object> LsoOtherDeductions { get; set; } = new List<object>();
        public decimal LsTotalOtherDeductions { get; set; }
        public decimal LsTotalDeductions { get; set; }
        public decimal LsTotalFromReverseSide { get; set; }
        public decimal LsInsurableSharesAndSavings { get; set; }
        public decimal LsPremiumDue { get; set; }
        public decimal LsPremiumDueLeftColumn { get; set; }
        public string LifeSavingsClaimDebitAccountId { get; set; }
        public string LifeSavingsClaimCreditAccountId { get; set; }
        public string LoanProtectionDebitAccountId { get; set; }
        public string LoanProtectionCreditAccountId { get; set; }
        public decimal TotalPremiumsDue { get; set; }
        public decimal TotalInsurableAmount { get; set; }
        public decimal TotalDeductions { get; set; }
        public LpDeductionSummary LpDeductionSummary { get; set; }
        public LsDeductionSummary LsDeductionSummary { get; set; }
        public CombinedSummary CombinedSummary { get; set; }
    }

    public class RootResponse
    {
        public InsurancePremiumData Data { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public List<object> Errors { get; set; } = new List<object>();
        public bool Success { get; set; }
    }



    /// <summary>
    /// Main DTO for combined Insurance Premium calculations including both
    /// Loan Protection (Protection du Pret) and Life Savings (Epargnes Vie).
    /// </summary>
    public class InsurancePremiumsDto
    {
        // ==================== HEADER INFORMATION ====================
        public string BranchId { get; set; }
        public string BranchName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime GeneratedDate { get; set; }
        public string Currency { get; set; } = "CFA";

        // ==================== CONFIGURATION VALUES ====================
        // Loan Protection Configuration
        public decimal MaxLoanProtectionAmount { get; set; }
        public decimal LoanProtectionRate { get; set; }
        public int MaxAgeForCoverage { get; set; }

        // Life Savings Configuration
        public decimal MaxLifeSavingsAmount { get; set; }
        public decimal LifeSavingsRate { get; set; }

        // ==================== LOAN PROTECTION (LEFT SIDE) ====================
        /// <summary>
        /// LINE 1 (LP): Total number of outstanding loans
        /// </summary>
        public int LPTotalNumberOfOutstandingLoans { get; set; }

        /// <summary>
        /// LINE 2 (LP): Total Amount of Outstanding Loans
        /// </summary>
        public decimal LPTotalAmountOfOutstandingLoans { get; set; }

        // Reverse Side Deductions - Loan Protection
        /// <summary>
        /// a. Loan balances in excess of LP Maxi Contract
        /// </summary>
        public decimal LPExcessLoanBalance { get; set; }

        /// <summary>
        /// b. Loan balances of members over age threshold
        /// </summary>
        public decimal LPLoansOverAgeThreshold { get; set; }

        /// <summary>
        /// c. Loans to clubs, Organisations and Unions
        /// </summary>
        public decimal LPLoansToOrganizations { get; set; }

        /// <summary>
        /// d. Other LP deductions with details
        /// </summary>
        public List<LoanProtectionOtherDeductionItem> LPOtherDeductions { get; set; } = new List<LoanProtectionOtherDeductionItem>();
        public decimal LPTotalOtherDeductions { get; set; }

        /// <summary>
        /// e. Total LP Deductions (a + b + c + d)
        /// </summary>
        public decimal LPTotalDeductions { get; set; }

        /// <summary>
        /// LINE 3 (LP): Total from reverse side (point e)
        /// </summary>
        public decimal LPTotalFromReverseSide { get; set; }

        /// <summary>
        /// LINE 4 (LP): Insurable loans (line 2 minus line 3)
        /// </summary>
        public decimal LPInsurableLoans { get; set; }

        /// <summary>
        /// LINE 5 (LP): LP Premium Due (Rate times line 4)
        /// </summary>
        public decimal LPPremiumDue { get; set; }

        // ==================== LIFE SAVINGS (RIGHT SIDE) ====================
        /// <summary>
        /// LINE 1 (LS): Total number of members
        /// </summary>
        public int LSTotalNumberOfMembers { get; set; }

        /// <summary>
        /// LINE 2 (LS): Total Shares and Savings
        /// </summary>
        public decimal LSTotalSharesAndSavings { get; set; }

        // Reverse Side Deductions - Life Savings
        /// <summary>
        /// a. Life Savings balance in excess of LS Contract
        /// </summary>
        public decimal LSExcessBalance { get; set; }

        /// <summary>
        /// b. Shares & Savings of clubs, organizations & unions
        /// </summary>
        // public  LSOrganizationalAccounts { get; set; }

        /// <summary>
        /// c. Other LS deductions with details
        /// </summary>
        public List<LifeSavingsOtherDeductionItem> LSOOtherDeductions { get; set; } = new List<LifeSavingsOtherDeductionItem>();
        public decimal LSTotalOtherDeductions { get; set; }

        /// <summary>
        /// d. Total LS deductions (a + b + c)
        /// </summary>
        public decimal LSTotalDeductions { get; set; }

        /// <summary>
        /// LINE 3 (LS): Total from reverse side (point d)
        /// </summary>
        public decimal LSTotalFromReverseSide { get; set; }

        /// <summary>
        /// LINE 4 (LS): Insurable Shares and Savings (line 2 minus line 3)
        /// </summary>
        public decimal LSInsurableSharesAndSavings { get; set; }

        /// <summary>
        /// LINE 5 (LS): LS Premium Due (Rate times line 4)
        /// </summary>
        public decimal LSPremiumDue { get; set; }

        /// <summary>
        /// LINE 6 (LS): LS Premium Due (left column) - For compatibility
        /// </summary>
        public decimal LSPremiumDueLeftColumn { get; set; }

        // ==================== COMBINED TOTALS ====================
        /// <summary>
        /// LINE 7: Total Premiums Due (line 5 LP + line 5 LS)
        /// </summary>
        public decimal TotalPremiumsDue => LPPremiumDue + LSPremiumDue;

        /// <summary>
        /// Grand Total of all insurable amounts (LP Insurable + LS Insurable)
        /// </summary>
        public decimal TotalInsurableAmount => LPInsurableLoans + LSInsurableSharesAndSavings;

        /// <summary>
        /// Grand Total of all deductions (LP Total Deductions + LS Total Deductions)
        /// </summary>
        public decimal TotalDeductions => LPTotalDeductions + LSTotalDeductions;

        // ==================== SUMMARY DICTIONARIES ====================
        /// <summary>
        /// Loan Protection deduction summary for reporting
        /// </summary>
        public Dictionary<string, decimal> LPDeductionSummary => new Dictionary<string, decimal>()
        {
            ["Excess Loan Balance"] = LPExcessLoanBalance,
            ["Loans Over Age Threshold"] = LPLoansOverAgeThreshold,
            ["Loans to Organizations"] = LPLoansToOrganizations,
            ["Other LP Deductions"] = LPTotalOtherDeductions,
            ["Total LP Deductions"] = LPTotalDeductions,
            ["LP Insurable Amount"] = LPInsurableLoans,
            ["LP Premium Due"] = LPPremiumDue
        };

        /// <summary>
        /// Life Savings deduction summary for reporting
        /// </summary>
        public Dictionary<string, decimal> LSDeductionSummary => new Dictionary<string, decimal>()
        {
            ["Excess Savings Balance"] = LSExcessBalance,
            // ["Organizational Accounts"] = LSOrganizationalAccounts,
            ["Other LS Deductions"] = LSTotalOtherDeductions,
            ["Total LS Deductions"] = LSTotalDeductions,
            ["LS Insurable Amount"] = LSInsurableSharesAndSavings,
            ["LS Premium Due"] = LSPremiumDue
        };

        /// <summary>
        /// Combined summary for overall reporting
        /// </summary>
        public Dictionary<string, decimal> CombinedSummary => new Dictionary<string, decimal>()
        {
            ["Total Outstanding Loans"] = LPTotalAmountOfOutstandingLoans,
            ["Total Shares & Savings"] = LSTotalSharesAndSavings,
            ["Total Insurable Amount"] = TotalInsurableAmount,
            ["Total Deductions"] = TotalDeductions,
            ["Total Premiums Due"] = TotalPremiumsDue
        };
    }


    /// <summary>
    /// Item for Loan Protection other deductions
    /// </summary>
    public class LoanProtectionOtherDeductionItem
    {
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string LoanId { get; set; }
        public string CustomerName { get; set; }
        public string LoanContractCode { get; set; }
        public string DeductionType { get; set; }
    }

    /// <summary>
    /// Item for Life Savings other deductions
    /// </summary>
    public class LifeSavingsOtherDeductionItem
    {
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string AccountId { get; set; }
        public string AccountNumber { get; set; }
        public string CustomerName { get; set; }
        public string DeductionType { get; set; }
    }

    public class InsurancePremiumDto
    {
        public string BranchId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ReportType { get; set; }
        public string PrintOption { get; set; }
    }

    public class IPSflatobject
    {
        public string BankName { get; set; }
        public string BankCode { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public string BranchPOBox { get; set; }
        public string BranchTell { get; set; }
        public string BranchFax { get; set; }
        public string NameOfCreditUnion { get; set; }
        public string Address { get; set; }
        public int ContractNumber { get; set; }
        public DateTime ReportForTheMonthOf { get; set; }
        public decimal TotalAmountOfOutstandingLoans { get; set; }
        public decimal TotalFromReverseSide { get; set; }
        public decimal InsurableLoans { get; set; }
        public decimal LPPremiumDue { get; set; }
        public decimal TotalNumberOfMembers { get; set; }
        public decimal TotalSharesandSavings { get; set; }
        public decimal TotatFromReverseSideLifeS { get; set; }
        public decimal LSPDRateTimeLine4 { get; set; }
        public decimal LSPDLine5leftcolumn { get; set; }
        public decimal TPDLine5plusLine6 { get; set; }
        public decimal LBIEOFLP { get; set; }
        public decimal LBOM { get; set; }
        public decimal LTCOAU { get; set; }
        public decimal OLPD { get; set; }
        public decimal TLD { get; set; }
        public decimal LSBIEOFLS { get; set; }
        public decimal SSOCOU { get; set; }
        public decimal OLSD { get; set; }
        public decimal TLSD { get; set; }
        public string Logo { get; set; }
        public string PrintedBy { get; set; }
        public DateTime PrintedOn { get; set; }
        public string TotalNumberOfOutstandingLoans { get; set; }
        public string InsurableSharesAndSaving { get; set; }
    }

}

