using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
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
        public DateTime? openedDate { get; set; }
        public DateTime? clossedDate { get; set; } = new DateTime(1900, 1, 1);
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
    public class EndOfDaySubTellerCommand
    {
        public CurrencyNotes currencyNotes { get; set; }=new CurrencyNotes();
        [Required]
        public string comment { get; set; }
        [Required]
        public int cashAtHand { get; set; }
        public DateTime operationDate { get; set; }
    }

    public class PrimaryTellerProvisioningHistory
    {
        public string id { get; set; }
        public string tellerId { get; set; }
        public string userIdInChargeOfThisTeller { get; set; }
        public string provisionedBy { get; set; }
        public DateTime? openedDate { get; set; }
        public DateTime? clossedDate { get; set; } = new DateTime(1900, 1, 1);
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

    }
    public class EndOfTheDay
    {
        public EndOfDayPrimaryTellerCommand EndOfDayPrimaryTellerCommand { get; set; } = new EndOfDayPrimaryTellerCommand();
        public EndOfDaySubTellerCommand EndOfDaySubTellerCommand { get; set; } = new EndOfDaySubTellerCommand();
        public List<PrimaryTellerProvisioningHistory> PrimaryTellerProvisioningHistories { get; set; }= new List<PrimaryTellerProvisioningHistory>();
        public List<SubTellerProvioningHistory> SubTellerProvioningHistories { get; set; } = new List<SubTellerProvioningHistory>();
    }
}
