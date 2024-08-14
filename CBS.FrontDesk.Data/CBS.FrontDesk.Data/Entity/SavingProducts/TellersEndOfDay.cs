using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountOperation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.SavingProducts
{
    public class SubTellerProvioningHistory
    {
        public string id { get; set; }
        public string tellerId { get; set; }
        public string userIdInChargeOfThisTeller { get; set; }
        public string provisionedBy { get; set; }
        public DateTime openedDate { get; set; }
        public DateTime clossedDate { get; set; } = new DateTime(1900, 1, 1);
        public decimal openOfDayAmount { get; set; } = 0;
        public decimal cashAtHand { get; set; } = 0;
        public decimal endOfDayAmount { get; set; } = 0;
        public decimal accountBalance { get; set; } = 0;
        public decimal previouseBalance { get; set; }
        public string lastUserID { get; set; }
        public string subTellerComment { get; set; }
        public string primaryTellerID { get; set; }
        public string bankId { get; set; }
        public string branchId { get; set; }
        public string startOfDayCurrencyNoteId { get; set; }
        public string clossedStatus { get; set; }
        public string primaryTellerComment { get; set; }
        public string primaryTellerConfirmationStatus { get; set; }
    }

    public class GetTellerOpenningAndClossingQuery
    {
        public bool ByBracnch { get; set; }
        [RequiredIfByBranch]
        public string BranchId { get; set; }
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date From")]
        public string DateFrom { get; set; }
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date To")]
        public string DateTo { get; set; }
    }

    public class RequiredIfByBranchAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var query = (GetTellerOpenningAndClossingQuery)validationContext.ObjectInstance;
            if (query.ByBracnch && string.IsNullOrWhiteSpace(query.BranchId))
            {
                return new ValidationResult("BranchId is required when ByBracnch is true.");
            }
            return ValidationResult.Success;
        }
    }

    public class OpenningAnclClossingTillDto
    {
        public string UserIdInChargeOfThisTeller { get; set; }
        public string ProvisionedBy { get; set; }
        public bool IsCashReplenished { get; set; }
        public decimal ReplenishedAmount { get; set; }
        public DateTime OpenedDate { get; set; }
        public DateTime ClossedDate { get; set; } = new DateTime(1900, 1, 1);
        public decimal OpenOfDayAmount { get; set; } = 0;
        public decimal AmountReplenished { get; set; }
        public bool IsRequestedForCashReplenishment { get; set; }
        public decimal CashAtHand { get; set; } = 0;
        public decimal EndOfDayAmount { get; set; } = 0;
        public decimal AccountBalance { get; set; } = 0;
        public decimal TellerAccountBalance { get; set; } = 0;
        public decimal LastOPerationAmount { get; set; } = 0;
        public string LastOperationType { get; set; }
        public decimal PreviouseBalance { get; set; }
        public string TellerComment { get; set; }
        public string PrimaryTeller { get; set; }
        public string BranchCode { get; set; }
        public string ClossedStatus { get; set; }
        public string PrimaryTellerComment { get; set; }
        public string PrimaryTellerConfirmationStatus { get; set; }
        public string TellerName { get; set; }
    }

    public class SubTellerProvisioningDto
    {
        public string id { get; set; }
        public string tellerId { get; set; }
        public string userIdInChargeOfThisTeller { get; set; }
        public string provisionedBy { get; set; }
        public bool IsCashReplenished { get; set; }
        public decimal ReplenishedAmount { get; set; }
        public DateTime openedDate { get; set; }
        public DateTime clossedDate { get; set; }
        public decimal openOfDayAmount { get; set; }
        public decimal cashAtHand { get; set; }
        public decimal endOfDayAmount { get; set; }
        public decimal accountBalance { get; set; }
        public decimal tellerAccountBalance { get; set; }
        public decimal lastOPerationAmount { get; set; }
        public string lastOperationType { get; set; }
        public decimal previouseBalance { get; set; }
        public string lastUserID { get; set; }
        public string subTellerComment { get; set; }
        public string primaryTellerID { get; set; }
        public string bankId { get; set; }
        public string branchId { get; set; }
        public string startOfDayCurrencyNoteId { get; set; }
        public string clossedStatus { get; set; }
        public string primaryTellerComment { get; set; }
        public string primaryTellerConfirmationStatus { get; set; }
        public Teller teller { get; set; }
    }
    public class PrimaryTellerProvisioningDto
    {
        public string Id { get; set; }
        public string TellerId { get; set; }
        public string ReferenceId { get; set; }
        public string CloseOfDayReferenceId { get; set; }
        public string UserIdInChargeOfThisTeller { get; set; }
        public string ProvisionedBy { get; set; }
        public DateTime? OpenedDate { get; set; }
        public string PrimaryTellerId { get; set; }
        public DateTime? ClossedDate { get; set; } = new DateTime(1900, 1, 1);
        public decimal OpenOfDayAmount { get; set; } = 0;
        public decimal CashReplenishmentAmount { get; set; } = 0;
        public string ReplenishmentReferenceNumber { get; set; }
        public bool IsCashReplenishment { get; set; }
        public decimal CashAtHand { get; set; } = 0;
        public decimal EndOfDayAmount { get; set; } = 0;
        public decimal AccountBalance { get; set; } = 0;
        public string DailyTellerId { get; set; }
        public decimal PreviouseBalance { get; set; }
        public string LastUserID { get; set; }
        public string BankId { get; set; }
        public string BranchId { get; set; }
        public string ClossedStatus { get; set; }
        public string PrimaryTellerComment { get; set; }
        public Teller teller { get; set; }
    }
    public class EndOfDaySubTellerCommand
    {
        public CurrencyNotes currencyNotes { get; set; } = new CurrencyNotes();
        [Required]
        public string comment { get; set; }
        [Required]
        public int cashAtHand { get; set; }
        public string operationDate { get; set; }
        public string tellerProvisioningId { get; set; }
        public string openOfDayAmount { get; set; }
    }
    public class EndOfDayBySubTellerIDCommand
    {
        [Required]
        public string comment { get; set; }
        [Required]
        public string primaryTellerConfirmationStatus { get; set; }
        public string subTellerProvioningHistoryID { get; set; }

    }

    public class PrimaryTellerProvisioningHistory
    {
        public string id { get; set; }
        public string tellerId { get; set; }
        public string userIdInChargeOfThisTeller { get; set; }
        public string provisionedBy { get; set; }
        public DateTime openedDate { get; set; }
        public DateTime clossedDate { get; set; } = new DateTime(1900, 1, 1);
        public decimal openOfDayAmount { get; set; } = 0;
        public decimal cashAtHand { get; set; } = 0;
        public decimal endOfDayAmount { get; set; } = 0;
        public decimal accountBalance { get; set; } = 0;
        public decimal previouseBalance { get; set; }
        public string lastUserID { get; set; }
        public string bankId { get; set; }
        public string branchId { get; set; }
        public string startOfDayCurrencyNoteId { get; set; }
        public string accountantComment { get; set; }
        public string accountantUserID { get; set; }
        public string clossedStatus { get; set; }
        public string primaryTellerComment { get; set; }
        public string accountantConfirmationStatus { get; set; }
        public string accountantCloseOfDayComment { get; set; }
    }
    public class EndOfDayPrimaryTellerCommand
    {
        [Required]
        public CurrencyNotes currencyNotes { get; set; } = new CurrencyNotes();
        [Required]
        public string comment { get; set; }
        [Required]
        public int cashAtHand { get; set; }
        public string primaryTellerProvioningHistoryID { get; set; }
        public string clossedStatus { get; set; }


    }
    public class EndOfDayAccountantCommand
    {
        [Required]
        public string comment { get; set; }
        [Required]
        public decimal amountRecieved { get; set; }
        public string primaryTellerProvioningHistoryID { get; set; }
        [Required]
        public string eodClosedStatus { get; set; }
        [Required]
        public string accountantConfirmationStatus { get; set; }
    }
    public class EndOfTheDay
    {
        public CloseOfDayRequest CloseOfDayRequest { get; set; } = new CloseOfDayRequest();
        public EndOfDayAccountantCommand EndOfDayAccountantCommand { get; set; } = new EndOfDayAccountantCommand();
        public EndOfDayBySubTellerIDCommand EndOfDayBySubTellerIDCommand { get; set; } = new EndOfDayBySubTellerIDCommand();
        public EndOfDayPrimaryTellerCommand EndOfDayPrimaryTellerCommand { get; set; } = new EndOfDayPrimaryTellerCommand();
        public EndOfDaySubTellerCommand EndOfDaySubTellerCommand { get; set; } = new EndOfDaySubTellerCommand();
        public List<PrimaryTellerProvisioningDto> PrimaryTellerProvisioningHistories { get; set; } = new List<PrimaryTellerProvisioningDto>();
        public PrimaryTellerProvisioningDto PrimaryTellerProvisioningHistory { get; set; } = new PrimaryTellerProvisioningDto();
        public List<SubTellerProvisioningDto> SubTellerProvioningHistories { get; set; } = new List<SubTellerProvisioningDto>();
        public SubTellerProvisioningDto SubTellerProvioningHistory { get; set; } = new SubTellerProvisioningDto();
        public List<TransactionHistory> TransactionHistories { get; set; } = new List<TransactionHistory>();
        public string Option { get; set; }
    }
    public class DailyTeller
    {
        public string Id { get; set; }
        [Required]
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string ProvisionedBy { get; set; }
        [Required]
        public string TellerId { get; set; }
        [Required]
        public string OperationType { get; set; }//Cash, NoneCash
        public bool Status { get; set; }
        public bool IsPrimary { get; set; }
        [Required]
        public string BranchId { get; set; }
        [Required]
        public decimal MaximumWithdrawalAmount { get; set; }
        [Required]
        public decimal MaximumCeilin { get; set; }
        public Teller Teller { get; set; }
        public GetAllTellerOperationsQuery GetAllTellerOperationsQuery { get; set; } = new GetAllTellerOperationsQuery();
        public Branch Branch { get; set; }
        public List<Branch> Branches { get; set; }
        public List<PrimaryTellerProvisioningHistory> PrimaryTellerProvisioningHistories { get; set; }
        public List<SubTellerProvioningHistory> SubTellerProvioningHistories { get; set; }

    }
    public class QueryParamWithDates
    {
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public string BranchId { get; set; }
    }
    public class TellerOperationGL
    {
        public DateTime Date { get; set; }
        public string Naration { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceBF { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal Balance { get; set; }

        public string AccountNumber { get; set; }
        public string TransactionType { get; set; }
        public string TransactionRef { get; set; }

        public string DailyReferences { get; set; }

        public string MemberId { get; set; }
        public string Description { get; set; }
        public string MemberAccountNumber { get; set; }


        public string TellerID { get; set; }

        public string BranchId { get; set; }

    }

    public class ExportTellerGL
    {
        public DateTime Date { get; set; }
        public string Naration { get; set; }
        public decimal BalanceBF { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal Balance { get; set; }
        public string BranchName { get; set; }
        public string TellerName { get; set; }
        public string UserName { get; set; }
        public string DailyReferences { get; set; }
        public string MemberId { get; set; }
        public string Description { get; set; }
        public string MemberAccountNumber { get; set; }
        public string AccountNumber { get; set; }
        public string TransactionType { get; set; }
        public string HeadOffice { get; set; }
        public string BranchCode { get; set; }
        public string BranchTel { get; set; }
        public string BranchAddress { get; set; }
        public string LogoUrl { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal ClosingBalance { get; set; }
        public int TotalTransactions { get; set; }
        public decimal TotalDebit { get; set; }
    }
    public class GetAllTellerOperationsQuery
    {
        public string QueryString { get; set; }
        public string TellerId { get; set; }
        public string DateFrom { get; set; }
        public string DateTo { get; set; }
        public string BranchId { get; set; }
        public bool IsByTeller { get; set; }
        public bool IsByBranch { get; set; }
        public bool IsByDate { get; set; }
        public bool IsPDF { get; set; }
    }
    public class CashReplenishmentSubTeller
    {
        public string Id { get; set; }
        [Required]
        public decimal RequestedAmount { get; set; }
        [Required]
        public decimal ConfirmedAmount { get; set; }
        public string RequesterUserId { get; set; }
        public string RequesterName { get; set; }
        public string ApprovedBy { get; set; }
        public string ApprovedByUserId { get; set; }
        public DateTime ApprovedDate { get; set; }
        public DateTime InitializeDate { get; set; }
        [Required]
        public string ApprovedComment { get; set; }
        [Required]
        public string Requetcomment { get; set; }
        public string ApprovedStatus { get; set; }//Pending,Approved, Rejected
        public string TellerId { get; set; }
        public string BranchId { get; set; }
        public bool Approved { get; set; }
        public Branch Branch { get; set; }
        public string TransactionReference { get; set; }
        public CurrencyNotes CurrencyNotes { get; set; } = new CurrencyNotes();

        public CashReplenishmentSubTeller()
        {
            Approved = true;
            ApprovedComment = "Approved: Cash replenishment authorized to maintain optimal cash levels for customer service and operational efficiency.";
            Requetcomment = "Cash replenishment needed to meet current customer demand and ensure seamless operations at the teller station.";
        }
    }
    public class CashReplenishmentPrimaryTeller
    {
        public string Id { get; set; }
        public string TellerId { get; set; }
        public bool Status { get; set; }
        public string BranchId { get; set; }
        [Required]
        public decimal RequestedAmount { get; set; }
        [Required]
        public decimal ConfirmedAmount { get; set; }
        public string RequesterUserId { get; set; }
        public string ApprovedBy { get; set; }
        public string ApprovedByUserId { get; set; }
        public DateTime ApprovedDate { get; set; }
        public DateTime InitializeDate { get; set; }
        [Required]
        public string ApprovedComment { get; set; }
        [Required]
        public string Requetcomment { get; set; }
        [Required]
        public string ApprovedStatus { get; set; }//Pending,Approved, Rejected
        public string TransactionReference { get; set; }
        public string AccountingPostingReference { get; set; }

        public string RequesterName { get; set; }
        public Teller Teller { get; set; } = new Teller();
        public Branch Branch { get; set; } = new Branch();
        public CurrencyNotes CurrencyNote { get; set; } = new CurrencyNotes();
        public CashReplenishmentPrimaryTeller()
        {
            Status = true;
            ApprovedComment = "Approved: Cash replenishment authorized to maintain optimal cash levels for customer service and operational efficiency.";
            Requetcomment = "Cash replenishment needed to meet current customer demand and ensure seamless operations at the teller station.";
        }

    }
}
