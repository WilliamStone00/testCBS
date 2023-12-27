using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity
{


    public class DepositLimit
    {
        public string id { get; set; }
        public string productId { get; set; }
        public string depositType { get; set; }
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
        public string product { get; set; }
    }

    public class Product
    {
        public string id { get; set; }
        public string name { get; set; }
        public double minAmount { get; set; }
        public double maxAmount { get; set; }
        public double yearlyInterestRate { get; set; }
        public string interestCalculationFrequency { get; set; }
        public string postingFrequency { get; set; }
        public bool isTerm { get; set; }
        public List<DepositLimit> depositLimits { get; set; }
        public List<WithdrawalLimit> withdrawalLimits { get; set; }
        public List<TransferLimit> transferLimits { get; set; }
        public double entryFee { get; set; }
        public double reopeningFee { get; set; }
        public double closingFee { get; set; }
        public double managementFee { get; set; }
        public double interestRate { get; set; }
        public DateTime termEndDate { get; set; }
        public DateTime termStartDate { get; set; }
        public DateTime createdDate { get; set; }
        public string createdBy { get; set; }
        public string managementFeeFrequency { get; set; }
    }

    public class CustomerAccount
    {
        public string id { get; set; }
        public string accountNumber { get; set; }
        public string balance { get; set; }
        public string previousBalance { get; set; }
        public string status { get; set; }
        public string productId { get; set; }
        public Product product { get; set; }
        public string customerId { get; set; }
        public DateTime createdDate { get; set; }
        public string createdBy { get; set; }
    }
    public class CustomerAccountDto
    {
        public string accountId { get; set; }
        public string accountNumber { get; set; }
        public string status { get; set; }
        public string productName { get; set; }
        public string productId { get; set; }
        public string customerId { get; set; }
        public string customerName { get; set; }
        public string profileType { get; set; }
        public string createdDate { get; set; }
        public string createdBy { get; set; }
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
