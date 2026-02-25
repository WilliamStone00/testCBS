using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountOperation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity
{



    public class CustomerAccount
    {
        public string id { get; set; }
        public string accountNumber { get; set; }
        public decimal balance { get; set; }
        public decimal previousBalance { get; set; } // Added
        public string status { get; set; } // Added
        public string productId { get; set; }
        public string customerId { get; set; }
        public string tellerId { get; set; } // Added
        public string branchCode { get; set; } // Added
        public string encryptedBalance { get; set; } // Added
        public decimal interestGenerated { get; set; }
        public decimal lastInterestPosted { get; set; } // Added
        public decimal blockedAmount { get; set; } // Added
        public string blockedId { get; set; } // Added
        public DateTime dateBlocked { get; set; } // Added
        public DateTime dateReleased { get; set; } // Added
        public string reasonOfBlocked { get; set; } // Added
        public string accountName { get; set; } // Added
        public string lastOperation { get; set; } // Added
        public string accountType { get; set; } // Added
        public bool isTellerAccount { get; set; } // Added
        public decimal openingBalance { get; set; } // Added
        public string openningOfDayStatus { get; set; } // Added
        public DateTime openningOfDayDate { get; set; } // Added
        public string openningOfDayReference { get; set; } // Added
        public decimal lastOperationAmount { get; set; } // Added
        public string bankId { get; set; } // Added
        public string branchId { get; set; } // Added
        public string modifiedBy { get; set; }
        public string customerName { get; set; }
        public string createdBy { get; set; }
        public DateTime createdDate { get; set; }
        public DateTime modifiedDate { get; set; }
        public DateTime dateOfOpeningBalance { get; set; } // Added
        public DateTime dateOfLastOperation { get; set; }
        public DateTime lastInterestCalculatedDate { get; set; }
        public SavingProduct product { get; set; }
        public List<WithdrawalNotification> WithdrawalNotifications { get; set; }
        public List<TransactionHistory> Transactions { get; set; }
    }

    //public class CustomerAccount
    //{
    //    public string id { get; set; }
    //    public string accountNumber { get; set; }
    //    public decimal balance { get; set; }
    //    public decimal previousBalance { get; set; }
    //    public string status { get; set; }
    //    public string productId { get; set; }
    //    public string customerId { get; set; }
    //    public string tellerId { get; set; }
    //    public string encryptedBalance { get; set; }
    //    public decimal interestGenerated { get; set; }
    //    public decimal lastInterestPosted { get; set; }
    //    public decimal blockedAmount { get; set; }
    //    public string blockedId { get; set; }
    //    public string reasonOfBlocked { get; set; }
    //    public string accountName { get; set; }
    //    public string lastOperation { get; set; }
    //    public string accountType { get; set; }
    //    public bool isTellerAccount { get; set; }
    //    public decimal openingBalance { get; set; }
    //    public decimal lastOperationAmount { get; set; }
    //    public string bankId { get; set; }
    //    public string branchId { get; set; }
    //    public string modifiedBy { get; set; }
    //    public string customerName { get; set; }
    //    public string createdBy { get; set; }
    //    public DateTime createdDate { get; set; }
    //    public DateTime modifiedDate { get; set; }
    //    public DateTime dateOfOpeningBalance { get; set; }
    //    public DateTime dateBlocked { get; set; }
    //    public DateTime dateReleased { get; set; }
    //    public DateTime dateOfLastOperation { get; set; }
    //    public DateTime lastInterestCalculatedDate { get; set; }
    //    public SavingProduct product { get; set; }
    //    public List<WithdrawalNotification> WithdrawalNotifications { get; set; }
    //    public List<TransactionHistory> transactions { get; set; }
    //}
    public class CustomerAccountDto
    {
        public string accountId { get; set; }
        public string accountNumber { get; set; }
        public string status { get; set; }
        public string productName { get; set; }
        public string productId { get; set; }
        public string customerId { get; set; }
        public string customerName { get; set; }
        public string createdDate { get; set; }
        public string BranchName { get; set; }
        public string BranchId { get; set; }
        public string phone { get; set; }
        public string createdBy { get; set; }
        public string customerCode { get; set; }
    }
    public class MemberAccountDto
    {
        public string Id { get; set; }
        public string AccountNumber { get; set; }
        public string AccountType { get; set; }
        public string AccountName { get; set; }
        public string Status { get; set; }
        public decimal Balance { get; set; }
        public decimal BlockedBalance { get; set; }
        public string Currency { get; set; }
    }

    public class TransferLimit
    {
        public string id { get; set; }
        public string productId { get; set; }
        public string transferType { get; set; }
        public double minAmount { get; set; }
        public double maxAmount { get; set; }
        public double feePercentage { get; set; }
        public DateTime createdDate { get; set; }
        public string createdBy { get; set; }
        public DateTime modifiedDate { get; set; }
        public string modifiedBy { get; set; }
        public DateTime deletedDate { get; set; }
        public string deletedBy { get; set; }
        public double objectState { get; set; }
        public bool isDeleted { get; set; }
    }

    public class WithdrawalLimit
    {
        public string id { get; set; }
        public string productId { get; set; }
        public double minAmount { get; set; }
        public double maxAmount { get; set; }
        public string withdrawalType { get; set; }
        public double feePercentage { get; set; }
        public DateTime createdDate { get; set; }
        public string createdBy { get; set; }
        public DateTime modifiedDate { get; set; }
        public string modifiedBy { get; set; }
        public DateTime deletedDate { get; set; }
        public string deletedBy { get; set; }
        public double objectState { get; set; }
        public bool isDeleted { get; set; }
    }

    public class Accountgetid
    {
        public string Id { get; set; }
        public string AccountNumber { get; set; }
        public decimal Balance { get; set; } = 0;
        public decimal PreviousBalance { get; set; } = 0;
        public string Status { get; set; } 
        public string ProductId { get; set; }
        public string CustomerId { get; set; }
        public string TellerId { get; set; }
        public string BranchCode { get; set; }
        public string CustomerName { get; set; }
        public string EncryptedBalance { get; set; }
        public decimal InterestGenerated { get; set; } = 0;
        public decimal LastInterestPosted { get; set; } = 0;
        public decimal BlockedAmount { get; set; } = 0;
        public string BlockedId { get; set; }
        public string ProfileType { get; set; }
        public DateTime DateBlocked { get; set; } = DateTime.MinValue;
        public DateTime DateReleased { get; set; } = DateTime.MinValue;
        public string ReasonOfBlocked { get; set; }
        //public decimal TellerInterestBalance { get; set; }
        public DateTime? DateOfLastOperation { get; set; } = DateTime.MinValue;
        public DateTime? LastInterestCalculatedDate { get; set; } = DateTime.MinValue;
        public string AccountName { get; set; }
        public string LastOperation { get; set; }
        public string AccountType { get; set; }
        public bool IsTellerAccount { get; set; }
        public string OpenningOfDayStatus { get; set; }
        public DateTime? OpenningOfDayDate { get; set; } = DateTime.MinValue;
        public string OpenningOfDayReference { get; set; }
        public decimal OpeningBalance { get; set; }
        public DateTime? DateOfOpeningBalance { get; set; } = DateTime.MinValue;
        public decimal LastOperationAmount { get; set; } = 0;
        public string BankId { get; set; }
        public string BranchId { get; set; }
        public string BranchName { get; set; }
        public string BankName { get; set; }
        public string BranchPrintingdetails { get; set; }
        public string PrintedBy { get; set; }
        public DateTime? printedOn { get; set; }
        public SavingProduct Product { get; set; }
        public List<WithdrawalNotification> WithdrawalNotifications { get; set; }

        /// <summary>Last closure reason applied to this account (for BI and compliance quick filters).</summary>
        public string LastClosureReason { get; set; }

        /// <summary>UTC time when account was finally closed (null if open).</summary>
        public DateTime? ClosedOn { get; set; } = DateTime.MinValue;
        public bool IsOverdraftEnabled { get; set; } = false;
        // ✅ Has the overdraft service been activated for this account?
        // This flag must be true before any OD request can be submitted.

        public decimal OverdraftLimit { get; set; } = 0;
        // ✅ The maximum limit assigned to this account (live usable OD limit).
        // Can be changed after approval by LM.

        public decimal OverdraftUsed { get; set; } = 0;
        // ✅ Amount already consumed from the overdraft limit.
        // Used for interest calculations, blocking thresholds, utilization %

        public decimal InterestRate { get; set; }
        // ✅ Interest rate applied on the used overdraft amount (monthly or annual)

        public decimal PenaltyRate { get; set; }
        // ✅ Additional rate or fee applied if overdraft becomes delinquent or overdrawn

        public string LinkedSalaryAccountId { get; set; }
        // ✅ (Optional) Account where salary is paid to.
        // If set, OD can be auto-recovered when salary is deposited.

        public string RecoverySourceAccountIds { get; set; }
        // ✅ (Optional) List of accounts used as backup recovery sources (group wallet, spouse, business partner)

        public bool AutoRecoverOnDeposit { get; set; } = true;
        // ✅ Should the OD be auto-cleared when new deposits enter the account?

        public DateTime? OverdraftActivationDate { get; set; }
        // ✅ When this OD feature was enabled on the account.

        public DateTime? CreatedDate { get; set; }
        public string AccountManager { get; set; }

        public DateTime? OverdraftExpiryDate { get; set; }
        // ✅ Optional: When this OD feature expires (e.g. 6-month promo or renewable OD)

        public bool IsBlocked { get; set; } = false;
        // ✅ If blocked, account cannot draw or use overdraft (due to default or risk event)

        public string ActivatedBy { get; set; }
        // ✅ User, teller, or officer ID who activated OD on the account (audit trail)

        public bool IsActive { get; set; } = true;
        // ✅ Overall activation record toggle — can be deactivated for auditing or archiving

        // 🧮 Helper (not mapped in DB)
        public decimal AvailableOverdraft => OverdraftLimit - OverdraftUsed;

        public decimal AvailableBalance =>
            IsOverdraftEnabled ? (Balance >= 0 ? Balance : OverdraftLimit + Balance) : Balance;
        public bool IsSalaryOverdraftAccount { get; set; }
        public bool IsCashInBlocked { get; set; }
        public bool IsWithdrawalBlocked { get; set; }
        public bool IsTransferBlocked { get; set; }

    }
}
