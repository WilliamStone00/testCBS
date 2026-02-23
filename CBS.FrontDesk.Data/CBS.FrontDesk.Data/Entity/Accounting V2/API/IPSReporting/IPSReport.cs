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
        //public Dictionary<string, decimal> LPDeductionSummary => new()
        //{
        //    ["Excess Loan Balance"] = LPExcessLoanBalance,
        //    ["Loans Over Age Threshold"] = LPLoansOverAgeThreshold,
        //    ["Loans to Organizations"] = LPLoansToOrganizations,
        //    ["Other LP Deductions"] = LPTotalOtherDeductions,
        //    ["Total LP Deductions"] = LPTotalDeductions,
        //    ["LP Insurable Amount"] = LPInsurableLoans,
        //    ["LP Premium Due"] = LPPremiumDue
        //};

        ///// <summary>
        ///// Life Savings deduction summary for reporting
        ///// </summary>
        //public Dictionary<string, decimal> LSDeductionSummary => new()
        //{
        //    ["Excess Savings Balance"] = LSExcessBalance,
        //    // ["Organizational Accounts"] = LSOrganizationalAccounts,
        //    ["Other LS Deductions"] = LSTotalOtherDeductions,
        //    ["Total LS Deductions"] = LSTotalDeductions,
        //    ["LS Insurable Amount"] = LSInsurableSharesAndSavings,
        //    ["LS Premium Due"] = LSPremiumDue
        //};

        ///// <summary>
        ///// Combined summary for overall reporting
        ///// </summary>
        //public Dictionary<string, decimal> CombinedSummary => new()
        //{
        //    ["Total Outstanding Loans"] = LPTotalAmountOfOutstandingLoans,
        //    ["Total Shares & Savings"] = LSTotalSharesAndSavings,
        //    ["Total Insurable Amount"] = TotalInsurableAmount,
        //    ["Total Deductions"] = TotalDeductions,
        //    ["Total Premiums Due"] = TotalPremiumsDue
        //};
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
        public string  ReportType { get; set; }
        
    }


}
