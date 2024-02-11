using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CBS.FrontDesk.Data.Entity.CustomerManagement;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountOperation;

namespace CBS.FrontDesk.Data.Entity.SavingProducts
{


    public class Account
    {

        public string id { get; set; }
        public string accountNumber { get; set; }
        public decimal balance { get; set; } = 0;
        public decimal previousBalance { get; set; } = 0;
        public string status { get; set; }
        public string productId { get; set; }
        public string customerId { get; set; }
        public string tellerId { get; set; }
        public string encryptedBalance { get; set; }
        public decimal interestGenerated { get; set; }
        public decimal tellerInterestBalance { get; set; }
        public string accountName { get; set; }
        public string lastOperation { get; set; }
        public decimal? lastOperationAmount { get; set; } = 0;
        public string bankId { get; set; }
        public string branchId { get; set; }
        public SavingProduct product { get; set; }
        public Teller teller { get; set; }

        public DateTime? createdDate { get; set; }
        public string createdBy { get; set; }
        public DateTime? modifiedDate { get; set; }
        public string modifiedBy { get; set; }
        public DateTime? deletedDate { get; set; }
        public string deletedBy { get; set; }
        public decimal objectState { get; set; }
        public bool isDeleted { get; set; }
        public List<TransactionHistory> transactions { get; set; }
        public IndividualProfile Customer { get; set; }
        public AccountDepositRequest AccountActivationRequest { get; set; } = new AccountDepositRequest();
        public List<CustomerAccount> Accounts { get; set; } = new List<CustomerAccount>();
        public List<TransactionHistory> TransactionHistories { get; set; } = new List<TransactionHistory>();
        public AccountBalance AccountBalance { get; set; } = new AccountBalance();
        public WithdrawalRequest WithdrawalRequest { get; set; } = new WithdrawalRequest();
        public DepositRequest DepositRequest { get; set; } = new DepositRequest();
        public TransferRequest TransferRequest { get; set; } = new TransferRequest();
        public string OperationType { get; set; }
    }

    public class AccountBalance
    {
        public List<Account> accounts { get; set; }
        public string totalBalance { get; set; }
    }
}
