using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.ReportDataSetDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.GeneralStatisticReport
{
    public class GeneralStatisticsReportRPT: HeadOffice
    {
        public MemberStatistics MembersWithAccounts { get; set; }
        public List<LiquidityRatioChartRow> LiquidityRatioChart { get; set; } = new List<LiquidityRatioChartRow>();

        public List<AccountTypeGenderSummary> AccountTypeSummaries { get; set; } = new List<AccountTypeGenderSummary>();
        public List<LoanCategoryGenderSummary> ParSummaries { get; set; } = new List<LoanCategoryGenderSummary>();

        /// <summary>
        /// Unified list of loan statistics per loan category (e.g., Outstanding, Granted This Month).
        /// </summary>
        public List<LoanCategoryGenderSummary> LoanSummaries { get; set; } = new List<LoanCategoryGenderSummary>();

        public List<LoanBreakdownItem> LoanBreakdowns { get; set; }

        public List<MissingCustomerInfo> MissingCustomers { get; set; } = new List<MissingCustomerInfo>();
        public List<CustomerBasicInfoDto> MembersDetails { get; set; } = new List<CustomerBasicInfoDto>();
        public GenderDistributionSummary CustomerGenderSummary { get; set; } = new GenderDistributionSummary();
       
        public List<LoanCategoryGenderSummary> LoanCategorySummaries { get; set; }= new List<LoanCategoryGenderSummary>();
        public List<LoanCategoryGenderSummary> LoanTypeSummaries { get; set; } = new List<LoanCategoryGenderSummary>();
        public List<LoanCategoryGenderSummary> LoanTargetSummaries { get; set; }= new List<LoanCategoryGenderSummary>();
        public List<GeneralStatisticsFlatRow> GeneralStatisticsFlatRow { get; set; } = new List<GeneralStatisticsFlatRow>();
        public List<LoanCategoryGenderSummary> LoanDeliquentStatusByGenderSummaries { get; set; } = new List<LoanCategoryGenderSummary>();

    }
    public class LoanDeliquentStatusByGenderSummary
    {
        public string Category { get; set; }

        public int MenCount { get; set; }
        public int WomenCount { get; set; }
        public int GroupsCount { get; set; }
        public int TotalCount => MenCount + WomenCount + GroupsCount;

        public decimal MenAmount { get; set; }
        public decimal WomenAmount { get; set; }
        public decimal GroupsAmount { get; set; }
        public decimal TotalAmount => MenAmount + WomenAmount + GroupsAmount;
        public decimal CountPercentage { get; set; }
        public decimal AmountPercentage { get; set; }
    }

    public class LiquidityRatioChartRow
    {
        public decimal TotalSavings { get; set; }
        public decimal TotalLoanBalance { get; set; }
        public double LiquidityRatio { get; set; } // In percentage (Loan / Savings * 100)
        public string ComplianceComment { get; set; } // "Compliant" or "⚠️ Risk of Liquidity Shortfall"
        public string Label => $"Ratio as of {DateTime.Now:dd MMM yyyy}";
        public decimal Difference { get; internal set; }
        public string LiquidityInsightAdvice { get; internal set; }
    }


    public class LiquidityRatioInfo
    {
        public decimal TotalLoanBalance { get; set; }
        public decimal TotalSavingsBalance { get; set; }
        public decimal Difference => TotalSavingsBalance - TotalLoanBalance;
        public double LoanToSavingsRatio => TotalSavingsBalance > 0
            ? Math.Round((double)(TotalLoanBalance / TotalSavingsBalance) * 100, 2)
            : 0;

        public string ComplianceMessage => LoanToSavingsRatio <= 75
            ? "✅ Compliant: Savings exceed loans, providing a strong liquidity buffer."
            : "❗ Alert: Loans exceed 75% of savings. Liquidity risk may be elevated.";
    }

    public class GeneralStatisticsReportRPTx : HeadOffice
    {
        public MemberStatistics MembersWithAccounts { get; set; }

        public List<AccountTypeGenderSummary> AccountTypeSummaries { get; set; } = new List<AccountTypeGenderSummary>();
        /// <summary>
        /// Unified list of loan statistics per loan category (e.g., Outstanding, Granted This Month).
        /// </summary>
        public List<LoanCategoryGenderSummary> LoanSummaries { get; set; } = new List<LoanCategoryGenderSummary>();

        public List<LoanBreakdownItem> LoanBreakdowns { get; set; }

        public GenderDistributionSummary CustomerGenderSummary { get; set; } = new GenderDistributionSummary();

        public List<LoanCategoryGenderSummary> LoanCategorySummaries { get; set; } = new List<LoanCategoryGenderSummary>();
        public List<LoanCategoryGenderSummary> LoanTypeSummaries { get; set; } = new List<LoanCategoryGenderSummary>();
        public List<LoanCategoryGenderSummary> LoanTargetSummaries { get; set; } = new List<LoanCategoryGenderSummary>();
        public List<GeneralStatisticsFlatRow> GeneralStatisticsFlatRow { get; set; } = new List<GeneralStatisticsFlatRow>();

    }
    public class GeneralStatisticsFlatRow
    {
        public string Title { get; set; }

        public int MenCount { get; set; }
        public int WomenCount { get; set; }
        public int GroupsCount { get; set; }
        public int TotalCount => MenCount + WomenCount + GroupsCount;

        public decimal MenAmount { get; set; }
        public decimal WomenAmount { get; set; }
        public decimal GroupsAmount { get; set; }
        public decimal TotalAmount => MenAmount + WomenAmount + GroupsAmount;

        public string DataGroup { get; set; } // e.g. Account, LoanSummary, etc.

        public double TotalCountPercentage { get; set; }
        public double TotalAmountPercentage { get; set; }
        public string Comment { get; set; }

    }

    public class AccountTypeGenderSummary
    {
        public string AccountType { get; set; }

        // Customer counts
        public int MenCount { get; set; }
        public int WomenCount { get; set; }
        public int GroupsCount { get; set; }
        public int TotalCount => MenCount + WomenCount + GroupsCount;

        // Balances
        public decimal MenAmount { get; set; }
        public decimal WomenAmount { get; set; }
        public decimal GroupsAmount { get; set; }
        public decimal TotalAmount => MenAmount + WomenAmount + GroupsAmount;
    }
    public class LoanCategoryGenderSummary
    {
        public string Category { get; set; } // e.g., "LoansOutstanding", "LoansGrantedThisMonth"

        public int MenCount { get; set; }
        public int WomenCount { get; set; }
        public int GroupsCount { get; set; }
        public int TotalCount => MenCount + WomenCount + GroupsCount;

        public decimal MenAmount { get; set; }
        public decimal WomenAmount { get; set; }
        public decimal GroupsAmount { get; set; }
        public decimal TotalAmount => MenAmount + WomenAmount + GroupsAmount;
    }
    public class MissingCustomerInfo : CustomerBasicInfoDto
    {
        public string Reason { get; set; }
    }
    public class GenderDistributionSummary
    {
        public int Men { get; set; }
        public int Women { get; set; }
        public int Groups { get; set; }

        public int Total => Men + Women + Groups;

        public double MenPercentage => Total > 0 ? Math.Round((double)Men / Total * 100, 2) : 0;
        public double WomenPercentage => Total > 0 ? Math.Round((double)Women / Total * 100, 2) : 0;
        public double GroupsPercentage => Total > 0 ? Math.Round((double)Groups / Total * 100, 2) : 0;

        public double TotalPercentage { get => MenPercentage + WomenPercentage + GroupsPercentage; private set { } }

    }
    public class MemberStatistics
    {
        public int Men { get; set; }
        public int Women { get; set; }
        public int Groups { get; set; }
        public int Missing => MissingDetails.Count;
        public int Total => Men + Women + Groups;

        public List<MissingCustomerInfo> MissingDetails { get; set; } = new List<MissingCustomerInfo>();
    }
    public class CustomerBasicInfoDto : HeadOffice
    {
        public string CustomerId { get; set; }
        public string LegalForm { get; set; }
        public string Gender { get; set; }
        public string PhoneNumber { get; set; }
        public string FullName { get; set; }
        public int Age { get; set; }
        public string MemberProfileType { get; set; }
        public bool Active { get; set; }
        public string ApprovalStatus { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string BranchId { get; set; }
    }
    public class LoanBreakdownItem
    {
        public string Category { get; set; }

        public int Men { get; set; }
        public int Women { get; set; }
        public int Groups { get; set; }

        public decimal MenAmount { get; set; }
        public decimal WomenAmount { get; set; }
        public decimal GroupsAmount { get; set; }

        public int Total => Men + Women + Groups;
        public decimal TotalAmount => MenAmount + WomenAmount + GroupsAmount;
    }
    public class GeneralStatisticsDashboard
    {
        public string BranchName { get; set; }
        public string DateRange { get; set; }

        public MemberStatistics MembersWithAccounts { get; set; }
        public List<AccountTypeGenderSummary> AccountTypeSummaries { get; set; } = new List<AccountTypeGenderSummary>();
        public List<LoanCategoryGenderSummary> LoanSummaries { get; set; } = new List<LoanCategoryGenderSummary>();
        public List<LoanBreakdownItem> LoanBreakdowns { get; set; } = new List<LoanBreakdownItem>();
        public List<CustomerBasicInfoDto> MembersDetails { get; set; } = new List<CustomerBasicInfoDto>();
        public List<MissingCustomerInfo> MissingCustomers { get; set; } = new List<MissingCustomerInfo>();
        public GenderDistributionSummary CustomerGenderSummary { get; set; } = new GenderDistributionSummary();
        public List<LoanCategoryGenderSummary> LoanTypeSummaries { get; set; } = new List<LoanCategoryGenderSummary>();
        public List<LoanCategoryGenderSummary> LoanTargetSummaries { get; set; } = new List<LoanCategoryGenderSummary>();
        public List<LoanCategoryGenderSummary> LoanCategorySummaries { get; set; } = new List<LoanCategoryGenderSummary>();

    }

    public class GenerateGeneralStatisticsReportQuery
    {
        public string BranchId { get; set; }

        // Automatically loads all account types
        public List<string> AccountTypes { get; set; } = Enum.GetNames(typeof(AccountType)).ToList();

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Loan status filter: e.g., "active", "closed", "all"
        public string LoanStatus { get; set; } = "all";

        // Allows multiple account profiles (e.g., MemberAccount, GroupAccount)
        public List<string> AccountProfiles { get; set; } = AccountProfileTypes.Default;
        public string MainReportType { get; set; }
    }
    public static class AccountProfileTypes
    {
        public const string MemberAccount = "MemberAccount";
        public const string DailyCollection = "DailyCollection";
        public const string MobileMoneyMTN = "MobileMoneyMTN";
        public const string MobileMoneyOrange = "MobileMoneyOrange";
        public const string Remittance = "Remittance";
        public const string CivilServantsSalaryCollection = "CivilServantsSalaryCollection";
        public const string PrivateInstitutionSalaryCollection = "PrivateInstitutionSalaryCollection";
        public const string UnknownSalaryMemberAccount = "UnknownSalaryMemberAccount";
        public const string AffiliateMemberSalaryAccount = "AffiliateMemberSalaryAccount";
        public const string PreRegistredMember = "PreRegistredMember";

        // Default selection for report query
        public static List<string> Default => new List<string>
        {
            MemberAccount
        };

        // Full list for dropdowns or filters
        public static List<string> All => new List<string>
        {
            MemberAccount,
            DailyCollection,
            MobileMoneyMTN,
            MobileMoneyOrange,
            Remittance,
            CivilServantsSalaryCollection,
            PrivateInstitutionSalaryCollection,
            UnknownSalaryMemberAccount,
            AffiliateMemberSalaryAccount,
            PreRegistredMember
        };
    }



    public enum AccountType
    {
        PreferenceShare,
        MemberShare,
        Deposit,
        Saving, Salary,
        Loan,
        Atm,
        Gav,
        DailyCollection,
        WesternUnion,
        MoneyGram,
        Ria,
        OFX,
        MPesa,
        Payoneer,
        WorldRemit,
        Membership,
        MobileMoneyMTN,
        MobileMoneyORANGE,
        Teller,
        MomocashCollectionMTN,
        MomocashCollectionOrange,
    }

 
}
