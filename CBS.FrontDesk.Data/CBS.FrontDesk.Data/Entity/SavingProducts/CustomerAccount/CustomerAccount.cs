using CBS.FrontDesk.Data.Entity.SavingProducts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity
{




    public class CustomerAccount
    {
        public string id { get; set; }
        public string accountNumber { get; set; }
        public string balance { get; set; }
        public string previousBalance { get; set; }
        public string status { get; set; }
        public string productId { get; set; }
        public string tellerId { get; set; }
        public SavingProduct product { get; set; }
        public DateTime createdDate { get; set; }
        public object teller { get; set; }
        public string createdBy { get; set; }
        public string customerId { get; set; }
        public double interestGenerated { get; set; }
        public DateTime modifiedDate { get; set; }
        public string modifiedBy { get; set; }
        public string bankId { get; set; }
        public string branchId { get; set; }
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
