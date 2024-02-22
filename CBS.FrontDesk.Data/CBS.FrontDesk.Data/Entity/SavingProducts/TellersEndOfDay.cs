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
        public string id { get; set; }
        public string tellerId { get; set; }
        public string userIdInChargeOfThisTeller { get; set; }
        public string provisionedBy { get; set; }
        public string openedDate { get; set; }
        public string clossedDate { get; set; }
        public decimal openOfDayAmount { get; set; }
        public decimal cashReplenishmentAmount { get; set; }
        public string replenishmentReferenceNumber { get; set; }
        public bool isCashReplenishment { get; set; }
        public decimal cashAtHand { get; set; }
        public decimal endOfDayAmount { get; set; }
        public decimal accountBalance { get; set; }
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
        public Teller teller { get; set; }
    }
    public class EndOfDaySubTellerCommand
    {
        public CurrencyNotes currencyNotes { get; set; }=new CurrencyNotes();
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
        public int amountRecieved { get; set; }
        public string primaryTellerProvioningHistoryID { get; set; }
        [Required]
        public string eodClosedStatus { get; set; }
        [Required]
        public string accountantConfirmationStatus { get; set; }
    }
    public class EndOfTheDay
    {
        public EndOfDayAccountantCommand EndOfDayAccountantCommand { get; set; } = new EndOfDayAccountantCommand();
        public EndOfDayBySubTellerIDCommand EndOfDayBySubTellerIDCommand { get; set; } = new EndOfDayBySubTellerIDCommand();
        public EndOfDayPrimaryTellerCommand EndOfDayPrimaryTellerCommand { get; set; } = new EndOfDayPrimaryTellerCommand();
        public EndOfDaySubTellerCommand EndOfDaySubTellerCommand { get; set; } = new EndOfDaySubTellerCommand();
        public List<PrimaryTellerProvisioningDto> PrimaryTellerProvisioningHistories { get; set; }= new List<PrimaryTellerProvisioningDto>();
        public PrimaryTellerProvisioningDto PrimaryTellerProvisioningHistory { get; set; } = new PrimaryTellerProvisioningDto();
        public List<SubTellerProvisioningDto> SubTellerProvioningHistories { get; set; } = new List<SubTellerProvisioningDto>();
        public SubTellerProvisioningDto SubTellerProvioningHistory { get; set; } = new SubTellerProvisioningDto();
        public List<TransactionHistory> TransactionHistories { get; set; } = new List<TransactionHistory>();
    }
}
