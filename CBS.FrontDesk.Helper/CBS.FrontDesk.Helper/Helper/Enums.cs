using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Helper
{
    public enum PolicyCode
    {
        TECH01
    }
    public enum AccountTypeEnumerations
    {
        PreferenceShare,
        MemberShare,
        Deposit,
        Saving,
        Loan,
        Atm,
        Gav,
        DailyCollection,
        Membership,
        MobileMoneyMTN,
        MobileMoneyORANGE,
        Teller,
        MomocashCollectionMTN,
        MomocashCollectionOrange,
    }
    public enum DashboardAccountingType
    {
        CashInHand,
        CashInBank,
        CashInVault,
        MTNMobileMoneyMaster,
        PreferenceShare,
        OrdinaryShares,
        Deposit, Savings,
        Gav,
        DailyCollections,
        MTNMobileMoney,
        OrangeMoneyMaster,
        OrangeMoney,
        TotalLiquidity,
        TotalExpense,
        TotalIncome
    }
    public enum InsuranceStatus
    {
        WAITING_PERIOD,
        SAVING_NOT_COMPLETED,
        INSURED, DEFAULT
    }
    public enum SMSTypes
    {
        Saving, Interst, SavingTargetReached,
        Cashout
    }
    public enum ServiceTypes
    {
        ClientMicroService,
        LoanMicroService,
        AccountMicroService,
        ClaimMicroService
    }
    public enum TransactionType
    {
        Saving,
        Withdrawal,
        Penalty,
        Loan, 
        Refund,
        InterestCalculation
    }
    public enum ResultStatus
    {
        Ok,
        Failed, Pending,Successful
       
    }
}
