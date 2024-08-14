using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.CustomerManagement;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountOperation;

namespace CBS.FrontDesk.Data.Entity.SavingProducts
{


    public class Account
    {

        public string Id { get; set; }
        public string AccountNumber { get; set; }
        public decimal Balance { get; set; } = 0;
        public decimal PreviousBalance { get; set; } = 0;
        public string Status { get; set; }
        public string ProductId { get; set; }
        public string CustomerId { get; set; }
        public string TellerId { get; set; }
        public string EncryptedBalance { get; set; }
        public decimal InterestGenerated { get; set; } = 0;
        public decimal LastInterestPosted { get; set; } = 0;
        public decimal BlockedAmount { get; set; } = 0;
        public string BlockedId { get; set; }
        public string ReasonOfBlocked { get; set; }
        public string AccountName { get; set; }
        public string LastOperation { get; set; }
        public string AccountType { get; set; }
        public bool IsTellerAccount { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal LastOperationAmount { get; set; } = 0;
        public string BankId { get; set; }
        public string BranchId { get; set; }
        public string ModifiedBy { get; set; }
        public string CreatedBy { get; set; }
        public string ErrorMessage { get; set; }
        public bool HasError { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime DateOfOpeningBalance { get; set; } = DateTime.MinValue;
        public DateTime DateBlocked { get; set; } = DateTime.MinValue;
        public DateTime DateReleased { get; set; } = DateTime.MinValue;
        public DateTime DateOfLastOperation { get; set; } = DateTime.MinValue;
        public DateTime LastInterestCalculatedDate { get; set; } = DateTime.MinValue;
        public virtual SavingProduct Product { get; set; }
        public virtual ICollection<TransactionHistory> Transactions { get; set; } = new Collection<TransactionHistory>();
        public string DeletedBy { get; set; }
        public bool IsDeleted { get; set; }
        public Teller Teller { get; set; }
        public IndividualProfile Customer { get; set; }
        public AccountDepositRequest AccountActivationRequest { get; set; } = new AccountDepositRequest();
        public List<CustomerAccount> Accounts { get; set; } = new List<CustomerAccount>();
        public List<TransactionHistory> TransactionHistories { get; set; } = new List<TransactionHistory>();
        public List<Loan> Loans { get; set; } = new List<Loan>();
        public List<Refund> LoanRepayments { get; set; } = new List<Refund>();
        public AccountBalance AccountBalance { get; set; } = new AccountBalance();
        public WithdrawalRequest WithdrawalRequest { get; set; } = new WithdrawalRequest();
        public DepositRequest DepositRequest { get; set; } = new DepositRequest();
        public LoanRepaymentCommand LoanRepaymentCommand { get; set; }=new LoanRepaymentCommand();
        public TransferRequest TransferRequest { get; set; } = new TransferRequest();
        public TransferConfirmation TransferConfirmation { get; set; } = new TransferConfirmation();
        public Transfer Transfer { get; set; } = new Transfer();
        public List<Transfer> Transfers { get; set; } = new List<Transfer>();
        public string OperationType { get; set; }
    }

    public class BalanceOfLoanAndSaving
    {
        public decimal SavingAccountBalance { get; set; }
        public decimal LoanBalance { get; set; }
    }
    public class AccountBalance
    {
        public List<Account> accounts { get; set; }
        public string totalBalance { get; set; }
    }
    public class GetTellerAccountBalanceQuery
    {
        public string TellerId { get; set; }
        public bool IsPrimary { get; set; }
        public bool HasValue { get; set; }
        public bool IsCloseOfDayPrimaryTeller { get; set; }
        public GetTellerAccountBalanceQuery(string tellerId = "N/A", bool isPrimary = false, bool hasValue = false, bool isCloseOfDayPrimaryTeller = false)
        {
            TellerId = tellerId;
            IsPrimary = isPrimary;
            HasValue = hasValue;
            IsCloseOfDayPrimaryTeller = isCloseOfDayPrimaryTeller;
        }
    }
    public class MembersAccountSummary
    {
        public string MemberName { get; set; }
        public string MemberReference { get; set; }
        public string BranchCode { get; set; }
        public decimal Saving { get; set; }
        public decimal PreferenceShare { get; set; }
        public decimal Share { get; set; }
        public decimal Deposit { get; set; }
        public decimal Loan { get; set; }
        public decimal Gav { get; set; }
        public decimal DailyCollection { get; set; }
        public decimal TotalBalance { get; set; }
        public decimal NetBalance { get; set; }
        public PaginationMetadata PaginationMetadata { get; set; }

    }
    public class MembersAccountSummaryDto
    {
        public string MemberName { get; set; }
        public string MemberReference { get; set; }
        public string BranchCode { get; set; }
        public decimal Saving { get; set; }
        public decimal PreferenceShare { get; set; }
        public decimal Share { get; set; }
        public decimal Deposit { get; set; }
        public decimal Loan { get; set; }
        public decimal Gav { get; set; }
        public decimal DailyCollection { get; set; }
        public decimal TotalBalance { get; set; }
        public decimal NetBalance { get; set; }

    }
}
