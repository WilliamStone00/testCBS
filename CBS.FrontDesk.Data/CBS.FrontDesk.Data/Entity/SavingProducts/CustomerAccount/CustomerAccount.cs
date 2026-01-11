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


}
